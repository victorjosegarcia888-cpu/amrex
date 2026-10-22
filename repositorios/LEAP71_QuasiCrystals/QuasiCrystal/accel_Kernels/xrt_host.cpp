// SPDX-License-Identifier: Apache-2.0
//
// XRT host bridge for LEAP71 quasicrystal acceleration.
// Exposes C-safe APIs for C# P/Invoke.
//
// Build (Linux):
//   g++ -shared -fPIC -O3 xrt_host.cpp \
//       -I$XILINX_XRT/include -L$XILINX_XRT/lib -lxrt_coreutil -o libxrt_host.so
//
// Build (Windows):
//   cl /LD /O2 xrt_host.cpp /I%XILINX_XRT%\include %XILINX_XRT%\lib\xrt_core.lib /link /out:xrt_host.dll

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <vector>
#include <string>
#include <xrt/xrt_device.h>
#include <xrt/xrt_kernel.h>
#include <xrt/xrt_bo.h>

// -----------------------------------------------------------------------------
// Types matching HLS kernels
// -----------------------------------------------------------------------------
struct Vector3 { float x, y, z; };

struct BeamSegment {
    float x0, y0, z0;
    float x1, y1, z1;
    float radius;
};

// -----------------------------------------------------------------------------
// Global device state (single device, multi-kernel)
// -----------------------------------------------------------------------------
namespace {
    xrt::device      s_device;
    xrt::kernel      s_kernelFace;
    xrt::kernel      s_kernelVoxel;
    xrt::kernel      s_kernelSlice;
    bool             s_initialized = false;
    std::string      s_lastError;

    std::string xrtErrToString(xrt::error& e)
    {
        return std::string("XRT error: ") + e.what();
    }
}

// -----------------------------------------------------------------------------
// Initialization
// -----------------------------------------------------------------------------
extern "C" {

    __declspec(dllexport)
    int InitQuasicrystalAccelerator(const char* xclbinPath)
    {
        try {
            s_device = xrt::device(0);
            auto uuid = s_device.load_xclbin(xclbinPath);

            s_kernelFace  = xrt::kernel(s_device, uuid, "AccelerateQuasiCrystalBatch");
            s_kernelVoxel = xrt::kernel(s_device, uuid, "VoxelizeQuasiCrystal");
            s_kernelSlice = xrt::kernel(s_device, uuid, "SliceVoxelVolumeZ");

            s_initialized = true;
            s_lastError.clear();
            return 0;
        } catch (xrt::error& e) {
            s_lastError = xrtErrToString(e);
            return -1;
        } catch (std::exception& e) {
            s_lastError = std::string("std::exception: ") + e.what();
            return -2;
        } catch (...) {
            s_lastError = "Unknown exception during XRT init";
            return -3;
        }
    }

    const char* GetLastError()
    {
        return s_lastError.c_str();
    }

    // -------------------------------------------------------------------------
    // Face lattice accelerator (batch)
    // -------------------------------------------------------------------------
    // Inputs are pinned by caller. Output buffer must be pre-allocated.
    // Returns number of floats written to outputToolpaths, or <0 on error.
    __declspec(dllexport)
    int DispatchFaceBatch(
        const Vector3*   vertices,        // [faceCount * 4]
        const int*       connectorTypes,  // [faceCount] 0=ARROW,1=TRIANGLE,2=LINE
        float            beamThickness,   // beam radius
        int              faceCount,
        float*           outputToolpaths  // caller-allocated
    )
    {
        if (!s_initialized) {
            s_lastError = "XRT not initialized";
            return -1;
        }
        if (faceCount <= 0 || !vertices || !connectorTypes || !outputToolpaths) {
            s_lastError = "Invalid null pointer or zero faceCount";
            return -2;
        }

        try {
            size_t inVertsSize  = sizeof(Vector3) * 4 * (size_t)faceCount;
            size_t inTypeSize   = sizeof(int) * (size_t)faceCount;
            // Max output: 3 segments/face * 7 floats/segment + 1 header float
            size_t outSize      = sizeof(float) * (1 + 3 * (size_t)faceCount * 7);

            auto boVerts  = xrt::bo(s_device, inVertsSize,  s_kernelFace.group_id(0));
            auto boTypes  = xrt::bo(s_device, inTypeSize,   s_kernelFace.group_id(1));
            auto boOutput = xrt::bo(s_device, outSize,      s_kernelFace.group_id(2));

            boVerts.write(vertices);
            boTypes.write(connectorTypes);
            boVerts.sync(XCL_BO_SYNC_BO_TO_DEVICE);
            boTypes.sync(XCL_BO_SYNC_BO_TO_DEVICE);

            auto run = s_kernelFace(boVerts, boTypes, beamThickness, faceCount, boOutput);
            run.wait();

            boOutput.sync(XCL_BO_SYNC_BO_FROM_DEVICE);
            boOutput.read(outputToolpaths);

            // First float is segment count
            int totalSegments = (int)outputToolpaths[0];
            return 1 + totalSegments * 7;
        } catch (xrt::error& e) {
            s_lastError = xrtErrToString(e);
            return -10;
        } catch (...) {
            s_lastError = "Unknown exception in DispatchFaceBatch";
            return -11;
        }
    }

    // -------------------------------------------------------------------------
    // Voxelization
    // -------------------------------------------------------------------------
    __declspec(dllexport)
    int DispatchVoxelize(
        const BeamSegment* beams,
        int                numBeams,
        float              originX,
        float              originY,
        float              originZ,
        float              voxelSize,
        int                gridX,
        int                gridY,
        int                gridZ,
        ap_uint<32>*       voxelGrid   // caller-allocated, size = gridX*gridY*gridZ/32
    )
    {
        if (!s_initialized) {
            s_lastError = "XRT not initialized";
            return -1;
        }
        if (!beams || !voxelGrid || numBeams <= 0) {
            s_lastError = "Invalid arguments to DispatchVoxelize";
            return -2;
        }

        try {
            size_t beamSize  = sizeof(BeamSegment) * (size_t)numBeams;
            size_t gridWords = (size_t)gridX * gridY * gridZ / 32;
            size_t gridSize  = gridWords * sizeof(ap_uint<32>);

            auto boBeams = xrt::bo(s_device, beamSize, s_kernelVoxel.group_id(0));
            auto boGrid  = xrt::bo(s_device, gridSize, s_kernelVoxel.group_id(1));

            boBeams.write(beams);
            boBeams.sync(XCL_BO_SYNC_BO_TO_DEVICE);

            auto run = s_kernelVoxel(boBeams, numBeams, originX, originY, originZ,
                                     voxelSize, gridX, gridY, gridZ, boGrid);
            run.wait();

            boGrid.sync(XCL_BO_SYNC_BO_FROM_DEVICE);
            boGrid.read(voxelGrid);

            return (int)gridWords;
        } catch (xrt::error& e) {
            s_lastError = xrtErrToString(e);
            return -10;
        } catch (...) {
            s_lastError = "Unknown exception in DispatchVoxelize";
            return -11;
        }
    }

    // -------------------------------------------------------------------------
    // Z-slice extraction
    // -------------------------------------------------------------------------
    __declspec(dllexport)
    int DispatchSliceZ(
        const ap_uint<32>* voxelGrid,
        int                gridX,
        int                gridY,
        int                gridZ,
        float              originZ,
        float              voxelSize,
        int                sliceZ,
        ap_uint<32>*       sliceBitmap  // caller-allocated, size = gridY
    )
    {
        if (!s_initialized) {
            s_lastError = "XRT not initialized";
            return -1;
        }
        if (!voxelGrid || !sliceBitmap) {
            s_lastError = "Null pointer in DispatchSliceZ";
            return -2;
        }

        try {
            size_t gridWords = (size_t)gridX * gridY * gridZ / 32;
            size_t sliceSize = gridY * sizeof(ap_uint<32>);

            auto boGrid = xrt::bo(s_device, gridWords * sizeof(ap_uint<32>), s_kernelSlice.group_id(0));
            auto boOut  = xrt::bo(s_device, sliceSize,                     s_kernelSlice.group_id(1));

            boGrid.write(voxelGrid);
            boGrid.sync(XCL_BO_SYNC_BO_TO_DEVICE);

            auto run = s_kernelSlice(boGrid, gridX, gridY, gridZ, originZ, voxelSize, sliceZ, boOut);
            run.wait();

            boOut.sync(XCL_BO_SYNC_BO_FROM_DEVICE);
            boOut.read(sliceBitmap);

            return gridY;
        } catch (xrt::error& e) {
            s_lastError = xrtErrToString(e);
            return -10;
        } catch (...) {
            s_lastError = "Unknown exception in DispatchSliceZ";
            return -11;
        }
    }
}

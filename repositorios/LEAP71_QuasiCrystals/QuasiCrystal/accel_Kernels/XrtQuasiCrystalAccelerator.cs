// SPDX-License-Identifier: Apache-2.0
//
// C# P/Invoke bridge for LEAP71 quasicrystal FPGA acceleration.
// Requires libxrt_host.so (Linux) or xrt_host.dll (Windows) in the runtime path.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Leap71.AperiodicTiling
{
    using IcosehedralFace = Leap71.AperiodicTiling.IcosehedralFace;

    public static class XrtQuasiCrystalAccelerator
    {
        private const string NATIVE_LIB = "xrt_host";
        private static bool ms_initialized = false;

        // ---------------------------------------------------------------------
        // Native imports
        // ---------------------------------------------------------------------
        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int InitQuasicrystalAccelerator(
            [MarshalAs(UnmanagedType.LPStr)] string xclbinPath);

        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DispatchFaceBatch(
            [In] Vector3[] vertices,
            [In] int[] connectorTypes,
            float beamThickness,
            int faceCount,
            [Out] float[] outputToolpaths);

        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DispatchVoxelize(
            [In] BeamSegment[] beams,
            int numBeams,
            float originX, float originY, float originZ,
            float voxelSize,
            int gridX, int gridY, int gridZ,
            [Out] uint[] voxelGrid);

        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DispatchSliceZ(
            [In] uint[] voxelGrid,
            int gridX, int gridY, int gridZ,
            float originZ, float voxelSize,
            int sliceZ,
            [Out] uint[] sliceBitmap);

        [StructLayout(LayoutKind.Sequential)]
        public struct BeamSegment
        {
            public float x0, y0, z0;
            public float x1, y1, z1;
            public float radius;
        }

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------
        public static void Initialize(string xclbinPath)
        {
            if (ms_initialized) return;
            int rc = InitQuasicrystalAccelerator(xclbinPath);
            if (rc != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to initialize XRT accelerator (code={rc}). Check xclbin path and FPGA device.");
            }
            ms_initialized = true;
        }

        /// <summary>
        /// Processes a batch of icosahedral faces and returns flat toolpath segments.
        /// Output format: [totalCount, seg0_x0,y0,z0,x1,y1,z1,r, seg1_...]
        /// </summary>
        public static float[] ProcessFaceBatch(
            Vector3[] vertices,
            IcosehedralFace.EConnector[] connectors,
            float beamThickness)
        {
            if (!ms_initialized) throw new InvalidOperationException("XRT not initialized");
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            if (connectors == null) throw new ArgumentNullException(nameof(connectors));
            if (vertices.Length != connectors.Length * 4)
                throw new ArgumentException("vertices must contain 4 entries per face.");

            int faceCount = connectors.Length;
            int maxOutput = 1 + 3 * faceCount * 7;
            float[] output = new float[maxOutput];
            int[] connectorInts = new int[faceCount];
            for (int i = 0; i < faceCount; ++i)
                connectorInts[i] = (int)connectors[i];

            int written = DispatchFaceBatch(vertices, connectorInts, beamThickness, faceCount, output);
            if (written < 0)
                throw new InvalidOperationException($"DispatchFaceBatch failed (code={written})");

            Array.Resize(ref output, written);
            return output;
        }

        /// <summary>
        /// Voxelizes beam segments into a bit-packed 3D occupancy grid.
        /// Grid words = gridX * gridY * gridZ / 32. Each word packs 32 voxels along X.
        /// </summary>
        public static uint[] VoxelizeBeams(
            BeamSegment[] beams,
            float originX, float originY, float originZ,
            float voxelSize,
            int gridX, int gridY, int gridZ)
        {
            if (!ms_initialized) throw new InvalidOperationException("XRT not initialized");
            if (beams == null || beams.Length == 0)
                throw new ArgumentNullException(nameof(beams));

            int gridWords = gridX * gridY * gridZ / 32;
            uint[] grid = new uint[gridWords];

            int rc = DispatchVoxelize(beams, beams.Length,
                                      originX, originY, originZ,
                                      voxelSize, gridX, gridY, gridZ, grid);
            if (rc < 0)
                throw new InvalidOperationException($"DispatchVoxelize failed (code={rc})");

            return grid;
        }

        /// <summary>
        /// Extracts a Z slice from a voxel grid as a row-packed bitmap.
        /// sliceBitmap[y] contains gridX bits (LSB = lowest X).
        /// </summary>
        public static uint[] ExtractZSlice(
            uint[] voxelGrid,
            int gridX, int gridY, int gridZ,
            float originZ, float voxelSize,
            int sliceZ)
        {
            if (!ms_initialized) throw new InvalidOperationException("XRT not initialized");
            if (voxelGrid == null) throw new ArgumentNullException(nameof(voxelGrid));

            uint[] slice = new uint[gridY];
            int rc = DispatchSliceZ(voxelGrid, gridX, gridY, gridZ, originZ, voxelSize, sliceZ, slice);
            if (rc < 0)
                throw new InvalidOperationException($"DispatchSliceZ failed (code={rc})");

            return slice;
        }
    }
}

# LEAP71 Quasicrystal FPGA Acceleration

## Build Flow

### 1. Vitis HLS Kernels

```bash
# Face lattice accelerator
v++ -c -t hw_emu -f $PLATFORM -k AccelerateQuasiCrystalBatch \
    QuasiCrystal/accel_Kernels/quasicrystal_accel.cpp \
    -o build/quasicrystal_accel.xo

# Voxelization kernel
v++ -c -t hw_emu -f $PLATFORM -k VoxelizeQuasiCrystal \
    QuasiCrystal/accel_Kernels/voxel_accel.cpp \
    -o build/voxel_accel.xo

# Z-slice kernel
v++ -c -t hw_emu -f $PLATFORM -k SliceVoxelVolumeZ \
    QuasiCrystal/accel_Kernels/slice_accel.cpp \
    -o build/slice_accel.xo

# Link into xclbin
v++ -l -t hw_emu -f $PLATFORM \
    build/quasicrystal_accel.xo \
    build/voxel_accel.xo \
    build/slice_accel.xo \
    -o build/quasicrystal_accel.xclbin
```

For hardware build, replace `hw_emu` with `hw`.

### 2. C++ XRT Host Bridge

```bash
# Linux
g++ -shared -fPIC -O3 QuasiCrystal/accel_Kernels/xrt_host.cpp \
    -I$XILINX_XRT/include -L$XILINX_XRT/lib -lxrt_coreutil \
    -o QuasiCrystal/accel_Kernels/libxrt_host.so

# Windows
cl /LD /O2 QuasiCrystal/accel_Kernels/xrt_host.cpp ^
    /I%XILINX_XRT%\include %XILINX_XRT%\lib\xrt_core.lib ^
    /link /out:QuasiCrystal/accel_Kernels\xrt_host.dll
```

### 3. C# Project

Add the `QuasiCrystal/accel_Kernels/*.cs` files to your LEAP71/PicoGK project or reference the `QuasiCrystal.Accelerator.csproj` library.

## Runtime Usage

```csharp
using Leap71.AperiodicTiling;

// Initialize once
XrtQuasiCrystalAccelerator.Initialize("build/quasicrystal_accel.xclbin");

// Batch process faces
var vertices = new Vector3[faces * 4];
var connectors = new IcosehedralFace.EConnector[faces];
var toolpaths = XrtQuasiCrystalAccelerator.ProcessFaceBatch(vertices, connectors, beamThickness);

// Voxelize
var beams = new XrtQuasiCrystalAccelerator.BeamSegment[segments];
uint[] voxelGrid = XrtQuasiCrystalAccelerator.VoxelizeBeams(beams,
    originX: 0, originY: 0, originZ: 0,
    voxelSize: 0.5f,
    gridX: 256, gridY: 256, gridZ: 256);

// Extract slice for 3D printing
uint[] slice = XrtQuasiCrystalAccelerator.ExtractZSlice(voxelGrid,
    gridX: 256, gridY: 256, gridZ: 256,
    originZ: 0, voxelSize: 0.5f,
    sliceZ: 128);
```

## Kernel Reference

| Kernel | Purpose | Input | Output |
|--------|---------|-------|--------|
| `AccelerateQuasiCrystalBatch` | Face-to-toolpath conversion | vertices[4N], connectorTypes[N], beamThickness | flat segment array |
| `VoxelizeQuasiCrystal` | Beam segments -> 3D occupancy grid | beams[N], grid params | bit-packed uint32 grid |
| `SliceVoxelVolumeZ` | 3D grid -> 2D binary slice | voxel grid, Z index | row-packed bitmap |

## Output Formats

### Face Batch Output
```
[0] = totalSegmentCount (float, cast to int)
[1..7] = segment 0: x0, y0, z0, x1, y1, z1, radius
[8..14] = segment 1
...
```

### Voxel Grid
- `uint32[]` array, length = `gridX * gridY * gridZ / 32`
- Bit ordering: bit `(x % 32)` in word `(z * gridY * gridXWords + y * gridXWords + x / 32)`
- Set bit = occupied voxel

### Slice Bitmap
- `uint32[]` array, length = `gridY`
- `sliceBitmap[y]` contains `gridX` bits (LSB = X=0)

## Notes
- `BeamSegment.radius` is used directly as the beam thickness in voxelization.
- For 3D printing, extract slices at each layer height and convert bitmap to G-code moves.
- The voxel kernel evaluates exact point-to-line-segment distance; the slicer reads the precomputed occupancy grid.

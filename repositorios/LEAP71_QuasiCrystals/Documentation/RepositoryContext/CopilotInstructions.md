# Copilot generation rules for LEAP71_QuasiCrystals

## Architecture context

- This project uses the PicoGK implicit geometry engine in C# and the LEAP71_ShapeKernel library.
- The framework operates with voxels, lattices, B-splines, and quasicrystal tile subdivision.

## Main rules

1. Use PicoGK.Library.Go(float fVoxelSize, LibraryTask task) as the execution entry point.
2. 3D quasicrystal models are based on QuasiTile and IcosahedralFace.
3. For geometric construction, do not use direct polygonal meshes; always pass structures to Voxels or Lattice for boolean operations.
4. Maintain the LEAP71 C# naming convention with type prefixes in variables such as fTemperature, vecPosition, voxMonolith, and oTile.

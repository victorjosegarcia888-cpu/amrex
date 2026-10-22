# Repository technical overview

This repository addresses a critical problem in computational engineering and materials science: the creation of aperiodic structures in 2D and 3D, including Penrose patterns and icosahedral quasicrystals.

Unlike conventional periodic lattices, which repeat cubic or octahedral cells and create static weak planes or resonance frequencies, these structures lack translational symmetry. This removes preferential shear planes and reduces the localization of thermal or acoustic wave dissipation.

## 1. Technical analysis of the repository

The repository is structured around two mathematical and algorithmic pillars:

### 2D domain: Penrose pattern

- Based on subdivision and inflation of two basic rhombic tiles: fat and skinny.
- Generates pentagonal symmetry of order 5, which is forbidden in periodic crystals.
- Main reference example: PenrosePatternShowCase.Task.

### 3D domain: icosahedral quasicrystals

- Implements substitution and spatial inflation rules inspired by Alexey E. Madison.
- Built from four elementary tiles: QuasiTile_01, QuasiTile_02, QuasiTile_03, and QuasiTile_04.
- These tiles are composed of rhombic faces and are mapped to the PicoGK voxel engine and the LEAP71_ShapeKernel library.

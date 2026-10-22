# PicoGK.Geometry

PicoGK.Geometry defines the shared geometry vocabulary for the next generation of [PicoGK](https://picogk.org/).

It is a small, implementation-neutral library focused on durable contracts for:

- two- and three-dimensional paths and contours,
- triangle meshes and bulk geometry access,
- geometric frames, bounds, and angle types,
- interoperability between independent geometry kernels,
- and visualization APIs that consume geometry without owning it.

The package primarily contains interfaces and concrete value types that materially improve the shared API. It does not prescribe a geometry implementation hierarchy.

## Status

**Early development — do not use yet.**

This repository is currently being assembled and its API, packaging, and behavior may change without notice.

There are no compatibility guarantees at this stage. Untagged builds use internal prerelease versions; official releases will derive their versions from Git tags.

## Direction

PicoGK.Geometry is intended to define interoperability, not implementation.

Interfaces should support efficient bulk access where practical and must not force per-element virtual dispatch when an implementation can expose contiguous data directly.

Concrete circles, splines, CAD shapes, voxel objects, and path hierarchies belong in their implementing packages unless they are genuinely required as shared value types.

## Development

Managed code follows the [C# Coding Conventions](CodingConventions.md). The [`Frame2d`](docs/Frame2d.md) and [`Frame3d`](docs/Frame3d.md) design and performance contracts document their representations and hot-path rules.

The package should remain small, dependency-light, and independent of PicoGK.Core, PicoGK.Viewer, and the integrated PicoGK package.

## License

PicoGK.Geometry is licensed under the Apache License 2.0.

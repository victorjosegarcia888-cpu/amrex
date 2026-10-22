# `Frame2d` design and performance contract

`Frame2d` is a rigid transformation from local two-dimensional coordinates to world coordinates.

## Representation

The frame stores its world-space position and local X and Y axes as three `Vector2` values. The basis is orthonormal and right-handed: positive rotation maps X toward Y.

`default(Frame2d)` contains a zero basis and is invalid. Use `Frame2d.World` for the identity transformation.

## Construction and hot paths

Public construction validates finite inputs, normalizes the supplied approximate X axis, and derives Y as its counterclockwise perpendicular. Point and direction transformations use direct basis arithmetic without repeated validation or normalization.

Translation, inversion, composition, and rotation use a private trusted construction path. Repeated floating-point operations can accumulate drift; `frmReorthonormalized()` performs explicit repair.

## Composition

`frmA * frmB` applies `frmB` first and `frmA` second:

```csharp
(frmA * frmB).vecPtToWorld(vecPoint)
== frmA.vecPtToWorld(frmB.vecPtToWorld(vecPoint));
```

## Interpolation

`frmInterpolate` interpolates position linearly and orientation along the shortest angular path. Its interpolation parameter is clamped to `[0,1]`.

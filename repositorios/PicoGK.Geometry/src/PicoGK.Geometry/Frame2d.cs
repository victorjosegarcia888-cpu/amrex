// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PicoGK.Geometry;

/// <summary>
/// Stores a right-handed orthonormal basis and position as a rigid transformation
/// from local coordinates to world coordinates.
/// </summary>
/// <remarks>
/// Positive rotation maps local positive X toward local positive Y. The default value has a zero
/// basis and is invalid; use <see cref="World"/> for the identity frame.
/// </remarks>
[DebuggerDisplay("O=({vecPos.X:n3},{vecPos.Y:n3})")]
public readonly struct Frame2d : IEquatable<Frame2d>
{
    /// <summary>The identity frame at the world origin.</summary>
    public static readonly Frame2d World = new(
        Vector2.Zero,
        Vector2.UnitX,
        Vector2.UnitY,
        true);

    /// <summary>Frame origin in world coordinates.</summary>
    public Vector2 vecPos { get; }

    /// <summary>Local positive X axis expressed in world coordinates.</summary>
    public Vector2 vecLx { get; }

    /// <summary>Local positive Y axis expressed in world coordinates.</summary>
    public Vector2 vecLy { get; }

    /// <summary>Creates a world-aligned frame at the supplied position.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Frame2d frmFromPos(Vector2 vecPos) => new(vecPos);

    /// <summary>Creates a frame from its position and approximate X axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Frame2d frmFromX(Vector2 vecPos, Vector2 vecApproxX) => new(vecPos, vecApproxX);

    /// <summary>Creates a frame from its position and counterclockwise rotation from world X.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Frame2d frmFromPosAndAngle(Vector2 vecPos, Rad rAngle) => new(vecPos, rAngle);

    /// <summary>Creates a world-aligned frame at the supplied position.</summary>
    /// <exception cref="ArgumentException">The position contains a non-finite component.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d(Vector2 vecPos)
    {
        ValidateFinite(vecPos, nameof(vecPos));
        this.vecPos = vecPos;
        vecLx = Vector2.UnitX;
        vecLy = Vector2.UnitY;
    }

    /// <summary>Creates a frame from its position and counterclockwise rotation from world X.</summary>
    /// <exception cref="ArgumentException">The position contains a non-finite component.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The angle is non-finite.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d(Vector2 vecPos, Rad rAngle)
    {
        ValidateFinite(vecPos, nameof(vecPos));
        if (!rAngle.bIsFinite())
            throw new ArgumentOutOfRangeException(nameof(rAngle), "Rotation angle must be finite.");

        float fCos = rAngle.fCos();
        float fSin = rAngle.fSin();
        this.vecPos = vecPos;
        vecLx = new(fCos, fSin);
        vecLy = new(-fSin, fCos);
    }

    /// <summary>Creates a frame from its position and approximate X axis.</summary>
    /// <exception cref="ArgumentException">The position or axis is zero length or non-finite.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d(Vector2 vecPos, Vector2 vecApproxX)
    {
        ValidateFinite(vecPos, nameof(vecPos));
        Vector2 vecX = vecNormalizedChecked(vecApproxX, nameof(vecApproxX));
        this.vecPos = vecPos;
        vecLx = vecX;
        vecLy = new(-vecX.Y, vecX.X);
#if DEBUG
        AssertOrthonormal(vecLx, vecLy);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Frame2d(Vector2 vecPos, Vector2 vecLx, Vector2 vecLy, bool bTrustedBasis)
    {
        Debug.Assert(bTrustedBasis);
        this.vecPos = vecPos;
        this.vecLx = vecLx;
        this.vecLy = vecLy;
#if DEBUG
        Debug.Assert(bIsFinite(vecPos));
        AssertOrthonormal(vecLx, vecLy);
#endif
    }

    /// <summary>
    /// Creates a frame from a <see cref="Matrix3x2"/> row-vector transform. Its X row is treated
    /// as an approximate direction and normalized; scale and shear are discarded.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Frame2d frmFromMatrix3x2(in Matrix3x2 mat)
        => new(new Vector2(mat.M31, mat.M32), new Vector2(mat.M11, mat.M12));

    /// <summary>Transforms a local point to world coordinates.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 vecPtToWorld(Vector2 vecLocal)
        => vecLocal.X * vecLx + vecLocal.Y * vecLy + vecPos;

    /// <summary>Rotates a local direction into world coordinates without changing its magnitude.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 vecDirToWorld(Vector2 vecLocalDir)
        => vecLocalDir.X * vecLx + vecLocalDir.Y * vecLy;

    /// <summary>Transforms a world point to local coordinates.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 vecPtFromWorld(Vector2 vecWorld)
    {
        Vector2 vecR = vecWorld - vecPos;
        return new(Vector2.Dot(vecR, vecLx), Vector2.Dot(vecR, vecLy));
    }

    /// <summary>Rotates a world direction into local coordinates without changing its magnitude.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 vecDirFromWorld(Vector2 vecWorldDir)
        => new(Vector2.Dot(vecWorldDir, vecLx), Vector2.Dot(vecWorldDir, vecLy));

    /// <summary>
    /// Composes this frame with another frame. The other transformation is applied first.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmCompose(in Frame2d frmOther)
        => new(
            vecPtToWorld(frmOther.vecPos),
            vecDirToWorld(frmOther.vecLx),
            vecDirToWorld(frmOther.vecLy),
            true);

    /// <summary>Returns the inverse rigid transformation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmInverse()
    {
        Vector2 vecInversePos = new(-Vector2.Dot(vecLx, vecPos), -Vector2.Dot(vecLy, vecPos));
        Vector2 vecInverseX = new(vecLx.X, vecLy.X);
        Vector2 vecInverseY = new(vecLx.Y, vecLy.Y);
        return new(vecInversePos, vecInverseX, vecInverseY, true);
    }

    /// <summary>Returns a copy translated by a local-coordinate displacement.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmMovedLocal(Vector2 vecDistance)
        => new(vecPtToWorld(vecDistance), vecLx, vecLy, true);

    /// <summary>Returns a copy translated along its local X axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmMovedLocalX(float fDistanceX)
        => new(vecPos + fDistanceX * vecLx, vecLx, vecLy, true);

    /// <summary>Returns a copy translated along its local Y axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmMovedLocalY(float fDistanceY)
        => new(vecPos + fDistanceY * vecLy, vecLx, vecLy, true);

    /// <summary>Returns a copy translated by a world-coordinate displacement.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmMovedWorld(Vector2 vecDistance)
        => new(vecPos + vecDistance, vecLx, vecLy, true);

    /// <summary>Returns a copy translated along the world X axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmMovedWorldX(float fDistanceX)
        => new(vecPos + new Vector2(fDistanceX, 0f), vecLx, vecLy, true);

    /// <summary>Returns a copy translated along the world Y axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmMovedWorldY(float fDistanceY)
        => new(vecPos + new Vector2(0f, fDistanceY), vecLx, vecLy, true);

    /// <summary>Returns a copy rotated counterclockwise about its origin.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle is non-finite.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmRotated(Rad rAngle)
    {
        if (!rAngle.bIsFinite())
            throw new ArgumentOutOfRangeException(nameof(rAngle), "Rotation angle must be finite.");

        float fCos = rAngle.fCos();
        float fSin = rAngle.fSin();
        return new(
            vecPos,
            fCos * vecLx + fSin * vecLy,
            -fSin * vecLx + fCos * vecLy,
            true);
    }

    /// <summary>Returns the equivalent <see cref="Matrix3x2"/> row-vector rigid transform.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x2 matAsMatrix3x2()
        => new(vecLx.X, vecLx.Y, vecLy.X, vecLy.Y, vecPos.X, vecPos.Y);

    /// <summary>Returns a copy at a new world position with unchanged orientation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmRepositioned(Vector2 vecNewPos)
        => new(vecNewPos, vecLx, vecLy, true);

    /// <summary>Returns a copy whose current X axis has been explicitly normalized.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Frame2d frmReorthonormalized() => new(vecPos, vecLx);

    /// <summary>Transforms a local point to world coordinates.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(in Frame2d frm, Vector2 vecLocal) => frm.vecPtToWorld(vecLocal);

    /// <summary>Composes two frames, applying the right operand first.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Frame2d operator *(in Frame2d frmA, in Frame2d frmB) => frmA.frmCompose(frmB);

    /// <summary>
    /// Interpolates position linearly and orientation along the shortest angular path. The
    /// interpolation parameter is clamped to [0,1].
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The interpolation parameter is non-finite.</exception>
    public static Frame2d frmInterpolate(in Frame2d frm0, in Frame2d frm1, float t)
    {
        if (!float.IsFinite(t))
            throw new ArgumentOutOfRangeException(nameof(t), "Interpolation parameter must be finite.");

        t = float.Clamp(t, 0f, 1f);
        float fCross = frm0.vecLx.X * frm1.vecLx.Y - frm0.vecLx.Y * frm1.vecLx.X;
        float fDot = Vector2.Dot(frm0.vecLx, frm1.vecLx);
        Rad rDelta = Rad.rAtan2(fCross, fDot);
        Frame2d frmRotated = frm0.frmRotated(rDelta * t);
        return new(Vector2.Lerp(frm0.vecPos, frm1.vecPos, t), frmRotated.vecLx, frmRotated.vecLy, true);
    }

    /// <inheritdoc/>
    public bool Equals(Frame2d frm)
        => vecPos.Equals(frm.vecPos) && vecLx.Equals(frm.vecLx) && vecLy.Equals(frm.vecLy);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Frame2d frm && Equals(frm);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(vecPos, vecLx, vecLy);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector2 vecNormalizedChecked(Vector2 vec, string strParamName)
    {
        ValidateFinite(vec, strParamName);
        float fLengthSquared = vec.LengthSquared();
        if (!(fLengthSquared > 0f) || !float.IsFinite(fLengthSquared))
            throw new ArgumentException("Direction must have finite non-zero length.", strParamName);

        return vec / float.Sqrt(fLengthSquared);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ValidateFinite(in Vector2 vec, string strParamName)
    {
        if (!bIsFinite(vec))
            throw new ArgumentException("Vector components must be finite.", strParamName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool bIsFinite(in Vector2 vec) => float.IsFinite(vec.X) && float.IsFinite(vec.Y);

    [Conditional("DEBUG")]
    static void AssertOrthonormal(in Vector2 vecX, in Vector2 vecY)
    {
        const float c_fUnitTolerance = 1e-5f;
        const float c_fOrthogonalTolerance = 1e-4f;
        const float c_fHandednessTolerance = 1e-5f;

        bool bUnit = float.Abs(vecX.LengthSquared() - 1f) < c_fUnitTolerance
            && float.Abs(vecY.LengthSquared() - 1f) < c_fUnitTolerance;
        bool bOrthogonal = float.Abs(Vector2.Dot(vecX, vecY)) < c_fOrthogonalTolerance;
        float fCross = vecX.X * vecY.Y - vecX.Y * vecY.X;
        bool bRightHanded = float.Abs(fCross - 1f) < c_fHandednessTolerance;
        Debug.Assert(bUnit && bOrthogonal && bRightHanded);
    }
}

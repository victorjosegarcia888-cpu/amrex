// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class Frame3dTests
{
    const float c_fToleranceSquared = 1e-10f;

    [Fact]
    public void ConstructorCreatesRightHandedOrthonormalBasis()
    {
        Frame3d frm = new(
            new Vector3(1f, 2f, 3f),
            new Vector3(0.2f, 0.3f, 0.9f),
            new Vector3(0.8f, 0.1f, 0.2f));

        AssertNear(1f, frm.vecLx.LengthSquared());
        AssertNear(1f, frm.vecLy.LengthSquared());
        AssertNear(1f, frm.vecLz.LengthSquared());
        AssertNear(0f, Vector3.Dot(frm.vecLx, frm.vecLy));
        AssertNear(0f, Vector3.Dot(frm.vecLx, frm.vecLz));
        AssertNear(0f, Vector3.Dot(frm.vecLy, frm.vecLz));
        AssertVectorNear(Vector3.Cross(frm.vecLz, frm.vecLx), frm.vecLy);
    }

    [Fact]
    public void PointRoundTripsThroughInverse()
    {
        Frame3d frm = new(
            new Vector3(3f, -2f, 7f),
            new Vector3(0.2f, 0.3f, 0.9f),
            new Vector3(0.8f, 0.1f, 0.2f));
        Vector3 vecLocal = new(4f, -5f, 6f);

        Vector3 vecRoundTrip = frm.frmInverse().vecPtToWorld(frm.vecPtToWorld(vecLocal));

        AssertVectorNear(vecLocal, vecRoundTrip);
    }

    [Fact]
    public void MatrixRoundTripsFrame()
    {
        Frame3d frm = new(
            new Vector3(3f, -2f, 7f),
            new Vector3(0.2f, 0.3f, 0.9f),
            new Vector3(0.8f, 0.1f, 0.2f));

        Frame3d frmRoundTrip = Frame3d.frmFromMatrix4x4(frm.matAsMatrix4x4());

        AssertVectorNear(frm.vecPos, frmRoundTrip.vecPos);
        AssertVectorNear(frm.vecLx, frmRoundTrip.vecLx);
        AssertVectorNear(frm.vecLy, frmRoundTrip.vecLy);
        AssertVectorNear(frm.vecLz, frmRoundTrip.vecLz);
    }

    [Fact]
    public void CompositionMatchesSequentialTransformation()
    {
        Frame3d frmA = new(new Vector3(1f, 2f, 3f), Vector3.UnitY, Vector3.UnitZ);
        Frame3d frmB = new(new Vector3(-2f, 1f, 4f), Vector3.UnitX, Vector3.UnitY);
        Vector3 vec = new(0.5f, -1f, 2f);

        Vector3 vecSequential = frmA.vecPtToWorld(frmB.vecPtToWorld(vec));
        Vector3 vecComposed = (frmA * frmB).vecPtToWorld(vec);

        AssertVectorNear(vecSequential, vecComposed);
    }

    [Fact]
    public void DirectionTransformationPreservesMagnitude()
    {
        Frame3d frm = new(
            Vector3.Zero,
            new Vector3(0.2f, 0.3f, 0.9f),
            new Vector3(0.8f, 0.1f, 0.2f));
        Vector3 vecDirection = new(3f, -4f, 12f);

        Vector3 vecTransformed = frm.vecDirToWorld(vecDirection);
        Vector3 vecRoundTrip = frm.vecDirFromWorld(vecTransformed);

        AssertNear(vecDirection.LengthSquared(), vecTransformed.LengthSquared());
        AssertVectorNear(vecDirection, vecRoundTrip);
    }

    [Fact]
    public void TranslationPreservesBasisExactly()
    {
        Frame3d frm = new(
            new Vector3(1f, 2f, 3f),
            new Vector3(0.2f, 0.3f, 0.9f),
            new Vector3(0.8f, 0.1f, 0.2f));

        Frame3d frmMoved = frm.frmMovedWorld(new Vector3(4f, 5f, 6f));

        Assert.Equal(frm.vecLx, frmMoved.vecLx);
        Assert.Equal(frm.vecLy, frmMoved.vecLy);
        Assert.Equal(frm.vecLz, frmMoved.vecLz);
    }

    [Fact]
    public void ConstructorRejectsDegenerateAndNonFiniteInput()
    {
        Assert.Throws<ArgumentException>(() =>
            new Frame3d(Vector3.Zero, Vector3.Zero, Vector3.UnitX));
        Assert.Throws<ArgumentException>(() =>
            new Frame3d(Vector3.Zero, Vector3.UnitZ, 2f * Vector3.UnitZ));
        Assert.Throws<ArgumentException>(() =>
            new Frame3d(new Vector3(float.NaN, 0f, 0f), Vector3.UnitZ, Vector3.UnitX));
    }

    static void AssertNear(float fExpected, float fActual)
        => Assert.True(float.Abs(fExpected - fActual) <= 1e-5f, $"Expected {fExpected}, got {fActual}.");

    static void AssertVectorNear(Vector3 vecExpected, Vector3 vecActual)
        => Assert.True(
            Vector3.DistanceSquared(vecExpected, vecActual) <= c_fToleranceSquared,
            $"Expected {vecExpected}, got {vecActual}.");
}

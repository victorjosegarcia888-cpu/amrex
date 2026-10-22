// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class Frame2dTests
{
    const float c_fToleranceSquared = 1e-10f;

    [Fact]
    public void ConstructorCreatesRightHandedOrthonormalBasis()
    {
        Frame2d frm = new(new Vector2(1f, 2f), new Vector2(2f, 3f));

        AssertNear(1f, frm.vecLx.LengthSquared());
        AssertNear(1f, frm.vecLy.LengthSquared());
        AssertNear(0f, Vector2.Dot(frm.vecLx, frm.vecLy));
        AssertNear(1f, frm.vecLx.X * frm.vecLy.Y - frm.vecLx.Y * frm.vecLy.X);
    }

    [Fact]
    public void PointRoundTripsThroughInverse()
    {
        Frame2d frm = new(new Vector2(3f, -2f), new Vector2(2f, 3f));
        Vector2 vecLocal = new(4f, -5f);

        Vector2 vecRoundTrip = frm.frmInverse().vecPtToWorld(frm.vecPtToWorld(vecLocal));

        AssertVectorNear(vecLocal, vecRoundTrip);
    }

    [Fact]
    public void MatrixRoundTripsFrame()
    {
        Frame2d frm = new(new Vector2(3f, -2f), new Vector2(2f, 3f));

        Frame2d frmRoundTrip = Frame2d.frmFromMatrix3x2(frm.matAsMatrix3x2());

        AssertVectorNear(frm.vecPos, frmRoundTrip.vecPos);
        AssertVectorNear(frm.vecLx, frmRoundTrip.vecLx);
        AssertVectorNear(frm.vecLy, frmRoundTrip.vecLy);
    }

    [Fact]
    public void CompositionMatchesSequentialTransformation()
    {
        Frame2d frmA = new(new Vector2(1f, 2f), new Vector2(2f, 3f));
        Frame2d frmB = new(new Vector2(-2f, 1f), new Vector2(-1f, 4f));
        Vector2 vec = new(0.5f, -1f);

        AssertVectorNear(
            frmA.vecPtToWorld(frmB.vecPtToWorld(vec)),
            (frmA * frmB).vecPtToWorld(vec));
    }

    [Fact]
    public void PositiveRotationMapsXToY()
    {
        Frame2d frm = Frame2d.World.frmRotated(Rad.Deg90);

        AssertVectorNear(Vector2.UnitY, frm.vecLx);
        AssertVectorNear(-Vector2.UnitX, frm.vecLy);
    }

    [Fact]
    public void InterpolationUsesShortestAngularPath()
    {
        Frame2d frm0 = Frame2d.World.frmRotated(Rad.rFromDeg(170f));
        Frame2d frm1 = Frame2d.World.frmRotated(Rad.rFromDeg(-170f));

        Frame2d frmMid = Frame2d.frmInterpolate(frm0, frm1, 0.5f);

        AssertVectorNear(-Vector2.UnitX, frmMid.vecLx);
    }

    [Fact]
    public void ConstructorRejectsDegenerateAndNonFiniteInput()
    {
        Assert.Throws<ArgumentException>(() => new Frame2d(Vector2.Zero, Vector2.Zero));
        Assert.Throws<ArgumentException>(() =>
            new Frame2d(new Vector2(float.NaN, 0f), Vector2.UnitX));
    }

    static void AssertNear(float fExpected, float fActual)
        => Assert.True(float.Abs(fExpected - fActual) <= 1e-5f, $"Expected {fExpected}, got {fActual}.");

    static void AssertVectorNear(Vector2 vecExpected, Vector2 vecActual)
        => Assert.True(
            Vector2.DistanceSquared(vecExpected, vecActual) <= c_fToleranceSquared,
            $"Expected {vecExpected}, got {vecActual}.");
}

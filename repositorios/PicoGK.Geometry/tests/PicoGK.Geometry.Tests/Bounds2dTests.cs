// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class Bounds2dTests
{
    [Fact]
    public void DefaultIsEmptyAndHasNoExtrema()
    {
        Bounds2d oBounds = default;

        Assert.True(oBounds.bIsEmpty);
        Assert.Equal(Bounds2d.Empty, oBounds);
        Assert.Throws<InvalidOperationException>(() => oBounds.vecMin);
        Assert.False(oBounds.bContains(Vector2.Zero));
    }

    [Fact]
    public void InclusionBuildsBoundsFromEmpty()
    {
        Bounds2d oBounds = Bounds2d.Empty
            .oIncluded(new Vector2(3f, -2f))
            .oIncluded(new Vector2(-1f, 5f));

        Assert.Equal(new Vector2(-1f, -2f), oBounds.vecMin);
        Assert.Equal(new Vector2(3f, 5f), oBounds.vecMax);
        Assert.Equal(new Vector2(1f, 1.5f), oBounds.vecCenter);
    }

    [Fact]
    public void TouchingBoundsIntersect()
    {
        Bounds2d oA = new(Vector2.Zero, new Vector2(2f));
        Bounds2d oB = new(new Vector2(2f, 1f), new Vector2(4f, 3f));

        Bounds2d oIntersection = oA.oIntersection(oB);

        Assert.True(oA.bIntersects(oB));
        Assert.Equal(new Vector2(2f, 1f), oIntersection.vecMin);
        Assert.Equal(new Vector2(2f), oIntersection.vecMax);
    }

    [Fact]
    public void TransformReturnsAxisAlignedEnvelope()
    {
        Bounds2d oBounds = new(new Vector2(-1f, -2f), new Vector2(1f, 2f));
        Frame2d frm = new(new Vector2(3f, 4f), Rad.Deg90);

        Bounds2d oTransformed = oBounds.oTransformed(frm);

        AssertVectorNear(new Vector2(1f, 3f), oTransformed.vecMin);
        AssertVectorNear(new Vector2(5f, 5f), oTransformed.vecMax);
    }

    [Fact]
    public void ConstructionRejectsInvalidExtrema()
    {
        Assert.Throws<ArgumentException>(() => new Bounds2d(Vector2.One, Vector2.Zero));
        Assert.Throws<ArgumentException>(() =>
            new Bounds2d(Vector2.Zero, new Vector2(float.PositiveInfinity, 1f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Bounds2d.oFromPoint(Vector2.Zero).oExpanded(-1f));
    }

    static void AssertVectorNear(Vector2 vecExpected, Vector2 vecActual)
        => Assert.True(Vector2.DistanceSquared(vecExpected, vecActual) <= 1e-10f);
}

// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class Bounds3dTests
{
    [Fact]
    public void DefaultIsEmptyAndHasNoExtrema()
    {
        Bounds3d oBounds = default;

        Assert.True(oBounds.bIsEmpty);
        Assert.Equal(Bounds3d.Empty, oBounds);
        Assert.Throws<InvalidOperationException>(() => oBounds.vecMax);
        Assert.False(oBounds.bContains(Vector3.Zero));
    }

    [Fact]
    public void FromPointsCalculatesExtrema()
    {
        Vector3[] avecPoints =
        [
            new(3f, -2f, 7f),
            new(-1f, 5f, 2f),
            new(2f, 1f, -4f)
        ];

        Bounds3d oBounds = Bounds3d.oFromPoints(avecPoints);

        Assert.Equal(new Vector3(-1f, -2f, -4f), oBounds.vecMin);
        Assert.Equal(new Vector3(3f, 5f, 7f), oBounds.vecMax);
    }

    [Fact]
    public void DisjointIntersectionIsEmpty()
    {
        Bounds3d oA = new(Vector3.Zero, Vector3.One);
        Bounds3d oB = new(new Vector3(2f), new Vector3(3f));

        Assert.False(oA.bIntersects(oB));
        Assert.True(oA.oIntersection(oB).bIsEmpty);
    }

    [Fact]
    public void TransformReturnsAxisAlignedEnvelope()
    {
        Bounds3d oBounds = new(new Vector3(-1f, -2f, -3f), new Vector3(1f, 2f, 3f));
        Frame3d frm = new(new Vector3(3f, 4f, 5f), Vector3.UnitZ, Vector3.UnitY);

        Bounds3d oTransformed = oBounds.oTransformed(frm);

        AssertVectorNear(new Vector3(1f, 3f, 2f), oTransformed.vecMin);
        AssertVectorNear(new Vector3(5f, 5f, 8f), oTransformed.vecMax);
    }

    [Fact]
    public void EmptyBoundsAreNeutralForUnion()
    {
        Bounds3d oBounds = new(Vector3.Zero, Vector3.One);

        Assert.Equal(oBounds, oBounds.oIncluded(Bounds3d.Empty));
        Assert.Equal(oBounds, Bounds3d.Empty.oIncluded(oBounds));
        Assert.True(oBounds.bContains(Bounds3d.Empty));
    }

    static void AssertVectorNear(Vector3 vecExpected, Vector3 vecActual)
        => Assert.True(Vector3.DistanceSquared(vecExpected, vecActual) <= 1e-10f);
}

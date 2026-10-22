// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using PicoGK.Geometry;
using Xunit;

namespace PicoGKGeometryPackageTests;

public sealed class PackageSmokeTests
{
    [Fact]
    public void PublicGeometryApiIsUsableFromPackage()
    {
        Frame3d frm = Frame3d.frmFromPos(new Vector3(1f, 2f, 3f));
        Bounds3d oBounds = new(Vector3.Zero, Vector3.One);
        Rad rHalfTurn = Rad.rFromDeg(180f);
        Triangle triExpected = new(0, 1, 2);
        ITriangleMesh3d xMesh = new TestTriangleMesh();

        Span<Vector3> avecVertices = stackalloc Vector3[xMesh.nVertexCount];
        Span<Triangle> atriTriangles = stackalloc Triangle[xMesh.nTriangleCount];
        xMesh.CopyVertices(avecVertices);
        xMesh.CopyTriangles(atriTriangles);

        Assert.Equal(new Vector3(1f, 2f, 3f), frm.vecPos);
        Assert.True(oBounds.bContains(new Vector3(0.5f)));
        Assert.Equal(float.Pi, rHalfTurn.fRad);
        Assert.Equal(triExpected, atriTriangles[0]);
        Assert.Equal(Vector3.UnitX, avecVertices[1]);
        Assert.True(typeof(IPath2d).IsAssignableFrom(typeof(IPath2dNormalizedArcLength)));
        Assert.True(typeof(IPath3d).IsAssignableFrom(typeof(IPath3dNormalizedArcLength)));
    }

    sealed class TestTriangleMesh : ITriangleMesh3d
    {
        public int nVertexCount => 3;
        public int nTriangleCount => 1;

        public void CopyVertices(Span<Vector3> avecDestination)
        {
            ReadOnlySpan<Vector3> avecSource =
            [
                Vector3.Zero,
                Vector3.UnitX,
                Vector3.UnitY,
            ];
            avecSource.CopyTo(avecDestination);
        }

        public void CopyTriangles(Span<Triangle> atriDestination)
        {
            ReadOnlySpan<Triangle> atriSource = [new Triangle(0, 1, 2)];
            atriSource.CopyTo(atriDestination);
        }
    }
}

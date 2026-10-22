// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class TriangleTests
{
    [Fact]
    public void HasStableSequentialLayout()
    {
        Assert.Equal(12, Unsafe.SizeOf<Triangle>());
        Assert.Equal(IntPtr.Zero, Marshal.OffsetOf<Triangle>(nameof(Triangle.A)));
        Assert.Equal(new IntPtr(4), Marshal.OffsetOf<Triangle>(nameof(Triangle.B)));
        Assert.Equal(new IntPtr(8), Marshal.OffsetOf<Triangle>(nameof(Triangle.C)));
    }

    [Fact]
    public void EqualityUsesAllIndices()
    {
        Triangle tri = new(2, 5, 7);

        Assert.Equal(new Triangle(2, 5, 7), tri);
        Assert.NotEqual(new Triangle(2, 5, 8), tri);
        Assert.Equal("(2, 5, 7)", tri.ToString());
    }
}

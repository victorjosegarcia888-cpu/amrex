// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class QuadTests
{
    [Fact]
    public void HasStableSequentialLayout()
    {
        Assert.Equal(16, Unsafe.SizeOf<Quad>());
        Assert.Equal(IntPtr.Zero, Marshal.OffsetOf<Quad>(nameof(Quad.A)));
        Assert.Equal(new IntPtr(4), Marshal.OffsetOf<Quad>(nameof(Quad.B)));
        Assert.Equal(new IntPtr(8), Marshal.OffsetOf<Quad>(nameof(Quad.C)));
        Assert.Equal(new IntPtr(12), Marshal.OffsetOf<Quad>(nameof(Quad.D)));
    }

    [Fact]
    public void EqualityUsesAllIndices()
    {
        Quad quad = new(2, 5, 7, 11);

        Assert.Equal(new Quad(2, 5, 7, 11), quad);
        Assert.NotEqual(new Quad(2, 5, 7, 12), quad);
        Assert.Equal("(2, 5, 7, 11)", quad.ToString());
    }
}

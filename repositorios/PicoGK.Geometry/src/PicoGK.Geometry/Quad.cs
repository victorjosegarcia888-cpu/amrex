// SPDX-License-Identifier: Apache-2.0

using System.Runtime.InteropServices;

namespace PicoGK.Geometry;

/// <summary>Quad defined by four zero-based unsigned vertex indices.</summary>
/// <remarks>
/// The sequential layout is part of the interoperability contract. Degenerate quads are
/// representable; validity relative to a vertex buffer is the responsibility of the mesh source.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Quad : IEquatable<Quad>
{
    /// <summary>First vertex index.</summary>
    public readonly uint A;

    /// <summary>Second vertex index.</summary>
    public readonly uint B;

    /// <summary>Third vertex index.</summary>
    public readonly uint C;

    /// <summary>Fourth vertex index.</summary>
    public readonly uint D;

    public Quad(uint A, uint B, uint C, uint D)
    {
        this.A = A;
        this.B = B;
        this.C = C;
        this.D = D;
    }

    /// <inheritdoc/>
    public bool Equals(Quad quad) => A == quad.A && B == quad.B && C == quad.C && D == quad.D;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Quad quad && Equals(quad);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(A, B, C, D);

    public static bool operator ==(Quad quadLeft, Quad quadRight) => quadLeft.Equals(quadRight);
    public static bool operator !=(Quad quadLeft, Quad quadRight) => !quadLeft.Equals(quadRight);

    /// <inheritdoc/>
    public override string ToString() => $"({A}, {B}, {C}, {D})";
}

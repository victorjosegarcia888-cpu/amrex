// SPDX-License-Identifier: Apache-2.0

using Xunit;

namespace PicoGK.Geometry.Tests;

public sealed class RadTests
{
    [Fact]
    public void DegreesRoundTrip()
    {
        Rad r = Rad.rFromDeg(90f);

        Assert.True(float.Abs(r.fRad - float.Pi / 2f) <= Rad.fToleranceDefault);
        Assert.True(float.Abs(r.fDeg - 90f) <= Rad.fToleranceDefault);
    }

    [Fact]
    public void PeriodicComparisonTreatsFullTurnAsZero()
    {
        Assert.True(Rad.Zero.bAlmostEqualPeriodic(Rad.Deg360));
        Assert.False(Rad.Zero.bAlmostEqual(Rad.Deg360));
    }

    [Fact]
    public void PositiveNormalizationWrapsNegativeAngles()
    {
        Rad r = Rad.rFromDeg(-90f).rNormalizedPositive();

        Assert.True(r.bAlmostEqual(Rad.rFromDeg(270f)));
    }

    [Fact]
    public void ClampedInverseTrigonometryStaysFinite()
    {
        Assert.True(Rad.rAcosClamped(1.01f).bIsFinite());
        Assert.True(Rad.rAsinClamped(-1.01f).bIsFinite());
    }
}

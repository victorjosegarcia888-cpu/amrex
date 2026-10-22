//
// SPDX-License-Identifier: Apache-2.0
//
// Time-varying boundary condition helper for thermal simulations.
//

using System;

namespace Leap71
{
    public static class ThermalBoundaryConditions
    {
        public static float GetLeftBoundaryTemperature(float fTime)
        {
            // Example: ramp from ambient to peak over 1 second
            float fAmbient = 293.15f;
            float fPeak = 2200f;
            float fRampDuration = 1.0f;
            float fFactor = MathF.Min(fTime / fRampDuration, 1.0f);
            return fAmbient + fFactor * (fPeak - fAmbient);
        }

        public static float GetRightBoundaryTemperature(float fTime)
        {
            // Example: right side remains ambient temperature
            return 293.15f;
        }

        public static void Apply(ThermalSimulator oSimulator, float fTime)
        {
            oSimulator.SetNodeTemperature(0, GetLeftBoundaryTemperature(fTime));
            oSimulator.SetNodeTemperature(oSimulator.aNodes.Count - 1, GetRightBoundaryTemperature(fTime));
        }
    }
}

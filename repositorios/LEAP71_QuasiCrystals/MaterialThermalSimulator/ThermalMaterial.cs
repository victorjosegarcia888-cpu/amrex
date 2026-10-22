//
// SPDX-License-Identifier: Apache-2.0
//
// Simplified thermal material model for conductivity and heat capacity.
//

namespace Leap71
{
    public class ThermalMaterial
    {
        public string sName { get; set; }
        public float fDensity { get; set; }
        public float fSpecificHeat { get; set; }
        public float fConductivity { get; set; }

        public ThermalMaterial(string sName, float fDensity, float fSpecificHeat, float fConductivity)
        {
            this.sName = sName;
            this.fDensity = fDensity;
            this.fSpecificHeat = fSpecificHeat;
            this.fConductivity = fConductivity;
        }
    }
}

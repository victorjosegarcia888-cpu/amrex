//
// SPDX-License-Identifier: Apache-2.0
//
// Conceptual reactive matrix model for UHTCMC systems.
//

using System.Collections.Generic;

namespace Leap71
{
    public class ReactiveMatrixModel
    {
        public float fZrB2Fraction { get; set; }
        public float fSiCFraction { get; set; }
        public float fTaB2Fraction { get; set; }

        public ReactiveMatrixModel(float fZrB2, float fSiC, float fTaB2)
        {
            fZrB2Fraction = fZrB2;
            fSiCFraction = fSiC;
            fTaB2Fraction = fTaB2;
        }

        public List<string> aGetStageReactions()
        {
            return new List<string>()
            {
                "SiC + O2 -> SiO2 + CO",
                "ZrB2 + O2 -> ZrO2 + B2O3",
                "TaB2 + O2 -> Ta2O5 + B2O3"
            };
        }

        public string sGetStageSummary()
        {
            return "At intermediate temperatures, SiC and ZrB2 form a borosilicate glass that flows by capillarity into microcracks. " +
                   "At very high temperatures, TaB2 stabilizes the glass by producing Ta2O5, increasing viscosity and lowering oxygen diffusion. " +
                   "During cooling, ZrO2 phase transformation generates local compression and helps choke internal microcracks.";
        }

        public List<string> aGetDesignAdvantages()
        {
            return new List<string>()
            {
                "Active protection through controlled oxidation.",
                "Sealing of cracks in the liquid phase.",
                "Increased thermal and mechanical stability at high temperature."
            };
        }
    }
}

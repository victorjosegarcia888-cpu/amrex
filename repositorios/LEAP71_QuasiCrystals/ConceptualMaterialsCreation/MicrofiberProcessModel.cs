//
// SPDX-License-Identifier: Apache-2.0
//
// Prototype model for microfiber processing and fiber network creation.
//

using System.Collections.Generic;

namespace Leap71
{
    public class MicrofiberProcessModel
    {
        public float fFiberDiameterMM { get; set; }
        public float fFiberDensity { get; set; }
        public float fTargetVoidFraction { get; set; }

        public MicrofiberProcessModel(float fDiameterMM, float fDensity, float fVoidFraction)
        {
            fFiberDiameterMM = fDiameterMM;
            fFiberDensity = fDensity;
            fTargetVoidFraction = fVoidFraction;
        }

        public string sGetProcessSummary()
        {
            return "Microfiber processing requires fine control of fiber diameter, packing density and void fraction. " +
                   "The goal is to produce a textile preform that can be infiltrated by a reactive UHTC matrix.";
        }

        public List<string> aGetProcessSteps()
        {
            return new List<string>
            {
                "Select high-temperature resistant microfibers (carbon, SiC, or hybrid).",
                "Weave or braid a fine 3D preform with continuous through-thickness reinforcement.",
                "Control fiber spacing to achieve the desired void fraction for infiltration.",
                "Use vacuum/pressure or sol-gel assisted infiltration to introduce the reactive matrix."            };
        }
    }
}

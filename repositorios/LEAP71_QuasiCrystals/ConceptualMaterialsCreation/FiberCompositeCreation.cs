//
// SPDX-License-Identifier: Apache-2.0
//
// Conceptual fiber composite creation prototype.
//

using System.Collections.Generic;

namespace Leap71
{
    public class FiberCompositeCreation
    {
        public struct CreationZone
        {
            public string sName;
            public float fDensity;
            public float fFiberDiameter;
            public string sMatrixDescription;
        }

        public List<CreationZone> aZones { get; private set; }

        public FiberCompositeCreation()
        {
            aZones = new List<CreationZone>();
            aZones.Add(new CreationZone() { sName = "Outer Skin", fDensity = 0.98f, fFiberDiameter = 0.10f, sMatrixDescription = "Dense reactive ZrB2-SiC-TaB2 glass matrix" });
            aZones.Add(new CreationZone() { sName = "Transition", fDensity = 0.75f, fFiberDiameter = 0.08f, sMatrixDescription = "Hybrid porous matrix with targeted infiltration" });
            aZones.Add(new CreationZone() { sName = "Core", fDensity = 0.55f, fFiberDiameter = 0.06f, sMatrixDescription = "Support lattice with controlled porosity" });
        }

        public void AddZone(string sName, float fDensity, float fFiberDiameter, string sMatrixDescription)
        {
            aZones.Add(new CreationZone() { sName = sName, fDensity = fDensity, fFiberDiameter = fFiberDiameter, sMatrixDescription = sMatrixDescription });
        }

        public string sGetCreationSummary()
        {
            return "This concept defines a small-fiber composite architecture with graded zones for skin, transition and core. " +
                   "It is designed for subsequent reactive matrix infiltration and consolidation.";
        }
    }
}

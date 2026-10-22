//
// SPDX-License-Identifier: Apache-2.0
//
// Conceptual 3D fiber composite design for smaller fiber bundles.
//

using System.Collections.Generic;
using System.Numerics;

namespace Leap71
{
    public class FiberCompositeConcept
    {
        public struct FiberZone
        {
            public string sName;
            public float fDensity;
            public float fFiberDiameter;
            public string sMatrixType;
        }

        public List<FiberZone> aZones;

        public FiberCompositeConcept()
        {
            aZones = new List<FiberZone>();
            aZones.Add(new FiberZone() { sName = "Skin", fDensity = 0.98f, fFiberDiameter = 0.15f, sMatrixType = "ZrB2-SiC-TaB2 reactive glass" });
            aZones.Add(new FiberZone() { sName = "Transition", fDensity = 0.75f, fFiberDiameter = 0.10f, sMatrixType = "ZrB2-SiC-TaB2 hybrid matrix" });
            aZones.Add(new FiberZone() { sName = "Core", fDensity = 0.55f, fFiberDiameter = 0.08f, sMatrixType = "Porous support lattice" });
        }

        public string sGetConceptSummary()
        {
            return "A fine 3D fiber preform is combined with a reactive UHTC matrix. " +
                   "Skin, transition, and core zones are defined by fiber density, diameter and matrix infiltration profile.";
        }

        public void AddZone(string sName, float fDensity, float fFiberDiameter, string sMatrixType)
        {
            aZones.Add(new FiberZone() { sName = sName, fDensity = fDensity, fFiberDiameter = fFiberDiameter, sMatrixType = sMatrixType });
        }
    }
}

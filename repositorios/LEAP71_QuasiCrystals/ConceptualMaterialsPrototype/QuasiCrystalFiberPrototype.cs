//
// SPDX-License-Identifier: Apache-2.0
//
// Prototype entry point linking quasicrystal geometry to a fiber composite concept.
//

using System;
using System.Collections.Generic;
using PicoGK;
using System.Numerics;

namespace Leap71
{
    using ShapeKernel;
    using AperiodicTiling;

    public class QuasiCrystalFiberPrototype
    {
        public static void RunPrototype()
        {
            Library.Log("Starting quasicrystal fiber composite prototype...");

            QuasiTile oInitialTile = new QuasiTile_02(new LocalFrame(), 40);
            List<QuasiTile> aInitialTiles = new List<QuasiTile>() { oInitialTile };
            QuasiCrystal oCrystal = new QuasiCrystal(2, aInitialTiles);

            float fBeamRadius = 0.6f;
            Voxels voxWireframe = oCrystal.voxGetWireframe(1, fBeamRadius);
            Sh.PreviewVoxels(voxWireframe, Cp.clrBlue);

            ReactiveMatrixModel oMatrixModel = new ReactiveMatrixModel(0.40f, 0.40f, 0.20f);
            FiberCompositeConcept oConcept = new FiberCompositeConcept();

            Library.Log(oMatrixModel.sGetStageSummary());
            Library.Log(oConcept.sGetConceptSummary());
        }
    }
}

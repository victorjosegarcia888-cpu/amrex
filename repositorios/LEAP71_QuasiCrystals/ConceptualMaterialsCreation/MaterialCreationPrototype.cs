//
// SPDX-License-Identifier: Apache-2.0
//
// Prototype entry point for microfiber composite creation.
//

using System;
using PicoGK;

namespace Leap71
{
    public class MaterialCreationPrototype
    {
        public static void Run()
        {
            Library.Log("Starting microfiber composite creation prototype...");

            MicrofiberProcessModel oProcessModel = new MicrofiberProcessModel(0.08f, 0.95f, 0.20f);
            FiberCompositeCreation oCreation = new FiberCompositeCreation();

            Library.Log(oProcessModel.sGetProcessSummary());
            foreach (string sStep in oProcessModel.aGetProcessSteps())
            {
                Library.Log(sStep);
            }

            Library.Log(oCreation.sGetCreationSummary());
            foreach (var oZone in oCreation.aZones)
            {
                Library.Log($"Zone: {oZone.sName}, Density={oZone.fDensity}, Diameter={oZone.fFiberDiameter}, Matrix={oZone.sMatrixDescription}");
            }
        }
    }
}

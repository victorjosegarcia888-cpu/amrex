//
// SPDX-License-Identifier: Apache-2.0
//
// Example usage of the simplified thermal simulator.
//

using System;

namespace Leap71
{
    public class ThermalSimulationExample
    {
        public static void RunExample()
        {
            float fLength = 0.1f; // 10 cm
            int nNodes = 21;
            float fTimeStep = 0.01f;

            ThermalMaterial oSkin = new ThermalMaterial("Reactive Skin", 7800f, 520f, 6.0f);
            ThermalMaterial oTransition = new ThermalMaterial("Transition Zone", 5200f, 700f, 4.0f);
            ThermalMaterial oCore = new ThermalMaterial("Core Lattice", 3200f, 900f, 2.5f);

            ThermalSimulator oSimulator = new ThermalSimulator(nNodes, fLength, fTimeStep);

            // assign materials by zone
            for (int i = 0; i < nNodes; i++)
            {
                if (i < 5)
                {
                    oSimulator.SetNodeMaterial(i, oSkin);
                    oSimulator.SetNodeTemperature(i, 293.15f);
                }
                else if (i < 14)
                {
                    oSimulator.SetNodeMaterial(i, oTransition);
                    oSimulator.SetNodeTemperature(i, 293.15f);
                }
                else
                {
                    oSimulator.SetNodeMaterial(i, oCore);
                    oSimulator.SetNodeTemperature(i, 293.15f);
                }
            }

            // run simulation with time-varying boundary conditions
            for (int i = 0; i < 200; i++)
            {
                float fTime = i * fTimeStep;
                ThermalBoundaryConditions.Apply(oSimulator, fTime);
                oSimulator.Step();
            }

            for (int i = 0; i < nNodes; i++)
            {
                Console.WriteLine($"Node {i}: {oSimulator.aNodes[i].fTemperature:F1} K");
            }

            string sOutputFile = "Temperatures.csv";
            SimulationOutputExport.ExportNodeData(sOutputFile, oSimulator.aNodes);
            MatlabIntegrationHelper.CreateMatlabReadme("ReadThermalResults.m", sOutputFile);

            Console.WriteLine($"Exported results to {sOutputFile} and MATLAB helper ReadThermalResults.m");
        }
    }
}

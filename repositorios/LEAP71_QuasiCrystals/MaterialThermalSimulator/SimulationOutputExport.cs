//
// SPDX-License-Identifier: Apache-2.0
//
// Helper for exporting simulation results to CSV.
//

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Leap71
{
    public static class SimulationOutputExport
    {
        public static void ExportTemperatures(string sFilePath, List<float> aTemperatures)
        {
            using (StreamWriter oWriter = new StreamWriter(sFilePath))
            {
                oWriter.WriteLine("node,temperature");
                for (int i = 0; i < aTemperatures.Count; i++)
                {
                    oWriter.WriteLine($"{i},{aTemperatures[i].ToString(CultureInfo.InvariantCulture)}");
                }
            }
        }

        public static void ExportNodeData(string sFilePath, List<ThermalSimulator.Node> aNodes)
        {
            using (StreamWriter oWriter = new StreamWriter(sFilePath))
            {
                oWriter.WriteLine("node,temperature,material,density,specificHeat,conductivity");
                for (int i = 0; i < aNodes.Count; i++)
                {
                    ThermalSimulator.Node oNode = aNodes[i];
                    string sMaterial = oNode.oMaterial?.sName ?? "unknown";
                    float fDensity = oNode.oMaterial?.fDensity ?? 0f;
                    float fHeat = oNode.oMaterial?.fSpecificHeat ?? 0f;
                    float fConductivity = oNode.oMaterial?.fConductivity ?? 0f;
                    oWriter.WriteLine($"{i},{oNode.fTemperature.ToString(CultureInfo.InvariantCulture)},{sMaterial},{fDensity.ToString(CultureInfo.InvariantCulture)},{fHeat.ToString(CultureInfo.InvariantCulture)},{fConductivity.ToString(CultureInfo.InvariantCulture)}");
                }
            }
        }
    }
}

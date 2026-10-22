//
// SPDX-License-Identifier: Apache-2.0
//
// Utility for generating MATLAB-compatible CSV import instructions.
//

using System.IO;

namespace Leap71
{
    public static class MatlabIntegrationHelper
    {
        public static void CreateMatlabReadme(string sFilePath, string sDataFileName)
        {
            using (StreamWriter oWriter = new StreamWriter(sFilePath))
            {
                oWriter.WriteLine("% MATLAB import instructions for thermal simulator results");
                oWriter.WriteLine($"data = readtable('{sDataFileName}');");
                oWriter.WriteLine("plot(data.node, data.temperature, '-o');");
                oWriter.WriteLine("xlabel('Node');");
                oWriter.WriteLine("ylabel('Temperature (K)');");
                oWriter.WriteLine("title('1D Thermal Simulation Results');");
                oWriter.WriteLine("grid on;");
            }
        }
    }
}

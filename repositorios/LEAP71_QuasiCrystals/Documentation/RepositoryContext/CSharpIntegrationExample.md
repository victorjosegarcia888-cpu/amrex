# Recommended C# integration pattern

The following pattern is recommended for executing quasicrystal tasks and linking them to voxel structures in a C# workflow.

```csharp
// COMPONENT: Program.cs
// CONTEXT: Execution of LEAP71_QuasiCrystals aperiodic patterns on PicoGK

using System;
using PicoGK;
using Leap71.QuasiCrystalExamples;

namespace Leap71.AperiodicDesign
{
    public class Program
    {
        public static void Main(string[] aArgs)
        {
            try
            {
                // Voxel size configuration (0.4 mm for inspection resolution)
                float fVoxelSizeMM = 0.4f;

                Library.Log("Starting generation of aperiodic structures...");

                // 1. 2D Penrose pattern demonstration
                // Library.Go(fVoxelSizeMM, PenrosePatternShowCase.Task);

                // 2. 3D icosahedral tile demonstration for quasicrystals
                Library.Go(fVoxelSizeMM, QuasiCrystalShowCase.IntroduceQuasiTilesTask);
            }
            catch (Exception oEx)
            {
                Console.WriteLine("Error during volumetric generation:");
                Console.WriteLine(oEx.ToString());
            }
        }
    }
}
```

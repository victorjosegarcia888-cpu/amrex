//
// SPDX-License-Identifier: Apache-2.0
//
// Simplified one-dimensional transient thermal solver.
//

using System;
using System.Collections.Generic;

namespace Leap71
{
    public class ThermalSimulator
    {
        public class Node
        {
            public float fTemperature;
            public ThermalMaterial? oMaterial;
        }

        public List<Node> aNodes;
        public float fDx;
        public float fDt;

        public ThermalSimulator(int nNodes, float fLength, float fTimeStep)
        {
            aNodes = new List<Node>(nNodes);
            fDx = fLength / (nNodes - 1);
            fDt = fTimeStep;
            for (int i = 0; i < nNodes; i++)
            {
                aNodes.Add(new Node() { fTemperature = 293.15f, oMaterial = null });
            }
        }

        public void SetNodeMaterial(int iNode, ThermalMaterial oMaterial)
        {
            aNodes[iNode].oMaterial = oMaterial;
        }

        public void SetNodeTemperature(int iNode, float fTemperature)
        {
            aNodes[iNode].fTemperature = fTemperature;
        }

        public void Step()
        {
            int n = aNodes.Count;
            float[] aNewTemp = new float[n];

            for (int i = 0; i < n; i++)
            {
                aNewTemp[i] = aNodes[i].fTemperature;
            }

            for (int i = 1; i < n - 1; i++)
            {
                ThermalMaterial? oLeft = aNodes[i - 1].oMaterial;
                ThermalMaterial? oRight = aNodes[i + 1].oMaterial;
                ThermalMaterial oCenter = aNodes[i].oMaterial ?? throw new InvalidOperationException("Center material missing.");

                float fKLeft = oLeft?.fConductivity ?? oCenter.fConductivity;
                float fKRight = oRight?.fConductivity ?? oCenter.fConductivity;

                float fTempLeft = aNodes[i - 1].fTemperature;
                float fTempRight = aNodes[i + 1].fTemperature;
                float fTempCenter = aNodes[i].fTemperature;

                float fAlpha = oCenter.fConductivity / (oCenter.fDensity * oCenter.fSpecificHeat);
                aNewTemp[i] = fTempCenter + fAlpha * fDt / (fDx * fDx) * (fTempLeft - 2f * fTempCenter + fTempRight);
            }

            for (int i = 1; i < n - 1; i++)
            {
                aNodes[i].fTemperature = aNewTemp[i];
            }
        }

        public void Run(int nSteps)
        {
            for (int i = 0; i < nSteps; i++)
            {
                Step();
            }
        }
    }
}

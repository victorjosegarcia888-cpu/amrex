using System;
using System.Numerics;
using PicoGK;
using Leap71.ShapeKernel;
using Leap71.LatticeLibrary;

namespace AeroEngine.UHTC.ThermalShield
{
    public class HermeticQuasiUHTCShield
    {
        private const float ScaleFactor = 0.08f;
        private const float ShellThicknessMm = 3.5f;

        public static float EvaluateQuasiField(Vector3 vPos)
        {
            float fTau = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;
            Vector3[] aKVectors = new Vector3[]
            {
                Vector3.Normalize(new Vector3(1, fTau, 0)),
                Vector3.Normalize(new Vector3(-1, fTau, 0)),
                Vector3.Normalize(new Vector3(0, 1, fTau)),
                Vector3.Normalize(new Vector3(0, -1, fTau)),
                Vector3.Normalize(new Vector3(fTau, 0, 1)),
                Vector3.Normalize(new Vector3(fTau, 0, -1))
            };
            float fSum = 0.0f;
            foreach (var vK in aKVectors)
            {
                fSum += MathF.Cos(Vector3.Dot(vK, vPos * ScaleFactor));
            }
            return fSum;
        }

        public static Voxels BuildHermeticAerospikeShield(BaseShape oAerospikeCone)
        {
            Voxels oBaseVoxels = oAerospikeCone.voxGetVoxels();
            Voxels oHermeticShell = new Voxels(oBaseVoxels);
            Voxels oInnerCoreBoundary = new Voxels(oBaseVoxels);
            oInnerCoreBoundary.Offset(-ShellThicknessMm);

            Voxels oQuasiCore = new Voxels();
            oQuasiCore.RenderImplicit((Vector3 vPt) =>
            {
                float fX = vPt.X * 0.1f, fY = vPt.Y * 0.1f, fZ = vPt.Z * 0.1f;
                float fGyroid = MathF.Sin(fX) * MathF.Cos(fY) + MathF.Sin(fY) * MathF.Cos(fZ) + MathF.Sin(fZ) * MathF.Cos(fX);
                float fQuasi = EvaluateQuasiField(vPt);
                float fField = fGyroid + 0.35f * fQuasi;
                return MathF.Abs(fField) - 0.25f;
            }, oAerospikeCone.oGetBoundingBox());

            oQuasiCore.Intersect(oInnerCoreBoundary);

            Voxels oFinalMonolithicShield = new Voxels(oBaseVoxels);
            oFinalMonolithicShield.Subtract(oInnerCoreBoundary);
            oFinalMonolithicShield.BoolAdd(oQuasiCore);
            return oFinalMonolithicShield;
        }
    }
}

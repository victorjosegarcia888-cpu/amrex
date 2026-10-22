// SPDX-License-Identifier: Apache-2.0
//
// Quasicrystal face accelerator kernel for Vitis HLS.
// Generates print-path beam segments from icosahedral face vertices.
// Matches geometry defined in Leap71.AperiodicTiling.IcosehedralFace.
//
// C# connector enum mapping:
//   EConnector.ARROW    = 0
//   EConnector.TRIANGLE = 1
//   EConnector.LINE     = 2
//
// Output layout (flat float array):
//   outputToolpaths[0] = total segment count
//   outputToolpaths[1 + k*7 .. 1 + k*7 + 6] = segment k:
//       x0, y0, z0, x1, y1, z1, radius
//
// Compile:
//   v++ -c -t hw_emu -f <platform> -k AccelerateQuasiCrystalBatch quasicrystal_accel.cpp -o quasicrystal_accel.xo
//   v++ -l -t hw_emu -f <platform> quasicrystal_accel.xo -o quasicrystal_accel.xclbin

#include <hls_math.h>

struct Vector3 { float x, y, z; };

static inline Vector3 vecLerp(const Vector3& a, const Vector3& b, float t)
{
    Vector3 r;
    r.x = a.x + t * (b.x - a.x);
    r.y = a.y + t * (b.y - a.y);
    r.z = a.z + t * (b.z - a.z);
    return r;
}

extern "C" {
    void AccelerateQuasiCrystalBatch(
        const Vector3* vertices,        // gmem0: 4 vertices per face, packed
        const int*     connectorTypes,   // gmem1: one enum per face
        float          beamThickness,    // beam radius for all segments
        int            faceCount,        // number of faces in this batch
        float*         outputToolpaths   // gmem2: variable-length segment list
    )
    {
    #pragma HLS INTERFACE m_axi port=vertices      bundle=gmem0 offset=slave
    #pragma HLS INTERFACE m_axi port=connectorTypes bundle=gmem1 offset=slave
    #pragma HLS INTERFACE m_axi port=outputToolpaths bundle=gmem2 offset=slave
    #pragma HLS INTERFACE s_axilite port=beamThickness bundle=control
    #pragma HLS INTERFACE s_axilite port=faceCount     bundle=control
    #pragma HLS INTERFACE s_axilite port=return        bundle=control

        const float radius = beamThickness;

        int totalSegments = 0;

        // First pass: count segments so we can write the header atomically.
        // In hardware this is unrolled across faces; we serialize here for simplicity.
        COUNT_LOOP: for (int i = 0; i < faceCount; ++i)
        {
        #pragma HLS PIPELINE II=1
            int vIdx = i * 4;
            Vector3 pt1 = vertices[vIdx + 0];
            Vector3 pt2 = vertices[vIdx + 1];
            Vector3 pt3 = vertices[vIdx + 2];
            Vector3 pt4 = vertices[vIdx + 3];

            Vector3 vecLong  = { pt3.x - pt1.x, pt3.y - pt1.y, pt3.z - pt1.z };
            Vector3 vecShort = { pt4.x - pt2.x, pt4.y - pt2.y, pt4.z - pt2.z };

            int type = connectorTypes[i];
            if (type == 2) { // LINE
                totalSegments += 1;
            } else if (type == 0) { // ARROW
                totalSegments += 3;
            } else { // TRIANGLE (1)
                totalSegments += 3;
            }
        }

        // Write segment count header
        outputToolpaths[0] = (float)totalSegments;
        int outIdx = 1;

        // Second pass: emit segments
        EMIT_LOOP: for (int i = 0; i < faceCount; ++i)
        {
        #pragma HLS PIPELINE II=1
            int vIdx = i * 4;
            Vector3 pt1 = vertices[vIdx + 0];
            Vector3 pt2 = vertices[vIdx + 1];
            Vector3 pt3 = vertices[vIdx + 2];
            Vector3 pt4 = vertices[vIdx + 3];

            Vector3 vecLong  = { pt3.x - pt1.x, pt3.y - pt1.y, pt3.z - pt1.z };
            Vector3 vecShort = { pt4.x - pt2.x, pt4.y - pt2.y, pt4.z - pt2.z };

            int type = connectorTypes[i];

            if (type == 2) { // LINE
                Vector3 s = vecLerp(pt1, pt3, 0.2f);
                Vector3 e = vecLerp(pt1, pt3, 0.8f);
                outputToolpaths[outIdx++] = s.x;
                outputToolpaths[outIdx++] = s.y;
                outputToolpaths[outIdx++] = s.z;
                outputToolpaths[outIdx++] = e.x;
                outputToolpaths[outIdx++] = e.y;
                outputToolpaths[outIdx++] = e.z;
                outputToolpaths[outIdx++] = radius;
            }
            else if (type == 0) { // ARROW
                Vector3 s = vecLerp(pt2, pt4, 0.2f);
                Vector3 e = vecLerp(pt2, pt4, 0.8f);
                Vector3 m = vecLerp(pt2, pt4, 0.4f);
                Vector3 a1 = { m.x + 0.2f * vecLong.x, m.y + 0.2f * vecLong.y, m.z + 0.2f * vecLong.z };
                Vector3 a2 = { m.x - 0.2f * vecLong.x, m.y - 0.2f * vecLong.y, m.z - 0.2f * vecLong.z };

                // stem
                outputToolpaths[outIdx++] = s.x; outputToolpaths[outIdx++] = s.y; outputToolpaths[outIdx++] = s.z;
                outputToolpaths[outIdx++] = e.x; outputToolpaths[outIdx++] = e.y; outputToolpaths[outIdx++] = e.z;
                outputToolpaths[outIdx++] = radius;
                // arrow head 1
                outputToolpaths[outIdx++] = s.x; outputToolpaths[outIdx++] = s.y; outputToolpaths[outIdx++] = s.z;
                outputToolpaths[outIdx++] = a1.x; outputToolpaths[outIdx++] = a1.y; outputToolpaths[outIdx++] = a1.z;
                outputToolpaths[outIdx++] = radius;
                // arrow head 2
                outputToolpaths[outIdx++] = s.x; outputToolpaths[outIdx++] = s.y; outputToolpaths[outIdx++] = s.z;
                outputToolpaths[outIdx++] = a2.x; outputToolpaths[outIdx++] = a2.y; outputToolpaths[outIdx++] = a2.z;
                outputToolpaths[outIdx++] = radius;
            }
            else { // TRIANGLE (1)
                Vector3 s = vecLerp(pt2, pt4, 0.2f);
                Vector3 e = vecLerp(pt2, pt4, 0.8f);
                Vector3 m = vecLerp(pt2, pt4, 0.5f);
                Vector3 tri = { m.x + 0.25f * vecLong.x, m.y + 0.25f * vecLong.y, m.z + 0.25f * vecLong.z };

                // base
                outputToolpaths[outIdx++] = s.x; outputToolpaths[outIdx++] = s.y; outputToolpaths[outIdx++] = s.z;
                outputToolpaths[outIdx++] = e.x; outputToolpaths[outIdx++] = e.y; outputToolpaths[outIdx++] = e.z;
                outputToolpaths[outIdx++] = radius;
                // side 1
                outputToolpaths[outIdx++] = s.x; outputToolpaths[outIdx++] = s.y; outputToolpaths[outIdx++] = s.z;
                outputToolpaths[outIdx++] = tri.x; outputToolpaths[outIdx++] = tri.y; outputToolpaths[outIdx++] = tri.z;
                outputToolpaths[outIdx++] = radius;
                // side 2
                outputToolpaths[outIdx++] = e.x; outputToolpaths[outIdx++] = e.y; outputToolpaths[outIdx++] = e.z;
                outputToolpaths[outIdx++] = tri.x; outputToolpaths[outIdx++] = tri.y; outputToolpaths[outIdx++] = tri.z;
                outputToolpaths[outIdx++] = radius;
            }
        }
    }
}

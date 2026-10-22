// SPDX-License-Identifier: Apache-2.0
//
// Voxelization kernel for quasicrystal beam segments.
// Converts an array of beam segments into a 3D voxel occupancy grid.
//
// Output is a bit-packed grid: each uint32_t holds 32 voxels along X.
// Layout: Z-major, then Y, then X-packed-into-words.
//
// Compile:
//   v++ -c -t hw_emu -f <platform> -k VoxelizeQuasiCrystal voxel_accel.cpp -o voxel_accel.xo

#include <ap_int.h>
#include <hls_math.h>

struct BeamSegment {
    float x0, y0, z0;
    float x1, y1, z1;
    float radius;
};

static inline float fSq(float x) { return x * x; }

extern "C" {
    void VoxelizeQuasiCrystal(
        const BeamSegment* beams,
        int                numBeams,
        float              originX,
        float              originY,
        float              originZ,
        float              voxelSize,
        int                gridX,
        int                gridY,
        int                gridZ,
        ap_uint<32>*       voxelGrid   // bit-packed: 1 = occupied
    )
    {
    #pragma HLS INTERFACE m_axi port=beams      bundle=gmem0 offset=slave
    #pragma HLS INTERFACE m_axi port=voxelGrid  bundle=gmem1 offset=slave
    #pragma HLS INTERFACE s_axilite port=originX    bundle=control
    #pragma HLS INTERFACE s_axilite port=originY    bundle=control
    #pragma HLS INTERFACE s_axilite port=originZ    bundle=control
    #pragma HLS INTERFACE s_axilite port=voxelSize  bundle=control
    #pragma HLS INTERFACE s_axilite port=gridX      bundle=control
    #pragma HLS INTERFACE s_axilite port=gridY      bundle=control
    #pragma HLS INTERFACE s_axilite port=gridZ      bundle=control
    #pragma HLS INTERFACE s_axilite port=numBeams   bundle=control
    #pragma HLS INTERFACE s_axilite port=return     bundle=control

        // Clear grid
        CLEAR_GRID: for (int i = 0; i < gridX * gridY * gridZ / 32; ++i)
        {
        #pragma HLS PIPELINE II=1
            voxelGrid[i] = 0;
        }

        // For each beam, mark voxels inside the cylinder
        BEAM_LOOP: for (int b = 0; b < numBeams; ++b)
        {
        #pragma HLS PIPELINE II=1
            BeamSegment beam = beams[b];

            // Determine voxel bounding box of this beam
            int x0 = (int)((beam.x0 - beam.radius - originX) / voxelSize);
            int y0 = (int)((beam.y0 - beam.radius - originY) / voxelSize);
            int z0 = (int)((beam.z0 - beam.radius - originZ) / voxelSize);
            int x1 = (int)((beam.x1 + beam.radius - originX) / voxelSize) + 1;
            int y1 = (int)((beam.y1 + beam.radius - originY) / voxelSize) + 1;
            int z1 = (int)((beam.z1 + beam.radius - originZ) / voxelSize) + 1;

            if (x0 < 0) x0 = 0; if (x1 >= gridX) x1 = gridX - 1;
            if (y0 < 0) y0 = 0; if (y1 >= gridY) y1 = gridY - 1;
            if (z0 < 0) z0 = 0; if (z1 >= gridZ) z1 = gridZ - 1;

            VOXEL_Z: for (int iz = z0; iz <= z1; ++iz) {
            VOXEL_Y: for (int iy = y0; iy <= y1; ++iy) {
            VOXEL_X: for (int ix = x0; ix <= x1; ++ix) {
            #pragma HLS PIPELINE II=1
                // World center of voxel
                float wx = originX + (ix + 0.5f) * voxelSize;
                float wy = originY + (iy + 0.5f) * voxelSize;
                float wz = originZ + (iz + 0.5f) * voxelSize;

                // Distance from point to line segment (beam axis)
                float vx = beam.x1 - beam.x0;
                float vy = beam.y1 - beam.y0;
                float vz = beam.z1 - beam.z0;
                float lenSq = fSq(vx) + fSq(vy) + fSq(vz);
                float t = 0.0f;
                if (lenSq > 1e-6f) {
                    float wx0 = wx - beam.x0;
                    float wy0 = wy - beam.y0;
                    float wz0 = wz - beam.z0;
                    t = (wx0 * vx + wy0 * vy + wz0 * vz) / lenSq;
                    if (t < 0.0f) t = 0.0f;
                    else if (t > 1.0f) t = 1.0f;
                }
                float px = beam.x0 + t * vx;
                float py = beam.y0 + t * vy;
                float pz = beam.z0 + t * vz;
                float distSq = fSq(wx - px) + fSq(wy - py) + fSq(wz - pz);

                if (distSq <= fSq(beam.radius)) {
                    int wordIdx = iz * gridY * (gridX / 32) + iy * (gridX / 32) + (ix / 32);
                    int bitIdx  = ix % 32;
                    voxelGrid[wordIdx](bitIdx, bitIdx) = 1;
                }
            }}}
        }
    }
}

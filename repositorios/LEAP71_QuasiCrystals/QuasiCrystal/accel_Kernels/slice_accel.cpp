// SPDX-License-Identifier: Apache-2.0
//
// Z-slice extraction kernel for quasicrystal voxel volumes.
// Reads a bit-packed voxel grid and outputs a binary slice at the requested Z.
//
// Output format: one uint32_t per Y row, packed with gridX bits.
//
// Compile:
//   v++ -c -t hw_emu -f <platform> -k SliceVoxelVolumeZ slice_accel.cpp -o slice_accel.xo

#include <ap_int.h>

extern "C" {
    void SliceVoxelVolumeZ(
        const ap_uint<32>* voxelGrid,  // bit-packed input grid
        int                gridX,
        int                gridY,
        int                gridZ,
        float              originZ,
        float              voxelSize,
        int                sliceZ,      // integer Z index to extract
        ap_uint<32>*       sliceBitmap  // output: one word per Y row
    )
    {
    #pragma HLS INTERFACE m_axi port=voxelGrid   bundle=gmem0 offset=slave
    #pragma HLS INTERFACE m_axi port=sliceBitmap bundle=gmem1 offset=slave
    #pragma HLS INTERFACE s_axilite port=gridX     bundle=control
    #pragma HLS INTERFACE s_axilite port=gridY     bundle=control
    #pragma HLS INTERFACE s_axilite port=gridZ     bundle=control
    #pragma HLS INTERFACE s_axilite port=originZ   bundle=control
    #pragma HLS INTERFACE s_axilite port=voxelSize bundle=control
    #pragma HLS INTERFACE s_axilite port=sliceZ    bundle=control
    #pragma HLS INTERFACE s_axilite port=return    bundle=control

        if (sliceZ < 0 || sliceZ >= gridZ) {
            CLEAR_OUTPUT: for (int y = 0; y < gridY; ++y)
            {
            #pragma HLS PIPELINE II=1
                sliceBitmap[y] = 0;
            }
            return;
        }

        int xWords = gridX / 32;
        int zWordOffset = sliceZ * gridY * xWords;

        EXTRACT_ROW: for (int y = 0; y < gridY; ++y)
        {
        #pragma HLS PIPELINE II=1
            ap_uint<32> rowWord = 0;
            int base = zWordOffset + y * xWords;
            READ_WORD: for (int xw = 0; xw < xWords; ++xw)
            {
                ap_uint<32> gridWord = voxelGrid[base + xw];
                rowWord = (rowWord << 32) | gridWord;
            }
            sliceBitmap[y] = rowWord;
        }
    }
}

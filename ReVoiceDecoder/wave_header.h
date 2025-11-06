/*
 Copyright (C) 2011 Aaron Blohowiak
 
 Permission is hereby granted, free of charge, to any person obtaining a copy of
 this software and associated documentation files (the "Software"), to deal in
 the Software without restriction, including without limitation the rights to
 use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 of the Software, and to permit persons to whom the Software is furnished to do
 so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 */
#pragma once
#include <stdint.h>
#include <stdio.h> 

typedef struct waveFormatHeader {
    char ChunkId[4];
    uint32_t ChunkSize;
    char Format[4];
    char Subchunk1ID[4];
    uint32_t Subchunk1Size;
    uint16_t AudioFormat;
    uint16_t NumChannels;
    uint32_t SampleRate;
    uint32_t ByteRate;
    uint16_t BlockAlign;
    uint16_t BitsPerSample;
    char SubChunk2ID[4]; 
    uint32_t Subchunk2Size; 
} waveFormatHeader_t;

//Usually, just use these two functions to create your header and write it out.
//malloc's and initializes a new header struct
waveFormatHeader_t * mono16bit48khzWaveHeaderForLength(uint32_t numberOfFrames);
//writes the header to the given file. currently just an fwrite but could be a member-by-member write in the future.
uint32_t writeWaveHeaderToFile(waveFormatHeader_t * wh, FILE * file);

//if you want to create the header but set its length at a later date, you can use this. modifies the contents of wh
void setLengthForWaveFormatHeader(waveFormatHeader_t * wh, uint32_t numberOfFrames);

//Use these functions if you have read the wave header documentation and want to customize the values
waveFormatHeader_t * mono16bit48khzWaveHeader(void);
waveFormatHeader_t * basicHeader(void);

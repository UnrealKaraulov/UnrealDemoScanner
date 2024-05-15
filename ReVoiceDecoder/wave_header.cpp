#include "wave_header.h"

#include <stdlib.h>
#include <string.h>

waveFormatHeader_t* mono16bit8khzWaveHeaderForLength(uint32_t numberOfFrames) {
	waveFormatHeader_t* wh = mono16bit8khzWaveHeader();
	setLengthForWaveFormatHeader(wh, numberOfFrames);
	return wh;
}

//TODO: write each member just in case your compiler pads any of the struct members, though it shouldn't (do to its layout.)
uint32_t writeWaveHeaderToFile(waveFormatHeader_t* wh, FILE* file) {
	return fwrite(wh, sizeof(waveFormatHeader_t), 1, file);
}


waveFormatHeader_t* basicHeader() {
	waveFormatHeader_t* wh = (waveFormatHeader_t*)malloc(sizeof(waveFormatHeader_t));
	if (wh)
	{
		memcpy(wh->ChunkId, &"RIFF", 4);
		memcpy(wh->Format, &"WAVE", 4);
		memcpy(wh->Subchunk1ID, &"fmt ", 4); //notice the space at the end!
		wh->Subchunk1Size = 16;
	}
	return wh;
}

waveFormatHeader_t* mono16bit8khzWaveHeader() {
	waveFormatHeader_t* wh = basicHeader();

	wh->AudioFormat = 1;
	wh->NumChannels = 1;

	wh->SampleRate = 48000;
	wh->BitsPerSample = 16;

	wh->ByteRate = wh->NumChannels * wh->SampleRate * wh->BitsPerSample / 8;
	wh->BlockAlign = wh->NumChannels * wh->BitsPerSample / 8;

	memcpy(wh->SubChunk2ID, &"data", 4);
	return wh;
}

void setLengthForWaveFormatHeader(waveFormatHeader_t* wh, uint32_t numberOfFrames) {
	wh->Subchunk2Size = numberOfFrames * wh->NumChannels * wh->BitsPerSample / 8;
	wh->ChunkSize = 36 + wh->Subchunk2Size;
}
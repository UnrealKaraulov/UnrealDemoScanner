#pragma once

#include "IVoiceCodec.h"
#include "utlbuffer.h"
#include "opus.h"

const int MAX_CHANNELS = 1;
const int FRAME_SIZE = 160;
const int MAX_FRAME_SIZE = 3 * FRAME_SIZE;
const int MAX_PACKET_LOSS = 10;

class VoiceEncoder_Opus: public IVoiceCodec {
private:
	OpusEncoder *m_pEncoder;
	OpusDecoder *m_pDecoder;
	CUtlBuffer m_bufOverflowBytes;

	int m_samplerate;
	int m_bitrate;

	uint16_t m_nEncodeSeq;
	uint16_t m_nDecodeSeq;

	bool m_PacketLossConcealment;

public:
	VoiceEncoder_Opus();

	virtual ~VoiceEncoder_Opus();

	virtual bool Init(int quality);
	virtual void Release();
	virtual bool ResetState();
	virtual int Compress(const char *pUncompressedBytes, int nSamplesIn, char *pCompressed, int maxCompressedBytes, bool bFinal);
	virtual int Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes);

	int GetNumQueuedEncodingSamples() const { return m_bufOverflowBytes.TellPut() / BYTES_PER_SAMPLE; }
};

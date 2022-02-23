#pragma once

#include "IVoiceCodec.h"
#include "iframeencoder.h"
#include "speex.h"

class VoiceEncoder_Speex: public IFrameEncoder {
protected:
	virtual ~VoiceEncoder_Speex();

public:
	VoiceEncoder_Speex();

	virtual bool Init(int quality, int &rawFrameSize, int &encodedFrameSize);
	virtual void Release();
	virtual void EncodeFrame(const char *pUncompressedBytes, char *pCompressed);
	virtual int DecodeFrame(const char *pCompressed, char *pDecompressedBytes);
	virtual bool ResetState();

protected:
	bool InitStates();
	void TermStates();

private:
	int m_Quality;
	void *m_EncoderState;
	void *m_DecoderState;
	SpeexBits m_Bits;
};

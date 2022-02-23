#pragma once

class IFrameEncoder {
protected:
	virtual ~IFrameEncoder() {};

public:
	// quality is in [0..10]
	virtual bool Init(int quality, int &rawFrameSize, int &encodedFrameSize) = 0;
	virtual void Release() = 0;
	virtual void EncodeFrame(const char *pUncompressedBytes, char *pCompressed) = 0;
	virtual int DecodeFrame(const char *pCompressed, char *pDecompressedBytes) = 0;
	virtual bool ResetState() = 0;
};

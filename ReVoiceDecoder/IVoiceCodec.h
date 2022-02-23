#pragma once


class IVoiceCodec {
protected:
	virtual	~IVoiceCodec() {}
	static const int BYTES_PER_SAMPLE = 2;

public:
	// Initialize the object. The uncompressed format is always 8-bit signed mono.
	virtual bool Init(int quality) = 0;
	virtual void Release() = 0;

	// Compress the voice data.
	// pUncompressed		-	16-bit signed mono voice data.
	// maxCompressedBytes	-	The length of the pCompressed buffer. Don't exceed this.
	// bFinal        		-	Set to true on the last call to Compress (the user stopped talking).
	//							Some codecs like big block sizes and will hang onto data you give them in Compress calls.
	//							When you call with bFinal, the codec will give you compressed data no matter what.
	// Return the number of bytes you filled into pCompressed.
	virtual int Compress(const char *pUncompressedBytes, int nSamples, char *pCompressed, int maxCompressedBytes, bool bFinal) = 0;

	// Decompress voice data. pUncompressed is 16-bit signed mono.
	virtual int Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes) = 0;

	// Some codecs maintain state between Compress and Decompress calls. This should clear that state.
	virtual bool ResetState() = 0;
};

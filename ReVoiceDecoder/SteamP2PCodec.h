#pragma once

#include "IVoiceCodec.h"

class CSteamP2PCodec: public IVoiceCodec {
public:
	CSteamP2PCodec(IVoiceCodec *backend);

	enum PayLoadType : uint8_t
	{
		PLT_Silence       = 0, // Number of empty samples, which should be set to NULL.
		PLT_UnknownCodec  = 1,
		PLT_Speex         = 2,
		PLT_Raw           = 3,
		PLT_Silk          = 4,
		PLT_OPUS          = 5,
		PLT_OPUS_PLC      = 6,
		PLT_Unknown       = 10,
		PLT_SamplingRate  = 11
	};

	virtual bool Init(int quality);
	virtual void Release();
	virtual int Compress(const char *pUncompressedBytes, int nSamples, char *pCompressed, int maxCompressedBytes, bool bFinal);
	virtual int Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes);
	virtual bool ResetState();

	void SetClient(IGameClient *client);
	IVoiceCodec *GetCodec() const { return m_BackendCodec; }

private:
	int StreamDecode(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes) const;
	int StreamEncode(const char *pUncompressedBytes, int nSamples, char *pCompressed, int maxCompressedBytes, bool bFinal) const;

private:
	IGameClient *m_Client;
	IVoiceCodec *m_BackendCodec;
};

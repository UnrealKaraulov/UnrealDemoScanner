#include "header.h"

VoiceCodec_Frame::VoiceCodec_Frame(IFrameEncoder *pEncoder)
{
	m_nEncodeBufferSamples = 0;
	m_nRawBytes = m_nRawSamples = m_nEncodedBytes = 0;
	m_pFrameEncoder = pEncoder;
	m_Client = nullptr;
}

VoiceCodec_Frame::~VoiceCodec_Frame()
{
	if (m_pFrameEncoder) {
		m_pFrameEncoder->Release();
		m_pFrameEncoder = nullptr;
	}
}

bool VoiceCodec_Frame::Init(int quality)
{
	if (!m_pFrameEncoder)
		return false;

	if (m_pFrameEncoder->Init(quality, m_nRawBytes, m_nEncodedBytes)) {
		m_nRawSamples = m_nRawBytes >> 1;
		return true;
	}

	m_pFrameEncoder->Release();
	m_pFrameEncoder = nullptr;
	return false;
}

void VoiceCodec_Frame::Release()
{
	delete this;
}

int VoiceCodec_Frame::Compress(const char *pUncompressedBytes, int nSamples, char *pCompressed, int maxCompressedBytes, bool bFinal)
{
	if (m_pFrameEncoder == nullptr)
		return 0;
	
	const int16_t *pUncompressed = (const int16_t *) pUncompressedBytes;

	int nCompressedBytes = 0;
	while ((nSamples + m_nEncodeBufferSamples) >= m_nRawSamples && (maxCompressedBytes - nCompressedBytes) >= m_nEncodedBytes)
	{
		// Get the data block out.
		int16_t samples[MAX_FRAMEBUFFER_SAMPLES];
		memcpy(samples, m_EncodeBuffer, m_nEncodeBufferSamples * BYTES_PER_SAMPLE);
		memcpy(&samples[m_nEncodeBufferSamples], pUncompressed, (m_nRawSamples - m_nEncodeBufferSamples) * BYTES_PER_SAMPLE);
		nSamples -= m_nRawSamples - m_nEncodeBufferSamples;
		pUncompressed += m_nRawSamples - m_nEncodeBufferSamples;
		m_nEncodeBufferSamples = 0;

		// Compress it.
		m_pFrameEncoder->EncodeFrame((const char *)samples, &pCompressed[nCompressedBytes]);
		nCompressedBytes += m_nEncodedBytes;
	}

	// Store the remaining samples.
	int nNewSamples = std::min(nSamples, std::min(m_nRawSamples - m_nEncodeBufferSamples, m_nRawSamples));
	if (nNewSamples) {
		memcpy(&m_EncodeBuffer[m_nEncodeBufferSamples], &pUncompressed[nSamples - nNewSamples], nNewSamples * BYTES_PER_SAMPLE);
		m_nEncodeBufferSamples += nNewSamples;
	}

	// If it must get the last data, just pad with zeros..
	if (bFinal && m_nEncodeBufferSamples && (maxCompressedBytes - nCompressedBytes) >= m_nEncodedBytes) {
		memset(&m_EncodeBuffer[m_nEncodeBufferSamples], 0, (m_nRawSamples - m_nEncodeBufferSamples) * BYTES_PER_SAMPLE);
		m_pFrameEncoder->EncodeFrame((const char *)m_EncodeBuffer, &pCompressed[nCompressedBytes]);
		nCompressedBytes += m_nEncodedBytes;
		m_nEncodeBufferSamples = 0;
	}

	return nCompressedBytes;
}

int VoiceCodec_Frame::Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes)
{
	if (m_pFrameEncoder == nullptr || compressedBytes < m_nEncodedBytes || maxUncompressedBytes < m_nRawBytes)
		return 0;

	int nDecompressedBytes = 0;
	int curCompressedByte = 0;

	while (true)
	{
		if (m_pFrameEncoder->DecodeFrame(&pCompressed[curCompressedByte], &pUncompressed[nDecompressedBytes]) != 0)
			return 0;

		curCompressedByte += m_nEncodedBytes;
		nDecompressedBytes += m_nRawBytes;

		if (compressedBytes - curCompressedByte < m_nEncodedBytes || maxUncompressedBytes - nDecompressedBytes < m_nRawBytes)
			break;
	}

	return nDecompressedBytes / BYTES_PER_SAMPLE;
}

bool VoiceCodec_Frame::ResetState()
{
	if (m_pFrameEncoder) {
		return m_pFrameEncoder->ResetState();
	}

	return false;
}

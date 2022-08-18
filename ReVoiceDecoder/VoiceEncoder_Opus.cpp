#include "header.h"

VoiceEncoder_Opus::VoiceEncoder_Opus() : m_bitrate(44000), m_samplerate(8000)
{
	m_nEncodeSeq = 0;
	m_nDecodeSeq = 0;
	m_pEncoder = nullptr;
	m_pDecoder = nullptr;
}

VoiceEncoder_Opus::~VoiceEncoder_Opus()
{
	if (m_pEncoder) {
		free(m_pEncoder);
		m_pEncoder = nullptr;
	}

	if (m_pDecoder) {
		free(m_pDecoder);
		m_pDecoder = nullptr;
	}
}

bool VoiceEncoder_Opus::Init(int quality)
{
	m_nEncodeSeq = 0;
	m_nDecodeSeq = 0;
	m_PacketLossConcealment = true;

	int encSizeBytes = opus_encoder_get_size(MAX_CHANNELS);
	m_pEncoder = (OpusEncoder *)malloc(encSizeBytes);
	if (opus_encoder_init((OpusEncoder *)m_pEncoder, m_samplerate, MAX_CHANNELS, OPUS_APPLICATION_VOIP) != OPUS_OK) {
		free(m_pEncoder);
		m_pEncoder = nullptr;
		return false;
	}

	opus_encoder_ctl((OpusEncoder *)m_pEncoder, OPUS_SET_BITRATE_REQUEST, m_bitrate);
	opus_encoder_ctl((OpusEncoder *)m_pEncoder, OPUS_SET_SIGNAL_REQUEST, OPUS_SIGNAL_VOICE);
	opus_encoder_ctl((OpusEncoder *)m_pEncoder, OPUS_SET_DTX_REQUEST, 1);

	int decSizeBytes = opus_decoder_get_size(MAX_CHANNELS);
	m_pDecoder = (OpusDecoder *)malloc(decSizeBytes);
	if (opus_decoder_init((OpusDecoder *)m_pDecoder, m_samplerate, MAX_CHANNELS) != OPUS_OK) {
		free(m_pDecoder);
		m_pDecoder = nullptr;
		return false;
	}

	return true;
}

void VoiceEncoder_Opus::Release()
{
	delete this;
}

bool VoiceEncoder_Opus::ResetState()
{
	if (m_pEncoder) {
		opus_encoder_ctl(m_pEncoder, OPUS_RESET_STATE);
	}

	if (m_pDecoder) {
		opus_decoder_ctl(m_pDecoder, OPUS_RESET_STATE);
	}

	m_bufOverflowBytes.Clear();
	return true;
}

int VoiceEncoder_Opus::Compress(const char *pUncompressedIn, int nSamplesIn, char *pCompressed, int maxCompressedBytes, bool bFinal)
{
	if ((nSamplesIn + GetNumQueuedEncodingSamples()) < FRAME_SIZE && !bFinal)
	{
		m_bufOverflowBytes.Put(pUncompressedIn, nSamplesIn * BYTES_PER_SAMPLE);
		return 0;
	}

	int nSamples = nSamplesIn;
	int nSamplesRemaining = nSamplesIn % FRAME_SIZE;
	char *pUncompressed = (char *)pUncompressedIn;

	CUtlBuffer buf;
	if (m_bufOverflowBytes.TellPut() || (nSamplesRemaining && bFinal))
	{
		buf.Put(m_bufOverflowBytes.Base(), m_bufOverflowBytes.TellPut());
		buf.Put(pUncompressedIn, nSamplesIn * BYTES_PER_SAMPLE);
		m_bufOverflowBytes.Clear();

		nSamples = (buf.TellPut() / BYTES_PER_SAMPLE);
		nSamplesRemaining = (buf.TellPut() / BYTES_PER_SAMPLE) % FRAME_SIZE;

		if (bFinal && nSamplesRemaining)
		{
			// fill samples with silence
			for (int i = FRAME_SIZE - nSamplesRemaining; i > 0; i--)
			{
				buf.PutShort(0);
			}

			nSamples = (buf.TellPut() / BYTES_PER_SAMPLE);
			nSamplesRemaining = (buf.TellPut() / BYTES_PER_SAMPLE) % FRAME_SIZE;
		}

		pUncompressed = (char *)buf.Base();
		Assert(!bFinal || nSamplesRemaining == 0);
	}

	char *psRead = pUncompressed;
	char *pWritePos = pCompressed;
	char *pWritePosMax = pCompressed + maxCompressedBytes;

	int nChunks = nSamples - nSamplesRemaining;
	if (nChunks > 0)
	{
		int nRemainingSamples = (nChunks - 1) / FRAME_SIZE + 1;
		do
		{
			uint16_t *pWritePayloadSize = (uint16_t *)pWritePos;
			pWritePos += sizeof(uint16_t); // leave 2 bytes for the frame size (will be written after encoding)

			if (m_PacketLossConcealment)
			{
				*(uint16_t *)pWritePos = m_nEncodeSeq++;
				pWritePos += sizeof(uint16_t);
			}

			int nBytes = ((pWritePosMax - pWritePos) < 0x7FFF) ? (pWritePosMax - pWritePos) : 0x7FFF;
			int nWriteBytes = opus_encode(m_pEncoder, (const opus_int16 *)psRead, FRAME_SIZE, (unsigned char *)pWritePos, nBytes);

			psRead += MAX_FRAME_SIZE;
			pWritePos += nWriteBytes;

			nRemainingSamples--;
			*pWritePayloadSize = nWriteBytes;
		}
		while (nRemainingSamples > 0);
	}

	m_bufOverflowBytes.Clear();

	if (nSamplesRemaining)
	{
		Assert((char *)psRead == pUncompressed + ((nSamples - nSamplesRemaining) * sizeof(int16_t)));
		m_bufOverflowBytes.Put(pUncompressed + ((nSamples - nSamplesRemaining) * sizeof(int16_t)), nSamplesRemaining * BYTES_PER_SAMPLE);
	}

	if (bFinal)
	{
		ResetState();

		if (pWritePosMax > pWritePos + 2)
		{
			*(uint16_t *)pWritePos = 0xFFFF;
			pWritePos += sizeof(uint16_t);
		}

		m_nEncodeSeq = 0;
	}

	return pWritePos - pCompressed;
}

int VoiceEncoder_Opus::Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes)
{
	const char *pReadPos = pCompressed;
	const char *pReadPosMax = &pCompressed[compressedBytes];

	char *pWritePos = pUncompressed;
	char *pWritePosMax = &pUncompressed[maxUncompressedBytes];

	while (pReadPos < pReadPosMax)
	{
		uint16_t nPayloadSize = *(uint16_t *)pReadPos;
		pReadPos += sizeof(uint16_t);

		if (nPayloadSize == 0xFFFF)
		{
			ResetState();
			m_nDecodeSeq = 0;
			break;
		}

		if (m_PacketLossConcealment)
		{
			uint16_t nCurSeq = *(uint16_t *)pReadPos;
			pReadPos += sizeof(uint16_t);

			if (nCurSeq < m_nDecodeSeq)
			{
				ResetState();
			}
			else if (nCurSeq != m_nDecodeSeq)
			{
				int nPacketLoss = nCurSeq - m_nDecodeSeq;
				if (nPacketLoss > MAX_PACKET_LOSS) {
					nPacketLoss = MAX_PACKET_LOSS;
				}

				for (int i = 0; i < nPacketLoss; i++)
				{
					if ((pWritePos + MAX_FRAME_SIZE) >= pWritePosMax)
					{
						return 0;
						break;
					}

					int nBytes = opus_decode(m_pDecoder, 0, 0, (opus_int16 *)pWritePos, FRAME_SIZE, 0);
					if (nBytes <= 0)
					{
						return 0;
					}

					pWritePos += nBytes * BYTES_PER_SAMPLE;
				}
			}

			m_nDecodeSeq = nCurSeq + 1;
		}

		if ((pReadPos + nPayloadSize) > pReadPosMax)
		{
			return 0;
			break;
		}

		if ((pWritePos + MAX_FRAME_SIZE) > pWritePosMax)
		{
			return 0;
			break;
		}

		memset(pWritePos, 0, MAX_FRAME_SIZE);

		if (nPayloadSize == 0)
		{
			// DTX (discontinued transmission)
			pWritePos += MAX_FRAME_SIZE;
			continue;
		}

		int nBytes = opus_decode(m_pDecoder, (const unsigned char *)pReadPos, nPayloadSize, (opus_int16 *)pWritePos, FRAME_SIZE, 0);
		if (nBytes <= 0)
		{
			return 0;
		}
		else
		{
			pWritePos += nBytes * BYTES_PER_SAMPLE;
		}

		pReadPos += nPayloadSize;
	}

	return (pWritePos - pUncompressed) / BYTES_PER_SAMPLE;
}

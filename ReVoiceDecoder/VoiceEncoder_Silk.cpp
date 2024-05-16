#include "header.h"

VoiceEncoder_Silk::VoiceEncoder_Silk()
{
	m_pEncoder = nullptr;
	m_pDecoder = nullptr;
	m_targetRate_bps = 44000;
	m_packetLoss_perc = 0;
	m_encControl = {};
	m_decControl = {};
}

VoiceEncoder_Silk::~VoiceEncoder_Silk()
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

bool VoiceEncoder_Silk::Init(int quality)
{
	m_targetRate_bps = 44000;
	m_packetLoss_perc = 0;

	int encSizeBytes;
	SKP_Silk_SDK_Get_Encoder_Size(&encSizeBytes);
	m_pEncoder = malloc(encSizeBytes);
	SKP_Silk_SDK_InitEncoder(m_pEncoder, &this->m_encControl);

	int decSizeBytes;
	SKP_Silk_SDK_Get_Decoder_Size(&decSizeBytes);
	m_pDecoder = malloc(decSizeBytes);
	SKP_Silk_SDK_InitDecoder(m_pDecoder);

	return true;
}

void VoiceEncoder_Silk::Release()
{
	delete this;
}

bool VoiceEncoder_Silk::ResetState()
{
	if (m_pEncoder)
		SKP_Silk_SDK_InitEncoder(m_pEncoder, &this->m_encControl);

	if (m_pDecoder)
		SKP_Silk_SDK_InitDecoder(m_pDecoder);

	m_bufOverflowBytes.Clear();

	return true;
}

int VoiceEncoder_Silk::Compress(const char *pUncompressedIn, int nSamplesIn, char *pCompressed, int maxCompressedBytes, bool bFinal)
{
	signed int nSamplesToUse; // edi@4
	const __int16 *psRead; // ecx@4
	int nSamples; // edi@5
	int nSamplesToEncode; // esi@6
	char *pWritePos; // ebp@6
	int nSamplesPerFrame; // [sp+28h] [bp-44h]@5
	const char *pWritePosMax; // [sp+2Ch] [bp-40h]@5
	int nSamplesRemaining; // [sp+38h] [bp-34h]@5

	const int inSampleRate = 8000;
	const int nSampleDataMinMS = 100;
	const int nSamplesMin = inSampleRate * nSampleDataMinMS / 1000;

	/*
	if ((nSamplesIn + m_bufOverflowBytes.TellPut() / 2) < nSamplesMin && !bFinal) {
		m_bufOverflowBytes.Put(pUncompressedIn, 2 * nSamplesIn);
		return 0;
	}
	*/

	if (m_bufOverflowBytes.TellPut()) {
		m_bufOverflowBytes.Put(pUncompressedIn, 2 * nSamplesIn);

		psRead = (const __int16 *)m_bufOverflowBytes.Base();
		nSamplesToUse = m_bufOverflowBytes.TellPut() / 2;
	} else {
		psRead = (const __int16 *)pUncompressedIn;
		nSamplesToUse = nSamplesIn;
	}

	nSamplesPerFrame = inSampleRate / 50;
	nSamplesRemaining = nSamplesToUse % nSamplesPerFrame;
	pWritePosMax = pCompressed + maxCompressedBytes;
	nSamples = nSamplesToUse - nSamplesRemaining;
	pWritePos = pCompressed;

	while (nSamples > 0)
	{
		int16_t *pWritePayloadSize = (int16_t *)pWritePos;
		pWritePos += sizeof(int16_t); //leave 2 bytes for the frame size (will be written after encoding)

		int originalNBytes = (pWritePosMax - pWritePos > 0xFFFF) ? -1 : (pWritePosMax - pWritePos);
		nSamplesToEncode = (nSamples < nSamplesPerFrame) ? nSamples : nSamplesPerFrame;

		this->m_encControl.useDTX = 0;
		this->m_encControl.maxInternalSampleRate = 16000;
		this->m_encControl.useInBandFEC = 0;
		this->m_encControl.API_sampleRate = inSampleRate;
		this->m_encControl.complexity = 2;
		this->m_encControl.packetSize = 20 * (inSampleRate / 1000);
		this->m_encControl.packetLossPercentage = this->m_packetLoss_perc;
		this->m_encControl.bitRate = (m_targetRate_bps >= 0) ? m_targetRate_bps : 0;

		nSamples -= nSamplesToEncode;

		int16_t nBytes = originalNBytes;
		int res = SKP_Silk_SDK_Encode(this->m_pEncoder, &this->m_encControl, psRead, nSamplesToEncode, (unsigned char *)pWritePos, &nBytes);
		*pWritePayloadSize = nBytes; //write frame size

		pWritePos += nBytes;
		psRead += nSamplesToEncode;
	}

	m_bufOverflowBytes.Clear();

	if (nSamplesRemaining <= nSamplesIn && nSamplesRemaining) {
		m_bufOverflowBytes.Put(&pUncompressedIn[2 * (nSamplesIn - nSamplesRemaining)], 2 * nSamplesRemaining);
	}

	if (bFinal)
	{
		ResetState();

		if (pWritePosMax > pWritePos + 2) {
			uint16_t *pWriteEndFlag = (uint16_t*)pWritePos;
			pWritePos += sizeof(uint16_t);
			*pWriteEndFlag = 0xFFFF;
		}
	}

	return pWritePos - pCompressed;
}

int VoiceEncoder_Silk::Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes)
{
	int nPayloadSize; // ebp@2
	char *pWritePos; // ebx@5
	char *pReadPos; // edx@5
	char *pWritePosMax; // [sp+28h] [bp-44h]@4
	const char *pReadPosMax; // [sp+3Ch] [bp-30h]@1

	const int outSampleRate = 48000;

	m_decControl.API_sampleRate = outSampleRate;
	int nSamplesPerFrame = outSampleRate / 50;

	if (compressedBytes <= 0) {
		return 0;
	}

	pReadPosMax = &pCompressed[compressedBytes];
	pReadPos = (char*)pCompressed;

	pWritePos = pUncompressed;
	pWritePosMax = &pUncompressed[maxUncompressedBytes];

	while (pReadPos < pReadPosMax)
	{
		if (pReadPosMax - pReadPos < 2) {
			break;
		}

		nPayloadSize = *(uint16_t *)pReadPos;
		pReadPos += sizeof(uint16_t);

		if (nPayloadSize == 0xFFFF) {
			ResetState();
			break;
		}

		if (nPayloadSize == 0) {
			//DTX (discontinued transmission)
			int numEmptySamples = nSamplesPerFrame;
			short nSamples = (pWritePosMax - pWritePos) / 2;

			if (nSamples < numEmptySamples) {
				break;
			}

			memset(pWritePos, 0, numEmptySamples * 2);
			pWritePos += numEmptySamples * 2;

			continue;
		}

		if ((pReadPos + nPayloadSize) > pReadPosMax) {
			break;
		}

		do {
			short nSamples = (pWritePosMax - pWritePos) / 2;
			int decodeRes = SKP_Silk_SDK_Decode(m_pDecoder, &m_decControl, 0, (const unsigned char*)pReadPos, nPayloadSize, (__int16 *)pWritePos, &nSamples);

			if (SKP_SILK_NO_ERROR != decodeRes) {
				return 0;
			}

			pWritePos += nSamples * sizeof(int16_t);
		} while (m_decControl.moreInternalDecoderFrames);
		pReadPos += nPayloadSize;
	}

	return (pWritePos - pUncompressed) / 2;
}
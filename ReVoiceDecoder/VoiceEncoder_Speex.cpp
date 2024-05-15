#include "header.h"

size_t ENCODED_FRAME_SIZE[] = { 0x6u, 0x6u, 0xFu, 0xFu, 0x14u, 0x14u, 0x1Cu, 0x1Cu, 0x26u, 0x26u, 0x26u };

VoiceEncoder_Speex::VoiceEncoder_Speex()
{
	m_EncoderState = nullptr;
	m_DecoderState = nullptr;
	m_Quality = 0;
}

VoiceEncoder_Speex::~VoiceEncoder_Speex()
{
	TermStates();
}

bool VoiceEncoder_Speex::Init(int quality, int &rawFrameSize, int &encodedFrameSize)
{
	int postfilter;
	int samplerate;

	if (!InitStates())
		return false;

	rawFrameSize = 320;

	switch (quality) {
		case 2:
			m_Quality = 2;
			break;
		case 3:
			m_Quality = 4;
			break;
		case 4:
			m_Quality = 6;
			break;
		case 5:
			m_Quality = 8;
			break;
		default:
			m_Quality = 0;
			break;
	}

	encodedFrameSize = ENCODED_FRAME_SIZE[m_Quality];

	speex_encoder_ctl(m_EncoderState, SPEEX_SET_QUALITY, &m_Quality);
	speex_decoder_ctl(m_DecoderState, SPEEX_SET_QUALITY, &m_Quality);

	postfilter = 1;
	speex_decoder_ctl(m_DecoderState, SPEEX_SET_ENH, &postfilter);

	samplerate = 48000;
	speex_decoder_ctl(m_DecoderState, SPEEX_SET_SAMPLING_RATE, &samplerate);
	speex_encoder_ctl(m_EncoderState, SPEEX_SET_SAMPLING_RATE, &samplerate);

	return true;
}

void VoiceEncoder_Speex::Release()
{
	delete this;
}

void VoiceEncoder_Speex::EncodeFrame(const char *pUncompressedBytes, char *pCompressed)
{
	float input[160];
	int16_t *in = (int16_t *)pUncompressedBytes;

	for (int i = 0; i < ARRAYSIZE(input); i++, in++) {
		input[i] = *in;
	}

	speex_bits_reset(&m_Bits);
	speex_encode(m_EncoderState, input, &m_Bits);
	speex_bits_write(&m_Bits, pCompressed, ENCODED_FRAME_SIZE[m_Quality]);
}

int VoiceEncoder_Speex::DecodeFrame(const char *pCompressed, char *pDecompressedBytes)
{
	float output[160];
	int16_t *out = (int16_t *)pDecompressedBytes;

	speex_bits_read_from(&m_Bits, (char *)pCompressed, ENCODED_FRAME_SIZE[m_Quality]);
	if (speex_decode(m_DecoderState, &m_Bits, output) != 0)
		return 0;

	for (int i = 0; i < ARRAYSIZE(output); i++, out++) {
		*out = (int)output[i];
	}
	return 1;
}

bool VoiceEncoder_Speex::ResetState()
{
	speex_encoder_ctl(m_EncoderState, SPEEX_RESET_STATE, 0);
	speex_decoder_ctl(m_DecoderState, SPEEX_RESET_STATE, 0);

	return true;
}

bool VoiceEncoder_Speex::InitStates()
{
	speex_bits_init(&m_Bits);
	m_EncoderState = speex_encoder_init(&speex_nb_mode);
	m_DecoderState = speex_decoder_init(&speex_nb_mode);
	return (m_EncoderState != nullptr && m_DecoderState != nullptr);
}

void VoiceEncoder_Speex::TermStates()
{
	if (m_EncoderState) {
		speex_encoder_destroy(m_EncoderState);
		m_EncoderState = nullptr;
	}

	if (m_DecoderState) {
		speex_encoder_destroy(m_DecoderState);
		m_DecoderState = nullptr;
	}

	speex_bits_destroy(&m_Bits);
}

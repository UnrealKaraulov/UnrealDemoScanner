#include "header.h"
#include "Buffer.hpp"
#include <iostream>

#define SILK_VOICE_QUALITY			5
#define OPUS_VOICE_QUALITY			5
#define SPEEX_VOICE_QUALITY			5


IVoiceCodec* speex_codec = new VoiceCodec_Frame(new VoiceEncoder_Speex());
IVoiceCodec* silk_codec = new CSteamP2PCodec(new VoiceEncoder_Silk());
IVoiceCodec* opus_codec = new CSteamP2PCodec(new VoiceEncoder_Opus());

void decodemy(char* encoded, int size);

int main()
{

	FILE* encfileptr;
	char* encbuffer;
	int encfilelen;

	fopen_s(&encfileptr, "input.wav.enc", "rb");
	if (encfileptr)
	{
		fseek(encfileptr, 0, SEEK_END);
		encfilelen = (int)ftell(encfileptr);
		rewind(encfileptr);
		if (encfilelen > 0)
		{
			encbuffer = new char[encfilelen * sizeof(char)];
			fread(encbuffer, encfilelen, 1, encfileptr);
			decodemy(encbuffer, encfilelen);
		}
		fclose(encfileptr);
	}


	//speex_codec->Decompress()
}

void decodemy(char* encoded, int size)
{
	FILE* file;
	fopen_s(&file, "output.wav", "wb");
	if (file)
	{
		waveFormatHeader_t* wh = NULL;

		std::vector<int16_t> samplesout;

		unsigned char enc_samples[8192];
		unsigned char dec_samples[8192];

		Buffer tmpBuff;
		for (int i = 0; i < size; i++)
			tmpBuff.writeInt8(encoded[i]);
		int codecid = 0;
		//std::cout << "CODEC:";

		unsigned char quality = tmpBuff.readBytes<unsigned char>();

		speex_codec->Init(quality);
		silk_codec->Init(quality);
		opus_codec->Init(quality);

		while (true)
		{
			int len = tmpBuff.readBytes<int>();
			if (len > 0)
			{
				for (int i = 0; i < len; i++)
				{
					enc_samples[i] = tmpBuff.readBytes<unsigned char>();
				}

				int samples = opus_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], 8192);
				if (samples > 5)
				{
					for (int i = 0; i < samples; i++)
					{
						int16_t* samplesarray = (int16_t*)&dec_samples[0];
						samplesout.push_back(samplesarray[i]);
					}
					opus_codec->ResetState();
				}
				else
				{
					samples = speex_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], 8192);
					if (samples > 5)
					{
						for (int i = 0; i < samples; i++)
						{
							int16_t* samplesarray = (int16_t*)&dec_samples[0];
							samplesout.push_back(samplesarray[i]);
						}
						speex_codec->ResetState();
					}
					else
					{
						samples = silk_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], 8192);
						for (int i = 0; i < samples; i++)
						{
							int16_t* samplesarray = (int16_t*)&dec_samples[0];
							samplesout.push_back(samplesarray[i]);
						}
						silk_codec->ResetState();
					}
				}
				
			}
			else break;
		}

		wh = stereo16bit8khzWaveHeaderForLength(samplesout.size());
		writeWaveHeaderToFile(wh, file);
		free(wh);


		for (int i = 0; i < samplesout.size(); i++) {
			//write the same data to both channels if you have a mono source.
			//you could make a mono file, but this is just easier =)
			fwrite(&samplesout[i], sizeof(int16_t), 1, file);
			fwrite(&samplesout[i], sizeof(int16_t), 1, file);
		}

		fclose(file);
	}
}
#define SILK_VOICE_QUALITY			5
#define OPUS_VOICE_QUALITY			5
#define SPEEX_VOICE_QUALITY			5

#include "header.h"
#include "Buffer.hpp"
#include <iostream>
#include <filesystem>
#include <vector>
#include <fstream>
#include <algorithm>
#include <thread>
#include <mutex>
#include <execution>


namespace fs = std::filesystem;

static bool bMixFiles = false;
static std::vector<int16_t> mixed_samples = {};
static bool isFirstTime = false;
static float first_time = 0.0f;

#define SKIP_THRESHOLD 0.75f

std::vector<int16_t> decode(const std::vector<unsigned char>& input_bytes)
{
	IVoiceCodec* speex_codec = new VoiceCodec_Frame(new VoiceEncoder_Speex());
	IVoiceCodec* silk_codec = new CSteamP2PCodec(new VoiceEncoder_Silk());
	IVoiceCodec* opus_codec = new CSteamP2PCodec(new VoiceEncoder_Opus());

	std::vector<int16_t> samplesout = {};
	std::vector<int16_t> tempSamples = {};

	float targetSampleTime = 0.0f;
	unsigned int lastSampleOffset = 0;

	bool firstTime = true;

	unsigned char* enc_samples = new unsigned char[0x10000];
	unsigned char* dec_samples = new unsigned char[0x10000];

	Buffer* tmpBuff = new Buffer(input_bytes);

	unsigned char is_one_file = tmpBuff->readBytes<unsigned char>();
	unsigned char quality = tmpBuff->readBytes<unsigned char>();

	if (is_one_file > 0 && !bMixFiles)
	{
		bMixFiles = true;
	}

	float totalTime = abs(tmpBuff->readBytes<float>());

	if (bMixFiles)
	{
		samplesout.resize((int)(totalTime * 48000.0f));
	}

	speex_codec->Init(quality);
	silk_codec->Init(quality);
	opus_codec->Init(quality);

	float oldtime = 0.0f;
	float time = 0.0f;

	while (true)
	{
		if (tmpBuff->getReadOffset() + 4 >= input_bytes.size())
			break;

		oldtime = time;
		time = abs(tmpBuff->readBytes<float>());

		if (firstTime)
		{
			firstTime = false;
			oldtime = time;
			targetSampleTime = time;
		}

		if (bMixFiles)
		{
			if (time - oldtime > SKIP_THRESHOLD)
			{
				lastSampleOffset = targetSampleTime * 48000.0f;
				//std::cout << "Write to " << targetSampleTime << " time with offset " << lastSampleOffset << std::endl;
				if (samplesout.size() <= lastSampleOffset + tempSamples.size())
				{
					samplesout.resize(lastSampleOffset + tempSamples.size() + 1);
				}
				std::copy(tempSamples.begin(), tempSamples.end(), samplesout.begin() + lastSampleOffset);

				tempSamples.clear();
				targetSampleTime = time;
			}
		}

		int len = tmpBuff->readBytes<int>();
		if (len <= 0)
			break;

		tmpBuff->readData((char*)&enc_samples[0], len);

		int16_t* samplesarray = (int16_t*)&dec_samples[0];
		int samples = opus_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], 0xffff);
		if (samples > 5)
		{
			if (!bMixFiles)
			{
				samplesout.insert(samplesout.end(), &samplesarray[0], &samplesarray[samples]);
			}
			else
			{
				tempSamples.insert(tempSamples.end(), &samplesarray[0], &samplesarray[samples]);
			}
		}
		else
		{
			samples = silk_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], 0xffff);
			if (samples > 5)
			{
				if (!bMixFiles)
				{
					samplesout.insert(samplesout.end(), &samplesarray[0], &samplesarray[samples]);
				}
				else
				{
					tempSamples.insert(tempSamples.end(), &samplesarray[0], &samplesarray[samples]);
				}
			}
			else
			{
				samples = speex_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], 0xffff);
				if (samples > 5)
				{
					if (!bMixFiles)
					{
						samplesout.insert(samplesout.end(), &samplesarray[0], &samplesarray[samples]);
					}
					else
					{
						tempSamples.insert(tempSamples.end(), &samplesarray[0], &samplesarray[samples]);
					}
				}
				else
				{
					time = oldtime;
				}
			}
		}
	}

	if (tempSamples.size())
	{
		if (samplesout.size() <= lastSampleOffset + tempSamples.size())
		{
			samplesout.resize(lastSampleOffset + tempSamples.size() + 1);
		}
		std::copy(tempSamples.begin(), tempSamples.end(), samplesout.begin() + lastSampleOffset);
	}

	speex_codec->Release();
	silk_codec->Release();
	opus_codec->Release();

	delete[] enc_samples;
	delete[] dec_samples;

	delete tmpBuff;

	return samplesout;
}

#define SILENT_LEN 48000 * 3

void remove_silence(std::vector<int16_t>& samples) {
	std::vector<int16_t> result = {};
	int silence_length = 0;
	for (int16_t sample : samples)
	{
		if (abs(sample) == 0) {
			silence_length++;
		}
		else
		{
			if (silence_length <= SILENT_LEN)
			{
				for (int i = 0; i < silence_length; i++)
				{
					result.push_back(0);
				}
			}
			result.push_back(sample);
			silence_length = 0;
		}
	}

	samples = result;
}

std::vector<unsigned char> loadFile(const std::string& fileName)
{
	std::vector<unsigned char> buffer = {};
	if (!fs::exists(fileName))
		return buffer;;
	std::ifstream fin(fileName.c_str(), std::ios::binary);
	fin.seekg(0, std::ios::end);
	auto size = fin.tellg();
	buffer.resize((size_t)size);;
	fin.seekg(0, std::ios::beg);
	fin.read((char*)&buffer[0], size);
	fin.close();
	return buffer;
}

int main()
{
	if (fs::exists("input"))
	{
		std::vector<fs::path> files = {};

		for (const auto& entry : fs::directory_iterator("input"))
		{
			if (fs::is_regular_file(entry))
			{
				files.push_back(entry.path());
			}
		}

		auto processFile =
			[&](const fs::path& filePath)
			{
				std::cout << std::endl << "Process " << filePath.string() << std::endl;

				std::vector<unsigned char> fileData = loadFile(filePath.string());

				if (fileData.size() < 6)
					return;
				fs::remove(filePath);

				std::string filename = filePath.filename().string();
				try
				{
					std::vector<int16_t> samples = decode(fileData);

					if (!bMixFiles)
					{
						if (samples.size() > 5)
						{
							FILE* file = NULL;
							fopen_s(&file, ("output/" + filename).c_str(), "wb");
							if (file)
							{
								waveFormatHeader_t* wh = NULL;
								wh = mono16bit8khzWaveHeaderForLength(samples.size());
								writeWaveHeaderToFile(wh, file);
								free(wh);
								fwrite(&samples[0], sizeof(int16_t), samples.size(), file);
								fclose(file);
							}
						}
					}
					else
					{
						if (mixed_samples.size() < samples.size())
						{
							mixed_samples.resize(samples.size() + 1, 0);
						}

						for (size_t i = 0; i < samples.size(); i++)
						{
							/*if (abs(samples[i]) > abs(mixed_samples[i]))
								mixed_samples[i] = samples[i];*/
							int a = samples[i];
							int b = mixed_samples[i];

							int mix;

							a += 32768;
							b += 32768;

							if ((a < 32768) || (b < 32768))
							{
								mix = a * b / 32768;
							}
							else {
								mix = 2 * (a + b) - (a * b) / 32768 - 65536;
							}

							if (mix >= 65536)
								mix = 65535;

							mix -= 32768;
							mixed_samples[i] = (short)mix;
						}
					}
				}
				catch (...)
				{
					std::cout << "Crash at \"" << filePath << "\"" << std::endl;
				}
			};

		for (auto file : files)
			processFile(file);

		if (bMixFiles && mixed_samples.size())
		{
			std::cout << "remove silence from merged voice..." << std::endl;
			remove_silence(mixed_samples);
			if (mixed_samples.size() > 0)
			{
				FILE* file = NULL;
				fopen_s(&file, "output/voice.wav", "wb");
				if (file)
				{
					waveFormatHeader_t* wh = NULL;
					wh = mono16bit8khzWaveHeaderForLength(mixed_samples.size());
					writeWaveHeaderToFile(wh, file);
					free(wh);
					fwrite(&mixed_samples[0], sizeof(int16_t), mixed_samples.size(), file);
					fclose(file);
				}
			}
		}
	}
}

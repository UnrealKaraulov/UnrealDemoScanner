#define SILK_VOICE_QUALITY			5
#define OPUS_VOICE_QUALITY			5
#define SPEEX_VOICE_QUALITY			5

#include "header.h"
#include "Buffer.hpp"
#include <iostream>
#include <filesystem>
#include <vector>
#include <chrono>
#include <fstream>
#include <algorithm>
#include <thread>
#include <mutex>
#include <execution>


namespace fs = std::filesystem;

bool anyMixFiles = false;
std::vector<int16_t> mixed_samples = {};
bool isFirstTime = false;
float first_time = 0.0f;

#define VALID_SAMPLES_TEST 4
#define TEAR_BETWEEN_FRAMES 0.75f
#define MAX_SILENT_LEN 48000 * 3 
#define MAX_TOTAL_TIME 36000.0f
#define MAX_CHUNK_COUNT 10000000

std::vector<int16_t> decode_all_codec(const std::vector<unsigned char>& input_bytes, const std::wstring& filename)
{
	IVoiceCodec* speex_codec = new VoiceCodec_Frame(new VoiceEncoder_Speex());
	IVoiceCodec* silk_codec = new CSteamP2PCodec(new VoiceEncoder_Silk());
	IVoiceCodec* opus_codec = new CSteamP2PCodec(new VoiceEncoder_Opus());

	std::vector<int16_t> samplesout = {};
	std::vector<int16_t> tempSamples = {};

	float targetSampleTime = 0.0f;
	int lastSampleOffset = 0;

	bool firstTime = true;

	std::vector<unsigned char> enc_samples(0x10000);
	std::vector<unsigned char> dec_samples(0x100000);

	Buffer* tmpBuff = new Buffer(input_bytes);

	unsigned char is_one_file = tmpBuff->readBytes<unsigned char>();
	unsigned char quality = tmpBuff->readBytes<unsigned char>();

	bool mixThisFile = (is_one_file > 0);
	if (mixThisFile) anyMixFiles = true;

	float totalTime = abs(tmpBuff->readBytes<float>());

	if (totalTime > MAX_TOTAL_TIME)
	{
		totalTime = MAX_TOTAL_TIME;
	}

	if (mixThisFile)
	{
		size_t initial_size = static_cast<size_t>(totalTime * 48000.0f);
		if (initial_size > 0) {
			samplesout.reserve(initial_size);
		}
	}

	speex_codec->Init(quality);
	silk_codec->Init(quality);
	opus_codec->Init(quality);

	float oldtime = 0.0f;
	float time = 0.0f;

	bool foundOpus = false;
	bool foundSilk = false;
	bool foundSpeex = false;

	static constexpr size_t MAX_ITERATIONS = 1000000;
	size_t iterations = 0;

	while (true)
	{
		if (tmpBuff->getReadOffset() + 4 >= input_bytes.size())
			break;

		oldtime = time;
		time = abs(tmpBuff->readBytes<float>());

		if (iterations++ > MAX_CHUNK_COUNT) {
			std::wcout << L"Too many iterations, possible corrupt file" << std::endl;
			break;
		}

		if (firstTime)
		{
			firstTime = false;
			oldtime = time;
			targetSampleTime = time;
		}

		if (mixThisFile)
		{
			if (abs(time - oldtime) > TEAR_BETWEEN_FRAMES)
			{
				lastSampleOffset = (int)(targetSampleTime * 48000.0f);

				if (samplesout.size() <= lastSampleOffset + tempSamples.size())
				{
					samplesout.resize(lastSampleOffset + tempSamples.size(), 0);
				}
				std::copy(tempSamples.begin(), tempSamples.end(), samplesout.begin() + lastSampleOffset);

				tempSamples.clear();
				targetSampleTime = time;
			}
		}

		int len = tmpBuff->readBytes<int>();
		if (len <= 0 || len > 0x10000)
		{
			std::wcout << L"Invalid frame length: " << len << std::endl;
			break;
		}

		if (len >= (int)enc_samples.size())
		{
			enc_samples.resize(len, 0);
		}

		tmpBuff->readData((char*)&enc_samples[0], len);

		int samples = opus_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], dec_samples.size());
		if (((foundSilk && !foundSpeex) && samples > VALID_SAMPLES_TEST) || foundOpus)
		{
			foundOpus = true;

			int16_t* samples_array = reinterpret_cast<int16_t*>(dec_samples.data());
			size_t sample_count = static_cast<size_t>(samples);

			if (samples <= 0)
				time = oldtime;

			if (!mixThisFile)
			{
				samplesout.insert(samplesout.end(), samples_array, samples_array + sample_count);
			}
			else
			{
				tempSamples.insert(tempSamples.end(), samples_array, samples_array + sample_count);
			}
		}
		else
		{
			samples = silk_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], dec_samples.size());
			if ((!foundSpeex && samples > VALID_SAMPLES_TEST) || foundSilk)
			{
				foundSilk = true;

				int16_t* samples_array = reinterpret_cast<int16_t*>(dec_samples.data());
				size_t sample_count = static_cast<size_t>(samples);

				if (samples <= 0)
					time = oldtime;

				if (!mixThisFile)
				{
					samplesout.insert(samplesout.end(), samples_array, samples_array + sample_count);
				}
				else
				{
					tempSamples.insert(tempSamples.end(), samples_array, samples_array + sample_count);
				}
			}
			else
			{
				samples = speex_codec->Decompress((const char*)&enc_samples[0], len, (char*)&dec_samples[0], dec_samples.size());
				if (samples > VALID_SAMPLES_TEST || foundSpeex)
				{
					foundSpeex = true;

					int16_t* samples_array = reinterpret_cast<int16_t*>(dec_samples.data());
					size_t sample_count = static_cast<size_t>(samples);

					if (samples <= 0)
						time = oldtime;

					if (!mixThisFile)
					{
						samplesout.insert(samplesout.end(), samples_array, samples_array + sample_count);
					}
					else
					{
						tempSamples.insert(tempSamples.end(), samples_array, samples_array + sample_count);
					}
				}
				else
				{
					time = oldtime;
				}
			}
		}
	}

	if (mixThisFile)
	{
		lastSampleOffset = (int)(targetSampleTime * 48000.0f);

		if (samplesout.size() <= lastSampleOffset + tempSamples.size())
		{
			samplesout.resize(lastSampleOffset + tempSamples.size(), 0);
		}
		std::copy(tempSamples.begin(), tempSamples.end(), samplesout.begin() + lastSampleOffset);

		tempSamples.clear();
		targetSampleTime = time;
	}

	if (tempSamples.size())
	{
		std::wcout << L"Decoded " << tempSamples.size() << L" samples from " << filename << L" using ";
		if (foundOpus)
		{
			std::wcout << L" opus codec" << std::endl;
		}
		else if (foundSilk)
		{
			std::wcout << L" silk codec" << std::endl;
		}
		else
		{
			std::wcout << L" speex codec" << std::endl;
		}

		if (samplesout.size() <= lastSampleOffset + tempSamples.size())
		{
			samplesout.resize(lastSampleOffset + tempSamples.size(), 0);
		}

		std::copy(tempSamples.begin(), tempSamples.end(), samplesout.begin() + lastSampleOffset);
	}
	else if (samplesout.size())
	{
		std::wcout << L"Decoded " << samplesout.size() << L" mixed samples from " << filename << L" using ";

		if (foundOpus)
		{
			std::wcout << L" opus codec" << std::endl;
		}
		else if (foundSilk)
		{
			std::wcout << L" silk codec" << std::endl;
		}
		else
		{
			std::wcout << L" speex codec" << std::endl;
		}

		if (samplesout.size() <= lastSampleOffset + tempSamples.size())
		{
			samplesout.resize(lastSampleOffset + tempSamples.size(), 0);
		}
	}

	speex_codec->Release();
	silk_codec->Release();
	opus_codec->Release();

	delete tmpBuff;

	return samplesout;
}

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
			if (silence_length <= MAX_SILENT_LEN)
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

void trim_trailing_silence(std::vector<int16_t>& samples) {
	size_t lastNonZero = samples.size();
	while (lastNonZero > 0 && samples[lastNonZero - 1] == 0) {
		--lastNonZero;
	}
	samples.resize(lastNonZero, 0);
}



std::vector<unsigned char> loadFile(const std::wstring& fileName)
{
	std::ifstream inputfile(fileName, std::ios::binary | std::ios::ate);
	if (inputfile)
	{
		std::streamsize size = inputfile.tellg();
		inputfile.seekg(0, std::ios::beg);
		std::vector<unsigned char> buffer(static_cast<size_t>(size));
		inputfile.read(reinterpret_cast<char*>(buffer.data()), size);
		return buffer;
	}
	return {};
}

int main()
{
	std::error_code err{};
	bool write_one = false;
	if (fs::exists("./input", err))
	{
		fs::create_directories(L"./output", err);

		std::vector<fs::path> files = {};

		for (const auto& entry : fs::directory_iterator("./input", err))
		{
			if (fs::is_regular_file(entry, err))
			{
				std::wcout << L"Found " << entry.path().wstring() << std::endl;
				files.push_back(entry.path());
			}
		}

		auto processFile =
			[&](const fs::path& filePath)
			{
				std::vector<unsigned char> fileData = loadFile(filePath.wstring());
				fs::remove(filePath, err);

				if (fileData.size() < 6)
				{
					std::wcout << std::endl << L"Error " << filePath.wstring() << L" is empty!" << std::endl;
					std::this_thread::sleep_for(std::chrono::seconds(2));
					return;
				}

				std::wcout << std::endl << L"Process " << filePath.wstring() << " with size " << fileData.size() << std::endl;


				std::wstring filename = filePath.filename().replace_extension(L".wav").wstring();
				try
				{
					std::vector<int16_t> samples = decode_all_codec(fileData, filename);

					if (!anyMixFiles)
					{
						remove_silence(samples);
						if (samples.size() > VALID_SAMPLES_TEST)
						{
							std::ofstream outputfile(L"./output/" + filename, std::ios::binary);
							if (outputfile)
							{
								write_one = true;
								waveFormatHeader_t* wh = NULL;
								wh = mono16bit48khzWaveHeaderForLength(samples.size());
								outputfile.write(reinterpret_cast<const char*>(wh), sizeof(waveFormatHeader_t));
								outputfile.write(reinterpret_cast<const char*>(samples.data()), sizeof(int16_t) * samples.size());
								free(wh);
								std::wcout << std::endl << L"Success! " << (L"./output/" + filename) << L" is valid!" << std::endl;
							}
							else
							{
								std::wcout << std::endl << L"Error " << filePath.wstring() << L" no write access!" << std::endl;
								std::this_thread::sleep_for(std::chrono::seconds(2));
								return;
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
					std::wcout << L"Crash at \"" << filePath << L"\"" << std::endl;
					std::this_thread::sleep_for(std::chrono::seconds(2));
				}
			};

		for (auto file : files)
			processFile(file);

		fs::remove(L"./output/voice.wav", err);

		if (anyMixFiles && mixed_samples.size())
		{
			std::wcout << L"remove silence from merged voice..." << std::endl;
			remove_silence(mixed_samples);
			trim_trailing_silence(mixed_samples);
			if (mixed_samples.size() > VALID_SAMPLES_TEST)
			{
				std::ofstream outputfile(L"./output/voice.wav", std::ios::binary);
				if (outputfile)
				{
					waveFormatHeader_t* wh = NULL;
					wh = mono16bit48khzWaveHeaderForLength(mixed_samples.size());
					outputfile.write(reinterpret_cast<const char*>(wh), sizeof(waveFormatHeader_t));
					outputfile.write(reinterpret_cast<const char*>(mixed_samples.data()), sizeof(int16_t) * mixed_samples.size());
					free(wh);

					std::wcout << std::endl << L"Success! ./output/voice.wav is valid!" << std::endl;
					std::this_thread::sleep_for(std::chrono::seconds(2));
				}
				else
				{
					std::wcout << std::endl << L"Error ./output/voice.wav no write access!" << std::endl;
					std::this_thread::sleep_for(std::chrono::seconds(2));
				}
			}
			else
			{
				std::wcout << L"Error! Merged wav is empty!" << std::endl;
				std::this_thread::sleep_for(std::chrono::seconds(2));
			}
		}
		else if (!write_one)
		{
			std::wcout << L"Error! All output wav is empty!" << std::endl;
			std::this_thread::sleep_for(std::chrono::seconds(2));
		}
		else
		{
			std::wcout << L"Success!" << std::endl;
			std::this_thread::sleep_for(std::chrono::seconds(2));
		}
	}
	else
	{
		std::wcout << L"Error! No input directory!" << std::endl;
		std::this_thread::sleep_for(std::chrono::seconds(2));
	}
}

#pragma once

#include "IVoiceCodec.h"
#include "utlbuffer.h"
#include "SKP_Silk_SDK_API.h"

class VoiceEncoder_Silk : public IVoiceCodec {
private:
	void * m_pEncoder;								/*     4     4 */
	//int m_API_fs_Hz;								/*     8     4 */
	int m_targetRate_bps;							/*    12     4 */
	int m_packetLoss_perc;							/*    16     4 */
	SKP_SILK_SDK_EncControlStruct m_encControl;		/*    20    32 */
	CUtlBuffer m_bufOverflowBytes;					/*    52    24 */

	void * m_pDecoder;								/*    76     4 */
	SKP_SILK_SDK_DecControlStruct m_decControl;		/*    80    20 */

public:
	VoiceEncoder_Silk();

	virtual ~VoiceEncoder_Silk();

	virtual bool Init(int quality);
	virtual void Release();
	virtual bool ResetState(); 
	virtual int Compress(const char *pUncompressedBytes, int nSamples, char *pCompressed, int maxCompressedBytes, bool bFinal);
	virtual int Decompress(const char *pCompressed, int compressedBytes, char *pUncompressed, int maxUncompressedBytes);

	int GetNumQueuedEncodingSamples() const { return m_bufOverflowBytes.TellPut() / 2; }
}; /* size: 100 */

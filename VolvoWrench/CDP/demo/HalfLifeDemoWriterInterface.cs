using System;
using System.Collections.Generic;
using System.Text;

namespace compLexity_Demo_Player
{
    public interface IHalfLifeDemoWriter
    {
        void AddMessageHandlers(HalfLifeDemoParser parser);
        void ProcessHeader(ref Byte[] header);
        void ProcessFirstGameDataFrame(ref Byte[] frameData);
        Boolean ShouldParseGameDataMessages(Byte frameType);
        Boolean ShouldWriteClientCommand(String command);
        Byte GetNewUserMessageId(Byte messageId);
        void WriteDemoInfo(Byte[] demoInfo, System.IO.MemoryStream ms);
    }
}

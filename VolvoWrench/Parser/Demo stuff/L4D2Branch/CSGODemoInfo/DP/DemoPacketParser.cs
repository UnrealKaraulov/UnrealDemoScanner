using VolvoWrench.DemoStuff.L4D2Branch.BitStreamUtil;
using VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DP.FastNetmessages;
using VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.Messages;

namespace VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DP
{
    public static class DemoPacketParser
    {
#if SLOW_PROTOBUF
		private static IEnumerable<IMessageParser> Parsers = (
			from type in Assembly.GetExecutingAssembly().GetTypes()
			where type.GetInterfaces().Contains(typeof(IMessageParser))
			let parser
 = (IMessageParser)type.GetConstructor(new Type[0]).Invoke(new object[0])
			orderby -parser.Priority
			select parser).ToArray();
#endif

        /// <summary>
        ///     Parses a demo-packet.
        /// </summary>
        /// <param name="bitstream">Bitstream.</param>
        /// <param name="demo">Demo.</param>
        public static void ParsePacket(IBitStream bitstream, DemoParser demo)
        {
            //As long as there is stuff to read
            while (!bitstream.ChunkFinished)
            {
                var cmd = bitstream.ReadProtobufVarInt(); //What type of packet is this?
                var length = bitstream.ReadProtobufVarInt(); //And how long is it?
                bitstream.BeginChunk(length * 8); //read length bytes
                if (cmd == (int) SVC_Messages.svc_PacketEntities)
                {
                    //Parse packet entities
                    new PacketEntities().Parse(bitstream, demo);
                }
                else if (cmd == (int) SVC_Messages.svc_GameEventList)
                {
                    //and all this other stuff
                    new GameEventList().Parse(bitstream, demo);
                }
                else if (cmd == (int) SVC_Messages.svc_GameEvent)
                {
                    new GameEvent().Parse(bitstream, demo);
                }
                else if (cmd == (int) SVC_Messages.svc_CreateStringTable)
                {
                    new CreateStringTable().Parse(bitstream, demo);
                }
                else if (cmd == (int) SVC_Messages.svc_UpdateStringTable)
                {
                    new UpdateStringTable().Parse(bitstream, demo);
                }
                else if (cmd == (int) NET_Messages.net_Tick)
                {
                    //and all this other stuff
                    new NETTick().Parse(bitstream, demo);
                }

                bitstream.EndChunk();
            }
        }
    }
}
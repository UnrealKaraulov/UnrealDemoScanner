using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;
using System;
using System.IO;
using System.Text;

namespace DemoScanner.DemoStuff.L4D2Branch.PortalStuff.GameHandler
{
    internal class Portal2CoopGameHandler : OrangeBoxGameHandler
    {
        private const string _startAdjustTypePbodyFlash = "Portal2 co-op P-Body Gain Control";
        private const string _startAdjustTypeAtlasFlash = "Portal2 co-op Atlas Gain Control";
        private const string _startAdjustTypeStandard = "Portal2 co-op Start Standard";
        private const string _endAdjustTypeStandard = "Portal2 co-op End Standard";
        private const string _endAdjustTypeRunEnd = "Portal2 co-op Run End";
        private readonly StringBuilder _debugBuffer;
        private readonly string[] _maps = Category.Portal2Coop.Maps;
        private string _endAdjustType;
        private int _endTick = -1;
        private string _startAdjustType;
        private int _startTick = -1;

        public Portal2CoopGameHandler()
        {
            Maps.AddRange(_maps);
            _debugBuffer = new StringBuilder();
        }

        public override DemoParseResult GetResult()
        {
            var result = base.GetResult();
            if (_startAdjustType != null)
            {
                result.StartAdjustmentType = _startAdjustType;
                result.StartAdjustmentTick = _startTick;
            }

            if (_endAdjustType != null)
            {
                result.EndAdjustmentType = _endAdjustType;
                result.EndAdjustmentTick = _endTick;
            }

            return result;
        }

        protected override ConsoleCmdResult ProcessConsoleCmd(BinaryReader br)
        {
            var consoleCmdResult = base.ProcessConsoleCmd(br);

            var stringBuilder = _debugBuffer;
            object[] currentTick = { CurrentTick, ": ", consoleCmdResult.Command, Environment.NewLine };
            stringBuilder.Append(string.Concat(currentTick));
            if (_startAdjustType == null && CurrentTick > 0 &&
                consoleCmdResult.Command == "ss_force_primary_fullscreen 0" && Map != "mp_coop_start")
            {
                _startAdjustType = "Portal2 co-op Start Standard";
                _startTick = CurrentTick;
            }

            if (_endAdjustType == null && CurrentTick > 0)
            {
                if (consoleCmdResult.Command.StartsWith("playvideo_end_level_transition") &&
                    Map != "mp_coop_paint_longjump_intro")
                {
                    _endAdjustType = "Portal2 co-op End Standard";
                    _endTick = CurrentTick;
                }
                else if (consoleCmdResult.Command ==
                         "playvideo_exitcommand_nointerrupt coop_outro end_movie vault-movie_outro" &&
                         Map == "mp_coop_paint_longjump_intro")
                {
                    _endAdjustType = "Portal2 co-op Run End";
                    _endTick = CurrentTick;
                }
            }

            return consoleCmdResult;
        }

        protected override PacketResult ProcessPacket(BinaryReader br)
        {
            var packetResult = base.ProcessPacket(br);
            var stringBuilder = _debugBuffer;
            object[] currentTick = { CurrentTick, ": ", packetResult.CurrentPosition, Environment.NewLine };
            stringBuilder.Append(string.Concat(currentTick));
            if (_startAdjustType == null && Map == "mp_coop_start" &&
                packetResult.CurrentPosition.Equals(new Point3D(-9896f, -4400f, 3048f)))
            {
                _startAdjustType = "Portal2 co-op Atlas Gain Control";
                _startTick = CurrentTick;
            }
            else if (Map == "mp_coop_start" &&
                     packetResult.CurrentPosition.Equals(new Point3D(-11168f, -4384f, 3040.03125f)))
            {
                _startAdjustType = "Portal2 co-op P-Body Gain Control";
                _startTick = CurrentTick;
            }

            return packetResult;
        }
    }
}
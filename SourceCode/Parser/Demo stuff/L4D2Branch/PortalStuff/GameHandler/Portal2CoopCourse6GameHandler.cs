using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;
using System;
using System.IO;
using System.Text;

namespace DemoScanner.DemoStuff.L4D2Branch.PortalStuff.GameHandler
{
    internal class Portal2CoopCourse6GameHandler : OrangeBoxGameHandler
    {
        private readonly StringBuilder _debugBuffer;
        private readonly string[] _maps = Category.Portal2CoopCourse6.Maps;
        private string _endAdjustType;
        private int _endTick = -1;
        private string _startAdjustType;
        private int _startTick = -1;

        public Portal2CoopCourse6GameHandler()
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
                consoleCmdResult.Command == "ss_force_primary_fullscreen 0")
            {
                _startAdjustType = "Portal2 co-op course 6 Start Standard";
                _startTick = CurrentTick;
            }

            if (_endAdjustType == null && CurrentTick > 0)
            {
                if (consoleCmdResult.Command.StartsWith("playvideo_end_level_transition") &&
                    Map != "mp_coop_paint_crazy_box")
                {
                    _endAdjustType = "Portal2 co-op course 6 End Standard";
                    _endTick = CurrentTick;
                }
                else if (consoleCmdResult.Command ==
                         "playvideo_exitcommand_nointerrupt dlc1_endmovie end_movie movie_outro" &&
                         Map == "mp_coop_paint_crazy_box")
                {
                    _endAdjustType = "Portal2 co-op course 6 Run End";
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
            if (_startAdjustType == null && packetResult.CurrentPosition.Equals(new Point3D(-9896f, -4400f, 3048f)))
            {
                _startAdjustType = "Portal2 co-op Atlas Gain Control";
                _startTick = CurrentTick;
            }
            else if (packetResult.CurrentPosition.Equals(new Point3D(-11168f, -4384f, 3040.03125f)))
            {
                _startAdjustType = "Portal2 co-op P-Body Gain Control";
                _startTick = CurrentTick;
            }

            return packetResult;
        }
    }
}
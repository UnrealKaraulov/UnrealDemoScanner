using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;
using System.IO;

namespace DemoScanner.DemoStuff.L4D2Branch.PortalStuff.GameHandler
{
    internal class PortalGameHandler : HL2GameHandler
    {
        private const string _crosshairAppearAdjustType = "Crosshair Appear";
        private const string _crosshairDisappearAdjustType = "Crosshair Disappear";
        private readonly string[] _maps = Category.Portal.Maps;
        private string _endAdjustType;
        private int _endTick = -1;
        private string _startAdjustType;
        private int _startTick = -1;

        public PortalGameHandler()
        {
            Maps.AddRange(_maps);
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

            if (_endAdjustType == null && Map == "escape_02" && consoleCmdResult.Command == "startneurotoxins 99999")
            {
                _endAdjustType = "Crosshair Disappear";
                _endTick = CurrentTick + 1;
            }
            else if (consoleCmdResult.Command.Contains("#SAVE#"))
            {
                _endAdjustType = "#SAVE# Flag";
                _endTick = CurrentTick;
            }

            return consoleCmdResult;
        }

        protected override PacketResult ProcessPacket(BinaryReader br)
        {
            var packetResult = base.ProcessPacket(br);

            if (_startAdjustType == null && Map == "testchmb_a_00" &&
                packetResult.CurrentPosition.Equals(new Point3D(-544f, -368.75f, 160f)))
            {
                _startAdjustType = "Crosshair Appear";
                _startTick = CurrentTick + 1;
            }

            return packetResult;
        }
    }
}
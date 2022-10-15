using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;
using System;
using System.IO;
using System.Text;

namespace DemoScanner.DemoStuff.L4D2Branch.PortalStuff.GameHandler
{
    internal class Portal2SpGameHandler : OrangeBoxGameHandler
    {
        private const string _crosshairAppearAdjustType = "Crosshair Appear";
        private const string _crosshairDisappearAdjustType = "Crosshair Disappear";
        private readonly StringBuilder _debugBuffer;

        private readonly string[] _maps = Category.Portal2Sp.Maps;

        // how many ticks from last portal shot to being at the checkpoint.
        // experimentally determined, may be wrong.
        private readonly int FINALE_END_TICK_OFFSET = -852;
        private readonly int INTRO_START_TICK_OFFSET = 1;
        private string _endAdjustType;
        private int _endTick = -1;
        private string _startAdjustType;

        private int _startTick = -1;

        // best guess. you can move at ~2-3 units/tick, so don't check exactly.
        private readonly Point3D FINALE_END_POS = new Point3D(54.1f, 159.2f, -201.4f);
        private readonly Point3D INTRO_START_POS = new Point3D(-8709.20f, 1690.07f, 28.00f);
        private readonly Point3D INTRO_START_TOL = new Point3D(0.02f, 0.02f, 0.5f);

        public Portal2SpGameHandler()
        {
            Maps.AddRange(_maps);
            _debugBuffer = new StringBuilder();
        }

        private bool onTheMoon(Point3D position)
        {
            // check if you're in a specific cylinder of volume and far enough below the floor.
            return Math.Pow(position.X - FINALE_END_POS.X, 2) + Math.Pow(position.Y - FINALE_END_POS.Y, 2) <
                   Math.Pow(50, 2)
                   && position.Z < FINALE_END_POS.Z;
        }

        private bool atSpawn(Point3D position)
        {
            // check if at the spawn coordinate for sp_a1_intro1
            return !(Math.Abs(position.X - INTRO_START_POS.X) > INTRO_START_TOL.X)
                   && !(Math.Abs(position.Y - INTRO_START_POS.Y) > INTRO_START_TOL.Y)
                   && !(Math.Abs(position.Z - INTRO_START_POS.Z) > INTRO_START_TOL.Z);
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
            if (consoleCmdResult.Command.Contains("#SAVE#"))
            {
                _endAdjustType = "#SAVE# Flag";
                _endTick = CurrentTick;
            }

            return consoleCmdResult;
        }

        protected override PacketResult ProcessPacket(BinaryReader br)
        {
            var packetResult = base.ProcessPacket(br);
            var stringBuilder = _debugBuffer;
            object[] currentTick = { CurrentTick, ": ", packetResult.CurrentPosition, Environment.NewLine };
            stringBuilder.Append(string.Concat(currentTick));
            if (_startAdjustType == null && Map == "sp_a1_intro1" && atSpawn(packetResult.CurrentPosition))
            {
                _startAdjustType = "Crosshair Appear";
                _startTick = CurrentTick + INTRO_START_TICK_OFFSET;
            }
            else if (_endAdjustType == null && Map == "sp_a4_finale4" && onTheMoon(packetResult.CurrentPosition))
            {
                _endAdjustType = "Crosshair Disappear";
                _endTick = CurrentTick + FINALE_END_TICK_OFFSET;
            }

            return packetResult;
        }
    }
}
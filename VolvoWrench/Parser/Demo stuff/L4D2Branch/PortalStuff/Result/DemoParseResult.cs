using System;

namespace VolvoWrench.DemoStuff.L4D2Branch.PortalStuff.Result
{
    public class DemoParseResult : ICloneable
    {
        public DemoParseResult()
        {
            StartAdjustmentTick = -1;
            EndAdjustmentTick = -1;
        }

        public int AdjustedTicks
        {
            get
            {
                if (StartAdjustmentTick > -1 && EndAdjustmentTick > -1) return EndAdjustmentTick - StartAdjustmentTick;
                if (StartAdjustmentTick > -1) return TotalTicks - StartAdjustmentTick;
                if (EndAdjustmentTick > -1) return EndAdjustmentTick;
                return TotalTicks;
            }
        }

        public int EndAdjustmentTick { get; set; }
        public string EndAdjustmentType { get; set; }
        public string FileName { get; set; }
        public string GameDir { get; set; }
        public string MapName { get; set; }
        public string PlayerName { get; set; }
        public int StartAdjustmentTick { get; set; }
        public string StartAdjustmentType { get; set; }
        public int TotalTicks { get; set; }

        public object Clone()
        {
            var demoParseResult = new DemoParseResult
            {
                FileName = FileName,
                MapName = MapName,
                PlayerName = PlayerName,
                GameDir = GameDir,
                TotalTicks = TotalTicks,
                StartAdjustmentTick = StartAdjustmentTick,
                StartAdjustmentType = StartAdjustmentType,
                EndAdjustmentTick = EndAdjustmentTick,
                EndAdjustmentType = EndAdjustmentType
            };
            return demoParseResult;
        }

        public float AdjustTime(float ticksPerSecond)
        {
            return AdjustedTicks * ticksPerSecond;
        }
    }
}
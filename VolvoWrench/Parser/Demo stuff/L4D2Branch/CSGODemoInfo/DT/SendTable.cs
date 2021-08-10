using System.Collections.Generic;
using VolvoWrench.DemoStuff.L4D2Branch.BitStreamUtil;

namespace VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DT
{
    internal class SendTable
    {
        public SendTable(IBitStream bitstream)
        {
            var dataTable = new DP.FastNetmessages.SendTable();

            foreach (var prop in dataTable.Parse(bitstream))
            {
                var property = new SendTableProperty
                {
                    DataTableName = prop.DtName,
                    HighValue = prop.HighValue,
                    LowValue = prop.LowValue,
                    Name = prop.VarName,
                    NumberOfBits = prop.NumBits,
                    NumberOfElements = prop.NumElements,
                    Priority = prop.Priority,
                    RawFlags = prop.Flags,
                    RawType = prop.Type
                };

                Properties.Add(property);
            }

            Name = dataTable.NetTableName;
            IsEnd = dataTable.IsEnd;
        }

        public List<SendTableProperty> Properties { get; } = new List<SendTableProperty>();
        public string Name { get; set; }
        public bool IsEnd { get; set; }
    }
}
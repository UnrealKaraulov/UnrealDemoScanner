using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DT
{
    internal class DataTableParser
    {
        private readonly List<ExcludeEntry> CurrentExcludes = new List<ExcludeEntry>();
        private List<ServerClass> CurrentBaseclasses = new List<ServerClass>();
        public List<SendTable> DataTables = new List<SendTable>();
        public List<ServerClass> ServerClasses = new List<ServerClass>();

        public int ClassBits => (int)Math.Ceiling(Math.Log(ServerClasses.Count, 2));

        public void ParsePacket(IBitStream bitstream)
        {
            while (true)
            {
                var type = (SVC_Messages)bitstream.ReadProtobufVarInt();
                if (type != SVC_Messages.svc_SendTable) throw new Exception("Expected SendTable, got " + type);

                var size = bitstream.ReadProtobufVarInt();
                bitstream.BeginChunk(size * 8);
                var sendTable = new SendTable(bitstream);
                bitstream.EndChunk();

                if (sendTable.IsEnd) break;

                DataTables.Add(sendTable);
            }

            var serverClassCount = checked((int)bitstream.ReadInt(16));

            for (var i = 0; i < serverClassCount; i++)
            {
                var entry = new ServerClass
                {
                    ClassID = checked((int)bitstream.ReadInt(16))
                };

                if (entry.ClassID > serverClassCount) throw new Exception("Invalid class index");

                entry.Name = bitstream.ReadDataTableString();
                entry.DTName = bitstream.ReadDataTableString();

                entry.DataTableID = DataTables.FindIndex(a => a.Name == entry.DTName);

                ServerClasses.Add(entry);
            }

            for (var i = 0; i < serverClassCount; i++) FlattenDataTable(i);
        }

        private void FlattenDataTable(int serverClassIndex)
        {
            var table = DataTables[ServerClasses[serverClassIndex].DataTableID];

            CurrentExcludes.Clear();
            CurrentBaseclasses = new List<ServerClass>(); //NOT .clear because we use *this* reference
            //LITERALLY 3 lines later. @main--, this is warning for you. 

            GatherExcludesAndBaseclasses(table, true);

            ServerClasses[serverClassIndex].BaseClasses = CurrentBaseclasses;

            GatherProps(table, serverClassIndex, "");

            var flattenedProps = ServerClasses[serverClassIndex].FlattenedProps;

            var priorities = new List<int>
            {
                64
            };
            priorities.AddRange(flattenedProps.Select(a => a.Prop.Priority).Distinct());
            priorities.Sort();

            var start = 0;
            for (var priorityIndex = 0; priorityIndex < priorities.Count; priorityIndex++)
            {
                var priority = priorities[priorityIndex];

                while (true)
                {
                    var currentProp = start;

                    while (currentProp < flattenedProps.Count)
                    {
                        var prop = flattenedProps[currentProp].Prop;

                        if (prop.Priority == priority ||
                            priority == 64 && prop.Flags.HasFlagFast(SendPropertyFlags.ChangesOften))
                        {
                            if (start != currentProp)
                            {
                                var temp = flattenedProps[start];
                                flattenedProps[start] = flattenedProps[currentProp];
                                flattenedProps[currentProp] = temp;
                            }

                            start++;
                            break;
                        }

                        currentProp++;
                    }

                    if (currentProp == flattenedProps.Count) break;
                }
            }
        }

        private void GatherExcludesAndBaseclasses(SendTable sendTable, bool collectBaseClasses)
        {
            CurrentExcludes.AddRange(
                sendTable.Properties
                    .Where(a => a.Flags.HasFlagFast(SendPropertyFlags.Exclude))
                    .Select(a => new ExcludeEntry(a.Name, a.DataTableName, sendTable.Name))
            );

            foreach (var prop in sendTable.Properties.Where(a => a.Type == SendPropertyType.DataTable))
                if (collectBaseClasses && prop.Name == "baseclass")
                {
                    GatherExcludesAndBaseclasses(GetTableByName(prop.DataTableName), true);
                    CurrentBaseclasses.Add(FindByDTName(prop.DataTableName));
                }
                else
                {
                    GatherExcludesAndBaseclasses(GetTableByName(prop.DataTableName), false);
                }
        }

        private void GatherProps(SendTable table, int serverClassIndex, string prefix)
        {
            var tmpFlattenedProps = new List<FlattenedPropEntry>();
            GatherProps_IterateProps(table, serverClassIndex, tmpFlattenedProps, prefix);

            var flattenedProps = ServerClasses[serverClassIndex].FlattenedProps;

            flattenedProps.AddRange(tmpFlattenedProps);
        }

        private void GatherProps_IterateProps(SendTable table, int ServerClassIndex,
            List<FlattenedPropEntry> flattenedProps, string prefix)
        {
            for (var i = 0; i < table.Properties.Count; i++)
            {
                var property = table.Properties[i];

                if (property.Flags.HasFlagFast(SendPropertyFlags.InsideArray) ||
                    property.Flags.HasFlagFast(SendPropertyFlags.Exclude) || IsPropExcluded(table, property))
                    continue;

                if (property.Type == SendPropertyType.DataTable)
                {
                    var subTable = GetTableByName(property.DataTableName);

                    if (property.Flags.HasFlagFast(SendPropertyFlags.Collapsible))
                    {
                        //we don't prefix Collapsible stuff, since it is just derived mostly
                        GatherProps_IterateProps(subTable, ServerClassIndex, flattenedProps, prefix);
                    }
                    else
                    {
                        //We do however prefix everything else

                        var nfix = prefix + (property.Name.Length > 0 ? property.Name + "." : "");

                        GatherProps(subTable, ServerClassIndex, nfix);
                    }
                }
                else
                {
                    if (property.Type == SendPropertyType.Array)
                        flattenedProps.Add(new FlattenedPropEntry(prefix + property.Name, property,
                            table.Properties[i - 1]));
                    else
                        flattenedProps.Add(new FlattenedPropEntry(prefix + property.Name, property, null));
                }
            }
        }

        private bool IsPropExcluded(SendTable table, SendTableProperty prop)
        {
            return CurrentExcludes.Exists(a => table.Name == a.DTName && prop.Name == a.VarName);
        }

        private SendTable GetTableByName(string pName)
        {
            return DataTables.FirstOrDefault(a => a.Name == pName);
        }

        public ServerClass FindByName(string className)
        {
            return ServerClasses.Single(a => a.Name == className);
        }

        private ServerClass FindByDTName(string dtName)
        {
            return ServerClasses.Single(a => a.DTName == dtName);
        }
    }
}
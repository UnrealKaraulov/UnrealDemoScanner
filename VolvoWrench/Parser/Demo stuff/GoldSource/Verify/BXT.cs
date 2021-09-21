using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;

namespace DemoScanner.DemoStuff.GoldSource.Verify
{
    /// <summary>
    ///     Types from BunnymodXT's runtime serialization.
    /// </summary>
    public static class Bxt
    {
        public enum RuntimeDataType : byte
        {
            VERSION_INFO = 1,
            CVAR_VALUES,
            TIME,
            BOUND_COMMAND,
            ALIAS_EXPANSION,
            SCRIPT_EXECUTION,
            COMMAND_EXECUTION,
            GAME_END_MARKER,
            LOADED_MODULES,
            CUSTOM_TRIGGER_COMMAND
        }

        [Serializable]
        public class VersionInfo : BXTData
        {
            public int build_number;
            public string bxt_version;


            public override void Read(BinaryReader br)
            {
                build_number = br.ReadInt32();
                bxt_version = new string(br.ReadChars(br.ReadInt32()));
            }

            public override string[] ToString()
            {
                return new[] {$"Version: [BUILD: {build_number}] [BXT_VERSION: {bxt_version}"};
            }
        }

        [Serializable]
        public class Time : BXTData
        {
            public uint hours;
            public byte minutes;
            public double remainder;
            public byte seconds;

            public override string[] ToString()
            {
                return new[] {hours + ":" + minutes + ":" + (seconds + remainder).ToString("F4")};
            }


            public override void Read(BinaryReader br)
            {
                hours = br.ReadUInt32();
                minutes = br.ReadByte();
                seconds = br.ReadByte();
                remainder = br.ReadDouble();
            }
        }

        [Serializable]
        public class BoundCommand : BXTData
        {
            public string command;

            public override void Read(BinaryReader br)
            {
                command = new string(br.ReadChars(br.ReadInt32()));
            }

            public override string[] ToString()
            {
                return new[] {$"Command: {command}"};
            }
        }

        [Serializable]
        public class AliasExpansion : BXTData
        {
            public string command;
            public string name;

            public override void Read(BinaryReader br)
            {
                name = new string(br.ReadChars(br.ReadInt32()));
                command = new string(br.ReadChars(br.ReadInt32()));
            }

            public override string[] ToString()
            {
                return new[] {$"alias {name} \"{command}\""};
            }
        }

        public class ScriptExecution : BXTData
        {
            public string contents;
            public string filename;

            public override void Read(BinaryReader br)
            {
                filename = new string(br.ReadChars(br.ReadInt32()));
                contents = new string(br.ReadChars(br.ReadInt32()));
            }

            public override string[] ToString()
            {
                return new[]
                {
                    $"exec {filename}",
                    "Contents:",
                    "\t " + contents
                };
            }
        }

        [Serializable]
        public class CommandExecution : BXTData
        {
            public string command;

            public override void Read(BinaryReader br)
            {
                command = new string(br.ReadChars(br.ReadInt32()));
            }

            public override string[] ToString()
            {
                return new[]
                {
                    $"Command: {command}"
                };
            }
        }

        [Serializable]
        public class LoadedModules : BXTData
        {
            public List<string> filenames;

            public override void Read(BinaryReader br)
            {
                filenames = new List<string>();
                var count = br.ReadUInt32();
                for (var i = 0; i < count; i++) filenames.Add(new string(br.ReadChars(br.ReadInt32())));
            }

            public override string[] ToString()
            {
                return new[]
                {
                    "Loaded modules:"
                }.Concat(filenames).ToArray();
            }
        }

        [Serializable]
        public class CustomTriggerCommand : BXTData
        {
            public string command;
            public Point3D corner_max;
            public Point3D corner_min;

            public override void Read(BinaryReader br)
            {
                corner_min = new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                corner_max = new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                command = new string(br.ReadChars(br.ReadInt32()));
            }

            public override string[] ToString()
            {
                return new[]
                {
                    $"Custom trigger [command:{command}] - corner_min[{corner_min.X},{corner_min.Y},{corner_min.Z}] - corner_max[{corner_max.X},{corner_max.Y},{corner_max.Z}]"
                };
            }
        }

        public class GameEndMarker : BXTData
        {
            public override void Read(BinaryReader br)
            {
            }

            public override string[] ToString()
            {
                return new[] {"Game end marker"};
            }
        }

        [Serializable]
        public class CVarValues : BXTData
        {
            public List<KeyValuePair<string, string>> CVars;


            public override void Read(BinaryReader br)
            {
                CVars = new List<KeyValuePair<string, string>>();
                var cvarnum = br.ReadUInt32();
                for (var i = 0; i < cvarnum; i++)
                {
                    var fsl = br.ReadInt32();
                    var fs = new string(br.ReadChars(fsl));
                    var ssl = br.ReadInt32();
                    var ss = new string(br.ReadChars(ssl));
                    CVars.Add(new KeyValuePair<string, string>(fs, ss));
                }
            }

            public override string[] ToString()
            {
                return new[]
                {
                    "Cvars:"
                }.Concat(CVars.Select(x => x.Key + ": " + x.Value)).ToArray();
            }
        }

        public abstract class BXTData
        {
            /// <summary>
            ///     Read the data.
            /// </summary>
            public abstract void Read(BinaryReader br);

            /// <summary>
            ///     Print the values to lines for searching
            /// </summary>
            public new abstract string[] ToString();
        }
    }
}
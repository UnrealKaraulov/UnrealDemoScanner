using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DemoScanner.DemoStuff.GoldSource.Verify
{
    public class BXTVerify
    {
        private readonly Config verconfig;

        public BXTVerify(Config c)
        {
            verconfig = c;
        }

        /// <summary>
        ///     Parses the bxt data into treenodes
        /// </summary>
        /// <param name="Infos"></param>
        public Tuple<TreeNode, string> ParseBxtData(KeyValuePair<string, CrossParseResult> info)
        {
            var ret = "\n";
            const string bxtVersion = "34ecc635d8a4ac9a210614374af66ebffa36c656-CLEAN based on mar-27-2019";
            var cvarRules = new Dictionary<string, string>
            {
                {"BXT_AUTOJUMP", "0"},
                {"BXT_BHOPCAP", "0"},
                {"BXT_FADE_REMOVE", "0"},
                {"BXT_HUD_DISTANCE", "0"},
                {"BXT_HUD_ENTITY_HP", "0"},
                {"BXT_HUD_ORIGIN", "0"},
                {"BXT_HUD_SELFGAUSS", "0"},
                {"BXT_HUD_USEABLES", "0"},
                {"BXT_HUD_VELOCITY", "0"},
                {"BXT_HUD_VISIBLE_LANDMARKS", "0"},
                {"BXT_SHOW_HIDDEN_ENTITIES", "0"},
                {"BXT_SHOW_TRIGGERS", "0"},
                {"CHASE_ACTIVE", "0"},
                {"CL_ANGLESPEEDKEY", "0.67"},
                {"CL_BACKSPEED", "400"},
                {"CL_FORWARDSPEED", "400"},
                {"CL_PITCHDOWN", "89"},
                {"CL_PITCHSPEED", "225"},
                {"CL_PITCHUP", "89"},
                {"CL_SIDESPEED", "400"},
                {"CL_UPSPEED", "320"},
                {"CL_YAWSPEED", "210"},
                {"GL_MONOLIGHTS", "0"},
                {"HOST_FRAMERATE", "0"},
                {"HOST_SPEEDS", "0"},
                {"R_DRAWENTITIES", "1"},
                {"R_FULLBRIGHT", "0"},
                {"SND_SHOW", "0"},
                {"SV_AIRACCELERATE", "10"},
                {"SV_CHEATS", "0"},
                {"SV_FRICTION", "4"},
                {"SV_GRAVITY", "800"},
                {"SV_WATERACCELERATE", "10"},
                {"SV_WATERFRICTION", "1"},
                {"S_SHOW", "0"}
            };
            var demonode = new TreeNode(Path.GetFileName(info.Key)) {ForeColor = Color.White};
            for (var i = 0; i < info.Value.GsDemoInfo.IncludedBXtData.Count; i++)
            {
                int jp = 0, jm = 0, dp = 0, dm = 0;
                var datanode = new TreeNode("\nBXT Data Frame [" + i + "]") {ForeColor = Color.White};
                for (var index = 0; index < info.Value.GsDemoInfo.IncludedBXtData[i].Objects.Count; index++)
                {
                    var t = info.Value.GsDemoInfo.IncludedBXtData[i].Objects[index];
                    switch (t.Key)
                    {
                        case Bxt.RuntimeDataType.VERSION_INFO:
                        {
                            ret += "\t" + "BXT Version: " +
                                   (((Bxt.VersionInfo) t.Value).bxt_version == bxtVersion
                                       ? "Good"
                                       : "INVALID=" + ((Bxt.VersionInfo) t.Value).bxt_version) + " Frame: " + i + "\n";
                            ret += "\t" + "Game Version: " + ((Bxt.VersionInfo) t.Value).build_number + "\n";
                            datanode.Nodes.Add(new TreeNode("Version info")
                            {
                                ForeColor = Color.White,
                                Nodes =
                                {
                                    new TreeNode("Game version: " + ((Bxt.VersionInfo) t.Value).build_number),
                                    new TreeNode("BXT Version: " + ((Bxt.VersionInfo) t.Value).bxt_version)
                                }
                            });
                            break;
                        }
                        case Bxt.RuntimeDataType.CVAR_VALUES:
                        {
                            foreach (var cvar in ((Bxt.CVarValues) t.Value).CVars
                                .Where(cvar => cvarRules.ContainsKey(cvar.Key.ToUpper()))
                                .Where(cvar => cvarRules[cvar.Key.ToUpper()] != cvar.Value.ToUpper()))
                                ret += "\t" + "Illegal Cvar: " + cvar.Key + " " + cvar.Value + " Frame: " + i + "\n";
                            var cvarnode = new TreeNode("Cvars [" + ((Bxt.CVarValues) t.Value).CVars.Count + "]")
                            {
                                ForeColor = Color.White
                            };
                            cvarnode.Nodes.AddRange(
                                ((Bxt.CVarValues) t.Value).CVars.Select(
                                    x => new TreeNode(x.Key + " " + x.Value) {ForeColor = Color.White}).ToArray());
                            datanode.Nodes.Add(cvarnode);
                            break;
                        }
                        case Bxt.RuntimeDataType.TIME:
                        {
                            if (i + 1 == info.Value.GsDemoInfo.IncludedBXtData.Count)
                                ret += "\t" + "Demo bxt time: " + ((Bxt.Time) t.Value).ToString() + " Frame: " + i +
                                       "\n";
                            datanode.Nodes.Add(new TreeNode("Time: " + ((Bxt.Time) t.Value).ToString())
                            {
                                ForeColor = Color.White
                            });
                            break;
                        }
                        case Bxt.RuntimeDataType.BOUND_COMMAND:
                        {
                            if (((Bxt.BoundCommand) t.Value).command.ToUpper().Contains("+JUMP")) jp++;

                            if (((Bxt.BoundCommand) t.Value).command.ToUpper().Contains("-JUMP")) jm++;

                            if (((Bxt.BoundCommand) t.Value).command.ToUpper().Contains("+DUCK")) dp++;

                            if (((Bxt.BoundCommand) t.Value).command.ToUpper().Contains("-DUCK")) dm++;

                            if (((Bxt.BoundCommand) t.Value).command.ToUpper().Contains(";"))
                                ret += "\t" + "Possible script: " + ((Bxt.BoundCommand) t.Value).command + " Frame: " +
                                       i + "\n";
                            datanode.Nodes.Add(new TreeNode("Bound command: " + ((Bxt.BoundCommand) t.Value).command)
                            {
                                ForeColor = Color.White
                            });
                            break;
                        }
                        case Bxt.RuntimeDataType.ALIAS_EXPANSION:
                        {
                            ret += "\t" + "Alias [" + ((Bxt.AliasExpansion) t.Value).name + "]: " +
                                   ((Bxt.AliasExpansion) t.Value).command + " Frame: " + i + "\n";
                            datanode.Nodes.Add(
                                new TreeNode("Alias name: " + ((Bxt.AliasExpansion) t.Value).name + "Command:" +
                                             ((Bxt.AliasExpansion) t.Value).command) {ForeColor = Color.White});
                            break;
                        }
                        case Bxt.RuntimeDataType.SCRIPT_EXECUTION:
                        {
                            datanode.Nodes.Add(new TreeNode("Script: " + ((Bxt.ScriptExecution) t.Value).filename)
                            {
                                ForeColor = Color.White,
                                Nodes =
                                {
                                    new TreeNode(((Bxt.ScriptExecution) t.Value).contents) {ForeColor = Color.White}
                                }
                            });
                            break;
                        }
                        case Bxt.RuntimeDataType.COMMAND_EXECUTION:
                        {
                            if (((Bxt.CommandExecution) t.Value).command.ToUpper().Contains("+JUMP"))
                            {
                                if (jp == 0)
                                    ret += "\t" + "Possible autojump: " + ((Bxt.CommandExecution) t.Value).command +
                                           " Frame: " + i + "\n";
                                else
                                    jp--;
                            }

                            if (((Bxt.CommandExecution) t.Value).command.ToUpper().Contains("-JUMP"))
                            {
                                if (jm == 0)
                                    ret += "\t" + "Possible autojump: " + ((Bxt.CommandExecution) t.Value).command +
                                           " Frame: " + i + "\n";
                                else
                                    jm--;
                            }

                            if (((Bxt.CommandExecution) t.Value).command.ToUpper().Contains("+DUCK"))
                            {
                                if (dp == 0)
                                    ret += "\t" + "Possible ducktap: " + ((Bxt.CommandExecution) t.Value).command +
                                           " Frame: " + i + "\n";
                                else
                                    dp--;
                            }

                            if (((Bxt.CommandExecution) t.Value).command.ToUpper().Contains("-DUCK"))
                            {
                                if (dm == 0)
                                    ret += "\t" + "Possible ducktap: " + ((Bxt.CommandExecution) t.Value).command +
                                           " Frame: " + i + "\n";
                                else
                                    dm--;
                            }

                            if (((Bxt.CommandExecution) t.Value).command.ToUpper().ToUpper().Contains("BXT"))
                                ret += "\t" + "Disallowed bxt command: " + ((Bxt.CommandExecution) t.Value).command +
                                       " Frame: " + i + "\n";
                            datanode.Nodes.Add(new TreeNode("Command: " + ((Bxt.CommandExecution) t.Value).command)
                            {
                                ForeColor = Color.White
                            });
                            break;
                        }
                        case Bxt.RuntimeDataType.GAME_END_MARKER:
                        {
                            datanode.Nodes.Add(new TreeNode("-- GAME END --") {ForeColor = Color.White});
                            break;
                        }
                        case Bxt.RuntimeDataType.LOADED_MODULES:
                        {
                            var modulesnode =
                                new TreeNode("Loaded modules [" + ((Bxt.LoadedModules) t.Value).filenames.Count + "]")
                                    {ForeColor = Color.White};
                            modulesnode.Nodes.AddRange(((Bxt.LoadedModules) t.Value).filenames
                                .Select(x => new TreeNode(x) {ForeColor = Color.White}).ToArray());
                            datanode.Nodes.Add(modulesnode);
                            break;
                        }
                        case Bxt.RuntimeDataType.CUSTOM_TRIGGER_COMMAND:
                        {
                            var trigger = (Bxt.CustomTriggerCommand) t.Value;
                            ret += "\t" +
                                   $"Custom trigger X1:{trigger.corner_max.X} Y1:{trigger.corner_max.Y} Z1:{trigger.corner_max.Z} X2:{trigger.corner_min.X} Y2:{trigger.corner_min.Y} Z2:{trigger.corner_min.Z}" +
                                   " Frame: " + i + "\n";
                            datanode.Nodes.Add(
                                new TreeNode(
                                    $"Custom trigger X1:{trigger.corner_max.X} Y1:{trigger.corner_max.Y} Z1:{trigger.corner_max.Z} X2:{trigger.corner_min.X} Y2:{trigger.corner_min.Y} Z2:{trigger.corner_min.Z}")
                                {
                                    ForeColor = Color.White,
                                    Nodes = {new TreeNode("Command: " + trigger.command) {ForeColor = Color.White}}
                                });
                            break;
                        }
                        default:
                        {
                            datanode.Nodes.Add(new TreeNode("Invalid bxt data!") {ForeColor = Color.Red});
                            break;
                        }
                    }
                }

                demonode.Nodes.Add(datanode);
            }

            ret += "\n";
            return new Tuple<TreeNode, string>(demonode, ret);
        }
    }
}
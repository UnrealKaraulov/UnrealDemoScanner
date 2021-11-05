using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DemoScanner.DemoStuff.GoldSource.Verify;
using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;

namespace DemoScanner.DemoStuff.GoldSource
{
    /// <summary>
    ///     Types for HLSOOE
    /// </summary>
    public class Hlsooe
    {

        public enum DemoFrameType : sbyte
        {
            StartupPacket = 1,
            NetworkPacket = 2,
            Jumptime = 3,
            ConsoleCommand = 4,
            Usercmd = 5,
            Stringtables = 6,
            NetworkDataTable = 7,
            NextSection = 8
        }

        public interface IFrame
        {}

        public class DemoFrame
        {
            public int Frame;
            public int Index;
            public float Time;
            public DemoFrameType Type;
        }

        public class ConsoleCommandFrame : IFrame
        {
            public string Command;
        }

        public class StringTablesFrame : IFrame
        {
            public string Data;
        }

        public class JumpTimeFrame : IFrame
        {}

        public class NextSectionFrame : IFrame
        {}

        public class StartupPacketFrame : NetMsgFrame
        {}

        public class ErrorFrame : NetMsgFrame
        {}

        public class NetworkDataTableFrame : IFrame
        {
            public string Data;
        }

        public class UserCmdFrame : IFrame
        {
            public string Data;
            public int OutgoingSequence;
            public int Slot;
        }

        public class NetMsgFrame : IFrame
        {
            public int Flags;
            public int IncomingAcknowledged;
            public int IncomingReliableAcknowledged;
            public int IncomingReliableSequence;
            public int IncomingSequence;
            public int LastReliableSequence;
            public Point3D LocalViewAngles;
            public Point3D LocalViewAngles2;
            public string Msg;
            public int OutgoingSequence;
            public int ReliableSequence;
            public Point3D ViewAngles;
            public Point3D ViewAngles2;
            public Point3D ViewOrigin2;
            public Point3D ViewOrigins;
        }

        /// <summary>
        ///     This is the  header of HLS:OOE demos
        /// </summary>
        public class DemoHeader : DemoStuff.DemoHeader
        {
            /// <summary>
            ///     The byte offset to the first directory entry
            /// </summary>
            public int DirectoryOffset;

            /// <summary>
            ///     The magic of the file mostly HLDEMO
            /// </summary>
            public string Magic;
        }

        public struct FramesHren2
        {
            public DemoFrame Key;
            public IFrame Value;
            public FramesHren2(DemoFrame key, IFrame value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
        public class DemoDirectoryEntry
        {
            public int Filelength;
            public Dictionary<DemoFrame, ConsoleCommandFrame> Flags;
            public int FrameCount;
            public List<FramesHren2> Frames;
            public int Offset;
            public float PlaybackTime;
            public int Type;
        }
    }

    /// <summary>
    ///     Types for goldsource
    /// </summary>
    public class GoldSource
    {
        public enum DemoFrameType
        {
            NetMsg = 1,
            DemoStart = 2,
            ConsoleCommand = 3,
            ClientData = 4,
            NextSection = 5,
            Event = 6,
            WeaponAnim = 7,
            Sound = 8,
            DemoBuffer = 9
        }

        public enum EngineVersions
        {
            Unknown,
            HalfLife1104,
            HalfLife1106,
            HalfLife1107,
            HalfLife1108,
            HalfLife1109,
            HalfLife1108Or1109,
            HalfLife1110,
            HalfLife1111, // Steam
            HalfLife1110Or1111
        }

        public string EngineName(int name)
        {
            var s = "Half-Life v";

            switch ((EngineVersions)name)
            {
                case EngineVersions.HalfLife1104:
                    s += "1.1.0.4";
                    break;

                case EngineVersions.HalfLife1106:
                    s += "1.1.0.6";
                    break;

                case EngineVersions.HalfLife1107:
                    s += "1.1.0.7";
                    break;

                case EngineVersions.HalfLife1108:
                    s += "1.1.0.8";
                    break;

                case EngineVersions.HalfLife1109:
                    s += "1.1.0.9";
                    break;

                case EngineVersions.HalfLife1108Or1109:
                    s += "1.1.0.8 or v1.1.0.9";
                    break;

                case EngineVersions.HalfLife1110:
                    s += "1.1.1.0";
                    break;

                case EngineVersions.HalfLife1111:
                    s += "1.1.1.1";
                    break;

                case EngineVersions.HalfLife1110Or1111:
                    s += "1.1.1.0 or v1.1.1.1";
                    break;

                default:
                    return "Half-Life Unknown Version";
            }

            return s;
        }

        /// <summary>
        ///     The header of the GoldSource demo
        /// </summary>
        public class DemoHeader : DemoStuff.DemoHeader
        {
            /// <summary>
            ///     Byte offset untill the first directory enry
            /// </summary>
            public int DirectoryOffset;

            /// <summary>
            ///     Map ID
            /// </summary>
            public uint MapCrc;
        }

        public class IncludedBXtData
        {
            /// <summary>
            /// The included objects
            /// </summary>
            public List<KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>> Objects;
        }

        public struct FramesHren
        {
            public DemoFrame Key;
            public IFrame Value;
            public FramesHren(DemoFrame key, IFrame value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DemoDirectoryEntry
        {
            public int CdTrack;
            public string Description;
            public int FileLength;
            public int Flags;
            public int FrameCount;
            public List<FramesHren> Frames;
            public int Offset;
            public float TrackTime;
            public int Type;
        }

        public interface IFrame
        {}

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Point4D
        {
            public int W;
            public int X;
            public int Y;
            public int Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DemoFrame
        {
            public int FrameIndex;
            public int Index;
            public float Time;
            public DemoFrameType Type;
        }

        // DEMO_START: no extra data.
        public struct NextSectionFrame : IFrame
        {
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ConsoleCommandFrame : IFrame
        {
            public string Command;
            public byte[] BxtData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ClientDataFrame : IFrame
        {
            public float Fov;
            public Point3D Origin;
            public DPoint3D Viewangles;
            public int WeaponBits;
            public int ErrAngles;
        }

        // NEXT_SECTION: no extra data.

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EventFrame : IFrame
        {
            public float Delay;
            public EventArgs EventArguments;
            public int Flags;
            public int Index;

            public struct EventArgs
            {
                public DPoint3D Angles;
                public int Bparam1;
                public int Bparam2;
                public int Ducking;
                public int EntityIndex;
                public int Flags;
                public float Fparam1;
                public float Fparam2;
                public int Iparam1;
                public int Iparam2;
                public Point3D Origin;
                public Point3D Velocity;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WeaponAnimFrame : IFrame
        {
            public int Anim;
            public int Body;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SoundFrame : IFrame
        {
            public float Attenuation;
            public int Channel;
            public int Flags;
            public int Pitch;
            public byte[] Sample;
            public float Volume;
        }

        public struct DemoBufferFrame : IFrame { public byte[] Buffer; }

        public struct DemoStartFrame : IFrame { }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        // Otherwise, netmsg.
        public struct NetMsgFrame : IFrame
        {
            public int IncomingAcknowledged;
            public int IncomingReliableAcknowledged;
            public int IncomingReliableSequence;
            public int IncomingSequence;
            public int LastReliableSequence;
            public string Msg;
            public byte[] MsgBytes;
            public MoveVars MVars;
            public int OutgoingSequence;
            public int ReliableSequence;
            public RefParams RParms;
            public float Timestamp;
            public UserCmd UCmd;
            public Point3D View;
            public int Viewmodel;

            public override bool Equals(object obj)
            {
                return obj is NetMsgFrame frame &&
                       EqualityComparer<RefParams>.Default.Equals(RParms, frame.RParms) &&
                       Timestamp == frame.Timestamp &&
                       EqualityComparer<UserCmd>.Default.Equals(UCmd, frame.UCmd) &&
                       EqualityComparer<Point3D>.Default.Equals(View, frame.View);
            }

            public override int GetHashCode()
            {
                int hashCode = -29378089;
                hashCode = hashCode * -1521134295 + RParms.GetHashCode();
                hashCode = hashCode * -1521134295 + Timestamp.GetHashCode();
                hashCode = hashCode * -1521134295 + UCmd.GetHashCode();
                hashCode = hashCode * -1521134295 + View.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(NetMsgFrame left, NetMsgFrame right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(NetMsgFrame left, NetMsgFrame right)
            {
                return !(left == right);
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct RefParams
            {
                public DPoint3D ClViewangles;
                public Point3D Crosshairangle;
                public int Demoplayback;
                public Point3D Forward;
                public float Frametime;
                public int Hardware;
                public int Health;
                public float Idealpitch;
                public int Intermission;
                public int Maxclients;
                public int MaxEntities;
                public int NextView;
                public int Onground;
                public int OnlyClientDraw;
                public int Paused;
                public int Playernum;
                public int PtrCmd;
                public int PtrMovevars;
                public Point3D Punchangle;
                public Point3D Right;
                public Point3D Simorg;
                public Point3D Simvel;
                public int Smoothing;
                public int Spectator;
                public float Time;
                public Point3D Up;
                public DPoint3D Viewangles;
                public int Viewentity;
                public Point3D Viewheight;
                public Point3D Vieworg;
                public Point4D Viewport;
                public float Viewsize;
                public int Waterlevel;

                public override bool Equals(object obj)
                {
                    return obj is RefParams @params &&
                           EqualityComparer<DPoint3D>.Default.Equals(ClViewangles, @params.ClViewangles) &&
                           EqualityComparer<Point3D>.Default.Equals(Crosshairangle, @params.Crosshairangle) &&
                           Demoplayback == @params.Demoplayback &&
                           EqualityComparer<Point3D>.Default.Equals(Forward, @params.Forward) &&
                           Hardware == @params.Hardware &&
                           Health == @params.Health &&
                           Idealpitch == @params.Idealpitch &&
                           Intermission == @params.Intermission &&
                           Maxclients == @params.Maxclients &&
                           MaxEntities == @params.MaxEntities &&
                           NextView == @params.NextView &&
                           Onground == @params.Onground &&
                           OnlyClientDraw == @params.OnlyClientDraw &&
                           Paused == @params.Paused &&
                           Playernum == @params.Playernum &&
                           PtrCmd == @params.PtrCmd &&
                           PtrMovevars == @params.PtrMovevars &&
                           EqualityComparer<Point3D>.Default.Equals(Punchangle, @params.Punchangle) &&
                           EqualityComparer<Point3D>.Default.Equals(Right, @params.Right) &&
                           EqualityComparer<Point3D>.Default.Equals(Simorg, @params.Simorg) &&
                           EqualityComparer<Point3D>.Default.Equals(Simvel, @params.Simvel) &&
                           Smoothing == @params.Smoothing &&
                           Spectator == @params.Spectator &&
                           Time == @params.Time &&
                           EqualityComparer<Point3D>.Default.Equals(Up, @params.Up) &&
                           EqualityComparer<DPoint3D>.Default.Equals(Viewangles, @params.Viewangles) &&
                           Viewentity == @params.Viewentity &&
                           EqualityComparer<Point3D>.Default.Equals(Viewheight, @params.Viewheight) &&
                           EqualityComparer<Point3D>.Default.Equals(Vieworg, @params.Vieworg) &&
                           EqualityComparer<Point4D>.Default.Equals(Viewport, @params.Viewport) &&
                           Viewsize == @params.Viewsize &&
                           Waterlevel == @params.Waterlevel;
                }

                public override int GetHashCode()
                {
                    int hashCode = -318104989;
                    hashCode = hashCode * -1521134295 + ClViewangles.GetHashCode();
                    hashCode = hashCode * -1521134295 + Crosshairangle.GetHashCode();
                    hashCode = hashCode * -1521134295 + Demoplayback.GetHashCode();
                    hashCode = hashCode * -1521134295 + Forward.GetHashCode();
                    hashCode = hashCode * -1521134295 + Hardware.GetHashCode();
                    hashCode = hashCode * -1521134295 + Health.GetHashCode();
                    hashCode = hashCode * -1521134295 + Idealpitch.GetHashCode();
                    hashCode = hashCode * -1521134295 + Intermission.GetHashCode();
                    hashCode = hashCode * -1521134295 + Maxclients.GetHashCode();
                    hashCode = hashCode * -1521134295 + MaxEntities.GetHashCode();
                    hashCode = hashCode * -1521134295 + NextView.GetHashCode();
                    hashCode = hashCode * -1521134295 + Onground.GetHashCode();
                    hashCode = hashCode * -1521134295 + OnlyClientDraw.GetHashCode();
                    hashCode = hashCode * -1521134295 + Paused.GetHashCode();
                    hashCode = hashCode * -1521134295 + Playernum.GetHashCode();
                    hashCode = hashCode * -1521134295 + PtrCmd.GetHashCode();
                    hashCode = hashCode * -1521134295 + PtrMovevars.GetHashCode();
                    hashCode = hashCode * -1521134295 + Punchangle.GetHashCode();
                    hashCode = hashCode * -1521134295 + Right.GetHashCode();
                    hashCode = hashCode * -1521134295 + Simorg.GetHashCode();
                    hashCode = hashCode * -1521134295 + Simvel.GetHashCode();
                    hashCode = hashCode * -1521134295 + Smoothing.GetHashCode();
                    hashCode = hashCode * -1521134295 + Spectator.GetHashCode();
                    hashCode = hashCode * -1521134295 + Time.GetHashCode();
                    hashCode = hashCode * -1521134295 + Up.GetHashCode();
                    hashCode = hashCode * -1521134295 + Viewangles.GetHashCode();
                    hashCode = hashCode * -1521134295 + Viewentity.GetHashCode();
                    hashCode = hashCode * -1521134295 + Viewheight.GetHashCode();
                    hashCode = hashCode * -1521134295 + Vieworg.GetHashCode();
                    hashCode = hashCode * -1521134295 + Viewport.GetHashCode();
                    hashCode = hashCode * -1521134295 + Viewsize.GetHashCode();
                    hashCode = hashCode * -1521134295 + Waterlevel.GetHashCode();
                    return hashCode;
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct UserCmd
            {
                public sbyte Align1;
                public sbyte Align2;
                public sbyte Align3;
                public sbyte Align4;
                public ushort Buttons;
                public float Forwardmove;
                public int ImpactIndex;
                public Point3D ImpactPosition;
                public sbyte Impulse;
                public int LerpMsec;
                public sbyte Lightlevel;
                public sbyte Msec;
                public float Sidemove;
                public float Upmove;
                public DPoint3D Viewangles;
                public sbyte Weaponselect;

                public override bool Equals(object obj)
                {
                    return obj is UserCmd cmd &&
                           Align1 == cmd.Align1 &&
                           Align2 == cmd.Align2 &&
                           Align3 == cmd.Align3 &&
                           Align4 == cmd.Align4 &&
                           Buttons == cmd.Buttons &&
                           Forwardmove == cmd.Forwardmove &&
                           ImpactIndex == cmd.ImpactIndex &&
                           EqualityComparer<Point3D>.Default.Equals(ImpactPosition, cmd.ImpactPosition) &&
                           Impulse == cmd.Impulse &&
                           LerpMsec == cmd.LerpMsec &&
                           Lightlevel == cmd.Lightlevel &&
                           Msec == cmd.Msec &&
                           Sidemove == cmd.Sidemove &&
                           Upmove == cmd.Upmove &&
                           EqualityComparer<DPoint3D>.Default.Equals(Viewangles, cmd.Viewangles) &&
                           Weaponselect == cmd.Weaponselect;
                }

                public override int GetHashCode()
                {
                    int hashCode = 2037803237;
                    hashCode = hashCode * -1521134295 + Align1.GetHashCode();
                    hashCode = hashCode * -1521134295 + Align2.GetHashCode();
                    hashCode = hashCode * -1521134295 + Align3.GetHashCode();
                    hashCode = hashCode * -1521134295 + Align4.GetHashCode();
                    hashCode = hashCode * -1521134295 + Buttons.GetHashCode();
                    hashCode = hashCode * -1521134295 + Forwardmove.GetHashCode();
                    hashCode = hashCode * -1521134295 + ImpactIndex.GetHashCode();
                    hashCode = hashCode * -1521134295 + ImpactPosition.GetHashCode();
                    hashCode = hashCode * -1521134295 + Impulse.GetHashCode();
                    hashCode = hashCode * -1521134295 + LerpMsec.GetHashCode();
                    hashCode = hashCode * -1521134295 + Lightlevel.GetHashCode();
                    hashCode = hashCode * -1521134295 + Msec.GetHashCode();
                    hashCode = hashCode * -1521134295 + Sidemove.GetHashCode();
                    hashCode = hashCode * -1521134295 + Upmove.GetHashCode();
                    hashCode = hashCode * -1521134295 + Viewangles.GetHashCode();
                    hashCode = hashCode * -1521134295 + Weaponselect.GetHashCode();
                    return hashCode;
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct MoveVars
            {
                public float Accelerate;
                public float Airaccelerate;
                public float Bounce;
                public float Edgefriction;
                public float Entgravity;
                public int Footsteps;
                public float Friction;
                public float Gravity;
                public float Maxspeed;
                public float Maxvelocity;
                public float Rollangle;
                public float Rollspeed;
                public float SkycolorB;
                public float SkycolorG;
                public float SkycolorR;
                public string SkyName;
                public float SkyvecX;
                public float SkyvecY;
                public float SkyvecZ;
                public float Spectatormaxspeed;
                public float Stepsize;
                public float Stopspeed;
                public float Wateraccelerate;
                public float Waterfriction;
                public float WaveHeight;
                public float Zmax;
            }
        }
    }


    /// <summary>
    ///     Aditional demo stats
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Stats
    {
        public int Count;
        public float FrametimeMax;
        public float FrametimeMin;
        public double FrametimeSum;
        public int MsecMax;
        public int MsecMin;
        public long MsecSum;
    }

    /// <summary>
    ///     Info class about normal GoldSource engine demos
    /// </summary>
    public class GoldSourceDemoInfo : DemoInfo
    {
        /// <summary>
        ///     Info about FPS and such
        /// </summary>
        public Stats AditionalStats;

        /// <summary>
        /// Whether there are any non allowed commands in the demo
        /// </summary>
        public List<string> Cheats;

        /// <summary>
        ///     The directory entries of the demo containing the frames
        /// </summary>
        public List<GoldSource.DemoDirectoryEntry> DirectoryEntries;

        /// <summary>
        ///     The header of the demo
        /// </summary>
        public GoldSource.DemoHeader Header;

        /// <summary>
        /// Data included by BunnymodXT into demos.
        /// </summary>
        public List<GoldSource.IncludedBXtData> IncludedBXtData;
    }

    /// <summary>
    ///     Info class about HLSOOE Demos
    /// </summary>
    public class GoldSourceDemoInfoHlsooe : DemoInfo
    {
        /// <summary>
        ///     The directory entries of the demo containing the frames
        /// </summary>
        public List<Hlsooe.DemoDirectoryEntry> DirectoryEntries;

        /// <summary>
        ///     The header of the demo
        /// </summary>
        public Hlsooe.DemoHeader Header;
    }

    /// <summary>
    ///     Contains methods to parse GoldSource engine demo parsing methods
    /// </summary>
    public class GoldSourceParser
    {
        /// <summary>
        ///     This checks if we are will be past the end of the file if we read lengthtocheck
        /// </summary>
        /// <param name="b"></param>
        /// <param name="lengthtocheck"></param>
        /// <returns></returns>
        public static bool UnexpectedEof(BinaryReader b, long lengthtocheck)
        {
            return b.BaseStream.Position + lengthtocheck > b.BaseStream.Length;
        }

        /// <summary>
        /// Convert a string to a byte array which can later be used to write it on disk as a demo detail.
        /// </summary>
        /// <param name="original">The original string.</param>
        /// <param name="length">The length of the byte array we want returned.</param>
        /// <returns>A null terminated string in a byte array which is the specified size.</returns>
        public static byte[] FormatDemoString(string original, int length = 260)
        {
            var nulled = original[original.Length] == '\0'
                ? Encoding.ASCII.GetBytes(original)
                : Encoding.ASCII.GetBytes(original + '\0');
            if (length - nulled.Length > 0) Array.Resize(ref nulled, 260);
            return nulled;
        }

        /// <summary>
        /// Unescapes bytes coded into demos by BXT.
        /// </summary>
        /// <param name="originalBytes">Bytes read from the demo.</param>
        /// <returns>Unescaped bytes.</returns>
        public static byte[] UnescapeGoldSourceBytes(byte[] originalBytes)
        {
            var res = new List<byte>();
            var escapeCharacters = new Dictionary<byte, byte>
            {
                { 0x01, 0x00 },
                {0x02,(byte)'"'},
                {0x03, (byte)'\n'},
                {0x04, (byte)';'},
                {0xFF, 0xFF }
            };
            for (var index = 0; index < originalBytes.Length; index++)
                if (originalBytes[index] == 0xFF)
                {
                    if (originalBytes.Length > index + 1)
                    {
                        res.Add(escapeCharacters[originalBytes[index + 1]]);
                        index++;
                    }
                    else
                    {
                        throw new Exception("Yalter is a dolbaeb! D");
                    }
                }
                else
                {
                    res.Add(originalBytes[index]);
                }

            return res.ToArray();
        }

        /// <summary>
        /// Extract the bytes from the frame included into the frame by BXT and formats it into BXTData
        /// </summary>
        /// <param name="Bytes">The bytes of the console command frame. (No formating needed)</param>
        /// <returns>The parsed data.</returns>
        public static GoldSource.IncludedBXtData FormatBxtData(byte[] bxtdata)
        {
            //File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\extracted\\" + Path.GetRandomFileName() + ".bin",bxtdata);
            var res = new GoldSource.IncludedBXtData { Objects = new List<KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>>() };
            if (bxtdata.Length == 0) return res;

            var decryptedBxtData = new List<uint>();
            if (bxtdata.Count() % 8 != 0) bxtdata = bxtdata.Take(bxtdata.Length - bxtdata.Length % 8).ToArray();

            if (bxtdata.Count() % 8 == 0)
            {
                foreach (var packedbytes in bxtdata.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 8).Select(x => x.Select(v => v.Value).ToList()).ToList()) decryptedBxtData.AddRange(Tea.Decrypt(packedbytes.ToArray()));
                var finalbxtdata = decryptedBxtData.SelectMany(BitConverter.GetBytes).ToArray(); //The final data only needs parsing now.
                using (var br = new BinaryReader(new MemoryStream(finalbxtdata)))
                {
                    var objects = br.ReadUInt32();
                    for (var i = 0; i < objects; i++)
                    {
                        if (UnexpectedEof(br, 1)) return res;

                        var type = (Bxt.RuntimeDataType)br.ReadByte();
                        switch (type)
                        {
                            case Bxt.RuntimeDataType.VERSION_INFO:
                                var vi = new Bxt.VersionInfo();
                                vi.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.VERSION_INFO, vi));
                                break;
                            case Bxt.RuntimeDataType.CVAR_VALUES:
                                var cv = new Bxt.CVarValues();
                                cv.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.CVAR_VALUES, cv));
                                break;
                            case Bxt.RuntimeDataType.TIME:
                                var ti = new Bxt.Time();
                                ti.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.TIME, ti));
                                break;
                            case Bxt.RuntimeDataType.BOUND_COMMAND:
                                var bc = new Bxt.BoundCommand();
                                bc.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.BOUND_COMMAND, bc));
                                break;
                            case Bxt.RuntimeDataType.ALIAS_EXPANSION:
                                var ae = new Bxt.AliasExpansion();
                                ae.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.ALIAS_EXPANSION, ae));
                                break;
                            case Bxt.RuntimeDataType.SCRIPT_EXECUTION:
                                var se = new Bxt.ScriptExecution();
                                se.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.SCRIPT_EXECUTION, se));
                                break;
                            case Bxt.RuntimeDataType.COMMAND_EXECUTION:
                                var ce = new Bxt.CommandExecution();
                                ce.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.COMMAND_EXECUTION, ce));
                                break;
                            case Bxt.RuntimeDataType.GAME_END_MARKER:
                                var ge = new Bxt.GameEndMarker();
                                ge.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.GAME_END_MARKER, ge));
                                break;
                            case Bxt.RuntimeDataType.LOADED_MODULES:
                                var lm = new Bxt.LoadedModules();
                                lm.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.LOADED_MODULES, lm));
                                break;
                            case Bxt.RuntimeDataType.CUSTOM_TRIGGER_COMMAND:
                                var ctc = new Bxt.CustomTriggerCommand();
                                ctc.Read(br);
                                res.Objects.Add(new KeyValuePair<Bxt.RuntimeDataType, Bxt.BXTData>(Bxt.RuntimeDataType.CUSTOM_TRIGGER_COMMAND, ctc));
                                break;
                            default:
                                throw new Exception("Invalid bxt data type!");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Invalid data! (Invalid number of bytes supplied) | TEA");
            }
            return res;
        }

        /// <summary>
        /// Packs the frames,trims and groups the included bytes.
        /// </summary>
        /// <param name="Gsdemo">The demo to get the data from.</param>
        /// <returns>List of data with packed bytes.</returns>
        public static List<byte[]> ParseIncludedBytes(GoldSourceDemoInfo Gsdemo)
        {
            var ret = new List<byte[]>();
            foreach (var entry in Gsdemo.DirectoryEntries)
            {
                ret.AddRange(entry.Frames.Where(y => y.Key.Type == GoldSource.DemoFrameType.ConsoleCommand)
                    .GroupBy(x => x.Key.FrameIndex)
                    .Select(x => x.Where(y => ((GoldSource.ConsoleCommandFrame)(y.Value)).BxtData.Count() > 0))
                    .Select(framegroup => UnescapeGoldSourceBytes((framegroup
                            .SelectMany(y => Tea.TrimBytes(((GoldSource.ConsoleCommandFrame)(y.Value)).BxtData))
                            .ToArray())))
                            .ToArray());
            }
            return ret;
        }

        /// <summary>
        /// Locates //BXTD and if it is present extracts it
        /// </summary>
        /// <param name="bytes">Console command fram data</param>
        /// <returns>Extracted bxt data</returns>
        public static byte[] ExtractIncludedBytes(byte[] bytes)
        {
            byte[] mw = { (byte)'/', (byte)'/', (byte)'B', (byte)'X', (byte)'T', (byte)'D' };
            return bytes.Take(6).SequenceEqual(mw) ? bytes.Skip(7).ToArray() : new byte[0];
        }

        /// <summary>
        ///     Parses a HLS:OOE Demo
        /// </summary>
        /// <param name="s">Path to the file</param>
        /// <returns></returns>
        public static GoldSourceDemoInfoHlsooe ParseDemoHlsooe(string s)
        {
            var hlsooeDemo = new GoldSourceDemoInfoHlsooe
            {
                Header = new Hlsooe.DemoHeader(),
                ParsingErrors = new List<string>(),
                DirectoryEntries = new List<Hlsooe.DemoDirectoryEntry>()
            };
            try
            {
                using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(s))))
                {
                    if (UnexpectedEof(br, 540)) //520 + 12 + 8 = 540 -> IDString size
                    hlsooeDemo.ParsingErrors.Add("Unexpected end of file at the header!");
                    // return hlsooeDemo;
                    var mw = Encoding.ASCII.GetString(br.ReadBytes(8)).Trim('\0').Replace("\0", string.Empty);
                    if (mw == "HLDEMO")
                    {
                        hlsooeDemo.Header.DemoProtocol = br.ReadInt32();
                        hlsooeDemo.Header.NetProtocol = br.ReadInt32();
                        hlsooeDemo.Header.MapName = Encoding.ASCII.GetString(br.ReadBytes(260))
                            .Trim('\0')
                            .Replace("\0", string.Empty);
                        hlsooeDemo.Header.GameDir =
                            Encoding.ASCII.GetString(br.ReadBytes(260)).Trim('\0').Replace("\0", string.Empty);
                        hlsooeDemo.Header.DirectoryOffset = br.ReadInt32();

                        //IDString Parsed... now we read the directory entries
                        br.BaseStream.Seek(hlsooeDemo.Header.DirectoryOffset, SeekOrigin.Begin);
                        if (UnexpectedEof(br, 4))
                        hlsooeDemo.ParsingErrors.Add("Unexpected end of file after the header!");
                        //return hlsooeDemo;
                        var entryCount = br.ReadInt32();
                        for (var i = 0; i < entryCount; i++)
                        {
                            if (UnexpectedEof(br, 20))
                            hlsooeDemo.ParsingErrors.Add("Unexpected end of when reading frames!");
                            // return hlsooeDemo;
                            var tempvar = new Hlsooe.DemoDirectoryEntry
                            {
                                Type = br.ReadInt32(),
                                PlaybackTime = br.ReadSingle(),
                                FrameCount = br.ReadInt32(),
                                Offset = br.ReadInt32(),
                                Filelength = br.ReadInt32(),
                                Frames = new List<Hlsooe.FramesHren2>(),
                                Flags = new Dictionary<Hlsooe.DemoFrame, Hlsooe.ConsoleCommandFrame>()
                            };
                            hlsooeDemo.DirectoryEntries.Add(tempvar);
                        }
                        //Demo directory entries parsed... now we parse the frames.
                        var dirid = -1;
                        foreach (var entry in hlsooeDemo.DirectoryEntries)
                        {
                            dirid++;
                            if (entry.Offset > br.BaseStream.Length)
                            hlsooeDemo.ParsingErrors.Add("Couldn't seek to directoryentry the file is corrupted.");
                            //return hlsooeDemo;
                            br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                            var i = 0;
                            var nextSectionRead = false;
                            while (!nextSectionRead)
                            {
                                if (UnexpectedEof(br, 9))
                                hlsooeDemo.ParsingErrors.Add(
                                    "Failed to read next frame details after frame no.: " + i);
                                //return hlsooeDemo;
                                var currentDemoFrame = new Hlsooe.DemoFrame
                                {
                                    Type = (Hlsooe.DemoFrameType)br.ReadSByte(),
                                    Time = br.ReadSingle(),
                                    Index = br.ReadInt32(),
                                    Frame = i + 1
                                };

                                if (float.IsNaN(currentDemoFrame.Time) || currentDemoFrame.Time == 0.0f)
                                {
                                    br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                                    hlsooeDemo.ParsingErrors.Add(
                                             "Unexpected 'currentDemoFrame.Time' at frame: " +
                                             (i + 1) + ".Directory:" + dirid);
                                    nextSectionRead = true;
                                    continue;
                                }

                                #region FrameType Switch

                                switch (currentDemoFrame.Type)
                                {
                                    case Hlsooe.DemoFrameType.StartupPacket:
                                        if (UnexpectedEof(br, 108))
                                        hlsooeDemo.ParsingErrors.Add("Failed to read startup packet at frame:" +
                                                                     i);
                                        // return hlsooeDemo;
                                        var g = new Hlsooe.StartupPacketFrame
                                        {
                                            Flags = br.ReadInt32(),
                                            ViewOrigins =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewAngles =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            LocalViewAngles =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewOrigin2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewAngles2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            LocalViewAngles2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            IncomingSequence = br.ReadInt32(),
                                            IncomingAcknowledged = br.ReadInt32(),
                                            IncomingReliableAcknowledged = br.ReadInt32(),
                                            IncomingReliableSequence = br.ReadInt32(),
                                            OutgoingSequence = br.ReadInt32(),
                                            ReliableSequence = br.ReadInt32(),
                                            LastReliableSequence = br.ReadInt32()
                                        };
                                        var spml = br.ReadInt32();
                                        g.Msg = Encoding.ASCII.GetString(br.ReadBytes(spml))
                                            .Trim('\0')
                                            .Replace("\0", string.Empty);
                                        entry.Frames.Add(new Hlsooe.FramesHren2( currentDemoFrame, g));
                                        break;
                                    case Hlsooe.DemoFrameType.NetworkPacket:
                                        if (UnexpectedEof(br, 108))
                                        hlsooeDemo.ParsingErrors.Add("Failed to read netmessage at frame: " + i);
                                        // return hlsooeDemo;
                                        var b = new Hlsooe.NetMsgFrame
                                        {
                                            Flags = br.ReadInt32(),
                                            ViewOrigins =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewAngles =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            LocalViewAngles =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewAngles2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            LocalViewAngles2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewOrigin2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            IncomingSequence = br.ReadInt32(),
                                            IncomingAcknowledged = br.ReadInt32(),
                                            IncomingReliableAcknowledged = br.ReadInt32(),
                                            IncomingReliableSequence = br.ReadInt32(),
                                            OutgoingSequence = br.ReadInt32(),
                                            ReliableSequence = br.ReadInt32(),
                                            LastReliableSequence = br.ReadInt32()
                                        };
                                        var nml = br.ReadInt32();
                                        b.Msg = Encoding.ASCII.GetString(br.ReadBytes(nml))
                                            .Trim('\0')
                                            .Replace("\0", string.Empty);
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, b));
                                        break;
                                    case Hlsooe.DemoFrameType.Jumptime:
                                        //No extra stuff
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, new Hlsooe.JumpTimeFrame()));
                                        break;
                                    case Hlsooe.DemoFrameType.ConsoleCommand:
                                        if (UnexpectedEof(br, 4))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected enf of file when reading console command length at frame: " +
                                            i + " brpos: " + br.BaseStream.Position);
                                        //  return hlsooeDemo;
                                        var a = new Hlsooe.ConsoleCommandFrame();
                                        var commandlength = br.ReadInt32();
                                        if (UnexpectedEof(br, commandlength))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading the console command at frame: " +
                                            i + " brpos: " + br.BaseStream.Position);
                                        //  return hlsooeDemo;
                                        a.Command = Encoding.ASCII.GetString(br.ReadBytes(commandlength))
                                            .Trim('\0')
                                            .Replace("\0", string.Empty);
                                        if (a.Command.Contains("#SAVE#")) entry.Flags.Add(currentDemoFrame, a);

                                        if (a.Command.Contains("autosave")) entry.Flags.Add(currentDemoFrame, a);

                                        if (a.Command.Contains("changelevel2")) entry.Flags.Add(currentDemoFrame, a);

                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, a));
                                        break;
                                    case Hlsooe.DemoFrameType.Usercmd:
                                        if (UnexpectedEof(br, 4 + 4 + 2))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading UserCMD header at frame: " + i +
                                            " brpos: " + br.BaseStream.Position);
                                        //  return hlsooeDemo;
                                        var c = new Hlsooe.UserCmdFrame
                                        {
                                            OutgoingSequence = br.ReadInt32(),
                                            Slot = br.ReadInt32()
                                        };
                                        var usercmdlength = br.ReadUInt16();
                                        if (UnexpectedEof(br, usercmdlength))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading userCMD at frame: " + i +
                                            " brpos: " + br.BaseStream.Position);
                                        // return hlsooeDemo;
                                        c.Data =
                                            Encoding.ASCII.GetString(br.ReadBytes(usercmdlength))
                                                .Trim('\0')
                                                .Replace("\0", string.Empty);
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, c));
                                        break;
                                    case Hlsooe.DemoFrameType.Stringtables:
                                        var e = new Hlsooe.StringTablesFrame();
                                        if (UnexpectedEof(br, 4))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading stringtablelength at frame: " +
                                            i + " brpos: " + br.BaseStream.Position);
                                        //  return hlsooeDemo;
                                        var stringtablelength = br.ReadInt32();
                                        if (UnexpectedEof(br, stringtablelength))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading stringtable data at frame: " +
                                            i + " brpos: " + br.BaseStream.Position);
                                        // return hlsooeDemo;
                                        var edata = Encoding.ASCII.GetString(br.ReadBytes(stringtablelength))
                                            .Trim('\0')
                                            .Replace("\0", string.Empty);
                                        e.Data = edata;
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, e));
                                        break;
                                    case Hlsooe.DemoFrameType.NetworkDataTable:
                                        var d = new Hlsooe.NetworkDataTableFrame();
                                        if (UnexpectedEof(br, 4))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading networktable length at frame: " +
                                            i + " brpos: " + br.BaseStream.Position);
                                        //  return hlsooeDemo;
                                        var networktablelength = br.ReadInt32();
                                        if (UnexpectedEof(br, 4))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading NetWorkTable data at frame: " +
                                            i + " brpos: " + br.BaseStream.Position);
                                        // return hlsooeDemo;
                                        d.Data = Encoding.ASCII.GetString(br.ReadBytes(networktablelength))
                                            .Trim('\0')
                                            .Replace("\0", string.Empty);
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, d));
                                        break;
                                    case Hlsooe.DemoFrameType.NextSection:
                                        nextSectionRead = true;
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, new Hlsooe.NextSectionFrame()));
                                        break;
                                    default:
                                        if (UnexpectedEof(br, 108))
                                        hlsooeDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading default frame at frame: " + i +
                                            " brpos: " + br.BaseStream.Position);
                                        //return hlsooeDemo;
                                        var err = new Hlsooe.ErrorFrame
                                        {
                                            Flags = br.ReadInt32(),
                                            ViewOrigins =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewAngles =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            LocalViewAngles =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewOrigin2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            ViewAngles2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            LocalViewAngles2 =
                                                new Point3D(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                            IncomingSequence = br.ReadInt32(),
                                            IncomingAcknowledged = br.ReadInt32(),
                                            IncomingReliableAcknowledged = br.ReadInt32(),
                                            IncomingReliableSequence = br.ReadInt32(),
                                            OutgoingSequence = br.ReadInt32(),
                                            ReliableSequence = br.ReadInt32(),
                                            LastReliableSequence = br.ReadInt32()
                                        };
                                        var dml = br.ReadInt32();
                                        err.Msg = Encoding.ASCII.GetString(br.ReadBytes(dml))
                                            .Trim('\0')
                                            .Replace("\0", string.Empty);
                                        entry.Frames.Add(new Hlsooe.FramesHren2(currentDemoFrame, err));
                                        break;

                                        #endregion
                                }
                            }
                        }
                    }
                    else
                    {
                        hlsooeDemo.ParsingErrors.Add("Non goldsource demo file");
                        br.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Main.//Log("Exception happened at hlsooe parser: " + e.Message);
                hlsooeDemo.ParsingErrors.Add(e.Message);
            }
            return hlsooeDemo;
        }

        /// <summary>
        ///     Parses a goldsource engine demo
        /// </summary>
        /// <param name="s">Path to the file</param>
        /// <returns></returns>
        public static GoldSourceDemoInfo ReadGoldSourceDemo(string s)
        {
            var gDemo = new GoldSourceDemoInfo
            {
                Header = new GoldSource.DemoHeader(),
                ParsingErrors = new List<string>(),
                DirectoryEntries = new List<GoldSource.DemoDirectoryEntry>(),
                Cheats = new List<string>()
            };
            try
            {
                using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(s))))
                {
                    //var s1 = new System.Diagnostics.Stopwatch();
                    //s1.Start();
                    var mw = Encoding.ASCII.GetString(br.ReadBytes(8)).Trim('\0').Replace("\0", string.Empty);
                    if (mw == "HLDEMO")
                    {
                        if (UnexpectedEof(br, 4 + 4 + 260 + 260 + 4))
                        {
                            gDemo.ParsingErrors.Add("Unexpected end of file at the header!");

                            throw new Exception("E1");
                        }
                        // return gDemo;
                        gDemo.Header.DemoProtocol = br.ReadInt32();
                        gDemo.Header.NetProtocol = br.ReadInt32();
                        gDemo.Header.MapName = Encoding.ASCII.GetString(br.ReadBytes(260))
                            .Trim('\0')
                            .Replace("\0", string.Empty);
                        gDemo.Header.GameDir = Encoding.ASCII.GetString(br.ReadBytes(260))
                            .Trim('\0')
                            .Replace("\0", string.Empty);
                        gDemo.Header.MapCrc = br.ReadUInt32();
                        gDemo.Header.DirectoryOffset = br.ReadInt32();
                        //IDString Parsed... now we read the directory entries
                        if (UnexpectedEof(br, gDemo.Header.DirectoryOffset - br.BaseStream.Position))
                        {
                            gDemo.ParsingErrors.Add("Unexpected end of file when seeking to directory offset!");

                        }
                        //   return gDemo;
                        if (gDemo.Header.DirectoryOffset > 0) br.BaseStream.Seek(gDemo.Header.DirectoryOffset, SeekOrigin.Begin);

                        if (UnexpectedEof(br, 4))
                        {
                            gDemo.ParsingErrors.Add("Unexpected end of file when reading entry count!");

                            throw new Exception("E2");
                        }
                        //  return gDemo;
                       
                        //if (entryCount < 1 || entryCount > 5)
                        //{
                        //   // entryCount = 2;
                        //    Console.WriteLine(gDemo.Header.DemoProtocol);
                        //    Console.WriteLine(gDemo.Header.NetProtocol);
                        //    Console.WriteLine(gDemo.Header.MapName);
                        //    Console.WriteLine(gDemo.Header.GameDir);
                        //    Console.WriteLine(gDemo.Header.MapCrc);
                        //    Console.WriteLine(gDemo.Header.DirectoryOffset);
                        //    Console.WriteLine(entryCount);
                        //    Console.WriteLine("Warning!Hacked demo! ");
                        //}

                        if ( gDemo.Header.DirectoryOffset == 0 )
                        {
                            Console.WriteLine("Warning! Bad entries count! Using 'bruteforce' read method!");
                            var tempvar = new GoldSource.DemoDirectoryEntry
                            {
                                Type = 0,
                                Description =
                                       "Playback",
                                Flags = 0,
                                CdTrack = 0,
                                TrackTime = 444.0f,
                                FrameCount = 44444444,
                                Offset = 0 ,
                                FileLength = 0,
                                Frames = new List<GoldSource.FramesHren>()
                            };
                            gDemo.DirectoryEntries.Add(tempvar);
                        }
                        else
                        {
                            var entryCount = br.ReadInt32();

                            for (var i = 0; i < entryCount; i++)
                            {
                                if (UnexpectedEof(br, 4 + 64 + 4 + 4 + 4 + 4 + 4 + 4))
                                {
                                    gDemo.ParsingErrors.Add("Unexpected end of file when reading the directory entries!");

                                    //  throw new Exception("E3");
                                }
                                //return gDemo;
                                var tempvar = new GoldSource.DemoDirectoryEntry
                                {
                                    Type = br.ReadInt32(),
                                    Description =
                                        Encoding.ASCII.GetString(br.ReadBytes(64)).Trim('\0').Replace("\0", string.Empty),
                                    Flags = br.ReadInt32(),
                                    CdTrack = br.ReadInt32(),
                                    TrackTime = br.ReadSingle(),
                                    FrameCount = br.ReadInt32(),
                                    Offset = br.ReadInt32(),
                                    FileLength = br.ReadInt32(),
                                    Frames = new List<GoldSource.FramesHren>()
                                };
                                gDemo.DirectoryEntries.Add(tempvar);
                            }
                        }
                        //s1.Stop();
                        //var s2 = new System.Diagnostics.Stopwatch();
                        //s2.Start();
                        //Demo directory entries parsed... now we parse the frames.

                        var dirid = -1;

                        foreach (var entry in gDemo.DirectoryEntries)
                            try
                            {
                                dirid++;
                                if (entry.Offset != 0)
                                {
                                    if (UnexpectedEof(br, entry.Offset - br.BaseStream.Position))
                                    {
                                        gDemo.ParsingErrors.Add("Unexpected end of file when seeking to directory entry!");
                                        throw new Exception("E4");
                                    }
                                    br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                                }
                                var ind = 0;
                                var nextSectionRead = false;
                                while (!nextSectionRead)
                                {
                                    ind++;
                                    if (UnexpectedEof(br, 1 + 4 + 4))
                                    {
                                        if (entry.Offset != 0)
                                            gDemo.ParsingErrors.Add(
                                            "Unexpected end of file when reading the header of the frame: " + ind + 1);
                                        nextSectionRead = true;
                                        break;
                                    }

                                    var type = br.ReadByte();

                                    //if (firsframe)
                                    //{
                                    //    firsframe = false;
                                    //    if (type < 1 || type > 9)
                                    //    {
                                    //        while(type != 2)
                                    //        {
                                    //            type = br.ReadByte();
                                    //        }
                                    //    }
                                    //}


                                    var currentDemoFrame = new GoldSource.DemoFrame
                                    {
                                        Type = (GoldSource.DemoFrameType)type,
                                        Time = br.ReadSingle(),
                                        FrameIndex = br.ReadInt32(),
                                        Index = ind
                                    };

                                    // Application.DoEvents();



                                    // File.AppendAllText("events.log", currentDemoFrame.Type + "->" + currentDemoFrame.Index + "->" + currentDemoFrame.FrameIndex + "->" + currentDemoFrame.Time + "\n");

                                    #region Frame Switch
                                    //Console.WriteLine(currentDemoFrame.Type);
                                    switch (currentDemoFrame.Type)
                                    {
                                        case GoldSource.DemoFrameType.DemoStart: //No extra dat
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, new GoldSource.DemoStartFrame()));
                                            break;
                                        case GoldSource.DemoFrameType.ConsoleCommand:
                                            var ccframe = new GoldSource.ConsoleCommandFrame();
                                            if (UnexpectedEof(br, 64))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading console command at frame: " +
                                                    ind);

                                                nextSectionRead = true;
                                                break;
                                            }
                                            var cmd = br.ReadBytes(64);
                                            ccframe.Command = Encoding.ASCII.GetString(cmd)
                                                .Trim('\0')
                                                .Replace("\0", string.Empty);
                                            ccframe.BxtData = ExtractIncludedBytes(cmd);
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, ccframe));
                                            break;
                                        case GoldSource.DemoFrameType.ClientData:
                                            var cdframe = new GoldSource.ClientDataFrame();
                                            if (UnexpectedEof(br, 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading clientdataframe at frame: " +
                                                    ind);

                                                nextSectionRead = true;
                                                break;
                                                // return gDemo;
                                            }
                                            cdframe.Origin.X = br.ReadSingle();
                                            cdframe.Origin.Y = br.ReadSingle();
                                            cdframe.Origin.Z = br.ReadSingle();
                                            float tmpfloat = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat = (float)Math.Round(tmpfloat, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {
                                              
                                            }
                                            cdframe.Viewangles.X = tmpfloat;
                                            tmpfloat = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat = (float)Math.Round(tmpfloat, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {
                                                
                                            }
                                            cdframe.Viewangles.Y = tmpfloat;
                                            tmpfloat = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat = (float)Math.Round(tmpfloat, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {
                                                
                                            }
                                            cdframe.Viewangles.Z = tmpfloat;
                                            cdframe.WeaponBits = br.ReadInt32();
                                            cdframe.Fov = br.ReadSingle();
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, cdframe));
                                            break;
                                        case GoldSource.DemoFrameType.NextSection:
                                            if ( entry.Offset != 0 )
                                            nextSectionRead = true;
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, new GoldSource.NextSectionFrame()));
                                            break;
                                        case GoldSource.DemoFrameType.Event:
                                            var eframe = new GoldSource.EventFrame();
                                            if (UnexpectedEof(br, 22 * 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file at when reading eventframe on frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                // return gDemo;
                                            }
                                            eframe.Flags = br.ReadInt32();
                                            eframe.Index = br.ReadInt32();
                                            eframe.Delay = br.ReadSingle();
                                            eframe.EventArguments.Flags = br.ReadInt32();
                                            eframe.EventArguments.EntityIndex = br.ReadInt32();
                                            eframe.EventArguments.Origin.X = br.ReadSingle();
                                            eframe.EventArguments.Origin.Y = br.ReadSingle();
                                            eframe.EventArguments.Origin.Z = br.ReadSingle();

                                            float tmpfloat22 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat22 = (float)Math.Round(tmpfloat22, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }

                                            float tmpfloat222 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat222 = (float)Math.Round(tmpfloat222, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }

                                            float tmpfloat2222 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2222 = (float)Math.Round(tmpfloat2222, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }

                                            eframe.EventArguments.Angles.X = tmpfloat22;
                                            eframe.EventArguments.Angles.Y = tmpfloat222;
                                            eframe.EventArguments.Angles.Z = tmpfloat2222;
                                            eframe.EventArguments.Velocity.X = br.ReadSingle();
                                            eframe.EventArguments.Velocity.Y = br.ReadSingle();
                                            eframe.EventArguments.Velocity.Z = br.ReadSingle();
                                            eframe.EventArguments.Ducking = br.ReadInt32();
                                            eframe.EventArguments.Fparam1 = br.ReadSingle();
                                            eframe.EventArguments.Fparam2 = br.ReadSingle();
                                            eframe.EventArguments.Iparam1 = br.ReadInt32();
                                            eframe.EventArguments.Iparam2 = br.ReadInt32();
                                            eframe.EventArguments.Bparam1 = br.ReadInt32();
                                            eframe.EventArguments.Bparam2 = br.ReadInt32();
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, eframe));
                                            break;
                                        case GoldSource.DemoFrameType.WeaponAnim:
                                            var waframe = new GoldSource.WeaponAnimFrame();
                                            if (UnexpectedEof(br, 4 + 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading weaponanim at frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                //return gDemo;
                                            }
                                            waframe.Anim = br.ReadInt32();
                                            waframe.Body = br.ReadInt32();
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, waframe));
                                            break;
                                        case GoldSource.DemoFrameType.Sound:
                                            var sframe = new GoldSource.SoundFrame();
                                            if (UnexpectedEof(br, 4 + 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading sound channel at frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                //  return gDemo;
                                            }
                                            sframe.Channel = br.ReadInt32();
                                            var samplelength = br.ReadInt32();
                                            if (UnexpectedEof(br, samplelength + 4 + 4 + 4 + 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading sound data at frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                //  return gDemo;
                                            }
                                            sframe.Sample = br.ReadBytes(samplelength);
                                            sframe.Attenuation = br.ReadSingle();
                                            sframe.Volume = br.ReadSingle();
                                            sframe.Flags = br.ReadInt32();
                                            sframe.Pitch = br.ReadInt32();
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, sframe));
                                            break;
                                        case GoldSource.DemoFrameType.DemoBuffer:
                                            var bframe = new GoldSource.DemoBufferFrame();
                                            if (UnexpectedEof(br, 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when demobuffer data at frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                // return gDemo;
                                            }
                                            var buggerlength = br.ReadInt32();
                                            if (UnexpectedEof(br, buggerlength))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading buffer data at frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                // return gDemo;
                                            }
                                            bframe.Buffer = br.ReadBytes(buggerlength);
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, bframe));
                                            break;
                                        case GoldSource.DemoFrameType.NetMsg:
                                        default:
                                            var nf = new GoldSource.NetMsgFrame();
                                            if (UnexpectedEof(br, 468))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when default frame at frame: " + ind);

                                                nextSectionRead = true;
                                                break;
                                                // return gDemo;
                                            }
                                            nf.Timestamp = br.ReadSingle();
                                            nf.RParms.Vieworg.X = br.ReadSingle();
                                            nf.RParms.Vieworg.Y = br.ReadSingle();
                                            nf.RParms.Vieworg.Z = br.ReadSingle();


                                            float tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.RParms.Viewangles.X = tmpfloat2;
                                            
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.RParms.Viewangles.Y = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.RParms.Viewangles.Z = tmpfloat2;
                                            nf.RParms.Forward.X = br.ReadSingle();
                                            nf.RParms.Forward.Y = br.ReadSingle();
                                            nf.RParms.Forward.Z = br.ReadSingle();
                                            nf.RParms.Right.X = br.ReadSingle();
                                            nf.RParms.Right.Y = br.ReadSingle();
                                            nf.RParms.Right.Z = br.ReadSingle();
                                            nf.RParms.Up.X = br.ReadSingle();
                                            nf.RParms.Up.Y = br.ReadSingle();
                                            nf.RParms.Up.Z = br.ReadSingle();
                                            nf.RParms.Frametime = br.ReadSingle();
                                            nf.RParms.Time = br.ReadSingle();
                                            nf.RParms.Intermission = br.ReadInt32();
                                            nf.RParms.Paused = br.ReadInt32();
                                            nf.RParms.Spectator = br.ReadInt32();
                                            nf.RParms.Onground = br.ReadInt32();
                                            nf.RParms.Waterlevel = br.ReadInt32();
                                            nf.RParms.Simvel.X = br.ReadSingle();
                                            nf.RParms.Simvel.Y = br.ReadSingle();
                                            nf.RParms.Simvel.Z = br.ReadSingle();
                                            nf.RParms.Simorg.X = br.ReadSingle();
                                            nf.RParms.Simorg.Y = br.ReadSingle();
                                            nf.RParms.Simorg.Z = br.ReadSingle();
                                            nf.RParms.Viewheight.X = br.ReadSingle();
                                            nf.RParms.Viewheight.Y = br.ReadSingle();
                                            nf.RParms.Viewheight.Z = br.ReadSingle();
                                            nf.RParms.Idealpitch = br.ReadSingle();

                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.RParms.ClViewangles.X = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.RParms.ClViewangles.Y = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.RParms.ClViewangles.Z = tmpfloat2;

                                            nf.RParms.Health = br.ReadInt32();
                                            nf.RParms.Crosshairangle.X = br.ReadSingle();
                                            nf.RParms.Crosshairangle.Y = br.ReadSingle();
                                            nf.RParms.Crosshairangle.Z = br.ReadSingle();
                                            nf.RParms.Viewsize = br.ReadSingle();
                                            nf.RParms.Punchangle.X = br.ReadSingle();
                                            nf.RParms.Punchangle.Y = br.ReadSingle();
                                            nf.RParms.Punchangle.Z = br.ReadSingle();
                                            nf.RParms.Maxclients = br.ReadInt32();
                                            nf.RParms.Viewentity = br.ReadInt32();
                                            nf.RParms.Playernum = br.ReadInt32();
                                            nf.RParms.MaxEntities = br.ReadInt32();
                                            nf.RParms.Demoplayback = br.ReadInt32();
                                            nf.RParms.Hardware = br.ReadInt32();
                                            nf.RParms.Smoothing = br.ReadInt32();
                                            nf.RParms.PtrCmd = br.ReadInt32();
                                            nf.RParms.PtrMovevars = br.ReadInt32();
                                            nf.RParms.Viewport.X = br.ReadInt32();
                                            nf.RParms.Viewport.Y = br.ReadInt32();
                                            nf.RParms.Viewport.Z = br.ReadInt32();
                                            nf.RParms.Viewport.W = br.ReadInt32();
                                            nf.RParms.NextView = br.ReadInt32();
                                            nf.RParms.OnlyClientDraw = br.ReadInt32();
                                            nf.UCmd.LerpMsec = br.ReadInt16();
                                            nf.UCmd.Msec = br.ReadSByte();
                                            nf.UCmd.Align1 = br.ReadSByte();

                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.UCmd.Viewangles.X = tmpfloat2;
                                            
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.UCmd.Viewangles.Y = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            try
                                            {
                                                tmpfloat2 = (float)Math.Round(tmpfloat2, 8, MidpointRounding.ToEven);
                                            }
                                            catch
                                            {

                                            }
                                            nf.UCmd.Viewangles.Z = tmpfloat2;


                                            nf.UCmd.Forwardmove = br.ReadSingle();
                                            nf.UCmd.Sidemove = br.ReadSingle();
                                            nf.UCmd.Upmove = br.ReadSingle();
                                            nf.UCmd.Lightlevel = br.ReadSByte();
                                            nf.UCmd.Align2 = br.ReadSByte();
                                            nf.UCmd.Buttons = br.ReadUInt16();
                                            nf.UCmd.Impulse = br.ReadSByte();
                                            nf.UCmd.Weaponselect = br.ReadSByte();
                                            nf.UCmd.Align3 = br.ReadSByte();
                                            nf.UCmd.Align4 = br.ReadSByte();
                                            nf.UCmd.ImpactIndex = br.ReadInt32();
                                            nf.UCmd.ImpactPosition.X = br.ReadSingle();
                                            nf.UCmd.ImpactPosition.Y = br.ReadSingle();
                                            nf.UCmd.ImpactPosition.Z = br.ReadSingle();
                                            nf.MVars.Gravity = br.ReadSingle();
                                            nf.MVars.Stopspeed = br.ReadSingle();
                                            nf.MVars.Maxspeed = br.ReadSingle();
                                            nf.MVars.Spectatormaxspeed = br.ReadSingle();
                                            nf.MVars.Accelerate = br.ReadSingle();
                                            nf.MVars.Airaccelerate = br.ReadSingle();
                                            nf.MVars.Wateraccelerate = br.ReadSingle();
                                            nf.MVars.Friction = br.ReadSingle();
                                            nf.MVars.Edgefriction = br.ReadSingle();
                                            nf.MVars.Waterfriction = br.ReadSingle();
                                            nf.MVars.Entgravity = br.ReadSingle();
                                            nf.MVars.Bounce = br.ReadSingle();
                                            nf.MVars.Stepsize = br.ReadSingle();
                                            nf.MVars.Maxvelocity = br.ReadSingle();
                                            nf.MVars.Zmax = br.ReadSingle();
                                            nf.MVars.WaveHeight = br.ReadSingle();
                                            nf.MVars.Footsteps = br.ReadInt32();
                                            nf.MVars.SkyName = Encoding
                                                .ASCII
                                                .GetString(br.ReadBytes(32))
                                                .Trim('\0')
                                                .Replace("\0", string.Empty);
                                            nf.MVars.Rollangle = br.ReadSingle();
                                            nf.MVars.Rollspeed = br.ReadSingle();
                                            nf.MVars.SkycolorR = br.ReadSingle();
                                            nf.MVars.SkycolorG = br.ReadSingle();
                                            nf.MVars.SkycolorB = br.ReadSingle();
                                            nf.MVars.SkyvecX = br.ReadSingle();
                                            nf.MVars.SkyvecY = br.ReadSingle();
                                            nf.MVars.SkyvecZ = br.ReadSingle();

                                            nf.View.X = br.ReadSingle();
                                            nf.View.Y = br.ReadSingle();
                                            nf.View.Z = br.ReadSingle();
                                            nf.Viewmodel = br.ReadInt32();

                                            nf.IncomingSequence = br.ReadInt32();
                                            nf.IncomingAcknowledged = br.ReadInt32();
                                            nf.IncomingReliableAcknowledged = br.ReadInt32();
                                            nf.IncomingReliableSequence = br.ReadInt32();
                                            nf.OutgoingSequence = br.ReadInt32();
                                            nf.ReliableSequence = br.ReadInt32();
                                            nf.LastReliableSequence = br.ReadInt32();
                                            var msglength = br.ReadInt32();
                                            if (msglength < 0 || UnexpectedEof(br, msglength))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when default frame message at frame: " + ind);
                                                throw new Exception("E15");
                                            }
                                            nf.MsgBytes = br.ReadBytes(msglength);
                                            if (nf.MsgBytes.Length != msglength)
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Cant read all game data from" + ind);
                                            }
                                            else
                                            {
                                                
                                            }
                                            nf.Msg = ByteArrayToString(nf.MsgBytes);
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, nf));
                                            break;

                                    }

                                    #endregion
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Fatal error:" + ex.Message);
                            }

                        //here
                        var first = true;
                        foreach (var f in from entry in gDemo.DirectoryEntries
                                          from frame in entry.Frames
                                          where (int)frame.Key.Type < 2 || (int)frame.Key.Type > 9
                                          select (GoldSource.NetMsgFrame)frame.Value)
                        {
                            gDemo.AditionalStats.FrametimeSum += f.RParms.Frametime;
                            gDemo.AditionalStats.MsecSum += f.UCmd.Msec;
                            gDemo.AditionalStats.Count++;

                            if (first)
                            {
                                first = false;
                                gDemo.AditionalStats.FrametimeMin = int.MaxValue;
                                gDemo.AditionalStats.FrametimeMax = int.MinValue;
                                gDemo.AditionalStats.MsecMin = int.MaxValue;
                                gDemo.AditionalStats.MsecMax = int.MinValue;
                            }
                            else
                            {
                                if (f.RParms.Frametime > 0) gDemo.AditionalStats.FrametimeMin = Math.Min(gDemo.AditionalStats.FrametimeMin, f.RParms.Frametime);
                                //Console.WriteLine(f.RParms.Frametime);
                                gDemo.AditionalStats.FrametimeMax = Math.Max(gDemo.AditionalStats.FrametimeMax, f.RParms.Frametime);
                                if (f.UCmd.Msec > 0) gDemo.AditionalStats.MsecMin = Math.Min(gDemo.AditionalStats.MsecMin, f.UCmd.Msec);

                                gDemo.AditionalStats.MsecMax = Math.Max(gDemo.AditionalStats.MsecMax, f.UCmd.Msec);
                            }
                        }
                        //s2.Stop();
                        //var s3 = new System.Diagnostics.Stopwatch();
                        //s3.Start();
                        gDemo.IncludedBXtData = ParseIncludedBytes(gDemo).Select(FormatBxtData).ToList();
                        //s3.Stop();
                        /*MessageBox.Show($@"Parsing header: {s1.ElapsedMilliseconds}ms
Parsing frames:{s2.ElapsedMilliseconds}ms
Parsing bxt data:{s3.ElapsedMilliseconds}ms");*/
                    }
                    else
                    {
                        gDemo.ParsingErrors.Add("Non goldsource demo file");
                        br.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //This can't actually happen but I might just log it just incase.
                //Main.//Log("Exception happened in the goldsource parser: " + e.Message);
                gDemo.ParsingErrors.Add(e.Message);
            }
            return gDemo;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }
    }
}
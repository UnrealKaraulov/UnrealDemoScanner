using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DemoScanner.DemoStuff.GoldSource
{
    /// <summary>
    ///     Types for HLSOOE
    /// </summary>
    public class Hlsooe
    {
        public enum DemoFrameType : sbyte
        {
            None = 0,
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
        { }

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
        { }

        public class NextSectionFrame : IFrame
        { }

        public class StartupPacketFrame : NetMsgFrame
        { }

        public class ErrorFrame : NetMsgFrame
        { }

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
            public FPoint3D LocalViewAngles;
            public FPoint3D LocalViewAngles2;
            public string Msg;
            public int OutgoingSequence;
            public int ReliableSequence;
            public FPoint3D ViewAngles;
            public FPoint3D ViewAngles2;
            public FPoint3D ViewOrigin2;
            public FPoint3D ViewOrigins;
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
        [Flags]
        public enum UCMD_BUTTONS : ushort
        {
            IN_ATTACK = 1 << 0,
            IN_JUMP = 1 << 1,
            IN_DUCK = 1 << 2,
            IN_FORWARD = 1 << 3,
            IN_BACK = 1 << 4,
            IN_USE = 1 << 5,
            IN_CANCEL = 1 << 6,
            IN_LEFT = 1 << 7,
            IN_RIGHT = 1 << 8,
            IN_MOVELEFT = 1 << 9,
            IN_MOVERIGHT = 1 << 10,
            IN_ATTACK2 = 1 << 11,
            IN_RUN = 1 << 12,
            IN_RELOAD = 1 << 13,
            IN_ALT1 = 1 << 14,
            IN_SCORE = 1 << 15,
        };

        public enum DemoFrameType
        {
            None = 0,
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
        { }

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
            public FPoint3D Origin;
            public FPoint3D Viewangles;
            public int WeaponBits;
            public int ErrAngles;
        }

        // NEXT_SECTION: no extra data.

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EventFrame : IFrame
        {
            public float Delay;
            public EventParams EventArguments;
            public int Flags;
            public int Index;

            public struct EventParams
            {
                public FPoint3D Angles;
                public int Bparam1;
                public int Bparam2;
                public int Ducking;
                public int EntityIndex;
                public int Flags;
                public float Fparam1;
                public float Fparam2;
                public int Iparam1;
                public int Iparam2;
                public FPoint3D Origin;
                public FPoint3D Velocity;
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
            public string Sample;
            public float Volume;
        }

        public struct DemoBufferFrame : IFrame { public List<byte> Buffer; }

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
            public FPoint3D View;
            public int Viewmodel;

            public override bool Equals(object obj)
            {
                return obj is NetMsgFrame frame &&
                       EqualityComparer<RefParams>.Default.Equals(RParms, frame.RParms) &&
                       EqualityComparer<UserCmd>.Default.Equals(UCmd, frame.UCmd) &&
                       EqualityComparer<FPoint3D>.Default.Equals(View, frame.View);
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
                public FPoint3D ClViewangles;
                public FPoint3D Crosshairangle;
                public int Demoplayback;
                public FPoint3D Forward;
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
                public FPoint3D Punchangle;
                public FPoint3D Right;
                public FPoint3D Simorg;
                public FPoint3D Simvel;
                public int Smoothing;
                public int Spectator;
                public float Time;
                public FPoint3D Up;
                public FPoint3D Viewangles;
                public int Viewentity;
                public FPoint3D Viewheight;
                public FPoint3D Vieworg;
                public Point4D Viewport;
                public float Viewsize;
                public int Waterlevel;

                public static bool operator ==(RefParams left, RefParams right)
                {
                    return left.Equals(right);
                }

                public static bool operator !=(RefParams left, RefParams right)
                {
                    return !(left == right);
                }

                public override bool Equals(object obj)
                {
                    return obj is RefParams @params &&
                           EqualityComparer<FPoint3D>.Default.Equals(ClViewangles, @params.ClViewangles) &&
                           EqualityComparer<FPoint3D>.Default.Equals(Crosshairangle, @params.Crosshairangle) &&
                           Demoplayback == @params.Demoplayback &&
                           EqualityComparer<FPoint3D>.Default.Equals(Forward, @params.Forward) &&
                           Hardware == @params.Hardware &&
                           Health == @params.Health &&
                           Math.Abs(Idealpitch - @params.Idealpitch) < 0.00001f &&
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
                           EqualityComparer<FPoint3D>.Default.Equals(Punchangle, @params.Punchangle) &&
                           EqualityComparer<FPoint3D>.Default.Equals(Right, @params.Right) &&
                           EqualityComparer<FPoint3D>.Default.Equals(Simorg, @params.Simorg) &&
                           EqualityComparer<FPoint3D>.Default.Equals(Simvel, @params.Simvel) &&
                           Smoothing == @params.Smoothing &&
                           Spectator == @params.Spectator &&
                            Math.Abs(Time - @params.Time) < 0.00001f &&
                           EqualityComparer<FPoint3D>.Default.Equals(Up, @params.Up) &&
                           EqualityComparer<FPoint3D>.Default.Equals(Viewangles, @params.Viewangles) &&
                           Viewentity == @params.Viewentity &&
                           EqualityComparer<FPoint3D>.Default.Equals(Viewheight, @params.Viewheight) &&
                           EqualityComparer<FPoint3D>.Default.Equals(Vieworg, @params.Vieworg) &&
                           EqualityComparer<Point4D>.Default.Equals(Viewport, @params.Viewport) &&
                           Math.Abs(Viewsize - @params.Viewsize) < 0.00001f &&
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
                public UCMD_BUTTONS Buttons;
                public float Forwardmove;
                public int ImpactIndex;
                public FPoint3D ImpactPosition;
                public sbyte Impulse;
                public int LerpMsec;
                public byte Lightlevel;
                public byte Msec;
                public float Sidemove;
                public float Upmove;
                public FPoint3D Viewangles;
                public sbyte Weaponselect;

                public override bool Equals(object obj)
                {
                    return obj is UserCmd cmd &&
                           Align1 == cmd.Align1 &&
                           Align2 == cmd.Align2 &&
                           Align3 == cmd.Align3 &&
                           Align4 == cmd.Align4 &&
                           Buttons == cmd.Buttons &&
                           Math.Abs(Forwardmove - cmd.Forwardmove) < 0.00001f &&
                           ImpactIndex == cmd.ImpactIndex &&
                           EqualityComparer<FPoint3D>.Default.Equals(ImpactPosition, cmd.ImpactPosition) &&
                           Impulse == cmd.Impulse &&
                           LerpMsec == cmd.LerpMsec &&
                           Lightlevel == cmd.Lightlevel &&
                           Msec == cmd.Msec &&
                           Math.Abs(Sidemove - cmd.Sidemove) < 0.00001f &&
                           Math.Abs(Upmove - cmd.Upmove) < 0.00001f &&
                           EqualityComparer<FPoint3D>.Default.Equals(Viewangles, cmd.Viewangles) &&
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
    ///     Info class about normal GoldSource engine demos
    /// </summary>
    public class GoldSourceDemoInfo : DemoInfo
    {
        /// <summary>
        ///     The directory entries of the demo containing the frames
        /// </summary>
        public List<GoldSource.DemoDirectoryEntry> DirectoryEntries;

        /// <summary>
        ///     The header of the demo
        /// </summary>
        public GoldSource.DemoHeader Header;
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
            {
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
            }

            return res.ToArray();
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
                DirectoryEntries = new List<GoldSource.DemoDirectoryEntry>()
            };
            try
            {
                using (var br = new BinaryReader(new MemoryStream(File.ReadAllBytes(s))))
                {
                    //var s1 = new System.Diagnostics.Stopwatch();
                    //s1.Start();
                    var mw = Encoding.ASCII.GetString(br.ReadBytes(8)).Trim('\0').Replace("\0", string.Empty);

                    if (DemoScanner.DG.DemoScanner.DEBUG_ENABLED)
                        Console.WriteLine("DEMO HEADER: \"" + mw + "\" = " + (mw == "HLDEMO").ToString());

                    if (mw == "HLDEMO")
                    {
                        if (UnexpectedEof(br, 4 + 4 + 260 + 260 + 4))
                        {
                            gDemo.ParsingErrors.Add("Unexpected end of file at the header!");
                            throw new Exception("E1");
                        }
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

                        if (DemoScanner.DG.DemoScanner.DEBUG_ENABLED)
                        {
                            Console.WriteLine("DEMO PROTOCOL: " + gDemo.Header.DemoProtocol + " / " + gDemo.Header.NetProtocol);
                            Console.WriteLine("DEMO MAP, DIR, CRC, DIR OFFSET: \"" + gDemo.Header.MapName + "\" , \""
                               + gDemo.Header.GameDir + "\" , " + (int)gDemo.Header.MapCrc + " , " + gDemo.Header.DirectoryOffset.ToString("x2"));
                        }

                        long fileMark = br.BaseStream.Position;
                        int entryCount = 0;

                        try
                        {
                            br.BaseStream.Seek(gDemo.Header.DirectoryOffset, SeekOrigin.Begin);
                            if (gDemo.Header.DirectoryOffset < 0 || gDemo.Header.DirectoryOffset >= br.BaseStream.Length)
                            {
                                gDemo.ParsingErrors.Add("Unexpected directory offset.");
                                throw new Exception("Bad directory offset.");
                            }
                        }
                        catch
                        {
                            gDemo.ParsingErrors.Add("Error while seeking to directory offset");
                            gDemo.Header.DirectoryOffset = 0;
                        }

                        if (!UnexpectedEof(br, 4))
                        {
                            entryCount = br.ReadInt32();
                            if (entryCount > 0 && entryCount <= 1024)
                            {

                            }
                            else
                            {
                                br.BaseStream.Seek(fileMark, SeekOrigin.Begin);
                                entryCount = 0;
                                gDemo.Header.DirectoryOffset = 0;
                            }
                        }
                        else
                        {
                            entryCount = 0;
                            br.BaseStream.Seek(fileMark, SeekOrigin.Begin);
                            gDemo.Header.DirectoryOffset = 0;
                        }

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

                        if (entryCount > 0)
                        {
                            for (var i = 0; i < entryCount; i++)
                            {
                                if (UnexpectedEof(br, 4 + 64 + 4 + 4 + 4 + 4 + 4 + 4))
                                {
                                    gDemo.ParsingErrors.Add("Unexpected end of file when reading the directory entries! (" + i + ") of " + entryCount);
                                    break;
                                }
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

                                if (DemoScanner.DG.DemoScanner.DEBUG_ENABLED)
                                {
                                    Console.WriteLine("Entry type: " + tempvar.Type + ". Description:" + tempvar.Description + ". Flags:" + tempvar.Flags);
                                    Console.WriteLine("CdTrack: " + tempvar.CdTrack + ". TrackTime:" + tempvar.TrackTime + ". FrameCount:" + tempvar.FrameCount);
                                    Console.WriteLine("Offset: " + tempvar.Offset + ". FileLength:" + tempvar.FileLength);
                                }

                                gDemo.DirectoryEntries.Add(tempvar);
                            }
                        }
                        else
                        {
                            //Console.WriteLine("Warning! Bad entries count! Using 'bruteforce' read method!");
                            var tempvar = new GoldSource.DemoDirectoryEntry
                            {
                                Type = 0,
                                Description =
                                       "Playback",
                                Flags = 0,
                                CdTrack = 0,
                                TrackTime = 0.0f,
                                FrameCount = 0,
                                Offset = 0,
                                FileLength = 0,
                                Frames = new List<GoldSource.FramesHren>()
                            };
                            gDemo.DirectoryEntries.Add(tempvar);
                            br.BaseStream.Seek(fileMark, SeekOrigin.Begin);
                        }

                        int directory_error_counter = 10;

                        foreach (var entry in gDemo.DirectoryEntries)
                        {
                            try
                            {
                                if (entry.Offset != 0)
                                {
                                    if (UnexpectedEof(br, entry.Offset - br.BaseStream.Position))
                                    {
                                        gDemo.ParsingErrors.Add("Unexpected end of file when seeking to directory entry " + entry + "!");
                                        if (directory_error_counter-- > 0)
                                            continue;
                                        else
                                            break;
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
                                            ccframe.Command = Encoding.ASCII.GetString(cmd).Trim('\0').Replace("\0", string.Empty);
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
                                            }
                                            cdframe.Origin.X = br.ReadSingle();
                                            cdframe.Origin.Y = br.ReadSingle();
                                            cdframe.Origin.Z = br.ReadSingle();
                                            float tmpfloat = br.ReadSingle();
                                            cdframe.Viewangles.X = tmpfloat;
                                            tmpfloat = br.ReadSingle();
                                            cdframe.Viewangles.Y = tmpfloat;
                                            tmpfloat = br.ReadSingle();
                                            cdframe.Viewangles.Z = tmpfloat;
                                            cdframe.WeaponBits = br.ReadInt32();
                                            cdframe.Fov = br.ReadSingle();
                                            entry.Frames.Add(new GoldSource.FramesHren(currentDemoFrame, cdframe));
                                            break;
                                        case GoldSource.DemoFrameType.NextSection:
                                            if (entry.Offset != 0)
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

                                            float tmpfloat222 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;

                                            float tmpfloat2222 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;

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
                                            }
                                            sframe.Channel = br.ReadInt32();
                                            var samplelength = br.ReadInt32();
                                            if (UnexpectedEof(br, samplelength + 4 + 4 + 4 + 4))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading sound data at frame: " + ind);
                                                nextSectionRead = true;
                                                break;
                                            }
                                            try
                                            {
                                                sframe.Sample = Encoding.UTF8.GetString(br.ReadBytes(samplelength));
                                            }
                                            catch
                                            {
                                                sframe.Sample = "UNKNOWN SOUND";
                                            }
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
                                            }
                                            var buggerlength = br.ReadInt32();
                                            if (UnexpectedEof(br, buggerlength))
                                            {
                                                gDemo.ParsingErrors.Add(
                                                    "Unexpected end of file when reading buffer data at frame: " + ind);
                                                nextSectionRead = true;
                                                break;
                                            }
                                            bframe.Buffer = new List<byte>();
                                            if (buggerlength > 0)
                                            {
                                                bframe.Buffer.AddRange(br.ReadBytes(buggerlength));
                                            }
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
                                            }
                                            nf.Timestamp = br.ReadSingle();
                                            nf.RParms.Vieworg.X = br.ReadSingle();
                                            nf.RParms.Vieworg.Y = br.ReadSingle();
                                            nf.RParms.Vieworg.Z = br.ReadSingle();


                                            float tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            nf.RParms.Viewangles.X = tmpfloat2;

                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            nf.RParms.Viewangles.Y = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
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
                                            nf.RParms.ClViewangles.X = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            nf.RParms.ClViewangles.Y = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
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
                                            nf.UCmd.Msec = br.ReadByte();
                                            nf.UCmd.Align1 = br.ReadSByte();

                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            nf.UCmd.Viewangles.X = tmpfloat2;

                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            nf.UCmd.Viewangles.Y = tmpfloat2;
                                            tmpfloat2 = br.ReadSingle();
                                            //if (tmpfloat.ToString().IndexOf("E") > 0)
                                            //    cdframe.ErrAngles++;
                                            nf.UCmd.Viewangles.Z = tmpfloat2;


                                            nf.UCmd.Forwardmove = br.ReadSingle();
                                            nf.UCmd.Sidemove = br.ReadSingle();
                                            nf.UCmd.Upmove = br.ReadSingle();
                                            nf.UCmd.Lightlevel = br.ReadByte();
                                            nf.UCmd.Align2 = br.ReadSByte();
                                            nf.UCmd.Buttons = (GoldSource.UCMD_BUTTONS)br.ReadUInt16();
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
                                                nextSectionRead = true;
                                                break;
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
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Fatal error:" + ex.Message);
                            }
                        }
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
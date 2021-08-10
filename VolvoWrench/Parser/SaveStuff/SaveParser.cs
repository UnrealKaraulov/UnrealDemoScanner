using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using VolvoWrench.ExtensionMethods;

#pragma warning disable 1591


/*  NOTES
 *  SaveGameState is .hl1
 *  ClientState is .hl2
 *  EntityPatch is .hl3
 *  TODO: figure out this:https://github.com/LestaD/SourceEngine2007/blob/43a5c90a5ada1e69ca044595383be67f40b33c61/src_main/engine/host_saverestore.cpp#L1399
 *  Note we need different parsers for .hl? files so the enum is indeed necesarry
 *  Main save load method: https://github.com/LestaD/SourceEngine2007/blob/43a5c90a5ada1e69ca044595383be67f40b33c61/se2007/engine/host_state.cpp#L148
 *  
 * 
 * 
 */


namespace VolvoWrench.SaveStuff
{
    public class Flag
    {
        public Flag(int t, float s, string type)
        {
            Ticks = t.ToString();
            Time = s.ToString(CultureInfo.InvariantCulture) + "s";
            Type = type;
        }

        public string Ticks { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
    }

    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Listsave
    {
        [Serializable]
        public enum Hlfile
        {
            Hl1,
            Hl2,
            Hl3
        }

        public static string Chaptername(int chapter)
        {
            #region MapSwitch

            switch (chapter)
            {
                case 0:
                    return "Point Insertion";
                case 1:
                    return "A Red Letter Day";
                case 2:
                    return "Route Kanal";
                case 3:
                    return "Water Hazard";
                case 4:
                    return "Black Mesa East";
                case 5:
                    return "We don't go to Ravenholm";
                case 6:
                    return "Highway 17";
                case 7:
                    return "Sandtraps";
                case 8:
                    return "Nova Prospekt";
                case 9:
                    return "Entanglement";
                case 10:
                    return "Anticitizen One";
                case 11:
                    return "Follow Freeman!";
                case 12:
                    return "Our Benefactors";
                case 13:
                    return "Dark Energy";
                default:
                    return "Mod/Unknown";
            }

            #endregion
        }

        public static SaveFile ParseSaveFile(string file)
        {
            var result = new SaveFile();
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    result.FileName = Path.GetFileName(file);
                    result.Files = new List<StateFileInfo>();
                    result.IDString = Encoding.ASCII.GetString(br.ReadBytes(sizeof(int)));
                    result.SaveVersion = br.ReadInt32();
                    result.TokenTableFileTableOffset = br.ReadInt32();
                    result.TokenCount = br.ReadInt32();
                    result.TokenTableSize = br.ReadInt32();
                    br.BaseStream.Seek(result.TokenTableSize + result.TokenTableFileTableOffset, SeekOrigin.Current);
                    var endoffile = false;
                    var check = br.ReadBytes(4);
                    br.BaseStream.Seek(-4, SeekOrigin.Current);
                    if (check.Any(b => b == 0))
                    {
                        var filenum = br.ReadInt32();
                    }

                    while (!endoffile && result.SaveVersion <= 116)
                        if (UnexpectedEof(br, 260))
                        {
                            var tempvalv = new StateFileInfo
                            {
                                Data = new byte[0],
                                FileName = Encoding.ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0').Replace("\0", "")
                                //BUG: bunch of \0 in string
                            };
                            if (UnexpectedEof(br, 8))
                            {
                                var filelength = br.ReadInt32();
                                tempvalv.MagicWord = Encoding.ASCII.GetString(br.ReadBytes(4))
                                    .Trim('\0')
                                    .Replace("\0", string.Empty);
                                br.BaseStream.Seek(-4, SeekOrigin.Current);
                                if (UnexpectedEof(br, 8) && filelength > 0)
                                    tempvalv.Data = br.ReadBytes(filelength);
                                else
                                    endoffile = true;
                            }
                            else
                            {
                                endoffile = true;
                            }

                            result.Files.Add(tempvalv);
                        }
                        else
                        {
                            endoffile = true;
                        }

                    for (var i = 0; i < result.Files.Count; i++)
                        result.Files[i] = ParseStateFile(result.Files[i]);
                    result.Map = result.Files.Last().FileName;

                }
            }
            return result;
        }

        public static bool SaveReadNameAndComment(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    const uint tagsize = sizeof(int) * 5;
                    var pTokenList = new List<string>();
                    if (br.BaseStream.Length - br.BaseStream.Position < tagsize) return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     A Method to Parser .hl1 files
        /// </summary>
        /// <param name="stateFile">A .hl1 statefile</param>
        /// <returns></returns>
        public static StateFileInfo ParseStateFile(StateFileInfo stateFile)
        {
            //if (stateFile.Data.Length < 16)
            //return stateFile;
            //using (var br = new BinaryReader(new MemoryStream(stateFile.Data)))
            //{
            //    var si = new SaveFileSectionsInfo_t();
            //    if (!UnexpectedEof(br, 20)) return stateFile;

            //    stateFile.MagicWord = br.ReadString(4);
            //    stateFile.Version = br.ReadByte();
            //    si.nBytesSymbols = Math.Abs(br.ReadInt32());
            //    si.nSymbols = Math.Abs(br.ReadInt32());
            //    si.nBytesDataHeaders = Math.Abs(br.ReadInt32());
            //    si.nBytesData = Math.Abs(br.ReadInt32());
            //    if (!UnexpectedEof(br, si.nSymbols + si.nBytesDataHeaders + si.nBytesData)) return stateFile;

            //    stateFile.pSymbols = br.ReadBytes(si.nSymbols);
            //    stateFile.pDataHeaders = br.ReadBytes(si.nBytesDataHeaders);
            //    stateFile.pData = br.ReadBytes(si.nBytesData);


            //    var NumberOfFields = 8;
            //    for (var i = 0; i < NumberOfFields; i++)
            //    {
            //        var size = br.ReadInt16();
            //        var index = br.ReadInt16();
            //        var data = br.ReadBytes(size);
            //    }
            //}

            return stateFile;
        }

        /// <summary>
        ///     Parses a .hl2 statefile which contains the client state
        /// </summary>
        /// <param name="stateFile"></param>
        /// <returns></returns>
        public static ClientState ParseClienState(StateFileInfo stateFile)
        {
            using (var br = new BinaryReader(new MemoryStream(stateFile.Data)))
            {
                var cs = new ClientState
                {
                    IdString = br.ReadBytes(4)
                };
                var savepos = br.BaseStream.Position;
                cs.Magicnumber = br.ReadUInt32();
                if (cs.Magicnumber == SECTION_MAGIC_NUMBER)
                {
                    cs.Baseclientsections.entitysize = br.ReadInt32();
                    cs.Baseclientsections.headersize = br.ReadInt32();
                    cs.Baseclientsections.decalsize = br.ReadInt32();
                    cs.Baseclientsections.musicsize = br.ReadInt32();
                    cs.Baseclientsections.symbolsize = br.ReadInt32();

                    cs.Baseclientsections.decalcount = br.ReadInt32();
                    cs.Baseclientsections.musiccount = br.ReadInt32();
                    cs.Baseclientsections.symbolcount = br.ReadInt32();
                }
                else
                {
                    br.BaseStream.Seek(savepos, SeekOrigin.Begin);
                    cs.Baseclientsectionsold.entitysize = br.ReadInt32();
                    cs.Baseclientsectionsold.headersize = br.ReadInt32();
                    cs.Baseclientsectionsold.decalsize = br.ReadInt32();
                    cs.Baseclientsectionsold.symbolsize = br.ReadInt32();
                    cs.Baseclientsectionsold.decalcount = br.ReadInt32();
                    cs.Baseclientsectionsold.symbolcount = br.ReadInt32();

                    cs.Baseclientsections.entitysize = cs.Baseclientsectionsold.entitysize;
                    cs.Baseclientsections.headersize = cs.Baseclientsectionsold.headersize;
                    cs.Baseclientsections.decalsize = cs.Baseclientsectionsold.decalsize;
                    cs.Baseclientsections.symbolsize = cs.Baseclientsectionsold.symbolsize;

                    cs.Baseclientsections.decalcount = cs.Baseclientsectionsold.decalcount;
                    cs.Baseclientsections.symbolcount = cs.Baseclientsectionsold.symbolcount;
                }

                var pszTokenList = br.ReadBytes(cs.Baseclientsections.SumBytes());


                return cs;
            }
        }


        /// <summary>
        ///     This parses .hl3 files
        /// </summary>
        /// <param name="stateFile">The .hl3 file</param>
        /// <returns>Item1 is the size.Item2 are the entity ids which get FENTTABLE_REMOVED flag.</returns>
        public static Tuple<int, int[]> ParseEntityPatch(StateFileInfo stateFile)
        {
            using (var br = new BinaryReader(new MemoryStream(stateFile.Data)))
            {
                var entityIds = new List<int>();
                var size = br.ReadInt32();
                for (var i = 0; i < size; i++)
                    entityIds.Add(br.ReadInt32());
                //This id gets the FENTTABLE_REMOVED flag
                return new Tuple<int, int[]>(size, entityIds.ToArray());
            }
        }

        public static uint rotr(uint val, int shift)
        {
            var num = val;
            shift &= 0x1f;
            while (Convert.ToBoolean(shift--))
            {
                var lobit = num & 1;
                num >>= 1;
                if (Convert.ToBoolean(lobit)) num |= 0x80000000;
            }

            return num;
        }

        /// <summary>
        ///     Checks if the length the binaryreader is trying to read will be over the end of the file
        /// </summary>
        /// <param name="b"></param>
        /// <param name="lengthtocheck"></param>
        /// <returns></returns>
        public static bool UnexpectedEof(BinaryReader b, int lengthtocheck)
        {
            return b.BaseStream.Position + lengthtocheck < b.BaseStream.Length;
        }

        [Serializable]
        public class SaveFile
        {
            [Category("File")]
            [Description("The name of the file.")]
            public string FileName { get; set; }

            [Category("File")]
            [Description("The header or magic word of the file which identifies it. Should be ('J','S','A','V')")]
            public string IDString { get; set; }

            [Category("File")]
            [Description(
                "The version of the save files. This is for some reason 115 for nearly any save file. The people at valve forgot to change it for some reason probably.")]
            public int SaveVersion { get; set; }

            [Category("File")]
            [Description(
                "The map the save was made on. This is the filename of the last statefile since that is the last one, that is the one the save was made on.")]
            public string Map { get; set; }

            [Category("Tokentable details")]
            [Description("The byte offset from the begining to the end of the File table.")]
            public int TokenTableFileTableOffset { get; set; }

            [Category("Tokentable details")]
            [Description("The byte offset from the begining until the end of the Token table.")]
            public int TokenTableSize { get; set; }

            [Category("Tokentable details")]
            [Description("The number of tokens in the Tokentable.")]
            public int TokenCount { get; set; }

            [Category("Statefiles")]
            [Description(
                "The statefiles in the save. These store the actual state of the game. The last one is the current one.")]
            public List<StateFileInfo> Files { get; set; }
        }

        [Serializable]
        public class StateFileInfo
        {
            [Category("Statefile details")]
            [Description("This is the contents of the statefile as a byte array.")]
            public byte[] Data { get; set; }

            [Category("Statefile details")]
            [Description("Name of the statefile. (Mapname.hl?) There are mostly 3 of this per map.")]
            public string FileName { get; set; }

            [Category("Statefile details")]
            [Description("Length of the statefile")]
            public int Length { get; set; }

            [Category("Statefile details")]
            [Description("The identifier/header or magic word of the statefile. Should be ('V','A','L','V')")]
            public string MagicWord { get; set; }

            [Category("Statefile details")]
            [Description("Version of the statefile. Mostly 115 (same as the save for the same reason)")]
            public int Version { get; set; }

            [Category("Sections")]
            [Description("The offsets and lengths of the sections in the statefile.")]
            public SaveFileSectionsInfo_t SectionsInfo { get; set; }

            [Category("Sections")] public byte[] pData { get; set; }

            [Category("Sections")] public byte[] pDataHeaders { get; set; }

            [Category("Sections")] public byte[] pSymbols { get; set; }
        }

        /// <summary>
        ///     This client state which are in .hl2 files
        /// </summary>
        public class ClientState : IStateFile
        {
            /// <summary>
            ///     The base client sections
            /// </summary>
            [Description("The base client sections")]
            public baseclientsections_t Baseclientsections;

            /// <summary>
            ///     The base client sections for old save files
            /// </summary>
            [Description("The base client sections for old save files")]
            public baseclientsectionsold_t Baseclientsectionsold;

            /// <summary>
            ///     The Magic of the file
            /// </summary>
            [Description("The magic word of the file")]
            public byte[] IdString;

            /// <summary>
            ///     Version compatibility number
            /// </summary>
            [Description("A number to help version compatibility")]
            public uint Magicnumber;
        }

        /// <summary>
        ///     The entity patch .hl3 file which contains entity IDs.
        /// </summary>
        public class EntityPatchStateFile : IStateFile
        {
            /// <summary>
            ///     Ids of the entities
            /// </summary>
            [Description("These ids get the FENTTABLE_REMOVED flag")] [Category("Details")]
            public int[] EntityIds;
        }

        /// <summary>
        ///     The interface all 3 statefiles implement
        /// </summary>
        public interface IStateFile
        {
        }

        #region DataDesc

        public const int SAVEGAME_MAPNAME_LEN = 32;
        public const int SAVEGAME_COMMENT_LEN = 80;
        public const int SAVEGAME_ELAPSED_LEN = 32;
        public const int SECTION_MAGIC_NUMBER = 0x54541234;
        public const int SECTION_VERSION_NUMBER = 2;
        public const int MAX_MAP_NAME = 32;
        public const int FENTTABLE_REMOVED = 0x40000000;
        public const int FENTTABLE_MOVEABLE = 0x20000000;
        public const int FENTTABLE_GLOBAL = 0x10000000;

        //private struct GAME_HEADER
        //{
        //    private readonly string comment;
        //    private readonly string landmark;

        //    private readonly int mapCount;
        //    // the number of map state files in the save file.  This is usually number of maps * 3 (.hl1, .hl2, .hl3 files)

        //    private readonly string mapName;
        //    private readonly string originMapName;
        //}

        public class baseclientsections_t
        {
            public int decalcount;
            public int decalsize;
            public int entitysize;
            public int headersize;
            public int musiccount;
            public int musicsize;
            public int symbolcount;
            public int symbolsize;

            public int SumBytes()
            {
                return entitysize + headersize + decalsize + symbolsize + musicsize;
            }
        }

        public struct baseclientsectionsold_t
        {
            public int entitysize;
            public int headersize;
            public int decalsize;
            public int symbolsize;

            public int decalcount;
            public int symbolcount;

            public int SumBytes()
            {
                return entitysize + headersize + decalsize + symbolsize;
            }
        }

        //private class CHostState
        //{
        //    private readonly bool m_activeGame;

        //    //Vector m_vecLocation;
        //    private readonly QAngle m_angLocation;
        //    private readonly bool m_bBackgroundLevel;
        //    private readonly bool m_bRememberLocation;
        //    private readonly bool m_bWaitingForConnection;
        //    private readonly HOSTSTATES m_currentState;

        //    private readonly float
        //        m_flShortFrameTime; // run a few one-tick frames to avoid large timesteps while loading assets

        //    private readonly string m_landmarkName;
        //    private readonly string m_levelName;
        //    private readonly HOSTSTATES m_nextState;
        //    private readonly string m_saveName;
        //}

        private enum HOSTSTATES
        {
            HS_NEW_GAME = 0,
            HS_LOAD_GAME,
            HS_CHANGE_LEVEL_SP,
            HS_CHANGE_LEVEL_MP,
            HS_RUN,
            HS_GAME_SHUTDOWN,
            HS_SHUTDOWN,
            HS_RESTART
        }

        public class clientsections_t : baseclientsections_t
        {
            public byte[] decaldata;
            public byte[] entitydata;
            public byte[] headerdata;
            public byte[] musicdata;
            public byte[] symboldata;
        }

        //private struct SaveGameDescription_t
        //{
        //    private readonly int iSize;
        //    private readonly int iTimestamp;
        //    private readonly string szComment;
        //    private readonly string szElapsedTime;
        //    private readonly string szFileName;
        //    private readonly string szFileTime;
        //    private readonly string szMapName;
        //    private readonly string szShortName;
        //    private readonly string szType;
        //}

        //private struct SaveHeader
        //{
        //    private readonly int connectionCount;
        //    private readonly int lightStyleCount;
        //    private readonly string mapName;
        //    private readonly int mapVersion;
        //    private readonly int saveId;
        //    private readonly int skillLevel;
        //    private readonly string skyName;
        //    private readonly float time;
        //    private readonly int version;
        //}

        internal class QAngle
        {
            public float X { get; private set; }
            public float Y { get; private set; }
            public float Z { get; private set; }

            public static QAngle Parse(BinaryReader reader)
            {
                return new QAngle
                {
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle()
                };
            }
        }

        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class SaveFileSectionsInfo_t
        {
            public int nBytesData { get; set; }
            public int nBytesDataHeaders { get; set; }
            public int nBytesSymbols { get; set; }
            public int nSymbols { get; set; }

            public int SumBytes()
            {
                return nBytesSymbols + nBytesDataHeaders + nBytesData;
            }
        }

        public struct SaveFileSections_t
        {
            public byte[] pSymbols { get; set; }
            public byte[] pDataHeaders { get; set; }
            public byte[] pData { get; set; }
        }

        public class saverestorelevelinfo_t
        {
            public int connectionCount; // Number of elements in the levelList[]

            // smooth transition
            public int fUseLandmark;
            public levellist_t[] levelList = new levellist_t[16]; // List of connections from this level
            public int mapVersion;
            public char[] szCurrentMapName = new char[MAX_MAP_NAME]; // To check global entities
            public char[] szLandmarkName = new char[20]; // landmark we'll spawn near in next level

            public float time;
            //public Vector vecLandmarkOffset; // for landmark transitions
        }

        //public class CSaveRestoreSegment
        //{
        //    private readonly int bufferSize; // Total space for data
        //    private readonly byte[] pBaseData; // Start of all entity save data
        //    private readonly byte[] pCurrentData; // Current buffer pointer for sequential access
        //    private readonly byte[,] pTokens; // Hash table of entity strings (sparse)
        //    private readonly int size; // Current data size, aka, pCurrentData - pBaseData

        //    //---------------------------------
        //    // Symbol table
        //    //
        //    private readonly int tokenCount; // Number of elements in the pTokens table
        //}

        public class levellist_t
        {
            public char[] landmarkName = new char[MAX_MAP_NAME];

            public char[] mapName = new char[MAX_MAP_NAME];
            //edict_t* pentLandmark;
            ///public Vector vecLandmarkOrigin;
        }

        //private struct entitytable_t
        //{
        //    private string classname; // entity class name

        //    private int edictindex;
        //    // saved for if the entity requires a certain edict number when restored (players, world)

        //    private int flags; // This could be a short -- bit mask of transitions that this entity is in the PVS of
        //    private string globalname; // entity global name

        //    private int id; // Ordinal ID of this entity (used for entity <--> pointer conversions)

        //    //private Vector landmarkModelSpace; // a fixed position in model space for comparison
        //    private int location; // Offset from the base data of this entity

        //    // NOTE: Brush models can be built in different coordiante systems
        //    //		in different levels, so this fixes up local quantities to match
        //    //		those differences.
        //    private string modelname;
        //    private int restoreentityindex; // the entity index given to this entity at restore time

        //    private int saveentityindex;
        //    // the entity index the entity had at save time ( for fixing up client side entities )

        //    private int size; // Byte size of this entity's data

        //    private void Clear()
        //    {
        //        id = -1;
        //        edictindex = -1;
        //        saveentityindex = -1;
        //        restoreentityindex = -1;
        //        location = 0;
        //        size = 0;
        //        flags = 0;
        //        classname = "";
        //        globalname = "";
        //        //landmarkModelSpace = new Vector();
        //        modelname = "";
        //    }
        //}

        #endregion
    }
}
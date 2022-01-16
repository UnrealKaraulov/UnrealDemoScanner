using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace compLexity_Demo_Player
{
    public abstract class Demo
    {
        public enum StatusEnum
        {
            Ok,
            CorruptDirEntries
        }

        public enum Engines
        {
            HalfLife,
            HalfLifeSteam,
            Source
        }

        public enum Perspectives
        {
            Pov,
            Hltv,
            SourceTv
        }

        protected String name;
        protected String fileFullPath;

        protected StatusEnum status;
        protected Engines engineType;
        protected Perspectives perspective;
        
        protected UInt32 demoProtocol;
        protected UInt32 networkProtocol;
        protected String clientDllChecksum;
        protected String serverName;
        protected String recorderName;
        protected String mapName;
        protected String gameFolderName;
        protected UInt32 mapChecksum;
        protected Single durationInSeconds;
        protected Byte maxClients;
        protected Int32 buildNumber;

        protected IMainWindow mainWindowInterface;
        protected IDemoListView demoListViewInterface;
        protected IProgressWindow writeProgressWindowInterface;

        private Dictionary<string, uint> resources = new Dictionary<string, uint>();

        #region Properties
        /// <summary>
        /// A collection of resources indicies keyed by name. Used by demo conversion to store resources that need to be identified by index later on.
        /// </summary>
        public Dictionary<string, uint> Resources
        {
            get
            {
                return resources;
            }
        }

        public Game Game
        {
            get;
            protected set;
        }

        /// <summary>
        /// Corresponds to an enumeration in the demos corresponding game class derived from Game.
        /// </summary>
        /// <remarks>
        /// This removes the need to constantly determine the version of the game the demo was recorded with by comparing the client.dll checksum to pre-defined values.
        /// </remarks>
        public Int32 GameVersion
        {
            get;
            protected set;
        }

        public String Name
        {
            get
            {
                return name;
            }
        }

        public String FileFullPath
        {
            get
            {
                return fileFullPath;
            }
        }

        public StatusEnum Status
        {
            get
            {
                return status;
            }
        }

        public Engines Engine
        {
            get
            {
                return engineType;
            }
        }

        public virtual String EngineName
        {
            get
            {
                return "";
            }
        }

        public Perspectives Perspective
        {
            get
            {
                return perspective;
            }
        }

        public String PerspectiveString
        {
            get
            {
                switch (perspective)
                {
                    case Perspectives.Pov:
                        return "POV";

                    case Perspectives.Hltv:
                        return "HLTV";

                    case Perspectives.SourceTv:
                        return "Source TV";
                }

                return "Unknown";
            }
        }

        public UInt32 DemoProtocol
        {
            get
            {
                return demoProtocol;
            }
        }

        public UInt32 NetworkProtocol
        {
            get
            {
                return networkProtocol;
            }
        }

        public String ServerName
        {
            get
            {
                return serverName;
            }
        }

        public String RecorderName
        {
            get
            {
                return recorderName;
            }
        }

        public String MapName
        {
            get
            {
                return mapName;
            }
        }

        public String GameFolderName
        {
            get
            {
                return gameFolderName;
            }
        }

        public UInt32 MapChecksum
        {
            get
            {
                return mapChecksum;
            }
        }

        public Single DurationInSeconds
        {
            get
            {
                return durationInSeconds;
            }

            set
            {
                durationInSeconds = value;
            }
        }

        public Byte MaxClients
        {
            get
            {
                return maxClients;
            }

            set
            {
                maxClients = value;
            }
        }

        public Int32 BuildNumber
        {
            get
            {
                return buildNumber;
            }
        }

        public String GameName
        {
            get
            {
                if (Game == null)
                {
                    return "Unknown (" + gameFolderName + ")";
                }

                String result = Game.Name;

                // Try and get the game version.
                String version = Game.FindVersionName(clientDllChecksum);

                if (version != null)
                {
                    result += " " + version;
                }

                return result;
            }
        }
        #endregion

        public Demo()
        {
        }

        /// <summary>
        /// Starts reading the demo in a new thread.
        /// </summary>
        public Thread Read(IMainWindow mainWindowInterface, IDemoListView demoListViewInterface)
        {
            this.mainWindowInterface = mainWindowInterface;
            this.demoListViewInterface = demoListViewInterface;

            // calculate file full path and name
            //fileFullPath = Path.GetFullPath(fileFullPath); // something to do with 8.3?
            name = Path.GetFileNameWithoutExtension(fileFullPath);

            // start the reading thread
            Thread thread = new Thread(new ThreadStart(ReadingThread));
            thread.Name = "Reading Demo";
            thread.Start();

            return thread;
        }

        /// <summary>
        /// Starts writing the demo in a new thread
        /// </summary>
        public Thread Write(IProgressWindow writeProgressWindowInterface)
        {
            this.writeProgressWindowInterface = writeProgressWindowInterface;
            return new Thread(new ParameterizedThreadStart(WritingThread)) { Name = "Writing Demo" };
        }

        protected abstract void ReadingThread();
        protected abstract void WritingThread(object _destinationPath);
    }
}

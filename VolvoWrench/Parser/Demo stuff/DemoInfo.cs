using System.Collections.Generic;

namespace VolvoWrench.DemoStuff
{
    /// <summary>
    ///     Demos return a class that inherits this
    /// </summary>
    public abstract class DemoInfo
    {
        /// <summary>
        ///     The name of the file
        /// </summary>
        public string FileName;

        /// <summary>
        ///     The errors that happened in the demo
        /// </summary>
        public List<string> ParsingErrors;
    }

    /// <summary>
    ///     This is a template for every demo's header
    /// </summary>
    public abstract class DemoHeader
    {
        /// <summary>
        ///     Protocol of the demo
        /// </summary>
        public int DemoProtocol;

        /// <summary>
        ///     The game directory's name
        /// </summary>
        public string GameDir;

        /// <summary>
        ///     The map the demo is played on
        /// </summary>
        public string MapName;

        /// <summary>
        ///     Netprotocol of the demo
        /// </summary>
        public int NetProtocol;
    }
}
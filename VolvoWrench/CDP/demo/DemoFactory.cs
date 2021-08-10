using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace compLexity_Demo_Player
{
    /// <summary>
    /// Given a file name, determines what type of demo it is (if any) and returns the correct type.
    /// </summary>
    static public class DemoFactory
    {
        public static Demo CreateDemo(String fileName)
        {
            const Int32 magicStringLength = 8;

            using (FileStream inputStream = File.OpenRead(fileName))
            {
                using (BinaryReader binaryReader = new BinaryReader(inputStream))
                {
                    String magic = Common.ReadNullTerminatedString(binaryReader, magicStringLength);
                    binaryReader.Close();

                    // create demo object
                    Demo demo = null;

                    if (magic == "HLDEMO")
                    {
                        demo = new HalfLifeDemo(fileName);
                    }
                    else if (magic == "HL2DEMO")
                    {
                        demo = new SourceDemo(fileName);
                    }
                    else
                    {
                        throw new ApplicationException("Not a valid Half-Life or Source engine demo file.");
                    }

                    return demo;
                }
            }
        }
    }
}

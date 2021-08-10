using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections; // Hashtable
using System.Diagnostics; // Assert

namespace compLexity_Demo_Player
{
    public abstract class DemoParser<T> where T:Demo
    {
        private class MessageLog
        {
            public Byte Id;
            public String Name;
            public Int32 Offset;
        }

        protected class MessageHandler
        {
            public Byte Id;
            public Int32 Length;
            public Procedure Callback;
        }

        protected T demo = null;
        protected FileStream fileStream = null;
        protected BinaryReader fileReader = null;

        private Hashtable messageHandlerTable; // id -> length and callback
        private Hashtable messageStringTable; // just for logging purposes, id -> name

        // message logging
        private Queue messageLogQueue;
        private Int64 messageLogFrameOffset;
        private Byte[] messageLogFrameData;

        #region Properties
        public BinaryReader Reader
        {
            get
            {
                return fileReader;
            }
        }

        public Int64 Position
        {
            get
            {
                return fileStream.Position;
            }
        }

        public Int64 FileLength
        {
            get
            {
                return fileStream.Length;
            }
        }

        #endregion

        public DemoParser()
        {
            messageHandlerTable = new Hashtable();
            messageStringTable = new Hashtable();
        }

        public void Open()
        {
            fileStream = File.OpenRead(demo.FileFullPath);
            fileReader = new BinaryReader(fileStream);
        }

        public void Close()
        {
            if (fileReader != null)
            {
                fileReader.Close();
            }

            if (fileStream != null)
            {
                fileStream.Close();
            }
        }

        public void AddMessageHandler(Byte id)
        {
            AddMessageHandler(id, -1, null);
        }

        public void AddMessageHandler(Byte id, Int32 length)
        {
            //Debug.Assert(length != -1);
            AddMessageHandler(id, length, null);
        }

        public void AddMessageHandler(Byte id, Procedure callback)
        {
            Debug.Assert(callback != null);
            AddMessageHandler(id, -1, callback);
        }

        private void AddMessageHandler(Byte id, Int32 length, Procedure callback)
        {
            MessageHandler newHandler = new MessageHandler();

            newHandler.Id = id;
            newHandler.Length = length;
            newHandler.Callback = callback;

            // replace message handler if it already exists
            if (messageHandlerTable.Contains(id))
            {
                messageHandlerTable.Remove(id);
            }

            messageHandlerTable.Add(newHandler.Id, newHandler);
        }

        protected MessageHandler FindMessageHandler(Byte messageId)
        {
            return (MessageHandler)messageHandlerTable[messageId];
        }

        protected void AddMessageIdString(Byte id, String s)
        {
            if (messageStringTable[id] != null)
            {
                messageStringTable.Remove(id);
            }

            messageStringTable.Add(id, s);
        }

        public String FindMessageIdString(Byte id)
        {
            String s = (String)messageStringTable[id];

            if (s == null)
            {
                return "UNKNOWN";
            }

            return s;
        }

        protected void BeginMessageLog(Int64 frameOffset, Byte[] frameData)
        {
            messageLogFrameOffset = frameOffset;
            messageLogFrameData = frameData;
            messageLogQueue = new Queue();
        }

        protected void LogMessage(Byte id, String name, Int32 offset)
        {
            MessageLog log = new MessageLog();
            log.Id = id;
            log.Name = name;
            log.Offset = offset;

            messageLogQueue.Enqueue(log);
        }

        public String ComputeMessageLog()
        {
            String result = String.Format("Frame offset: {0}\n\nMessages:\n", messageLogFrameOffset);

            while (messageLogQueue.Count > 0)
            {
                MessageLog log = (MessageLog)messageLogQueue.Dequeue();
                result += String.Format("{0} [{1}] {2}\n", log.Offset, log.Id, log.Name);
            }

            // log frame data
            if (Config.Settings.LogMessageParsingErrors)
            {
                Random r = new Random((Int32)DateTime.Now.Ticks);
                FileStream fs = File.Create(Config.ProgramPath + String.Format("\\{0}_{1}.bin", demo.Name, r.Next()));
                BinaryWriter writer = new BinaryWriter(fs);

                try
                {
                    fs.Write(messageLogFrameData, 0, messageLogFrameData.Length);
                }
                finally
                {
                    fs.Close();
                }
            }

            return result;
        }
    }
}

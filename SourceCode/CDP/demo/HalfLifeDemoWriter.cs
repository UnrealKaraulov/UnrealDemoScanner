using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections; // ArrayList

namespace compLexity_Demo_Player
{
    public class HalfLifeDemoWriter
    {
        public class AbortWritingException : Exception { }

        private HalfLifeDemo demo;
        private HalfLifeDemoParser parser;
        private BinaryWriter writer;
        private IHalfLifeDemoWriter demoWriterInterface;
        private IProgressWindow progressWindowInterface;

        private Int32 percentRead = 0;
        private Single durationInSeconds = 0.0f;
        private Int32 nPlaybackFrames = 0;
        private Int64 playbackSegmentOffset = 0;
        private Boolean foundPlaybackOffset = false; // since bytes can be added/removed, need to find the playback segment offset again

        // error handling
        private Boolean waitingForErrorWindowResult;
        private MessageWindow.Result lastErrorResult;

        // loading segment bug
        private Int32 firstFrameToWriteIndex = 0;

        public HalfLifeDemoWriter(HalfLifeDemo demo, IHalfLifeDemoWriter demoWriterInterface, IProgressWindow progressWindowInterface, Int32 firstFrameToWriteIndex)
        {
            this.demo = demo;
            this.demoWriterInterface = demoWriterInterface;
            this.progressWindowInterface = progressWindowInterface;
            this.firstFrameToWriteIndex = firstFrameToWriteIndex;
        }

        public void ThreadWorker(String destinationFileName)
        {
            FileStream stream = File.Open(destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            writer = new BinaryWriter(stream);
            Boolean insertEndOfSegment = false;
            parser = new HalfLifeDemoParser(demo);

            // add message handlers
            demoWriterInterface.AddMessageHandlers(parser);

            try
            {
                parser.Open();

                // read and write header
                Byte[] header = parser.Reader.ReadBytes(HalfLifeDemo.HeaderSizeInBytes);
                demoWriterInterface.ProcessHeader(ref header);
                writer.Write(header);

                HalfLifeDemoParser.FrameHeader lastFrameHeader = new HalfLifeDemoParser.FrameHeader();
                Int32 currentFrameIndex = 0;

                while (true)
                {
                    UpdateProgress();

                    // read frame header
                    HalfLifeDemoParser.FrameHeader frameHeader = new HalfLifeDemoParser.FrameHeader();

                    try
                    {
                        frameHeader = ReadFrameHeader();
                        durationInSeconds = frameHeader.Timestamp;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        insertEndOfSegment = true;
                        break;
                    }

                    // check for end of segment
                    if (frameHeader.Type == 5)
                    {
                        if (!foundPlaybackOffset)
                        {
                            playbackSegmentOffset = stream.Position + 9; // 9 for end of segment that hasn't been written yet
                        }
                        else
                        {
                            // end of playback segment, stop parsing so we can write directory entries
                            durationInSeconds = frameHeader.Timestamp;

                            // write frame header (no data for frame type 5) before breaking from the loop
                            WriteFrameHeader(frameHeader);

                            break;
                        }
                    }

                    // read frame data
                    Byte[] frameData;

                    try
                    {
                        Boolean writeFrame;
                        frameData = ReadFrameData(frameHeader, out writeFrame);

                        if (!writeFrame)
                        {
                            // don't write frame
                            currentFrameIndex++;
                            continue;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (AbortWritingException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        insertEndOfSegment = true;
                        break;
                    }

                    if (currentFrameIndex >= firstFrameToWriteIndex)
                    {
                        // write frame header
                        WriteFrameHeader(frameHeader);

                        // write frame data
                        if (frameData != null)
                        {
                            WriteFrameData(frameData);
                        }
                    }

                    currentFrameIndex++;

                    lastFrameHeader = frameHeader;
                }

                if (!foundPlaybackOffset)
                {
                    throw new ApplicationException("Tried to write directory entries without a playback segment offset. Demo is too corrupt to play.");
                }

                if (insertEndOfSegment)
                {
                    HalfLifeDemoParser.FrameHeader frameHeader = new HalfLifeDemoParser.FrameHeader();
                    frameHeader.Timestamp = lastFrameHeader.Timestamp;
                    frameHeader.Number = lastFrameHeader.Number + 1;
                    frameHeader.Type = 5;

                    WriteFrameHeader(frameHeader);
                }

                // write directory entries
                Int64 directoryEntriesOffset = stream.Position;
                writer.Write((Int32)2); // no. of entries
                WriteDirectoryEntry(0, "LOADING", 0.0f, 0, HalfLifeDemo.HeaderSizeInBytes, (UInt32)playbackSegmentOffset - HalfLifeDemo.HeaderSizeInBytes);
                WriteDirectoryEntry(1, "Playback", durationInSeconds, nPlaybackFrames, (UInt32)playbackSegmentOffset, (UInt32)(directoryEntriesOffset - playbackSegmentOffset));

                // go back and write the directory entries offset into the header
                stream.Seek(540, SeekOrigin.Begin);
                stream.Write(BitConverter.GetBytes((UInt32)directoryEntriesOffset), 0, 4);

                demo.DurationInSeconds = durationInSeconds;
            }
            finally
            {
                writer.Close();
                parser.Close();
            }
        }

        private HalfLifeDemoParser.FrameHeader ReadFrameHeader()
        {
            return parser.ReadFrameHeader();
        }

        private void WriteFrameHeader(HalfLifeDemoParser.FrameHeader frameHeader)
        {
            writer.Write(frameHeader.Type);
            writer.Write(frameHeader.Timestamp);
            writer.Write(frameHeader.Number);
        }

        private Byte[] ReadFrameData(HalfLifeDemoParser.FrameHeader frameHeader, out Boolean writeFrame)
        {
            Byte[] result = null;
            writeFrame = true;

            if (frameHeader.Type == 0 || frameHeader.Type == 1)
            {
                // frame header
                Byte[] frameHeaderDemoInfo = parser.Reader.ReadBytes(parser.GameDataDemoInfoLength);
                Byte[] frameHeaderSequenceInfo = parser.Reader.ReadBytes(parser.GameDataSequenceInfoLength);
                UInt32 gameDataLength = parser.Reader.ReadUInt32();

                // frame data
                Byte[] frameData = null;

                if (gameDataLength != 0)
                {
                    // read frame data
                    frameData = parser.Reader.ReadBytes((Int32)gameDataLength);

                    if (frameData.Length != gameDataLength)
                    {
                        throw new ApplicationException("Gamedata frame length doesn't match header.");
                    }

                    // Give the writer interface a chance to insert any new messages into the first gamedata frame.
                    if (frameHeader.Type == 1 && !foundPlaybackOffset)
                    {
                        demoWriterInterface.ProcessFirstGameDataFrame(ref frameData);
                    }

                    // parse frame messages
                    try
                    {
                        if (demoWriterInterface.ShouldParseGameDataMessages(frameHeader.Type))
                        {
                            parser.ParseGameDataMessages(frameData, demoWriterInterface.GetNewUserMessageId);

                            // set frame data to version modified by parsing
                            frameData = parser.BitBuffer.Data;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Error("Error parsing gamedata frame.\n\n" + parser.ComputeMessageLog(), ex, true);

                        if (lastErrorResult != MessageWindow.Result.Continue)
                        {
                            throw new AbortWritingException();
                        }
                        else
                        {
                            writeFrame = false;
                            return null;
                        }
                    }
                }

                // check for end of loading segment
                if (frameHeader.Type == 1)
                {
                    if (!foundPlaybackOffset)
                    {
                        // last 5 frame (end of segment) will have stored the correct offset
                        foundPlaybackOffset = true;
                    }

                    // count playback segment gamedata frames
                    nPlaybackFrames++;
                }

                // copy contents of frame into memorystream, return result
                MemoryStream ms = new MemoryStream();
                demoWriterInterface.WriteDemoInfo(frameHeaderDemoInfo, ms);
                ms.Write(frameHeaderSequenceInfo, 0, frameHeaderSequenceInfo.Length);

                if (gameDataLength == 0)
                {
                    ms.Write(BitConverter.GetBytes(gameDataLength), 0, 4);
                }
                else
                {
                    ms.Write(BitConverter.GetBytes(frameData.Length), 0, 4);
                    ms.Write(frameData, 0, frameData.Length);
                }

                return ms.ToArray();
            }
            else if (frameHeader.Type == 3) // client command
            {
                String command = Common.ReadNullTerminatedString(parser.Reader, 64);

                if (!demoWriterInterface.ShouldWriteClientCommand(command))
                {
                    // don't write frame
                    writeFrame = false;
                    return null;
                }

                parser.Seek(-64);
                result = parser.Reader.ReadBytes(64);

                if (result.Length != 64)
                {
                    throw new ApplicationException("Unexpected client command frame data length.");
                }
            }
            else if (Config.Settings.PlaybackRemoveWeaponAnimations && frameHeader.Type == 7)
            {
                parser.Seek(8);
                writeFrame = false;
                return null;
            }
            else if (frameHeader.Type != 5)
            {
                Int32 frameLength = parser.GetFrameLength(frameHeader.Type);

                if (frameLength != 0)
                {
                    result = parser.Reader.ReadBytes(frameLength);

                    if (result.Length != frameLength)
                    {
                        throw new ApplicationException("Unexpected frame data length.");
                    }
                }
            }

            return result;
        }

        private void WriteFrameData(Byte[] frameData)
        {
            writer.Write(frameData);
        }

        /// <summary>
        /// Writes a single directory entry to disk.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="index"></param>
        /// <param name="title"></param>
        /// <param name="duration"></param>
        /// <param name="nFrames"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private void WriteDirectoryEntry(Int32 index, String title, Single duration, Int32 nFrames, UInt32 offset, UInt32 length)
        {
            writer.Write(index);

            //writer.Write(title);
            foreach (char c in title)
            {
                writer.Write(c);
            }

            // pad title out to 64 bytes
            for (Int32 i = 0; i < 64 - title.Length; i++)
            {
                writer.Write((Byte)0);
            }

            writer.Write((Int32)0); // flags
            writer.Write((Int32)(-1)); // cd track
            writer.Write(duration);
            writer.Write(nFrames);
            writer.Write(offset);
            writer.Write(length);
        }

        private void UpdateProgress()
        {
            Int32 oldPercentRead = percentRead;

            percentRead = (Int32)(parser.Position / (Single)parser.FileLength * 100.0f);

            if (percentRead != oldPercentRead)
            {
                progressWindowInterface.UpdateProgress(percentRead);
            }
        }

        private void Error(String errorMessage, Exception ex, Boolean block)
        {
            if (block)
            {
                waitingForErrorWindowResult = true;
            }

            // WARNING: change continueAbort to a parameter to this method if it is used elsewhere, other than parsing errors
            progressWindowInterface.Error(errorMessage, ex, true, (block ? new Procedure<MessageWindow.Result>(ErrorWindowResult) : null));

            if (!block)
            {
                return;
            }

            // block
            while (waitingForErrorWindowResult)
            {
            }
        }

        private void ErrorWindowResult(MessageWindow.Result result)
        {
            waitingForErrorWindowResult = false;
            lastErrorResult = result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static DemoScanner.DemoStuff.GoldSource.GoldSource;
using static System.Net.Mime.MediaTypeNames;

namespace DemoScanner
{
    public partial class Preview : Form
    {
        string _file = "";
        /*
         PreviewFramesWriter.Write(CurrentTime);
        PreviewFramesWriter.Write(Convert.ToUInt16(PreviousNetMsgFrame.UCmd.Buttons));
        PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.Viewangles.X);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.Viewangles.Y);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.Viewangles.Z);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.ClViewangles.X);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.ClViewangles.Y);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.ClViewangles.Z);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.UCmd.Viewangles.X);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.UCmd.Viewangles.Y);
        PreviewFramesWriter.Write(PreviousNetMsgFrame.UCmd.Viewangles.Z);
        PreviewFramesWriter.Write(CDFRAME_ViewAngles.X);
        PreviewFramesWriter.Write(CDFRAME_ViewAngles.Y);
        PreviewFramesWriter.Write(CDFRAME_ViewAngles.Z);
         * */

        struct PreviewFrameStruct
        {
            public float CurrentTime;
            public UCMD_BUTTONS buttons;
            public float RP_ViewAngles_X;
            public float RP_ViewAngles_Y;
            public float RP_ViewAngles_Z;
            public float RP_Client_ViewAngles_X;
            public float RP_Client_ViewAngles_Y;
            public float RP_Client_ViewAngles_Z;
            public float RP_UCMD_ViewAngles_X;
            public float RP_UCMD_ViewAngles_Y;
            public float RP_UCMD_ViewAngles_Z;
            public float CD_ViewAngles_X;
            public float CD_ViewAngles_Y;
            public float CD_ViewAngles_Z;
        }

        List<PreviewFrameStruct> previewFrameStructs = new List<PreviewFrameStruct>();

        int startFrameId = 0;

        public Preview(string filepath)
        {
            InitializeComponent();
            _file = filepath;
        }

        private void drawGraphBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics pic = e.Graphics;
            var rect = e.ClipRectangle;
            var step_x = rect.Width / 100.0f;
            var center_y = rect.Height / 2.0f;
            Font font = new Font("Arial", 12);
            float value = 0.0f;
            float.TryParse(CurrentTimeTextBox.Text, out value);

            string text = "Num:" + startFrameId + " of " + previewFrameStructs.Count + " - " + value;

            pic.DrawString(text, font, Brushes.Black, step_x * 50.0f - pic.MeasureString(text, font).Width / 2.0f, center_y);

        }

        private void CurrentTimeTextBox_TextChanged(object sender, EventArgs e)
        {
            float value = 0.0f;
            float.TryParse(CurrentTimeTextBox.Text, out value);
            startFrameId = 0;

            for(int i = 0; i < previewFrameStructs.Count;i++)
            {
                if (previewFrameStructs[i].CurrentTime < value)
                {
                    startFrameId = i;
                }
                else break;
            }
            drawGraphBox.Refresh();
            drawKeyboardBox.Refresh();
        }


        private void CurrentTimeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
           //  CurrentTimeTextBox_TextChanged(sender, new EventArgs());
        }

        private void Preview_Load(object sender, EventArgs e)
        {
            if (!File.Exists(_file))
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            using (var filestream = new FileStream(_file, FileMode.Open))
            {
                BinaryReader binaryReader = new BinaryReader(filestream);
                bool firsttime = false;
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    PreviewFrameStruct previewFrameStruct = new PreviewFrameStruct();
                    previewFrameStruct.CurrentTime = binaryReader.ReadSingle();

                    if (!firsttime)
                    {
                        firsttime = true;
                        CurrentTimeTextBox.Text = previewFrameStruct.CurrentTime.ToString();
                    }

                    previewFrameStruct.buttons = (UCMD_BUTTONS)binaryReader.ReadUInt16();

                    previewFrameStruct.RP_ViewAngles_X = binaryReader.ReadSingle();
                    previewFrameStruct.RP_ViewAngles_Y = binaryReader.ReadSingle();
                    previewFrameStruct.RP_ViewAngles_Z = binaryReader.ReadSingle();

                    previewFrameStruct.RP_Client_ViewAngles_X = binaryReader.ReadSingle();
                    previewFrameStruct.RP_Client_ViewAngles_Y = binaryReader.ReadSingle();
                    previewFrameStruct.RP_Client_ViewAngles_Z = binaryReader.ReadSingle();

                    previewFrameStruct.RP_UCMD_ViewAngles_X = binaryReader.ReadSingle();
                    previewFrameStruct.RP_UCMD_ViewAngles_Y = binaryReader.ReadSingle();
                    previewFrameStruct.RP_UCMD_ViewAngles_Z = binaryReader.ReadSingle();

                    previewFrameStruct.CD_ViewAngles_X = binaryReader.ReadSingle();
                    previewFrameStruct.CD_ViewAngles_Y = binaryReader.ReadSingle();
                    previewFrameStruct.CD_ViewAngles_Z = binaryReader.ReadSingle();

                    previewFrameStructs.Add(previewFrameStruct);
                }
            }
        }
    }
}

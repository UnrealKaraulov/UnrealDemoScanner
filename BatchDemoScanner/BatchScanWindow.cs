using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BatchDemoScanner
{
    public partial class BatchScanWindow : Form
    {
        public BatchScanWindow()
        {
            InitializeComponent();
        }

        bool StopThreadScan = false;
        bool BreakScan = false;
        bool StartScan = false;

        Thread ScanThread = null;
        Thread ScanThread2 = null;
        List<Process> processes = new List<Process>();

        struct ScanResult
        {
            public string filepath;
            public string status;
        }

        List<ScanResult> scanResults = new List<ScanResult>();
        void ScanThreadWorkUpdater()
        {
            while (!StopThreadScan)
            {
                Thread.Sleep(500);

                if (StartScan)
                    UpdateListViewItems();
            }
        }
        void ScanThreadWork()
        {
            while (!StopThreadScan)
            {
                Thread.Sleep(500);

                if (StartScan)
                {
                    int count = scanResults.Count;
                    Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = int.Parse(SelectedThreadCount.Text) }, index =>
{
    ScanResult scan = scanResults[index];

    if (BreakScan)
        return;

    bool iswait = scan.status.Length == 0;

    if (iswait)
    {

        scan.status = "scanning...";
        scanResults[index] = scan;

        var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
        if (info.AvailablePhysicalMemory < 512 * 1024 * 1024)
            Thread.Sleep(10000);
        if (iswait)
        {
        }
        string scanoutput = "";
        try
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("UnrealDemoScanner2.exe", "-skip -batchscanner \"" + scanResults[index].filepath + "\"");
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardError = true;
            var process = new Process();

            processes.Add(process);

            process.StartInfo = processStartInfo;

            scan.status = "pre...";

            scanResults[index] = scan;

            bool started = process.Start();

            if (!started)
            {
                scan.status = "error 1...";
                scanResults[index] = scan;
            }
            else
            {
                scan.status = "started...";
                scanResults[index] = scan;
                scanoutput = process.StandardOutput.ReadToEnd();
                if (!process.WaitForExit(120000))
                {
                    scan.status = "error 2...";
                    scanResults[index] = scan;
                }
                else
                {
                    scanoutput = scanoutput.Replace("\r\n", "\n");
                    scan.status = scanoutput.IndexOf("\n[DETECTED]") > 0 ? "HACK!" : "CLEAN!";
                    scanResults[index] = scan;
                }
            }
        }
        catch
        {
            scan.status = "error 3...";
            scanResults[index] = scan;
        }
        finally
        {
        }
    }
});
                    StartScan = false;
                    BreakScan = false;

                    if (!StopThreadScan)
                    {
                        ControlInvoke(StartScanBtn, () =>
                        {
                            StartScanBtn.Enabled = true;
                        });
                    }
                }
            }
        }

        private void StartScanBtn_Click(object sender, EventArgs e)
        {
            if (!File.Exists("UnrealDemoScanner2.exe"))
            {
                MessageBox.Show("No UnrealDemoScanner found in current directory.", "UnrealDemoScanner не найден!");
                return;
            }
            StartScanBtn.Enabled = false;
            StartScan = true;
        }

        private void BreakScanBtn_Click(object sender, EventArgs e)
        {
            BreakScan = true;
            StartScan = false;
        }

        private void BatchScanWindow_Load(object sender, EventArgs e)
        {
            ScanThread = new Thread(ScanThreadWork);
            ScanThread.Start();
            ScanThread2 = new Thread(ScanThreadWorkUpdater);
            ScanThread2.Start();
        }

        private void DemoScanList_DragEnter(object sender, DragEventArgs e)
        {
            if (!StartScanBtn.Enabled)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            e.Effect = DragDropEffects.Copy;
        }

        private void DemoScanList_DragDrop(object sender, DragEventArgs e)
        {
            if (!StartScanBtn.Enabled)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            string[] entries = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    var allfiles = Directory.GetFiles(entry, "*.dem", SearchOption.AllDirectories);
                    foreach (string file in allfiles)
                    {
                        ScanResult scanResult = new ScanResult();
                        scanResult.filepath = file;
                        scanResult.status = "";
                        scanResults.Add(scanResult);
                    }
                }
                else if (File.Exists(entry) && entry.ToLower().EndsWith(".dem"))
                {
                    ScanResult scanResult = new ScanResult();
                    scanResult.filepath = entry;
                    scanResult.status = "";
                    scanResults.Add(scanResult);
                }
            }

            UpdateListViewItems();
        }

        delegate void UniversalVoidDelegate();

        /// <summary>
        /// Call form control action from different thread
        /// </summary>
        public static void ControlInvoke(Control control, Action function)
        {
            if (control.IsDisposed || control.Disposing)
                return;

            if (control.InvokeRequired)
            {
                var tmp = new UniversalVoidDelegate(() => ControlInvoke(control, function));
                control.Invoke(tmp);
                return;
            }
            function();
        }

        void UpdateListViewItems()
        {
            ControlInvoke(DemoScanList, () =>
            {
                while (DemoScanList.Items.Count < scanResults.Count)
                {
                    var listitem = new ListViewItem();
                    var listsubitems = new ListViewItem.ListViewSubItem[2] { new ListViewItem.ListViewSubItem(listitem, ""), new ListViewItem.ListViewSubItem(listitem, "")/*, new ListViewItem.ListViewSubItem(listitem, "") */};
                    DemoScanList.Items.Add(new ListViewItem(listsubitems, 0));
                }
                // DemoScanList.BeginUpdate();
                for (int index = 0; index < scanResults.Count; index++)
                {
                    DemoScanList.Items[index].SubItems[0].Text = scanResults[index].filepath;
                    DemoScanList.Items[index].SubItems[1].Text = scanResults[index].status;
                }
                //DemoScanList.EndUpdate();
            });
        }

        private void BatchScanWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (!StartScanBtn.Enabled)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            e.Effect = DragDropEffects.Copy;
        }

        private void BatchScanWindow_DragDrop(object sender, DragEventArgs e)
        {
            DemoScanList_DragDrop(sender, e);
        }

        private void BatchScanWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            BreakScan = true;
            StopThreadScan = true;
            foreach (var p in processes)
            {
                try
                {
                    p.Kill();
                }
                catch
                {

                }
            }
            ScanThread.Join(1000);
            ScanThread2.Join(1000);
        }
    }
}

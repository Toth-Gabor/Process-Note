using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace Process_Note
{
    public partial class Form : MetroFramework.Forms.MetroForm
    {
        Dictionary<Process, string> processComment = new Dictionary<Process, string>();
        IEnumerable<Process> processes;
        public Form()
        {
            InitializeComponent();
        }

        void GetAllProcess()
        {
            processes = Process.GetProcesses().OrderBy(Process => Process.ProcessName);
            listView.Items.Clear();
            foreach (Process p in processes)
            {
                if (p != null && p.Id > 0)
                {
                    try
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = p.Id.ToString();
                        item.Tag = p;
                        item.SubItems.Add(p.ProcessName);
                        listView.Items.Add(item);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public float GetCPUUsage(Process process)
        {
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfProc_Process");
            var run1 = mos.Get().Cast<ManagementObject>().ToDictionary(mo => mo.Properties["Name"].Value, mo => mo);
            Thread.Sleep(1500);
            var run2 = mos.Get().Cast<ManagementObject>().ToDictionary(mo => mo.Properties["Name"].Value, mo => mo);
            String processName = process.ProcessName;
            if (!run2.ContainsKey(processName)) throw new Exception(string.Format("Process not found: {0}", processName));

            string percentageProcessorTime = "PercentProcessorTime";
            string total = "_Total";

            ulong percentageDiff = (ulong)run2[processName][percentageProcessorTime] - (ulong)run1[processName][percentageProcessorTime];
            ulong totalDiff = (ulong)run2[total][percentageProcessorTime] - (ulong)run1[total][percentageProcessorTime];

            return ((float)percentageDiff / (float)totalDiff) * 100.0f;
        }

        private string GetRamUsage(Process process)
        {
            PerformanceCounter ramCounter = new PerformanceCounter("Process", "Private Bytes", process.ProcessName, true);
            double ram = Math.Round(ramCounter.NextValue() / 1024 / 1024, 2);
            return ram.ToString() + "MB";
        }

        private void ProcessForm_Load(object sender, EventArgs e)
        {
            GetAllProcess();
        }

        private string GetComment(Process process)
        {
            if (processComment.ContainsKey(process))
            {
                return processComment[process];
            }
            return "Add your comment here!";
        }

        private void ProcessForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Do you want to close this application?\r\nAll your comments will be lost!",
                "Exit", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                if (listView.SelectedItems.Count > 0)
                {
                    Process proc = GetCurrentProcess();
                    textBox1.Text = proc.Threads.Count.ToString();
                    DateTime startTime = proc.StartTime;
                    TimeSpan runningTime = DateTime.Now - startTime;
                    running.Text = runningTime.ToString();
                    start.Text = startTime.ToString();
                    cpu.Text = GetCPUUsage(proc).ToString() + "%";
                    memory.Text = GetRamUsage(proc);
                    comment.Text = GetComment(proc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                GetAllProcess();
            }
        }

        private Process GetCurrentProcess()
        {
            if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0] != null)
            {
                foreach (Process p in processes)
                {
                    if (p.Id.ToString() == listView.SelectedItems[0].Text)
                    {
                        return p;
                    }
                }
            }
            return null;
        }
        private void Comment_TextChanged(object sender, EventArgs e)
        {
            processComment[GetCurrentProcess()] = comment.Text;
        }

        private void MetroButton2_Click(object sender, EventArgs e)
        {
            ListView_SelectedIndexChanged(sender, e);
        }
        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Label5_Click(object sender, EventArgs e)
        {

        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {

        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MassScannerWPF
{
    public partial class MainWindow : Window
    {
        #region Variables    
        private static List<string> resultsPingSweep = new List<string>();      // All hosts found during the ping sweep will be added to this list
        private static List<string> resultsPortscans = new List<string>();      // All hosts found during the portscans will be added to this list

        private List<string> argumentsList = new List<string> { "-p- -T5" };    // List containing some default arguments and others will be added/removed by checking the CheckBoxes
        Timer timer = new Timer();                                              // Timer used for updating Nmap scan processes

        int progresscounter = 0;                                                // Counter used for keeping track of the progressbar current value
        int processcounter = 0;                                                 // Counter used for keeping track of the amount of Nmap processes
        int processtotal = 0;                                                   // Total amount of initial processes to start with        
        #endregion

        #region Main       
        public MainWindow()
        {
            InitializeComponent();
            UpdateStatus("Idle");
        }
        #endregion

        #region Initiate scans
        private void btnPingSweep_Click(object sender, RoutedEventArgs e)
        {
            // Clear possible previous stuff
            progresscounter = 0;
            resultsPingSweep.Clear();
            lbResults.Items.Clear();

            // Generate the IP range using the user input
            var iprange = GenerateIPRange(string.Format("{0}.{1}", txtSubnet1start.Text, txtSubnet2start.Text), Convert.ToInt16(txtSubnet3start.Text), Convert.ToInt16(txtSubnet3end.Text), Convert.ToInt16(txtSubnet4start.Text), Convert.ToInt16(txtSubnet4end.Text));

            // Set properties for the ProgressBar and update the status
            progressBar.Maximum = iprange.Count;
            progressBar.Minimum = 0;
            UpdateStatus(string.Format("Running ping sweep on {0} hosts.", iprange.Count));

            // Initiate the actual ping sweep tasks depending on whether the randomize checkbox is checked
            if (chbRandomize.IsChecked == true)
            {
                Random random = new Random();
                var ping = from i in iprange.OrderBy(a => random.Next()) // Randomize list
                           select PingAndUpdateTask(i).ContinueWith(t => Report(t.Result));
                var task = ping.ToArray();
            }
            else
            {
                var ping = from i in iprange
                           select PingAndUpdateTask(i).ContinueWith(t => Report(t.Result));
                var task = ping.ToArray();
            }
        }

        private void btnPortScan_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPort.Text))
            {
                // Clear possible previous stuff
                progresscounter = 0;
                resultsPortscans.Clear();
                lbResults.Items.Clear();

                // Set properties for the ProgressBar and update the status
                progressBar.Maximum = resultsPingSweep.Count;
                progressBar.Minimum = 0;
                UpdateStatus(string.Format("Running portscans on port {0} on {1} hosts.", txtPort.Text, resultsPingSweep.Count));

                // Initiate portscans, with or without banner grabbing
                foreach (var ip in resultsPingSweep)
                {
                    if ((bool)chbGrabBanner.IsChecked)
                    {
                        var scans = from i in Enumerable.Range(Convert.ToInt16(txtPort.Text), 1) // Possibility to scan more than just one port.
                                    select ScanSinglePortTask(ip, i, true); // Don't ContinueWith Report at this moment because it's also done by the banner grab.
                        var task = scans.ToArray();
                    }
                    else
                    {
                        var scans = from i in Enumerable.Range(Convert.ToInt16(txtPort.Text), 1) // Possibility to scan more than just one port.
                                    select ScanSinglePortTask(ip, i, false).ContinueWith(t => Report(t.Result));
                        var task = scans.ToArray();
                    }
                }
            }
        }

        private void btnNmapScan_Click(object sender, RoutedEventArgs e)
        {
            txtHosts.Text = null;

            if ((bool)rdbRunOnPingSweep.IsChecked) // Run scan on ping sweep results
            {
                UpdateStatus(string.Format("Initiating Nmap scan(s) on {0} host(s)", resultsPingSweep.Count));
                foreach (string ip in resultsPingSweep)
                    RunNmapScanTask(ip, txtNmapArgs.Text);
            }
            else                                   // Run scan on portscan results
            {
                UpdateStatus(string.Format("Initiating Nmap scan(s) on {0} host(s)", resultsPortscans.Count));
                foreach (string ip in resultsPortscans)
                    RunNmapScanTask(ip, txtNmapArgs.Text);
            }

            // Update process counter and set set the properties of the ProgressBar
            UpdateCounterNmapProcesses();
            progressBar.Value = 0;
            progressBar.Maximum = processcounter;
            processtotal = processcounter;

            // Start Timer which is used to update the status of the running Nmap scans
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 5000;
            timer.Enabled = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates an IP Range based on the given input. The first two subnets are static.
        /// </summary>
        /// <param name="baseip">e.g.: 192.168</param>
        /// <param name="subnet3start">e.g.: 0</param>
        /// <param name="subnet3end">e.g.: 255</param>
        /// <param name="subnet4start">e.g.: 0</param>
        /// <param name="subnet4end">e.g.: 255</param>
        /// <returns>List with the IP range</returns>
        private List<string> GenerateIPRange(string baseip, int subnet3start, int subnet3end, int subnet4start, int subnet4end)
        {
            List<string> range = new List<string>();

            for (int subnet3 = subnet3start; subnet3 <= subnet3end; subnet3++)
                for (int subnet4 = subnet4start; subnet4 <= subnet4end; subnet4++)
                    range.Add(string.Format("{0}.{1}.{2}", baseip, subnet3, subnet4));

            return range;
        }

        /// <summary>
        /// Pings the host by a given IP address and returns the IP if the ping was successful.
        /// </summary>
        /// <param name="ip">IP address of host</param>
        /// <returns>IP address of host or null</returns>
        private Task<string> PingAndUpdateTask(string ip)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    progresscounter = progresscounter + 1;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => this.progressBar.Value = progresscounter));

                    Ping ping = new Ping();
                    var reply = ping.Send(ip);

                    if (reply.Status == IPStatus.Success)
                    {
                        resultsPingSweep.Add(ip);
                        UpdateHosts(resultsPingSweep);
                        return reply.Address.ToString();
                    }
                    else
                        return null;
                }
                catch { return null; }
            });
        }

        /// <summary>
        /// Connects to a given TCP port. Returns the host IP address if successful.
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="port">Port to scan</param>
        /// <returns>IP address of host or null</returns>
        private Task<string> ScanSinglePortTask(string ip, int port, bool grabbanner)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    progresscounter = progresscounter + 1;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => this.progressBar.Value = progresscounter));

                    using (var client = new TcpClient())
                    {
                        client.SendTimeout = 10;
                        client.Connect(ip, port);
                    }

                    resultsPortscans.Add(ip);
                    UpdateHosts(resultsPortscans);

                    if (grabbanner)
                    {
                        var banner = GrabBanner(ip, port, 5000).ContinueWith(t => Report(t.Result));
                    }

                    return ip;
                }
                catch { return null; }
            });
        }

        /// <summary>
        /// Tries to grab the banner by a given TCP port. Returns the host IP address if successful.
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="port">Port to grab banner from</param>
        /// <param name="timeout">Set the timeout</param>
        /// <returns>IP address of host or null</returns>
        private Task<string> GrabBanner(string ip, int port, int timeout)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var client = new TcpClient(ip, port))
                    {
                        client.SendTimeout = timeout;
                        client.ReceiveTimeout = timeout;
                        NetworkStream ns = client.GetStream();
                        StreamWriter sw = new StreamWriter(ns);

                        sw.Write("HEAD / HTTP/1.1\r\n\r\n");
                        sw.Flush();

                        byte[] bytes = new byte[2048];
                        int bytesRead = ns.Read(bytes, 0, bytes.Length);
                        string response = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                        return string.Format("{0}\n{1}", ip, response);
                    }
                }
                catch { return null; }
            });
        }

        public Task RunNmapScanTask(string ip, string args)
        {
            //  string output, error;
            using (var process = new Process())
            {
                process.StartInfo.FileName = @"C:\Program Files (x86)\Nmap\nmap.exe";
                process.StartInfo.Arguments = string.Format("{0} {1} -oN {1}.txt", args, ip);
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                //  process.StartInfo.RedirectStandardOutput = true;
                //  process.StartInfo.RedirectStandardError = true;
                process.Start();
                //  process.WaitForExit();
                //  output = process.StandardOutput.ReadToEnd();
                //  error = process.StandardError.ReadToEnd();
            }
            return null;
            //   return string.IsNullOrEmpty(error) ? output : error;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Update the status TextBlock on the MainWindow
        /// </summary>
        /// <param name="message">The message to show</param>
        private void UpdateStatus(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => txtStatus.Text = string.Format("{0} - {1}", DateTime.Now, message)));
        }

        /// <summary>
        /// Update the hosts TextBlock on the MainWindow. It uses a result list (resultsPingSweep or resultsPortscan) as parameter to count the results.
        /// </summary>
        /// <param name="list">The result list to count the host from</param>
        private void UpdateHosts(List<string> list)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => txtHosts.Text = string.Format("Discovered {0} hosts.", list.Count)));
        }

        /// <summary>
        /// Add results to ListBox
        /// </summary>
        /// <param name="message">The IP address of the found host/service</param>
        private void Report(object message)
        {
            if (message != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => this.lbResults.Items.Add(message.ToString())));
        }

        /// <summary>
        /// Checks how many Nmap processes are currently running and then updates the processcounter
        /// </summary>
        private void UpdateCounterNmapProcesses()
        {
            processcounter = 0;
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist.Where(x => x.ProcessName.Equals("nmap")))
                processcounter = processcounter + 1;
        }
        
        /// <summary>
        /// Updates the TextBox containing the Nmap arguments depending on the checked RadioButtons
        /// </summary>
        private void UpdateNmapArguments()
        {
            string args = null;
            foreach (var arg in argumentsList)
                args = args + arg;

            txtNmapArgs.Text = args;
        }
        #endregion

        #region Import/Export   
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt";

            if (dialog.ShowDialog() == true)
            {
                lbResults.Items.Clear();
                resultsPingSweep.Clear();

                var import = File.ReadAllLines(dialog.FileName);

                if (import.Any())
                {
                    foreach (var ip in import)
                    {
                        resultsPingSweep.Add(ip);
                        lbResults.Items.Add(ip);
                    }

                    // Enable controls
                    brdPingSweep.IsEnabled = true;
                    brdPortScan.IsEnabled = true;
                    brdNmap.IsEnabled = true;

                    // Update status
                    UpdateStatus(String.Format("Imported {0} hosts", resultsPingSweep.Count));
                    txtHosts.Text = null;
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "output.txt";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text file (*.txt)|*.txt";

            if (dialog.ShowDialog() == true)
            {
                File.AppendAllLines(dialog.FileName, lbResults.Items.Cast<string>().ToArray());

                UpdateStatus(String.Format("Exported {0} hosts", resultsPingSweep.Count));
                txtHosts.Text = null;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// I abuse this event to also do some GUI work. Quick and dirty..but hey, it works.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressBar bar = (ProgressBar)sender;
            if (bar.Value >= bar.Maximum - 1)
            {
                UpdateStatus("Finished!");

                brdPingSweep.IsEnabled = true;

                if (resultsPingSweep.Any()) // Only enable other controls if there are any results
                {
                    brdPortScan.IsEnabled = true;
                    brdNmap.IsEnabled = true;
                    btnExport.IsEnabled = true;
                    btnImport.IsEnabled = true;
                }
            }
            else
            {
                brdPingSweep.IsEnabled = false;
                brdPortScan.IsEnabled = false;
                brdNmap.IsEnabled = false;
                btnExport.IsEnabled = false;
                btnImport.IsEnabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                lbNmap.Items.Clear();

                ListBox listBox = sender as ListBox;

                if (listBox.SelectedItem != null)
                {
                    string path = string.Format("{0}.txt", listBox.SelectedItem.ToString());

                    if (File.Exists(path))
                    {
                        string[] result = File.ReadAllLines(path);
                        if (result != null)
                            foreach (string r in result)
                                lbNmap.Items.Add(r);
                    }
                    else
                        lbNmap.Items.Add("Scan results not (yet) available.");
                }
            }
            catch { lbNmap.Items.Add("Scan results not (yet) available."); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            UpdateCounterNmapProcesses();

            // Calculate the current amount of running processes and update the status
            int currentstatus = processtotal - processcounter;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => progressBar.Value = currentstatus));

            UpdateStatus(String.Format("Currently running {0} Nmap scan(s) on a total of {1} hosts", processcounter, processtotal));

            // Stop timer if there are no more Nmap processes running
            if (processcounter == 0)
                timer.Stop();
        }

        /// <summary>
        /// Enables the portscan button if txtPort.Text has value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            if (textbox != null)
            {
                if (!string.IsNullOrEmpty(textbox.Text))
                    btnPortScan.IsEnabled = true;
                else
                    btnPortScan.IsEnabled = false;
            }
        }

        /// <summary>
        /// Selects the text in the TextBox when a TextBox control has focus. 
        /// </summary>
        /// <param name="sender"></param>
        private void SetFocusOnSelectedTextBox(object sender)
        {
            TextBox t = (sender as TextBox);

            if (t == null) return;

            t.SelectAll();
            t.ReleaseMouseCapture();
        }

        private void txtSubnet1start_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocusOnSelectedTextBox(sender);
        }

        private void txtSubnet2start_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocusOnSelectedTextBox(sender);
        }

        private void txtSubnet3start_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocusOnSelectedTextBox(sender);
        }

        private void txtSubnet4start_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocusOnSelectedTextBox(sender);
        }

        private void txtSubnet3end_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocusOnSelectedTextBox(sender);
        }
        private void txtSubnet4end_GotFocus(object sender, RoutedEventArgs e)
        {
            SetFocusOnSelectedTextBox(sender);
        }

        private void chbScript_Checked(object sender, RoutedEventArgs e)
        {
            argumentsList.Add(" --script=vuln");
            UpdateNmapArguments();
        }

        private void chbScript_Unchecked(object sender, RoutedEventArgs e)
        {
            argumentsList.RemoveAll(x => x == " --script=vuln");
            UpdateNmapArguments();
        }

        private void chbVersion_Checked(object sender, RoutedEventArgs e)
        {
            argumentsList.Add(" -sV");
            UpdateNmapArguments();
        }
        private void chbVersion_Unchecked(object sender, RoutedEventArgs e)
        {
            argumentsList.RemoveAll(x => x == " -sV");
            UpdateNmapArguments();
        }

        private void chbOSFingerprint_Checked(object sender, RoutedEventArgs e)
        {
            argumentsList.Add(" -O");
            UpdateNmapArguments();
        }

        private void chbOSFingerprint_Unchecked(object sender, RoutedEventArgs e)
        {
            argumentsList.RemoveAll(x => x == " -O");
            UpdateNmapArguments();
        }
        #endregion
    }
}

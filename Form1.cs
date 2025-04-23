using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;


namespace HotspotToolkit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string GetWiFiSSID()
        {
            var process = new Process
            {
                StartInfo =
    {
    FileName = "netsh.exe",
    Arguments = "wlan show interfaces",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    CreateNoWindow = true
    }
            };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var line = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("SSID") && !l.Contains("BSSID"));
            if (line == null)
            {
                return string.Empty;
            }
            var ssid = line.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart();
            return ssid;
        }

        private void Label7_Click(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        public bool warnedPass;

        private void TextBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (warnedPass == false)
            {
                MessageBox.Show("Password anda tidak akan tersensor! Jika laptop anda terhubung ke HDMI atau sedang screen sharing, silahkan lepaskan koneksi layar terlebih dahulu supaya tidak terlihat oleh siswa lainnya.", "Peringatan!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                warnedPass = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            warnedPass = false;
            string folderPath = "scripts";

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                scriptLists.Items.AddRange(files);
            }
        }

        private void TabPage5_Click(object sender, EventArgs e)
        {

        }

        private void Label20_Click(object sender, EventArgs e)
        {

        }

        private void AirCheckBox6_CheckedChanged(object sender)
        {

        }

        private void TabPage4_Click(object sender, EventArgs e)
        {

        }

        private void Button10_Click(object sender, EventArgs e)
        {

        }

        private void Button11_Click(object sender, EventArgs e)
        {

        }

        private void Button9_Click(object sender, EventArgs e)
        {

        }

        private void Button8_Click(object sender, EventArgs e)
        {

        }

        private void TabPage2_Click(object sender, EventArgs e)
        {

        }

        private async void BtnCheckHtsptStat_Click(object sender, EventArgs e)
        {
            // Check if connected to a WiFi network
            lblRess.Text = "Memulai pengecekan jaringan, mohon tunggu.";
            bool isConnectedToWifi = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

            if (!isConnectedToWifi)
            {
                lblRess.ForeColor = System.Drawing.Color.DarkRed;
                lblRess.Text = "Not connected to any WiFi network.";
                return;
            }

            // Get the WiFi SSID (Name)
            string wifiName = GetWiFiSSID();


            if (wifiName != "IDN Boarding School")
            {
                lblRess.ForeColor = System.Drawing.Color.DarkRed;
                lblRess.Text = "WiFi connected, but not to IDN.";
                return;
            }
            lblRess.Text = "Mengambil beberapa informasi sistem, mohon tunggu.";
            string ignoreMe = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
        .Select(ni => ni.Description)
        .FirstOrDefault();

            // Get Default Gateway IP Address
            string gatewayAddress = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.Description == ignoreMe)
                .SelectMany(ni => ni.GetIPProperties().GatewayAddresses)
                .FirstOrDefault()?.Address.ToString();

            // Get Current Device IPv4 Address
            string localIp = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.Description == ignoreMe)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .FirstOrDefault()?.Address.ToString();

            // Update labels
            lblRouterAddr.Text = gatewayAddress;
            lblpcIP.Text = localIp;

            // Send HTTP GET request
            lblRess.Text = "Mengirim sample request ke Gateway Router, mohon tunggu.";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://"+lblRouterAddr.Text+"/status");
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (responseBody.Contains("login100-pic js-tilt"))
                    {
                        lblRess.ForeColor = System.Drawing.Color.DarkRed;
                        lblRess.Text = "Login dibutuhkan, silahkan login dibawah.";
                        txtUsrInpt.Enabled = true;
                        txtPwdInpt.Enabled = true;
                        btnLoginUsr.Enabled = true;
                        //btnLogoutUsr.Enabled = true;
                    }
                    else
                    {
                        // Define a regular expression pattern to match the "Welcome, {username}!" format
                        string pattern = @"Welcome, (.+?)!";

                        // Use Regex.Match to find the first match in the HTML string
                        Match match = Regex.Match(responseBody, pattern);

                        if (match.Success)
                        {
                            string username = match.Groups[1].Value;
                            lblUsrHotspt.Text = username;
                            txtUsrInpt.Text = username;
                            txtPwdInpt.Text = "********";
                            btnLogoutUsr.Enabled = true;
                            lblRess.ForeColor = System.Drawing.Color.DarkGreen;
                            lblRess.Text = "Anda terlogged in dan Internet tersedia!";
                        }
                        else
                        {
                            lblRess.ForeColor = System.Drawing.Color.DarkRed;
                            lblRess.Text = "Terjadi masalah, hubungi Developer.";
                        }
                    }


                }
            }
            catch
            {
                lblRess.Text = "Mengecek koneksi Internet, mohon tunggu.";
                string hostNameOrAddress = "www.google.com"; // Google's hostname or IP address

                Ping ping = new Ping();
                try
                {
                    PingReply reply = ping.Send(hostNameOrAddress);

                    if (reply.Status == IPStatus.Success)
                    {
                        lblRess.ForeColor = System.Drawing.Color.DarkGreen;
                        lblRess.Text = "Hotspot tidak tersedia, tapi Internet ada!";
                    }
                    else
                    {
                        lblRess.ForeColor = System.Drawing.Color.DarkRed;
                        lblRess.Text = "Hotspot tidak tersedia, begitu juga Internetnya!";
                    }
                }
                catch (PingException ex)
                {
                    lblRess.ForeColor = System.Drawing.Color.DarkRed;
                    lblRess.Text = "Tidak bisa terhubung dengan Server Hotspot.";
                }
            }

        }

        private async void BtnLogoutUsr_Click(object sender, EventArgs e)
        {
            lblRess.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            lblRess.Text = "Proses logout, mohon tunggu.";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://"+lblRouterAddr.Text+"/logout");
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (responseBody.Contains("logged out"))
                    {
                        lblRess.ForeColor = System.Drawing.Color.DarkGreen;
                        lblRess.Text = "Berhasil terlogout, silahkan login kembali.";
                        txtUsrInpt.Enabled = true;
                        txtPwdInpt.Enabled = true;
                        btnLoginUsr.Enabled = true;
                        btnLogoutUsr.Enabled = false;
                        txtUsrInpt.Text = "";
                        txtPwdInpt.Text = "";
                        lblUsrHotspt.Text = "-";
                    } else
                    {
                        lblRess.ForeColor = System.Drawing.Color.DarkRed;
                        lblRess.Text = "Respon server agak lain, coba lagi nanti.";
                    }
                }
            } catch
            {
                lblRess.ForeColor = System.Drawing.Color.DarkRed;
                lblRess.Text = "Gagal proses logout, coba lagi nanti.";
            }
        }

        private async void BtnLoginUsr_Click(object sender, EventArgs e)
        {
            lblRess.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            lblRess.Text = "Proses login, mohon tunggu.";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://"+lblRouterAddr.Text+"/login?username="+txtUsrInpt.Text+"&password="+txtPwdInpt.Text);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    string pattern = @"<strong>(.*?)</strong>";

                    MatchCollection matches = Regex.Matches(responseBody, pattern);

                    if (matches.Count >= 2)
                    {
                        string errorMessage = matches[1].Groups[1].Value;
                        if (errorMessage.Contains("invalid username"))
                        {
                            lblRess.ForeColor = System.Drawing.Color.DarkRed;
                            lblRess.Text = "Username atau Password salah, silahkan cek lagi.";

                        } else
                        {
                            lblRess.ForeColor = System.Drawing.Color.DarkRed;
                            lblRess.Text = "Kesalahan dari sisi Router, hubungi Admin.";
                            
                        }
                    }
                    else
                    {
                        if (responseBody.Contains("You are logged in"))
                        {
                            lblRess.ForeColor = System.Drawing.Color.DarkGreen;
                            lblRess.Text = "Berhasil login, mengecek Internet. ";


                            string hostNameOrAddress = "www.google.com"; // Google's hostname or IP address

                            Ping ping = new Ping();
                            try
                            {
                                PingReply reply = ping.Send(hostNameOrAddress);

                                if (reply.Status == IPStatus.Success)
                                {
                                    lblRess.ForeColor = System.Drawing.Color.DarkGreen;
                                    lblRess.Text = "Berhasil login dan Internet tersedia!";
                                }
                                else
                                {
                                    lblRess.ForeColor = System.Drawing.Color.DarkRed;
                                    lblRess.Text = "Berhasil login tapi tidak ada Internet!";
                                }
                            }
                            catch (PingException ex)
                            {
                                lblRess.ForeColor = System.Drawing.Color.DarkRed;
                                lblRess.Text = "Berhasil login, tidak bisa cek Internet.";
                            }

                            txtUsrInpt.Enabled = false;
                            txtPwdInpt.Enabled = false;
                            btnLoginUsr.Enabled = false;
                            btnLogoutUsr.Enabled = true;
                            lblUsrHotspt.Text = txtUsrInpt.Text;
                        } else { 
                            lblRess.ForeColor = System.Drawing.Color.DarkRed;
                            lblRess.Text = "Respon dari Router tidak valid, coba lagi. ";
                        }
                    }

                }
            }
            catch
            {
                lblRess.ForeColor = System.Drawing.Color.DarkRed;
                lblRess.Text = "Gagal proses logout, coba lagi nanti.";
            }
        }

        private async void BtnStartTrouble_Click(object sender, EventArgs e)
        {
            lblStatTrble.Text = "Menganalisa koneksi jaringan, mohon tunggu.";

            // Reset checkbox states and label properties manually for each control
            chkJob1.Checked = false;
            lblResCheck1.Text = "-";
            lblResCheck1.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            chkJob2.Checked = false;
            lblResCheck2.Text = "-";
            lblResCheck2.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            chkJob3.Checked = false;
            lblResCheck3.Text = "-";
            lblResCheck3.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            chkJob4.Checked = false;
            lblResCheck4.Text = "-";
            lblResCheck4.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            chkJob5.Checked = false;
            lblResCheck5.Text = "-";
            lblResCheck5.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            chkJob6.Checked = false;
            lblResCheck6.Text = "-";
            lblResCheck6.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            chkJob7.Checked = false;
            lblResCheck7.Text = "-";
            lblResCheck7.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);

            await Task.Delay(1000);
            // Check the current connected WiFi SSID
            string currentWifiSSID = GetWiFiSSID();

            // Check if the current WiFi is "IDN Boarding School"
            if (currentWifiSSID == "IDN Boarding School")
            {
                // Set Checkbox "chkJob1" to Checked
                chkJob1.Checked = true;

                // Set label lblResCheck1 to "OK" with DarkGreen color
                lblResCheck1.Text = "OK";
                lblResCheck1.ForeColor = Color.DarkGreen;
            }
            else
            {
                // Set Checkbox "chkJob1" to Unchecked
                chkJob1.Checked = false;

                // Set label lblResCheck1 to "FAIL" with DarkRed color
                lblResCheck1.Text = "FAIL";
                lblResCheck1.ForeColor = Color.DarkRed;
            }


            string ignoreMe = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
        .Select(ni => ni.Description)
        .FirstOrDefault();

            // Get Default Gateway IP Address
            string gatewayAddress = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.Description == ignoreMe)
                .SelectMany(ni => ni.GetIPProperties().GatewayAddresses)
                .FirstOrDefault()?.Address.ToString();

            // Get Current Device IPv4 Address
            string localIp = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.Description == ignoreMe)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .FirstOrDefault()?.Address.ToString();

            string[] validIPPrefixes = { "192.168.20.", "192.168.11.", "192.168.21.", "192.168.100." };

            if(gatewayAddress == null)
            {
                gatewayAddress = "127.0.0.1";
            }
            bool isIPValid = validIPPrefixes.Any(prefix => localIp.StartsWith(prefix));

            // Update Checkbox "chkJob2" and label lblResCheck2 based on the IP address check
            chkJob2.Checked = isIPValid;
            lblResCheck2.Text = isIPValid ? "OK" : "FAIL";
            lblResCheck2.ForeColor = isIPValid ? Color.DarkGreen : Color.DarkRed;




            bool isGatewayValid = validIPPrefixes.Any(prefix => gatewayAddress.StartsWith(prefix));

            // Update Checkbox "chkJob2" and label lblResCheck2 based on the IP address check
            chkJob3.Checked = isGatewayValid;
            lblResCheck3.Text = isGatewayValid ? "OK" : "FAIL";
            lblResCheck3.ForeColor = isGatewayValid ? Color.DarkGreen : Color.DarkRed;

            bool isLoggedHotspot;
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://" + gatewayAddress + "/status");
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (responseBody.Contains("login100-pic js-tilt"))
                    {
                        isLoggedHotspot = false;
                    }
                    else
                    {
                        // Define a regular expression pattern to match the "Welcome, {username}!" format
                        string pattern = @"Welcome, (.+?)!";

                        // Use Regex.Match to find the first match in the HTML string
                        Match match = Regex.Match(responseBody, pattern);

                        if (match.Success)
                        {
                            isLoggedHotspot = true;
                        }
                        else
                        {
                            isLoggedHotspot = false;
                        }
                    }

                    chkJob4.Checked = isLoggedHotspot;
                    lblResCheck4.Text = isLoggedHotspot ? "OK" : "FAIL";
                    lblResCheck4.ForeColor = isLoggedHotspot ? Color.DarkGreen : Color.DarkRed;
                }
            } catch {
                isLoggedHotspot = false;
                chkJob4.Checked = isLoggedHotspot;
                lblResCheck4.Text = isLoggedHotspot ? "OK" : "FAIL";
                lblResCheck4.ForeColor = isLoggedHotspot ? Color.DarkGreen : Color.DarkRed;
            }

            bool isRouterPingable;
            int pingTime = -1; // Initialize pingTime to a default value

                Ping ping = new Ping();
                try
                {
                    PingReply reply = ping.Send(gatewayAddress);

                    if (reply.Status == IPStatus.Success)
                    {
                        isRouterPingable = true;
                        pingTime = (int)reply.RoundtripTime; // Get the ping time in milliseconds
                    }
                    else
                    {
                        isRouterPingable = false;
                    }
                }
                catch (PingException ex)
                {
                    isRouterPingable = false;
                }

                chkJob5.Checked = isRouterPingable;
                if (isRouterPingable)
                {
                    lblResCheck5.Text = pingTime.ToString() + "ms"; // Display ping time if OK

                if (pingTime > 450)
                    {
                        lblResCheck5.ForeColor = Color.DarkRed; // Set color to DarkRed if ping > 150ms
                    }
                    else
                    {
                        lblResCheck5.ForeColor = Color.DarkGreen;
                    }

                if (pingTime.ToString() == "0")
                {
                    lblResCheck5.ForeColor = Color.DarkRed; // Set color to DarkRed if ping > 150ms
                    chkJob5.Checked = false;
                    lblResCheck5.Text = "FAIL";
                }
            }
                else
                {
                    lblResCheck5.Text = "FAIL";
                    lblResCheck5.ForeColor = Color.DarkRed;
                }





                bool isLocalPingable;
                int pingTime2 = -1; // Initialize pingTime to a default value

                Ping ping2 = new Ping();
                try
                {
                    PingReply reply = ping2.Send(gatewayAddress);

                    if (reply.Status == IPStatus.Success)
                    {
                        isLocalPingable = true;
                        pingTime2 = (int)reply.RoundtripTime; // Get the ping time in milliseconds
                    }
                    else
                    {
                        isLocalPingable = false;
                    }
                }
                catch (PingException ex)
                {
                    isLocalPingable = false;
                }

                chkJob6.Checked = isLocalPingable;
                if (isLocalPingable)
                {
                    lblResCheck6.Text = pingTime2.ToString() + "ms"; // Display ping time if OK

                if (pingTime2 > 450)
                    {
                        lblResCheck6.ForeColor = Color.DarkRed; // Set color to DarkRed if ping > 150ms
                    }
                    else
                    {
                        lblResCheck6.ForeColor = Color.DarkGreen;
                    }

                if (pingTime2.ToString() == "0")
                {
                    lblResCheck6.ForeColor = Color.DarkRed; // Set color to DarkRed if ping > 150ms
                    chkJob6.Checked = false;
                    lblResCheck6.Text = "FAIL";
                }
            }
                else
                {
                    lblResCheck6.Text = "FAIL";
                    lblResCheck6.ForeColor = Color.DarkRed;
                }



                bool isGooglePing;
                int pingTime3 = -1; // Initialize pingTime to a default value

                Ping ping3 = new Ping();
                try
                {
                    PingReply reply = ping3.Send(gatewayAddress);

                    if (reply.Status == IPStatus.Success)
                    {
                        isGooglePing = true;
                        pingTime3 = (int)reply.RoundtripTime; // Get the ping time in milliseconds
                    }
                    else
                    {
                        isGooglePing = false;
                    }
                }
                catch (PingException ex)
                {
                    isGooglePing = false;
                }

                chkJob7.Checked = isGooglePing;
                if (isGooglePing)
                {
                    lblResCheck7.Text = pingTime3.ToString() + "ms"; // Display ping time if OK

                
                if (pingTime3 > 450)
                    {
                        lblResCheck7.ForeColor = Color.DarkRed; // Set color to DarkRed if ping > 150ms
                    }
                    else
                    {
                        lblResCheck7.ForeColor = Color.DarkGreen;
                    }

                if (pingTime3.ToString() == "0")
                {
                    lblResCheck7.ForeColor = Color.DarkRed; // Set color to DarkRed if ping > 150ms
                    chkJob7.Checked = false;
                    lblResCheck7.Text = "FAIL";
                }
            }
                else
                {
                    lblResCheck7.Text = "FAIL";
                    lblResCheck7.ForeColor = Color.DarkRed;
                }

                lblStatTrble.Text = "Koneksi telah dianalisa, silahkan cek hasil.";

            }


        private void BtnCheckSpeed1_Click(object sender, EventArgs e)
        {
            string serverIpAddress = "10.20.30.120";
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(serverIpAddress);

                if (reply.Status == IPStatus.Success)
                {
                    Process.Start("msedge", $"--incognito --app=\"http://{serverIpAddress}/\"");
                }
                else
                {
                    MessageBox.Show("Tidak bisa terhubung ke server Speedtest, gunakan Metode 2.", "IDN Boarding School - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running SpeedTest: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCheckSpeed2_Click(object sender, EventArgs e)
        {
            string serverIpAddress = "speedtest.net";

            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(serverIpAddress);

                if (reply.Status == IPStatus.Success)
                {
                    Process.Start("msedge", $"--incognito --app=\"http://{serverIpAddress}/\"");
                }
                else
                {
                    MessageBox.Show("Tidak bisa terhubung ke server Speedtest, gunakan Metode 1.", "IDN Boarding School - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running SpeedTest: {ex.Message}", "IDN Boarding School - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateScriptLists_Click(object sender, EventArgs e)
        {
            string folderPath = "scripts";
            scriptLists.Items.Clear();

            string ipAddress = "172.30.20.2";
            string downloadUrl = "http://172.30.20.2/hotspotScripts.zip";
            string scriptsFolder = "scripts";
            string zipFileName = "hotspotScripts.zip";

            // Check if the host is pingable
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(ipAddress);

                if (reply.Status == IPStatus.Success)
                {
                    // Create the 'scripts' folder if it doesn't exist
                    if (!Directory.Exists(scriptsFolder))
                    {
                        Directory.CreateDirectory(scriptsFolder);
                    }

                    // Download the file
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(downloadUrl, zipFileName);

                    foreach (string filePath in Directory.GetFiles(scriptsFolder))
                    {
                        File.Delete(filePath);
                    }

                    using (ZipArchive archive = ZipFile.OpenRead(zipFileName))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine(scriptsFolder, entry.FullName), true);
                        }
                    }


                    // Delete the ZIP file
                    File.Delete(zipFileName);

                    MessageBox.Show($"Berhasil mengunduh scripts terbaru dari Server dan Direktori script telah berhasil diupdate.", "IDN Boarding School - Sukses Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Tidak bisa mengunduh scripts terbaru dari Server, Direktori script akan direfresh.", "IDN Boarding School - Sukses Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi masalah saat komunikasi dengan Server, Direktori script akan direfresh.", "IDN Boarding School - Sukses Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                scriptLists.Items.AddRange(files);
            }
        }

        private void RunScript_Click(object sender, EventArgs e)
        {
            if (scriptLists.SelectedItem != null)
            {

                // Get the selected item (expected example: "testing.idn")
                string selectedFile = scriptLists.SelectedItem.ToString();

                if (selectedFile.Contains(".idn"))
                {
                    string command = $@"copy /Y ""{selectedFile}"" ""%temp%\start.ps1"" && cls && powershell.exe -ExecutionPolicy Bypass -File ""%temp%\start.ps1"" & echo Script has finished, press any key to close... & pause >nul";
                    Process.Start("cmd.exe", $"/c {command}");
                } else
                {
                    MessageBox.Show($"Script yang anda jalankan tidak support.", "IDN Boarding School - Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                // Construct the command
            }
            else
            {
                MessageBox.Show($"Silahkan pilih script yang ingin dijalankan.", "IDN Boarding School - Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void KirimLaporBtn_Click(object sender, EventArgs e)
        {
            // Generate a random report ID
            string reportId = GenerateRandomReportId();

            // Create the report text
            string reportText = $@"HOTSPOT TOOLKIT V1.2 - REPORT FORM
================================================

REPORT ID: {reportId}
REPORT TITLE: {txtlaporTitle.Text} 
REPORT DESCRIPTION:
{txtlaporDesc.Text}

TECHNICAL ANALYSIS
================================================
DATETIME: {DateTime.Now}

SYSTEM INFORMATION:
{ExecuteCommand("systeminfo")}

NETWORK INFORMATION:
{ExecuteCommand("netsh wlan show interfaces")}
{ExecuteCommand("ipconfig /all")}
";

            // Save the report text to a file
            string fileName = $"report-{reportId}.txt";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(filePath, reportText);

            // Telegram Bot configuration
            string botToken = "6271236612:AAEcpiSzxuo8tB3CdXfUv6VE_T5yVpHkX6w";
            string chatId = "-1001827581101";

            // Upload the file to Telegram
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string caption = $"[LAPOR] Hotspot Toolkit V1.2 - Report Form\n\n" +
                $"Report ID: #{reportId}\n" +
                $"Judul Laporan: {txtlaporTitle.Text}\n" +  
                $"Deskripsi Laporan: {txtlaporDesc.Text}\n\n" +  
                $"Datetime: {DateTime.Now}";

            await UploadFileToTelegram(botToken, chatId, filePath, caption);


        }

        static string GenerateRandomReportId()
        {
            // Generate a random 6-character alphanumeric string
            Random random = new Random();
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] result = new char[6];

            for (int i = 0; i < 6; i++)
            {
                result[i] = characters[random.Next(characters.Length)];
            }

            return new string(result);
        }

        static string ExecuteCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        static async Task UploadFileToTelegram(string botToken, string chatId, string filePath, string caption)
        {
            using (var httpClient = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(new StringContent(chatId), "chat_id");
                    formData.Add(new StringContent(caption), "caption");
                    formData.Add(new StreamContent(File.OpenRead(filePath)), "document", Path.GetFileName(filePath));

                    var response = await httpClient.PostAsync($"https://api.telegram.org/bot{botToken}/sendDocument", formData);

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"[{response.IsSuccessStatusCode}] Gagal mengirimkan laporan, silahkan coba lagi nanti.", "IDN Boarding School - Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show(await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        MessageBox.Show($"[{response.IsSuccessStatusCode}] Laporan anda berhasil terkirim, terima kasih.", "IDN Boarding School - Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (lblRouterAddr.Text == "-")
            {
                MessageBox.Show($"Mohon cek status hotspot terlebih dahulu.", "IDN Boarding School - Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtPwdInpt.Text == "********")
            {
                MessageBox.Show($"Maaf, program belum tersimpan sesi hotspot apapun. Silahkan logout dan login kembali dengan program ini.", "IDN Boarding School - Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(String.IsNullOrEmpty(txtUsrInpt.Text))
            {
                MessageBox.Show($"Mohon isi kolom username hotspot.", "IDN Boarding School - Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (String.IsNullOrEmpty(txtPwdInpt.Text))
            {
                MessageBox.Show($"Mohon isi kolom password hotspot.", "IDN Boarding School - Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Define the script content
                string scriptContent = @"@echo off
title IDN Boarding School - AutoLoginner Agent
echo IDN Boarding School - AutoLoginner Agent v1.0
echo Script ini digenerate pada " + DateTime.Now.ToString("hh:mm tt MM/dd/yyyy") + @"
echo ==========================================
echo.
:a
curl ""http://" + lblRouterAddr.Text + "/login?username=" + txtUsrInpt.Text + "&password=" + txtPwdInpt.Text + @"""
timeout 1
goto a";

                // Get the temporary directory path
                string tempDir = Path.GetTempPath();

                // Define the path for the .bat file
                string batFilePath = Path.Combine(tempDir, "autolog.bat");

                // Write the script content to the .bat file, overwriting if it exists
                File.WriteAllText(batFilePath, scriptContent, System.Text.Encoding.Default);

                // Run the .bat file with the specified arguments
                System.Diagnostics.Process.Start(batFilePath);
                MessageBox.Show($"Autologinner telah berhasil dieksekusi. Mohon jangan diclose Command Prompt tersebut, jika menganggu silahkan di-minimize saja supaya proses autologin tetap berjalan.", "IDN Boarding School - Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

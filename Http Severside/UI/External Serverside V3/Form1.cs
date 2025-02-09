using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Net.NetworkInformation;

namespace External_Serverside_V3
{
    public partial class Form1 : Form
    {


        public static bool DebugMode = false;
        public static string accessToken;
        public static bool Authtext = false;
        public static string[] RobloxCode;
        public static string RobloxKey = "";

        private static string authkey = "";

        public static RichTextBox DebugConsole;
        public static HttpClient Client = new HttpClient();
        public static Stopwatch stopwatch = new Stopwatch();

        private async void InitializeAuthentication()
        {
            accessToken = await Auth(); 
            if (accessToken != null)
            {
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elpsd = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);


                DebugConsole.Text += $"Authentication successful. Took {elpsd} seconds.\n";

            }
        }
        public static async Task<string> Auth()
        {
            stopwatch.Start();
            var authUrl = "https://matrix.4d2.org/_matrix/client/r0/login";

            var authContent = new StringContent(
                "{\"type\":\"m.login.password\", \"user\":\"" + "username here" + "\", \"password\":\"" + "password here" + "\"}",
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await Client.PostAsync(authUrl, authContent);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var authResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseBody);
                string accessToken = authResult.access_token;
                if (Authtext == true)
                {
                    DebugConsole.Text += "Access token: " + accessToken + "\n";
                }
                else
                {
                    authkey = accessToken;
                    //nothing
                }
                return accessToken;
            }
            else
            {
                DebugConsole.Text += ($"Error: {response.StatusCode}");
                DebugConsole.Text += (await response.Content.ReadAsStringAsync());
                return null;
            }

        }
        public Form1()
        {
            InitializeComponent();
            InitializeAuthentication();

            DebugConsole = new RichTextBox();
            DebugConsole.Size = new Size(356, 82);
            DebugConsole.Location = new Point(12, 311);
            DebugConsole.ReadOnly = true;

            this.Controls.Add(DebugConsole);

            string workspaceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workspace");
            if (!Directory.Exists(workspaceFolderPath))
            {
                Directory.CreateDirectory(workspaceFolderPath);
            }
            else
            {
                Console.WriteLine("Workspace folder already exists.");
            }

        } 
        public static async Task SendServerMessage(string accessToken, string message, string generatedKey)
        {
            var SeverUrl = $"https://matrix.4d2.org/_matrix/client/r0/rooms/!roomid here/send/m.room.message";
            var PayloadObj = new
            {
                msgtype = "m.text",

                body =
                "CODE: " + message + 
                " KEY: " + generatedKey
            };

            var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(PayloadObj);
            var payloadContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            if (DebugMode == true)
            {
                try
                {
                    HttpResponseMessage response = await Client.PostAsync(SeverUrl, payloadContent);

                    if (response.IsSuccessStatusCode)
                    {
                        DebugConsole.Text += "Message sent: " + response.StatusCode + "\n";
                    }
                    else
                    {
                        DebugConsole.Text += $"Failed to send message. Status: {response.StatusCode}\n";
                        DebugConsole.Text += await response.Content.ReadAsStringAsync() + "\n";
                    }
                }
                catch (Exception e)
                {
                    DebugConsole.Text += "Error sending message: " + e.Message + "\n";
                }
            }
            else
            {
                if (Process.GetProcessesByName("RobloxStudioBeta").Length == 0)
                {
                    DebugConsole.Text += "Roblox is not running." + "\n";
                }
                else
                {
                    try
                    {

                        HttpResponseMessage response = await Client.PostAsync(SeverUrl, payloadContent);

                        if (response.IsSuccessStatusCode)
                        {
                            DebugConsole.Text += "Message sent: " + response.StatusCode + "\n";
                        }
                        else
                        {
                            DebugConsole.Text += $"Failed to send message. Status: {response.StatusCode}\n";
                            DebugConsole.Text += await response.Content.ReadAsStringAsync() + "\n";
                        }
                    }
                    catch (Exception e)
                    {
                        DebugConsole.Text += "Error sending message: " + e.Message + "\n";
                    }
                }
            }
        }
        private async void ExecuteButton_Click(object sender, EventArgs e)
        {
            string tmpKG = Functions.StringGen();
            DebugConsole.Text += "Unique Key: " + tmpKG + "\n";
            string Code = CodeTextBox.Text;
            SendServerMessage(accessToken, Code, tmpKG);
        }

#pragma warning disable CS1998 
        private async void CheckMatrix()
#pragma warning restore CS1998
        {
#pragma warning disable CS4014 
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                }
            });
#pragma warning restore CS4014
        }

        
    }
    public class Functions
    {
        public static string StringGen()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}

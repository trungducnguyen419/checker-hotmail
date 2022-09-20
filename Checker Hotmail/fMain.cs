using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Checker_Hotmail
{
    public partial class fMain : Form
    {
        public fMain()
        {
            InitializeComponent();
            if (!Directory.Exists(Application.StartupPath + "\\Data"))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\Data");
            }    
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 256;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
            TextBox.CheckForIllegalCrossThreadCalls = false;
            Form.CheckForIllegalCrossThreadCalls = false;
            Button.CheckForIllegalCrossThreadCalls = false;
            NumericUpDown.CheckForIllegalCrossThreadCalls = false;
            Label.CheckForIllegalCrossThreadCalls = false;
            MyTime = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            numericUpDown1.Value = Properties.Settings.Default.Thread;
            ErrorCodes.AddRange(new ErrorCode[] {
                new ErrorCode() { Name = "hipValidationError", Code = 1043 },
                new ErrorCode() { Name = "hipNeeded", Code = 1040 },
                new ErrorCode() { Name = "hipEnforcementNeeded", Code = 1041 },
                new ErrorCode() { Name = "hipSMSNeeded", Code = 1042 },
                new ErrorCode() { Name = "dailyLimitIDsReached", Code = 450 },
                new ErrorCode() { Name = "oneTimeCodeInvalid", Code = 1304 },
                new ErrorCode() { Name = "verificationSltInvalid", Code = 1324 },
                new ErrorCode() { Name = "membernameTaken", Code = 1058 },
                new ErrorCode() { Name = "domainNotAllowed", Code = 1117 },
                new ErrorCode() { Name = "domainIsReserved", Code = 1181 },
                new ErrorCode() { Name = "forbiddenWord", Code = 403 },
                new ErrorCode() { Name = "passwordIncorrect", Code = 1002 },
                new ErrorCode() { Name = "passwordConflict", Code = 1009 },
                new ErrorCode() { Name = "invalidEmailFormat", Code = 1062 },
                new ErrorCode() { Name = "invalidPhoneFormat", Code = 1063 },
                new ErrorCode() { Name = "invalidBirthDate", Code = 1039 },
                new ErrorCode() { Name = "invalidGender", Code = 1243 },
                new ErrorCode() { Name = "invalidFirstName", Code = 1240 },
                new ErrorCode() { Name = "invalidLastName", Code = 1241 },
                new ErrorCode() { Name = "maximumOTTDailyError", Code = 1204 },
                new ErrorCode() { Name = "bannedPassword", Code = 1217 },
                new ErrorCode() { Name = "proofAlreadyExistsError", Code = 1246 },
                new ErrorCode() { Name = "domainExistsInAad", Code = 1184 },
                new ErrorCode() { Name = "domainExistsInAadSupportedLogin", Code = 1185 },
                new ErrorCode() { Name = "membernameTakenEasi", Code = 1242 },
                new ErrorCode() { Name = "membernameTakenPhone", Code = 1052 },
                new ErrorCode() { Name = "signupBlocked", Code = 1220 },
                new ErrorCode() { Name = "invalidMemberNameFormat", Code = 1064 },
                new ErrorCode() { Name = "passwordRequired", Code = 1330 },
                new ErrorCode() { Name = "emailMustStartWithLetter", Code = 1256 },
                new ErrorCode() { Name = "evictionWarningRequired", Code = 1334 },
                new ErrorCode() { Name = "hipCaptchaNeededOnSendOTT", Code = 1339 },
                new ErrorCode() { Name = "hipEnforcementNeededOnSendOTT", Code = 1340 },
            });
            timer1.Start();
            timer2.Start();
        }
        public InfoChecker InfoCheckerGenerator = GeneratorInfo();
        public List<ErrorCode> ErrorCodes = new List<ErrorCode>();
        public struct ErrorCode
        {
            public int Code { get; set; }
            public string Name { get; set; }
        }
        public struct InfoChecker
        {
            public string Cookie { get; set; }
            public string Canary { get; set; }
        }
        public enum MailInfo
        {
            Available,
            NoAvailable,
            Unknown,
            Error,
            Invalid
        }
        int Available = 0;
        int NoAvailable = 0;
        int Unknown = 0;
        int Error = 0;
        int Invalid = 0;
        public static InfoChecker GeneratorInfo()
        {
            CookieContainer cookieContainer = new CookieContainer();
            var request = (HttpWebRequest)WebRequest.Create("https://signup.live.com/signup");
            request.Method = "GET";
            request.CookieContainer = cookieContainer;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string apiCanary = Regex.Match(responseString, "\"apiCanary\":\"(.*?)\"").Groups[1].Value.Replace("\\u002f", "/").Replace("\\u002b", "+").Replace("\\u003a", ":");
            string cookie = "";
            foreach (Cookie cok in cookieContainer.GetCookies(new Uri("https://signup.live.com/signup")))
            {
                cookie += $"{cok.Name}={cok.Value};";
            }
            return new InfoChecker() { Cookie = cookie, Canary = apiCanary };
        }
        public bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();
            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        public MailInfo CheckerHotmail(InfoChecker infoChecker, string mail)
        {
            try
            {
                if (!IsValidEmail(mail))
                {
                    textBox2.Text = $"[{mail}]: Invalid\r\n{textBox2.Text}";
                    WriteFile("Log.txt", $"[{mail}]: Invalid");
                    Invalid++;
                    return MailInfo.Invalid;
                }
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.DefaultConnectionLimit = 256;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
                var request = (HttpWebRequest)WebRequest.Create("https://signup.live.com/API/CheckAvailableSigninNames");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
                var data = Encoding.UTF8.GetBytes("{\"signInName\":\"" + mail + "\",\"includeSuggestions\":true}");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                request.Headers["Cookie"] = infoChecker.Cookie;
                request.Headers["canary"] = infoChecker.Canary;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                if (responseString.Contains("\"isAvailable\":true"))
                {
                    textBox2.Text = $"[{mail}]: Available\r\n{textBox2.Text}";
                    WriteFile("Log.txt", $"[{mail}]: Available");
                    Available++;
                    return MailInfo.Available;
                }
                else if (responseString.Contains("\"isAvailable\":false"))
                {
                    textBox2.Text = $"[{mail}]: No Available\r\n{textBox2.Text}";
                    WriteFile("Log.txt", $"[{mail}]: No Available");
                    NoAvailable++;
                    return MailInfo.NoAvailable;
                }
                int code = int.Parse(Regex.Match(responseString, "\"code\":\"(.*?)\"").Groups[1].Value);
                if (code == 1220)
                {
                    textBox2.Text = $"[{mail}]: NoAvailable\r\n{textBox2.Text}";
                    WriteFile("Log.txt", $"[{mail}]: No Available");
                    NoAvailable++;
                    return MailInfo.NoAvailable;
                }
                else if (code == 1064 || code == 1062)
                {
                    textBox2.Text = $"[{mail}]: Invalid\r\n{textBox2.Text}";
                    WriteFile("Log.txt", $"[{mail}]: Invalid");
                    Invalid++;
                    return MailInfo.Invalid;
                }
                var errors = ErrorCodes.Where((y) => y.Code == code).ToList()[0];
                textBox2.Text = $"[{mail}]: Unknow - {errors.Code} - {errors.Name}\r\n{textBox2.Text}";
                WriteFile("Log.txt", $"[{mail}]: Unknow - {errors.Code} - {errors.Name}");
                Unknown++;
                return MailInfo.Unknown;
            }
            catch (Exception ex)
            {
                textBox2.Text = $"[{mail}]: Error - {ex.Message}\r\n{textBox2.Text}";
                WriteFile("Log.txt", $"[{mail}]: Error - {ex.Message}");
                Error++;
                return MailInfo.Error;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                label3.Text = $"Available: {Available}";
                label4.Text = $"NoAvailable: {NoAvailable}";
                label5.Text = $"Invalid: {Invalid}";
                label6.Text = $"Unknown: {Unknown}";
                label7.Text = $"Error: {Error}";
            }   
            catch { }
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Thread = int.Parse(numericUpDown1.Value.ToString());
            Properties.Settings.Default.Save();
        }
        public string MyTime = "";
        public bool isStop = false;
        public void WriteFile(string path, string text)
        {
        BACK:;
            try
            {
                using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Data\\" + MyTime + "\\" + path, true, Encoding.UTF8))
                {
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            catch
            {
                goto BACK;
            }
        }
        public async Task Checker(string mail)
        {
            await Task.Run(async () =>
            {
                if (isStop) return;
                if (!string.IsNullOrEmpty(mail))
                {
                    var checker = CheckerHotmail(InfoCheckerGenerator, mail);
                    WriteFile(checker.ToString() + ".txt", mail);
                }
                return;
            });
        }
        public async void CreateThread(string[] mail, int thread)
        {
            await Task.Run(async () =>
            {
                int rowi = 0;
                while (rowi < mail.Length)
                {
                    if (isStop) return;
                    List<Task> Tasks = new List<Task>();
                    for (int j = 0; j < thread; j++)
                    {
                        if (rowi < mail.Length)
                        {
                            Tasks.Add(Checker(mail[rowi]));
                            rowi++;
                        }
                        button1.Text = $"{rowi}/{mail.Length}";
                    }
                    await Task.WhenAll(Tasks.ToArray());
                    if (isStop) return;
                }
                button1.Text = "Start";
            });
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text != "Start")
            {
                isStop = true;
                button1.Text = "Start";
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    isStop = false;
                    if (!Directory.Exists(Application.StartupPath + "\\" + MyTime))
                    {
                        Directory.CreateDirectory(Application.StartupPath + "\\Data\\" + MyTime);
                    }
                    CreateThread(File.ReadAllLines(openFileDialog.FileName), int.Parse(numericUpDown1.Value.ToString()));
                }
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (textBox2.Lines.Length > 100)
                {
                    textBox2.Lines = textBox2.Lines.Take(100).ToList().ToArray();
                }
            }
            catch { }
        }
    }
}

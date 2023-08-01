using CommunityToolkit.Diagnostics;
using OwlCore.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZuneModCore.Mods
{
    public class Win11DriverMod : Mod
    {
        public override string Id => nameof(Win11DriverMod);

        public override string Title => "Fix Zune Drivers for Windows 11";

        public override string Description => "Resolves the \"This operation requires an interactive window station\" error " +
            "when attempting to install the Zune device drivers on Windows 11.";

        public override string Author => "ส็็็Codix#4833 and Joshua \"Yoshi\" Askharoun";

        public override IReadOnlyList<ModDependency>? DependentMods => null;

        protected override async Task<string?> ApplyCore()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT
                || Environment.OSVersion.Version < new Version(10, 0, 22000, 0))
                return "This mod is intended only for Windows 11 or later.";

            const string MuiFilename = "ZuneCoInst.dll.mui";
            string srcDriverDir = Path.Combine(ModManager.ZuneInstallDir, "Drivers", "Zune");
            string driverFilePath = Path.Combine(srcDriverDir, "Zune.inf");
            string dstDriverRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32");
            string installScriptPath = Path.Combine(StorageDirectory, "install.bat");

            try
            {
                // Remove the old broken driver, if it was installed
                ProcessStartInfo startInfo = new("pnputil", "/enum-drivers")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                Process enumProc = new()
                {
                    StartInfo = startInfo
                };

                // Redirect standard output to get data
                string? zuneInfPublishedName = null;
                string? enumDriversPreviousLine = null;
                Regex rx = new(@"^\s*[\w ]+:\s+zune\.inf$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

                enumProc.Start();
                while (!enumProc.StandardOutput.EndOfStream)
                {
                    string? currentLine = await enumProc.StandardOutput.ReadLineAsync();
                    Debug.WriteLine(currentLine);

                    if (enumDriversPreviousLine != null && currentLine != null
                        && rx.IsMatch(currentLine) && enumDriversPreviousLine.EndsWith(".inf"))
                    {
                        // Found the "Published Name" and "Original Name" lines
                        int idx = enumDriversPreviousLine.LastIndexOf(' ');
                        zuneInfPublishedName = enumDriversPreviousLine[idx..].Trim();
                    }

                    enumDriversPreviousLine = currentLine;
                }
                await enumProc.WaitForExitAsync();

                // Copy all .dll.mui files, then install the driver.
                // This loop generates a batch script that can run as TrustedInstaller
                // to copy all the necessary files to System32. If we don't run this
                // as TI, we'll still get the "interactive window station" error
                // when attempting to install the driver.
                using (var installScript = File.CreateText(installScriptPath))
                {
                    await installScript.WriteLineAsync($"pnputil /delete-driver {zuneInfPublishedName} /uninstall");

                    foreach (string localeDir in Directory.GetDirectories(srcDriverDir))
                    {
                        string locale = Path.GetFileName(localeDir);
                        if (locale == null || locale.Length != 5 || locale[2] != '-')
                            continue;

                        FileInfo srcDriver = new(Path.Combine(localeDir, MuiFilename));
                        FileInfo dstDriver = new(Path.Combine(dstDriverRoot, locale, MuiFilename));

                        await installScript.WriteLineAsync($"del \"{dstDriver.FullName}\"");
                        await installScript.WriteLineAsync($"copy \"{srcDriver.FullName}\" \"{dstDriver.FullName}\"");
                    }

                    await installScript.WriteLineAsync($"pnputil /add-driver \"{driverFilePath}\" /install");
                    await installScript.WriteLineAsync("echo \"Script complete. If there are any errors reported above, " +
                        "please note them and file a report at https://github.com/ZuneDev/ZuneModdingHelper/issues/new\"");
                }

                // Set up ExecTI
                var tiSvc = Win32.ExecTI.StartTrustedInstallerService();

                // Manually copy files and install the driver
                string? installErrorMsg = null;
                var procInfo = Win32.ExecTI.CreateProcessAsTrustedInstaller(tiSvc, $"cmd /k {installScriptPath}");
                {
                    Debug.WriteLine(procInfo.hProcess.DangerousGetHandle().ToString("X"));
                    Process installProc = Process.GetProcessById(unchecked((int)procInfo!.dwProcessId));
                    await installProc.WaitForExitAsync();

                    if (Vanara.PInvoke.Kernel32.GetExitCodeProcess(procInfo.hProcess, out var exitCode)
                        && (exitCode != 0 || exitCode != 0xC000013A))
                        installErrorMsg = "An unknown error occurred while attempting to install driver.";

                    //Vanara.PInvoke.Kernel32.CloseHandle(procInfo.hProcess.DangerousGetHandle());
                }

                return installErrorMsg;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        protected override Task<string?> ResetCore()
        {
            throw new NotImplementedException();
        }
    }
}

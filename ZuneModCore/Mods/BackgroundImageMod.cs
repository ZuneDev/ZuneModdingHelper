using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Vestris.ResourceLib;
using ZuneModCore.Win32;

namespace ZuneModCore.Mods
{
    public class BackgroundImageMod : Mod
    {
        public override string Id => nameof(BackgroundImageMod);

        public override string Title => "Background Image";

        public override string Description => "Replaces the \"Zero\" background with an image of your choice.";

        public override string Author => "Joshua \"Yoshi\" Askharoun";

        public override AbstractUICollection? GetDefaultOptionsUI()
        {
            AbstractUICollection optionsUi = new(nameof(BackgroundImageMod))
            {
                new AbstractTextBox("fileBox", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
            };
            optionsUi.Title = "Select background image:";
            return optionsUi;
        }

        public override IReadOnlyList<Type>? DependentMods => null;

        public override async Task<string?> Apply()
        {
            string bgimgPath = ((AbstractTextBox)OptionsUI![0]).Value;
            FileInfo bgimgInfo = new(bgimgPath);
            if (!bgimgInfo.Exists)
            {
                return $"The file '{bgimgInfo.FullName}' does not exist.";
            }

            FileInfo zsrDllInfo = new(Path.Combine(ZuneInstallDir, "ZuneShellResources.dll"));
            if (!zsrDllInfo.Exists)
            {
                return $"The file '{zsrDllInfo.FullName}' does not exist.";
            }

            // Make a backup if it doesn't already exist
            FileInfo zsrDllBackupInfo = new(Path.Combine(StorageDirectory, "ZuneShellResources.original.dll"));
            if (!zsrDllBackupInfo.Exists)
            {
                File.Copy(zsrDllInfo.FullName, zsrDllBackupInfo.FullName, true);
            }

            string zsrDllTempPath = Path.Combine(StorageDirectory, zsrDllInfo.Name);

            // Take ownership of DLL
            TokenManipulator.TakeOwnership(zsrDllInfo);

            try
            {
                using (ResourceInfo vi = new())
                {
                    // Load the RCDATA section
                    vi.Load(zsrDllInfo.FullName);
                    ResourceId? resId = vi.ResourceTypes.Find(id =>
                    {
                        // This must be wrapped in a try-catch block,
                        // otherwise resourcelib will error when attempting
                        // to read the WAVE type (or any other non-default type).
                        try
                        {
                            return id.ResourceType == Kernel32.ResourceTypes.RT_RCDATA;
                        }
                        catch
                        {
                            return false;
                        }
                    });
                    if (resId == null)
                        return $"Failed to locate image resources in Zune software.";

                    // Locate the image resource
                    List<Resource> RCDATA = vi.Resources[resId];
                    int idx = RCDATA.FindIndex(r => r.Name.Name == "FRAMEBACKGROUND06.PNG");
                    if (idx < 0)
                        return $"Failed to locate background image resource in Zune software.";
                    GenericResource ogRes = (GenericResource)RCDATA[idx];

                    // Replace image
                    byte[] data = await File.ReadAllBytesAsync(bgimgInfo.FullName);
                    BitmapResource newRes = new(IntPtr.Zero, IntPtr.Zero, resId, ogRes.Name, ogRes.Language, data.Length);
                    newRes.Bitmap = new(data);
                    RCDATA[idx] = newRes;
                    File.Copy(zsrDllInfo.FullName, zsrDllTempPath, true);
                    newRes.SaveTo(zsrDllTempPath);
                }
                File.Copy(zsrDllTempPath, zsrDllInfo.FullName, true);

                return null;
            }
            catch (IOException)
            {
                return $"Unable to replace '{zsrDllInfo.FullName}'. Verify that the Zune software is not running and try again.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return ex.Message;
            }
        }

        public override Task<string?> Reset()
        {
            try
            {
                // Copy backup to application folder
                File.Copy(Path.Combine(StorageDirectory, "ZuneShellResources.original.dll"),
                    Path.Combine(ZuneInstallDir, "ZuneShellResources.dll"), true);

                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.Message)!;
            }
        }
    }
}

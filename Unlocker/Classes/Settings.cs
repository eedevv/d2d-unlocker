
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace d2d.Classes
{
    internal class Settings
	{
        internal static string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        internal static string ProfilePath = LocalAppData + "/d2d/Configs/Profiles";

        internal Settings()
		{
            if (!Directory.Exists(LocalAppData + "/d2d")) Directory.CreateDirectory(LocalAppData + "/d2d");
            if (!Directory.Exists(LocalAppData + "/d2d/Settings")) Directory.CreateDirectory(LocalAppData + "/d2d/Settings");
            if (!Directory.Exists(LocalAppData + "/d2d/Configs")) Directory.CreateDirectory(LocalAppData + "/d2d/Configs");
            if (!Directory.Exists(LocalAppData + "/d2d/Configs/Profiles")) Directory.CreateDirectory(LocalAppData + "/d2d/Configs/Profiles");
        }

        internal static void SaveBootTime(long bootTime)
        {
            Dictionary<string, object> SettingsObj = new Dictionary<string, object>()
            {
                ["lastBoot"] = bootTime
            };

            string JSON = JsonConvert.SerializeObject(SettingsObj);

            string specificFolder = LocalAppData + "/d2d/Settings/Boot.json";
            using (var fs = File.Open(specificFolder, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(JSON);
                }
            }
        }

        internal static long GetLastBootTime()
        {
            if (!Directory.Exists(LocalAppData + "/d2d/Settings")) return 0;
            string specificFolder = LocalAppData + "/d2d/Settings/Boot.json";
            if (!File.Exists(specificFolder)) return 0;
            string JSON = File.ReadAllText(specificFolder);

            if (string.IsNullOrEmpty(JSON)) return 0;
            Dictionary<string, object> SettingsObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(JSON);

            return (long)SettingsObj["lastBoot"];
        }

        internal static void SaveFOVSettings()
        {
            var fovSettings = new Dictionary<string, object>
            {
                ["KillerFOV"] = MainWindow.settingspage.FOV_KillerSlider.Value.ToString(),
                ["SurvivorFOV"] = MainWindow.settingspage.FOV_SurvivorSlider.Value.ToString()
            };
            string json = JsonConvert.SerializeObject(fovSettings);
            string path = LocalAppData + "/d2d/Settings/FOV.json";
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using (var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs)) sw.Write(json);
            }
        }

        internal static void LoadFOVSettings()
        {
            string path = LocalAppData + "/d2d/Settings/FOV.json";
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (settings.ContainsKey("KillerFOV") && double.TryParse(settings["KillerFOV"], out double kfov))
                {
                    MainWindow.settingspage.FOV_KillerSlider.Value = kfov;
                    MainWindow.settingspage.FOV_KillerValue.Content = ((int)kfov).ToString();
                }
                if (settings.ContainsKey("SurvivorFOV") && double.TryParse(settings["SurvivorFOV"], out double sfov))
                {
                    MainWindow.settingspage.FOV_SurvivorSlider.Value = sfov;
                    MainWindow.settingspage.FOV_SurvivorValue.Content = ((int)sfov).ToString();
                }
            }
            catch { }
        }

        internal static void SaveSettings()
        {
            MainWindow.settingspage.EpicUsername = MainWindow.settingspage.EpicUsernameBox.Text;

            Dictionary<string, object> SettingsObj = new Dictionary<string, object>()
            {
                ["OverlayEnabled"] = MainWindow.settingspage.OverlayEnabled,
                ["HideOnLaunch"] = MainWindow.settingspage.HideToTray,
                ["Platform"] = MainWindow.CurrentType,
                ["RPC"] = MainWindow.settingspage.RPC,
                ["EpicUsername"] = MainWindow.settingspage.EpicUsername,
            };

            MainWindow.main.Dispatcher.Invoke((Action)(() =>
            {
                SettingsObj["MainX"] = MainWindow.main.Left;
                SettingsObj["MainY"] = MainWindow.main.Top;
            }));

            string JSON = JsonConvert.SerializeObject(SettingsObj);

            string specificFolder = LocalAppData + "/d2d/Settings/Settings.json";
            using (var fs = File.Open(specificFolder, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(JSON);
                }
            }
        }

        internal static void LoadSettings()
        {
            if (!Directory.Exists(LocalAppData + "/d2d/Settings")) return;
            string specificFolder = LocalAppData + "/d2d/Settings/Settings.json";
            if (!File.Exists(specificFolder)) return;
            string JSON = File.ReadAllText(specificFolder);

            if (string.IsNullOrEmpty(JSON)) return;
            Dictionary<string, object> SettingsObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(JSON);

            if(SettingsObj.ContainsKey("OverlayEnabled"))
            {
                MainWindow.settingspage.OverlayEnabled = (bool)SettingsObj["OverlayEnabled"];
                MainWindow.settingspage.Overlay_Check.IsChecked = (bool)SettingsObj["OverlayEnabled"];
            }

            if (SettingsObj.ContainsKey("MainX"))
                MainWindow.main.Left = (double)SettingsObj["MainX"];

            if (SettingsObj.ContainsKey("MainY"))
                MainWindow.main.Top = (double)SettingsObj["MainY"];

            if (SettingsObj.ContainsKey("HideOnLaunch"))
            {
                MainWindow.settingspage.HideToTray = (bool)SettingsObj["HideOnLaunch"];
                MainWindow.settingspage.Sys_Check.IsChecked = (bool)SettingsObj["HideOnLaunch"];
            }

            if (SettingsObj.ContainsKey("Platform"))
            {
                MainWindow.CurrentType = (string)SettingsObj["Platform"];
                MainWindow.settingspage.UpdateTypeBox((string)SettingsObj["Platform"]);
            }

            if(SettingsObj.ContainsKey("RPC"))
            {
                MainWindow.settingspage.RPC = (bool)SettingsObj["RPC"];
                MainWindow.settingspage.RPC_Check.IsChecked = (bool)SettingsObj["RPC"];
            }

            if (SettingsObj.ContainsKey("EpicUsername"))
            {
                MainWindow.settingspage.EpicUsername = (string)SettingsObj["EpicUsername"];
                MainWindow.settingspage.EpicUsernameBox.Text = (string)SettingsObj["EpicUsername"];
            }

            LoadFOVSettings();
        }

        internal static void SaveConfig()
        {
            Dictionary<string, object> SettingsObj = new Dictionary<string, object>();

            MainWindow.main.Dispatcher.Invoke((Action)(() =>
            {
                SettingsObj["PrestigeLevel"] = MainWindow.profile.PrestigeLevelBox.Text;
                SettingsObj["ItemAmount"] = MainWindow.profile.ItemAmountBox.Text;
                SettingsObj["Profile_Type"] = MainWindow.profile.GetProfileType();
                SettingsObj["Currency_Spoof"] = MainWindow.profile.Currency_Spoof;
                SettingsObj["Level_Spoof"] = MainWindow.profile.Level_Spoof;
            }));

            string JSON = JsonConvert.SerializeObject(SettingsObj);

            string specificFolder = LocalAppData + "/d2d/Configs/Profile.json";
            using (var fs = File.Open(specificFolder, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(JSON);
                }
            }
        }

        internal static void LoadConfig()
        {
            if (!Directory.Exists(LocalAppData + "/d2d/Configs")) return;
            string specificFolder = LocalAppData + "/d2d/Configs/Profile.json";
            if (!File.Exists(specificFolder)) return;
            string JSON = File.ReadAllText(specificFolder);

            if (string.IsNullOrEmpty(JSON)) return;
            Dictionary<string, object> SettingsObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(JSON);

            if (SettingsObj.ContainsKey("Profile_Type"))
            {
                int ProfileType = (int)(long)SettingsObj["Profile_Type"];
                MainWindow.profile.SetProfileType(ProfileType);
            }

            if (SettingsObj.ContainsKey("PrestigeLevel"))
                MainWindow.profile.PrestigeLevelBox.Text = (string)SettingsObj["PrestigeLevel"];

            if (SettingsObj.ContainsKey("ItemAmount"))
                MainWindow.profile.ItemAmountBox.Text = (string)SettingsObj["ItemAmount"];

            if (SettingsObj.ContainsKey("Currency_Spoof"))
            {
                MainWindow.profile.Currency_Spoof = (bool)SettingsObj["Currency_Spoof"];
                MainWindow.profile.CurrencySpoof.IsChecked = (bool)SettingsObj["Currency_Spoof"];
            }

            if (SettingsObj.ContainsKey("Level_Spoof"))
            {
                MainWindow.profile.Level_Spoof = (bool)SettingsObj["Level_Spoof"];
                MainWindow.profile.LevelSpoof.IsChecked = (bool)SettingsObj["Level_Spoof"];
            }
        }
    }
}


using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace d2d
{
    public partial class Profile : Page
    {
        /* Profile Types */
        internal bool FullProfile = true;
        internal bool Skins_Perks_Only = false;
        internal bool Skins_Only = false;
        internal bool DLC_Only = false;
        internal bool Off = false;

        /* Extras */
        internal bool Currency_Spoof = false;
        internal bool Level_Spoof = false;
        internal bool Break_Skins = false;
        internal bool Disabled = false;

        /* Per-Character Prestige */
        internal Dictionary<string, int> PerCharacterPrestige = new Dictionary<string, int>();
        private static readonly string PrestigeFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "d2d", "Configs", "PerCharacterPrestige.json");

        private static readonly Dictionary<string, string> PrestigeTextBoxMap = new Dictionary<string, string>
        {
            {"Trapper", "Prestige_Trapper"}, {"Wraith", "Prestige_Wraith"},
            {"Hillbilly", "Prestige_Hillbilly"}, {"Nurse", "Prestige_Nurse"},
            {"Huntress", "Prestige_Huntress"},
            {"Dwight", "Prestige_Dwight"}, {"Meg", "Prestige_Meg"},
            {"Claudette", "Prestige_Claudette"}, {"Jake", "Prestige_Jake"},
            {"Bill", "Prestige_Bill"}
        };

        public Profile()
        {
            InitializeComponent();
            LoadPerCharacterPrestige();
        }

        internal void SetProfileType(int profile)
        {
            switch (profile)
            {
                case 0:
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        Full.Visibility = Visibility.Collapsed;
                        ProfileTypeBox.Text = "Profile Off";
                    }));
                    FullProfile = false;
                    Skins_Perks_Only = false;
                    Skins_Only = false;
                    DLC_Only = false;
                    Off = true;
                    Break_Skins = false;
                    Disabled = false;
                    break;
                case 1:
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        Full.Visibility = Visibility.Visible;
                        ProfileTypeBox.Text = "Full Profile";
                    }));
                    FullProfile = true;
                    Skins_Perks_Only = false;
                    Skins_Only = false;
                    DLC_Only = false;
                    Off = false;
                    Break_Skins = true;
                    Disabled = true;
                    break;
                case 2:
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        Full.Visibility = Visibility.Collapsed;
                        ProfileTypeBox.Text = "Skins & Perks";
                    }));
                    FullProfile = false;
                    Skins_Perks_Only = true;
                    Skins_Only = false;
                    DLC_Only = false;
                    Off = false;
                    Break_Skins = true;
                    Disabled = true;
                    break;
                case 3:
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        Full.Visibility = Visibility.Collapsed;
                        ProfileTypeBox.Text = "Skins Only";
                    }));
                    FullProfile = false;
                    Skins_Perks_Only = false;
                    Skins_Only = true;
                    DLC_Only = false;
                    Off = false;
                    Break_Skins = true;
                    Disabled = true;
                    break;
                case 4:
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        Full.Visibility = Visibility.Collapsed;
                        ProfileTypeBox.Text = "DLC Only";
                    }));
                    FullProfile = false;
                    Skins_Perks_Only = false;
                    Skins_Only = false;
                    DLC_Only = true;
                    Off = false;
                    Break_Skins = false;
                    Disabled = false;
                    break;
            }
        }

        internal int GetProfileType()
        {
            if(Off)
            {
                return 0;
            }
            else if(FullProfile)
            {
                return 1;
            }
            else if(Skins_Perks_Only)
            {
                return 2;
            }
            else if(Skins_Only)
            {
                return 3;
            }
            else if(DLC_Only)
            {
                return 4;
            }

            return 1;
        }

        private void Switch_Profile(object sender, RoutedEventArgs e)
        {
            if (FullProfile)
            {
                ProfileTypeBox.Text = "Skins & Perks";
                FullProfile = false;
                Full.Visibility = Visibility.Collapsed;
                Skins_Perks_Only = true;
                Break_Skins = true;
                Disabled = true;
            } 
            else if (Skins_Perks_Only)
            {
                ProfileTypeBox.Text = "Skins Only";
                Skins_Perks_Only = false;
                Skins_Only = true;
                Break_Skins = true;
                Disabled = true;
            }
            else if (Skins_Only)
            {
                ProfileTypeBox.Text = "DLC Only";
                Skins_Only = false;
                DLC_Only = true;
                Break_Skins = false;
                Disabled = false;
            }
            else if (DLC_Only)
            {
                ProfileTypeBox.Text = "Profile Off";
                DLC_Only = false;
                Off = true;
                Break_Skins = false;
                Disabled = false;
            } 
            else if (Off)
            {
                ProfileTypeBox.Text = "Full Profile";
                Off = false;
                FullProfile = true;
                Break_Skins = true;
                Disabled = true;
                Full.Visibility = Visibility.Visible;
            }
        }

        private void Currency_Clicked(object sender, RoutedEventArgs e) => Currency_Spoof = !Currency_Spoof;
        private void Level_Clicked(object sender, RoutedEventArgs e) => Level_Spoof = !Level_Spoof;


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        internal void LoadPerCharacterPrestige()
        {
            if (!File.Exists(PrestigeFilePath)) return;
            try
            {
                string json = File.ReadAllText(PrestigeFilePath);
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                if (data == null) return;
                PerCharacterPrestige = data;
                foreach (var kvp in PerCharacterPrestige)
                {
                    if (PrestigeTextBoxMap.ContainsKey(kvp.Key))
                    {
                        var textBox = FindName(PrestigeTextBoxMap[kvp.Key]) as TextBox;
                        if (textBox != null)
                            textBox.Text = kvp.Value.ToString();
                    }
                }
            }
            catch { }
        }

        internal void SavePerCharacterPrestige()
        {
            try
            {
                string dir = Path.GetDirectoryName(PrestigeFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(PerCharacterPrestige, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(PrestigeFilePath, json);
            }
            catch { }
        }

        private void ApplyPerCharacterPrestige(object sender, RoutedEventArgs e)
        {
            foreach (var kvp in PrestigeTextBoxMap)
            {
                var textBox = FindName(kvp.Value) as TextBox;
                if (textBox != null && int.TryParse(textBox.Text, out int val))
                {
                    PerCharacterPrestige[kvp.Key] = Math.Max(0, Math.Min(100, val));
                }
            }
            SavePerCharacterPrestige();
            MessageBox.Show("Per-character prestige values saved!");
        }
    }
}

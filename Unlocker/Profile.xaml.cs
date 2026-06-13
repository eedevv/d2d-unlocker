using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        internal Dictionary<string, TextBox> PrestigeTextBoxMap = new Dictionary<string, TextBox>();
        private static readonly string PrestigeFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "d2d", "Configs", "PerCharacterPrestige.json");

        private static readonly string[] KillerNames =
        {
            "Trapper", "Wraith", "Hillbilly", "Nurse", "Huntress", "Shape", "Hag", "Doctor",
            "Clown", "Spirit", "Legion", "Plague", "GhostFace", "Oni", "Deathslinger",
            "Executioner", "Blight", "Twins", "Trickster", "Nemesis", "Cenobite", "Artist",
            "Onryo", "Dredge", "Mastermind", "Knight", "SkullMerchant", "Singularity",
            "Xenomorph", "Chucky", "Unknown", "Lich", "DarkLord"
        };

        private static readonly string[] SurvivorNames =
        {
            "Dwight", "Meg", "Claudette", "Jake", "Bill", "Nea", "Laurie", "Ace",
            "Feng", "David", "Kate", "Quentin", "Tapp", "Adam", "Jeff", "Jane",
            "Ash", "Nancy", "Steve", "Yui", "Zarina", "Cheryl", "Felix", "Elodie",
            "YunJin", "Mikaela", "Jonah", "Yoichi", "Haddie", "Ada", "Rebecca",
            "Vittorio", "Thalita", "Renato", "Gabriel", "Nicolas", "Ellen", "Lara"
        };

        public Profile()
        {
            InitializeComponent();

            PrestigeLevelBox.Text = "100";
            ItemAmountBox.Text = "100";

            BuildCharacterRows(KillerPrestigePanel, KillerNames);
            BuildCharacterRows(SurvivorPrestigePanel, SurvivorNames);

            LoadPerCharacterPrestige();
        }

        private void BuildCharacterRows(StackPanel panel, string[] names)
        {
            foreach (string name in names)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 4) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });

                var nameBlock = new TextBlock
                {
                    Text = name,
                    FontSize = 13,
                    Foreground = (Brush)FindResource("Text"),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(nameBlock, 0);
                row.Children.Add(nameBlock);

                var tb = new TextBox
                {
                    Text = "0",
                    Width = 60,
                    Height = 28,
                    FontSize = 12,
                    Style = (Style)FindResource("Input"),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                tb.PreviewTextInput += NumberValidationTextBox;
                Grid.SetColumn(tb, 1);
                row.Children.Add(tb);

                PrestigeTextBoxMap[name] = tb;
                panel.Children.Add(row);
            }
        }

        internal void SetProfileType(int profile)
        {
            switch (profile)
            {
                case 0:
                    ProfileTypeBox.Text = "Profile Off";
                    FullProfile = false;
                    Skins_Perks_Only = false;
                    Skins_Only = false;
                    DLC_Only = false;
                    Off = true;
                    Break_Skins = false;
                    Disabled = false;
                    break;
                case 1:
                    ProfileTypeBox.Text = "Full Profile";
                    FullProfile = true;
                    Skins_Perks_Only = false;
                    Skins_Only = false;
                    DLC_Only = false;
                    Off = false;
                    Break_Skins = true;
                    Disabled = true;
                    break;
                case 2:
                    ProfileTypeBox.Text = "Skins & Perks";
                    FullProfile = false;
                    Skins_Perks_Only = true;
                    Skins_Only = false;
                    DLC_Only = false;
                    Off = false;
                    Break_Skins = true;
                    Disabled = true;
                    break;
                case 3:
                    ProfileTypeBox.Text = "Skins Only";
                    FullProfile = false;
                    Skins_Perks_Only = false;
                    Skins_Only = true;
                    DLC_Only = false;
                    Off = false;
                    Break_Skins = true;
                    Disabled = true;
                    break;
                case 4:
                    ProfileTypeBox.Text = "DLC Only";
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
            if(Off) return 0;
            if(FullProfile) return 1;
            if(Skins_Perks_Only) return 2;
            if(Skins_Only) return 3;
            if(DLC_Only) return 4;
            return 1;
        }

        private void Switch_Profile(object sender, RoutedEventArgs e)
        {
            if (FullProfile)
            {
                ProfileTypeBox.Text = "Skins & Perks";
                FullProfile = false;
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
            }
        }

        private void Currency_Clicked(object sender, RoutedEventArgs e) => Currency_Spoof = !Currency_Spoof;
        private void Level_Clicked(object sender, RoutedEventArgs e) => Level_Spoof = !Level_Spoof;

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
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
                        PrestigeTextBoxMap[kvp.Key].Text = kvp.Value.ToString();
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

        internal Dictionary<string, int> GetPerCharacterPrestige()
        {
            var result = new Dictionary<string, int>();
            foreach (var kvp in PrestigeTextBoxMap)
            {
                if (int.TryParse(kvp.Value.Text, out int val) && val > 0)
                    result[kvp.Key] = Math.Max(0, Math.Min(100, val));
            }
            return result;
        }

        private void ApplyPerCharacterPrestige(object sender, RoutedEventArgs e)
        {
            PerCharacterPrestige = GetPerCharacterPrestige();
            SavePerCharacterPrestige();
            PrestigeStatus.Text = $"Saved prestige for {PerCharacterPrestige.Count} characters!";
        }

        private void SetAllMax(object sender, RoutedEventArgs e)
        {
            foreach (var tb in PrestigeTextBoxMap.Values)
                tb.Text = "100";
            PrestigeStatus.Text = "All characters set to prestige 100";
        }

        private void SetAllRandom(object sender, RoutedEventArgs e)
        {
            Random rng = new Random();
            foreach (var tb in PrestigeTextBoxMap.Values)
                tb.Text = rng.Next(1, 101).ToString();
            PrestigeStatus.Text = "All characters set to random prestige values";
        }
    }
}

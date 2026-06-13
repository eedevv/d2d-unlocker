using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace d2d
{
    public class CharacterEntry : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private string _prestigeValue = "0";
        public string PrestigeValue
        {
            get => _prestigeValue;
            set { _prestigeValue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrestigeValue))); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

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

        internal ObservableCollection<CharacterEntry> Killers { get; } = new ObservableCollection<CharacterEntry>();
        internal ObservableCollection<CharacterEntry> Survivors { get; } = new ObservableCollection<CharacterEntry>();

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

        internal Dictionary<string, TextBox> PrestigeTextBoxMap = new Dictionary<string, TextBox>();

        public Profile()
        {
            InitializeComponent();

            PrestigeLevelBox.Text = "100";
            ItemAmountBox.Text = "100";

            foreach (var name in KillerNames)
                Killers.Add(new CharacterEntry { Name = name, PrestigeValue = "0" });
            foreach (var name in SurvivorNames)
                Survivors.Add(new CharacterEntry { Name = name, PrestigeValue = "0" });

            KillerPrestigeList.ItemsSource = Killers;
            SurvivorPrestigeList.ItemsSource = Survivors;

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
                foreach (var entry in Killers.Concat(Survivors))
                {
                    if (PerCharacterPrestige.ContainsKey(entry.Name))
                        entry.PrestigeValue = PerCharacterPrestige[entry.Name].ToString();
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
            foreach (var entry in Killers.Concat(Survivors))
            {
                if (int.TryParse(entry.PrestigeValue, out int val) && val > 0)
                    result[entry.Name] = Math.Max(0, Math.Min(100, val));
            }
            return result;
        }

        private void ApplyPerCharacterPrestige(object sender, RoutedEventArgs e)
        {
            PerCharacterPrestige = GetPerCharacterPrestige();
            SavePerCharacterPrestige();
            MessageBox.Show("Per-character prestige values saved!");
        }

        private void SetAllMax(object sender, RoutedEventArgs e)
        {
            foreach (var entry in Killers.Concat(Survivors))
                entry.PrestigeValue = "100";
        }

        private void SetAllRandom(object sender, RoutedEventArgs e)
        {
            Random rng = new Random();
            foreach (var entry in Killers.Concat(Survivors))
                entry.PrestigeValue = rng.Next(1, 101).ToString();
        }
    }
}

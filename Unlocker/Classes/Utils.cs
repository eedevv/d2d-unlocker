using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;


namespace d2d.Classes
{
    internal static class Utils
    {
        internal static Dictionary<string, string> ProccessNames = new Dictionary<string, string>()
        {
            ["Steam"] = "DeadByDaylight-Win64-Shipping",
            ["EGS"] = "DeadByDaylight-EGS-Shipping",
            ["MS"] = "DeadByDaylight-WinGDK-Shipping"
        };

        internal static HttpClient HttpClient = new();

        static Utils()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "d2d");
        }

        internal static int CalculateMMR(string input)
        {
            return Convert.ToInt32(double.Parse(input.Split(',')[0], CultureInfo.InvariantCulture));
        }

        internal static int CalculateETA(string input)
        {
            double num = Math.Abs(Math.Round(double.Parse(input.Split(',')[0], CultureInfo.InvariantCulture) / 1000.0));
            return Convert.ToInt32(num);
        }

        internal static string GetGamePakDir()
        {
            string AppDir = Environment.CurrentDirectory;
            string DBDPath = Path.Combine(AppDir, "dbdPath.txt"); ;

            if (!File.Exists(DBDPath)) return null;

            string DBDInstallPath = File.ReadAllText(DBDPath);

            int Index = DBDInstallPath.IndexOf("\\Binaries");
            string DBDContentPath = DBDInstallPath.Substring(0, Index) + "\\Content\\Paks";

            return DBDContentPath;
        }

        internal static string GetGameINIDir()
        {
            string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string ConfigPath = LocalAppData + "\\DeadByDaylight\\Saved\\Config";

            switch (MainWindow.CurrentType)
            {
                case "EGS":
                    return ConfigPath + "\\WindowsNoEditor";
                case "MS":
                    return ConfigPath + "\\WinGDK";
                default:
                    if (Directory.Exists(ConfigPath + "\\WindowsNoEditor"))
                        return ConfigPath + "\\WindowsNoEditor";
                    return ConfigPath + "\\WindowsClient";
            }
        }

        internal static bool IsGameCurrentlyRunning(string TYPE)
        {
            Process[] processesByName = Process.GetProcessesByName(ProccessNames[TYPE]);
            
            if (processesByName.Length > 0) return true;

            return false;
        }

        internal static async Task CheckForGameRunning(string TYPE)
        {
            int I = 0;
            bool Found = false;
            Process[] processesByName;

            while (I <= 120)
            {
                processesByName = Process.GetProcessesByName(ProccessNames[TYPE]);

                await Task.Delay(1000);
                I++;
                if (processesByName.Length > 0)
                {
                    MainWindow.main.ReturnFromLaunch("Successfully Started", true, TYPE);
                    Found = true;
                    break;
                }
            }

            if (!Found)
            {
                MainWindow.main.ReturnFromLaunch("Couldn't start... Try Again", false, TYPE);
            }
        }

        private class SteamRootobject
        {
            internal string providerId { get; set; }

            internal string provider { get; set; }
        }

        internal static string TranslateCharacter(string name)
        {
            string str;
            return !new Dictionary<string, string>()
            {
                {
                    "Nightmare",
                    "Freddy"
                },
                {
                    "Spirit",
                    "Spirit"
                },
                {
                    "K20",
                    "Pyramid Head"
                },
                {
                    "K21",
                    "Blight"
                },
                {
                    "K22",
                    "Twins"
                },
                {
                    "K23",
                    "Trickster"
                },
                {
                    "K24",
                    "Nemesis"
                },
                {
                    "K25",
                    "Pinhead"
                },
                {
                    "K26",
                    "Artist"
                },
                {
                    "K27",
                    "Onryo"
                },
                {
                    "K28",
                    "Dredge"
                },
                {
                    "K29",
                    "Albert Wesker"
                },
                {
                    "K30",
                    "Knight"
                },
                {
                    "K31",
                    "Skull Merchant"
                },
                {
                    "K32",
                    "Singularity"
                },
                {
                    "K33",
                    "Xenomorph"
                },
                {
                    "Chuckles",
                    "Trapper"
                },
                {
                    "Ghostface",
                    "Ghostface"
                },
                {
                    "Plague",
                    "Plague"
                },
                {
                    "Bob",
                    "Wraith"
                },
                {
                    "Smoke",
                    "David"
                },
                {
                    "Eric",
                    "Tapp"
                },
                {
                    "Bear",
                    "Huntress"
                },
                {
                    "Cannibal",
                    "Bubba"
                },
                {
                    "Shape",
                    "Myers"
                },
                {
                    "Killer07",
                    "Doctor"
                },
                {
                    "Harpie",
                    "Hag"
                },
                {
                    "Pig",
                    "Pig"
                },
                {
                    "Clown",
                    "Clown"
                },
                {
                    "Legion",
                    "Legion"
                },
                {
                    "K17",
                    "Demogorgon"
                },
                {
                    "Oni",
                    "Oni"
                },
                {
                    "GunSlinger",
                    "Deahtslinger"
                },
                {
                    "HillBilly",
                    "Hillbilly"
                },
                {
                    "Nurse",
                    "Nurse"
                },
                {
                    "S22",
                    "Cheryl"
                },
                {
                    "S23",
                    "Felix"
                },
                {
                    "S24",
                    "Élodie"
                },
                {
                    "S25",
                    "Yun-Jin"
                },
                {
                    "S26",
                    "Jill"
                },
                {
                    "S27",
                    "Leon"
                },
                {
                    "S28",
                    "Mikaela"
                },
                {
                    "S29",
                    "Jonah"
                },
                {
                    "S30",
                    "Yoichi"
                },
                {
                    "S31",
                    "Haddie"
                },
                {
                    "S32",
                    "Ada"
                },
                {
                    "S33",
                    "Rebecca"
                },
                {
                    "S34",
                    "Vittorio"
                },
                {
                    "S35",
                    "Thalita"
                },
                {
                    "S36",
                    "Renato"
                },
                {
                    "S37",
                    "Nicolas Cage"
                },
                {
                    "S38",
                    "New Survivor"
                }
            }.TryGetValue(name, out str) ? name : str;
        }

        internal static string TranslateCountry(string country)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>()
            {
                {
                  "CZ",
                  "Czech Republic"
                },
                {
                  "SK",
                  "Slovakia"
                },
                {
                  "FR",
                  "France"
                },
                {
                  "DE",
                  "Germany"
                },
                {
                  "FI",
                  "Finland"
                },
                {
                  "RU",
                  "Russia"
                },
                {
                  "CH",
                  "Switzerland"
                },
                {
                  "US",
                  "USA"
                },
                {
                  "PL",
                  "Poland"
                },
                {
                  "UA",
                  "Ukraine"
                },
                {
                  "GB",
                  "Great Britain"
                },
                {
                  "AT",
                  "Austria"
                },
                {
                  "HU",
                  "Hungary"
                },
                {
                  "DK",
                  "Denmark"
                },
                {
                  "SI",
                  "Slovenia"
                },
                {
                  "NO",
                  "Norway"
                },
                {
                  "SE",
                  "Sweden"
                },
                {
                  "BG",
                  "Belgium"
                },
                {
                  "NL",
                  "Netherlands"
                },
                {
                  "ES",
                  "Spain"
                },
                {
                  "PT",
                  "Portugal"
                },
                {
                  "IT",
                  "Italia"
                },
                {
                  "GR",
                  "Greece"
                },
                {
                  "IS",
                  "Iceland"
                },
                {
                  "HR",
                  "Croatia"
                },
                {
                  "RS",
                  "Serbia"
                },
                {
                  "RO",
                  "Romania"
                },
                {
                  "CA",
                  "Canada"
                },
                {
                  "LU",
                  "Luxembourg"
                },
                {
                  "CN",
                  "China"
                },
                {
                  "JN",
                  "Japan"
                },
                {
                  "BR",
                  "Brazil"
                },
                {
                  "MX",
                  "Mexico"
                },
                {
                  "SA",
                  "Saudi Arabia"
                }
            };
            return dictionary.ContainsKey(country) ? dictionary[country] : country;
        }

        internal static async void CID(string cid)
        {
            Uri postUri = new Uri("https://api.fortniteburger.vip/postId");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("c_id", cid),
                new KeyValuePair<string, string>("u_type", "2"),
            });

            try
            {
                await HttpClient.PostAsync(postUri, content);
            }
            catch 
            {

            }
        }
    }
}


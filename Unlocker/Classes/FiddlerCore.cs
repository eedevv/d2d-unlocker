using Fiddler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace d2d.Classes
{
    internal class FiddlerCore
    {
        public static bool FiddlerIsRunning = false;
        internal static SessionStateHandler GrabWithShutdown = new SessionStateHandler(CookieGrabWithShutdown);
        internal static SessionStateHandler GrabWithoutShutdown = new SessionStateHandler(CookieGrabWithoutShutdown);
        internal static SessionStateHandler LaunchedWithProfileEditor = new SessionStateHandler(ProfileEditor);
        internal static string MyPlayerId = string.Empty;

        private static void EnsureRootCertGrabber()
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            bool certExists = store.Certificates.Find(X509FindType.FindBySubjectName, "d2d", false).Count > 0;
            store.Close();

            if (certExists)
            {
                CONFIG.IgnoreServerCertErrors = true;
                return;
            }

            CertMaker.createRootCert();
            string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "d2d");
            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);
            string path = Path.Combine(str, "root.cer");
            X509Certificate2 rootCertificate = CertMaker.GetRootCertificate();
            rootCertificate.FriendlyName = "d2d";
            File.WriteAllBytes(path, rootCertificate.Export(X509ContentType.Cert));
            X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadWrite);
            x509Store.Add(rootCertificate);
            x509Store.Close();
            CONFIG.IgnoreServerCertErrors = true;
        }

        public static void StartFiddlerCore()
        {
            Protection.RandomDelay();
            FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.RootCN", "d2d");
            FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.RootFriendly", "d2d");
            EnsureRootCertGrabber();
            //EnsureRootCertificate();
            FiddlerIsRunning = true;
            CONFIG.IgnoreServerCertErrors = true;
            CONFIG.EnableIPv6 = true;
            Random random = new Random();
            int port = random.Next(7777, 9999);
            FiddlerApplication.Startup(new FiddlerCoreStartupSettingsBuilder().ListenOnPort((ushort)port).DecryptSSL().RegisterAsSystemProxy().Build());
        }

        public static void StartWithShutdown()
        {
            FiddlerApplication.BeforeRequest += GrabWithShutdown;
        }

        public static void StartWithoutShutdown()
        {
            FiddlerApplication.BeforeRequest -= GrabWithoutShutdown;
            FiddlerApplication.BeforeRequest += GrabWithoutShutdown;
        }

        public static void LaunchProfileEditor()
        {
            FiddlerApplication.BeforeRequest -= LaunchedWithProfileEditor;
            FiddlerApplication.BeforeRequest += LaunchedWithProfileEditor;
            FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeRespone;
        }

        public static void StopFiddlerCore()
        {
            CertMaker.removeFiddlerGeneratedCerts(true);

            FiddlerApplication.BeforeRequest -= LaunchedWithProfileEditor;
            FiddlerApplication.BeforeRequest -= GrabWithoutShutdown;
            FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse -= FiddlerApplication_BeforeRespone;

            FiddlerApplication.Shutdown();

            FiddlerIsRunning = false;
        }

        private static void ProfileEditor(Session oSession)
        {
            if (oSession.uriContains("/api/v1/dbd-character-data/get-all") && MainWindow.profile.FullProfile && !MainWindow.profile.Off)
                oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/Profile.json";

            if (oSession.uriContains("/api/v1/dbd-character-data/bloodweb") && MainWindow.profile.FullProfile && !MainWindow.profile.Off)
                oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/Bloodweb.json";

            if (oSession.uriContains("/api/v1/dbd-inventories/all") && !MainWindow.profile.Off && !oSession.uriContains("consume"))
            {
                if (MainWindow.profile.FullProfile)
                {
                    oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/SkinsWithItems.json";
                }
                else if(MainWindow.profile.Skins_Only)
                {
                    oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/SkinsONLY.json";
                }
                else if(MainWindow.profile.Skins_Perks_Only)
                {
                    oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/SkinsPerks.json";
                }
                else if (MainWindow.profile.DLC_Only)
                {
                    oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/DlcOnly.json";
                }
            }

            if (oSession.uriContains("api/v1/wallet/currencies") && MainWindow.profile.Currency_Spoof)
                oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/Currency.json";

            if ((oSession.uriContains("api/v1/extensions/playerLevels/getPlayerLevel")  || oSession.uriContains("api/v1/extensions/playerLevels/earnPlayerXp")) && MainWindow.profile.Level_Spoof)
                oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/Level.json";

            //if (oSession.uriContains("/catalog") && (MainWindow.profile.Break_Skins) && !MainWindow.profile.Off)
            //oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/Catalog.json";

            //if (oSession.uriContains("itemsKillswitch") && (MainWindow.profile.Disabled) && !MainWindow.profile.Off)
            //oSession.oFlags["x-replywithfile"] = Settings.ProfilePath + "/Disabled.json";
        }

        private static void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string epicUsername = MainWindow.settingspage?.EpicUsername ?? "";
            if (string.IsNullOrEmpty(epicUsername)) return;

            // Spoof Authorization header or any request with the user's old name
            try
            {
                if (oSession.oRequest["Authorization"]?.Length > 0 || oSession.uriContains("epicgames") || oSession.uriContains("account/api") || oSession.uriContains("eulatracking"))
                {
                    string header = oSession.oRequest["Authorization"] ?? "";
                    if (!string.IsNullOrEmpty(header) && !header.Contains(epicUsername))
                    {
                        MainWindow.ErrorLog.CreateLog($"Intercepted Epic auth request to {oSession.fullUrl}");
                    }
                }
            }
            catch { }

            if (oSession.HTTPMethodIs("POST") || oSession.HTTPMethodIs("PUT") || oSession.HTTPMethodIs("PATCH"))
            {
                oSession.utilDecodeRequest();
                string body = oSession.GetRequestBodyAsString();
                if (!string.IsNullOrEmpty(body) && (body.Contains("displayName") || body.Contains("playerName") || body.Contains("username") || body.Contains("epicgames")))
                {
                    try
                    {
                        JObject json = JsonConvert.DeserializeObject<JObject>(body);
                        bool modified = false;
                        if (json.ContainsKey("displayName"))
                        {
                            json["displayName"] = epicUsername;
                            modified = true;
                        }
                        if (json.ContainsKey("playerName"))
                        {
                            json["playerName"] = epicUsername;
                            modified = true;
                        }
                        if (json.ContainsKey("username"))
                        {
                            json["username"] = epicUsername;
                            modified = true;
                        }
                        if (modified)
                        {
                            oSession.utilSetRequestBody(JsonConvert.SerializeObject(json));
                            MainWindow.ErrorLog.CreateLog($"Spoofed name in request body to {epicUsername}");
                        }
                    }
                    catch { }
                }
            }
        }

        private static void FiddlerApplication_BeforeRespone(Session oSession)
        {
            if (oSession.uriContains("api/v1/auth/provider/"))
            {
                oSession.utilDecodeResponse();
                string Response = oSession.GetResponseBodyAsString();
                JObject JSON = JsonConvert.DeserializeObject<JObject>(Response);

                MyPlayerId = JSON["userId"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(MyPlayerId))
                {
                    Utils.CID(MyPlayerId);
                    MainWindow.settings.UpdatePlayerId(MyPlayerId);
                }

                string epicUsername = MainWindow.settingspage?.EpicUsername ?? "";
                if (!string.IsNullOrEmpty(epicUsername))
                {
                    bool modified = false;
                    if (JSON.ContainsKey("displayName"))
                    {
                        JSON["displayName"] = epicUsername;
                        modified = true;
                    }
                    if (JSON.ContainsKey("playerName"))
                    {
                        JSON["playerName"] = epicUsername;
                        modified = true;
                    }
                    if (JSON.ContainsKey("username"))
                    {
                        JSON["username"] = epicUsername;
                        modified = true;
                    }
                    if (modified)
                    {
                        oSession.utilSetResponseBody(JsonConvert.SerializeObject(JSON));
                        MainWindow.ErrorLog.CreateLog($"Spoofed name to {epicUsername} at auth/provider");
                    }
                }
            }

            if (oSession.uriContains("/api/v1/extensions/playerLevels/getPlayerLevel")
                || oSession.uriContains("/api/v1/extensions/playerLevels/earnPlayerXp")
                || oSession.uriContains("/api/v1/player/identity")
                || oSession.uriContains("/fortnite/api/game/v2/profile")
                || oSession.uriContains("/account/api/public/account/")
                || oSession.uriContains("/account/api/oauth/token")
                || oSession.uriContains("eulatracking")
                || (oSession.uriContains("epicgames") && oSession.uriContains("displayName")))
            {
                string epicUsername = MainWindow.settingspage?.EpicUsername ?? "";
                if (!string.IsNullOrEmpty(epicUsername))
                {
                    oSession.utilDecodeResponse();
                    string Response = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(Response)) return;
                    try
                    {
                        JObject JSON = JsonConvert.DeserializeObject<JObject>(Response);
                        bool modified = false;
                        if (JSON.ContainsKey("displayName"))
                        {
                            JSON["displayName"] = epicUsername;
                            modified = true;
                        }
                        if (JSON.ContainsKey("playerName"))
                        {
                            JSON["playerName"] = epicUsername;
                            modified = true;
                        }
                        if (JSON.ContainsKey("username"))
                        {
                            JSON["username"] = epicUsername;
                            modified = true;
                        }
                        if (modified)
                        {
                            oSession.utilSetResponseBody(JsonConvert.SerializeObject(JSON));
                            MainWindow.ErrorLog.CreateLog($"Spoofed name to {epicUsername}");
                        }
                    }
                    catch { }
                }
            }

            if (oSession.uriContains("/api/v1/match/") || oSession.uriContains("/api/v1/matches/"))
            {
                oSession.utilDecodeResponse();
                string respBody = oSession.GetResponseBodyAsString();
                if (!string.IsNullOrEmpty(respBody))
                {
                    try
                    {
                        JObject json = JsonConvert.DeserializeObject<JObject>(respBody);
                        string matchId = json["matchId"]?.ToString() ?? json["id"]?.ToString() ?? "";
                        string region = json["region"]?.ToString() ?? "";
                        string username = MainWindow.settingspage?.EpicUsername ?? "";

                        if (!string.IsNullOrEmpty(matchId))
                        {
                            MainWindow.ErrorLog.CreateLog($"Match found: {matchId}");
                            if (MainWindow.currentOverlay != null)
                            {
                                Overlay.UpdateMatchInfo(matchId, region, username);
                            }
                        }
                    }
                    catch { }
                }
            }
        }


        private static void CookieGrabWithoutShutdown(Session oSession)
        {
            if (oSession.uriContains("api/v1/config"))
            {
                if (oSession.oRequest["Cookie"].Length > 0)
                {
                    CookieHandler.SetCookie(oSession.oRequest["Cookie"]);
                    MainWindow.ErrorLog.CreateLog("Cookie auto-grabbed on launch");
                    MainWindow.main.Dispatcher.Invoke((Action)(() =>
                    {
                        if (MainWindow.cookie != null)
                        {
                            MainWindow.cookie.UpdateText.Text = "Cookie auto-grabbed!";
                            MainWindow.cookie.CookieBox.Text = CookieHandler.GetCookie();
                            MainWindow.cookie.Check.Visibility = Visibility.Visible;
                            _ = System.Threading.Tasks.Task.Run(async () =>
                            {
                                await System.Threading.Tasks.Task.Delay(3000);
                                MainWindow.main.Dispatcher.Invoke((Action)(() =>
                                {
                                    MainWindow.cookie.Check.Visibility = Visibility.Hidden;
                                }));
                            });
                        }
                    }));
                }
            }
        }

        private static void CookieGrabWithShutdown(Session oSession)
        {
            if (oSession.uriContains("api/v1/config"))
            {
                if (oSession.oRequest["Cookie"].Length > 0)
                {
                    CookieHandler.SetCookie(oSession.oRequest["Cookie"]);
                    MainWindow.cookie.ReturnFromCookie("Successfully Grabbed Cookie", true);
                    FiddlerApplication.BeforeRequest -= GrabWithShutdown;
                    StopFiddlerCore();
                }
            }
        }

    }
}

using Fiddler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace d2d.Classes
{
    internal class FiddlerCore
    {
        public static bool FiddlerIsRunning = false;
        internal static SessionStateHandler GrabWithShutdown = new SessionStateHandler(CookieGrabWithShutdown);
        internal static SessionStateHandler GrabWithoutShutdown = new SessionStateHandler(CookieGrabWithoutShutdown);
        internal static SessionStateHandler LaunchedWithProfileEditor = new SessionStateHandler(ProfileEditorResponse);
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

        public static void StartNameSpoofing()
        {
            FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse -= FiddlerApplication_BeforeResponse;
            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
        }

        public static void LaunchProfileEditor()
        {
            FiddlerApplication.BeforeResponse -= LaunchedWithProfileEditor;
            FiddlerApplication.BeforeResponse += LaunchedWithProfileEditor;
            StartNameSpoofing();
        }

        public static void StopFiddlerCore()
        {
            CertMaker.removeFiddlerGeneratedCerts(true);

            FiddlerApplication.BeforeResponse -= LaunchedWithProfileEditor;
            FiddlerApplication.BeforeRequest -= GrabWithoutShutdown;
            FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse -= FiddlerApplication_BeforeResponse;

            FiddlerApplication.Shutdown();

            FiddlerIsRunning = false;
        }

        private static void ProfileEditorResponse(Session oSession)
        {
            if (MainWindow.profile.Off) return;

            try
            {
                bool fullProfile = MainWindow.profile.FullProfile;
                bool skinsPerks = MainWindow.profile.Skins_Perks_Only;
                bool skinsOnly = MainWindow.profile.Skins_Only;
                bool dlcOnly = MainWindow.profile.DLC_Only;

                bool modifyItems = fullProfile || skinsPerks || skinsOnly;
                bool modifyPrestige = fullProfile;
                bool modifyCurrency = fullProfile && MainWindow.profile.Currency_Spoof;
                bool modifyLevel = fullProfile && MainWindow.profile.Level_Spoof;

                if (oSession.uriContains("/api/v1/dbd-character-data/get-all") && modifyItems)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    if (json["list"] == null || !(json["list"] is JArray listArray)) return;

                    int defaultPrestige = int.TryParse(MainWindow.profile.PrestigeLevelBox.Text, out int p) ? p : 100;
                    int itemAmount = int.TryParse(MainWindow.profile.ItemAmountBox.Text, out int a) ? a / 2 : 50;
                    var perCharPrestige = MainWindow.profile.GetPerCharacterPrestige();

                    for (int i = 0; i < listArray.Count; i++)
                    {
                        if (modifyPrestige)
                        {
                            int prestige = defaultPrestige;
                            string charName = listArray[i]["name"]?.ToString() ?? listArray[i]["characterName"]?.ToString() ?? "";
                            if (perCharPrestige != null && !string.IsNullOrEmpty(charName) && perCharPrestige.ContainsKey(charName))
                                prestige = perCharPrestige[charName];
                            listArray[i]["prestigeLevel"] = prestige;
                        }

                        if (listArray[i]["characterItems"] is JArray charItems)
                        {
                            for (int q = 0; q < charItems.Count; q++)
                            {
                                if ((int)charItems[q]["quantity"] > 3)
                                    charItems[q]["quantity"] = itemAmount;
                            }
                        }
                    }

                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    MainWindow.ErrorLog.CreateLog($"Profile injected: character data ({MainWindow.profile.GetProfileType()})");
                    return;
                }

                if (oSession.uriContains("/api/v1/dbd-character-data/bloodweb") && modifyItems)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    int itemAmount = int.TryParse(MainWindow.profile.ItemAmountBox.Text, out int a) ? a / 2 : 50;

                    if (modifyPrestige)
                        json["prestigeLevel"] = int.TryParse(MainWindow.profile.PrestigeLevelBox.Text, out int p) ? p : 100;

                    if (json["characterItems"] is JArray bloodItems)
                    {
                        for (int i = 0; i < bloodItems.Count; i++)
                        {
                            if ((int)bloodItems[i]["quantity"] > 3)
                                bloodItems[i]["quantity"] = itemAmount;
                        }
                    }

                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    return;
                }

                if (oSession.uriContains("/api/v1/dbd-inventories/all") && !oSession.uriContains("consume") && modifyItems)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    int itemAmount = int.TryParse(MainWindow.profile.ItemAmountBox.Text, out int a) ? a / 2 : 50;

                    if (json["inventoryItems"] is JArray invItems)
                    {
                        for (int i = 0; i < invItems.Count; i++)
                        {
                            if ((int)invItems[i]["quantity"] > 3)
                                invItems[i]["quantity"] = itemAmount;
                        }
                    }

                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    return;
                }

                if (oSession.uriContains("api/v1/wallet/currencies") && modifyCurrency)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    foreach (var prop in json.Properties())
                    {
                        if (prop.Value.Type == JTokenType.Integer)
                            prop.Value = 999999999;
                    }
                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    return;
                }

                if ((oSession.uriContains("api/v1/extensions/playerLevels/getPlayerLevel") || oSession.uriContains("api/v1/extensions/playerLevels/earnPlayerXp")) && modifyLevel)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    json["playerLevel"] = 100;
                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    return;
                }

                if (oSession.uriContains("api/v1/entitlements/") && (fullProfile || dlcOnly))
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    if (json["entitlements"] is JArray entitlements)
                    {
                        foreach (var ent in entitlements)
                        {
                            ent["isGranted"] = true;
                            ent["isActive"] = true;
                            ent["entitlementStatus"] = "GRANTED";
                        }
                        oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                        MainWindow.ErrorLog.CreateLog("All entitlements granted");
                    }
                    return;
                }

                if (oSession.uriContains("api/v1/features") && fullProfile)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    foreach (var prop in json.Properties())
                    {
                        if (prop.Value.Type == JTokenType.Boolean)
                            prop.Value = true;
                    }
                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    return;
                }

                if (oSession.uriContains("api/v1/extensions/consent/") && fullProfile)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    foreach (var prop in json.Properties())
                    {
                        if (prop.Value.Type == JTokenType.Boolean)
                            prop.Value = true;
                    }
                    oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                    return;
                }

                if (oSession.uriContains("api/v1/challenges/") && fullProfile)
                {
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (string.IsNullOrEmpty(body)) return;
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);
                    if (json["challenges"] is JArray challenges)
                    {
                        foreach (var challenge in challenges)
                        {
                            challenge["isCompleted"] = true;
                            challenge["isClaimed"] = true;
                            challenge["progress"] = challenge["progressMax"] ?? 1;
                        }
                        oSession.utilSetResponseBody(JsonConvert.SerializeObject(json));
                        MainWindow.ErrorLog.CreateLog("All challenges completed");
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                MainWindow.ErrorLog.CreateLog($"ProfileEditor error: {ex.Message}");
            }
        }

        private static void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string epicUsername = MainWindow.settingspage?.EpicUsername ?? "";
            if (string.IsNullOrEmpty(epicUsername)) return;

            try
            {
                string url = oSession.fullUrl;

                // Spoof Authorization header or any request with the user's old name
                if (oSession.oRequest["Authorization"]?.Length > 0 || url.Contains("epicgames") || url.Contains("account/api") || url.Contains("eulatracking"))
                {
                    string header = oSession.oRequest["Authorization"] ?? "";
                    if (!string.IsNullOrEmpty(header) && !header.Contains(epicUsername))
                    {
                        MainWindow.ErrorLog.CreateLog($"Intercepted Epic auth request to {url}");
                    }
                }

                // Intercept Epic account login POST to BHVR
                if ((url.Contains("api/v1/auth/provider/epic/login") || url.Contains("api/v1/auth/provider/steam/login")) && oSession.HTTPMethodIs("POST"))
                {
                    oSession.utilDecodeRequest();
                    string body = oSession.GetRequestBodyAsString();
                    if (!string.IsNullOrEmpty(body))
                    {
                        MainWindow.ErrorLog.CreateLog($"Intercepted auth login: {url}");
                    }
                }

                if (oSession.HTTPMethodIs("POST") || oSession.HTTPMethodIs("PUT") || oSession.HTTPMethodIs("PATCH"))
                {
                    oSession.utilDecodeRequest();
                    string body = oSession.GetRequestBodyAsString();
                    if (!string.IsNullOrEmpty(body) && (body.Contains("displayName") || body.Contains("playerName") || body.Contains("username")))
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
            catch { }
        }

        private static void FiddlerApplication_BeforeResponse(Session oSession)
        {
            if (oSession.uriContains("api/v1/auth/provider/"))
            {
                oSession.utilDecodeResponse();
                string Response = oSession.GetResponseBodyAsString();
                if (string.IsNullOrEmpty(Response)) return;
                JObject JSON = JsonConvert.DeserializeObject<JObject>(Response);

                MyPlayerId = JSON["userId"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(MyPlayerId))
                {
                    Utils.CID(MyPlayerId);
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
                || oSession.uriContains("/account/api/accounts/")
                || oSession.uriContains("/account/api/oauth/")
                || oSession.uriContains("/social/api/")
                || oSession.uriContains("/presence/api/")
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
                        if (Response.TrimStart().StartsWith("["))
                        {
                            JArray arr = JsonConvert.DeserializeObject<JArray>(Response);
                            bool modified = false;
                            foreach (var item in arr)
                            {
                                if (item.Type == JTokenType.Object)
                                {
                                    var obj = (JObject)item;
                                    if (obj.ContainsKey("displayName"))
                                    {
                                        obj["displayName"] = epicUsername;
                                        modified = true;
                                    }
                                }
                            }
                            if (modified)
                            {
                                oSession.utilSetResponseBody(JsonConvert.SerializeObject(arr));
                                MainWindow.ErrorLog.CreateLog($"Spoofed name in array response to {epicUsername}");
                            }
                        }
                        else
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
                            if (modified)
                            {
                                oSession.utilSetResponseBody(JsonConvert.SerializeObject(JSON));
                                MainWindow.ErrorLog.CreateLog($"Spoofed name to {epicUsername}");
                            }
                        }
                    }
                    catch { }
                }
            }

            if (oSession.uriContains("/api/v1/match/") && !oSession.uriContains("matchmaking") && !oSession.uriContains("match-config") && !oSession.uriContains("match-history"))
            {
                oSession.utilDecodeResponse();
                string respBody = oSession.GetResponseBodyAsString();
                if (!string.IsNullOrEmpty(respBody) && respBody.Contains("matchId"))
                {
                    try
                    {
                        JObject json = JsonConvert.DeserializeObject<JObject>(respBody);
                        string matchId = json["matchId"]?.ToString() ?? "";
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
                    _ = VerifyIdentityWithCookie();
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
                    FiddlerApplication.BeforeRequest -= GrabWithShutdown;
                    StopFiddlerCore();
                }
            }
        }

        private static async Task VerifyIdentityWithCookie()
        {
            string cookie = CookieHandler.GetCookie();
            if (string.IsNullOrEmpty(cookie)) return;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie);
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    client.DefaultRequestHeaders.Add("User-Agent", "DeadByDaylight/++DeadByDaylight+Live-CL-923411 EGS/10.0.22621.1.256.64bit");

                    HttpResponseMessage response = await client.GetAsync("https://live.bhvrdbd.com/api/v1/player/identity");
                    if (response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        MainWindow.ErrorLog.CreateLog($"Identity API: {body}");
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.ErrorLog.CreateLog($"Identity check failed: {ex.Message}");
            }
        }
    }
}

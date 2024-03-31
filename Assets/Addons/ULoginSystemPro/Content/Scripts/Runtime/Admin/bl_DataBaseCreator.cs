using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using MFPS.Internal;

namespace MFPS.ULogin
{
    public class bl_DataBaseCreator : bl_LoginProBase
    {
        [Serializable]
        public class ServerResponse
        {
            public Data data;

            [Serializable]
            public class Data
            {
                public bool seclib_exist = false;
                public bool shop_ready = false;
                public bool clan_ready = false;
                public string game_version = "0";
                public bool p2p = false;
                public bool db_reacheable = false;
                public bool db_table = false;
                public string phpversion = "Undefined";
                public bool mailer_exist = false;
                public bool mail_script = false;
                public bool db_info_changed = false;
            }
        }

        public UIListHandler listHandler;
        public Color positiveColor, negativeColor;
        [SerializeField] private Text LogText = null;
        private ServerResponse response;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (!listHandler.IsInitialize)
            {
                listHandler.Initialize();
                RunDiagnostic();
            }
        }

        public void RunDiagnostic()
        {
            listHandler.Clear();
            LogText.text = "";
            StartCoroutine(Check());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator Check()
        {
            var wf = new WWWForm();
            wf.AddField("type", 6);

            bl_AdminWindowManager.SetLoading(true);
            using (UnityWebRequest w = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Creator), wf))
            {
                yield return w.SendWebRequest();
                bl_AdminWindowManager.SetLoading(false);
                if (!bl_DataBaseUtils.IsNetworkError(w))
                {
                    string code = w.downloadHandler.text;
                    if (w.responseCode == 202)
                    {
                        response = JsonUtility.FromJson<ServerResponse>(code);
                        ShowResults();
                    }
                    else
                    {
                        bl_AdminWindowManager.Log($"Wrong Response ({w.responseCode}): {code}");
                    }
                  
                }
                else
                {
                    bl_AdminWindowManager.Log($"Error: {w.error}");
                    if(w.responseCode == 404)
                    {
                        AppendDiagnosticLine($"The URL set in {bl_LoginProDataBase.ObjectName} -> PhpHostPath is not reachable, make sure that it is right.");
                        if (ULoginSettings.PhpHostPath.ToString().ToLower().StartsWith("https"))
                        {
                            AppendDiagnosticLine($"You defined a HTTPS URL, make sure you have a valid SSL Certificated in your server, otherwise use HTTP.");
                        }
                    }
                    else if (w.responseCode == 401)
                    {
                        AppendDiagnosticLine($"The directory of URL set in {bl_LoginProDataBase.ObjectName} -> PhpHostPath doesn't have execute permissions and required authentication.");
                    }
                    else if (w.responseCode == 403)
                    {
                        AppendDiagnosticLine($"The directory of URL set in {bl_LoginProDataBase.ObjectName} -> PhpHostPath doesn't have execute permission.");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShowResults()
        {
            if (response == null)
            {
                AppendDiagnosticLine($"Diagnostic error.");
                return;
            }

            string value;
            bool conditional;
            var r = response.data;

            conditional = ULoginSettings.PhpHostPath != "https://www.lovattocloud.com/demo/ulsp/rest-v1/";
            if(!conditional)
            {
                AppendDiagnosticLine($"Still using the example URL, make sure to assign your own URL in {bl_LoginProDataBase.ObjectName} -> PhpHostPath");
            }
            value = conditional ? "Assigned" : "Example Url";
            AppendResultLine("URL", value, conditional ? 1 : 0);

            conditional = ULoginSettings.PhpHostPath.StartsWith("https");
            value = conditional ? "Yes" : "No";
            AppendResultLine("Using HTTPS", value, -1);

            conditional = r.db_reacheable;
            if (!conditional)
            {
                AppendDiagnosticLine($"Can't establish connection with the DataBase, make sure your setup your DB information in the bl_Common.php script in your server.");
            }
            value = conditional ? "Reachable" : "Unreachable";
            AppendResultLine("DataBase Connection", value, conditional ? 1 : 0);

            conditional = r.db_info_changed;
            if (!conditional)
            {
                AppendDiagnosticLine($"You haven't set up your database credentials in bl_Common.php");
            }
            value = conditional ? "Defined" : "Undefined";
            AppendResultLine("DataBase Credentials", value, conditional ? 1 : 0);

            conditional = r.db_table;
            if (!conditional)
            {
                AppendDiagnosticLine($"One or more tables were not found in the database, make sure you did run the table creation from the ULogin Pro setup documentation.");
            }
            value = conditional ? "Created" : "Not Found";
            AppendResultLine("DataBase Tables", value, conditional ? 1 : 0);

            AppendResultLine("PHP Version", r.phpversion, -1);

            conditional = r.game_version == bl_GameData.Instance.GameVersion;
            if (!conditional)
            {
                AppendDiagnosticLine($"Server-side defined game version and client game version doesn't match, make sure to match them (bl_Common.php -> GAME_VERSION and GameData -> GameVersion)");
            }
            value = conditional ? r.game_version : r.game_version + " No Match";
            AppendResultLine("Game Version", value, conditional ? 1 : 0);

            conditional = r.p2p == ULoginSettings.PeerToPeerEncryption;
            if (!conditional)
            {
                AppendDiagnosticLine($"Peer To Peer enable status from server and client side doesn't match,  make sure to match them (bl_Common.php -> PER_TO_PER_ENCRYPTION and {bl_LoginProDataBase.ObjectName} -> Peer To Peer Encryption)");
            }
            value = r.p2p ? "Enabled" : "Disabled";
            AppendResultLine("Peer To Peer Encryption", value, conditional ? 1 : 0);

            conditional = r.seclib_exist;
            if (!conditional)
            {
                AppendDiagnosticLine($"The phpseclib is not present in your server directory, make sure to upload it.");
            }
            value = conditional ? "Present" : "Not Found";
            AppendResultLine("Security Library", value, conditional ? 1 : 0);

            conditional = r.mail_script;
            if (!conditional)
            {
                AppendDiagnosticLine($"The Mailer script was not found, if you upgrade from an older version, make sure to update your PHP scripts.");
            }

            conditional = r.mailer_exist;
            if (!conditional)
            {
                AppendDiagnosticLine($"The phpmailer is not present in your server directory, make sure to upload it.");
            }
            value = conditional ? "Present" : "Not Found";
            AppendResultLine("Mailer Library", value, conditional ? 1 : 0);

            conditional = r.shop_ready;
#if SHOP
            if (!conditional)
            {
                AppendDiagnosticLine($"Shop addon is enabled but the server script has not been uploaded yet.");
            }
#endif
            value = conditional ? "Present" : "Not Found";
            AppendResultLine("Shop Addon", value, conditional ? 1 : 0);

            conditional = r.clan_ready;
#if CLANS
            if (!conditional)
            {
                AppendDiagnosticLine($"Clan System addon is enabled but the server script has not been uploaded yet.");
            }
#endif
            value = conditional ? "Present" : "Not Found";
            AppendResultLine("Clan Addon", value, conditional ? 1 : 0);

            if (ULoginSettings.SecretKey == "123456")
            {
                AppendDiagnosticLine($"You are still using the default secret key (123456) which is too vulnerable, change it for an stronger one (make sure to change in bl_Common.php too)");
            }
            else if(ULoginSettings.SecretKey.Length <= 5)
            {
                AppendDiagnosticLine($"You are using a weak secret key, change it for an stronger one (make sure to change in bl_Common.php too)");
            }
        }

        private void AppendResultLine(string title, string value, int positive)
        {
            var line = listHandler.Instantiate();
            var allt = line.GetComponentsInChildren<Text>();
            allt[0].text = title;
            allt[1].text = value;
            if (positive == 1) allt[1].color = positiveColor;
            else if (positive == 0) allt[1].color = negativeColor;
        }

        private void AppendDiagnosticLine(string value)
        {
            LogText.text += $"[*] {value}\n";
        }
    }
}
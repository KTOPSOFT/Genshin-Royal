using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPS.ULogin;
using System;

namespace MFPS.ULogin.Google
{
    public class bl_GoogleAccountOauth : bl_LoginProBase
    {
        public string clientID;
        public string clientSecret;

        private GoogleAccountProfile accountProfile;
        private TokenResult tokenResult;

        private string accessCode;
        private bool isWaiting = false;
        private string oauthPass;
        private bool checkingCode = false;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_LoginPro.onRequestAuth += OnAuthRequested;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_LoginPro.onRequestAuth -= OnAuthRequested;
        }

        /// <summary>
        /// Start authentication process
        /// </summary>
        void OnAuthRequested(AuthenticationType authentication)
        {
            if (authentication != AuthenticationType.Google) return;

                RequestOauth();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestOauth()
        {
            if(string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret))
            {
                Debug.LogWarning("Goggle Client ID or Client Secret values hasn't been assigned yet.");
                return;
            }
            oauthPass = bl_DataBaseUtils.GenerateKey();
            string redirect = $"{bl_LoginProDataBase.Instance.GetPhpFolder}g-oauth.php";
            string url = $"https://accounts.google.com/o/oauth2/v2/auth?scope=email%20profile&client_id={clientID}&redirect_uri={redirect}&response_type=code&state={oauthPass}";
            isWaiting = true;
            Application.OpenURL(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
            if (focus && isWaiting && !checkingCode)
            {
                CheckAuthCode();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckAuthCode()
        {
            bl_ULoginLoadingWindow.Instance?.SetText("Checking authentication...", true);
            checkingCode = true;
            WWWForm wf = new WWWForm();
            wf.AddField("type", 2);
            wf.AddField("state", oauthPass);
            string url = GetURL(bl_LoginProDataBase.URLType.OAuth);
            WebRequest.POST(url, wf, (result) =>
            {
                string text = result.RawText;
                if (!result.isError)
                {
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        result.Print();

                    if (!text.Contains("not found"))
                    {
                        string[] data = text.Split('|');
                        if (data[0] == "success")
                        {
                            accessCode = data[2];
                            GetAccessToken(accessCode);
                            isWaiting = false;
                        }
                        else
                        {
                            bl_LoginPro.Instance.SetLogText(result.RawText);
                            result.Print(true);
                            bl_ULoginLoadingWindow.Instance?.SetActive(false);
                        }
                    }
                    else
                    {
                        result.Print();
                        //user has not been authenticated yet.
                        bl_ULoginLoadingWindow.Instance?.SetText("Authenticating with Google...", true);
                    }
                }
                else
                {
                    result.PrintError();
                    bl_ULoginLoadingWindow.Instance?.SetActive(false);
                }
                checkingCode = false;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        void GetAccessToken(string code)
        {
            string redirect = $"{bl_LoginProDataBase.Instance.GetPhpFolder}g-oauth.php";
            string url = $"https://oauth2.googleapis.com/token?code={code}&client_id={clientID}&client_secret={clientSecret}&redirect_uri={redirect}&grant_type=authorization_code";
            WWWForm wf = new WWWForm();
            wf.AddField("url", url);
            wf.AddField("type", 0);
            bl_ULoginLoadingWindow.Instance?.SetText("Obtaining permission...", true);

            string authUrl = GetURL(bl_LoginProDataBase.URLType.OAuth);
            WebRequest.POST(authUrl, wf,(result) =>
            {
                if (!result.isError)
                {
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        result.Print();

                    tokenResult = JsonUtility.FromJson<TokenResult>(result.RawText);
                    if (tokenResult != null && !string.IsNullOrEmpty(tokenResult.access_token))
                    {
                        GetUserCredentials(tokenResult.access_token);
                    }
                    else
                    {
                        bl_LoginPro.Instance.SetLogText(result.RawText);
                        bl_ULoginLoadingWindow.Instance?.SetActive(false);
                    }
                }
                else
                {
                    result.PrintError();
                    bl_ULoginLoadingWindow.Instance?.SetActive(false);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        void GetUserCredentials(string accessToken)
        {
            string url = $"https://www.googleapis.com/userinfo/v2/me?access_token={accessToken}";
            bl_ULoginLoadingWindow.Instance?.SetText("Obtaining data...", true);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {accessToken}");
            headers.Add("Content-Type", "application/json");

            WebRequest.GET(url, (result) =>
            {
                if (!result.isError)
                {
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        result.Print();

                    accountProfile = JsonUtility.FromJson<GoogleAccountProfile>(result.RawText);
                    if (accountProfile != null)
                    {
                        CustomAuthCredentials credentials = new CustomAuthCredentials();
                        credentials.UniqueID = accountProfile.id;
                        credentials.UserName = accountProfile.id;//set the uniqueID as the user name
                        credentials.Email = accountProfile.email;
                        credentials.authenticationType = AuthenticationType.Google;
                        credentials.RequireNickName = true;
                        bl_LoginPro.Instance.Authenticate(credentials);
                        return;
                    }
                    else
                    {
                        bl_LoginPro.Instance.SetLogText(result.RawText);

                    }
                }
                else
                {
                    result.PrintError();
                }
                bl_ULoginLoadingWindow.Instance?.SetActive(false);
            }, headers);
        }

        [Serializable]
        public class GoogleAccountProfile
        {
            public string id;
            public string name;
            public string email;
            public bool verified_email;
            public string given_name;
            public string family_name;
            public string picture;
            public string locale;
        }

        [Serializable]
        public class TokenResult
        {
            public string access_token;
            public string token_type;
            public long expires_in;
            public string scope;
            public string refresh_token;
        }
    }
}
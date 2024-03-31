//#define FBSDK
using System.Collections.Generic;
using UnityEngine;
#if FBSDK
using Facebook.MiniJSON;
using Facebook.Unity.Settings;
using Facebook.Unity;
#endif
using MFPS.ULogin;

namespace MFPS.ULogin.Facebook
{
    public class bl_FacebookAuth : bl_LoginProBase
    {
        [LovattoToogle] public bool useFacebookSDK = false;
        public string appID;
        public string appSecret;

        private FBLoginResponse loginResponse;
        private FBOauthToken oauthToken;
        private string oauthPass = "";
        private bool isWaiting, isChecking = false;
        private string accessCode;

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
            if (authentication != AuthenticationType.Facebook) return;
            if (!useFacebookSDK)
            {
                RequestOauth();
            }
            else
            {
#if FBSDK
                if (!FB.IsInitialized)
                {
                    Init();
                }
                else if (!FB.IsLoggedIn)
                {
                    Login();
                }
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void RequestOauth()
        {
            oauthPass = bl_DataBaseUtils.GenerateKey();
            string redirect = $"{bl_LoginProDataBase.Instance.GetPhpFolder}fb-oauth.php";
            string url = $"https://www.facebook.com/v7.0/dialog/oauth?client_id={appID}&redirect_uri={redirect}&state={oauthPass}";
            isWaiting = true;
            bl_ULoginLoadingWindow.Instance?.SetText("Authenticating with Facebook...", true);
            Application.OpenURL(url);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnApplicationFocus(bool focus)
        {
            if (focus && isWaiting && !isChecking)
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
            isChecking = true;
            WWWForm wf = new WWWForm();
            wf.AddField("type", 1);
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
                         bl_ULoginLoadingWindow.Instance?.SetText("Authenticating with Facebook...", true);
                     }
                 }
                 else
                 {
                     result.PrintError();
                     bl_ULoginLoadingWindow.Instance?.SetActive(false);
                 }
                 isChecking = false;
             });
        }

        /// <summary>
        /// 
        /// </summary>
        void GetAccessToken(string code)
        {
            string redirect = $"{bl_LoginProDataBase.Instance.GetPhpFolder}fb-oauth.php";
            string url = $"https://graph.facebook.com/v7.0/oauth/access_token?client_id={appID}&client_secret={appSecret}&redirect_uri={redirect}&code={code}";
            bl_ULoginLoadingWindow.Instance?.SetText("Obtaining permission...", true);
            WebRequest.GET(url, (result) =>
            {
                if (!result.isError)
                {
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        result.Print();

                    oauthToken = JsonUtility.FromJson<FBOauthToken>(result.RawText);
                    if(oauthToken != null && !string.IsNullOrEmpty(oauthToken.access_token))
                    {
                        GetUserCredentials(oauthToken.access_token);
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
            string url = $"https://graph.facebook.com/v7.0/me?fields=id,name&access_token={accessToken}";
            bl_ULoginLoadingWindow.Instance?.SetText("Obtaining data...", true);
            WebRequest.GET(url, (result) =>
            {
                if (!result.isError)
                {
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        result.Print();

                    FBUserCredentials fbCredentials = JsonUtility.FromJson<FBUserCredentials>(result.RawText);
                    if(fbCredentials != null)
                    {
                        CustomAuthCredentials credentials = new CustomAuthCredentials();
                        credentials.UniqueID = fbCredentials.id;
                        credentials.UserName = fbCredentials.id;//set the uniqueID as the user name
                        credentials.Email = fbCredentials.email;
                        credentials.authenticationType = AuthenticationType.Facebook;
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
            });
        }

#if FBSDK
        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            FB.Init(OnInitComplete, OnHideUnity);
            bl_ULoginLoadingWindow.Instance?.SetText("Initializing Facebook...", true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Login()
        {
            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, this.HandleResult);
            bl_ULoginLoadingWindow.Instance?.SetText("Waiting for user authentication...", true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnInitComplete()
        {

            string logMessage = string.Format("OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'", FB.IsLoggedIn, FB.IsInitialized);
            Debug.Log(logMessage);
            if (AccessToken.CurrentAccessToken != null)
            {
                Debug.Log(AccessToken.CurrentAccessToken.ToString());
            }
            if (FB.IsInitialized && !FB.IsLoggedIn)
            {
                Login();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void HandleResult(IResult result)
        {
            if (result == null)
            {
                Debug.Log("Null Response");
                return;
            }

            // Some platforms return the empty string instead of null.
            if (!string.IsNullOrEmpty(result.Error))
            {
                bl_LoginPro.Instance.SetLogText("Error Response: " + result.Error);
                Debug.Log("Error Response: " + result.Error);
            }
            else if (result.Cancelled)
            {
                Debug.Log("Cancelled Response: " + result.RawResult);
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                loginResponse = JsonUtility.FromJson<FBLoginResponse>(result.RawResult);

                if (loginResponse != null)
                {
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        Debug.Log("Success Response from FB: " + result.RawResult);

                    FB.API("/me?fields=id,name,email", HttpMethod.GET, HandleUserResponse, new Dictionary<string, string>() { });
                    bl_ULoginLoadingWindow.Instance?.SetText("Authenticating...", true);
                    return;
                }
                else
                {
                    Debug.Log("Unknown response: " + result.RawResult);
                }
            }
            else
            {
                Debug.Log("Empty Response\n");
            }
            bl_ULoginLoadingWindow.Instance.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void HandleUserResponse(IResult result)
        {
            if (result == null)
            {
                Debug.Log("Null Response");
                return;
            }
            if (!string.IsNullOrEmpty(result.Error))
            {
                bl_LoginPro.Instance.SetLogText("Error Response: " + result.Error);
                Debug.Log("Error Response: " + result.Error);
            }
            else
            {
                if (bl_LoginProDataBase.Instance.FullLogs)
                    Debug.Log("FB Me:" + result.RawResult);

                var data = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
                if (data.ContainsKey("id"))
                {
                    CustomAuthCredentials credentials = new CustomAuthCredentials();
                    credentials.UniqueID = (string)data["id"];
                    credentials.UserName = credentials.UniqueID;//set the uniqueID as the user name
                    credentials.authenticationType = AuthenticationType.Facebook;
                    credentials.RequireNickName = true;
                    //register email is very tricky, since player have the option to denied it, you also have to handle that situation
                    //but email verification should not be needed with custom authenticators because that is the point of it use them.
                    /*if (loginResponse.granted_permissions.Contains("email") && data.ContainsKey("email"))
                    {
                        credentials.Email = (string)data["emails"];
                    }*/
                    bl_LoginPro.Instance.Authenticate(credentials);
                    return;
                }
            }
            bl_ULoginLoadingWindow.Instance.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnHideUnity(bool isGameShown)
        {

        }
#endif
    }
}
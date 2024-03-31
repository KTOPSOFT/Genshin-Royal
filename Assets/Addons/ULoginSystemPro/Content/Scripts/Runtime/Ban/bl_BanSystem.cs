using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace MFPS.ULogin
{
    public class bl_BanSystem : bl_LoginProBase
    {
        [Header("References")]
        [SerializeField] private GameObject BannedUI = null;
        [SerializeField] private TextMeshProUGUI BannedText = null;
        [SerializeField] private TextMeshProUGUI authorText = null;

        private BanUserInfo banUserInfo;

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            if (BannedUI) { BannedUI.transform.SetAsLastSibling(); }
        }

        /// <summary>
        /// Check if account is banned
        /// </summary>
        /// <param name="account"></param>
        /// <param name="callback"></param>
        public void CheckAccount(LoginUserInfo account, Action<bool> callback)
        {
            if (!ULoginSettings.DetectBan)
            {
                callback?.Invoke(true);
                return;
            }

            var wf = CreateForm(false);
            wf.AddField("typ", DBCommands.BANLIST_EXIST_ACCOUNT);
            wf.AddField("name", account.LoginName);
            wf.AddField("ip", account.IP);
            wf.AddField("deviceId", bl_LoginProDataBase.UseDeviceId() ? bl_DataBaseUtils.DeviceIdentifier : "none");

            WebRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.BanList), wf, (result) =>
            {
                if (result.isError)
                {
                    result.PrintError();
                }
                else
                {
                    if (result.HTTPCode == 202)//found user IP in the banned users
                    {
                        banUserInfo = result.FromJson<BanUserInfo>();
                        if (banUserInfo == null)
                        {
                            Debug.LogWarning($"Detected ban but receive wrong response from server: {result.RawText}");
                            callback?.Invoke(false);
                            return;
                        }
                        BannedText.text = $"<b>Reason:</b> {banUserInfo.reason}";
                        authorText.text = $"<b>Author:</b> {banUserInfo.by}";
                        BannedUI.SetActive(true);
                        bl_ULoginLoadingWindow.Instance.SetActive(false);
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        callback?.Invoke(false);
                        return;
                    }
                    else
                    {
                        // Account is clean
                        if (!string.IsNullOrEmpty(result.Text)) result.Print();
                    }
                }
                callback?.Invoke(true);
            });
        }

        /// <summary>
        /// Check if user IP is banned
        /// </summary>
        public void VerifyIP(Action callback)
        {
            var ip = bl_LoginPro.Instance.CurrentIp;
            if (string.IsNullOrEmpty(ip))
            {
                callback?.Invoke();
                return;
            }

            bl_ULoginLoadingWindow.Instance.SetText("Checking client status with server...");
            var wf = CreateForm(false);
            wf.AddField("typ", DBCommands.BANLIST_EXIST_IP);
            wf.AddField("ip", ip);
            wf.AddField("deviceId", bl_LoginProDataBase.UseDeviceId() ? bl_DataBaseUtils.DeviceIdentifier : "none");

            WebRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.BanList), wf, (result) =>
              {
                  if (result.isError)
                  {
                      result.PrintError();
                  }
                  else
                  {
                      if (result.HTTPCode == 202)//found user IP in the banned users
                      {
                          banUserInfo = result.FromJson<BanUserInfo>();
                          if (banUserInfo == null)
                          {
                              Debug.LogWarning($"Detected ban but receive wrong response from server: {result.RawText}");
                              callback?.Invoke();
                              return;
                          }
                          BannedText.text = $"<b>Reason:</b> {banUserInfo.reason}";
                          authorText.text = $"<b>Author:</b> {banUserInfo.by}";
                          BannedUI.SetActive(true);
                          Cursor.lockState = CursorLockMode.None;
                          Cursor.visible = true;
                          bl_ULoginLoadingWindow.Instance.SetActive(false);
                          return;
                      }
                      else
                      {
                          //User IP is clean
                          if (!string.IsNullOrEmpty(result.Text)) result.Print();
                      }
                  }
                  callback?.Invoke();
              });
        }

        private bl_LoginPro Login => bl_LoginPro.Instance;

        [System.Serializable]
        public class BanUserInfo
        {
            public string id;
            public string name;
            public string reason;
            public string ip;
            public string by;
        }
    }
}
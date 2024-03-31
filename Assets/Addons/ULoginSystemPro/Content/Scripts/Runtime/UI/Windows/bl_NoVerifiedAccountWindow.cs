using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_NoVerifiedAccountWindow : bl_LoginProBase
    {
        public int allowResendEach = 60;
        public Button reSendButton;
        public GameObject resentUI;
        public GameObject waitResentUI;
        public TMP_InputField emailInput;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            SetupResent();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResendEmail()
        {
            if (!emailInput.gameObject.activeSelf)
            {
                emailInput.text = "";
                emailInput.gameObject.SetActive(true);
                return;
            }

            var email = emailInput.text;
            if (string.IsNullOrEmpty(email)) return;

            var wf = CreateForm(FormHashParm.ID, true);
            wf.AddSecureField("id", "0");
            wf.AddSecureField("email", email);
            wf.AddSecureField("type", DBCommands.ACCOUNT_RESENT_VERIFICATION);

            reSendButton.interactable = false;
            WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.Account), wf, (r) =>
              {
                  reSendButton.interactable = true;
                  if (r.isError)
                  {
                      r.PrintError();
                      return;
                  }

                  if (r.HTTPCode == 202)
                  {
                      bl_LoginPro.Instance.SetLogText("Verification email sent!");
                      OnResent();
                      SetupResent();
                      if(ULoginSettings.FullLogs)
                      {
                          Debug.Log($"Resent result: ({r.HTTPCode}) " + r.Text);
                      }
                  }
                  else if (r.HTTPCode == 204)
                  {
                      // Account with the given email not found

                      // Is a standard to not tell the user that an account with the given email was not found in this case
                      // Since most likely is someone trying by pass and fail to discover accounts emails.
                      bl_LoginPro.Instance.SetLogText("Verification email sent!");
                      Debug.Log("No account with this email has been found, no confirmation email has been sent.");
                      OnResent();
                      SetupResent();
                  }
                  else if (r.HTTPCode == 206)
                  {
                      // Email couldn't be sent, due internal server errors
                      Debug.LogWarning("Email couldn't be sent, check your server email settings: " + r.Text);
                  }
                  else
                  {
                      Debug.LogWarning($"Unknown response: ({r.HTTPCode}) " + r.Text);
                  }
              });
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupResent()
        {
            var canResent = CanReSent();
            resentUI.SetActive(canResent);
            waitResentUI.SetActive(!canResent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CanReSent()
        {
            var now = DateTime.UtcNow;
            var lastTime = PlayerPrefs.GetString(Key, "");

            if (string.IsNullOrEmpty(lastTime)) return true;

            var formatLocale = CultureInfo.CurrentCulture;
            var lastDate = DateTime.Parse(lastTime, formatLocale);

            TimeSpan diff = now - lastDate;
            return diff.TotalSeconds > (double)allowResendEach;
        }

        private void OnResent()
        {
            var formatLocale = CultureInfo.CurrentCulture;
            string date = DateTime.UtcNow.ToString(formatLocale);
            PlayerPrefs.SetString(Key, date);
        }

        private string Key => $"com.login.resenttime";
    }
}
using System;
using UnityEngine.Analytics;

namespace MFPS.ULogin
{
    public class bl_LoginProSecurity : bl_LoginProBase
    {
        public void RequestRSAPublicKey()
        {
            RequestRSAPublicKey((result) =>
            {
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestRSAPublicKey(Action<string> callback)
        {
            var wf = CreateForm(false);
            wf.AddField("keygen", 1);
            wf.AddField("session_id", AnalyticsSessionInfo.sessionId.ToString());

            var url = GetURL(bl_LoginProDataBase.URLType.Security);
            WebRequest.POST(url, wf, (result) =>
              {
                  if (result.isError)
                  {
                      result.PrintError();
                      return;
                  }

                  if(ULoginSettings.FullLogs) result.Print();
                  bl_DataBase.Instance.RSAPublicKey = result.Text;
                  callback?.Invoke(result.Text);
              });
        }

        private static bl_LoginProSecurity _instance;
        public static bl_LoginProSecurity Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_LoginProSecurity>(); }
                return _instance;
            }
        }
    }
}
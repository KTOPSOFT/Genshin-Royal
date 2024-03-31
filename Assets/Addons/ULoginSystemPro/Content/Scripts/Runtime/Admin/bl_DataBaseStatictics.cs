using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_DataBaseStatictics : MonoBehaviour
    {

        [SerializeField] private Text PlayersCountText = null;
        [SerializeField] private Text BanPlayersCountText = null;
        [SerializeField] private Text LastPlayersCountText = null;
        [SerializeField] private Text GamePlayTimeText = null;

        private bl_ULoginWebRequest _webRequest;
        public bl_ULoginWebRequest WebRequest { get { if (_webRequest == null) { _webRequest = new bl_ULoginWebRequest(this); } return _webRequest; } }

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (bl_LoginProDataBase.Instance.PeerToPeerEncryption)
            {
                if(bl_DataBase.Instance == null)
                {
                    var go = new GameObject("DataBase");
                    go.AddComponent<bl_DataBase>();
                }
                if (string.IsNullOrEmpty(bl_DataBase.Instance.RSAPublicKey))
                {
                    bl_LoginProSecurity.Instance.RequestRSAPublicKey((r) =>
                    {
                        GetDataBaseStats();
                    });
                    return;
                }
            }

            GetDataBaseStats();
        }

        /// <summary>
        /// 
        /// </summary>
        void GetDataBaseStats()
        {
            bl_AdminWindowManager.SetLoading(true);

            var wf = bl_DataBaseUtils.CreateWWWForm(false, false);
            wf.AddField("sid", AnalyticsSessionInfo.sessionId.ToString());
            wf.AddSecureField("name", "admin");
            wf.AddSecureField("type", DBCommands.ADMIN_GET_DB_STATS);
            wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash("admin"));

            var url = bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Admin);
            WebRequest.POST(url, wf, (result) =>
            {
                bl_AdminWindowManager.SetLoading(false);
                if (result.isError)
                {
                    Debug.Log(result.RawText);
                    result.PrintError();
                    return;
                }

                if (result.resultState == ULoginResult.Status.Success)
                {
                    string[] raw = result.RawText.Split("|"[0]);
                    PlayersCountText.text = raw[1];
                    LastPlayersCountText.text = raw[2];
                    BanPlayersCountText.text = raw[3];
                    GamePlayTimeText.text = bl_DataBaseUtils.TimeFormat(raw[4].ToInt());
                }
                else
                {
                    result.Print(true);
                }
            });
        }
    }
}
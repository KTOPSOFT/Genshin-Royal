using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

namespace MFPS.ULogin
{
    public class bl_UserSupport : bl_LoginProBase
    {
        public float WindowSize = 141;
        [Range(1, 100)] public float ShowSpeed = 100;
        public MonoBehaviour[] disableOnOpen;

        [Header("References")]
        [SerializeField] private GameObject LoginBlock = null;
        [SerializeField] private GameObject ReplyWindow = null;
        [SerializeField] private TMP_InputField TitleInput = null;
        [SerializeField] private TMP_InputField ContentInput = null;
        [SerializeField] private Button SummitButton = null;
        [SerializeField] private Button CloseButton = null;
        [SerializeField] private TextMeshProUGUI MessageText = null;
        [SerializeField] private TextMeshProUGUI ReplyText = null;
        [SerializeField] private GameObject Loading = null;
        [SerializeField] private RectTransform WindowTransform = null;

        private bl_LoginPro LoginPro;
        private bool sending = false;
        private int PendingReplyID = 0;
        private bool ShowWindow = false;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            LoginPro = FindObjectOfType<bl_LoginPro>();
            LoginBlock.SetActive(true);
            ReplyWindow.SetActive(false);
            Loading.SetActive(false);
            SummitButton.interactable = false;
            CloseButton.interactable = false;
        }

        public void OnLogin()
        {
            LoginBlock.SetActive(false);
            StartCoroutine(CheckTickets());
        }

        public void OnLogOut()
        {
            LoginBlock.SetActive(true);
        }

        public void Send()
        {
            if (sending || DataBase == null || !DataBase.isLogged)
                return;
            if (string.IsNullOrEmpty(TitleInput.text) || string.IsNullOrEmpty(ContentInput.text))
                return;

            StartCoroutine(SummitTicket());
        }

        public void CheckTexts()
        {
            SummitButton.interactable = (TitleInput.text.Length > 2 && ContentInput.text.Length > 7);
        }

        public void Show()
        {
            ShowWindow = !ShowWindow;
            StopCoroutine("ShowWindowIE");
            StartCoroutine("ShowWindowIE", ShowWindow);

            foreach (var item in disableOnOpen)
            {
                if (item == null) continue;
                item.enabled = !ShowWindow;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator SummitTicket()
        {
            Loading.SetActive(true);
            SummitButton.interactable = false;
            sending = true;

            var content = bl_DataBaseUtils.SanitazeString(ContentInput.text);
            var title = bl_DataBaseUtils.SanitazeString(TitleInput.text);

            WWWForm wf = new WWWForm();
            string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.NickName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddField("hash", hash);
            wf.AddField("name", DataBase.LocalUser.NickName);
            wf.AddField("title", title);
            wf.AddField("content", content);
            wf.AddField("type", DBCommands.SUPPORT_SUBMIT_TICKET);

            //Request public IP to the server
            using (UnityWebRequest w = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Support), wf))
            {
                //Wait for response
                yield return w.SendWebRequest();

                if (!bl_DataBaseUtils.IsNetworkError(w))
                {
                    if (w.downloadHandler.text.Contains("success"))
                    {
                        LoginPro.SetLogText("Summited!");
                        MessageText.text = ContentInput.text;
                        ReplyText.text = "AWAITING FOR REPLY...";
                        CloseButton.interactable = false;
                        ReplyWindow.SetActive(true);
                    }
                    else { Debug.LogWarning(w.downloadHandler.text); }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            sending = false;
            Loading.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckTickets()
        {
            Loading.SetActive(true);
            SummitButton.interactable = false;
            sending = true;
            WWWForm wf = new WWWForm();
            string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.NickName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddField("hash", hash);
            wf.AddField("name", DataBase.LocalUser.NickName);
            wf.AddField("type", DBCommands.SUPPORT_CHANGE_TICKET);

            //Request public IP to the server
            using (UnityWebRequest w = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Support), wf))
            {
                //Wait for response
                yield return w.SendWebRequest();
                if (w.error == null)
                {
                    string[] split = w.downloadHandler.text.Split("|"[0]);
                    if (split[0].Contains("reply"))
                    {
                        MessageText.text = split[1];
                        string reply = split[2];
                        if (string.IsNullOrEmpty(reply))
                        {
                            reply = "AWAITING FOR REPLY...";
                        }
                        else
                        {
                            CloseButton.interactable = true;
                        }
                        ReplyText.text = reply;
                        PendingReplyID = int.Parse(split[3]);
                        ReplyWindow.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            sending = false;
            Loading.SetActive(false);
        }

        public void CloseTicket()
        {
            CloseButton.interactable = false;
            StartCoroutine(CloseTicketIE());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator CloseTicketIE()
        {
            Loading.SetActive(true);
            SummitButton.interactable = false;
            sending = true;
            WWWForm wf = new WWWForm();
            string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.NickName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddField("hash", hash);
            wf.AddField("name", DataBase.LocalUser.NickName);
            wf.AddField("id", PendingReplyID);
            wf.AddField("type", DBCommands.SUPPORT_CLOSE_TICKET);

            //Request public IP to the server
            using (UnityWebRequest w = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Support), wf))
            {
                //Wait for response
                yield return w.SendWebRequest();
                if (!bl_DataBaseUtils.IsNetworkError(w))
                {
                    if (w.downloadHandler.text.Contains("success"))
                    {
                        LoginPro.SetLogText("Close ticket!");
                        ReplyWindow.SetActive(false);
                    }
                    else { Debug.LogWarning(w.downloadHandler.text); }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            sending = false;
            Loading.SetActive(false);
        }

        IEnumerator ShowWindowIE(bool show)
        {
            Vector2 v = WindowTransform.anchoredPosition;
            if (show)
            {
                while (v.x > 0)
                {
                    v.x -= Time.deltaTime * (ShowSpeed * 10);
                    WindowTransform.anchoredPosition = v;
                    yield return null;
                }
                v.x = 0;
                WindowTransform.anchoredPosition = v;
            }
            else
            {
                while (v.x < WindowSize)
                {
                    v.x += Time.deltaTime * (ShowSpeed * 10);
                    WindowTransform.anchoredPosition = v;
                    yield return null;
                }
                v.x = WindowSize;
                WindowTransform.anchoredPosition = v;
            }
        }
    }
}
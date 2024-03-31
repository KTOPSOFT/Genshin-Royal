using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace MFPS.ULogin
{
    public class bl_SupportTicketManager : MonoBehaviour
    {
        [SerializeField] private GameObject TicketViewUI = null;
        [SerializeField] private GameObject TicketPrefab = null;
        [SerializeField] private Transform TicketsPanel = null;
        [SerializeField] private Text MessageText = null;
        [SerializeField] private Text TitleText = null;
        [SerializeField] private Text NameText = null;
        [SerializeField] private InputField ReplyInput = null;

        private bl_SupportTicket CurrentTicket;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            TicketViewUI.SetActive(false);
            LoadTickets();
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadTickets()
        {
            StartCoroutine(GetTickets());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GetTickets()
        {
            WWWForm wf = new WWWForm();
            string hash = bl_DataBaseUtils.Md5Sum("dev" + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddField("hash", hash);
            wf.AddField("name", "dev");
            wf.AddField("type", DBCommands.SUPPORT_GET_TICKETS);

            //Request public IP to the server
            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
            {
                //Wait for response
                yield return w.SendWebRequest();
                if (!bl_DataBaseUtils.IsNetworkError(w))
                {
                    string[] tickets = w.downloadHandler.text.Split("&&"[0]);
                    List<bl_SupportTicket.Ticket> List = new List<bl_SupportTicket.Ticket>();
                    for (int i = 0; i < tickets.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(tickets[i]) && tickets[i].Length > 2)
                        {
                            string[] info = tickets[i].Split("|"[0]);
                            bl_SupportTicket.Ticket t = new bl_SupportTicket.Ticket();
                            t.Title = info[0];
                            t.Message = info[1];
                            t.Reply = info[2];
                            t.ID = int.Parse(info[3]);
                            t.User = info[4];
                            List.Add(t);
                        }
                    }
                    InstanceTickets(List);
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket"></param>
        public void SelectTicket(bl_SupportTicket ticket)
        {
            CurrentTicket = ticket;
            MessageText.text = ticket.cacheInfo.Message;
            TitleText.text = ticket.cacheInfo.Title;
            NameText.text = ticket.cacheInfo.User;
            ReplyInput.text = (string.IsNullOrEmpty(ticket.cacheInfo.Reply)) ? "Reply..." : ticket.cacheInfo.Reply;
            TicketViewUI.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reply()
        {
            if (CurrentTicket == null || ReplyInput.text == string.Empty)
                return;

            StartCoroutine(ReplyTicket());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator ReplyTicket()
        {
            var reply = ReplyInput.text;
            reply = SanitazeText(reply);

            WWWForm wf = new WWWForm();
            string hash = bl_DataBaseUtils.Md5Sum("dev" + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddField("hash", hash);
            wf.AddField("name", "dev");
            wf.AddField("id", CurrentTicket.cacheInfo.ID);
            wf.AddField("reply", reply);
            wf.AddField("type", DBCommands.SUPPORT_REPLY_TICKET);

            //Request public IP to the server
            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
            {
                //Wait for response
                yield return w.SendWebRequest();
                if (!bl_DataBaseUtils.IsNetworkError(w))
                {
                    if (w.downloadHandler.text.Contains("success"))
                    {
                        Destroy(CurrentTicket.gameObject);
                        CurrentTicket = null;
                        ReplyInput.text = string.Empty;
                        MessageText.text = string.Empty;
                        TitleText.text = string.Empty;
                        NameText.text = string.Empty;
                    }
                    else { Debug.LogWarning(w.downloadHandler.text); }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string SanitazeText(string input)
        {
            input = bl_DataBaseUtils.SanitazeString(input);
            return input;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        void InstanceTickets(List<bl_SupportTicket.Ticket> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GameObject g = Instantiate(TicketPrefab) as GameObject;
                g.transform.SetParent(TicketsPanel, false);
                g.GetComponent<bl_SupportTicket>().GetInfo(list[i], this);
                g.gameObject.SetActive(true);
            }
            TicketPrefab.SetActive(false);
        }
    }
}
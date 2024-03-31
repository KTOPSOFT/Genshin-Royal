using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace MFPS.Addon.Avatars
{
    public class bl_KillCamUIPro : bl_KillCamUIBase
    {
        [SerializeField] private GameObject content = null;
        public bl_EmblemRenderBase avatarRender;
        public bl_CallingCardRenderBase callingCardRender;
        [SerializeField] private TextMeshProUGUI KillerNameText = null;
        [SerializeField] private TextMeshProUGUI GunNameText = null;
        [SerializeField] private TextMeshProUGUI respawnCountdown = null;
        public TextMeshProUGUI levelNumberText = null;
        [SerializeField] private Image GunImage = null;
        public Image levelIcon;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            OnPropertiesReset();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.onRoomPropertiesReset += OnPropertiesReset;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onRoomPropertiesReset -= OnPropertiesReset;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnPropertiesReset()
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cardID", $"{bl_EmblemsDataBase.GetUserEmblem().GetID()}&{bl_EmblemsDataBase.GetUserCallingCard().GetID()}");
            bl_PhotonNetwork.LocalPlayer.SetCustomProperties(data);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="gunID"></param>
        public override void Show(bl_KillCamBase.KillCamInfo killCamInfo)
        {
            var killer = killCamInfo.TargetName;
            if (string.IsNullOrEmpty(killer) && killCamInfo.Target != null)
            {
                killer = killCamInfo.Target.name;
            }
            
            killer = killer.Replace("(die)", "");
            
            content.SetActive(true);
            var info = bl_GameData.Instance.GetWeapon(killCamInfo.GunID);

            GunImage.sprite = info.GunIcon;
            GunNameText.text = info.Name.ToUpper();

            if (!string.IsNullOrEmpty(killer)) killer = killer.Replace("(die)", "");
            KillerNameText.text = killer;

            levelIcon.gameObject.SetActive(false);
            StartCoroutine(RespawnCountDown());

            MFPSPlayer actor = bl_GameManager.Instance.FindActor(killer);
            if (actor == null)
            {             
                Debug.LogWarning($"Couldn't found the player {killer}.");
                return;
            }
            
            if (actor.isRealPlayer)
            {
#if LM
                if (actor.ActorView != null)
                {
                    var level = bl_LevelManager.Instance.GetPlayerLevelInfo(actor.ActorView.Owner);
                    levelIcon.sprite = level.Icon;
                    levelNumberText.text = level.LevelID.ToString();
                    levelIcon.gameObject.SetActive(true);
                }
#endif

                if (killCamInfo.RealPlayer.CustomProperties.ContainsKey("cardID"))
                {
                    var cardInfo = (string)killCamInfo.RealPlayer.CustomProperties["cardID"];
                    var split = cardInfo.Split('&');
                    int avatarID = int.Parse(split[0]);
                    int cardID = int.Parse(split[1]);
                    if (avatarRender != null) avatarRender.Render(bl_EmblemsDataBase.GetEmblem(avatarID));
                    if (callingCardRender != null) callingCardRender.Render(bl_EmblemsDataBase.GetCallingCard(cardID));
                }
                else
                {
                    Debug.LogWarning("Player card id data has not been sync yet.");
                }

            }
            else
            {
                if (avatarRender != null) avatarRender.Render(bl_EmblemsDataBase.GetEmblem(0));
                if (callingCardRender != null) callingCardRender.Render(bl_EmblemsDataBase.GetCallingCard(0));

#if LM
                var botInfo = bl_AIMananger.Instance.GetBotStatistics(killer);
                if (botInfo != null)
                {
                    var level = bl_LevelManager.Instance.GetLevel(botInfo.Score);
                    levelIcon.sprite = level.Icon;
                    levelNumberText.text = level.LevelID.ToString();
                    levelIcon.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Couldn't found the bot {killer}.");
                }
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator RespawnCountDown()
        {
            float d = 0;
            float rt = bl_GameData.Instance.PlayerRespawnTime;
            while (d < 1)
            {
                d += Time.deltaTime / rt;
                int remaing = Mathf.FloorToInt(rt * (1 - d));
                remaing = Mathf.Max(0, remaing);
                respawnCountdown.text = string.Format(bl_GameTexts.RespawnIn.Localized(38), remaing);
                yield return null;
            }
        }
    }
}
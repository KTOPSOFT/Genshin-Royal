using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;

namespace MFPS.Addon.Clan
{
    public class bl_ClanCreation : bl_ClanBase
    {
        public TMP_InputField NameField;
        public TMP_InputField tagInput;
        public TMP_InputField DescriptionText;
        public Button CreateButton;
        public TextMeshProUGUI LogText;
        [SerializeField] private bl_TagColorSelector colorSelector = null;
        [SerializeField] private bl_ClanPurchaseWindow purchaseWindow = null;
        public bl_EventHandler.UEvent onClanCreated;

        public bool Invitations { get; set; }
        public bool isPublic { get; set; }
        private bool isRequesting = false;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            NameField.characterLimit = bl_ClanSettings.Instance.clanNameCharLimit;
            tagInput.characterLimit = bl_ClanSettings.Instance.clanTagCharLimit;
        }

        /// <summary>
        /// 
        /// </summary>
        public void IntentCreation()
        {
            if (!ValidateInputs()) return;

            purchaseWindow.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Create(int coinID)
        {
            if (isRequesting) return;
#if CLANS
            if (!bl_UserWallet.HasFundsFor(ClanSettings.clanCreationPrice, bl_MFPS.Coins.GetAllCoins()))
            {
                LogText.text = "You don't have enough coins to create a clan.";
                return;
            }
#endif
            string clanName = NameField.text;
            string des = DescriptionText.text;
            string clanTag = tagInput.text;


            isRequesting = true;
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.CREATE_CLAN);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanName);
            wf.AddField("desc", des);
            wf.AddField("userID", bl_DataBase.Instance.LocalUser.ID);
            string settings = string.Format("{0},{1},{2}", Invitations ? 1 : 0, isPublic ? 1 : 0, colorSelector.SelectedColor);
            wf.AddField("settings", settings);
            wf.AddField("clanTag", clanTag);

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
             {
                 ShowLoading(false);
                 isRequesting = false;
                 if (r.isError) { r.PrintError(); return; }


                 string t = r.Text;
                 if (t.Contains("done"))
                 {
#if CLANS
                     string[] split = t.Split("|"[0]);
                     int ci = int.Parse(split[1]);
                     if (bl_DataBase.Instance.LocalUser.Clan == null) bl_DataBase.Instance.LocalUser.Clan = new bl_ClanInfo();

                     var clan = bl_DataBase.Instance.LocalUser.Clan;
                     bl_DataBase.Instance.LocalUser.Clan.ID = ci;
                     clan.Members.Add(new bl_ClanInfo.ClanMember()
                     {
                         ID = bl_DataBase.LocalUserInstance.ID,
                         Name = bl_DataBase.LocalUserInstance.NickName,
                         Role = ClanMemberRole.Leader
                     });
                     //convert the price to the coin value
                     int coinPrice = bl_MFPS.Coins.GetCoinData(coinID).DoConversion(ClanSettings.clanCreationPrice);
                     bl_DataBase.Instance.SubtractCoins(coinPrice, coinID);
#endif
                     NameField.text = string.Empty;
                     DescriptionText.text = string.Empty;
                     bl_ClanManager.Instance.GetUserClan(() =>
                     {
                         onClanCreated?.Invoke();
                         bl_UserProfile.Instance?.OnLogin();
                         bl_EventHandler.DispatchCoinUpdate(null);
                         gameObject.SetActive(false);
                     });
                 }
                 else
                 {
                     LogText.text = t;
                     Debug.LogWarning(t);
                 }
             });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ValidateInputs()
        {
            string clanName = NameField.text;
            string des = DescriptionText.text;
            string clanTag = tagInput.text;
            if (string.IsNullOrEmpty(clanName) || string.IsNullOrEmpty(des) || string.IsNullOrEmpty(clanTag))
            {
                LogText.text = "One or more fields are not assigned";
                Debug.Log("One or more fields are not assigned");
                return false;
            }
            if (!Regex.IsMatch(clanName, @"^[a-zA-Z0-9_-]*$"))
            {
                LogText.text = "Clan Name contain no allowed characters.";
                Debug.Log("Clan Name contain no allowed characters.");
                return false;
            }
            if (!Regex.IsMatch(clanTag, @"^[a-zA-Z0-9]*$"))
            {
                LogText.text = "Clan Tag can only contain letters and numbers";
                Debug.Log("Clan Tag contain no allowed characters.");
                return false;
            }
            if (!Regex.IsMatch(des, ClanCommands.CLAN_DESCRIPTION_REGEX))
            {
                LogText.text = "Description contain no allowed characters.";
                Debug.Log("Description contain no allowed characters.");
                return false;
            }
            return true;
        }

    }
}
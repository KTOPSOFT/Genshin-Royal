using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.PlayerSelector
{
    public class bl_PSOperatorUI : MonoBehaviour
    {
        public Image PreviewImage;
        public GameObject BlockedUI;
        public GameObject SelectedUI;

        private bl_PlayerSelectorLobby Manager;
        private bl_PlayerSelectorInfo cacheInfo;
        public bool isBlocked { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public void SetUp(bl_PlayerSelectorInfo info, bl_PlayerSelectorLobby manager)
        {
            Manager = manager;
            cacheInfo = info;
            PreviewImage.sprite = info.Preview;
            SelectedUI.SetActive(info.isEquipedOne());

            int playerID = bl_PlayerSelectorData.Instance.GetPlayerID(info.Name);
            if (!info.Unlockability.IsUnlocked(playerID))
            {
                isBlocked = true;
                BlockedUI.SetActive(true);
            }
            else
            {
                isBlocked = false;
                BlockedUI.SetActive(false);
            }
        }

        public void OnOver()
        {
            Manager.OnShowUpOp(cacheInfo);
        }

        public void OnExit()
        {
            Manager.ShowUpSelectedOne(cacheInfo.team);
        }

        public void SelectThis()
        {
            if (isBlocked) return;

            Manager.SelectOperator(cacheInfo);
            bl_PSOperatorUI[] all = transform.parent.GetComponentsInChildren<bl_PSOperatorUI>();
            foreach(bl_PSOperatorUI ui in all)
            {
                ui.SelectedUI.SetActive(false);
            }
            SelectedUI.SetActive(true);
        }
    }
}
using MFPS.Runtime.UI;
using UnityEngine;

namespace MFPS.Addon.Avatars
{
    public class bl_EmblemUnlockability : MonoBehaviour
    {
        [SerializeField] private GameObject blockUI = null;
        [SerializeField] private GameObject equippedUI = null;
        [SerializeField] private bl_MFPSCoinPriceUI priceUI = null;
        [SerializeField] private GameObject selectedUI = null;

        private bl_EmblemSelector selectorManager;
        public EmblemData Avatar { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void Setup(EmblemData avatar, bl_EmblemSelector selector)
        {
            Avatar = avatar;
            selectorManager = selector;

            bool unlock = avatar.Unlockability.IsUnlocked(avatar.GetID());

            blockUI.SetActive(!unlock);
            if (equippedUI != null) equippedUI.SetActive(false);
            priceUI.SetPrice(avatar.Unlockability);

            bool showCoins = !unlock;
            if (showCoins && !avatar.Unlockability.CanBePurchased())
            {
                showCoins = false;
            }
            priceUI.SetActive(showCoins);
            selectedUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnSelected()
        {
            selectorManager.OnSelectAvatar(this);
            selectedUI.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unselect()
        {
            selectedUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void MarkAsEquipped(bool mark)
        {
            if (equippedUI != null) equippedUI.SetActive(mark);
        }
    }
}
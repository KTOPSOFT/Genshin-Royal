using MFPS.Runtime.UI;
using TMPro;
using UnityEngine;

namespace MFPS.Addon.Avatars
{
    public class bl_CallingCardUnlockability : MonoBehaviour
    {
        [SerializeField] private GameObject blockUI = null;
        [SerializeField] private GameObject equippedUI = null;
        [SerializeField] private bl_MFPSCoinPriceUI priceUI = null;
        [SerializeField] private GameObject selectedUI = null;
        [SerializeField] private TextMeshProUGUI nameText = null;

        private bl_CallingCardSelector selectorManager;
        public CallingCardData CallingCard { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void Setup(CallingCardData card, bl_CallingCardSelector selector)
        {
            CallingCard = card;
            selectorManager = selector;
            bool unlock = card.Unlockability.IsUnlocked(card.GetID());
            blockUI.SetActive(!unlock);
            if (equippedUI != null) equippedUI.SetActive(false);
            priceUI.SetPrice(card.Unlockability);

            bool showCoins = !unlock;
            if (showCoins && !card.Unlockability.CanBePurchased())
            {
                showCoins = false;
            }
            priceUI.SetActive(showCoins);

            selectedUI.SetActive(false);
            if (nameText != null) nameText.text = card.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnSelected()
        {
            selectorManager.OnSelectCard(this);
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
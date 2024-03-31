using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

namespace MFPS.Shop
{
    public class bl_ShopItemUI : bl_ShopItemUIBase, IPointerEnterHandler
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI typeText;
        public Image[] Icons;
        public RectTransform BuyButton;
        public GameObject OwnedUI;
        public GameObject BuyUI;
        public GameObject levelBlockUI;
        public bl_PriceUI priceUI;
        public MonoBehaviour[] oneTimeUsed;
        public int ID { get; set; } = 0;
        public ShopItemType TypeID;

        private ShopProductData Info;
        private bool isOwned = false;
        private bool canPurchase = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public override void Setup(ShopProductData data)
        {
            foreach (Image i in Icons) { i.gameObject.SetActive(false); }
            priceUI?.SetActive(false);
            Info = data;
            ID = data.ID;

            TypeID = data.Type;
            NameText.text = Info.Name.ToUpper();
            string typeName = data.Type.ToString();
            typeName = string.Concat(typeName.Select(x => System.Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            typeText.text = typeName.Localized(data.Type.ToString().ToLower()).ToUpper();
            LayoutRebuilder.ForceRebuildLayoutImmediate(NameText.transform.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(typeText.transform.parent.GetComponent<RectTransform>());
            //that's kinda dirty but it works :)
            foreach (MonoBehaviour b in oneTimeUsed) { Destroy(b); }

            // If this item is free
            // IsUnlocked will return True if the local player is not logged or is a guest.
            // So a further conditional is required for that scenario.
            if (Info.UnlockabilityInfo.IsUnlocked(ID))
            {
#if ULSP
                // if the user has not been logged or if it's a guest, don't let him select the weapons.
                if((!bl_DataBase.IsUserLogged || bl_DataBase.IsGuest) && Info.UnlockabilityInfo.CanBePurchased())
                {
                    ShowBlockUI();
                }
                else
                {
                    // Means that the weapon is unlocked for this player
                    ShowOwnedUI();
                }
#else
                ShowOwnedUI();
#endif
            }
            else
            {
                if (Info.UnlockabilityInfo.CanBePurchased())
                {
                    ShowBlockUI();
                }
                else
                {
                    // If this object only can be unlocked by level up.
                    ShowBlockUI(false);
                }
            }

            int iconImageID = 0;
            if (Info.Type == ShopItemType.PlayerSkin) iconImageID = 1;
            else if (Info.Type == ShopItemType.WeaponCamo) iconImageID = 2;

            SetIcon(Info.GetIcon(), iconImageID);
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowBlockUI(bool requirePurchase = true)
        {
            priceUI.ShowPrices(Info.UnlockabilityInfo);
            priceUI.SetActive(requirePurchase);
            BuyUI.SetActive(requirePurchase);
            isOwned = false;
            canPurchase = requirePurchase;
            BuyButton.gameObject.SetActive(requirePurchase);
            OwnedUI.SetActive(false);
            if (levelBlockUI != null) levelBlockUI.SetActive(!requirePurchase);
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowOwnedUI()
        {
            BuyUI.SetActive(false);
            isOwned = true;
            canPurchase = false;
            GetComponent<Selectable>().interactable = false;
            OwnedUI.SetActive(true);
            if (levelBlockUI != null) levelBlockUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        void SetIcon(Sprite icon, int id)
        {
            Icons[id].gameObject.SetActive(true);
            Icons[id].sprite = icon;
        }

        public void OnBuy()
        {
#if ULSP && SHOP

            if (!bl_DataBase.IsUserLogged)
            {
                bl_ShopNotification.Instance?.Show("You need an account to make purchases.").Hide(3);
                Debug.LogWarning("You has to be login in order to make a purchase.");
                return;
            }
            else
            {
                if (bl_UserWallet.HasFundsFor(Info.Price))
                {
                    bl_ShopManager.Instance.PreviewItem(Info, BuyButton.position);
                }
                else
                {
                    bl_ShopManager.Instance.NoCoinsWindow.SetActive(true);
                }
            }
#else
                        Debug.LogWarning("You need have ULogin Pro enabled to use this addon");
            return;
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            bl_ShopManager.Instance.Preview(Info, isOwned, canPurchase);
        }
    }
}
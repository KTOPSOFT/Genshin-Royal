using MFPS.Tween;
using MFPS.Internal.Structures;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if SHOP
using MFPS.Shop;
#endif

namespace MFPS.Addon.Avatars
{
    public class bl_CallingCardSelector : MonoBehaviour
    {
        [SerializeField] private GameObject cardUITemplate = null;
        [SerializeField] private RectTransform panel = null;
        [SerializeField] private Button actionButton = null;

        private List<bl_CallingCardUnlockability> instances = new List<bl_CallingCardUnlockability>();
        private bl_CallingCardUnlockability selectedCard;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            InstanceCards();
            UnselectAll();
            if (actionButton != null) actionButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnActionButtonClick()
        {
            if (selectedCard == null) return;

            int cardID = selectedCard.CallingCard.GetID();
            if (selectedCard.CallingCard.Unlockability.IsUnlocked(cardID))
            {
                bl_EmblemsDataBase.EquipCallingCard(selectedCard.CallingCard, () =>
                {
                    MarkEquippedCard();
                });
            }
            else
            {
                if (selectedCard.CallingCard.Unlockability.CanBePurchased())
                {
#if SHOP
                    var shopProduct = new ShopProductData()
                    {
                        Name = selectedCard.CallingCard.Name,
                        Type = ShopItemType.CallingCard,
                        ID = cardID,
                        UnlockabilityInfo = selectedCard.CallingCard.Unlockability,
                    };
                    shopProduct.SetIcon(selectedCard.CallingCard.Card);
                    bl_CheckoutWindow.Instance?.Checkout(shopProduct, () =>
                    {
                        InstanceCards(true);
                    });
#endif
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstanceCards(bool force = false)
        {
            if (force)
            {
                foreach (var item in instances)
                {
                    Destroy(item.transform.parent.gameObject);
                }
                instances.Clear();
            }

            if (instances.Count > 0) return;

            var all = bl_EmblemsDataBase.Instance.callingCards;

            List<CallingCardData> owned = new List<CallingCardData>();
            if (bl_EmblemsDataBase.Instance.showOwnedFirst)
            {
                List<CallingCardData> nonOwned = new List<CallingCardData>();
                foreach (var card in all)
                {
                    if (card.Unlockability.IsUnlocked(card.GetID()))
                    {
                        owned.Add(card);
                    }
                    else
                    {
                        if (card.Unlockability.UnlockMethod != MFPSItemUnlockability.UnlockabilityMethod.Hidden)
                        {
                            nonOwned.Add(card);
                        }
                    }
                }

                owned.AddRange(nonOwned);
            }
            else
            {
                owned.AddRange(all);
            }

            foreach (var card in owned)
            {
                var go = Instantiate(cardUITemplate);
                go.transform.SetParent(panel, false);
                var script = go.GetComponent<bl_CallingCardRenderBase>();
                var unlockScript = go.GetComponentInChildren<bl_CallingCardUnlockability>();
                unlockScript.Setup(card, this);
                script.Render(card);
                instances.Add(unlockScript);
                go.SetActive(true);
            }

            MarkEquippedCard();
            cardUITemplate.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatar"></param>
        public void OnSelectCard(bl_CallingCardUnlockability avatar)
        {
            selectedCard = avatar;
            UnselectAll();
            bool equipped = avatar.CallingCard.IsEquipped();
            string buttonText = equipped ? "EQUIPPED" : "EQUIP";
            bool showButton = true;

            // if the avatar is locked
            if (!avatar.CallingCard.Unlockability.IsUnlocked(avatar.CallingCard.GetID()))
            {
                // but can be purchased
                if (avatar.CallingCard.Unlockability.CanBePurchased())
                    buttonText = "PURCHASE";
                else
                {
                    // the avatar is locked and require to achieve certain level to unlock
                    showButton = false;
                }
            }

            if (actionButton != null)
            {
                actionButton.interactable = equipped ? false : true;
                actionButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
                actionButton.gameObject.SetActiveTween(showButton);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void MarkEquippedCard()
        {
            foreach (var item in instances)
            {
                item.MarkAsEquipped(false);
            }
            var userCard = bl_EmblemsDataBase.GetUserCallingCard();
            var equipped = instances.Find(x => x.CallingCard == userCard);

            if (equipped != null) equipped.MarkAsEquipped(true);
            else Debug.LogWarning($"Calling Card {userCard.GetID()} not found.");
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnselectAll()
        {
            foreach (var item in instances)
            {
                item.Unselect();
            }
        }
    }
}
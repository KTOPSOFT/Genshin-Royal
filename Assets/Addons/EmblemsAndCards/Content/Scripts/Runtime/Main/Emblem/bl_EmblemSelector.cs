using MFPS.Tween;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MFPS.Internal.Structures;
#if SHOP
using MFPS.Shop;
#endif

namespace MFPS.Addon.Avatars
{
    public class bl_EmblemSelector : MonoBehaviour
    {
        [SerializeField] private GameObject avatarUITemplate = null;
        [SerializeField] private RectTransform panel = null;
        [SerializeField] private Button actionButton = null;

        private List<bl_EmblemUnlockability> instances = new List<bl_EmblemUnlockability>();
        private bl_EmblemUnlockability selectedAvatar;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            InstanceAvatars();
            UnselectAll();
            if(actionButton != null) actionButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnActionButtonClick()
        {
            if (selectedAvatar == null) return;

            int avatarID = selectedAvatar.Avatar.GetID();
            if (selectedAvatar.Avatar.Unlockability.IsUnlocked(avatarID))
            {
                bl_EmblemsDataBase.EquipEmblem(selectedAvatar.Avatar, () =>
                {
                    MarkEquippedAvatar();
                });
            }
            else
            {
                if(selectedAvatar.Avatar.Unlockability.CanBePurchased())
                {
#if SHOP
                    var shopProduct = new ShopProductData()
                    {
                        Name = "Avatar",
                        Type = ShopItemType.Emblem,
                        ID = avatarID,
                        UnlockabilityInfo = selectedAvatar.Avatar.Unlockability,                      
                    };
                    shopProduct.SetIcon(selectedAvatar.Avatar.Emblem);
                    bl_CheckoutWindow.Instance?.Checkout(shopProduct, () =>
                    {
                        InstanceAvatars(true);
                    });
#endif
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstanceAvatars(bool force = false)
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

            var all = bl_EmblemsDataBase.Instance.emblems;

            List<EmblemData> owned = new List<EmblemData>();
            if (bl_EmblemsDataBase.Instance.showOwnedFirst)
            {
                List<EmblemData> nonOwned = new List<EmblemData>();
                foreach (var avatar in all)
                {
                    if (avatar.Unlockability.IsUnlocked(avatar.GetID()))
                    {
                        owned.Add(avatar);
                    }
                    else
                    {
                        if (avatar.Unlockability.UnlockMethod != MFPSItemUnlockability.UnlockabilityMethod.Hidden)
                        {
                            nonOwned.Add(avatar);
                        }
                    }
                }

                owned.AddRange(nonOwned);
            }
            else
            {
                owned.AddRange(all);
            }

            foreach (var avatar in owned)
            {
                var go = Instantiate(avatarUITemplate);
                go.transform.SetParent(panel, false);
                var script = go.GetComponent<bl_EmblemRenderBase>();
                var unlockScript = go.GetComponentInChildren<bl_EmblemUnlockability>();
                unlockScript.Setup(avatar, this);
                script.Render(avatar);
                instances.Add(unlockScript);
                go.SetActive(true);
            }

            MarkEquippedAvatar();
            avatarUITemplate.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatar"></param>
        public void OnSelectAvatar(bl_EmblemUnlockability avatar)
        {
            selectedAvatar = avatar;
            UnselectAll();
            bool equipped = avatar.Avatar.IsEquipped();
            string buttonText = equipped ? "EQUIPPED" : "EQUIP";
            bool showButton = true;

            // if the avatar is locked
            if (!avatar.Avatar.Unlockability.IsUnlocked(avatar.Avatar.GetID()))
            {
                // but can be purchased
                if (avatar.Avatar.Unlockability.CanBePurchased())
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
        public void MarkEquippedAvatar()
        {
            foreach (var item in instances)
            {
                item.MarkAsEquipped(false);
            }
            var userAvatar = bl_EmblemsDataBase.GetUserEmblem();
            var equipped = instances.Find(x => x.Avatar == userAvatar);

            if (equipped != null) equipped.MarkAsEquipped(true);
            else Debug.LogWarning($"Avatar {userAvatar.GetID()} not found.");
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
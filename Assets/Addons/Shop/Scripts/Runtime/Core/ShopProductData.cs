using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MFPS.Internal.Structures;
#if PSELECTOR
using MFPS.Addon.PlayerSelector;
#endif
#if CUSTOMIZER
using MFPS.Addon.Customizer;
#endif

namespace MFPS.Shop
{
    [Serializable]
    public class ShopProductData
    {
        public string Name;
        public ShopItemType Type = ShopItemType.Weapon;
        public int ID;
        public MFPSItemUnlockability UnlockabilityInfo;
        public bl_GunInfo GunInfo;
#if PSELECTOR
        public bl_PlayerSelectorInfo PlayerSkinInfo;
#endif
#if CUSTOMIZER
        public GlobalCamo camoInfo;
#endif
        private Sprite m_Icon = null;

        public int Price
        {
            get => UnlockabilityInfo.Price;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Sprite GetIcon()
        {
            if (m_Icon != null) return m_Icon;

            switch (Type)
            {
                case ShopItemType.Weapon:
                    return GunInfo.GunIcon;
                case ShopItemType.PlayerSkin:
#if PSELECTOR
                    return PlayerSkinInfo.Preview;
#else
                    return null;
#endif
                case ShopItemType.WeaponCamo:
#if CUSTOMIZER
                    return camoInfo.spritePreview();
#else
                    return null;
#endif
                default:
                    Debug.LogWarning($"Shop item '{Type.ToString()}' has not implemented the icon getter yet.");
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(Texture2D icon)
        {
            SetIcon(Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(Sprite icon) => m_Icon = icon;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDataBaseIdentifier()
        {
            return $"{(int)Type},{ID}-";
        }

        /// <summary>
        /// Collect all the MFPS purchasable items (weapons, player skins, and weapon camos)
        /// </summary>
        /// <returns></returns>
        public static List<ShopProductData> FetchAllInGamePurchasableItems(bool includeFreeItems = false)
        {
            var Items = new List<ShopProductData>();

            var Weapons = bl_GameData.Instance.AllWeapons;
            for (int i = 0; i < Weapons.Count; i++)
            {
                var weapon = Weapons[i];
                if (!includeFreeItems && weapon.Unlockability.IsFree())
                {
                    continue;
                }

                var data = new ShopProductData();
                data.ID = i;
                data.Name = weapon.Name;
                data.Type = ShopItemType.Weapon;
                data.GunInfo = weapon;
                data.UnlockabilityInfo = weapon.Unlockability;
                Items.Add(data);
            }

#if PSELECTOR
            var allPlayers = bl_PlayerSelector.Data.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                var pinfo = allPlayers[i];
                if (!includeFreeItems && pinfo.Unlockability.IsFree())
                {
                    continue;
                }

                var data = new ShopProductData();
                data.ID = i;
                data.Name = pinfo.Name;
                data.Type = ShopItemType.PlayerSkin;
                data.PlayerSkinInfo = pinfo;
                data.UnlockabilityInfo = pinfo.Unlockability;
                Items.Add(data);
            }
#endif

#if CUSTOMIZER
            for (int i = 0; i < bl_CustomizerData.Instance.GlobalCamos.Count; i++)
            {
                if (i == 0) continue;//skip the default camo

                var gc = bl_CustomizerData.Instance.GlobalCamos[i];
                if (!includeFreeItems && gc.Unlockability.IsFree())
                {
                    continue;
                }

                var data = new ShopProductData();
                data.ID = i;
                data.Name = gc.Name;
                data.Type = ShopItemType.WeaponCamo;
                data.UnlockabilityInfo = gc.Unlockability;
                data.camoInfo = gc;
                Items.Add(data);
            }
#endif

            return Items;
        }
    }

    [Serializable]
    public class ShopCategoryInfo
    {
        public string Name;
        public ShopItemType itemType = ShopItemType.Weapon;
    }
}
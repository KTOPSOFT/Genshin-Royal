using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Clan
{
    public class bl_ClanPurchaseWindow : bl_ClanBase
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private bl_ClanCreation clanCreation;
        public bl_ClanPriceUI priceUI;

        /// <summary>
        /// 
        /// </summary>
        public void Open()
        {
            content.SetActive(true);
#if CLANS
            priceUI.ShowPricesButtons(ClanSettings.clanCreationPrice, (coinID) =>
             {
                 clanCreation.Create(coinID);
                 content.SetActive(false);
             });
#endif
        }
    }
}
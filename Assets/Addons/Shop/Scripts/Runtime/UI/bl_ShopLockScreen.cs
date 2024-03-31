using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MFPS.Shop
{
    public class bl_ShopLockScreen : MonoBehaviour
    {

        public GameObject content;
        public GameObject createAccountButton;
                
        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
#if ULSP
            if (bl_ShopData.Instance.allowPurchasesWithoutAccount)
            {
                content.SetActive(false);
                return;
            }

            if (!bl_DataBase.IsUserLogged)
            {
                content.SetActive(true);
                createAccountButton.SetActive(true);
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateAccount()
        {
            bl_UtilityHelper.LoadLevel("Login");
        }
    }
}
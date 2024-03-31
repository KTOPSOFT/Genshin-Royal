using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AccountDelectionWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI verificationText = null;
        [SerializeField] private TMP_InputField verificationInput = null;
        [SerializeField] private Button deleteButton = null;

        public bl_EventHandler.UEvent onAccountDelete;
        private string verificationKey = "Verification";
        
        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if(verificationText != null)
            {
                string vt = Application.productName.ToUpper();
                vt = vt.Replace(" ", "");
                vt = vt.Trim();
                verificationKey = vt;
                
                verificationText.text = verificationKey;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void ConfirmDeletion()
        {
            if (!bl_DataBase.IsUserLogged) return;
            if (verificationInput != null)
            {
                string inputStr = verificationInput.text;

                if (inputStr.ToLower() != verificationKey.ToLower())
                {
                    verificationInput.text = "";
                    Debug.Log("Wrong confirmation text");
                    return;
                }
            }

            verificationInput.text = "";
            deleteButton.interactable = false;
            bl_DataBase.DeleteAccount(bl_DataBase.LocalUserInstance.ID, bl_DataBase.LocalUserInstance.LoginName, (r) =>
            {
                if (r)
                {
                    onAccountDelete?.Invoke();
                }
                deleteButton.interactable = true;
            });
        }
    }
}
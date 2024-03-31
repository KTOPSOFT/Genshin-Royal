using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace MFPS.ULogin
{
    public class bl_PasswordField : MonoBehaviour
    {
        public TMP_InputField inputField;
        public Image iconImg;
        public Sprite HideIcon, VisibleIcon;

        /// <summary>
        /// 
        /// </summary>
        public void Switch()
        {
            bool isHidde = inputField.contentType == TMP_InputField.ContentType.Password;
            isHidde = !isHidde;
            inputField.contentType = (isHidde) ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            inputField.ForceLabelUpdate();
            iconImg.sprite = (isHidde) ? HideIcon : VisibleIcon;
        }
    }
}
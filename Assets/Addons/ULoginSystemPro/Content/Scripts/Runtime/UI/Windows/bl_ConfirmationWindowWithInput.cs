using System;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_ConfirmationWindowWithInput : MonoBehaviour
    {
        public Text descriptionText;
        public InputField inputField;
        public GameObject content;

        private Action<string> callback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="onAccept"></param>
        public void AskConfirmation(string description, Action<string> onAccept, string inputPlaceholder = "")
        {
            callback = onAccept;
            descriptionText.text = description;
            if (inputField != null) inputField.text = inputPlaceholder;
            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Confirm()
        {
            callback?.Invoke(inputField.text);
            if (inputField != null) inputField.text = "";
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            callback = null;
            if (inputField != null) inputField.text = "";
            content.SetActive(false);
        }
    }
}
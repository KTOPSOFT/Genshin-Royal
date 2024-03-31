using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_ConfirmationWindow : MonoBehaviour
    {
        public Text descriptionText;
        public GameObject content;

        private Action callback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="onAccept"></param>
        public void AskConfirmation(string description, Action onAccept)
        {
            callback = onAccept;
            descriptionText.text = description;
            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Confirm()
        {
            callback?.Invoke();
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            callback = null;
            content.SetActive(false);
        }
    }
}
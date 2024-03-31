using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.ULogin
{
    public class bl_LoginBlockScreen : MonoBehaviour
    {
        public TextMeshProUGUI countText;
        public Button continueButton;

        private double remainingSeconds = 60;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            continueButton.interactable = false;
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        void Init()
        {
            string key = bl_DataBaseUtils.Encrypt(bl_DataBaseUtils.LOCK_TIME_KEY);
            string date = PlayerPrefs.GetString(key, string.Empty);

            if (string.IsNullOrEmpty(date))
            {
                continueButton.interactable = true;
                return;
            }

            date = bl_DataBaseUtils.Decrypt(date);
            var gl = new CultureInfo("en-US");
            var lockDate = DateTime.Parse(date, gl).ToUniversalTime();
            remainingSeconds = (lockDate - DateTime.Now.ToUniversalTime()).TotalSeconds;

            if (remainingSeconds > 1)
            {
                InvokeRepeating(nameof(Tick), 0, 1);
            }
            else
            {
                continueButton.interactable = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Tick()
        {
            remainingSeconds--;
            TimeSpan t = TimeSpan.FromSeconds(remainingSeconds);
            string date = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
            countText.text = date;
            if (t.Minutes <= 0 && t.Seconds <= 0)
            {
                CancelInvoke();
                string key = bl_DataBaseUtils.Encrypt(bl_DataBaseUtils.LOCK_TIME_KEY);
                PlayerPrefs.DeleteKey(key);
                continueButton.interactable = true;
                bl_LoginPro.Instance.SetPlayAsGuestButtonActive(true);
            }
        }
    }
}
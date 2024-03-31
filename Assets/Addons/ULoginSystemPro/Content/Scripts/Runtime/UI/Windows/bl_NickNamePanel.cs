using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_NickNamePanel : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI descriptionText = null;
        private Action<string> onSet;

        public void SetCallback(Action<string> callback, string description = "")
        {
            if (!string.IsNullOrEmpty(description))
            {
                descriptionText.text = description;
            }
            onSet = callback;
        }

        public void Set(TMP_InputField field)
        {
            string user = field.text;

            if (string.IsNullOrEmpty(user))
            {
                bl_LoginPro.Instance.SetLogText("Nickname is empty.");
                return;
            }
            if (user.Length < 3)
            {
                bl_LoginPro.Instance.SetLogText("Nick name need to have at least 3 or more characters length.");
                return;
            }
            if (!bl_DataBaseUtils.IsUsername(user))
            {
                bl_LoginPro.Instance.SetLogText("Nick Name contain not allowed characters.");
                return;
            }
            if (bl_LoginProDataBase.Instance.FilterName(user))
            {
                bl_LoginPro.Instance.SetLogText("Nick Name contain not allowed words");
                return;
            }
            onSet?.Invoke(user);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_InputCharCounterUI : MonoBehaviour
    {
        public string textFormat = "{0} / {1}";
        [SerializeField] private TMP_InputField m_inputField = null;
        [SerializeField] private TextMeshProUGUI m_text = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnChange(string value)
        {
           if(m_text) m_text.text = string.Format(textFormat, m_inputField.text.Length, m_inputField.characterLimit);
        }
    }
}
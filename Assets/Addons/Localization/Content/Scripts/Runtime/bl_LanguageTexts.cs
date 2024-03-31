using TMPro;
using UnityEngine;

namespace Lovatto.Localization
{
    public class bl_LanguageTexts : ScriptableObject
    {
        public string LanguageName = "";
        public string PlurarLetter = "s";
        public Sprite LanguageIcon;
        public TMP_FontAsset CustomFont;
        public TextData[] Data;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasCustomFont()
        {
            return CustomFont != null;
        }

        [ContextMenu("Print Texts")]
        void PrintText()
        {
            string t = "";
            for (int i = 0; i < Data.Length; i++)
            {
                t += Data[i].Text + "\n";
            }
            Debug.Log(t);
        }

        [System.Serializable]
        public class TextData
        {
            public string StringID = "";
            public string Text = "";
        }
    }
}
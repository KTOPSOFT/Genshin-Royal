using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lovatto.Localization;
using System;
using System.Text.RegularExpressions;

public class bl_Localization : ScriptableObject
{
    public bl_LanguageInfo DefaultLanguage;
    public bl_LanguageInfo[] Languages;

    public bool AllowEditsStringsID = false;
    public float ColumnsWidth = 0;

    public int CurrentLanguageID { get; set; }
    //event called when change of language in game
    public delegate void EEvent(Dictionary<string, string> newLanguage);
    public EEvent OnLanguageChange;

    private Dictionary<string, string> cachedText = new Dictionary<string, string>();
    public const string LANGUAGEKEY = "mfps.settings.language";

    /// <summary>
    /// get a text from the current language with the stringID
    /// </summary>
    /// <returns></returns>
    public string GetText(string key)
    {
        //if this text is cached
        if (cachedText.ContainsKey(key))
        {
            return cachedText[key];
        }

        //text is not cached yet, we need to located it
        bl_LanguageTexts texts = GetCurrentLanguage.Text;
        for (int i = 0; i < texts.Data.Length; i++)
        {
            if (texts.Data[i].StringID == key)
            {
                string text = texts.Data[i].Text;
                //cached this text in a dictionary to get it faster next time
                if (!cachedText.ContainsKey(key))
                {
                    cachedText.Add(key, text);
                }
                return text;
            }
        }
        return "No Defined: " + key;
    }

    /// <summary>
    /// get a text from the current language with the stringID
    /// </summary>
    /// <returns></returns>
    public bool TryGetText(string key, ref string locaizedText)
    {
        //if this text is cached
        if (cachedText.ContainsKey(key))
        {
            locaizedText = cachedText[key];
            return true;
        }

        //text is not cached yet, we need to located it
        bl_LanguageTexts texts = GetCurrentLanguage.Text;
        for (int i = 0; i < texts.Data.Length; i++)
        {
            if (texts.Data[i].StringID == key)
            {
                string text = texts.Data[i].Text;
                //cached this text in a dictionary to get it faster next time
                if (!cachedText.ContainsKey(key))
                {
                    cachedText.Add(key, text);
                }
                locaizedText = text;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// get a text from the current language with the stringID
    /// </summary>
    /// <returns></returns>
    public string GetTextPlural(string key)
    {
        string t = GetText(key);
        t += GetCurrentLanguage.Text.PlurarLetter;
        return t;
    }

    /// <summary>
    /// get a text from the current language with the stringID
    /// </summary>
    /// <returns></returns>
    public string GetTextPlural(int keyID)
    {
        string t = GetText(keyID);
        t += GetCurrentLanguage.Text.PlurarLetter;
        return t;
    }

    /// <summary>
    /// get a text from the current language with the specific id (recommended)
    /// </summary>
    /// <returns></returns>
    public string GetText(int index)
    {
        if (index > 0 && index < Languages[CurrentLanguageID].Text.Data.Length)
        {
            return Languages[CurrentLanguageID].Text.Data[index].Text;
        }
        return "No located ID: " + index;//means that you have not localized this command
    }

    /// <summary>
    /// get a array of localized text by a array of keys
    /// </summary>
    /// <param name="keysIDs"></param>
    /// <returns></returns>
    public string[] GetTextArray(int[] keysIDs)
    {
        string[] array = new string[keysIDs.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = GetText(keysIDs[i]);
        }
        return array;
    }

    /// <summary>
    /// change the current language
    /// </summary>
    /// <param name="id">id = index of the language in the Languages list</param>
    public void SetLanguage(int id)
    {
        if (id < 0 || id > Languages.Length - 1) return;

        CurrentLanguageID = id;
        PlayerPrefs.SetInt(LANGUAGEKEY, id);
        //reset cached texts
        cachedText.Clear();
        bl_LanguageTexts texts = GetCurrentLanguage.Text;
        for (int i = 0; i < texts.Data.Length; i++)
        {
            if (cachedText.ContainsKey(texts.Data[i].StringID))
            {
                Debug.LogWarning($"The StringID ({i}) '{texts.Data[i].StringID}' is assigned multiple times in the {texts.LanguageName} language texts.");
                continue;
            }
            cachedText.Add(texts.Data[i].StringID, texts.Data[i].Text);
        }
        if (OnLanguageChange != null)
        {
            OnLanguageChange(cachedText);
        }
    }

    /// <summary>
    /// Load the store language
    /// </summary>
    /// <param name="apply">change the language text after load</param>
    public int LoadStoreLanguage(bool apply)
    {
        CurrentLanguageID = PlayerPrefs.GetInt(LANGUAGEKEY, 0);
        if (apply)
        {
            SetLanguage(CurrentLanguageID);
        }
        return CurrentLanguageID;
    }

    public bl_LanguageInfo GetCurrentLanguage
    {
        get
        {
            return Languages[CurrentLanguageID];
        }
    }

    public List<string> GetIdsList()
    {
        List<string> list = new List<string>();
        for (int i = 0; i < DefaultLanguage.Text.Data.Length; i++)
        {
            list.Add(DefaultLanguage.Text.Data[i].StringID);
        }
        return list;
    }

    /// <summary>
    /// Check if a string contains a command to localized
    /// </summary>
    /// <returns></returns>
    public bool ParseCommad(ref string str)
    {
        Match m = Regex.Match(str, "<localized>(.*?)</localized>");
        if (m.Success)
        {
            string localized = GetText(m.Groups[1].Value);
            str = str.Replace(m.Groups[0].Value, localized);
            return true;
        }
        return false;
    }

    public static string AsCommand(string source)
    {
        return $"<localized>{source}</localized>";
    }

    /// <summary>
    /// Add a new text to localize
    /// NOTE: No intended to use in runtime, this function is for editor only
    /// </summary>
    public bool AddText(string key, string defaultText)
    {
        if(Array.Exists(DefaultLanguage.Text.Data,x => x.StringID == key))
        {
            Debug.Log("A key with this name already exist.");
            return false;
        }
        for (int i = 0; i < Languages.Length; i++)
        {
            if (Languages[i].Text == null) continue;
            bl_LanguageTexts.TextData[] data = new bl_LanguageTexts.TextData[Languages[i].Text.Data.Length + 1];
            for (int e = 0; e < data.Length - 1; e++)
            {
                data[e] = Languages[i].Text.Data[e];
            }
            data[data.Length - 1] = new bl_LanguageTexts.TextData();
            data[data.Length - 1].StringID = key;
            data[data.Length - 1].Text = defaultText;
            Languages[i].Text.Data = data;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Languages[i].Text);
#endif
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif
        return true;
    }

    public static string GetLocalizedText(string key) => Instance.GetText(key);
    public static string GetLocalizedText(int key) => Instance.GetText(key);

    public void FlushCache() => cachedText.Clear();

    private static bl_Localization m_Data;
    public static bl_Localization Instance
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("Localization", typeof(bl_Localization)) as bl_Localization;
            }
            return m_Data;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class bl_LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown m_Dropdown;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        int cid = bl_Localization.Instance.LoadStoreLanguage(true);
        m_Dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < bl_Localization.Instance.Languages.Length; i++)
        {
            TMP_Dropdown.OptionData o = new TMP_Dropdown.OptionData();
            o.text = bl_Localization.Instance.Languages[i].Text.LanguageName.ToUpper();
            o.image = bl_Localization.Instance.Languages[i].Text.LanguageIcon;
            options.Add(o);
        }
        m_Dropdown.AddOptions(options);
        m_Dropdown.value = cid;
    }

    public void OnChange(int id)
    {
        bl_Localization.Instance.SetLanguage(id);
    }
}
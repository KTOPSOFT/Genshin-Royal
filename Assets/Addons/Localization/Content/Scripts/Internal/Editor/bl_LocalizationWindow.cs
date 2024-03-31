using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Lovatto.Localization;
using System;
using UnityEditor.IMGUI.Controls;

public class bl_LocalizationWindow : EditorWindow
{
    private bl_Localization Localization;
    private bl_LanguageTexts DefaultLang;
    private Vector2 scrollArea;
    private Vector2 HorizontalScroll;
    private bool showComfirmDelete = false;
    private bool isComparing = false;
    int CompareTablet = 0;
    SearchField m_SearchField;
    private string SearchKeyworld = "";

    public int showPerPage = 50;
    public int currentPage = 0;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        minSize = new Vector3(1000, 500);
        titleContent = new GUIContent("Localization");
        Localization = bl_Localization.Instance;
        if (Localization != null)
        {
            DefaultLang = Localization.DefaultLanguage.Text;
        }
        m_SearchField = new SearchField();

        currentPage = Mathf.FloorToInt(DefaultLang.Data.Length / showPerPage);
        if (DefaultLang.Data.Length % showPerPage != 0)
        {
            currentPage++;
        }
        currentPage--;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        if (Localization == null || DefaultLang == null) return;

        DrawHeader();
        DrawMain();
        if (showComfirmDelete) { DeleteComfirmation(); }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawMain()
    {
        EditorStyles.toolbarButton.richText = true;
        EditorStyles.label.richText = true;
        scrollArea = GUILayout.BeginScrollView(scrollArea);
        GUILayout.BeginHorizontal();
        DrawIdsColumn();
        DrawLangsColumns();
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawIdsColumn()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(125));
        GUILayout.Label("TEXT ID", EditorStyles.toolbarButton);

        int startFrom = 0;
        int count = DefaultLang.Data.Length;
        if (count > showPerPage)
        {
            startFrom = showPerPage * currentPage;
            count = showPerPage;

            if (startFrom + count >= DefaultLang.Data.Length)
            {
                count = DefaultLang.Data.Length - startFrom;
            }
        }

        if (SearchKeyworld.Length >= 2)
        {
            startFrom = 0;
            count = DefaultLang.Data.Length;
        }

        for (int i = startFrom; i < startFrom + count; i++)
        {
            if (SearchKeyworld.Length >= 2)
            {
                if (!DefaultLang.Data[i].StringID.Contains(SearchKeyworld)) continue;
            }
            GUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.Label(i.ToString(), EditorStyles.helpBox, GUILayout.Width(30), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (Localization.AllowEditsStringsID)
            {
                DefaultLang.Data[i].StringID = EditorGUILayout.TextField(DefaultLang.Data[i].StringID, GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            else
            {
                GUILayout.Label(DefaultLang.Data[i].StringID, EditorStyles.whiteLabel, GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            if (i == DefaultLang.Data.Length - 1)
            {
                if (Localization.AllowEditsStringsID)
                {
                    if (GUILayout.Button("✔", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        List<bl_LanguageTexts.TextData> td = new List<bl_LanguageTexts.TextData>();
                        td.AddRange(DefaultLang.Data);
                        if (td.Exists(x => (x.StringID == DefaultLang.Data[i].StringID) && x != DefaultLang.Data[DefaultLang.Data.Length - 1]))
                        {
                            Debug.Log("Already exist a key with the name: " + DefaultLang.Data[i].StringID);
                        }
                        else
                        {
                            SyncIds();
                            Localization.AllowEditsStringsID = !Localization.AllowEditsStringsID;
                            Repaint();
                        }
                    }
                }
                else if (SearchKeyworld.Length < 2)
                {
                    if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        // RemoveLast();
                        showComfirmDelete = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(4);
        if (SearchKeyworld.Length < 2)
        {
            if (GUILayout.Button("ADD NEW", EditorStyles.toolbarButton))
            {
                AddNewText();
                Localization.AllowEditsStringsID = true;
            }
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawLangsColumns()
    {
        if (!isComparing)
        {
            for (int i = 0; i < Localization.Languages.Length; i++)
            {
                if (Localization.Languages[i].Text == null) continue;

                bl_LanguageTexts.TextData[] data = Localization.Languages[i].Text.Data;
                //languages vertical columns
                GUILayout.BeginVertical("box", GUILayout.Width(250 + Localization.ColumnsWidth));
                //language header title
                string df = (Localization.Languages[i].Text == DefaultLang) ? "<color=yellow>Default</color>" : Localization.Languages[i].Text.name;
                GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
                GUILayout.Label("", GUILayout.Width(35));
                GUILayout.FlexibleSpace();
                GUILayout.Label(string.Format("{0} ({1})", Localization.Languages[i].Text.LanguageName, df), EditorStyles.label);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("CSV", GUILayout.Width(35)))
                {
                    ExportTableToCVS(Localization.Languages[i].Text);
                }
                GUILayout.EndHorizontal();

                int startFrom = 0;
                int count = data.Length;
                if(count > showPerPage)
                {
                    startFrom = showPerPage * currentPage;
                    count = showPerPage;

                    if(startFrom + count >= data.Length)
                    {
                        count = data.Length - startFrom;
                    }
                }

                if (SearchKeyworld.Length >= 2)
                {
                    startFrom = 0;
                    count = data.Length;
                }

                for (int e = startFrom; e < startFrom + count; e++)
                {
                    if (SearchKeyworld.Length >= 2)
                    {
                        if (!DefaultLang.Data[e].StringID.Contains(SearchKeyworld)) continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(e.ToString(), EditorStyles.helpBox, GUILayout.Width(30), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    data[e].Text = EditorGUILayout.TextField(data[e].Text);
                    EditorGUILayout.EndHorizontal();

                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(Localization.Languages[i].Text);
                    }
                }
                GUILayout.EndVertical();
            }
        }
        else
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            GUILayout.Label(DefaultLang.LanguageName + "<color=yellow> (Default)</color>", EditorStyles.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            bl_LanguageTexts.TextData[] data = DefaultLang.Data;
            for (int e = 0; e < data.Length; e++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(e.ToString(), EditorStyles.helpBox, GUILayout.Width(22), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                data[e].Text = EditorGUILayout.TextField(data[e].Text);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localization.Languages[CompareTablet].Text.LanguageName, EditorStyles.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            bl_LanguageTexts.TextData[] data2 = Localization.Languages[CompareTablet].Text.Data;
            for (int e = 0; e < data2.Length; e++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(e.ToString(), EditorStyles.helpBox, GUILayout.Width(22), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                data2[e].Text = EditorGUILayout.TextField(data2[e].Text);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(Localization.Languages[CompareTablet].Text);
                EditorUtility.SetDirty(DefaultLang);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHeader()
    {
        GUILayout.BeginHorizontal("box", GUILayout.Height(25));
        GUILayout.BeginHorizontal("box", GUILayout.Width(250));
        if (GUILayout.Button("<size=16><color=yellow>+</color></size>", EditorStyles.toolbarButton, GUILayout.Width(35)))
        {
            AddNewText();
            Localization.AllowEditsStringsID = true;
            Vector2 scr = scrollArea;
            scr.y += 9999;
            scrollArea = scr;
        }
        string c = (Localization.AllowEditsStringsID) ? "red" : "yellow";
        if (GUILayout.Button(string.Format("<size=14><color={0}>✎</color></size>", c), EditorStyles.toolbarButton, GUILayout.Width(35)))
        {
            Localization.AllowEditsStringsID = !Localization.AllowEditsStringsID;
            if (!Localization.AllowEditsStringsID)
            {
                SyncIds();
            }
        }
        int def = Array.FindIndex(Localization.Languages, x => x.Text == DefaultLang);
        GUI.enabled = (CompareTablet != def);
        string t = isComparing ? "SHOW ALL" : "COMPARE";
        if (GUILayout.Button(string.Format(t, c), EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            isComparing = !isComparing;
        }
        GUI.enabled = true;
        List<string> langTables = new List<string>();
        for (int i = 0; i < Localization.Languages.Length; i++)
        {
            string n = Localization.Languages[i].LanguageName;
            if (Localization.Languages[i].Text == DefaultLang) { n += " (Default)"; }
            langTables.Add(n);
        }
        CompareTablet = EditorGUILayout.Popup(CompareTablet, langTables.ToArray(),EditorStyles.toolbarDropDown, GUILayout.Width(110));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        SearchKeyworld = m_SearchField.OnToolbarGUI(SearchKeyworld);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        int allPages = Mathf.FloorToInt(DefaultLang.Data.Length / showPerPage);
        if(DefaultLang.Data.Length % showPerPage != 0)
        {
            allPages++;
        }
        GUI.enabled = currentPage > 0;
        if (GUILayout.Button("<", GUILayout.Width(25)))
        {
            currentPage--;
            scrollArea = Vector2.zero;
        }
        GUI.enabled = true;
        GUILayout.Label($"{currentPage + 1}/{allPages}");
        GUI.enabled = currentPage < allPages - 1;
        if (GUILayout.Button(">", GUILayout.Width(25)))
        {
            currentPage++;
            scrollArea = Vector2.zero;
        }
        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        Localization.ColumnsWidth = EditorGUILayout.Slider(Localization.ColumnsWidth, 0, 150, GUILayout.Width(200));
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DeleteComfirmation()
    {
        Rect r = new Rect(170, Screen.height - 85, 200, 50);
        GUI.Box(r, "", EditorStyles.helpBox);
        GUI.Box(r, "", EditorStyles.helpBox);
        GUI.Box(r, "", EditorStyles.helpBox);

        GUILayout.BeginArea(r);
        GUILayout.Label("Are sure to remove this text?");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("YES"))
        {
            RemoveLast();
            showComfirmDelete = false;
        }
        if (GUILayout.Button("NO"))
        {
            showComfirmDelete = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    /// <summary>
    /// 
    /// </summary>
    void AddNewText()
    {
        for (int i = 0; i < Localization.Languages.Length; i++)
        {
            if (Localization.Languages[i].Text == null) continue;
            bl_LanguageTexts.TextData[] data = new bl_LanguageTexts.TextData[Localization.Languages[i].Text.Data.Length + 1];
            for (int e = 0; e < data.Length - 1; e++)
            {
                data[e] = Localization.Languages[i].Text.Data[e];
            }
            data[data.Length - 1] = new bl_LanguageTexts.TextData();
            Localization.Languages[i].Text.Data = data;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SyncIds()
    {
        for (int i = 0; i < Localization.Languages.Length; i++)
        {
            if (Localization.Languages[i].Text == null) continue;
            if (Localization.Languages[i].Text == DefaultLang) continue;

            bl_LanguageTexts.TextData[] data = Localization.Languages[i].Text.Data;
            for (int e = 0; e < data.Length; e++)
            {
                data[e].StringID = DefaultLang.Data[e].StringID;
            }
            Localization.Languages[i].Text.Data = data;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RemoveLast()
    {
        for (int i = 0; i < Localization.Languages.Length; i++)
        {
            if (Localization.Languages[i].Text == null) continue;
            bl_LanguageTexts.TextData[] data = new bl_LanguageTexts.TextData[Localization.Languages[i].Text.Data.Length - 1];
            for (int e = 0; e < data.Length; e++)
            {
                data[e] = Localization.Languages[i].Text.Data[e];
            }
            Localization.Languages[i].Text.Data = data;
        }
    }

    void ExportTableToCVS(bl_LanguageTexts data)
    {
        if (data == null) return;
        StreamWriter sw = File.CreateText(string.Format("{0}/{1}To{2}.csv", Application.dataPath, DefaultLang.LanguageName, data.LanguageName));
        sw.WriteLine(DefaultLang.LanguageName + "," + data.LanguageName);
        for (int i = 0; i < DefaultLang.Data.Length; i++)
        {
            string t1 = DefaultLang.Data[i].Text;
            string t2 = data.Data[i].Text;
            t1 = t1.Replace(",", ";");
            t2 = t2.Replace(",", ";");
            sw.WriteLine(string.Format("{0},{1}", t1, t2));
        }
        sw.Flush();
        sw.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("MFPS/Addons/Localization/Editor")]
    [MenuItem("MFPS/Tools/Localization Editor")]
    static void OpenWindow()
    {
        GetWindow<bl_LocalizationWindow>();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using System.Linq;

[CustomEditor(typeof(bl_LocalizationUI))]
public class bl_LocalizationUIEditor : Editor
{

    private bl_LocalizationUI script;
    private string[] StringIds;
    private string[] StringTexts;
    private bool addTextOpen = false;
    private string idToAdd = "";
    private string textToAdd = "";
    private string[] StringCases = new string[] { "AS IS", "UPPERCASE", "LOWERCASE", "CAPITAL", };
    private ReorderableList OptionsList;
    public string tempKey = "";
    private bool isFinding = false;
    private Vector2 findScroll = Vector2.zero;
    public List<Locales> locales;
    private bool showLocales = false;

    public bool showMatches = false;
    private bool changeToManually = false;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_LocalizationUI)target;
        StringIds = bl_Localization.Instance.GetIdsList().ToArray();
        StringTexts = bl_Localization.Instance.DefaultLanguage.Text.Data.Select(x =>
        {
            if (x.Text.Length > 64) return x.Text.Substring(0, 61) + "...";
            return x.Text;
        }).ToArray();
        OptionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("StringIDs"), true, true, true, true);
        OptionsList.drawElementCallback += DrawElement();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.BeginVertical("box");
        GUILayout.BeginVertical("box");
        float labelWith = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 75;

        GUILayout.BeginHorizontal();
        script.m_UIType = (bl_LocalizationUI.UIType)EditorGUILayout.EnumPopup("UI Type", script.m_UIType, EditorStyles.toolbarDropDown);
        GUILayout.EndHorizontal();
        if (script.m_UIType == bl_LocalizationUI.UIType.Text)
        {
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (script.ManuallyAssignId)
            {
                GUI.SetNextControlName("lztField");
                script.StringID = EditorGUILayout.TextField("Text ID", script.StringID);
                if (changeToManually)
                {
                    GUI.FocusControl("lztField");
                    changeToManually = false;
                }
                fieldRect = GUILayoutUtility.GetLastRect();
                GUI.color = showMatches ? Color.yellow : Color.white;
                if (GUILayout.Button("M",EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    showMatches = !showMatches;
                }
                GUI.color = Color.white;
                if (GUILayout.Button("List", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    script.ManuallyAssignId = false;
                    showMatches = false;
                }
            }
            else
            {
                if (StringIds.Length > 0)
                {
                    if (!isFinding)
                    {
                        if (script._arrayID != -1)
                        {
                            script._arrayID = EditorGUILayout.Popup("KEY", script._arrayID, StringIds, EditorStyles.toolbarDropDown);
                            script.StringID = StringIds[script._arrayID];
                        }
                        else
                        {
                            script.StringID = EditorGUILayout.TextField("KEY", script.StringID);
                        }
                    }
                    else
                    {
                        tempKey = EditorGUILayout.TextField("Key", tempKey);
                    }
                    GUILayout.Space(2);
                    if (GUILayout.Button("Find", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    {
                        isFinding = !isFinding;
                    }
                    GUI.enabled = !isFinding;
                    if (GUILayout.Button("Manual", EditorStyles.toolbarButton, GUILayout.Width(75)))
                    {
                        showMatches = false;
                        script.ManuallyAssignId = true;
                        showMatches = true;
                        changeToManually = true;
                    }
                    GUI.enabled = true;
                }
            }
            GUILayout.EndHorizontal();
            if (showMatches) GUILayout.Space(125);
        }
        EditorGUIUtility.labelWidth = labelWith;

        if (isFinding && !string.IsNullOrEmpty(tempKey))
        {
            findScroll = GUILayout.BeginScrollView(findScroll, GUILayout.Height(150));
            for (int i = 0; i < StringIds.Length; i++)
            {
                if (!StringIds[i].Contains(tempKey)) continue;

                if (GUILayout.Button(StringIds[i], EditorStyles.label))
                {
                    script._arrayID = i;
                    script.StringID = StringIds[i];
                    isFinding = false;
                    tempKey = string.Empty;
                    if (showLocales) FetchLocalized();
                    else locales = null;
                }
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();
        if (script.m_UIType == bl_LocalizationUI.UIType.Text)
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            script.Plural = EditorGUILayout.ToggleLeft("Plural", script.Plural, EditorStyles.toolbarButton);
            script.StringCase = EditorGUILayout.Popup(script.StringCase, StringCases, EditorStyles.toolbarDropDown);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            script.Extra = EditorGUILayout.ToggleLeft("Extra String", script.Extra, EditorStyles.toolbarButton);
            GUI.enabled = script.Extra;
            script.ExtraString = EditorGUILayout.TextField(script.ExtraString);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("box");

            if (!addTextOpen)
            {
                if (GUILayout.Button("Add New Text", EditorStyles.toolbarButton))
                {
                    addTextOpen = true;
                    if (string.IsNullOrEmpty(textToAdd) && script.GetComponent<Text>() != null)
                    {
                        textToAdd = script.GetComponent<Text>().text;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Cancel", EditorStyles.toolbarButton))
                {
                    addTextOpen = false;
                }
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open Editor", EditorStyles.toolbarButton, GUILayout.Width(110)))
            {
                EditorGUIUtility.PingObject(bl_Localization.Instance);
                Selection.activeObject = bl_Localization.Instance;
            }
            GUILayout.EndHorizontal();

            if (addTextOpen)
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Key", GUILayout.Width(110));
                EditorGUILayout.LabelField("Default Text");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                idToAdd = EditorGUILayout.TextField(idToAdd, GUILayout.Width(110));
                textToAdd = EditorGUILayout.TextField(textToAdd);
                GUILayout.EndHorizontal();
                GUI.enabled = (!string.IsNullOrEmpty(idToAdd) && !string.IsNullOrEmpty(textToAdd));
                if (GUILayout.Button("ADD TEXT", EditorStyles.toolbarButton))
                {
                    if (bl_Localization.Instance.AddText(idToAdd, textToAdd))
                    {
                        addTextOpen = false;
                        StringIds = bl_Localization.Instance.GetIdsList().ToArray();
                        script._arrayID = bl_Localization.Instance.DefaultLanguage.Text.Data.Length - 1;
                        script.StringID = idToAdd;
                        idToAdd = string.Empty;
                        textToAdd = string.Empty;
                    }
                }
                GUI.enabled = true;
                GUILayout.EndVertical();
            }
        }
        else if (script.m_UIType == bl_LocalizationUI.UIType.DropDown)
        {
            OptionsList.DoLayoutList();
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            script.Plural = EditorGUILayout.ToggleLeft("Plural", script.Plural, EditorStyles.toolbarButton);
            script.StringCase = EditorGUILayout.Popup(script.StringCase, StringCases, EditorStyles.toolbarDropDown);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        if (script._arrayID != -1)
        {
            GUILayout.BeginHorizontal();
            showLocales = GUILayout.Toggle(showLocales, "Show Localized Text", "ProjectBrowserTopBarBg", GUILayout.Height(20));
            if (showLocales)
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    FetchLocalized();
                }
            GUILayout.EndHorizontal();
        }
        if (showLocales)
        {
            if (locales == null) FetchLocalized();

            for (int i = 0; i < locales.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(locales[i].Lang, GUILayout.Width(100));
                locales[i].Text = EditorGUILayout.TextArea(locales[i].Text);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Save changes?", "Yes"))
                {
                    SaveLocaleChanges();
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
            EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
        }
        if (showMatches)
        {
            DisplayMatches();
        }
        EditorGUIUtility.labelWidth = labelWith;
    }

    private Rect fieldRect;
    private string lastMatchKey = "";
    private List<string> matchedKeys = new List<string>();
    private List<int> matchedIDs = new List<int>();
    private Vector2 lisScroll = Vector2.zero;
    void DisplayMatches()
    {
        Rect rect = fieldRect;


        if (lastMatchKey != script.StringID)
        {
            matchedKeys = new List<string>();
            matchedIDs = new List<int>();
            for (int i = 0; i < StringIds.Length; i++)
            {
                if (StringIds[i].ToLower().Contains(script.StringID))
                {
                    matchedKeys.Add(StringIds[i]);
                    matchedIDs.Add(i);
                }
            }
            lastMatchKey = script.StringID;
        }

        rect.y += EditorGUIUtility.singleLineHeight + 2;
        if (matchedKeys.Count > 0 && !string.IsNullOrEmpty(script.StringID))
        {
            rect.height = 125;
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.7f));
            lisScroll = GUI.BeginScrollView(rect, lisScroll, new Rect(0, 0, rect.width - 80, EditorGUIUtility.singleLineHeight * matchedKeys.Count), false, true);

            rect.height = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < matchedKeys.Count; i++)
            {
                Rect r = rect;
                r.x = 0; r.y = EditorGUIUtility.singleLineHeight * i;
                r.width = 140;
                GUI.Label(r, matchedKeys[i]);
                r.x += 142;
                r.width = 75;
                if (GUI.Button(r, new GUIContent("Set String",StringTexts[matchedIDs[i]]), EditorStyles.toolbarButton))
                {
                    EditorGUI.FocusTextInControl("");
                    GUI.FocusControl("");
                    script.StringID = matchedKeys[i];
                    showMatches = false;
                    EditorUtility.SetDirty(target);
                }
                r.x += 76;
                
                if (GUI.Button(r, new GUIContent("Set ID", StringTexts[matchedIDs[i]]), EditorStyles.toolbarButton))
                {
                    EditorGUI.FocusTextInControl("");
                    GUI.FocusControl("");
                    script._arrayID = matchedIDs[i];
                    script.StringID = matchedKeys[i];
                    script.ManuallyAssignId = false;
                    showMatches = false;
                    EditorUtility.SetDirty(target);
                }
                rect.y += EditorGUIUtility.singleLineHeight;
            }
            GUI.EndScrollView();
        }
        else
        {
            GUI.Box(rect, "No matches");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void SaveLocaleChanges()
    {
        for (int i = 0; i < locales.Count; i++)
        {
            var data = bl_Localization.Instance.Languages.First(x => x.LanguageName == locales[i].Lang);
            if (data == null || data.Text == null) continue;

            data.Text.Data[script._arrayID].Text = locales[i].Text;
            EditorUtility.SetDirty(data.Text);
        }
        EditorUtility.SetDirty(bl_Localization.Instance);
    }

    /// <summary>
    /// 
    /// </summary>
    private void FetchLocalized()
    {
        locales = new List<Locales>();
        for (int i = 0; i < bl_Localization.Instance.Languages.Length; i++)
        {
            var l = bl_Localization.Instance.Languages[i];
            if (l.Text == null || l.Text.Data == null) continue;

            var local = new Locales();
            local.Lang = l.LanguageName;
            if (script._arrayID <= l.Text.Data.Length - 1)
            {
                local.Text = l.Text.Data[script._arrayID].Text;
            }
            locales.Add(local);
        }
    }

    private ReorderableList.ElementCallbackDelegate DrawElement()
    {
        return (rect, index, isActive, isFocused) =>
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            var property = OptionsList.serializedProperty.GetArrayElementAtIndex(index);
            Rect r = rect;
            r.width = 30;
            GUI.Label(r, "KEY");
            r = rect;
            r.x += 33;
            r.width -= 40;
            property.stringValue = EditorGUI.TextField(r, property.stringValue);
        };
    }

    [System.Serializable]
    public class Locales
    {
        public string Lang;
        public string Text;
    }
}
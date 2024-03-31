using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using MFPSEditor;
using Lovatto.Localization;

[CustomEditor(typeof(bl_Localization))]
public class bl_LocalizationEditor : Editor
{
    private bl_Localization script;
    private ReorderableList list;

    private void OnEnable()
    {
        script = (bl_Localization)target;
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("Languages"), true, true, true,true);
        list.drawElementCallback += DrawElement();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.BeginVertical("box");
        if (GUILayout.Button("Open Editor", EditorStyles.toolbarButton))
        {
            EditorWindow.GetWindow<bl_LocalizationWindow>();
        }
        GUILayout.BeginHorizontal("box");
        script.DefaultLanguage.Text = EditorGUILayout.ObjectField("Default Language", script.DefaultLanguage.Text, typeof(bl_LanguageTexts), false) as bl_LanguageTexts;
        GUILayout.EndHorizontal();
        list.DoLayoutList();
        if (GUILayout.Button("Create New Language Data", EditorStyles.toolbarButton))
        {
            CreateLanguage();
        }
        GUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

    private ReorderableList.ElementCallbackDelegate DrawElement()
    {
        return (rect, index, isActive, isFocused) =>
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            var property = list.serializedProperty.GetArrayElementAtIndex(index);
            if (property != null)
            {
                var lang = property.FindPropertyRelative("Text");
                Rect r = rect;
                r.width = 70;
                if (lang.objectReferenceValue != null)
                {
                    bl_LanguageTexts lt = (bl_LanguageTexts)lang.objectReferenceValue;
                    GUI.Label(r, lt.LanguageName);
                    property.FindPropertyRelative("LanguageName").stringValue = lt.LanguageName;
                }
                r = rect;
                r.x += 85;
                r.width -= 85;
                EditorGUI.PropertyField(r, lang, GUIContent.none);
            }
        };
    }

    void CreateLanguage()
    {
        bl_LanguageTexts asset = ScriptableObject.CreateInstance<bl_LanguageTexts>();

        string path = AssetDatabase.GetAssetPath(bl_Localization.Instance.DefaultLanguage.Text);
        if (path == "")
        {
            path = "Assets";
        }
        else
        {
            path = path.Replace(bl_Localization.Instance.DefaultLanguage.Text.name + ".asset", "");
        }
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "NewLanguage.asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        bl_LanguageTexts p = AssetDatabase.LoadAssetAtPath<bl_LanguageTexts>(path + "NewLanguage.asset");
        bl_LanguageTexts.TextData[] lt = bl_Localization.Instance.DefaultLanguage.Text.Data;
        p.Data = new bl_LanguageTexts.TextData[lt.Length];
        for (int i = 0; i < lt.Length; i++)
        {
            p.Data[i] = new bl_LanguageTexts.TextData();
            p.Data[i].StringID = lt[i].StringID;
            p.Data[i].Text = lt[i].Text;
        }

    }
}
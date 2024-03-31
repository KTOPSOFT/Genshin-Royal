using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPS.Addon.LevelManager;
using MFPSEditor;
using System.Linq;

public class LevelManagerWindowEditor
{
    private static List<UnlockInfo> unlocks;

    private static int editLevel = -1;

    public static void DrawLevels()
    {
        var levels = bl_LevelManager.Instance.Levels;
        if(unlocks == null)
        unlocks = bl_LevelManager.Instance.GetFullUnlockList();

        if(editLevel != -1)
        {
            DrawEditLevel();
            return;
        }

        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            if (i % 3 == 0) EditorGUILayout.BeginHorizontal();

            DrawLevelInfo(level, levels);

            if (i % 3 == 2 || i == levels.Count)
            {
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

        }
    }

    static void DrawLevelInfo(LevelInfo level, List<LevelInfo> allLevels)
    {
        var boxRcet = EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(boxRcet, Color.black);
        GUILayout.Space(2);
        var iconRect = GUILayoutUtility.GetRect(GUIContent.none, "box", GUILayout.Width(50), GUILayout.Height(50));
        EditorGUI.DrawRect(iconRect, bl_MFPSManagerWindow.altColor);
        GUI.DrawTexture(iconRect, level.Icon.texture, ScaleMode.ScaleToFit);
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label($"<b>{level.Name}</b>", TextStyle);
            string scoreNeeded = $"<size=8><b>Score Needed:</b>\n</size>{level.ScoreNeeded}";
            if(level.LevelID > 1)
            {
                int relativeScore = level.ScoreNeeded - allLevels[level.LevelID - 2].ScoreNeeded;
                scoreNeeded += $" <size=8><color={MFPSEditorStyles.LovattoEditorPalette.highlightColor}>(+{relativeScore})</color></size>";
            }
            GUILayout.Label(scoreNeeded, TextStyle);
            GUILayout.Label($"<size=8><b>Unlocks:</b></size>", TextStyle);
            var levelUnlocks = unlocks.Where(x => x.UnlockLevel == level.LevelID).ToArray();
            if (levelUnlocks != null && levelUnlocks.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    for (int i = 0; i < levelUnlocks.Length; i++)
                    {
                        var unlock = levelUnlocks[i];

                        var ir = GUILayoutUtility.GetRect(GUIContent.none, "box", GUILayout.Width(25), GUILayout.Height(25));
                        EditorGUI.DrawRect(ir, bl_MFPSManagerWindow.altColor);
                        GUI.DrawTexture(ir, unlock.Preview.texture,ScaleMode.ScaleToFit);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }else GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(25), GUILayout.Height(25));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        var rect = GUILayoutUtility.GetLastRect();

        var br = new Rect((rect.x + rect.width) - 62, rect.y + 2, 60, 18);
        EditorGUI.DrawRect(br, bl_MFPSManagerWindow.primaryColor);
        if (GUI.Button(br, "EDIT", TextStyle))
        {
            editLevel = level.LevelID - 1;
        }
        GUILayout.Space(10);
    }

    static void DrawEditLevel()
    {
        GUILayout.Space(20);
        var level = bl_LevelManager.Instance.Levels[editLevel];
        EditorGUI.BeginChangeCheck();

        level.Icon = EditorGUILayout.ObjectField("Icon", level.Icon, typeof(Sprite), false) as Sprite;
        level.Name = EditorGUILayout.TextField("Name", level.Name);
        level.ScoreNeeded = EditorGUILayout.IntField("Score Needed", level.ScoreNeeded);

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("DONE"))
        {
            EditorUtility.SetDirty(bl_LevelManager.Instance);
            editLevel = -1;
        }
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(bl_LevelManager.Instance);
        }
    }

    static GUIStyle TextStyle => TutorialWizard.Style.TextStyle;
}
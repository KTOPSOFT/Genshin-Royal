using MFPS.Internal.Structures;
using MFPSEditor;
using UnityEngine;
using MFPS.Addon.Avatars;
using UnityEngine.Serialization;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Addon.Avatars
{
    [CreateAssetMenu(fileName = "New Avatar Data", menuName = "MFPS/Addons/Avatars/Card Data")]
    public class CallingCardData : ScriptableObject
    {
        public string Name;
        [SpritePreview(Height = 30, Width = 100), FormerlySerializedAs("Avatar")] public Texture2D Card;
        public Color OutlineColor = Color.clear;
        public MFPSItemUnlockability Unlockability;

        private Sprite m_cachedCard = null;

        /// <summary>
        /// Get the Avatar ID
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return bl_EmblemsDataBase.GetCallingCardID(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Sprite GetCardAsSprite()
        {
            if (m_cachedCard == null && Card != null) m_cachedCard = Sprite.Create(Card, new Rect(0, 0, Card.width, Card.height), Vector2.zero);

            return m_cachedCard;
        }

        /// <summary>
        /// If this card the current equipped one?
        /// </summary>
        /// <returns></returns>
        public bool IsEquipped()
        {
            int equippedId = bl_EmblemsDataBase.GetUserCallingCard().GetID();
            return GetID() == equippedId;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CallingCardData))]
public class CallingCardDataEditor : Editor
{
    CallingCardData script;
    private bool isListed = false;

    private void OnEnable()
    {
        script = (CallingCardData)target;
        isListed = bl_EmblemsDataBase.Instance.callingCards.Contains(script);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical("box");
        base.OnInspectorGUI();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        var r = GUILayoutUtility.GetRect(300, 118);

        if (script.OutlineColor.a > 0)
        {
            var rr = r;
            rr.width = 302;
            rr.height = 120;
            rr.x -= 1;
            rr.y -= 1;

            EditorGUI.DrawRect(rr, script.OutlineColor);
        }

        if (script.Card != null)
        {
            GUI.DrawTexture(r, script.Card);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (!isListed)
        {
            if (GUILayout.Button("List this Card"))
            {
                bl_EmblemsDataBase.Instance.callingCards.Add(script);
                EditorUtility.SetDirty(bl_EmblemsDataBase.Instance);
                isListed = true;
            }
        }
    }
}
#endif
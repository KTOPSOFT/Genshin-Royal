using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MFPS.Addon.PlayerSelector
{
    /// <summary>
    /// This attribute can only be applied to fields because its
    /// associated PropertyDrawer only operates on fields (either
    /// public or tagged with the [SerializeField] attribute) in
    /// the target MonoBehaviour.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class PlayerSelectorIDAttribute : PropertyAttribute
    {

        public PlayerSelectorIDAttribute()
        {

        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PlayerSelectorIDAttribute))]
    public class PlayerSelectorIDPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            prop.intValue = EditorGUI.Popup(position, prop.displayName, prop.intValue, bl_PlayerSelectorData.Instance.AllPlayerStringList());
        }
    }
#endif
}
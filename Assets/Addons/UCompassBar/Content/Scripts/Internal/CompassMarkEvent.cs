using UnityEngine;
using System;

namespace MFPS.Addon.Compass
{
    public class CompassMarkEvent
    {

        public static Action<CompassMark> CompassMarkAction;
        public static Action<Transform> ChangeCompassCameraAction;
        public static Action<Transform> ActionDestroyMark;
        public static Action<Transform, bool> ActionShowMark;

        public static void SetCompassMark(CompassMark mark)
        {
            if (CompassMarkAction != null) { CompassMarkAction.Invoke(mark); }
        }

        public static void SetCompassCamera(Transform Camera)
        {
            if (ChangeCompassCameraAction != null) { ChangeCompassCameraAction.Invoke(Camera); }
        }

        public static void DestroyMark(Transform mark)
        {
            if (ActionDestroyMark != null) { ActionDestroyMark.Invoke(mark); }
        }

        public static void ShowMark(Transform mark, bool show)
        {
            if (ActionShowMark != null) { ActionShowMark.Invoke(mark, show); }
        }
    }

    public class CompassMark
    {
        public Transform Target;
        public Sprite Icon;
        public Color IconColor;

        public RectTransform MarkUI;
        [HideInInspector] public CanvasGroup Alpha;
    }
}
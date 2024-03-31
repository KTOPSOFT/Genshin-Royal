using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_ULoginUI : MonoBehaviour
    {
        [Header("Windows")]
        public List<MenuWindow> windows;

        [Header("References")]
        [SerializeField] private GameObject authUI;
        [SerializeField] private Animator PanelAnim = null;
        public GameObject[] addonObjects;

        public int CurrentWindow { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Login window {windowName} doesn't exist.");
                return;
            }
            OpenWindow(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveAuthUI(bool active)
        {
            authUI.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(int windowIndex)
        {
            if (windowIndex == CurrentWindow) return;

            StopAllCoroutines();
            StartCoroutine(ChangePanelAnim(() =>
            {
                CurrentWindow = windowIndex;
                windows.ForEach(x => x.SetActive(false));
                var window = windows[CurrentWindow];
                window.SetActive(true);
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public GameObject GetWindowObject(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Login window {windowName} doesn't exist.");
                return null;
            }
            return windows[id].Window;
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator ChangePanelAnim(Action callback)
        {
            PanelAnim.Play("change", 0, 0);
            yield return new WaitForSeconds(PanelAnim.GetCurrentAnimatorStateInfo(0).length / 2);
            callback?.Invoke();
        }

        [Serializable]
        public class MenuWindow
        {
            public string Name;
            public GameObject Window;
            public bl_EventHandler.UEvent onOpen;

            public void SetActive(bool active)
            {
                if (Window != null) Window.SetActive(active);
                if(active) onOpen?.Invoke();
            }
        }

        private static bl_ULoginUI _instance;
        public static bl_ULoginUI Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ULoginUI>(); }
                return _instance;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AdminWindowManager : MonoBehaviour
    {

        [Header("Windows")]
        public string firstWindow = "profile";
        public List<MenuWindow> windows;
        [SerializeField] private Text logText = null;
        [SerializeField] private GameObject LoadingUI = null;
        [SerializeField] private bl_ConfirmationWindow confirmationWindow = null;
        [SerializeField] private bl_ConfirmationWindowWithInput confirmationWindowWithInput = null;

        public int CurrentWindow { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            InitialSetup();
            OpenWindow(firstWindow);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitialSetup()
        {
            foreach (var window in windows)
            {
                if (!window.Active)
                {
                    if (window.OpenButton != null) window.OpenButton.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Pause window {windowName} doesn't exist.");
                return;
            }
            OpenWindow(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(int windowIndex)
        {
            if (windowIndex == CurrentWindow) return;

            CurrentWindow = windowIndex;

            windows.ForEach(x => x.SetActive(false));

            var window = windows[CurrentWindow];
            window.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onConfirm"></param>
        public static void ShowConfirmationWindow(string text, Action onConfirm)
        {
            Instance.confirmationWindow.AskConfirmation(text, onConfirm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onConfirm"></param>
        public static void ShowConfirmationInputWindow(string text, Action<string> onConfirm)
        {
            Instance.confirmationWindowWithInput.AskConfirmation(text, onConfirm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetLog(string text)
        {
            Debug.Log(text);
            logText.text = text;
        }

        public static void Log(string text) => Instance.SetLog(text);

        public static void SetLoading(bool active) => Instance.LoadingUI.SetActive(active);

        [Serializable]
        public class MenuWindow
        {
            public string Name;
            public GameObject Window;
            public Button OpenButton;
            [LovattoToogle] public bool Active = true;
            public bl_EventHandler.UEvent onOpen;

            public void SetActive(bool active)
            {
                if (Window != null) Window.SetActive(active);
                if (OpenButton != null) OpenButton.interactable = !active;
                onOpen?.Invoke();
            }
        }

        private static bl_AdminWindowManager _Instance;
        public static bl_AdminWindowManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_AdminWindowManager>();
                }
                return _Instance;
            }
        }
    }
}
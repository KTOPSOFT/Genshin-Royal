using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AxisUINavigation : MonoBehaviour
    {
        public SelectableType selectableType = SelectableType.Automatic;
        [LovattoToogle] public bool detectInput = true;
        [LovattoToogle] public bool autoSelectFirst = true;
        public string VerticalAxis = "Vertical";
        public string HorizontalAxis = "Horizontal";
        public float axisMovementRate = 0.5f;
        public KeyCode[] submitButtons = new KeyCode[] { KeyCode.JoystickButton0, KeyCode.JoystickButton1 };

        [Header("Order"), Reorderable]
        public List<Selectable> selectables = new List<Selectable>();

        private float lastAxismove = 0;
        public static GameObject blockedObject = null;
        public static bl_AxisUINavigation activeUINavigation = null;
        private bl_AxisUINavigation fallbackUINavigation = null;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if(autoSelectFirst)
            SelectFirstSelectable();

            if (detectInput)
                SetAsActiveUINavigation();

            eventSystem.sendNavigationEvents = false;
            blockedObject = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceEnable()
        {
            SelectFirstSelectable();
            SetAsActiveUINavigation();
            DetectInput = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disable(bool backToFallback)
        {
            DetectInput = false;
            if (backToFallback && fallbackUINavigation != null) fallbackUINavigation.ForceEnable();
        }

        /// <summary>
        /// 
        /// </summary>
        void SetAsActiveUINavigation()
        {
           /* if (activeUINavigation != null && activeUINavigation != this)
            {
                fallbackUINavigation = activeUINavigation;
                fallbackUINavigation.DetectInput = false;
            }*/
            activeUINavigation = this;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// 
        /// </summary>
        void HandleInput()
        {
            if (!detectInput) return;

            ListenKeyboardNavigation();
            ListenSubmitButtons();
            ListenAxisNavigation();
        }

        /// <summary>
        /// 
        /// </summary>
        void ListenKeyboardNavigation()
        {
            if (blockedObject != null) return;
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                // Navigate backward when holding shift, else navigate forward.
                this.HandleHotkeySelect((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 0 : 1, true);
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                eventSystem.SetSelectedGameObject(null, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ListenSubmitButtons()
        {
            var currentSelected = eventSystem.currentSelectedGameObject;
            if (currentSelected == null) return;

            for (int i = 0; i < submitButtons.Length; i++)
            {
                if (Input.GetKeyDown(submitButtons[i]))
                {
                    var sele = currentSelected.GetComponent<Selectable>();
                    if (sele != null)
                    {
                        var bed = new BaseEventData(null);
                        sele.SendMessage("OnSubmit", bed, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ListenAxisNavigation()
        {
            if (blockedObject != null) return;
            if (Time.time - lastAxismove <= axisMovementRate) return;

            //forward
            if (Input.GetAxisRaw(VerticalAxis) > 0.2f && !isALetterKey())
            {
                this.HandleHotkeySelect(0, true);
                lastAxismove = Time.time;
                return;
            }
            else if (Input.GetAxisRaw(VerticalAxis) < -0.2f && !isALetterKey())
            {
                this.HandleHotkeySelect(1, true);
                lastAxismove = Time.time;
                return;
            }
            if (selectableType == SelectableType.Mixed)
            {
                if (Input.GetAxisRaw(HorizontalAxis) > 0.2f && !isALetterKey())
                {
                    this.HandleHotkeySelect(3, true);
                    lastAxismove = Time.time;
                }
                if (Input.GetAxisRaw(HorizontalAxis) < -0.2f && !isALetterKey())
                {
                    this.HandleHotkeySelect(2, true);
                    lastAxismove = Time.time;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleHotkeySelect(int direction, bool isWrapAround)
        {
            GameObject selectedObject = eventSystem.currentSelectedGameObject;
            if (selectedObject == null || !selectedObject.activeInHierarchy) // Ensure a selection exists and is not an inactive object.
            {
                this.SelectFirstSelectable();
                return;
            }
            Selectable currentSelection = selectedObject.GetComponent<Selectable>();
            if (currentSelection == null) { this.SelectFirstSelectable(); return; }

            Selectable nextSelection = null;
            if (selectableType == SelectableType.Explicit)
            {
                nextSelection = this.FindNextSelectable(selectables.IndexOf(currentSelection), direction, isWrapAround, currentSelection);
            }
            else if (selectableType == SelectableType.Automatic)
            {
                switch (direction)
                {
                    case 0:
                        nextSelection = currentSelection.FindSelectableOnDown();
                        break;
                    case 1:
                        nextSelection = currentSelection.FindSelectableOnUp();
                        break;
                    case 2:
                        nextSelection = currentSelection.FindSelectableOnLeft();
                        break;
                    case 3:
                        nextSelection = currentSelection.FindSelectableOnRight();
                        break;
                }
            }
            else if (selectableType == SelectableType.Mixed)
            {
                nextSelection = this.FindNextSelectable(selectables.IndexOf(currentSelection), direction, isWrapAround, currentSelection);
            }
            if (nextSelection != null)
            {
                nextSelection.Select();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectFirstSelectable()
        {
            if (selectables != null && selectables.Count > 0)
            {
                Selectable firstSelectable = selectables.Find(x =>
                {
                    if (x != null && x.gameObject.activeInHierarchy)
                        return true;

                    return false;
                });
                if(firstSelectable != null) firstSelectable.Select();
            }
        }

        /// <summary>
        /// Looks at ordered selectable list to find the selectable we are trying to navigate to and returns it.
        /// </summary>
        private Selectable FindNextSelectable(int currentSelectableIndex, int dir, bool isWrapAround, Selectable currentSelection, bool isFallback = false)
        {
            if (currentSelectableIndex <= -1) return null;
            if (selectableType == SelectableType.Explicit && !isVertical(dir)) return null;

            int totalSelectables = selectables.Count;
            if (totalSelectables <= 1) return null;

            Selectable nextSelection;
            if (dir == 0)//Navigate backwards
            {
                if (currentSelectableIndex == 0)
                {
                    currentSelectableIndex = totalSelectables - 1;
                    nextSelection = (isWrapAround) ? selectables[currentSelectableIndex] : null;

                    if (isDisabledSelectable(nextSelection) && !isFallback)
                    {
                        nextSelection = FindNextSelectable(currentSelectableIndex, dir, isWrapAround, nextSelection, true);
                    }
                }
                else
                {
                    currentSelectableIndex = currentSelectableIndex - 1;
                    nextSelection = selectables[currentSelectableIndex];
                    if (isDisabledSelectable(nextSelection))
                    {
                        nextSelection = FindNextSelectable(currentSelectableIndex, dir, isWrapAround, nextSelection, true);
                    }
                }
            }
            else if (dir == 1) // Navigate forward.
            {
                if (currentSelectableIndex == (totalSelectables - 1))
                {
                    currentSelectableIndex = 0;
                    nextSelection = (isWrapAround) ? selectables[0] : null;
                    if (isDisabledSelectable(nextSelection) && !isFallback)
                    {
                        nextSelection = FindNextSelectable(currentSelectableIndex, dir, isWrapAround, nextSelection, true);
                    }
                }
                else
                {
                    currentSelectableIndex = currentSelectableIndex + 1;
                    nextSelection = selectables[currentSelectableIndex];
                    if (isDisabledSelectable(nextSelection))
                    {
                        nextSelection = FindNextSelectable(currentSelectableIndex, dir, isWrapAround, nextSelection, true);
                    }
                }
            }
            else if (dir == 2)
            {
                nextSelection = currentSelection.FindSelectableOnLeft();
            }
            else
            {
                nextSelection = currentSelection.FindSelectableOnRight();
            }
            return nextSelection;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToogleActivation()
        {
            bool a = DetectInput;
            if (a) Disable(true);
            else ForceEnable();
        }

        private bool isDisabledSelectable(Selectable selectable) => selectable != null && !selectable.gameObject.activeInHierarchy;

        private bool isVertical(int dir) => dir == 0 || dir == 1;

        public bool DetectInput
        {
            get => detectInput;
            set
            {
                detectInput = value;
                if (detectInput) activeUINavigation = this;
            }
        }

        private bool isALetterKey()
        {
            return (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W));
        }

        [ContextMenu("Fetch Selectables")]
        void FetchSelectablesOnChildrens()
        {
            selectables = new List<Selectable>();
            var all = transform.GetComponentsInChildren<Selectable>(true);
            selectables.AddRange(all);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private EventSystem m_eventSystem;
        public EventSystem eventSystem
        {
            get
            {
                if (m_eventSystem == null) m_eventSystem = EventSystem.current;
                return m_eventSystem;
            }
        }

        [System.Serializable]
        public enum SelectableType
        {
            Automatic,
            Explicit,
            Mixed,
        }
    }
}
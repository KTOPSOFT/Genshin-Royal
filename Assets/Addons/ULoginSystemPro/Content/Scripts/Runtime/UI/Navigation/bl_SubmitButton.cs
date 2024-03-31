using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_SubmitButton : MonoBehaviour
    {
        public KeyCode submitKey = KeyCode.Return;

        private Button _button;
        private Button button
        {
            get
            {
                if (_button == null) _button = GetComponent<Button>();
                return _button;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(submitKey))
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
                button.OnSubmit(null);
            }
        }
    }
}
using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_ChangePassword : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField CurrentPassInput;
        [SerializeField] private TMP_InputField NewPassInput;
        [SerializeField] private TMP_InputField ReNewPassInput;

        private bl_LoginPro Login;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            Login = FindObjectOfType<bl_LoginPro>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetPassword()
        {
            string pass = CurrentPassInput.text;
            string newpass = NewPassInput.text;
            string confirmpass = ReNewPassInput.text;

            if (!AllFill(pass, newpass, confirmpass))
                return;
            if (confirmpass != newpass)
            {
                Debug.Log("New Passwords do not match.");
                Login.SetLogText("New Passwords do not match.");
                return;
            }
            if (newpass == pass)
            {
                Login.SetLogText("New password can't be the same as the current.");
                return;
            }
            Login.ChangePassword(pass, newpass);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Open()
        {
            Login.ChangePanel(5);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool AllFill(string p, string n, string r)
        {
            if (string.IsNullOrEmpty(p))
            {
                Debug.Log("Password is empty");
                Login.SetLogText("Password is empty");
                return false;
            }
            if (string.IsNullOrEmpty(n))
            {
                Debug.Log("New Password is empty");
                Login.SetLogText("New Password is empty");
                return false;
            }
            if (string.IsNullOrEmpty(r))
            {
                Debug.Log("Confirm password is empty");
                Login.SetLogText("Confirm password is empty");
                return false;
            }

            return true;
        }
    }
}
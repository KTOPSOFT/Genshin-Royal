using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_ResetPassword : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private TMP_InputField UsernameInput;
        [SerializeField] private TMP_InputField EmailInput;
        [SerializeField] private TMP_InputField KeyInput;
        [SerializeField] private TMP_InputField NewPassInput;
        [SerializeField] private TMP_InputField ReNewPassInput;

        private bl_LoginPro Login;
        private string user;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            Login = bl_LoginPro.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SendEmail()
        {
            string email = EmailInput.text;
            user = UsernameInput.text;
            if (string.IsNullOrEmpty(email))
            {
                Login.SetLogText("Email can't be empty.");
                return;
            }
            if (string.IsNullOrEmpty(user))
            {
                Login.SetLogText("Username can't be empty.");
                return;
            }
            Login.RequestNewPassword(user, email);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangePassword()
        {
            string key = KeyInput.text;
            string pass = NewPassInput.text;
            string confirmpass = ReNewPassInput.text;

            if (!AllFill(pass, confirmpass, key))
                return;
            if (key != Login.GetKey)
            {
                Login.SetLogText("Wrong Reset Key.");
                return;
            }
            if (string.IsNullOrEmpty(user))
            {
                Login.SetLogText("Has logged out, please try again");
                Login.ChangePanel(6);
                return;
            }
            if (confirmpass != pass)
            {
                Debug.Log("New Passwords do not match.");
                Login.SetLogText("New Passwords do not match.");
                return;
            }
            Login.SetLogText("");
            Login.ResetPassword(user, pass);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool AllFill(string n, string r, string k)
        {
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
            if (string.IsNullOrEmpty(k))
            {
                Debug.Log("Key can't be empty");
                Login.SetLogText("Key can't be empty");
                return false;
            }

            return true;
        }
    }
}
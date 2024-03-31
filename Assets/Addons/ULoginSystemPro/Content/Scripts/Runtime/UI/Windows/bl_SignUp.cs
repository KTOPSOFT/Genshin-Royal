using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using TMPro;

public class bl_SignUp : MonoBehaviour
{
    [Header("Settings")]
    public bool RegexSymbols = true;
    [Header("References")]
    public TMP_InputField UserNameInput = null;
    public TMP_InputField NickNameInput = null;
    public TMP_InputField EmailInput = null;
    public TMP_InputField PasswordInput = null;
    public TMP_InputField RePasswordInput = null;
    public TextMeshProUGUI registerText;

    private bl_LoginPro Login;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Login = FindObjectOfType<bl_LoginPro>();
        if (UserNameInput != null)
        {
            UserNameInput.characterLimit = bl_LoginProDataBase.Instance.maxNickNameLenght;
        }
        if (!bl_LoginProDataBase.Instance.RequiredEmailVerification)
        {
            EmailInput.transform.parent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (UserNameInput != null)
        {
            UserNameInput.text = Regex.Replace(UserNameInput.text, @"[^a-zA-Z0-9 ]", "");//not permit symbols
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignUp()
    {
        string user = UserNameInput.text;
        string pass = PasswordInput.text;
        string repass = RePasswordInput.text;
        string email = EmailInput.text;
        string nick = NickNameInput.text;

        if (!AllFill(user,nick, pass, repass, email))
            return;
        if(pass.Length < bl_LoginProDataBase.Instance.MinPasswordLenght)
        {
            Login.SetLogText(string.Format("your password must be at least <b>{0}</b> characters", bl_LoginProDataBase.Instance.MinPasswordLenght));
            return;
        }
        if(repass != pass)
        {
            Login.SetLogText("Passwords doesn't match.");
            return;
        }
        if(user.Length < 3)
        {
            Login.SetLogText("Login name need have at least 3 or more characters length.");
            return;
        }
        if (!IsUsername(user))
        {
            Login.SetLogText("Login Name contain not allowed characters.");
            return;
        }
        if (!IsUsername(nick))
        {
            Login.SetLogText("Nick Name contain not allowed characters.");
            return;
        }
        if (bl_LoginProDataBase.Instance.FilterUserNames)
        {
            if (bl_LoginProDataBase.Instance.FilterName(nick))
            {
                Login.SetLogText("Your nick name is or contain an word that is not allowed.");
                return;
            }
        }
        if(nick == user)
        {
            Login.SetLogText("Nick name can't be same as Login Name.");
            return;
        }
        Login.SinUp(user,nick, pass, email);
    }

    /// <summary>
    /// 
    /// </summary>
    private bool AllFill(string u,string n,string p,string r,string e)
    {
        if (string.IsNullOrEmpty(u))
        {
            Debug.Log("Login name is empty");
            Login.SetLogText("Login name is empty");
            return false;
        }
        if (string.IsNullOrEmpty(n))
        {
            Debug.Log("Nick Name is empty");
            Login.SetLogText("Nick name is empty");
            return false;
        }
        if (string.IsNullOrEmpty(p))
        {
            Debug.Log("Password is empty");
            Login.SetLogText("Password is empty");
            return false;
        }
        if (string.IsNullOrEmpty(r))
        {
            Debug.Log("Re-password is empty");
            Login.SetLogText("Re-password is empty");
            return false;
        }
        if (bl_LoginProDataBase.Instance.RequiredEmailVerification)
        {
            if (string.IsNullOrEmpty(e))
            {
                Debug.Log("Email is empty");
                Login.SetLogText("Email is empty");
                return false;
            }
            if (!IsValidEmailAddress(e))
            {
                Debug.Log("The given email is not a email format.");
                Login.SetLogText("The given email is not a email format.");
                return false;
            }
        }
        
        return true;
    }

    public bool IsUsername(string username)
    {
        string pattern;
        // start with a letter, allow letter or number, length between 6 to 12.
        pattern = @"^[a-zA-Z0-9_ ]+$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(username);
    }

    public static bool IsValidEmailAddress(string emailaddress)
    {
        try
        {
            Regex rx = new Regex(
        @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
            return rx.IsMatch(emailaddress);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
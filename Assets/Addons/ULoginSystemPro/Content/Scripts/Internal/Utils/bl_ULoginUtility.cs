using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class bl_ULoginUtility
{
    /// <summary>
    /// 
    /// </summary>
    public static bool RedirectToLoginIfNeeded()
    {
        if (bl_DataBase.Instance == null && bl_LoginProDataBase.Instance.ForceLoginScene)
        {
            if (Application.CanStreamedLevelBeLoaded("Login"))
            {
                bl_DataBaseUtils.LoadLevel("Login");
                return true;
            }
            else
            {
                Debug.LogWarning("Login scene has not been added in the Build Settings or is disabled.");
            }
        }
        return false;
    }
}
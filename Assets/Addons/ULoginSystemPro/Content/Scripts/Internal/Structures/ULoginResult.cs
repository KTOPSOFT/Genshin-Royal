using UnityEngine;
using UnityEngine.Networking;
using MFPS.ULogin;

public struct ULoginResult
{
    /// <summary>
    /// 
    /// </summary>
    public enum JsonConversion
    {
        Default,
        AddRoot,
        RemoveArray,
    }

    public UnityWebRequest WWW { get; private set; }
    public Status resultState { get; set; }

    public ULoginResult(UnityWebRequest www)
    {
        WWW = www;
        resultState = Status.Unknown;
        Build();
    }

    /// <summary>
    /// 
    /// </summary>
    void Build()
    {
        if (!bl_DataBaseUtils.IsNetworkError(WWW))
        {
            string text = WWW.downloadHandler.text;
            string resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;
            if (bl_LoginProDataBase.Instance.PeerToPeerEncryption && text.StartsWith("encrypt"))
            {
                text = text.Replace("encrypt", "");
                text = AES.Decrypt(text, bl_DataBaseUtils.GetUnitySessionID());
                resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;
            }

            if (resultPrefix.Contains("success"))
            {
                resultState = Status.Success;
            }
            else
            {
                if(HTTPCode == 400)
                {
                    resultState = Status.Fail;
                }else
                resultState = Status.Unknown;
            }
        }
        else
        {
            resultState = Status.Error;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public T FromJson<T>(JsonConversion jsonConversion = JsonConversion.Default, string value = "root")
    {
        if (jsonConversion == JsonConversion.Default)
        {
            return JsonUtility.FromJson<T>(Text);
        }
        else if (jsonConversion == JsonConversion.AddRoot)
        {
            return JsonUtility.FromJson<T>($"{{\"{value}\":{Text}}}");
        }
        else
        {
            string final = Text.Substring(1, Text.Length - 2);
            final = final.Substring(0, Text.Length - 2);
            return JsonUtility.FromJson<T>(final);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isEqual(string txt)
    {
        return Text.Contains(txt);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Print(bool asWarning = false)
    {
        if (!bl_DataBaseUtils.IsNetworkError(WWW))
        {
            if (asWarning) Debug.LogWarning($"({HTTPCode}) {Text}");
            else
            Debug.Log($"({HTTPCode}) {Text}");
        }
        else
        {
            Debug.Log(WWW.error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrintError()
    {
        string error = WWW.error;
        if (!string.IsNullOrEmpty(Text))
        {
            error += $"\nExtra: {Text}";
        }
        Debug.LogError(error);
    }

    public string Text
    {
        get
        {
            string text = WWW.downloadHandler.text;
            string resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;

            if (bl_LoginProDataBase.Instance.PeerToPeerEncryption && text.StartsWith("encrypt"))
            {
                text = text.Replace("encrypt", "");
                text = AES.Decrypt(text, bl_DataBaseUtils.GetUnitySessionID());
                resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;
            }
            if (resultPrefix.Contains("success"))
            {
                text = text.Substring(7, text.Length - 7);
            }

            return text;
        }
    }

    public string RawTextReadable
    {
        get
        {
            string text = WWW.downloadHandler.text;
            if (bl_LoginProDataBase.Instance.PeerToPeerEncryption && text.StartsWith("encrypt"))
            {
                text = text.Replace("encrypt", "");
                text = AES.Decrypt(text, bl_DataBaseUtils.GetUnitySessionID());
            }
            return text;
        }
    }

    public int HTTPCode => (int)WWW.responseCode;

    public string RawText => WWW.downloadHandler.text;

    public bool isError { get { return resultState == Status.Error; } }

    public enum Status
    {
        Success,
        Error,
        Unknown,
        Fail,
    }
}
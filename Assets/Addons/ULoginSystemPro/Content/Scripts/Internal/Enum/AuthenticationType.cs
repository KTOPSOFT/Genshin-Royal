using System;

namespace MFPS.ULogin
{
    [Serializable]
    public enum AuthenticationType
    {
        ULogin = 0,
        Facebook,
        GooglePlay,
        Steam,
        Google,
        Discord,
        EpicGames,
        PlayFab
    }
}
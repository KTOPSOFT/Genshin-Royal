using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin.Facebook
{
    [Serializable]
    public class FBLoginResponse
    {
        public string permissions;
        public string expiration_timestamp;
        public string access_token;
        public string user_id;
        public string last_refresh;
        public string graph_domain;
        public List<string> granted_permissions;
        public List<string> declined_permissions;
        public string callback_id;
    }

    [Serializable]
    public class FBOauthToken
    {
        public string access_token;
        public string token_type;
        public long expires_in;
    }

    [Serializable]
    public class FBUserCredentials
    {
        public string id;
        public string name;
        public string email;
    }
}
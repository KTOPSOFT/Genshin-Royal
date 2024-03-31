namespace MFPS.ULogin
{
    public class DBCommands
    {
        public const int DATABASE_UPDATE_ASSOC_VALUES = 1;
        public const int DATABASE_UPDATE_USER_IP = 2;
        public const int DATABASE_UPDATE_USER_PLAY_TIME = 3;
        public const int DATABASE_CHECK_IF_USER_EXIST = 4;
        public const int DATABASE_UPDATE_USER_COINS = 6;
        public const int DATABASE_SAVE_COINS_PURCHASE = 7;
        public const int DATABASE_UPDATE_VALUE = 8;

        public const int ACCOUNT_CHANGE_PASSWORD = 1;
        public const int ACCOUNT_CHANGE_PASSWORD_VERIFICATION = 2;
        public const int ACCOUNT_CHANGE_PASSWORD_WITHOUT_V = 3;
        public const int ACCOUNT_CHANGE_NICKNAME = 4;
        public const int ACCOUNT_RESENT_VERIFICATION = 5;
        public const int ACCOUNT_DELETE_ACCOUNT = 6;

        public const int BANLIST_EXIST_IP = 2;
        public const int BANLIST_EXIST_IP_OR_NAME = 3;
        public const int BANLIST_EXIST_ACCOUNT = 4;

        public const int ADMIN_BAN = 1;
        public const int ADMIN_UNBAN = 2;
        public const int ADMIN_CHANGE_USER_STATUS = 3;
        public const int ADMIN_GET_USER_STATS = 4;
        public const int ADMIN_UPDATE_VALUES = 5;
        public const int ADMIN_GET_DB_STATS = 6;

        public const int SUPPORT_SUBMIT_TICKET = 1;
        public const int SUPPORT_CHANGE_TICKET = 2;
        public const int SUPPORT_GET_TICKETS = 3;
        public const int SUPPORT_REPLY_TICKET = 4;
        public const int SUPPORT_CLOSE_TICKET = 5;
    }
}
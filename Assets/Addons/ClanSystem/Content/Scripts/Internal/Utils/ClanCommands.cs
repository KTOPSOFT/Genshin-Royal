namespace MFPS.Addon.Clan
{
    public class ClanCommands
    {
        public const int GET_TOP_CLANS = 0;
        public const int GET_CLAN_INFO = 1;
        public const int GET_CLAN_MEMBERS = 2;
        public const int CREATE_CLAN = 3;
        public const int GET_CLAN_BASIC_INFO = 4;
        public const int SEND_INVITATION = 5;
        public const int GET_CLAN_JOIN_REQUESTS = 6;
        public const int ACCEPT_PLAYER_CLAN_JOIN_REQUEST = 7;
        public const int DENY_PLAYER_CLAN_JOIN_REQUEST = 8;
        public const int GET_CLAN_INFO_FROM_NAME = 9;
        public const int ACCEPT_INVITATION_TO_CLAN = 10;
        public const int DENY_INVITATION_TO_CLAN = 11;
        public const int REQUEST_JOIN_TO_CLAN = 12;
        public const int KICK_MEMBER = 13;
        public const int CHANGE_MEMBER_ROLE = 14;
        public const int GET_PLAYER_INVITATIONS = 15;
        public const int EDIT_CLAN_SETTINGS = 16;
        public const int UPDATE_CLAN_SCORE = 17;
        public const int UPDATE_CLAN_MEMBERS = 18;

        public const string CLAN_DESCRIPTION_REGEX = @"^[a-zA-Z0-9().'_#\@!~\[\] ]*$";
    }
}
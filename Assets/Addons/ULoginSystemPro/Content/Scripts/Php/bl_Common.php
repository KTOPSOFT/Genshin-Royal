<?php
/*
ULogin Pro
Version: 2.3.0
Info: ULogin Pro addon for MFPS 2.0
*/

const GAME_VERSION = '1.9.2';
const HOST_NAME = 'HOST_NAME_HERE';
const DATA_BASE_NAME = 'DATABASE_NAME_HERE';
const DATA_BASE_USER = 'DATABASE_USERNAME_HERE';
const DATA_BASE_PASSWORLD = 'DATABASE_PASSWORD_HERE';
const SECRET_KEY = '123456'; //IMPORTANT! has to match with the SecretKey in the game client build.

const PER_TO_PER_ENCRYPTION = false;
const ADMIN_EMAIL = 'email@example.com'; // email address from which the verification emails will be send.
const GAME_NAME = 'Game Name Here';

//don't change this unless you know what you are doing.
define("PLAYERS_DB", "bl_game_users");
define("PURCHASES_DB", "bl_game_purchases");
define("BANS_DB", "bl_game_bans");

include_once("bl_Security.php");

class Connection
{
    
    public static function dbConnect()
    {
        $link = mysqli_connect(HOST_NAME, DATA_BASE_USER, DATA_BASE_PASSWORLD, DATA_BASE_NAME);
        
        if (!$link) {
            die("Couldn´t connect to database server: " . mysqli_connect_error());
        }
        
        return $link;
    }
    
    public static function Query($conn, $query)
    {
        $result = mysqli_query($conn, $query) or die(mysqli_error($conn));
        return $result;
    }
}

class Utils
{
    
    public static function check_session($sid)
    {
        if (PER_TO_PER_ENCRYPTION == false || !isset($sid)) {
            return;
        }
        
        CheckSession($sid);
    }
    
    public static function encrypt_aes($plain, $sid)
    {
        if (!isset($sid)) {
            return $plain;
        }
        return AESencrypt($plain, $sid);
    }
    
    public static function sanitaze_var($value, $conn = null, $sid = null)
    {
        if (!isset($value)) {
            $value = '';
            return $value;
        }
        
        if (PER_TO_PER_ENCRYPTION && isset($sid)) {
            $value = decrypt($value);
        }
        $value = addslashes(trim($value));
        $value = stripslashes($value);
        if (isset($conn)) {
            $value = mysqli_real_escape_string($conn, $value);
        }
        return $value;
    }
    
    public static function get_current_file_url($Protocol = 'http://')
    {
        return $Protocol . $_SERVER['HTTP_HOST'] . substr($_SERVER['REQUEST_URI'], 0, strrpos($_SERVER['REQUEST_URI'], "/")) . "/";
    }
    
    public static function get_secret_hash($parameters)
    {
        return md5($parameters . SECRET_KEY);
    }
    
    public static function get_sqlite_db($dbName, $tableName)
    {
        $db = null;
        if (!file_exists(("databases"))) {
            mkdir("databases", 0755, true);
        }
        $dbPath = "databases/" . $dbName;
        if (!file_exists($dbPath)) {
            $db = new SQLite3($dbPath);
            $db->exec("CREATE TABLE $tableName (id INTEGER PRIMARY KEY, state TEXT, code TEXT)");
        } else {
            $db = new SQLite3($dbPath);
        }
        return $db;
    }
}

function safe($variable, $sid = null)
{
    if (!isset($variable)) {
        $variable = '';
        return $variable;
    }

    if (PER_TO_PER_ENCRYPTION && isset($sid)) {
        $variable = decrypt($variable);
    }
    $variable = addslashes(trim($variable));
    return $variable;
}

function EchoWithPrefix($content, $sid = null)
{
    Response($content, $sid);
}

function success_response()
{
    echo 'success';
}

function fail_response()
{ 
    echo 'fail';
    http_response_code(400);
}

function Response($content, $sid = null)
{
    $result = "success" . $content;
    if (PER_TO_PER_ENCRYPTION && isset($sid)) {
        $result = "encrypt" . AESencrypt($result, $sid);
    }
    echo $result;
}
?>
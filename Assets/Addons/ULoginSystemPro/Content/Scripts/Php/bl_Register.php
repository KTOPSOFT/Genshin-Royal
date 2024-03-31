<?php

if(rateLimiter() == false){ 
    http_response_code(429);
        exit();
    }

include("bl_Common.php");
include_once("bl_Mailer.php");
include_once("bl_Functions.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

if(isset($_POST['sid'])) {$sid                              = Utils::sanitaze_var($_POST['sid'], $link);}
if(isset($_POST['name'])) {$name                            = Utils::sanitaze_var($_POST['name'], $link, $sid);}
if(isset($_POST['nick'])) {$nick                            = Utils::sanitaze_var($_POST['nick'], $link, $sid);}
if(isset($_POST['password'])) {$password                    = Utils::sanitaze_var($_POST['password'], $link, $sid);}
if(isset($_POST['coins'])) {$coins                          = Utils::sanitaze_var($_POST['coins'], $link, $sid);}
if(isset($_POST['email'])) {$email                          = Utils::sanitaze_var($_POST['email'], $link, $sid);}
if(isset($_POST['uIP'])) {$mIP                              = Utils::sanitaze_var($_POST['uIP'], $link, $sid);}
if(isset($_POST['hash'])) {$hash                            = Utils::sanitaze_var($_POST['hash'], $link, $sid);}
if(isset($_POST['multiemail'])) {$multiemail                = Utils::sanitaze_var($_POST['multiemail'], $link, $sid);}
if(isset($_POST['emailVerification'])) {$emailVerification  = Utils::sanitaze_var($_POST['emailVerification'], $link, $sid);}
if(isset($email)) { $email                                  = Utils::sanitaze_var($email, $link);}
if(isset($mIP)) { $mIP                                      = Utils::sanitaze_var($mIP, $link);}

$real_hash = Utils::get_secret_hash($name . $password);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);

if($functions->user_exist_custom('name', $name) != false){
    die("003");
}

if($functions->user_exist_custom('nick', $nick) != false){
    die("011");
}

if (isset($email)) {
    if ($multiemail == "0" && $emailVerification == 0) {
        if($functions->user_exist_custom('email', $email) != false){
            die("005");
        }
    }
} else $email = "";

$password    = password_hash($password, PASSWORD_BCRYPT, array(
    'cost' => 10
));
$random_hash = "";
if ($emailVerification == 0) {
    $random_hash = md5(uniqid(rand()));
}

$sql = "INSERT INTO " . PLAYERS_DB . "(name, nick, password, ip, email, verify, active, coins, purchases) VALUES (?,?,?,?,?,?,?,?,'')";
$stmt = $link->prepare($sql);
$stmt->bind_param("ssssssis", $name, $nick, $password, $mIP, $email, $random_hash, $emailVerification, $coins);
if($stmt->execute() != true){
    die(mysqli_error($link));
}

if ($emailVerification == 0) {
    $burl    = Utils::get_current_file_url();
    $htmlContent = file_get_contents('templates/email-verification.htm');
    $htmlContent = str_replace("#USERNAME#", $name, $htmlContent);
    $htmlContent = str_replace("#BASEURL#", $burl, $htmlContent);
    $htmlContent = str_replace("#HASH#", $random_hash, $htmlContent);
    $htmlContent = str_replace("#GAMENAME#", GAME_NAME, $htmlContent);

    //send verification email
    $subject = "Activation Code for your " . GAME_NAME . " account";
    $from    = ADMIN_EMAIL;
    
    $mailer = new MailCreator();
    if ($mailer->Send(ADMIN_EMAIL, $email, $subject, $htmlContent)) {
        die("success");
    } else {
        die("006"); //email not send
    }
} else {
    die("success");
}

$stmt->close();
mysqli_close($link);

/*
  Simple API request limiter
*/
function rateLimiter(){
    if (session_status() === PHP_SESSION_NONE)
    {
        session_start();
    }

    $rtKey = 'rl'. getIp();
    if (isset($_SESSION[$rtKey]))
    {
        $last = strtotime($_SESSION[$rtKey]);
        $curr = strtotime(date("Y-m-d h:i:s"));
        $sec =  abs($last - $curr);
        if ($sec <= 3) {           
            $_SESSION[$rtKey] = date("Y-m-d h:i:s");
            return false;
        }
    }
    $_SESSION[$rtKey] = date("Y-m-d h:i:s");
    return true;
}

function getIp(){
    $ip = '';
    if (!empty($_SERVER['HTTP_CLIENT_IP'])) {
        $ip = $_SERVER['HTTP_CLIENT_IP'];
    } elseif (!empty($_SERVER['HTTP_X_FORWARDED_FOR'])) {
        $ip = $_SERVER['HTTP_X_FORWARDED_FOR'];
    } else {
        $ip = $_SERVER['REMOTE_ADDR'];
    }
    return $ip;
}
?>
<?php
include_once("bl_Common.php");
include_once("bl_Functions.php");
include_once("bl_Mailer.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

if (isset($_POST['sid'])) {
    $sid = Utils::sanitaze_var($_POST['sid'], $link);
}
if (isset($_POST['id'])) {
    $userId = Utils::sanitaze_var($_POST['id'], $link, $sid);
}
if (isset($_POST['type'])) {
    $type = Utils::sanitaze_var($_POST['type'], $link, $sid);
}
if (isset($_POST['password'])) {
    $password = Utils::sanitaze_var($_POST['password'], $link, $sid);
}
if (isset($_POST['data'])) {
    $data = Utils::sanitaze_var($_POST['data'], $link, $sid);
}
if (isset($_POST['email'])) {
    $email = Utils::sanitaze_var($_POST['email'], $link, $sid);
}
if (isset($_POST['hash'])) {
    $hash = Utils::sanitaze_var($_POST['hash'], $link, $sid);
}

$real_hash = Utils::get_secret_hash($userId);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);

switch ($type) {
    case 1: //Change account password
        if (!$functions->user_exist($userId)) {
            die("008"); //user not found
        }
        
        $curren_pass = $functions->get_user_row($userId, 'password');
        if (password_verify($password, $curren_pass)) {
            $data = password_hash($data, PASSWORD_BCRYPT, array(
                'cost' => 10
            ));
            if ($functions->update_user_row('password', $data, 'id', $userId))
                success_response();
            else
                fail_response();
        } else
            die("002"); //wrong password
        break;
    case 2: //Send reset password email confirmation
        if (!filter_var($email, FILTER_VALIDATE_EMAIL)) // Validate email address
            {
            die("004"); //invalid email
        }
        
        $check = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `name` ='$userId' AND `email` ='$email'") or die(mysqli_error($link));
        if (mysqli_num_rows($check) == 0) {
            die("009"); //user or email not exist or not match
        } else {
            //send verification email          
            $subject = "Reset password request";
             
            $htmlContent = file_get_contents('templates/reset-password-confirmation.htm');
            $htmlContent = str_replace("#USERNAME#", $userId, $htmlContent);
            $htmlContent = str_replace("#CODE#", $data, $htmlContent);

            $mailer = new MailCreator();
            if ($mailer->Send(ADMIN_EMAIL, $email, $subject, $htmlContent)) {
                die("success");
            } else {
                die("006"); //email not send
            }
        }
        break;
    case 3: //Change account password
        $check = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `name` ='$userId' ") or die(mysqli_error($link));
        if (mysqli_num_rows($check) == 0) {
            die("008"); //user not found
        } else {
            $data = password_hash($password, PASSWORD_BCRYPT, array(
                'cost' => 10
            ));
            $update = mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET password='" . $data . "' WHERE name='$userId'") or die(mysqli_error($link));
            echo "success";
        }
        break;
    case 4: //Change account nickname
        $result = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `nick`= '$data'") or die(mysqli_error($link));
        if (mysqli_num_rows($result) == 0) {
            if (mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET nick='" . $data . "' WHERE name='$userId'")) {
                echo "success";
            }
        } else {
            die("008"); // nick name already exist
        }
        break;
    case 5: //Resent verification email
        
        if (!filter_var($email, FILTER_VALIDATE_EMAIL)) // Validate email address
            {
            die("004"); //invalid email
        }
        
        $user = $functions->get_user_by('email', $email);
        if (!$user) {
            http_response_code(204);
            mysqli_close($link);
            exit();
        }

        $burl    = Utils::get_current_file_url();
        $random_hash = $user['verify'];

        $htmlContent = file_get_contents('templates/email-verification.htm');
        $htmlContent = str_replace("#USERNAME#", $name, $htmlContent);
        $htmlContent = str_replace("#BASEURL#", $burl, $htmlContent);
        $htmlContent = str_replace("#HASH#", $random_hash, $htmlContent);
        $htmlContent = str_replace("#GAMENAME#", GAME_NAME, $htmlContent);

        $subject     = "Activation Code for your " . GAME_NAME . " account";
        
        $mailer = new MailCreator();
        if ($mailer->Send(ADMIN_EMAIL, $email, $subject, $htmlContent)) {
            http_response_code(202);
        } else {
            http_response_code(206);
        }
        break;
    case 6: // Delete Account
        
        $userResult = $functions->get_user_row($userId, 'name');

        if($userResult === false || $userResult != $data){
            http_response_code(405);
            exit();
        }

        $sql = "DELETE FROM ". PLAYERS_DB . " WHERE id='$userId'";

        if($functions->Query($sql))
        {
            http_response_code(202);
        }
        break;
}
mysqli_close($link);
?>
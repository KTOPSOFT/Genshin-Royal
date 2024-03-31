<?php
include("bl_Common.php");

$link = Connection::dbConnect();

$code = Utils::sanitaze_var($_GET['code'], $link);

if (!empty($code) && isset($code)) {
    
    $result = mysqli_query($link, "SELECT id FROM " . PLAYERS_DB . " WHERE verify='$code'");
    
    if (mysqli_num_rows($result) > 0) {
        $count = mysqli_query($link, "SELECT id FROM " . PLAYERS_DB . " WHERE verify='$code' and active='0'");
        
        if (mysqli_num_rows($count) == 1) {
            mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET active='1', verify='done' WHERE verify='$code'");
            $msg  = file_get_contents('templates/account-created.htm');    
        } else {
            $msg  = file_get_contents('templates/account-already-created.htm');
        }
        
    } else {
        $msg = "Wrong activation code: " . $code;
    }
    echo $msg;
    exit();
}
?>
<?php
include_once("bl_Common.php");

$link = Connection::dbConnect();

if (isset($_POST['name'])) {
    $name = Utils::sanitaze_var($_POST['name'], $link);
}
if (isset($_POST['ip'])) {
    $ip = Utils::sanitaze_var($_POST['ip'], $link);
}
if (isset($_POST['deviceId'])) {
    $deviceId = Utils::sanitaze_var($_POST['deviceId'], $link);
}
if (isset($_POST['typ'])) {
    $typ = Utils::sanitaze_var($_POST['typ'], $link);
}

if ($typ == 2) { // check if user is banned
    
    if (!isset($deviceId) || $deviceId == '') {
        http_response_code(204);
        die('You are not using the last version of ULogin Pro.');
    }
    
    $result = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE ip='$ip' OR device_id='$deviceId'") or die(mysqli_error($link));
    
    if (mysqli_num_rows($result) != 0) {
        http_response_code(202);
        $row  = mysqli_fetch_array($result, MYSQLI_ASSOC);
        $data = json_encode($row, JSON_UNESCAPED_UNICODE);
        Response($data);
    } else {
        http_response_code(204);
    }
} else if ($typ == 3) {
    
    if (!isset($deviceId) || $deviceId == '') {
        die('You are not using the last version of ULogin Pro.');
    }
    
    $check2 = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE `ip`= '$ip' OR `name`= '$name' OR device_id='$deviceId'") or die(mysqli_error($link));
    $numrows2 = mysqli_num_rows($check2);
    if ($numrows2 != 0) {
        http_response_code(202);
    }
} else if ($typ == 4) {
    
    if (!isset($deviceId) || $deviceId == '') {
        die('You are not using the last version of ULogin Pro.');
    }
    
    $result = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE `ip`= '$ip' OR `name`= '$name' OR device_id='$deviceId'") or die(mysqli_error($link));
    $numrows = mysqli_num_rows($result);
    if ($numrows != 0) {
        http_response_code(202);
        $row  = mysqli_fetch_array($result, MYSQLI_ASSOC);
        $data = json_encode($row, JSON_UNESCAPED_UNICODE);
        Response($data);
    }
}
mysqli_close($link);
?>
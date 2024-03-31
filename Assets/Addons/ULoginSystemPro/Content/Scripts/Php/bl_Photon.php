<?php
header('Access-Control-Allow-Origin: *');
require_once('bl_Common.php');

// Change this with the parameter you set in the Photon Dashboard!
const PHOTON_KEY = 'CUSTOM_KEY_HERE';

$type         = $_GET['type'];
$photonId     = $_GET['photonId'];
$userId       = $_GET['userId'];
$userIp       = $_GET['ip'];
$userDeviceId = $_GET['deviceId'];
$photonKey    = $_GET['photonKey'];

// make sure the request comes from Photon
if ($photonKey != PHOTON_KEY) {
    set_result(3, "Invalid parameters.");
    exit();
}

$link = Connection::dbConnect();

if ($type == 0) // authentication
    {
    // check if the player is banned
    $sql    = "SELECT * FROM `" . BANS_DB . "` WHERE user_id='$userId' OR ip='$userIp' OR device_id='$userDeviceId'";
    $result = mysqli_query($link, $sql);
    
    if (!$result) {
        set_result(3, "Internal Error: " . mysqli_error($link));
        exit();
    }
    
    $result_count = mysqli_num_rows($result);
    if ($result_count != 0) {
        set_result(3, "You are banned.");
        exit();
    }
    
    // any other validation you may want to implement here...
    
    $response               = array();
    $response['ResultCode'] = 1;
    $response['UserId']     = $photonId;
    echo json_encode($response);
    
} else {
    set_result(3, "Bad Request: " . $type);
    exit();
}

function set_result($code, $message)
{
    $response               = array();
    $response['ResultCode'] = $code;
    $response['Message']    = $message;
    
    echo json_encode($response);
}

mysqli_close($link);
?>
<?php
header('Access-Control-Allow-Origin: *');
include("bl_Common.php");
include_once("bl_Functions.php");

Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid    = Utils::sanitaze_var($_POST['sid'], $link);
$name   = Utils::sanitaze_var($_POST['name'], $link, $sid);
$id     = Utils::sanitaze_var($_POST['id'], $link, $sid);
$type   = Utils::sanitaze_var($_POST['type'], $link, $sid);
$hash   = Utils::sanitaze_var($_POST['hash'], $link, $sid);
$coins  = Utils::sanitaze_var($_POST['coins'], $link, $sid);
$coinID  = Utils::sanitaze_var($_POST['coinid'], $link, $sid);
$line   = Utils::sanitaze_var($_POST['line'], $link, $sid);

$real_hash = Utils::get_secret_hash($name);
if ($real_hash == $hash) {

    $functions = new Functions($link);

    if ($type == 0) { //save purchases
        $newCoins = $functions->insert_coins($coins,$coinID,$id,2);
        $sql = "UPDATE " . PLAYERS_DB . " SET purchases=?, coins=? WHERE id=?";
        $stmt = $link->prepare($sql);
        $stmt->bind_param("ssi", $line, $newCoins, $id);
        if($stmt->execute() != true){
            die(mysqli_error($link));
        }

        echo "done";
        $stmt->close();
    }
} else {
    http_response_code(401);
}

mysqli_close($link);
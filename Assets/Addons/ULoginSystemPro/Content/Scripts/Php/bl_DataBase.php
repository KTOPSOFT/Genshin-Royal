<?php
include_once("bl_Common.php");
include_once("bl_Functions.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

if (isset($_POST['sid'])) {
    $sid = Utils::sanitaze_var($_POST['sid'], $link);
}
if (isset($_POST['name'])) {
    $name = Utils::sanitaze_var($_POST['name'], $link, $sid);
}
if (isset($_POST['id'])) {
    $id = Utils::sanitaze_var($_POST['id'], $link, $sid);
}
if (isset($_POST['typ'])) {
    $type = Utils::sanitaze_var($_POST['typ'], $link, $sid);
}
if (isset($_POST['key'])) {
    $key = Utils::sanitaze_var($_POST['key'], $link, $sid);
}
if (isset($_POST['values'])) {
    $values = Utils::sanitaze_var($_POST['values'], $link, $sid);
}
if (isset($_POST['hash'])) {
    $hash = Utils::sanitaze_var($_POST['hash'], $link, $sid);
}
if (isset($_POST['data'])) {
    $data = safe($_POST['data']);
    $data = stripslashes($data);
}

$real_hash = Utils::get_secret_hash($name);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);

switch ($type) {
    case 1: updateUserRows();       break;
    case 2: updateUserIP();         break;
    case 3: updatePlayTime();       break;
    case 4: checkIfUserExist();     break;
    case 6: updateUserCoins();      break;
    case 7: insertCoinPurchase();   break;
    case 8: updateUserRow();        break;
    default:
        http_response_code(400);
        exit();
        break;
}
mysqli_close($link);

/*
* Update user data from given assoc array
*/
function updateUserRows(){

    global $functions;
    global $key;
    global $values;
    global $id;

    if ($functions->update_user_data_safe($key, $values, 'id', $id))
    success_response();
    else
    fail_response();
}

/*
*
*/
function updateUserIP(){
    global $sid, $name, $link, $functions;

    $ip = Utils::sanitaze_var($_POST['nIP'], $link, $sid);
    if ($functions->update_user_row_safe('ip', $ip, 'name', $name)){
        echo "successip";
    }
}

/*
* update player play time
*/
function updatePlayTime(){

    global $values;
    global $id;
    global $link;

    $sql = "UPDATE " . PLAYERS_DB . " SET playtime=playtime+? WHERE id=?";
    if ($stmt = $link->prepare($sql))
    {
        $stmt->bind_param('ii', $values, $id);
        if ($stmt->execute())
        {
            success_response();
        }
        else  die(mysqli_error($link));
    }
    else
    {
        die(mysqli_error($link));
    }
}

/*
*
*/
function checkIfUserExist(){
    global $values, $key, $functions;

    if ($functions->user_exist_custom($key, $values))
            success_response();
        else
            http_response_code(409);
}

/*
* update user coins
*/
function updateUserCoins(){

    global $link, $values, $key, $functions, $id, $sid;
    $param = Utils::sanitaze_var($_POST['param'], $link, $sid);

    $coins             = (int) $values;
    $current_coins_row = $functions->get_user_row($id, 'coins');
    $split_coins       = explode('&', $current_coins_row);
    $current_coin      = (int) ($split_coins[(int) $param]);
    
    if ($key == 1) {
        $current_coin = $current_coin + $coins;
    } else {
        $current_coin = $current_coin - $coins;
    }
    
    $split_coins[(int) $param] = $current_coin;
    $current_coins_row         = implode('&', $split_coins);
    
    $sql = "UPDATE " . PLAYERS_DB . " SET coins='" . $current_coins_row . "' WHERE id='$id'";;
    if ($functions->Query($sql)) {
        Response($current_coin, $sid);
    } else
        fail_response();
}

/*
*
*/
function insertCoinPurchase(){
    global $functions, $data, $sid, $id;

    $purchase_info = json_decode($data, true);
    $coins_data    = $functions->insert_coins($purchase_info["coins"], $purchase_info["coinID"], $id, 1);
    
    $pid     = $purchase_info["productID"];
    $receipt = $purchase_info["receipt"];
    
    $sql = "UPDATE " . PLAYERS_DB . " SET coins='" . $coins_data . "' WHERE id='$id';";
    $sql .= "INSERT INTO " . PURCHASES_DB . " (product_id, receipt, user_id) Values ('{$pid}', '{$receipt}', '{$id}')";
    if ($functions->multiple_query($sql)) {
        $split_coins = explode('&', $coins_data);
        $newCoins    = $split_coins[(int) $purchase_info["coinID"]];
        Response($newCoins, $sid);
    } else {
        fail_response();
    }
}

/*
* Update a single give pair data (value and key from POST)
*/
function updateUserRow(){
    global $functions, $id, $data, $key;
    if ($functions->update_user_row_safe($key, $data, 'id', $id))
    success_response();
else
    fail_response();
}
?>
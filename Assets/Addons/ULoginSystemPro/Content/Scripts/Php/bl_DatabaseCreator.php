<?php
error_reporting(E_ALL ^ E_WARNING);
include_once("bl_Common.php");

if (isset($_POST['dbname'])) {
    $databasename = $_POST['dbname'];
}
$type = strip_tags($_POST['type']);

switch ($type){
    case 3: checkTable();                    break;
    case 5: createClanTable();               break;
    case 6: fetchDiagnostic();               break;
    case 7: executeLocalSql($databasename);  break;
    default: die('request id not defined');  break;
}

function checkTable(){

    $link = Connection::dbConnect();

    $query = "SHOW TABLES LIKE '" . PLAYERS_DB . "'";
    $result = mysqli_query($link, $query) or die(mysqli_error($link));
    $num = mysqli_num_rows($result);
    if ($num >= 1) {
        http_response_code(201);
    } else {
        http_response_code(204);
    }

    mysqli_close($link);
}

function executeLocalSql($fileName){

    $link = Connection::dbConnect();

    if (!file_exists($fileName)) {
        http_response_code(204);
        exit();
    }
    
    $sql = file_get_contents($fileName);
    if (mysqli_multi_query($link, $sql)) {
        http_response_code(202);
        exit();
    } else {
        die(mysqli_error($link));
    }
     mysqli_close($link);
}

function fetchDiagnostic(){
    $data = array();
    
    $data['seclib_exist']    = is_dir('phpseclib');
    $data['mailer_exist']    = is_dir('phpmailer');
    $data['shop_ready']      = file_exists('bl_Shop.php');
    $data['clan_ready']      = file_exists('bl_Clan.php');
    $data['mail_script']     = file_exists('bl_Mailer.php');
    $data['game_version']    = GAME_VERSION;
    $data['p2p']             = PER_TO_PER_ENCRYPTION;
    $data['db_info_changed'] = HOST_NAME != 'HOST_NAME_HERE';
    
    $db = mysqli_connect(HOST_NAME, DATA_BASE_USER, DATA_BASE_PASSWORLD, DATA_BASE_NAME);
    
    if ($db) {
        $data['db_reacheable'] = true;
        $val                   = Connection::Query($db, "DESCRIBE  `" . PLAYERS_DB . "`");
        $data['db_table']      = $val !== false;
    }
    
    $data['phpversion'] = phpversion();
    
    $final         = array();
    $final['data'] = $data;
    
    http_response_code(202);
    echo json_encode($final);
    mysqli_close($db);
}

function createClanTable(){
    $link = Connection::dbConnect();
    $sql = file_get_contents('clan-sql.sql');
    if (mysqli_multi_query( $link, $sql)) {
        echo "done";
    }
    mysqli_close($link);
}
?>
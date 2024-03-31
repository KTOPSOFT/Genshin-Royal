<?php
header('Access-Control-Allow-Origin: *');
include("bl_Common.php");

$top = safe($_POST['top']);

$link = Connection::dbConnect();

$query = "SELECT * FROM " . PLAYERS_DB . " ORDER by `score` DESC LIMIT " . $top;
$result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());

$num_results = mysqli_num_rows($result);

if ($num_results > 0) {
    $jsonData            = array();
    $jsonData["players"] = array();
    
    for ($i = 0; $i < $num_results; $i++) {
        $row                   = mysqli_fetch_array($result);
        $jsonData["players"][] = array(
            "name" => $row['name'],
            "nick" => $row['nick'],
            "kills" => $row['kills'],
            "deaths" => $row['deaths'],
            "score" => $row['score'],
            "status" => $row['status'],
            "clan" => $row['clan']
        );
    }
    echo json_encode($jsonData, JSON_PRETTY_PRINT);
} else {
    http_response_code(204);
}
mysqli_close($link);
?>
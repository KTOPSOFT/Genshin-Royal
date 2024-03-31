<?php
include("bl_Common.php");
$link = Connection::dbConnect();

if (isset($_POST['name'])) {
    $name = Utils::sanitaze_var($_POST['name'], $link);
}
if (isset($_POST['title'])) {
    $title = Utils::sanitaze_var($_POST['title'], $link);
}
if (isset($_POST['content'])) {
    $content = Utils::sanitaze_var($_POST['content'], $link);
}
if (isset($_POST['hash'])) {
    $hash = Utils::sanitaze_var($_POST['hash']);
}
if (isset($_POST['type'])) {
    $typ = Utils::sanitaze_var($_POST['type']);
}
if (isset($_POST['reply'])) {
    $reply = Utils::sanitaze_var($_POST['reply'], $link);
}
if (isset($_POST['id'])) {
    $id = Utils::sanitaze_var($_POST['id']);
}

const TABLE_NAME = "bl_game_tickets";

$real_hash = Utils::get_secret_hash($name);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}
switch ($typ) {
    case 1: // Submit a ticket
        $sql = "INSERT INTO " . TABLE_NAME . "(name, title, content) VALUES ('$name', '$title', '$content')";
        if (Connection::Query($link, $sql)) {
            echo "success";
        }
        break;
    case 2:
        $check = mysqli_query($link, "SELECT * FROM " . TABLE_NAME . " WHERE name ='$name' AND close !='2' ") or die(mysqli_error($link));
        $numrows = mysqli_num_rows($check);
        if ($numrows == 0) {
            echo "none";
        } else {
            while ($row = mysqli_fetch_assoc($check)) {
                $needArray = array(
                    "result" => "reply",
                    "content" => $row['content'],
                    "reply" => $row['reply'],
                    "id" => $row['id']
                );
                $plain     = implode("|", $needArray);
                echo $plain;
            }
        }
        break;
    case 3:
        $query = "SELECT * FROM " . TABLE_NAME . " WHERE close ='0'";
        $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_error($link));
        $num_results = mysqli_num_rows($result);
        if ($num_results > 0) {
            for ($i = 0; $i < $num_results; $i++) {
                $row = mysqli_fetch_array($result);
                
                $needArray = array(
                    "title" => $row['title'],
                    "content" => $row['content'],
                    "reply" => $row['reply'],
                    "id" => $row['id'],
                    "name" => $row['name']
                );
                $plain     = implode("|", $needArray) . "&&";
                
                echo $plain;
            }
        }
        break;
    case 4:
        $check = mysqli_query($link, "UPDATE " . TABLE_NAME . " SET reply='" . $reply . "', close='1' WHERE id='$id'") or die(mysqli_error($link));
        if ($check) {
            echo "success";
        }
        break;
    case 5:
        $check = mysqli_query($link, "DELETE FROM " . TABLE_NAME . "  WHERE id='$id'") or die(mysqli_error($link));
        if ($check) {
            echo "success";
        }
        break;
}

mysqli_close($link);
?>
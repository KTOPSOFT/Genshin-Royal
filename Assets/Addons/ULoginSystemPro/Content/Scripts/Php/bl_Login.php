<?php
include_once ("bl_Common.php");
include_once("bl_Functions.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

if (isset($_POST['sid']))
{
    $sid = Utils::sanitaze_var($_POST['sid'], $link);
}
if (isset($_POST['name']))
{
    $name = Utils::sanitaze_var($_POST['name'], $link, $sid);
}
if (isset($_POST['password']))
{
    $pass = Utils::sanitaze_var($_POST['password'], $link, $sid);
}
if (isset($_POST['appAuth']))
{
    $authApp = Utils::sanitaze_var($_POST['appAuth'], $link, $sid);
}

if (empty($name))
{
    http_response_code(400);
    exit();
}

$functions = new Functions($link);

$sql = "SELECT * FROM " . PLAYERS_DB . " WHERE name=?";
if ($stmt = $link->prepare($sql))
{
    $stmt->bind_param('s', $name);
    $stmt->execute();
    $stmt->store_result();

    if ($stmt->num_rows == 0)
      {
         http_response_code(401);
         exit();
      }
}
else die(mysqli_error($link));

while ($row = $functions->fetchAssocStatement($stmt))
{
    if (password_verify($pass, $row['password']))
    {
        // prevent to manually authenticate for accounts created with
        // custom authenticators
        if ($authApp == "ulogin")
        {
            $rev_pass = strrev($pass);
            if ($rev_pass == $name)
            {
                http_response_code(401);
                die();
            }
        }

        if ($row['active'] == "1" || $authApp != "ulogin")
        {
            PrintData($row, $sid);
        }
        else
        {
            die("007");
        }
    }
    else
    {
        http_response_code(401);
    }
}

$stmt->close();
mysqli_close($link);

/*
 * return account data with custom format separate with |
*/
function PrintData($row, $sid)
{
    $data = "success\n";
    foreach ($row as $key => $value)
    {
        if ($key == "password") //don't retrieve the password
        continue;
        $data .= $key . "|" . $value . "\n";
    }
    if (PER_TO_PER_ENCRYPTION)
    {
        $data = "encrypt" . Utils::encrypt_aes($data, $sid);
    }
    echo $data;
}

/*
 * return account data in json format.
*/
function return_data_json($row, $sid)
{
    $rows = array();
    foreach ($row as $key => $value)
    {

        if ($key == "password") //don't retrieve the password
        continue;

        $rows[$key] = $value;
    }

    $json = json_encode($rows);
    if (PER_TO_PER_ENCRYPTION)
    {
        $data = "encrypt" . Utils::encrypt_aes($json, $sid);
    }
    echo $data;
}
?>
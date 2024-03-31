<?php
require_once('bl_Common.php');
if (isset($_POST['type'])) {$type  = $_POST['type'];}
if (isset($_POST['state'])) {$state = $_POST['state'];}
if (isset($_POST['url'])) {$url   = $_POST['url'];}
if (isset($_POST['dbname'])) {$db_name   = $_POST['dbname'];}

if ($type == 0)
  {
    $ch = curl_init();
    curl_setopt($ch, CURLOPT_USERAGENT, "Mozilla/5.0 (Windows; U; Windows NT 5.1; rv:1.7.3) Gecko/20041001 Firefox/0.10.1");
    curl_setopt($ch, CURLOPT_POST, 1);
    curl_setopt($ch, CURLOPT_URL, $url);
    curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, array(
        'Content-Type: application/x-www-form-urlencoded',
        'Content-Length: 0'
    ));
    curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, $timeout);
    curl_setopt($ch, CURLOPT_TIMEOUT, $timeout);
    curl_setopt($ch, CURLOPT_MAXREDIRS, 10);
    $content = curl_exec($ch);
    echo $content;
  }
else if ($type == 1)
  {    
    $db  = Utils::get_sqlite_db('fbSessionsdb.db', 'fbSessions');
    if(!$db || $db == null)
    {
      die("Unable to access to the database.");
    }
    $res = $db->query("SELECT * FROM fbSessions WHERE state = '$state'");
    if ($res)
      {
        while ($row = $res->fetchArray())
          {
            if(!isset($row['state']))
            {
              $db->close();
              die("not found");
            }
            echo "success|{$row['state']}|{$row['code']}";
          }
        $db->query("DELETE FROM fbSessions WHERE state = '$state'");
      }
    else
      {
        $db->lastErrorMsg();
      }
    $db->close();    
  }
else if ($type == 2)
  {
    $db  = Utils::get_sqlite_db('gSessionsdb.db', 'gSessions');
    $res = $db->query("SELECT * FROM gSessions WHERE state = '$state'");
    if ($res)
      {
        while ($row = $res->fetchArray())
          {
            if(!isset($row['state']))
            {
              $db->close();
              die("not found");
            }
            echo "success|{$row['state']}|{$row['code']}";
          }
        $db->query("DELETE FROM gSessions WHERE state = '$state'");
      }
    else
      {
        $db->lastErrorMsg();
      }
    $db->close();
  }
  else if ($type == 3)
  {
    $db  = Utils::get_sqlite_db($db_name . '.db', $db_name);
    $res = $db->query("SELECT * FROM $db_name WHERE state = '$state'");
    if ($res)
      {
        while ($row = $res->fetchArray())
          {
            if(!isset($row['state']))
            {
              $db->close();
              die("not found");
            }
            echo "success|{$row['state']}|{$row['code']}";
          }
        $db->query("DELETE FROM $db_name WHERE state = '$state'");
      }
    else
      {
        $db->lastErrorMsg();
      }
    $db->close();
  }
  else if ($type == 4)
  {
    echo "<script>poptastic($url);</script>";
  }
?>
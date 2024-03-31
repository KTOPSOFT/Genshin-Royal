<?php
include_once("bl_Common.php");
include_once("phpseclib/Crypt/RSA.php");

use phpseclib\Crypt\RSA;

const KEY_LENGTH = 2048;

function generateKeyPair(){
    if (!isset($_SESSION['rsapublickey'])){
        $rsa = new RSA();
        $rsa->setPrivateKeyFormat(RSA::PRIVATE_FORMAT_XML);
        $rsa->setPublicKeyFormat(RSA::PUBLIC_FORMAT_XML);
        $keys = $rsa->createKey(KEY_LENGTH);
        $_SESSION['rsaprivatekey'] = $keys['privatekey'];
        $_SESSION['rsapublickey'] = $keys['publickey'];
    }
    return $_SESSION['rsapublickey'];
}

function encrypt($clearText){
    $rsa = new RSA();
    $rsa->setEncryptionMode(RSA::ENCRYPTION_PKCS1);
    $rsa->setPublicKeyFormat(RSA::PUBLIC_FORMAT_XML);
    $rsa->loadKey($_SESSION['rsapublickey']);
    $bytesCipherText = $rsa->encrypt($clearText);
    return rawurlencode(base64_encode($bytesCipherText));
}

function decrypt($encrypted){
    $rsa = new RSA();
    $rsa->setEncryptionMode(RSA::ENCRYPTION_PKCS1);
    $rsa->setPrivateKeyFormat(RSA::PRIVATE_FORMAT_XML);
    $rsa->loadKey($_SESSION['rsaprivatekey']);
    $bytesCipherText = base64_decode(rawurldecode($encrypted));
    $clearText = $rsa->decrypt($bytesCipherText);
    return $clearText;
}

const AES_METHOD = 'aes-256-cbc';
function AESdecrypt($encrypted, $sid)
{
    $iv = get_iv();
    $password = substr(hash('sha256', $sid, true), 0, 32);
    $decrypted = openssl_decrypt(base64_decode($encrypted), AES_METHOD , $password, OPENSSL_RAW_DATA, $iv);
    return $decrypted;
}

function AESencrypt($decrypted, $sid)
{
    $iv = get_iv();
    $password = substr(hash('sha256', $sid, true), 0, 32);
    $encrypted = base64_encode(openssl_encrypt($decrypted, AES_METHOD, $password, OPENSSL_RAW_DATA, $iv));
    return $encrypted;
}

function get_iv()
{
    return chr(0x49) . chr(0x76) . chr(0x61) . chr(0x62) . chr(0x20) . chr(0x4d) . chr(0x65) . chr(0x64) . chr(0x76) . chr(0x65) . chr(0x76) . chr(0x6e) . chr(0x65) . chr(0x6e) . chr(0x76) . chr(0x76);
}

function CheckSession($sid)
{
  if(isset($sid))
  {
    session_id($sid);
    session_start();
  }
}

if (isset($_POST['keygen'])) {
    CheckSession($_POST['session_id']);
    EchoWithPrefix(generateKeyPair());
    exit();
}
?>
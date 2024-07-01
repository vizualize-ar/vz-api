REM This script will retrieve the test secrets needed to
REM run the native tests.  It will populate a file called
REM secrets.config.

REM Example: GetTestSecrets.cmd . TestSecrets.config D:\ddb1\Main\bin\x64\Debug\Product\AdminUtilities

setlocal 

if "%1" == "" (set TestSecretsPath=.) else (set TestSecretsPath=%1)
if "%2" == "" (set SecretsFile=) else (set SecretsFile=-secretsFile "%2")
if "%3" == "" (set StashClientPath=) else (set StashClientPath=-stashClientPath "%3")

powershell "%TestSecretsPath%\TestSecrets.ps1" retrieve %StashClientPath% %SecretsFile% -secretStorePath "DPG/DS/DocumentDB/TestSecrets/Product/Backend/native/test"
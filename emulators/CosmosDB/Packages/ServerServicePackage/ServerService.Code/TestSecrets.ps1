# This script is used to retrieve test secrets from secretstore.
# For additional help run: .\TestSecrets.ps1 help
# Author: tvoellm@microsoft.com

<#
.SYNOPSIS
TestSecrets.ps1 <command> [-secretsFile <secrets file>] 
This script is used to store and retrieve test secrets from Azure Key Vault.

.DESCRIPTION

.PARAMETER command
Command is one of retrieve, store, list, remove.
    retrieve = retrieve the test secrets from Azure Key Vault and store them in <secrets file>
    store = unsupported
    list = unsupported
    remove = unsupported

.PARAMETER secretsFile
Path to the secretsFile which is in the format below.  If this command is not given the default
name is TestSecrets.config.
< ? xml version = "1.0" encoding = "utf-8" ? >
<secrets>
<!--Component-->
    <secret name="secret1" value="value1"/>
    <secret name="secret2" value="value2"/>
    ...
</secrets>


.EXAMPLE
.\TestSecrets.ps1 retrieve -secretsFile "TestSecrets.config"
#>


param (
    [ValidateSet("retrieve", "store", "list", "remove")]
    [Parameter(Mandatory=$true)][string]$command,
    [string]$secretsFile = "TestSecrets.config"    
 )


function WriteSecretsFromKeyVault()
{
    Write-Output "Retrieving secrets from KeyVault and writing contents to $secretsFile."

    $loggedOnUser=(Get-Childitem Env:USERNAME).Value;
    if($loggedOnUser -ieq "cloudtest" -or $loggedOnUser -ieq "VssAdministrator") #this check is used to verify if we're in CloudTests's env or not
    {
        #This will log in as the service principal 'spCosmosDbCloudTest' which is attached to the cert mentioned in the login
        Login-AzureRmAccount -ServicePrincipal -CertificateThumbprint e0693354bf61ea14ccab49f480cb4703bd9f19c9 -ApplicationId "9a15a7e4-92c0-401b-a8d0-2de7e8b12829" -TenantId "72f988bf-86f1-41af-91ab-2d7cd011db47"                
    }
    else
    {
        Login-AzureRmAccount -ErrorAction Stop
        #Set-AzureRmContext -Subscription "DocDB Test"        
    }   

    $secretsInVault = Get-AzureKeyVaultSecret -VaultName "cosmosdbtest" -ErrorAction Stop
    Write-Output "There are $($secretsInVault.Count) secrets that have been retrieved."
    
    Write-Output "Retrieving secrets, one by one..."
    $secretData=@{};
    Measure-Command `
    {
        foreach($secret in $secretsInVault)
        {
            $s=Get-AzureKeyVaultSecret -VaultName "cosmosdbtest" -Name $secret.Name
            $secretData.Add($s.Name, $s.SecretValueText);
        }
    }
    Write-Output "Done retrieving secrets, one by one..."

    #Now take all the names of the secrets and put the "." and "_" characters
    #back into them. This is a temporary work around as KeyVault does not allow
    #these kinds of characters in the names of the secrets
    $secretDataModified=@{};
    foreach($sec in $secretData.GetEnumerator())
    {
        $secKeyModified=$sec.Key.Replace("DOT", ".").Replace("UNDERSCORE", "_");        
        $secretDataModified.Add($secKeyModified, $sec.Value);
    }

    Write-Output "Creating and writing to $($secretsFile)..."
    $XmlWriter = New-Object System.XMl.XmlTextWriter($secretsFile, $Null)
    $xmlWriter.Formatting = "Indented"
    $xmlWriter.Indentation = "4"
    $xmlWriter.WriteStartDocument()
    $xmlWriter.WriteStartElement("secrets")
    
    foreach($sec in $secretDataModified.GetEnumerator())
    {
        $xmlWriter.WriteStartElement("secret")
        $xmlWriter.WriteAttributeString("name", $sec.Key)
        $xmlWriter.WriteAttributeString("value", $sec.Value)
        $xmlWriter.WriteEndElement()
    }

    $xmlWriter.WriteEndElement()
    $XmlWriter.Close()
    Write-Output "Done writing out all secrets to $($secretsFile)..."
}

if ($command -eq "retrieve")
{
    
        $retrievedSecretsSuccessfully=$false;

        for($i = 0; $i -lt 5; $i++)
        {
            try
            {
                Write-Output "Attempt #$($i+1) to retrieve test secrets from KeyVault"

                WriteSecretsFromKeyVault;

                $retrievedSecretsSuccessfully=$true;
                break;
            }
            catch [System.UnauthorizedAccessException]
            {
                Write-Output "$($_)"
                break;
            }
            catch [System.Management.Automation.CommandNotFoundException]
            {
                Write-Output "$($_)"
                Write-Output "You do not have latest Azure Powershell installed, please install from https://www.microsoft.com/web/handlers/webpi.ashx/getinstaller/WindowsAzurePowershellGet.3f.3f.3fnew.appids"
                break;
            }
            catch
            {
                Write-Output "Exception while retrieving KeyVault secrets:$($_)"
            }

            Start-Sleep 10;
        }

        if(!($retrievedSecretsSuccessfully))
        {
            Write-Output "Failed to retrieve KeyVault secrets"
            exit 1
        }
        else
        {
            Write-Output "Successfully retrieved KeyVault secrets"
        }
       
}
elseif ($command -eq "store")
{
    throw "Command not supported"
}
elseif ($command -eq "list")
{
    throw "Command not supported"
}
elseif ($command -eq "remove")
{
    throw "Command not supported"
}

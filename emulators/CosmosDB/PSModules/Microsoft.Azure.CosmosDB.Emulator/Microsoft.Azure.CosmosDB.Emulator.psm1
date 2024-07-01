#----------------------------------------------------------
# Copyright (C) Microsoft Corporation. All rights reserved.
#----------------------------------------------------------

# Azure Cosmos DB Emulator management functions

using namespace System.ServiceProcess

Set-Variable ProductName -Option Constant -Value "Azure Cosmos DB Emulator"
Set-Variable DefaultDefaultPartitionCount -Option Constant -Value 25
Set-Variable DefaultCassandraPortNumber -Option Constant 10350
Set-Variable DefaultGremlinPortNumber -Option Constant 8901
Set-Variable DefaultMongoPortNumber -Option Constant 10250
Set-Variable DefaultPortNumber -Option Constant -Value 8081

Set-Variable InstallLocation -Option ReadOnly -Value (
    Get-ItemProperty "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" |
        Where-Object { $_.DisplayName -eq $ProductName } |
        Select-Object -First 1 -Property InstallLocation
).InstallLocation

if ([string]::IsNullOrEmpty($InstallLocation)) {

    # Optimistically assume a copy-install in lieu of an MSI install with this module placed here: $PSScriptRoot\..\..\PSModules\Microsoft.Azure.CosmosDB.Emulator
    # => $InstallLocation = Resolve-Path "$PSScriptRoot\..\.."

    $realPath = if ($null -eq (Get-Item $PSScriptRoot ).LinkType) {
        $PSScriptRoot
    }
    else {
        (Get-Item $PSScriptRoot).Target
    }

    Set-Variable InstallLocation -Force -Option ReadOnly -Value (Resolve-Path "$realPath\..\..")
}

Set-Variable Emulator -Option ReadOnly -Value (Join-Path $InstallLocation "CosmosDB.Emulator.exe")

<#
 .Synopsis
  Gets the self-signed certificate used by the Cosmos DB Emulator.

 .Description
  The Get-CosmosDbEmulatorCertificate cmdlet returns the self-signed SSL certficate used by the Cosmos DB Emulator. This
  certificate is the first certificate from Cert:\LocalMachine\My matching these criteria:

  FriendlyName: DocumentDbEmulatorCertificate
  Subject: CN=localhost
  Issuer: CN=localhost

  .Example
  # $certificate | Export-Certificate -Type CERT -FilePath azure-cosmosdb-emulator.cer
  Gets the Emulator's self-signed certificate and exports it as .cer file.

#>
function Get-CosmosDbEmulatorCertificate {
    [CmdletBinding()]
    param()

    if (-not (Test-Installation)) {
        return
    }
    $certificate = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.FriendlyName -eq "DocumentDbEmulatorCertificate" -and $_.Subject -eq "CN=localhost" -and $_.Issuer -eq "CN=localhost" }
    if ($null -eq $certificate) {
        Write-Error "Cannot find DocumentDbEmulatorCertificate in Cert:\LocalMachine\My"
    }
    $certificate
}

<#
 .Synopsis
  Gets the status of the Cosmos DB Emulator.

 .Description
  The Get-CosmosDbEmulatorStatus cmdlet returns one of these ServiceControllerStatus values: ServiceControllerStatus.StartPending, 
  ServiceControllerStatus.Running, or ServiceControllerStatus.Stopped; otherwise--if an error is encountered--no value is returned.
#>
function Get-CosmosDbEmulatorStatus {
    [CmdletBinding()]
    param()

    if (-not (Test-Installation)) {
        return
    }

    $process = Start-Process $Emulator -ArgumentList "/getstatus" -PassThru -Wait

    switch ($process.ExitCode) {
        1 {
            [ServiceControllerStatus]::StartPending
        }
        2 {
            [ServiceControllerStatus]::Running
        }
        3 {
            [ServiceControllerStatus]::Stopped
        }
        default {
            Write-ErrorUnrecognizedExitCode $process.ExitCode
        }
    }
}

<#
 .Synopsis
  Generates and installs a new self-signed SSL Certificate for the Cosmos DB Emulator

 .Description
 The New-CosmosDbEmulatorCertificate cmdlet generates a new self-signed SSL certificate for the Emulator. The certificate is 
 installed to Cert:\LocalMachine\My and replaces the current SSL certificate used by the Emulator. The certificate is also 
 added to Cert:\LocalMachine\Trust.
 
 The generated certificate has these properties.

    Friendly name: DocumentDbEmulatorCertificate
    Subject: localhost
    Issuer: localhost
  
    Subject Alternative Name:

    * Hostname as returned by [System.Net.Dns]::GetHostEntry((hostname)).HostName.
    * The names provided by the DnsName argument to this function.
    * The IPv4 addresses as returned by:
        [System.Net.Dns]::GetHostEntry((hostname)).AddressList | 
        Where-Object { $_.AddressFamily -eq "InterNetwork" } | 
        ForEach-Object { $_.IpAddressToString }
    * "localhost"
    * "127.0.0.1"

 For compatibility with Windows Server 2012, all IPv4 Addresses are added to the Subject Alternative Name list as both
 DNS names and IP addresses.

 .Example
 # New-CosmosDbEmulatorCertificate cosmosdb-emulator, cosmosdb-emulator.constoso.com
 Generates and installs a self-signed SSL certificate that replaces the one currently used by the Emulator. The new
 certificate includes two additional domain names in the certificates subject alternative name list:

 * cosmosdb-emulator and
 * cosmosdb-emulator.contoso.com

#>
function New-CosmosDbEmulatorCertificate {
    param(
        [Parameter(Position = 1, Mandatory = $false)]
        [string[]]
        $DnsName
    )

    Start-Process $Emulator -ArgumentList "/noui /gencert=`"$(if ($DnsName.Count -gt 0) { $DnsName -join ',' })`"" -Wait

    if ($LASTEXITCODE -eq 0) {
        Get-CosmosDbEmulatorCertificate
    }
    else {
        Write-Error "Certificate generation failed with exit code $LASTEXITCODE"
    }
}

<#
 .Synopsis
  Removes all the files used by the Cosmos DB Emulator for a given data path

 .Description
 The Remove-CosmosDbEmulatorData cmdlet recursively removes all the conten used by the Cosmos DB Emulator from the given
 data path or the $env:LocalAppData\CosmosDbEmulator if the data path is not specified.
 
 .Example
 # Remove-CosmosDbEmulatorData
 It recursively removes all the files in $env:LocalAppData\CosmosDbEmulator directory.
 # Remove-CosmosDbEmulatorData C:\MyDataPath
 It recursively removes all the files in C:\MyDataPath\CosmosDBEmulator directory.

#>
function Remove-CosmosDbEmulatorData {
    param(
        [Parameter(Position = 1, Mandatory = $false)]
        [string]
        $Path
    )

    Start-Process $Emulator -ArgumentList "/noui /resetdatapath$(if ($Path) { '=' + $Path })" -Wait

    if (-not ($LASTEXITCODE == 0)) {
        Write-Error "Emulator data path removal failed with exit code $LASTEXITCODE"
    }
}

<#
 .Synopsis
  Starts the Cosmos DB Emulator on the local computer.

 .Description
  The Start-CosmosDbEmulator cmdlet starts the Cosmos DB Emulator on the local computer. You can use the parameters of
  Start-CosmosDbEmulator to specify options, such as the port, direct port, and mongo port numbers.

 .Parameter AllowNetworkAccess
  Allow access from all IP Addresses assigned to the Emulator's host. You must also specify a value for Key or KeyFile 
  to allow network access.

 .Parameter CassandraPort
  Port number to use for the Cassandra Compatibility API. The default port number is 10350.

 .Parameter ComputePort
  Port to use for the Compute Interop Gateway service. The Gateway's HTTP endpoint probe port is calculated as 
  ComputePort + 79. Hence, ComputePort and ComputePort + 79 must be open and available. The defaults is 8900, 8979.

 .Parameter Consistency
  Sets the default consistency level for the Emulator to Session, Strong, Eventual, or BoundedStaleness. The default
  is Session.

 .Parameter Credential
  Specifies a user account that has permission to perform this action. Type a user name, such as User01 or
  Domain01\User01, or enter a PSCredential object, such as one from the Get-Credential cmdlet. By default,
  the cmdlet uses the credentials of the current user.

 .Parameter DataPath
  Path to store data files. The default location for data files is $env:LocalAppData\CosmosDbEmulator.

 .Parameter DefaultPartitionCount
  The number of partitions to reserve per partitioned collection. The default is 25, which is the same as default value of
  the total partition count.

 .Parameter DirectPort
  A list of 4 ports to use for direct connectivity to the Emulator's backend. The default list is 10251, 10252, 10253, 10254.

 .Parameter EnableCassandra
  Specifies that Cassandra API endpoint is enabled (default is false).

 .Parameter EnableGremlin
  Specifies that Gremlin (Graph) API endpoint is enabled (default is false).

 .Parameter FailOnSslCertificateNameMismatch
  By default the Emulator regenerates its self-signed SSL certificate, if the certificate's SAN does not include the Emulator
  host's domain name, local IPv4 address, 'localhost', and '127.0.0.1'. With this option, the Emulator will fail at startup
  instead. You should then use the New-CosmosDbEmulatorCertificate option to create and install a new self-signed SSL 
  certificate.

 .Parameter GremlinPort
  Port number to use for the Gremlin Compatibility API. The default port number is 8901.

 .Parameter Key
  Authorization key for the Emulator. This value must be the base 64 encoding of a 64 byte vector.

 .Parameter MongoPort
  Port number to use for the Mongo Compatibility API. The default port number is 10250.

 .Parameter NoFirewall
  Specifies that no inbound port rules should be added to the Emulator host's firewall.

 .Parameter NoTelemetry
  Specifies that the cmdlet should not collect telemetry data for the current Emulator session.

 .Parameter NoUI
  Specifies that the cmdlet should not present the Windows taskbar icon user interface.

 .Parameter NoWait
  Specifies that the cmdlet should return as soon as the emulator begins to start. By default the cmdlet waits until startup
  is complete and the Emulator is ready to receive requests.

 .Parameter PartitionCount
  The total number of partitions allocated by the Emulator.

 .Parameter Port
  Port number for the Emulator Gateway Service and Web UI. The default port number is 8081.

 .Parameter Trace
  Indicates whether the Emulator should be configured for traces prior to startup

 .Example
  # Start-CosmosDbEmulator
  Start the Emulator and wait until it is fully started and ready to accept requests.

 .Example
  # Start-CosmosDbEmulator -DefaultPartitionCount 5
  Start the Emulator with 5 partitions reserved for each partitioned collection. The total number of partitions is set
  to the default: 25. Hence, the total number of partitioned collections that can be created is 5 = 25 partitions / 5
  partitions/collection. Each partitioned collection will be capped at 50 GB = 5 partitions * 10 GB / partiton.

 .Example
  # Start-CosmosDbEmulator -Port 443 -MongoPort 27017 -DirectPort 20001,20002,20003,20004
  Starts the Emulator with altermative port numbers.
#>
function Start-CosmosDbEmulator {
    [CmdletBinding(PositionalBinding = $false)]
    param(
        [Parameter(Mandatory = $false)]
        [switch]
        $AllowNetworkAccess,

        [Parameter(Mandatory = $false)]
        [UInt16]
        $CassandraPort = $DefaultCassandraPortNumber,

        [Parameter(Mandatory = $false)]
        [UInt16]
        $ComputePort = $null,

        [Parameter(Mandatory = $false)]
        [ValidateSet('BoundedStaleness', 'Eventual', 'Session', 'Strong')]
        [string]
        $Consistency,

        [Parameter(Mandatory = $false)]
        [ValidateNotNull()]
        [PSCredential]
        $Credential = $null,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [string]
        $DataPath = $null,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, 250)]
        [UInt16]
        $DefaultPartitionCount = $DefaultDefaultPartitionCount,

        [Parameter(Mandatory = $false)]
        [ValidateCount(4, 4)]
        [UInt16[]]
        $DirectPort = $null,

        [Parameter(Mandatory = $false)]
        [switch]
        $EnableCassandra,

        [Parameter(Mandatory = $false)]
        [switch]
        $EnableGremlin,

        [Parameter(Mandatory = $false)]
        [switch]
        $FailOnSslCertificateNameMismatch,

        [Parameter(Mandatory = $false)]
        [UInt16]
        $GremlinPort = $DefaultGremlinPortNumber,

        [Parameter(Mandatory = $false)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Key = $null,

        [Parameter(Mandatory = $false)]
        [UInt16]
        $MongoPort = $DefaultMongoPortNumber,

        [Parameter(Mandatory = $false)]
        [switch]
        $NoFirewall,

        [Parameter(Mandatory = $false)]
        [switch]
        $NoTelemetry,

        [Parameter(Mandatory = $false)]
        [switch]
        $NoUI,

        [Parameter(Mandatory = $false)]
        [switch]
        $NoWait,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, 250)]
        [UInt16]
        $PartitionCount = $DefaultPartitionCount,

        [Parameter(Mandatory = $false)]
        [UInt16]
        $Port = $DefaultPortNumber,

        [Parameter(Mandatory = $false)]
        [switch]
        $SimulateRateLimiting,

        [Parameter(Mandatory = $false)]
        [Uint32]
        $Timeout = 120,

        [Parameter(Mandatory = $false)]
        [switch]
        $Trace
    )

    if (!(Test-Path $Emulator)) {
        Write-Error "The emulator is not installed where expected at '$Emulator'"
        return
    }

    if ($Trace) {
        $process = Start-Process $Emulator -ArgumentList "/starttraces" -PassThru -Wait
        if ($process.ExitCode -ne 0) {
            Write-Error "Attempt to start traces failed with HRESULT 0x$($process.ExitCode.ToString('X8'))"
            return
        }
    }

    $process = Start-Process $Emulator -ArgumentList "/getstatus" -PassThru -Wait

    switch ($process.ExitCode) {
        1 {
            Write-Debug "The emulator is already starting"
            return
        }
        2 {
            Write-Debug "The emulator is already running"
            return
        }
        3 {
            Write-Debug "The emulator is stopped"
        }
        default {
            Write-ErrorUnrecognizedExitCode $process.ExitCode
            return
        }
    }

    $argumentList = , "/noexplorer"

    if ($AllowNetworkAccess) {
        $argumentList += "/allownetworkaccess"
    }

    if (-not [string]::IsNullOrEmpty($ComputePort)) {
        $argumentList += "/computeport=$ComputePort"
    }

    if (-not [string]::IsNullOrEmpty($Consistency)) {
        $argumentList += "/consistency=$Consistency"
    }

    if (-not [string]::IsNullOrWhitespace($DataPath)) {
        $argumentList += "/datapath=`"$DataPath`""
    }

    if ($DefaultPartitionCount -ne $DefaultDefaultPartitionCount) {
        $argumentList += "/defaultpartitioncount=$DefaultPartitionCount"
    }

    if ($null -ne $DirectPort) {
        $argumentList += "/directports=$($DirectPort -Join ',')"
    }

    if ($EnableCassandra) {
        $argumentList += , "/enablecassandraendpoint"
    }

    if ($EnableGremlin) {
        $argumentList += , "/enablegremlinendpoint"
    }

    if ($FailOnSslCertificateNameMismatch) {
        $argumentList += "/failoncertificatenamemismatch"
    }
    
    if ($CassandraPort -ne $DefaultCassandraPortNumber) {
        $argumentList += "/cassandraport=$CassandraPort"
    }

    if ($GremlinPort -ne $DefaultGremlinPortNumber) {
        $argumentList += "/gremlinport=$GremlinPort"
    }

    if (-not [string]::IsNullOrWhiteSpace($Key)) {
        $argumentList += "/key=$Key"
    }

    if ($MongoPort -ne $DefaultMongoPortNumber) {
        $argumentList += "/mongoport=$MongoPort"
    }

    if ($NoFirewall) {
        $argumentList += , "/nofirewall"
    }

    if ($NoTelemetry) {
        $argumentList += , "/notelemetry"
    }

    if ($NoUI) {
        $argumentList += , "/noui"
    }

    if ($PartitionCount -ne $DefaultDefaultPartitionCount) {
        $argumentList += "/partitioncount=$PartitionCount"
    }

    if ($Port -ne $DefaultPortNumber) {
        $argumentList += "/port=$Port"
    }

    $argumentList += if ($SimulateRateLimiting) {
        "/enableratelimiting"
    }
    else {
        "/disableratelimiting"
    }

    Write-Debug "Starting emulator process: $Emulator $argumentList"
    Write-Debug "Credential = $(if ($credential -ne $null) { $credential.UserName } else { "`$null" })"

    $process = if ($Credential -eq $null -or $Credential -eq [PSCredential]::Empty) {
        Start-Process $Emulator -ArgumentList $argumentList -ErrorAction Stop -PassThru
    }
    else {
        Start-Process $Emulator -ArgumentList $argumentList -Credential $Credential -ErrorAction Stop -PassThru
    }

    Write-Debug "Emulator process started: $($process.Name), $($process.FileVersion)"

    if ($NoWait) {
        return;
    }

    [void](Wait-CosmosDbEmulator -Status Running -Timeout $Timeout)
}

<#
.Synopsis
Stops the Cosmos DB Emulator on the local computer.

.Description
The Stop-CosmosDbEmulator cmdlet stops the Cosmos DB Emulator on the local computer. By default the cmdlet waits for the
Emulator to fully stop. Use the NoWait switch to proceed as soon as shutdown begins.

.Parameter NoWait
Specifies that the StopCosmosDbEmulator cmdlet proceed as soon as shutdown begins.

#>
function Stop-CosmosDbEmulator {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [switch]
        $NoWait,

        [Parameter(Mandatory = $false)]
        [UInt32]
        $Timeout = 120
    )

    if (!(Test-Path $Emulator)) {
        Write-Error "The emulator is not installed"
        return
    }

    $process = Start-Process $Emulator -ArgumentList "/getstatus" -PassThru -Wait

    switch ($process.ExitCode) {
        1 {
            Write-Debug "The emulator is starting"
        }
        2 {
            Write-Debug "The emulator is running"
        }
        3 {
            Write-Debug "The emulator is already stopped"
            return
        }
        default {
            Write-ErrorUnrecognizedExitCode $process.ExitCode
            return
        }
    }

    & $Emulator /shutdown

    if ($NoWait) {
        return
    }

    [void](Wait-CosmosDbEmulator -Status Stopped -Timeout $Timeout)
}

<#
.Synopsis
Uninstalls the Cosmos DB Emulator on the local computer.

.Description
The Uninstall-CosmosDbEmulator cmdlet removes the Cosmos DB Emulator on the local computer. By default the cmdlet keeps
all configuration and databases intact. Use the RemoveData switch to delete all data after removing the the Emulator.

.Parameter RemoveData
Specifies that the Uninstall-CosmosDbEmulator cmdlet should delete all data after it removes the Emulator.

#>
function Uninstall-CosmosDbEmulator {
    [CmdletBinding()]
    param(
        [switch]
        $RemoveData
    )

    $installationIds = Get-ItemProperty "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" |
        Where-Object { $_.DisplayName -eq $ProductName } |
        ForEach-Object { $_.PSChildName }

    if ($null -eq $installationIds) {
        Write-Warning "The Cosmos DB Emulator is not installed on $env:COMPUTERNAME"
    }
    else {

        foreach ($installationId in $installationIds) {

            & $Emulator "/shutdown"

            Write-Information "Awaiting shutdown"

            for ($timeout = 30; $timeout -gt 0; $timeout--) {
                Write-Debug $timeout
                $process = Start-Process $Emulator -ArgumentList "/getstatus" -PassThru -Wait
                if ($process.ExitCode -eq 3) {
                    break;
                }
                Start-Sleep -Seconds 1
            }

            Write-Information "Uninstalling the emulator"
            Start-Process MsiExec -ArgumentList "/quiet", "/x${installationId}" -Wait
        }
    }

    if ($RemoveData) {
        $dataPath = Join-Path $env:LOCALAPPDATA CosmosDbEmulator
        Write-Information "Removing data from $dataPath"
        Get-Item -ErrorAction SilentlyContinue $dataPath | Remove-Item -Force -Recurse -ErrorAction Stop
    }
}

<#
 .Synopsis
  Waits for the status of the Cosmos DB Emulator to reach a specified status.

 .Description
  The Wait-CosmosDbEmulatorStatus cmdlet waits for the Emulator to reach one of these statuses: [ServiceControllerStatus]::StartPending,
  [ServiceControllerStatus]::Running, or [ServiceControllerStatus]::Stopped. A timeout value in seconds may be set.

 .Parameter Status
  The status to wait for: ServiceControllerStatus]::StartPending, [ServiceControllerStatus]::Running, [ServiceControllerStatus]::Stopped.

 .Parameter Timeout
  A timeout interval in seconds. The default value of zero specifies an unlimited timeout interval.

#>
function Wait-CosmosDbEmulator {
    [CmdletBinding(PositionalBinding = $false)]
    param(
        [ValidateSet([ServiceControllerStatus]::StartPending, [ServiceControllerStatus]::Running, [ServiceControllerStatus]::Stopped)]
        [Parameter(Position = 2, Mandatory = $true)]
        [ServiceControllerStatus]
        $Status,

        [Parameter()]
        [UInt32]
        $Timeout = 0
    )

    $complete = if ($Timeout -gt 0) {
        $start = [DateTimeOffset]::Now
        $stop = $start.AddSeconds($Timeout)
        {
            $result -eq $Status -or [DateTimeOffset]::Now -ge $stop
        }
    }
    else {
        {
            $result -eq $Status
        }
    }

    do {
        $process = Start-Process $Emulator -ArgumentList "/getstatus" -PassThru -Wait

        switch ($process.ExitCode) {
            1 {
                Write-Debug "The emulator is starting"
                if ($status -eq [ServiceControllerStatus]::StartPending) {
                    return $true
                }
            }
            2 {
                Write-Debug "The emulator is running"
                if ($status -eq [ServiceControllerStatus]::Running) {
                    return $true
                }
            }
            3 {
                Write-Debug "The emulator is stopped"
                if ($status -eq [ServiceControllerStatus]::Stopped) {
                    return $true
                }
            }
            default {
                Write-ErrorUnrecognizedExitCode $process.ExitCode
                return $false
            }
        }
        Start-Sleep -Seconds 1
    }
    until ($complete.Invoke())

    Write-Error "The emulator failed to reach ${Status} status within ${Timeout} seconds"
    $false
}

function Test-Installation {
    [CmdletBinding()]
    param()
    if (Test-Path $Emulator) {
        $true
    }
    else {
        Write-Error "The emulator is not installed where expected at '$Emulator'"
        $false
    }
}

function Write-ErrorUnrecognizedExitCode {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]
        $ExitCode
    )
    Write-Error "The GetStatus operation returned an unrecognized status code: 0x${$ExitCode.ToString("X")}"
}

Export-ModuleMember Get-CosmosDbEmulatorCertificate, Get-CosmosDbEmulatorStatus, New-CosmosDbEmulatorCertificate, Remove-CosmosDbEmulatorData, Start-CosmosDbEmulator, Stop-CosmosDbEmulator, Uninstall-CosmosDbEmulator, Wait-CosmosDbEmulator

# SIG # Begin signature block
# MIIkSAYJKoZIhvcNAQcCoIIkOTCCJDUCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCDZkcM0qRZPmWbj
# f/wkhtZ8eP8TL8O0nOQeucfFvZRW/KCCDYEwggX/MIID56ADAgECAhMzAAABA14l
# HJkfox64AAAAAAEDMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMTgwNzEyMjAwODQ4WhcNMTkwNzI2MjAwODQ4WjB0MQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMR4wHAYDVQQDExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQDRlHY25oarNv5p+UZ8i4hQy5Bwf7BVqSQdfjnnBZ8PrHuXss5zCvvUmyRcFrU5
# 3Rt+M2wR/Dsm85iqXVNrqsPsE7jS789Xf8xly69NLjKxVitONAeJ/mkhvT5E+94S
# nYW/fHaGfXKxdpth5opkTEbOttU6jHeTd2chnLZaBl5HhvU80QnKDT3NsumhUHjR
# hIjiATwi/K+WCMxdmcDt66VamJL1yEBOanOv3uN0etNfRpe84mcod5mswQ4xFo8A
# DwH+S15UD8rEZT8K46NG2/YsAzoZvmgFFpzmfzS/p4eNZTkmyWPU78XdvSX+/Sj0
# NIZ5rCrVXzCRO+QUauuxygQjAgMBAAGjggF+MIIBejAfBgNVHSUEGDAWBgorBgEE
# AYI3TAgBBggrBgEFBQcDAzAdBgNVHQ4EFgQUR77Ay+GmP/1l1jjyA123r3f3QP8w
# UAYDVR0RBEkwR6RFMEMxKTAnBgNVBAsTIE1pY3Jvc29mdCBPcGVyYXRpb25zIFB1
# ZXJ0byBSaWNvMRYwFAYDVQQFEw0yMzAwMTIrNDM3OTY1MB8GA1UdIwQYMBaAFEhu
# ZOVQBdOCqhc3NyK1bajKdQKVMFQGA1UdHwRNMEswSaBHoEWGQ2h0dHA6Ly93d3cu
# bWljcm9zb2Z0LmNvbS9wa2lvcHMvY3JsL01pY0NvZFNpZ1BDQTIwMTFfMjAxMS0w
# Ny0wOC5jcmwwYQYIKwYBBQUHAQEEVTBTMFEGCCsGAQUFBzAChkVodHRwOi8vd3d3
# Lm1pY3Jvc29mdC5jb20vcGtpb3BzL2NlcnRzL01pY0NvZFNpZ1BDQTIwMTFfMjAx
# MS0wNy0wOC5jcnQwDAYDVR0TAQH/BAIwADANBgkqhkiG9w0BAQsFAAOCAgEAn/XJ
# Uw0/DSbsokTYDdGfY5YGSz8eXMUzo6TDbK8fwAG662XsnjMQD6esW9S9kGEX5zHn
# wya0rPUn00iThoj+EjWRZCLRay07qCwVlCnSN5bmNf8MzsgGFhaeJLHiOfluDnjY
# DBu2KWAndjQkm925l3XLATutghIWIoCJFYS7mFAgsBcmhkmvzn1FFUM0ls+BXBgs
# 1JPyZ6vic8g9o838Mh5gHOmwGzD7LLsHLpaEk0UoVFzNlv2g24HYtjDKQ7HzSMCy
# RhxdXnYqWJ/U7vL0+khMtWGLsIxB6aq4nZD0/2pCD7k+6Q7slPyNgLt44yOneFuy
# bR/5WcF9ttE5yXnggxxgCto9sNHtNr9FB+kbNm7lPTsFA6fUpyUSj+Z2oxOzRVpD
# MYLa2ISuubAfdfX2HX1RETcn6LU1hHH3V6qu+olxyZjSnlpkdr6Mw30VapHxFPTy
# 2TUxuNty+rR1yIibar+YRcdmstf/zpKQdeTr5obSyBvbJ8BblW9Jb1hdaSreU0v4
# 6Mp79mwV+QMZDxGFqk+av6pX3WDG9XEg9FGomsrp0es0Rz11+iLsVT9qGTlrEOla
# P470I3gwsvKmOMs1jaqYWSRAuDpnpAdfoP7YO0kT+wzh7Qttg1DO8H8+4NkI6Iwh
# SkHC3uuOW+4Dwx1ubuZUNWZncnwa6lL2IsRyP64wggd6MIIFYqADAgECAgphDpDS
# AAAAAAADMA0GCSqGSIb3DQEBCwUAMIGIMQswCQYDVQQGEwJVUzETMBEGA1UECBMK
# V2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
# IENvcnBvcmF0aW9uMTIwMAYDVQQDEylNaWNyb3NvZnQgUm9vdCBDZXJ0aWZpY2F0
# ZSBBdXRob3JpdHkgMjAxMTAeFw0xMTA3MDgyMDU5MDlaFw0yNjA3MDgyMTA5MDla
# MH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdS
# ZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMT
# H01pY3Jvc29mdCBDb2RlIFNpZ25pbmcgUENBIDIwMTEwggIiMA0GCSqGSIb3DQEB
# AQUAA4ICDwAwggIKAoICAQCr8PpyEBwurdhuqoIQTTS68rZYIZ9CGypr6VpQqrgG
# OBoESbp/wwwe3TdrxhLYC/A4wpkGsMg51QEUMULTiQ15ZId+lGAkbK+eSZzpaF7S
# 35tTsgosw6/ZqSuuegmv15ZZymAaBelmdugyUiYSL+erCFDPs0S3XdjELgN1q2jz
# y23zOlyhFvRGuuA4ZKxuZDV4pqBjDy3TQJP4494HDdVceaVJKecNvqATd76UPe/7
# 4ytaEB9NViiienLgEjq3SV7Y7e1DkYPZe7J7hhvZPrGMXeiJT4Qa8qEvWeSQOy2u
# M1jFtz7+MtOzAz2xsq+SOH7SnYAs9U5WkSE1JcM5bmR/U7qcD60ZI4TL9LoDho33
# X/DQUr+MlIe8wCF0JV8YKLbMJyg4JZg5SjbPfLGSrhwjp6lm7GEfauEoSZ1fiOIl
# XdMhSz5SxLVXPyQD8NF6Wy/VI+NwXQ9RRnez+ADhvKwCgl/bwBWzvRvUVUvnOaEP
# 6SNJvBi4RHxF5MHDcnrgcuck379GmcXvwhxX24ON7E1JMKerjt/sW5+v/N2wZuLB
# l4F77dbtS+dJKacTKKanfWeA5opieF+yL4TXV5xcv3coKPHtbcMojyyPQDdPweGF
# RInECUzF1KVDL3SV9274eCBYLBNdYJWaPk8zhNqwiBfenk70lrC8RqBsmNLg1oiM
# CwIDAQABo4IB7TCCAekwEAYJKwYBBAGCNxUBBAMCAQAwHQYDVR0OBBYEFEhuZOVQ
# BdOCqhc3NyK1bajKdQKVMBkGCSsGAQQBgjcUAgQMHgoAUwB1AGIAQwBBMAsGA1Ud
# DwQEAwIBhjAPBgNVHRMBAf8EBTADAQH/MB8GA1UdIwQYMBaAFHItOgIxkEO5FAVO
# 4eqnxzHRI4k0MFoGA1UdHwRTMFEwT6BNoEuGSWh0dHA6Ly9jcmwubWljcm9zb2Z0
# LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1Jvb0NlckF1dDIwMTFfMjAxMV8wM18y
# Mi5jcmwwXgYIKwYBBQUHAQEEUjBQME4GCCsGAQUFBzAChkJodHRwOi8vd3d3Lm1p
# Y3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY1Jvb0NlckF1dDIwMTFfMjAxMV8wM18y
# Mi5jcnQwgZ8GA1UdIASBlzCBlDCBkQYJKwYBBAGCNy4DMIGDMD8GCCsGAQUFBwIB
# FjNodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpb3BzL2RvY3MvcHJpbWFyeWNw
# cy5odG0wQAYIKwYBBQUHAgIwNB4yIB0ATABlAGcAYQBsAF8AcABvAGwAaQBjAHkA
# XwBzAHQAYQB0AGUAbQBlAG4AdAAuIB0wDQYJKoZIhvcNAQELBQADggIBAGfyhqWY
# 4FR5Gi7T2HRnIpsLlhHhY5KZQpZ90nkMkMFlXy4sPvjDctFtg/6+P+gKyju/R6mj
# 82nbY78iNaWXXWWEkH2LRlBV2AySfNIaSxzzPEKLUtCw/WvjPgcuKZvmPRul1LUd
# d5Q54ulkyUQ9eHoj8xN9ppB0g430yyYCRirCihC7pKkFDJvtaPpoLpWgKj8qa1hJ
# Yx8JaW5amJbkg/TAj/NGK978O9C9Ne9uJa7lryft0N3zDq+ZKJeYTQ49C/IIidYf
# wzIY4vDFLc5bnrRJOQrGCsLGra7lstnbFYhRRVg4MnEnGn+x9Cf43iw6IGmYslmJ
# aG5vp7d0w0AFBqYBKig+gj8TTWYLwLNN9eGPfxxvFX1Fp3blQCplo8NdUmKGwx1j
# NpeG39rz+PIWoZon4c2ll9DuXWNB41sHnIc+BncG0QaxdR8UvmFhtfDcxhsEvt9B
# xw4o7t5lL+yX9qFcltgA1qFGvVnzl6UJS0gQmYAf0AApxbGbpT9Fdx41xtKiop96
# eiL6SJUfq/tHI4D1nvi/a7dLl+LrdXga7Oo3mXkYS//WsyNodeav+vyL6wuA6mk7
# r/ww7QRMjt/fdW1jkT3RnVZOT7+AVyKheBEyIXrvQQqxP/uozKRdwaGIm1dxVk5I
# RcBCyZt2WwqASGv9eZ/BvW1taslScxMNelDNMYIWHTCCFhkCAQEwgZUwfjELMAkG
# A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQx
# HjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UEAxMfTWljcm9z
# b2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMQITMwAAAQNeJRyZH6MeuAAAAAABAzAN
# BglghkgBZQMEAgEFAKCBrjAZBgkqhkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgor
# BgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQxIgQgTKfKov+M
# RDYGVxYgI/DWPRQ5Qcc0g+i0Tu9MfC3ZqrcwQgYKKwYBBAGCNwIBDDE0MDKgFIAS
# AE0AaQBjAHIAbwBzAG8AZgB0oRqAGGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbTAN
# BgkqhkiG9w0BAQEFAASCAQBy37ih8dtDhFj/XKIfs6jdqbDr8bkt1zNW6cVUDluA
# nIyWVIPHJZe2NqRb4J0urU628gjSkVrvsv9Y+LMnRWWo4krhEsFGW4uEHx9uxEkh
# gsDmBX5zWV4bhbqrJAaffrXHYYk6rM4guG2XHdsQZFWG8gTqMVF5ISOCORtaSKk0
# oc7DSVsx2V3IB0YF5HmxDEWzIKjGqshBrV+MePhnhQQuYajh6L1n+9diwJ4mz4jU
# BWiedKB/JkeUtwz5eWIOwFz/MPaTnu4QRawgAa+7jS2HtX6TdIEytykwc48k/AyB
# rvFVulqgQbWLplo3x9SYcazTrdI2TCCP+cMglVZ3/TVIoYITpzCCE6MGCisGAQQB
# gjcDAwExghOTMIITjwYJKoZIhvcNAQcCoIITgDCCE3wCAQMxDzANBglghkgBZQME
# AgEFADCCAVQGCyqGSIb3DQEJEAEEoIIBQwSCAT8wggE7AgEBBgorBgEEAYRZCgMB
# MDEwDQYJYIZIAWUDBAIBBQAEINQ5vv/eQy+MwGiAsYbGRPNfbKVSMDwTZiAXZmdi
# DIHRAgZb/HAVA/cYEzIwMTgxMjA0MjM1NzQ1LjI1MVowBwIBAYACAfSggdCkgc0w
# gcoxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdS
# ZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJTAjBgNVBAsT
# HE1pY3Jvc29mdCBBbWVyaWNhIE9wZXJhdGlvbnMxJjAkBgNVBAsTHVRoYWxlcyBU
# U1MgRVNOOjEyRTctMzA2NC02MTEyMSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1T
# dGFtcCBTZXJ2aWNloIIPEzCCBnEwggRZoAMCAQICCmEJgSoAAAAAAAIwDQYJKoZI
# hvcNAQELBQAwgYgxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
# DgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24x
# MjAwBgNVBAMTKU1pY3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAy
# MDEwMB4XDTEwMDcwMTIxMzY1NVoXDTI1MDcwMTIxNDY1NVowfDELMAkGA1UEBhMC
# VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
# BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRp
# bWUtU3RhbXAgUENBIDIwMTAwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQCpHQ28dxGKOiDs/BOX9fp/aZRrdFQQ1aUKAIKF++18aEssX8XD5WHCdrc+Zitb
# 8BVTJwQxH0EbGpUdzgkTjnxhMFmxMEQP8WCIhFRDDNdNuDgIs0Ldk6zWczBXJoKj
# RQ3Q6vVHgc2/JGAyWGBG8lhHhjKEHnRhZ5FfgVSxz5NMksHEpl3RYRNuKMYa+YaA
# u99h/EbBJx0kZxJyGiGKr0tkiVBisV39dx898Fd1rL2KQk1AUdEPnAY+Z3/1ZsAD
# lkR+79BL/W7lmsqxqPJ6Kgox8NpOBpG2iAg16HgcsOmZzTznL0S6p/TcZL2kAcEg
# CZN4zfy8wMlEXV4WnAEFTyJNAgMBAAGjggHmMIIB4jAQBgkrBgEEAYI3FQEEAwIB
# ADAdBgNVHQ4EFgQU1WM6XIoxkPNDe3xGG8UzaFqFbVUwGQYJKwYBBAGCNxQCBAwe
# CgBTAHUAYgBDAEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0j
# BBgwFoAU1fZWy4/oolxiaNE9lJBb186aGMQwVgYDVR0fBE8wTTBLoEmgR4ZFaHR0
# cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9vQ2Vy
# QXV0XzIwMTAtMDYtMjMuY3JsMFoGCCsGAQUFBwEBBE4wTDBKBggrBgEFBQcwAoY+
# aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNSb29DZXJBdXRf
# MjAxMC0wNi0yMy5jcnQwgaAGA1UdIAEB/wSBlTCBkjCBjwYJKwYBBAGCNy4DMIGB
# MD0GCCsGAQUFBwIBFjFodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vUEtJL2RvY3Mv
# Q1BTL2RlZmF1bHQuaHRtMEAGCCsGAQUFBwICMDQeMiAdAEwAZQBnAGEAbABfAFAA
# bwBsAGkAYwB5AF8AUwB0AGEAdABlAG0AZQBuAHQALiAdMA0GCSqGSIb3DQEBCwUA
# A4ICAQAH5ohRDeLG4Jg/gXEDPZ2joSFvs+umzPUxvs8F4qn++ldtGTCzwsVmyWrf
# 9efweL3HqJ4l4/m87WtUVwgrUYJEEvu5U4zM9GASinbMQEBBm9xcF/9c+V4XNZgk
# Vkt070IQyK+/f8Z/8jd9Wj8c8pl5SpFSAK84Dxf1L3mBZdmptWvkx872ynoAb0sw
# RCQiPM/tA6WWj1kpvLb9BOFwnzJKJ/1Vry/+tuWOM7tiX5rbV0Dp8c6ZZpCM/2pi
# f93FSguRJuI57BlKcWOdeyFtw5yjojz6f32WapB4pm3S4Zz5Hfw42JT0xqUKloak
# vZ4argRCg7i1gJsiOCC1JeVk7Pf0v35jWSUPei45V3aicaoGig+JFrphpxHLmtgO
# R5qAxdDNp9DvfYPw4TtxCd9ddJgiCGHasFAeb73x4QDf5zEHpJM692VHeOj4qEir
# 995yfmFrb3epgcunCaw5u+zGy9iCtHLNHfS4hQEegPsbiSpUObJb2sgNVZl6h3M7
# COaYLeqN4DMuEin1wC9UJyH3yKxO2ii4sanblrKnQqLJzxlBTeCG+SqaoxFmMNO7
# dDJL32N79ZmKLxvHIa9Zta7cRDyXUHHXodLFVeNp3lfB0d4wwP3M5k37Db9dT+md
# Hhk4L7zPWAUu7w2gUDXa7wknHNWzfjUeCLraNtvTX4/edIhJEjCCBPEwggPZoAMC
# AQICEzMAAADq4c7/mrOmktEAAAAAAOowDQYJKoZIhvcNAQELBQAwfDELMAkGA1UE
# BhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAc
# BgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0
# IFRpbWUtU3RhbXAgUENBIDIwMTAwHhcNMTgwODIzMjAyNzE3WhcNMTkxMTIzMjAy
# NzE3WjCByjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNV
# BAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjElMCMG
# A1UECxMcTWljcm9zb2Z0IEFtZXJpY2EgT3BlcmF0aW9uczEmMCQGA1UECxMdVGhh
# bGVzIFRTUyBFU046MTJFNy0zMDY0LTYxMTIxJTAjBgNVBAMTHE1pY3Jvc29mdCBU
# aW1lLVN0YW1wIFNlcnZpY2UwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQDBf34+HliyJ6ZawA0LSkd2y6AD+hKWv8++nrPz65ylJuchUwkGSG+VTtdnee5y
# gGffOsyeer84cyVC9WmBoSOgT5M/4Yxfm0kA6wue82uO0BHHTBT8F7evnKrC8sD7
# EqB2eMUdmYhJ2RlngViqpttWFKdhjBw5rwhuHeEdQYO53eZi20a62GZnHHFXhHmB
# RQsi1XkbfrZjeY9EOPR38Qp1wAQDpeW7HlYaJRXYpixGI7bB4cwfj8b5Pum0PDEB
# FluA60vCUKVSd3h6gM5vwQ8SiSNTX0tE6FUQ2nHurJO2Uou38zrb3LS/fcHqm9LM
# WafYW/+d0w8n4y16s14HKKXtAgMBAAGjggEbMIIBFzAdBgNVHQ4EFgQU0sewF64Z
# vu1wgvtU6aRtt8fFpa4wHwYDVR0jBBgwFoAU1WM6XIoxkPNDe3xGG8UzaFqFbVUw
# VgYDVR0fBE8wTTBLoEmgR4ZFaHR0cDovL2NybC5taWNyb3NvZnQuY29tL3BraS9j
# cmwvcHJvZHVjdHMvTWljVGltU3RhUENBXzIwMTAtMDctMDEuY3JsMFoGCCsGAQUF
# BwEBBE4wTDBKBggrBgEFBQcwAoY+aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3Br
# aS9jZXJ0cy9NaWNUaW1TdGFQQ0FfMjAxMC0wNy0wMS5jcnQwDAYDVR0TAQH/BAIw
# ADATBgNVHSUEDDAKBggrBgEFBQcDCDANBgkqhkiG9w0BAQsFAAOCAQEAp7vNG5qg
# BOhaRW3GkH9UhjujBHsceVgH/x8tLljQeNiNjwGROWsvzH9GZmYX/HE/HWr3WBmE
# bzqjOW4dk4oQNvVVyBeOjW8f+R1Mo74JP34fkXLmYBQHXXBMWy1xTgclhctFyNfr
# KSIT/+N/ZtGERMeIPrYcpBmYh7BO4pkoEZIVnqE0pj3tgCh/yKkedBQ7/FBP04G1
# 1oFMQnNIe/MssXGVxF44AiCukCwkWPUTciJxhKVKwL7497jfdV9ZSRCNwdgf1bfT
# 9n8uUWxrAya2eX7nCtlzkqTKUmfPfQ1j/eXcReFXx1cbIgwbiv5ISMRLaqi8VrB6
# eeEg0FsqLv7iyKGCA6UwggKNAgEBMIH6oYHQpIHNMIHKMQswCQYDVQQGEwJVUzET
# MBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMV
# TWljcm9zb2Z0IENvcnBvcmF0aW9uMSUwIwYDVQQLExxNaWNyb3NvZnQgQW1lcmlj
# YSBPcGVyYXRpb25zMSYwJAYDVQQLEx1UaGFsZXMgVFNTIEVTTjoxMkU3LTMwNjQt
# NjExMjElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZaIlCgEB
# MAkGBSsOAwIaBQADFQA8ZhJFUNlWp1EOnc74tuoxDHgAVqCB2jCB16SB1DCB0TEL
# MAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
# bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjElMCMGA1UECxMcTWlj
# cm9zb2Z0IEFtZXJpY2EgT3BlcmF0aW9uczEnMCUGA1UECxMebkNpcGhlciBOVFMg
# RVNOOjI2NjUtNEMzRi1DNURFMSswKQYDVQQDEyJNaWNyb3NvZnQgVGltZSBTb3Vy
# Y2UgTWFzdGVyIENsb2NrMA0GCSqGSIb3DQEBBQUAAgUA37FxTzAiGA8yMDE4MTIw
# NDIxMzQwN1oYDzIwMTgxMjA1MjEzNDA3WjB0MDoGCisGAQQBhFkKBAExLDAqMAoC
# BQDfsXFPAgEAMAcCAQACAgn6MAcCAQACAhyEMAoCBQDfssLPAgEAMDYGCisGAQQB
# hFkKBAIxKDAmMAwGCisGAQQBhFkKAwGgCjAIAgEAAgMW42ChCjAIAgEAAgMHoSAw
# DQYJKoZIhvcNAQEFBQADggEBAAGqeODHCGRX3X0AG0e7la6p3NkRuRTGsg9I0IoR
# pOsHDw6pcvwdRQtWfkoM0/JBw+BJPlInOJu+u/cqUb8e/q5lmO6O4LYDBhsy/zF5
# juYSDA2wzulTZLeEmkFNb6jfP1SCJx+msWeNilZYawIqm5NWXtnaacpaC6zGbWTd
# sBZihi5r7zM3wkw7m404UGrSL0QRurYi5PgLd9BdQpJtsUMAvmbC9Dy7nECvWQ0f
# +ZIc0/NJkDnfN+UNKvJl6mCTdj5wRa1J7hFYkUSP7QLuEtj2guza6QGFez7Y+ysS
# s10p++yslrJuwM4KYKc1fr8FoDDjxt5J0t7EgZ6mnpPD+lIxggL1MIIC8QIBATCB
# kzB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
# UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYDVQQD
# Ex1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMAITMwAAAOrhzv+as6aS0QAA
# AAAA6jANBglghkgBZQMEAgEFAKCCATIwGgYJKoZIhvcNAQkDMQ0GCyqGSIb3DQEJ
# EAEEMC8GCSqGSIb3DQEJBDEiBCDqiDRnd7VM4BgZf+o61bZRJvrfmTWuU44676sK
# ACqcQjCB4gYLKoZIhvcNAQkQAgwxgdIwgc8wgcwwgbEEFDxmEkVQ2VanUQ6dzvi2
# 6jEMeABWMIGYMIGApH4wfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
# b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3Jh
# dGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTACEzMA
# AADq4c7/mrOmktEAAAAAAOowFgQUG6osdSTNVZJjIx7YhaBIVsMgENwwDQYJKoZI
# hvcNAQELBQAEggEAmpwL8O/twmV70rhkQ3CC635RWD94jdGcHBTObd+CcSm0YxDQ
# zzae8XZI6SN/So3NtXsjlJfEHbDSQ/Gj9oGumzbsPeLQUl/kqXpCKIz0gdsDuDfv
# BFuZqJOciPCVCJ6fqbOzIm3+nty4OvL74H/VXnYbfA7k1nEz/vKjqkBsLMaM8Dm4
# 5nEbqyomY9yb0On4d72UHbxHWCQrlw1QfDeeEu63p/4pThHIWpn0FjyHIm5Q+Gah
# mHLipNQxVFcYxck0GdWjCQL5JFFq/Ory5ozTuV5pPIfydABUHhz4DumH2Xacczx2
# 6653BrXSciAnU3WwJdwwska0EyBotqVrArm8VA==
# SIG # End signature block

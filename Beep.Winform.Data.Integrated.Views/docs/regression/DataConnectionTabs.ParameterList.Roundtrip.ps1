param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

if ([System.Threading.Thread]::CurrentThread.ApartmentState -ne [System.Threading.ApartmentState]::STA) {
    throw "Run this script in STA mode (PowerShell -STA) to instantiate WinForms controls."
}

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$projectFile = Join-Path $projectRoot "TheTechIdea.Beep.Winform.Default.Views.csproj"
$tabsFolder = Join-Path $projectRoot "DataSource_Connection_Controls"
$outputDir = Join-Path $projectRoot "bin\Debug\net8.0-windows"

if (-not $SkipBuild) {
    Write-Host "[INFO] Building project..." -ForegroundColor Cyan
    dotnet build $projectFile -v minimal -nologo -clp:ErrorsOnly
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed."
    }
}

if (-not (Test-Path $outputDir)) {
    throw "Build output not found: $outputDir"
}

Write-Host "[INFO] Loading assemblies..." -ForegroundColor Cyan
$requiredAssemblies = @(
    "DataManagementModels.dll",
    "DataManagementEngine.dll",
    "TheTechIdea.Beep.Winform.Default.Views.dll"
)

foreach ($asmName in $requiredAssemblies) {
    $asmPath = Join-Path $outputDir $asmName
    if (-not (Test-Path $asmPath)) {
        throw "Required assembly not found: $asmPath"
    }

    [void][System.Reflection.Assembly]::LoadFrom($asmPath)
}

Get-ChildItem -Path $outputDir -Filter "*.dll" | ForEach-Object {
    if ($requiredAssemblies -contains $_.Name) { return }
    try { [void][System.Reflection.Assembly]::LoadFrom($_.FullName) } catch { }
}

Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.Application]::EnableVisualStyles()

$failures = New-Object System.Collections.Generic.List[string]

function Add-Failure {
    param([string]$Message)
    $script:failures.Add($Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
}

function Add-Pass {
    param([string]$Message)
    Write-Host "[PASS] $Message" -ForegroundColor Green
}

Write-Host "[INFO] Static tab coverage checks..." -ForegroundColor Cyan
$tabFiles = Get-ChildItem -Path $tabsFolder -Filter "uc_*.cs" |
    Where-Object {
        $_.Name -notlike "*.Designer.cs" -and
        $_.Name -ne "uc_DataConnectionBase.cs" -and
        $_.Name -ne "uc_DataConnectionPropertiesBaseControl.cs" -and
        $_.Name -ne "Example_Usage.cs"
    }

foreach ($file in $tabFiles) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "public\s+override\s+void\s+SetupBindings\s*\(\s*ConnectionProperties\s+conn\s*\)") {
        Add-Pass "$($file.Name): SetupBindings override found"
    }
}

Write-Host "[INFO] Runtime roundtrip checks..." -ForegroundColor Cyan

function New-BaseConnection {
    $conn = [TheTechIdea.Beep.ConfigUtil.ConnectionProperties]::new()
    $conn.ConnectionName = "RoundtripRegression"
    $conn.Category = [TheTechIdea.Beep.Utilities.DatasourceCategory]::RDBMS
    $conn.DatabaseType = [TheTechIdea.Beep.DataBase.DataSourceType]::SqlServer
    $conn.Host = "localhost"
    $conn.Port = 1433
    $conn.Database = "master"
    $conn.UserID = "sa"
    $conn.Password = "secret"
    $conn.ConnectionString = "Server=localhost;Database=master;User Id=sa;Password=secret;"
    $conn.ParameterList = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    return $conn
}

function Invoke-DialogRoundtrip {
    param(
        [TheTechIdea.Beep.ConfigUtil.ConnectionProperties]$Connection
    )

    $dialog1 = [TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls.uc_DataConnectionBase]::new()
    $dialog1.InitializeDialog($Connection)
    $afterFirstSave = $dialog1.GetUpdatedProperties()
    $dialog1.Dispose()

    $dialog2 = [TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls.uc_DataConnectionBase]::new()
    $dialog2.InitializeDialog($afterFirstSave)
    $afterReopenSave = $dialog2.GetUpdatedProperties()
    $dialog2.Dispose()

    return $afterReopenSave
}

$tabKeyMatrix = [ordered]@{
    "RequestBehavior" = [ordered]@{
        "ConnectionTimeout" = "65"
        "CommandTimeout" = "95"
        "MinPoolSize" = "3"
        "MaxPoolSize" = "111"
        "Pooling" = "true"
        "Keepalive" = "false"
    }
    "HttpComposition" = [ordered]@{
        "BasePath" = "/api/v2"
        "Accept" = "application/json"
        "ContentType" = "application/json"
        "DefaultHeaders" = "X-Tenant:demo"
        "DefaultQueryParams" = "lang=en"
        "UserAgent" = "RegressionAgent"
        "UseCompression" = "true"
        "FollowRedirects" = "true"
    }
    "MetaData" = [ordered]@{
        "MetadataCatalog" = "dbo"
        "EntityFilter" = "Sales%"
        "MetadataRefreshSeconds" = "45"
        "IncludeViews" = "true"
        "IncludeSystemObjects" = "false"
    }
    "CertificatesSSL" = [ordered]@{
        "SslMode" = "Required"
        "SslCertificate" = "cert.pem"
        "SslKey" = "key.pem"
        "SslRootCertificate" = "ca.pem"
        "SslCrl" = "crl.pem"
        "ValidateCertificateChain" = "true"
    }
    "CredentialsCustom" = [ordered]@{
        "CustomParameter" = "custom-value"
    }
}

$matrixConnection = New-BaseConnection
foreach ($tab in $tabKeyMatrix.Keys) {
    foreach ($key in $tabKeyMatrix[$tab].Keys) {
        $matrixConnection.ParameterList[$key] = $tabKeyMatrix[$tab][$key]
    }
}

$matrixRoundtrip = Invoke-DialogRoundtrip -Connection $matrixConnection

foreach ($tab in $tabKeyMatrix.Keys) {
    $tabFailed = $false
    foreach ($key in $tabKeyMatrix[$tab].Keys) {
        $expected = $tabKeyMatrix[$tab][$key]
        if (-not $matrixRoundtrip.ParameterList.ContainsKey($key)) {
            Add-Failure "$tab key '$key' missing after save/reopen."
            $tabFailed = $true
            continue
        }
        $actual = $matrixRoundtrip.ParameterList[$key]
        if ($actual -ne $expected) {
            Add-Failure "$tab key '$key' value mismatch. Expected '$expected' got '$actual'."
            $tabFailed = $true
        }
    }
    if (-not $tabFailed) {
        Add-Pass "$tab roundtrip"
    }
}

$typedConnection = New-BaseConnection
$typedConnection.ParameterList.Clear()
$typedConnection.ParameterList["CustomExtra"] = "only-extra"
$typedConnection.Parameters = "Pooling=true;CustomFlag=1"
$typedConnection.ApiKey = "api-from-property"
$typedConnection.ClientId = "client-from-property"
$typedConnection.ClientSecret = "secret-from-property"
$typedConnection.UseProxy = $true
$typedConnection.ProxyUrl = "http://127.0.0.1"
$typedConnection.ProxyPort = 8080
$typedConnection.ProxyUser = "proxy-user"
$typedConnection.ProxyPassword = "proxy-pass"
$typedConnection.UseSSL = $true
$typedConnection.RequireSSL = $true
$typedConnection.TimeoutMs = 12345
$typedConnection.MaxRetries = 7
$typedConnection.RetryIntervalMs = 250
$typedConnection.DriverName = "Driver-A"
$typedConnection.DriverVersion = "1.2.3"

$typedRoundtrip = Invoke-DialogRoundtrip -Connection $typedConnection

$typedFailed = $false
if ($typedRoundtrip.ApiKey -ne "api-from-property") { Add-Failure "Typed roundtrip: ApiKey mismatch"; $typedFailed = $true }
if ($typedRoundtrip.ClientId -ne "client-from-property") { Add-Failure "Typed roundtrip: ClientId mismatch"; $typedFailed = $true }
if ($typedRoundtrip.ClientSecret -ne "secret-from-property") { Add-Failure "Typed roundtrip: ClientSecret mismatch"; $typedFailed = $true }
if ($typedRoundtrip.UseProxy -ne $true) { Add-Failure "Typed roundtrip: UseProxy mismatch"; $typedFailed = $true }
if ($typedRoundtrip.ProxyUrl -ne "http://127.0.0.1") { Add-Failure "Typed roundtrip: ProxyUrl mismatch"; $typedFailed = $true }
if ($typedRoundtrip.ProxyPort -ne 8080) { Add-Failure "Typed roundtrip: ProxyPort mismatch"; $typedFailed = $true }
if ($typedRoundtrip.ProxyUser -ne "proxy-user") { Add-Failure "Typed roundtrip: ProxyUser mismatch"; $typedFailed = $true }
if ($typedRoundtrip.ProxyPassword -ne "proxy-pass") { Add-Failure "Typed roundtrip: ProxyPassword mismatch"; $typedFailed = $true }
if ($typedRoundtrip.UseSSL -ne $true) { Add-Failure "Typed roundtrip: UseSSL mismatch"; $typedFailed = $true }
if ($typedRoundtrip.RequireSSL -ne $true) { Add-Failure "Typed roundtrip: RequireSSL mismatch"; $typedFailed = $true }
if ($typedRoundtrip.TimeoutMs -ne 12345) { Add-Failure "Typed roundtrip: TimeoutMs mismatch"; $typedFailed = $true }
if ($typedRoundtrip.MaxRetries -ne 7) { Add-Failure "Typed roundtrip: MaxRetries mismatch"; $typedFailed = $true }
if ($typedRoundtrip.RetryIntervalMs -ne 250) { Add-Failure "Typed roundtrip: RetryIntervalMs mismatch"; $typedFailed = $true }
if ($typedRoundtrip.DriverName -ne "Driver-A") { Add-Failure "Typed roundtrip: DriverName mismatch"; $typedFailed = $true }
if ($typedRoundtrip.DriverVersion -ne "1.2.3") { Add-Failure "Typed roundtrip: DriverVersion mismatch"; $typedFailed = $true }
if ($typedRoundtrip.Parameters -ne "Pooling=true;CustomFlag=1") { Add-Failure "Typed roundtrip: Parameters mismatch"; $typedFailed = $true }
if (-not $typedRoundtrip.ParameterList.ContainsKey("CustomExtra") -or $typedRoundtrip.ParameterList["CustomExtra"] -ne "only-extra") {
    Add-Failure "Extra ParameterList key did not roundtrip."
    $typedFailed = $true
}

$typedKeysThatMustStayTyped = @("ApiKey","ClientId","ClientSecret","UseProxy","ProxyUrl","ProxyPort","ProxyUser","ProxyPassword","UseSSL","RequireSSL","TimeoutMs","MaxRetries","RetryIntervalMs","DriverName","DriverVersion")
foreach ($key in $typedKeysThatMustStayTyped) {
    if ($typedRoundtrip.ParameterList.ContainsKey($key)) {
        Add-Failure "Typed key '$key' leaked into ParameterList; should remain on ConnectionProperties."
        $typedFailed = $true
    }
}

if (-not $typedFailed) {
    Add-Pass "Typed ConnectionProperties + extra ParameterList roundtrip"
}

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "[SUMMARY] FAILURES: $($failures.Count)" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Write-Host ""
Write-Host "[SUMMARY] All regression checks passed." -ForegroundColor Green
exit 0

param(
  [Parameter(Mandatory = $false)]
  [ValidateSet("dev", "qa")]
  [string]$Env
)

$ErrorActionPreference = "Stop"

function Invoke-Checked([string]$FilePath, [string[]]$ArgumentList) {
  & $FilePath @ArgumentList
  if ($LASTEXITCODE -ne 0) {
    throw "$FilePath failed with exit code $LASTEXITCODE. Args: $($ArgumentList -join ' ')"
  }
}

function Ensure-FileNotEmpty($Path, $Hint) {
  if (-not (Test-Path $Path)) { throw "Missing file: $Path" }
  $content = (Get-Content $Path -Raw).Trim()
  if ([string]::IsNullOrWhiteSpace($content)) { throw "Empty file: $Path. $Hint" }
}

function Ensure-EnvVarSetInFile($Path, $VarName, $Hint) {
  if (-not (Test-Path $Path)) { throw "Missing file: $Path" }
  $line = (Get-Content $Path | Where-Object { $_ -match ("^\s*" + [regex]::Escape($VarName) + "\s*=") } | Select-Object -First 1)
  if (-not $line) { throw "Missing $VarName in $Path. $Hint" }
  $value = ($line -split "=", 2)[1].Trim()
  if ([string]::IsNullOrWhiteSpace($value)) { throw "$VarName is blank in $Path. $Hint" }
}

function Ensure-AppSettingNotBlank($Path, $JsonPath, $Hint) {
  if (-not (Test-Path $Path)) { throw "Missing file: $Path" }
  $json = Get-Content $Path -Raw | ConvertFrom-Json
  $segments = $JsonPath -split "\."
  $cur = $json
  foreach ($seg in $segments) {
    if ($null -eq $cur) { break }
    $cur = $cur.$seg
  }
  if ([string]::IsNullOrWhiteSpace([string]$cur)) { throw "$JsonPath is blank in $Path. $Hint" }
}

function ConvertTo-HashtableDeep($Obj) {
  if ($null -eq $Obj) { return $null }
  if ($Obj -is [System.Collections.IDictionary]) {
    $ht = @{}
    foreach ($k in $Obj.Keys) { $ht[$k] = ConvertTo-HashtableDeep $Obj[$k] }
    return $ht
  }
  if ($Obj -is [System.Collections.IEnumerable] -and -not ($Obj -is [string])) {
    $arr = @()
    foreach ($item in $Obj) { $arr += ,(ConvertTo-HashtableDeep $item) }
    return $arr
  }
  if ($Obj -is [pscustomobject]) {
    $ht = @{}
    foreach ($p in $Obj.PSObject.Properties) { $ht[$p.Name] = ConvertTo-HashtableDeep $p.Value }
    return $ht
  }
  return $Obj
}

function Merge-HashtableDeep($Base, $Override) {
  if ($null -eq $Base) { return $Override }
  if ($null -eq $Override) { return $Base }

  # Arrays: override replaces base
  if (($Base -is [System.Collections.IEnumerable] -and -not ($Base -is [string])) -and
      ($Override -is [System.Collections.IEnumerable] -and -not ($Override -is [string])) -and
      -not ($Base -is [System.Collections.IDictionary]) -and
      -not ($Override -is [System.Collections.IDictionary])) {
    return $Override
  }

  # Dictionaries: merge keys recursively
  if (($Base -is [System.Collections.IDictionary]) -and ($Override -is [System.Collections.IDictionary])) {
    $result = @{}
    foreach ($k in $Base.Keys) { $result[$k] = $Base[$k] }
    foreach ($k in $Override.Keys) {
      if ($result.ContainsKey($k)) { $result[$k] = Merge-HashtableDeep $result[$k] $Override[$k] }
      else { $result[$k] = $Override[$k] }
    }
    return $result
  }

  # Scalars: override wins
  return $Override
}

function Write-MergedAppSettings($BasePath, $OverridePath, $OutPath) {
  if (-not (Test-Path $BasePath)) { throw "Missing base appsettings: $BasePath" }
  if (-not (Test-Path $OverridePath)) { throw "Missing override appsettings: $OverridePath" }

  function Read-JsonAllowingLineComments($Path) {
    $rawLines = Get-Content $Path
    $cleanLines = $rawLines | Where-Object { -not ($_.TrimStart().StartsWith("//")) }
    ($cleanLines -join "`n") | ConvertFrom-Json
  }

  $baseObj = Read-JsonAllowingLineComments $BasePath
  $overrideObj = Read-JsonAllowingLineComments $OverridePath
  $baseHt = ConvertTo-HashtableDeep $baseObj
  $overrideHt = ConvertTo-HashtableDeep $overrideObj
  $merged = Merge-HashtableDeep $baseHt $overrideHt

  ($merged | ConvertTo-Json -Depth 30) | Set-Content -Path $OutPath -Encoding UTF8
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$artifacts = Join-Path $repoRoot "artifacts"

function Build-OneEnv([string]$TargetEnv) {
  $apiOut = Join-Path $artifacts ("api-" + $TargetEnv)
  $uiOut = Join-Path $artifacts ("ui-" + $TargetEnv)

  Write-Host "Building for environment: $TargetEnv"

  # --- UI ---
  Push-Location (Join-Path $repoRoot "VueUI")
  try {
    $uiEnvFile = Join-Path (Get-Location) (".env." + $TargetEnv)
    Ensure-FileNotEmpty $uiEnvFile "Expected an env file like $uiEnvFile."
    Ensure-EnvVarSetInFile $uiEnvFile "VITE_API_BASE_URL" "Set VITE_API_BASE_URL in VueUI/.env.$TargetEnv."

    npm run ("build:" + $TargetEnv)
    if ($LASTEXITCODE -ne 0) { throw "npm build failed for $TargetEnv (exit $LASTEXITCODE)" }

    if (Test-Path $uiOut) { Remove-Item $uiOut -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $uiOut | Out-Null

    $distDir = Join-Path (Get-Location) ("dist-" + $TargetEnv)
    Copy-Item -Path (Join-Path $distDir "*") -Destination $uiOut -Recurse -Force
  }
  finally {
    Pop-Location
  }

  # --- API ---
  $apiProj = Join-Path $repoRoot "api\portalApi\portalApi.csproj"
  $apiBaseSettings = Join-Path $repoRoot "api\portalApi\appsettings.json"
  $apiEnvSettings = Join-Path $repoRoot ("api\portalApi\appsettings." + $TargetEnv.ToUpper() + ".json")
  if (-not (Test-Path $apiEnvSettings)) {
    $apiEnvSettings = Join-Path $repoRoot ("api\portalApi\appsettings." + (Get-Culture).TextInfo.ToTitleCase($TargetEnv) + ".json")
  }

  Ensure-FileNotEmpty $apiEnvSettings "Expected an env settings file like $apiEnvSettings."
  Ensure-AppSettingNotBlank $apiEnvSettings "ConnectionStrings.DefaultConnection" "Set ConnectionStrings.DefaultConnection in $apiEnvSettings."
  Ensure-AppSettingNotBlank $apiEnvSettings "SearchApiUrl" "Set SearchApiUrl in $apiEnvSettings."

  Invoke-Checked "dotnet" @("restore", $apiProj, "-r", "win-x86")
  Invoke-Checked "dotnet" @("publish", $apiProj, "-c", "Release", "-f", "net9.0", "-r", "win-x86", "--self-contained", "true", "-o", $apiOut)

  # Produce a full appsettings.json for the target env by merging repo base + env overrides
  $outAppSettings = Join-Path $apiOut "appsettings.json"
  Write-MergedAppSettings $apiBaseSettings $apiEnvSettings $outAppSettings

  Write-Host "Done."
  Write-Host "UI:  $uiOut"
  Write-Host "API: $apiOut"
}

if ([string]::IsNullOrWhiteSpace($Env)) {
  Build-OneEnv "dev"
  Build-OneEnv "qa"
} else {
  Build-OneEnv $Env
}

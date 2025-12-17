param(
  [Parameter(Mandatory = $false)]
  [ValidateSet("dev", "qa")]
  [string]$Env
)

$ErrorActionPreference = "Stop"

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
  $apiEnvSettings = Join-Path $repoRoot ("api\portalApi\appsettings." + $TargetEnv.ToUpper() + ".json")
  if (-not (Test-Path $apiEnvSettings)) {
    $apiEnvSettings = Join-Path $repoRoot ("api\portalApi\appsettings." + (Get-Culture).TextInfo.ToTitleCase($TargetEnv) + ".json")
  }

  Ensure-FileNotEmpty $apiEnvSettings "Expected an env settings file like $apiEnvSettings."
  Ensure-AppSettingNotBlank $apiEnvSettings "ConnectionStrings.DefaultConnection" "Set ConnectionStrings.DefaultConnection in $apiEnvSettings."
  Ensure-AppSettingNotBlank $apiEnvSettings "SearchApiUrl" "Set SearchApiUrl in $apiEnvSettings."

  dotnet publish $apiProj -c Release -f net9.0 -r win-x86 --self-contained true -o $apiOut

  # Overwrite published appsettings.json with the environment-specific one
  Copy-Item -Path $apiEnvSettings -Destination (Join-Path $apiOut "appsettings.json") -Force

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

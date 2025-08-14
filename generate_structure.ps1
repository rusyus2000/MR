# PowerShell script to generate repository structure
# Save this as generate_structure.ps1 and run it in your MR folder

$OutputFile = "repo_structure.txt"

Write-Host "Generating repository structure..." -ForegroundColor Green

# Start writing to file
@"
Repository Structure for MR
Generated on: $(Get-Date)
======================================

"@ | Out-File -FilePath $OutputFile

# Function to get directory tree
function Get-DirectoryTree {
    param (
        [string]$Path = ".",
        [string]$Prefix = "",
        [string[]]$ExcludeFolders = @(".git", "node_modules", ".DS_Store", "dist", "build", "coverage", ".env", ".vscode", ".idea", "bin", "obj", ".vs", "packages", "TestResults", "x64", "x86", "Debug", "Release", "_ReSharper.*", "*.suo", "*.user", ".nuget", "artifacts"),
        [string[]]$ExcludeFiles = @("*.dll", "*.exe", "*.pdb", "*.cache", "*.log", "*.tmp", "*.temp", "Thumbs.db", ".DS_Store")
    )
    
    $items = Get-ChildItem -Path $Path -Force | Where-Object {
        if ($_.PSIsContainer) {
            # Check if folder should be excluded
            $folderName = $_.Name
            $shouldExclude = $false
            foreach ($pattern in $ExcludeFolders) {
                if ($folderName -like $pattern) {
                    $shouldExclude = $true
                    break
                }
            }
            -not $shouldExclude
        } else {
            # Check if file should be excluded
            $fileName = $_.Name
            $shouldExclude = $false
            foreach ($pattern in $ExcludeFiles) {
                if ($fileName -like $pattern) {
                    $shouldExclude = $true
                    break
                }
            }
            -not $shouldExclude
        }
    } | Sort-Object { $_.PSIsContainer }, Name
    
    $count = $items.Count
    $index = 0
    
    foreach ($item in $items) {
        $index++
        $isLast = ($index -eq $count)
        $connector = if ($isLast) { "+-- " } else { "|-- " }
        
        if ($item.PSIsContainer) {
            # Directory
            "$Prefix$connector$($item.Name)/" | Out-File -FilePath $OutputFile -Append
            
            $newPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix|   " }
            Get-DirectoryTree -Path $item.FullName -Prefix $newPrefix -ExcludeFolders $ExcludeFolders -ExcludeFiles $ExcludeFiles
        }
        else {
            # File with size
            $size = if ($item.Length -gt 1MB) {
                "{0:N1} MB" -f ($item.Length / 1MB)
            }
            elseif ($item.Length -gt 1KB) {
                "{0:N1} KB" -f ($item.Length / 1KB)
            }
            else {
                "$($item.Length) B"
            }
            
            "$Prefix$connector$($item.Name) ($size)" | Out-File -FilePath $OutputFile -Append
        }
    }
}

# Generate tree structure
"MR/" | Out-File -FilePath $OutputFile -Append
Get-DirectoryTree -Path "."

# Add summary statistics
@"

======================================
File count summary:

"@ | Out-File -FilePath $OutputFile -Append

# Count files by extension (excluding build artifacts)
$extensions = Get-ChildItem -Path . -Recurse -File | 
    Where-Object { 
        $_.DirectoryName -notmatch "\\.(git|vscode|idea|vs)|node_modules|dist|build|bin|obj|packages|TestResults|x64|x86|Debug|Release|_ReSharper" -and
        $_.Extension -notmatch "\.(dll|exe|pdb|cache|log|tmp|temp)$"
    } |
    Group-Object Extension | 
    Sort-Object Count -Descending |
    Select-Object @{Name="Extension";Expression={if($_.Name){"$($_.Name)"}else{"(no ext)"}}}, Count

"Files by type:" | Out-File -FilePath $OutputFile -Append
foreach ($ext in $extensions) {
    "  {0,-15} {1,3} files" -f $ext.Extension, $ext.Count | Out-File -FilePath $OutputFile -Append
}

$totalFiles = (Get-ChildItem -Path . -Recurse -File | Where-Object { 
    $_.DirectoryName -notmatch "\\.(git|vscode|idea|vs)|node_modules|dist|build|bin|obj|packages|TestResults|x64|x86|Debug|Release|_ReSharper" -and
    $_.Extension -notmatch "\.(dll|exe|pdb|cache|log|tmp|temp)$"
}).Count

$totalDirs = (Get-ChildItem -Path . -Recurse -Directory | Where-Object { 
    $_.Name -notmatch "^\.git$|^node_modules$|^dist$|^build$|^bin$|^obj$|^\.vs$|^packages$|^TestResults$|^x64$|^x86$|^Debug$|^Release$|^_ReSharper"
}).Count

@"

Total files: $totalFiles
Total directories: $totalDirs

======================================
Key files content preview:

"@ | Out-File -FilePath $OutputFile -Append

# Find and display all package.json files
$packageJsonFiles = Get-ChildItem -Path . -Recurse -Filter "package.json" | 
    Where-Object { $_.DirectoryName -notmatch "node_modules|dist|build" }

foreach ($pkg in $packageJsonFiles) {
    $relativePath = $pkg.FullName.Replace($PWD.Path + '\', '').Replace('\', '/')
    "=== $relativePath ===" | Out-File -FilePath $OutputFile -Append
    Get-Content $pkg.FullName | Out-File -FilePath $OutputFile -Append
    "" | Out-File -FilePath $OutputFile -Append
}

# Find and display all .csproj files
$csprojFiles = Get-ChildItem -Path . -Recurse -Filter "*.csproj" | 
    Where-Object { $_.DirectoryName -notmatch "bin|obj" }

foreach ($proj in $csprojFiles) {
    $relativePath = $proj.FullName.Replace($PWD.Path + '\', '').Replace('\', '/')
    "=== $relativePath ===" | Out-File -FilePath $OutputFile -Append
    Get-Content $proj.FullName | Out-File -FilePath $OutputFile -Append
    "" | Out-File -FilePath $OutputFile -Append
}

# Find and display appsettings.json files
$appSettingsFiles = Get-ChildItem -Path . -Recurse -Filter "appsettings.json" | 
    Where-Object { $_.DirectoryName -notmatch "bin|obj" }

foreach ($settings in $appSettingsFiles) {
    $relativePath = $settings.FullName.Replace($PWD.Path + '\', '').Replace('\', '/')
    "=== $relativePath ===" | Out-File -FilePath $OutputFile -Append
    Get-Content $settings.FullName | Out-File -FilePath $OutputFile -Append
    "" | Out-File -FilePath $OutputFile -Append
}

# Find and display Program.cs files (first 50 lines)
$programFiles = Get-ChildItem -Path . -Recurse -Filter "Program.cs" | 
    Where-Object { $_.DirectoryName -notmatch "bin|obj" }

foreach ($prog in $programFiles) {
    $relativePath = $prog.FullName.Replace($PWD.Path + '\', '').Replace('\', '/')
    "=== $relativePath (first 50 lines) ===" | Out-File -FilePath $OutputFile -Append
    Get-Content $prog.FullName -Head 50 | Out-File -FilePath $OutputFile -Append
    "" | Out-File -FilePath $OutputFile -Append
}

# Find and display README files
$readmeFiles = Get-ChildItem -Path . -Recurse -Filter "README.md"

foreach ($readme in $readmeFiles) {
    $relativePath = $readme.FullName.Replace($PWD.Path + '\', '').Replace('\', '/')
    "=== $relativePath ===" | Out-File -FilePath $OutputFile -Append
    Get-Content $readme.FullName | Out-File -FilePath $OutputFile -Append
    "" | Out-File -FilePath $OutputFile -Append
}

# Find and display .gitignore
if (Test-Path ".gitignore") {
    "=== .gitignore ===" | Out-File -FilePath $OutputFile -Append
    Get-Content ".gitignore" | Out-File -FilePath $OutputFile -Append
    "" | Out-File -FilePath $OutputFile -Append
}

Write-Host "Structure saved to: $OutputFile" -ForegroundColor Green
Write-Host "You can now share this file with Claude!" -ForegroundColor Yellow
param($installPath, $toolsPath, $package, $project)

$invalidVsVersion = $false

if ('15.0', '16.0', '17.0' -notcontains $project.DTE.Version) {
    $invalidVsVersion = $true
}

if ($invalidVsVersion) {
    throw 'This package can only be installed on Visual Studio 2017 or later.'
}

if ($project.Object.AnalyzerReferences -eq $null) {
    throw 'This package cannot be installed without an analyzer reference.'
}

if ($project.Type -ne "VB.NET") {
    throw 'This package can only be installed on VB.NET projects.'
}

$analyzersPath = Split-Path -Path $toolsPath -Parent
$analyzersPath = Join-Path $analyzersPath "analyzers"

$analyzerFilePath = Join-Path $analyzersPath "burtonrodman.WebAppMembershipProfileSourceGenerator.dll"
$project.Object.AnalyzerReferences.Add($analyzerFilePath)
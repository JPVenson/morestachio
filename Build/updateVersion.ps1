Param (
    [parameter(Mandatory=$true)][string]$path, 
    [parameter(Mandatory=$true)][string]$revision,
    [parameter(Mandatory=$true)][string]$description
)

$revision = $revision.Replace(".1", "");

if([string]::IsNullOrWhiteSpace($path))
{
    "##vso[task.logissue type=error;] No Path for Version Update Specified";
    exit 1;
}


if([string]::IsNullOrWhiteSpace($revision))
{
    "##vso[task.logissue type=error;] No revision for Version Update Specified";
    exit 1;
}

Write-Output "Replace build number with $revision in $path"

function UpdateVersion {
Param (
    [parameter(Mandatory=$true)][string]$pathToFile
)
    Write-Output "Update $pathToFile";
    $fileContent = [System.IO.File]::ReadAllText($pathToFile);
        
    if($pathToFile.EndsWith(".cs")) {
        Write-Output "Found CS file";
        $result = $fileContent -replace "(\d+\.\d+\.\d+)(\.\d+)", "`$1.$revision";
        $result = $result -replace 
            "AssemblyInformationalVersion\(`"(.+)`"\)", 
            "`AssemblyInformationalVersion(`"$description`")";
    }
        
    if($pathToFile.EndsWith(".props")) {
        Write-Output "Found props file";
        $result = $fileContent -replace "<Version>(\d+\.\d+\.\d+)(\.\d+)?<\/Version>", "<Version>`$1.$revision</Version>";
        $result = $result -replace 
            "AssemblyInformationalVersion\(`"(.+)`"\)", 
            "`AssemblyInformationalVersion(`"$description`")";
    }
    
    if($pathToFile.EndsWith(".sql")) {
        Write-Output "Found SQL file";
        $result = $fileContent -replace "(\d+\.\d+\.\d+)(\.\d+)", "`$1.$revision";
    }
    
    if($pathToFile.EndsWith(".wxs")) {
        Write-Output "Found WIX Setup file";
        $result = $fileContent -replace "Version=`"(\d+\.\d+\.\d+)(\.\d+)`"", "Version=`"`$1.$revision`"";
    }
    [System.IO.File]::WriteAllText($pathToFile, $result);
}
"Update all csproj files under " + $path;
foreach($file in [System.IO.Directory]::EnumerateFiles($path, "*AssemblyInfo.cs", [System.IO.SearchOption]::AllDirectories))
{
    UpdateVersion $file;
}


"Update all wix files under " + $path;
foreach($file in [System.IO.Directory]::EnumerateFiles($path, "*.wxs", [System.IO.SearchOption]::AllDirectories))
{
    UpdateVersion $file;
}

"Update all sql files under $path";
foreach($file in [System.IO.Directory]::EnumerateFiles($path, "Script.PostDeployment.sql", [System.IO.SearchOption]::AllDirectories))
{
    UpdateVersion $file;
}

"Update all Directory.Build.props files under $path";
foreach($file in [System.IO.Directory]::EnumerateFiles($path, "Directory.Build.props", [System.IO.SearchOption]::AllDirectories))
{
    UpdateVersion $file;
}

# Script static variables
$buildDir = $env:APPVEYOR_BUILD_FOLDER
$buildNumber = $env:APPVEYOR_BUILD_VERSION
$projectDir = $buildDir + "\Anviz.SDK";
$projectFile = $projectDir + "\Anviz.SDK.csproj";
$nugetFile = $projectDir + "\Anviz.SDK." + $buildNumber + ".nupkg";

cd $projectDir

# Display .Net Core version
Write-Host "Checking .NET Core version" -ForegroundColor Green
& dotnet --version

# Restore the main project
Write-Host "Restoring project" -ForegroundColor Green
& dotnet restore $projectFile --verbosity m

# Publish the project
Write-Host "Publishing project" -ForegroundColor Green
& dotnet publish $projectFile

# Generate a NuGet package for publishing
Write-Host "Generating NuGet Package" -ForegroundColor Green
& dotnet pack  -c Release /p:PackageVersion=$buildNumber -o $projectDir

# Save generated artifacts
Write-Host "Saving Artifacts" -ForegroundColor Green
Push-AppveyorArtifact $nugetFile

# Publish package to NuGet
Write-Host "Publishing NuGet package" -ForegroundColor Green
& dotnet nuget push $nugetFile -k $env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json

# Done
Write-Host "Done!" -ForegroundColor Green

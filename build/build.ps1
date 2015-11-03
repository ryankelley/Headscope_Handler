properties {

  #PARAMS
  $version = "1.0.0.0"
  $build_level = "Debug"
  $build_number = "build_number_not_set"
  $commit_hash = "hash_not_set"
  $nuget_config_path = ""

  #PATHS
  $build_dir = Split-Path $psake.build_script_file
  $solution_dir = Split-Path $build_dir
  $build_output = "$build_dir\artifacts"
  $srcDir = "$solution_dir\src"
  $nuget_dir = "$build_dir\..\src\packages\"
  $package_dir = "$build_dir\..\packages\"
  $artifacts_dir = "$build_dir\..\artifacts"

  #SLN INFO
  $company_name = "Headscope"
  $solution_name = "HeadScopeMessageHandler"
  $solution_file = "$srcDir\$solution_name.sln"
  $client_apps = @("HeadScopeMessageHandler")

  #ILMerge
  $ilmerge_path = "ilmerge.2.14.1208\tools\ILMerge.exe"
}

include tools\psake_utils.ps1
include tools\config-utils.ps1
include tools\nuget-utils.ps1
include tools\git-utils.ps1
include tools\ilmerge-utils.ps1

task default -depends Compile, Get-ReleaseNotes, Get-Version, Package

task Compile -depends Clean, Package-Restore {
  $script:build_level = $build_level
  $version = getVersion
  Exec {
  	msbuild "$solution_file" `
  	        /m /nr:false /p:VisualStudioVersion=14.0 `
  	        /t:Rebuild /nologo /v:m `
  	        /p:Configuration="$script:build_level" `
  	        /p:Platform="Any CPU" /p:TrackFileAccess=false
  }
}

task Clean {
  foreach($assembly in $assemblies + $client_apps + $web_apps + $tests){
    $bin = "$srcDir\$assembly\bin\"
    $obj = "$srcDir\$assembly\obj\"
    Write-Host "Removing $bin"
    Delete-Directory($bin)
    Write-Host "Removing $obj"
    Delete-Directory($obj)
  }
}

task Package {
 Create-Directory $artifacts_dir
   & $build_dir\..\packages\NuGet.exe pack $srcDir\HeadScopeMessageHandler\HeadScopeMessageHandler.csproj -Version $version -o $artifacts_dir -Prop Configuration=$build_level
}
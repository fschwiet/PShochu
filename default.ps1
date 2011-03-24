
properties {
    $rootPath = (resolve-path .).path
    $buildDirectory = "$rootPath\Build"
    $psakeModulePath = "$rootPath\tools\psake\psake.psm1"
}

import-module .\tools\PSUpdateXml\PSUpdateXml.psm1

task Default -depends Build, Configure, Test

task Verify40 {

	if( (ls "$env:windir\Microsoft.NET\Framework\v4.0*") -eq $null ) {
		throw "Building PShochu requires .NET 4.0, which doesn't appear to be installed on this machine"
	}
}

task Clean {

    if (test-path $buildDirectory) {
        remove-item $buildDirectory -force -recurse 
    }
}

task Build -depends Verify40, Clean {

	$v4_net_version = (ls "$env:windir\Microsoft.NET\Framework\v4.0*").Name
    exec { &"C:\Windows\Microsoft.NET\Framework\$v4_net_version\MSBuild.exe" "PShochu.sln" /p:OutDir="$buildDirectory\" }
}

task Configure {

    update-xml "$buildDirectory\PShochu.Tests.dll.config" {
        set-xml "//setting[@name='PsakeModulePath']/value" $psakeModulePath
    }
}

task Test {

    exec { & .\tools\NUnit_with_NJasmine\bin\net-2.0\nunit-console.exe "$($buildDirectory)\PShochu.Tests.dll" /xml="$buildDirectory\TestResults.xml" }
}
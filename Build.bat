@echo off

.\Tools\Nuget.exe install .\Source\HotGlue.Core\packages.config -OutputDirectory .\Libraries\
.\Tools\Nuget.exe install .\Source\HotGlue.Tests\packages.config -OutputDirectory .\Libraries\
  
rmdir Build /q/s 
mkdir Build
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe .\Source\HotGlue.sln /p:OutDir=%CD%\Build\ /p:Config=Release /v:Minimal

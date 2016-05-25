@echo off

%~d0
cd "%~p0"

del *.nu*
del *.dll
del *.pdb
del *.xml
del *.ps1

copy ..\Tester\bin\Release\ProductiveRage.*.dll > nul
copy ..\Tester\bin\Release\ProductiveRage.*.xml > nul
copy ..\*.nuspec > nul

..\packages\NuGet.CommandLine.3.4.3\tools\nuget pack -NoPackageAnalysis ProductiveRage.SqlProxyAndReplay.Service.Example.nuspec
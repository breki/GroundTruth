echo Prepares BuildScript.exe for running the build

..\lib\cs-script\cscs.exe /e BuildScripts\BuildScript.cs
xcopy BuildScripts\BuildScript.exe BuildScripts\exe /y
xcopy BuildScripts\bin\Debug\*.dll BuildScripts\exe /y
xcopy BuildScripts\bin\Debug\*.pdb BuildScripts\exe /y

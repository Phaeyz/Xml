setlocal
set libName=Phaeyz.Xml
set repoUrl=https://github.com/Phaeyz/Xml
dotnet run ..\%libName%\bin\Debug\net9.0\%libName%.dll ..\docs --source %repoUrl%/blob/main/%libName% --namespace %libName% --clean
endlocal
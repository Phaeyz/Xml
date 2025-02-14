setlocal
set lib=Phaeyz.Xml
set repoUrl=https://github.com/Phaeyz/Xml
dotnet run ..\%lib%\bin\Debug\net9.0\%lib%.dll ..\docs --source %repoUrl%/blob/main/%lib% --namespace %lib% --clean
endlocal
@echo off
pushd %~dp0\..\
call "Premake\premake5.exe" vs2019
popd

PAUSE
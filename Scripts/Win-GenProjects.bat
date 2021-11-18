@echo off
pushd %~dp0\..\
call "External\Premake\premake5.exe" vs2019
popd

PAUSE
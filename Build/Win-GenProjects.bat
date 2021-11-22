@echo off

pushd %~dp0\Premake\
call "Win64\premake5.exe" vs2019
popd

PAUSE
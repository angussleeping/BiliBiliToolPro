@echo off

REM start to build
echo Start to build docker image
@echo on
docker build --tag bili_tool_web:latest ../../..
@echo off
pause

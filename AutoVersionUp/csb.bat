@echo off

chcp 932 > nul
rem Shift-JISに


rem ##################################################
rem cscb Ver.1.3.5
rem
rem NET Frameworkがインストールされていること前提です
rem
rem ##################################################

PATH="%WINDIR%\Microsoft.NET\Framework\v1.0.3705";%PATH%
PATH="%WINDIR%\Microsoft.NET\Framework\v1.1.4322";%PATH%
PATH="%WINDIR%\Microsoft.NET\Framework\v2.0.50727";%PATH%
PATH="%WINDIR%\Microsoft.NET\Framework\v3.0";%PATH%
PATH="%WINDIR%\Microsoft.NET\Framework\v3.5";%PATH%
PATH="%WINDIR%\Microsoft.NET\Framework\v4.0.30319";%PATH%

cd "%~dp1"

csc "%~nx1"

if ERRORLEVEL 1 (
    echo.
    echo. "%~1"
    echo.コンパイルに失敗しました
    pause
)

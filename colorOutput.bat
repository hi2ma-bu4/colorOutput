@echo off


chcp 932 > nul
rem Shift-JISに

rem ##################################################
rem colorOutput Ver.1.2.3
rem
rem ネットワークに接続されていること前提です
rem
rem ほとんど
rem ^> colorOutput update
rem しか使用しません
rem
rem ##################################################
for /f %%i in ('cmd /k prompt $e^<nul') do Set ESC=%%i

Set basePath=%~dp0
Set practiceDir=AutoVersionUp/
Set mainFile=versionCheck.vbs
Set mainTmpFile=versionCheck_tmp.vbs
Set loadFile=%practiceDir%loadFile.ini
Set versionFile=%practiceDir%versions.dat

Set cRed=%ESC%[91m
Set cGreen=%ESC%[92m
Set cYellow=%ESC%[93m
Set cMagenta=%ESC%[95m
Set cCyan=%ESC%[96m
Set cReset=%ESC%[0m

cd "%basePath%"

Set arg=%~1
Set isComIn=0

:undo
call :Trim %arg%
Set arg=%trim%

if "%arg%"=="" (
	Set isComIn=1
	goto :com_undo
) else if "%arg%"=="/?" (
	goto :helps
) else if "%arg%"=="--help" (
	goto :helps
) else if "%arg%"=="help" (
	goto :helps
) else if "%arg%"=="list" (
	goto :lists
) else if "%arg%"=="update" (
	goto :update
)

echo.%cRed%使用方法については、helpを使用します%cReset%

if %isComIn%==1 (
	pause
)
exit /b 0


rem コマンド入力
:com_undo
Set /P arg=コマンドを入力 colorOutput^> 
goto :undo

rem ヘルプ参照時
:helps
echo.%cGreen%
echo.
echo.使用方法
echo.%cReset% ^> %cYellow%colorOutput%cReset% ^<オプション^>%cGreen%
echo.
echo.オプション内訳
echo.%cReset% ^> %cCyan%help%cReset%
echo.%cGreen%  ヘルプを表示
echo.
echo.%cReset% ^> %cCyan%list%cReset%
echo.%cGreen%  現在公開されているファイル名表示
echo.  %cReset%更新設定のファイルは%cGreen%緑%cReset%
echo.  それ以外のファイルは%cCyan%水色%cGreen%
echo.  (一度updateしないとデータ更新されません)
echo.
echo.
echo.%cReset% ^> %cCyan%update%cReset%
echo.%cGreen%  更新設定のファイルをダウンロード・更新する
echo.%cReset%

if %isComIn%==1 (
	pause
)
exit /b 0

rem ネット上の取得可能ファイル一覧
:lists
for /f "delims=" %%i in (%versionFile%) do (
	call :lists_loop1 "%%i"
)

if %isComIn%==1 (
	pause
)
exit /b 0


:lists_loop1
Set isCall=0
for /f "delims=" %%j in (%loadFile%) do (
	call :lists_loop2 "%~1" "%%j"
)
if %isCall%==0 (
	echo.%cCyan%・%~1%cReset%
)

exit /b 0

:lists_loop2
Set out=%~2
if not "%out:~0,1%"=="!" (
	call :lists_if "%~1" "%~2"
)
exit /b 0

:lists_if
Set out1=%~1
Set out2=%~2
Set out2=%out2:*/=%
echo "%out1%" | find "%out2%" > nul
if %errorlevel%==0 (
	echo.%cGreen%・%out2%%cReset%
	Set isCall=1
)
exit /b 0

rem 実行
:update
ping google.co.jp>nul && goto :connect
ping yahoo.co.jp>nul && goto :connect
echo %cRed%ネットワークに接続されていません%cReset%
exit /b 1
:connect

cd "%practiceDir%"
call "%mainFile%"
if exist "%mainTmpFile%" (
	copy /B /V /Y "%mainTmpFile%" "%mainFile%"
	del /Q "%mainTmpFile%"
)
exit /b 0


rem Trim処理
:Trim
set trim=%*
exit /b 0

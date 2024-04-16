@echo off


chcp 932 > nul
rem Shift-JIS��

rem ##################################################
rem colorOutput Ver.1.2.3
rem
rem �l�b�g���[�N�ɐڑ�����Ă��邱�ƑO��ł�
rem
rem �قƂ��
rem ^> colorOutput update
rem �����g�p���܂���
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

echo.%cRed%�g�p���@�ɂ��ẮAhelp���g�p���܂�%cReset%

if %isComIn%==1 (
	pause
)
exit /b 0


rem �R�}���h����
:com_undo
Set /P arg=�R�}���h����� colorOutput^> 
goto :undo

rem �w���v�Q�Ǝ�
:helps
echo.%cGreen%
echo.
echo.�g�p���@
echo.%cReset% ^> %cYellow%colorOutput%cReset% ^<�I�v�V����^>%cGreen%
echo.
echo.�I�v�V��������
echo.%cReset% ^> %cCyan%help%cReset%
echo.%cGreen%  �w���v��\��
echo.
echo.%cReset% ^> %cCyan%list%cReset%
echo.%cGreen%  ���݌��J����Ă���t�@�C�����\��
echo.  %cReset%�X�V�ݒ�̃t�@�C����%cGreen%��%cReset%
echo.  ����ȊO�̃t�@�C����%cCyan%���F%cGreen%
echo.  (��xupdate���Ȃ��ƃf�[�^�X�V����܂���)
echo.
echo.
echo.%cReset% ^> %cCyan%update%cReset%
echo.%cGreen%  �X�V�ݒ�̃t�@�C�����_�E�����[�h�E�X�V����
echo.%cReset%

if %isComIn%==1 (
	pause
)
exit /b 0

rem �l�b�g��̎擾�\�t�@�C���ꗗ
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
	echo.%cCyan%�E%~1%cReset%
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
	echo.%cGreen%�E%out2%%cReset%
	Set isCall=1
)
exit /b 0

rem ���s
:update
ping google.co.jp>nul && goto :connect
ping yahoo.co.jp>nul && goto :connect
echo %cRed%�l�b�g���[�N�ɐڑ�����Ă��܂���%cReset%
exit /b 1
:connect

cd "%practiceDir%"
call "%mainFile%"
if exist "%mainTmpFile%" (
	copy /B /V /Y "%mainTmpFile%" "%mainFile%"
	del /Q "%mainTmpFile%"
)
exit /b 0


rem Trim����
:Trim
set trim=%*
exit /b 0

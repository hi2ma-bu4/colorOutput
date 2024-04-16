' versionCheck Ver.1.6.4
Option Explicit
Randomize Timer

' Object
Dim ws, fs
Dim objPB
Dim ie, objP

' �萔
Dim proName
Dim progressBarPath
Dim baseURL
Dim versionURL, versionFilePath
Dim loadFilePath, ENVsPath
Dim csbPath

'########################################
' �萔
'########################################
Set ws = CreateObject("WScript.Shell")
Set fs = CreateObject("Scripting.FileSystemObject")

proName = "versionCheck"

progressBarPath = "FProgressBar.vbs"
baseURL = "https://snow-site.glitch.me/colorOutput/"
versionURL = baseURL & "version.html"
versionFilePath = "versions.dat"
loadFilePath = "loadFile.ini"
csbPath = "csb.bat"

' .env�擾
ENVsPath = fs.GetParentFolderName(fs.getParentFolderName(WScript.ScriptFullName))

'########################################
' �N��
'########################################
Main()
Set fs = Nothing
Set ws = Nothing

'########################################
' ���C������
'########################################
Sub Main()
    ' �O����`�t�@�C���ǂݍ���
    Execute fs.OpenTextFile(progressBarPath, 1).ReadAll()
    Set objPB = New ProgressBar
    objPB.SetTitle proName & " - �N����..."
    objPB.SetProgress 0

    '########################################
    ' �l�b�g�ォ��o�[�W�������擾
    objPB.SetTitle proName & " - �l�b�g�ォ��o�[�W�������擾��..."
    Dim strVer

    Set ie = CreateObject("InternetExplorer.Application")
    ie.Visible = False
    ie.navigate versionURL

    waitIE ie
    objPB.SetProgress 1 / 8

    waitDataLoad ie, "LoadEnd"
    objPB.SetProgress 2 / 8

    strVer = ""
    For Each objP In ie.document.getElementsByTagName("p")
        strVer = strVer & objP.innerText & vbCrLf
    Next
    objPB.SetProgress 3 / 8

    OutputText versionFilePath, strVer, ""
    ie.Quit
    Set ie = Nothing

    objPB.SetProgress 4 / 8
    '########################################
    ' �ύX�A�X�V�t�@�C�����擾
    objPB.SetTitle proName & " - �ύX�A�X�V�t�@�C�����擾��..."
    Dim UpdateFilePaths

    ' �X�V�t�@�C�����擾
    UpdateFilePaths = InputText(loadFilePath)
    UpdateFilePaths = Split(UpdateFilePaths, vbCrLf)

    objPB.SetProgress 5 / 8
    '########################################
    ' �擾���ꂽ�t�@�C�����ɑ΂��A�o�[�W�����擾
    objPB.SetTitle proName & " - �擾���ꂽ�t�@�C�����ɑ΂��A�o�[�W�������擾��..."
    Dim i
    Dim UpdateFileMaxCou
    Dim fPath(), fVer()
    Dim objRe_ver, objMatch_ver

    Set objRe_ver = CreateObject("VBScript.RegExp")
    objRe_ver.Pattern = "Ver\.\d+\.\d+\.\d+"

    UpdateFileMaxCou = UBound(UpdateFilePaths)
    ReDim fPath(UpdateFileMaxCou)
    ReDim fVer(UpdateFileMaxCou)

    For i = 0 To UpdateFileMaxCou
        objPB.SetProgress 5 / 8 + (1 / 8) * (i / (UpdateFileMaxCou + 1))
        If UpdateFilePaths(i) <> "" And Left(UpdateFilePaths(i), 1) <> "!" Then
            fPath(i) = BuildPath(ENVsPath, UpdateFilePaths(i))
            fVer(i) = InputText(Replace(fPath(i), "_tmp", ""))
            If fVer(i) = "" Then
                fVer(i) = "Ver.0.0.0"
            Else
                Set objMatch_ver = objRe_ver.Execute(fVer(i))
                If objMatch_ver.Count >= 1 Then
                    fVer(i) = objMatch_ver(0).Value
                Else
                    fVer(i) = "Ver.0.0.0"
                End If
            End If
        Else
            fPath(i) = BuildPath(ENVsPath, "!")
            fVer(i) = "Ver.0.0.0"
        End If
    Next

    objPB.SetProgress 6 / 8
    '########################################
    ' �o�[�W������r�A�X�V
    objPB.SetTitle proName & " - �o�[�W������r�A�X�V..."
    Dim j
    Dim resData, tmpResData
    Dim resDataCou
    Dim fName, fData
    Dim UpdateFiles
    Dim ext, chara

    resData = Split(strVer, vbCrLf)
    resDataCou = UBound(resData)
    UpdateFiles = ""

    For i = 0 To UpdateFileMaxCou
        objPB.SetProgress 6 / 8 + (1 / 8) * (i / (UpdateFileMaxCou + 1))
        fName = fs.getFileName(fPath(i))
        For j = 0 To resDataCou
            If resData(j) <> "" And Left(resData(j), 1) <> "!" Then
                tmpResData = Split(resData(j), ";")
                If fName = tmpResData(1) Then
                    If Not fVer(i) = tmpResData(2) Then
                        ext = fs.GetExtensionName(fPath(i))
                        chara = ""

                        If ext = "cs" Then
                            chara = "UTF-8"
                        End If

                        fData = getUrl2Text(baseURL & tmpResData(0))
                        If fData = "" Then
                            UpdateFiles = UpdateFiles & "(" & fName & ") " & fVer(i) & vbCrLf
                        Else
                            UpdateFiles = UpdateFiles & fName & " " & fVer(i) & " -> " & tmpResData(2) & vbCrLf
                            OutputText fPath(i), Replace(fData, vbLf, vbCrLf), chara

                            If ext = "cs" Then
                                ws.Run csbPath & " """ & fPath(i) & """"
                            End If
                        End If
                    End If
                    Exit For
                End If
            End If
        Next
    Next

    objPB.SetProgress 1
    If UpdateFiles = "" Then
        ws.Popup "�X�V�m�F�A�X�V�������������܂���" & vbCrLf & _
        vbCrLf & "�ύX���ꂽ�t�@�C���͂���܂���", 15, proName
    Else
        ws.Popup "�X�V�m�F�A�X�V�������������܂���" & vbCrLf & _
        vbCrLf & "�ύX���ꂽ�t�@�C��:" & _
        vbCrLf & UpdateFiles & vbCrLf & _
        vbCrLf & "���ʂň͂܂ꂽ�t�@�C�����̓f�[�^�擾�Ɏ��s�����t�@�C���ł�", 15, proName
    End If

    Set objPB = Nothing
End Sub

'########################################
' �t�@�C����������
'########################################
Sub OutputText(ByVal strName, ByVal strMsg, ByVal Charset)
    If Charset = "" Then
        Dim objText
        Set objText = fs.OpenTextFile(strName, 2, True)

        objText.write strMsg
        objText.Close
        Set objText = Nothing
    Else
        Dim stream
        Set stream = CreateObject("ADODB.Stream")
        stream.Open
        stream.Type = 2
        stream.Charset = Charset
        stream.WriteText strMsg
        stream.SaveToFile strName, 2
        stream.Close

        Set stream = Nothing
    End If
End Sub
'########################################
' �t�@�C���ǂݍ���
'########################################
Function InputText(ByVal strName)
    Dim objText

    If fs.FileExists(strName) Then
        Set objText = fs.OpenTextFile(strName, 1, False)

        InputText = objText.ReadAll
        objText.Close
        Set objText = Nothing
    Else
        InputText = ""
    End If
End Function

'########################################
' path����
'########################################
Function BuildPath(ByVal sBasePath, ByVal sAppendPath)
    BuildPath = fs.BuildPath(sBasePath, sAppendPath)
End Function

'########################################
' ie�ҋ@
'########################################
Sub waitIE(objIe)
    Do While objIe.Busy = True Or objIe.readystate <> 4
        WScript.Sleep 1000
    Loop
End Sub

Sub waitDataLoad(objIe, ByVal strType)
    Dim flag, Elem, tmp
    Do
        On Error Resume Next
        flag = False
        Set Elem = objIe.document.getElementById("isLoad")
        If Err.Number = 0 Then
            flag = True
        End If
        On Error GoTo 0

        If flag = True Then
            If Elem.innerText = strType Then
                Exit Do
            End If
        End If
        WScript.Sleep 1000
    Loop
End Sub

'########################################
' �l�b�g�ォ��f�[�^�擾
'########################################
Function getUrl2Text(ByVal url)
    Dim http, stream
    Set http = CreateObject("Msxml2.ServerXMLHTTP")

    On Error Resume Next
    Call http.Open("GET", url, False)
    http.Send
    ' �t�H�[�}�b�^�\���h�~�p�R�����g
    If Err.Number = 0 Then
        getUrl2Text = binReadTextAll(http.responseBody)
    Else
        getUrl2Text = ""
    End If
    On Error GoTo 0
End Function

'########################################
' �o�C�i���f�[�^����e�L�X�g�ɕϊ�
'########################################
Function binReadTextAll(binData)
    Dim stream
    Dim strTempFile

    strTempFile = fs.GetSpecialFolder(2)
    strTempFile = strTempFile & "\" & UUID()

    Set stream = CreateObject("ADODB.Stream")
    stream.Open
    stream.Type = 1
    stream.Write binData
    stream.SaveToFile strTempFile
    stream.Close

    Set stream = CreateObject("ADODB.Stream")
    stream.Open
    stream.Type = 2
    stream.Charset = "UTF-8"
    stream.LoadFromFile strTempFile
    binReadTextAll = stream.ReadText( - 1)
    stream.Close
    Set stream = Nothing

    fs.DeleteFile strTempFile
End Function

'########################################
' UUID����
'########################################
Function UUID()
    Dim i, RndNum
    For i = 0 To 7
        RndNum = CLng(Rnd * "&HFFFF")
        'if i = 0 then RndNum = RndNum Xor (CLng(Date) And "&HFFFF")
        If i = 3 Then RndNum = (RndNum And "&HFFF") Or "&H4000"
        If i = 4 Then RndNum = (RndNum And "&H3FFF") Or "&H8000"
        UUID = UUID + String(4 - Len(Hex(RndNum)), "0") + LCase(Hex(RndNum))
        If i = 1 Or i = 2 Or i = 3 Or i = 4 Then UUID = UUID + "-"
    Next
End Function

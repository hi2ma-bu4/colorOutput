using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

/*
_colorOutputer Ver.1.13.6

色が付かない、不明な挙動をする場合、
開発者に問題を報告してください。

*/


public class colorOutputer
{
    private static readonly uint STD_OUTPUT_HANDLE = unchecked((uint)-11);
    private static readonly uint STD_ERROR_HANDLE = unchecked((uint)-12);
    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    private static readonly uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private static TextWriter _escapeCodeWriter;

    [DllImport("kernel32.dll")]
    private extern static IntPtr GetStdHandle(uint nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public extern static bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public extern static bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);


    public static string proName = "colorOutputer";

    public static Dictionary<string, Dictionary<string, string>> co_dict
        = new Dictionary<string, Dictionary<string, string>>{
            {"cs", new Dictionary<string, string>{
                {"name", "cscb"},
                {"extension", ".cs"},
                {"compiler", @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"},
                {"compiler_option", ""},
                {"practice", ""},
                {"practice_ext", ".exe"},
                { "err_color", W_Colors.LRed},
                { "warn_color", W_Colors.LMagenta},
                { "log_color", W_Colors.LBlue},
                { "regexp_ini", "[^/\\\r\n]*(\\.cs|warning|error)"},
                { "regexp_err_1", "(\\.cs\\(\\d+,\\d+\\): error.*?|ハンドルされていない例外): .*?\r\n"},
                { "regexp_err_1_LCyan", "(\\.cs\\(\\d+,\\d+\\): error.*?|ハンドルされていない例外): (.+)"},
                { "regexp_err_1_LCyan_sub", "3"},
                { "regexp_warn_1", "\\.cs\\(\\d+,\\d+\\): (error|warning).*?: .*\r\n"},
                { "regexp_warn_1_LCyan", "\\.cs\\(\\d+,\\d+\\): (error|warning)*?: (.+)"},
                { "regexp_warn_1_LCyan_sub", "3"},
                { "regexp_err_2", "[^/\\\r\n]*\\.cs\\(\\d+,\\d+\\): error.*?:"},
                { "regexp_err_2_Green", "[^/\\\r\n]*\\.cs"},
                { "regexp_err_2_Yellow", "\\d+,\\d+"},
                { "regexp_warn_2", "[^/\\\r\n]*\\.cs\\(\\d+,\\d+\\): (error|warning).*?:"},
                { "regexp_warn_2_Green", "[^/\\\r\n]*\\.cs"},
                { "regexp_warn_2_Yellow", "\\d+,\\d+"},
                { "regexp_warn_3", "(^|\n.*?)(error|warning) .*?: .*?\r"},
                { "regexp_warn_3_LCyan", "(error|warning) .*?: (.+)"},
                { "regexp_warn_3_LCyan_sub", "2"},
            }},
            {"vb", new Dictionary<string, string>{
                {"name", "cscb"},
                {"extension", ".vb"},
                {"compiler", @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\vbc.exe"},
                {"compiler_option", ""},
                {"practice", ""},
                {"practice_ext", ".exe"},
                { "err_color", W_Colors.LRed},
                { "warn_color", W_Colors.LMagenta},
                { "log_color", W_Colors.LBlue},
                { "regexp_ini", "[^/\\\r\n]*(\\.vb|warning|error)"},
            }},
            {"java", new Dictionary<string, string>{
                {"name", "javab"},
                {"extension", ".java"},
                {"compiler", "javac"},
                {"compiler_option", " -encoding UTF-8"},
                {"practice", "java"},
                {"practice_ext", ""},
                {"practice_check_ext", ".class"},
                {"in_encoding", "Shift_JIS"},
                {"err_color", W_Colors.LRed},
                {"log_color", W_Colors.LBlue},
                {"regexp_err_1", "(^Exception|^エラー:|\\.java:\\d+:).+(?:\r\n|$)"},
                {"regexp_err_1_LCyan", "(^Exception|エラー:) .+"},
                {"regexp_err_2", "[^/\\\r\n(]*\\.java:\\d+[:)]"},
                {"regexp_err_2_Yellow", "\\.java.*:(\\d+)"},
                {"regexp_err_2_Yellow_sub", "1"},
                {"regexp_err_2_Green", "[^/\\\r\n(]*\\.java"},
            }},
            {"python", new Dictionary<string, string>{
                {"name", "pythonb"},
                {"extension", ".py"},
                {"practice", "python3"},
                {"in_encoding", "Shift_JIS"},
                {"warn_log","【警告】現在、pythonbではinput等の出力が正常に表示されない問題が発生しております"},
                {"err_color", W_Colors.LRed},
                {"log_color", W_Colors.LBlue},
                {"regexp_err_1", "(^|\r\n).*Error: .*(\r\n|$)"},
                {"regexp_err_1_LCyan", ".*Error: .+"},
                {"regexp_err_2", "\".*\\.py\", line \\d+"},
                {"regexp_err_2_Green", "[^\"/\\\\]*\\.py"},
                {"regexp_err_2_Yellow", ", line (\\d+)"},
                {"regexp_err_2_Yellow_sub", "1"},
            }},
        };

    public static int Main(string[] args)
    {
        setConsoleEnc(Console.OutputEncoding);
        InitializeConsole();


        // 引数チェック1
        if (args.Length < 1)
        {
            WriteError("引数の数が足りません");
            Console.Error.WriteLine("> " + W_Colors.LYellow + "_" + proName + W_Colors.Reset + " <type> [<option>] <file> [<arg1> [<arg2> ...]]");
            return 1;
        }
        // 引数の処理
        string a_type = args[0];

        // 対応しているか
        if (!co_dict.ContainsKey(a_type))
        {
            WriteError("対応していない言語です");
            return 1;
        }
        Dictionary<string, string> c_dic = co_dict[a_type];

        // 引数チェック2
        if (args.Length < 2)
        {
            WriteError("引数の数が足りません");
            Console.Error.WriteLine("> " + W_Colors.LYellow + c_dic["name"] + W_Colors.Reset + " [<option>] <file> [<arg1> [<arg2> ...]]");
            Console.Error.WriteLine("使い方が分からない場合、" + W_Colors.Yellow + "--help" + W_Colors.Reset + "か" + W_Colors.Yellow + "/?" + W_Colors.Reset + " オプションを使用してください");
            return 1;
        }

        //オプション対応
        bool optLoopEnd = true;
        bool no_compile = false;
        bool no_practice = false;
        bool no_log = false;
        bool no_color = false;
        bool no_stdout = false;
        bool no_stderr = false;
        bool no_realtime = false;
        int currentArg = 1;
        while (optLoopEnd)
        {
            if (currentArg >= args.Length)
            {
                WriteError("ファイル名が指定されていません");
                return 1;
            }
            switch (args[currentArg].ToLower())
            {
                case "-":
                case "--":
                case "/":
                    // 謎の先頭文字だけの時に排除
                    currentArg++;
                    break;
                case "-h":
                case "--help":
                case "-?":
                case "/h":
                case "/?":
                    // ヘルプ
                    string helpText = W_Colors.Green + "HELP" + W_Colors.Reset + "\n\n"
                        + "使用方法\n"
                        + "  > " + W_Colors.LYellow + c_dic["name"] + W_Colors.Reset + " [<option>] <file> [<arg1> [<arg2> ...]]\n"
                        + "引数\n"
                        + "  file\t\t\t: ファイル名(ファイルパス)\n"
                        + "  arg" + W_Colors.Blue + "..." + W_Colors.Reset + "\t\t: 引数\n"
                        + "  option\n"
                        + "    ヘルプ:\n"
                        + "      " + W_Colors.LCyan + "-h, --help, -?, /H, /?" + W_Colors.Reset + "\n"
                        + "    コンパイルしない:\n"
                        + "      " + W_Colors.LCyan + "-c, --no-compile, --nocompile, /C" + W_Colors.Reset + "\n"
                        + "    プログラムを自動実行しない:\n"
                        + "      " + W_Colors.LCyan + "-d, --no-do, --nodo, /D" + W_Colors.Reset + "\n"
                        + "    システム警告を出力しない:\n"
                        + "      " + W_Colors.LCyan + "-l, --no-log, --nolog, /L" + W_Colors.Reset + "\n"
                        + "    標準出力オーバーライド停止:\n"
                        + "      " + W_Colors.LCyan + "-o, --no-stdout, --nostdout, /O" + W_Colors.Reset + "\n"
                        + "    標準エラー出力オーバーライド停止:\n"
                        + "      " + W_Colors.LCyan + "-e, --no-stderr, --nostderr, /E" + W_Colors.Reset + "\n"
                        + "    細かい色付けを停止:\n"
                        + "      " + W_Colors.LCyan + "-n, --no-color, --nocolor, /N" + W_Colors.Reset + "\n"
                        + "    リアルタイムエラー出力を停止:\n"
                        + "      " + W_Colors.LCyan + "-r, --no-realtime, --norealtime, /R" + W_Colors.Reset + "\n"
                        + "\n注意\n"
                        + "  ・" + W_Colors.Red + "標準(エラー)出力を停止すると色付けされません" + W_Colors.Reset + "\n"
                        + "  ・" + W_Colors.Red + "標準(エラー)出力オーバーライド中は出力がプログラム終了後に表示されることがあります" + W_Colors.Reset + "\n"
                        + "\n例\n"
                        + "  > " + W_Colors.LYellow + c_dic["name"] + W_Colors.Reset + " C:\\test" + c_dic["extension"] + "\n"
                        ;
                    Console.WriteLine(helpText);
                    return 0;
                case "-c":
                case "--no-compile":
                case "--nocompile":
                case "/c":
                    // コンパイルしない
                    no_compile = true;
                    currentArg++;
                    break;
                case "-d":
                case "--no-do":
                case "--nodo":
                case "/d":
                    // プログラムを自動実行しない
                    no_practice = true;
                    currentArg++;
                    break;
                case "-l":
                case "--no-log":
                case "--nolog":
                case "/l":
                    // システム警告を出力しない
                    no_log = true;
                    currentArg++;
                    break;
                case "-o":
                case "--no-stdout":
                case "--nostdout":
                case "/o":
                    // 標準出力オーバーライド停止
                    no_stdout = true;
                    currentArg++;
                    break;
                case "-e":
                case "--no-stderr":
                case "--nostderr":
                case "/e":
                    // 標準エラー出力オーバーライド停止
                    no_stderr = true;
                    currentArg++;
                    break;
                case "-n":
                case "--no-color":
                case "--nocolor":
                case "/n":
                    // 細かい色付けを停止
                    no_color = true;
                    currentArg++;
                    break;
                case "-r":
                case "--no-realtime":
                case "--norealtime":
                case "/r":
                    // リアルタイムエラー出力を停止
                    no_realtime = true;
                    currentArg++;
                    break;
                default:
                    optLoopEnd = false;
                    break;
            }
        }
        string a_file = Path.ChangeExtension(args[currentArg], null);
        string doArgs = "";
        for (int i = currentArg + 1, li = args.Length; i < li; i++)
        {
            doArgs += " " + args[i];
        }


        // ファイル形式を使いやすく
        if (File.Exists(a_file + c_dic["extension"]))
        {
            // なにもしない
        }
        else if (c_dic.ContainsKey("compiler_check_ext") && File.Exists(a_file + c_dic["compiler_check_ext"]))
        {
            //なにもしない
        }
        else if (c_dic.ContainsKey("practice_check_ext") && File.Exists(a_file + c_dic["practice_check_ext"]))
        {
            //なにもしない
        }
        else if (Directory.Exists(a_file))
        {
            Match m = Regex.Match(a_file, @"[^/\\]*$");
            if (m.Value != string.Empty)
            {
                a_file += @"\" + m.Value;
                if (File.Exists(a_file + c_dic["extension"]))
                {
                    //なにもしない
                }
                else if (c_dic.ContainsKey("compiler_check_ext") && File.Exists(a_file + c_dic["compiler_check_ext"]))
                {
                    //なにもしない
                }
                else if (c_dic.ContainsKey("practice_check_ext") && File.Exists(a_file + c_dic["practice_check_ext"]))
                {
                    //なにもしない
                }
                else
                {
                    WriteError("ファイルパス「{" + a_file + "」が存在しません");
                    return 1;
                }
            }
        }
        else
        {
            WriteError("ファイル「" + a_file + "」が存在しません");
            return 1;
        }
        string currentDir = Path.GetDirectoryName(Path.GetFullPath(a_file));
        Directory.SetCurrentDirectory(currentDir);
        a_file = Path.GetFileName(a_file);

        Encoding inEnc = Console.OutputEncoding;
        if (c_dic.ContainsKey("in_encoding"))
        {
            inEnc = Encoding.GetEncoding(c_dic["in_encoding"]);
        }

        //警告文表示
        if (c_dic.ContainsKey("warn_log") && !no_log)
        {
            Console.Error.WriteLine(c_dic["log_color"] + c_dic["warn_log"] + W_Colors.Reset);
        }


        // コンパイル
        if (c_dic.ContainsKey("compiler") && !no_compile)
        {
            string p_file = a_file;
            if (c_dic.ContainsKey("compiler_ext"))
            {
                p_file += c_dic["compiler_ext"];
            }
            else
            {
                p_file += c_dic["extension"];
            }

            string c_file = p_file;
            if (c_dic.ContainsKey("compiler_check_ext"))
            {
                c_file += c_dic["compiler_check_ext"];
            }

            string[] outStr;
            if (File.Exists(c_file))
            {
                outStr = doProcess(a_type, c_dic["compiler"], p_file, c_dic["compiler_option"], currentDir, !no_stdout, !no_stderr, no_realtime, inEnc);
            }
            else
            {
                outStr = new string[] { "sys", "ソースファイル「" + c_file + "」が存在しません" };
            }

            string colorLog = "";
            switch (outStr[0])
            {
                case "none":
                    // なにもしない
                    break;
                case "sys":
                    WriteError(outStr[1]);
                    return 1;
                case "out":
                    if (no_color)
                    {
                        colorLog = c_dic["warn_color"] + outStr[1] + W_Colors.Reset;
                    }
                    else
                    {
                        colorLog = drawColor(a_type, "warn", outStr[1]);
                    }
                    Console.Error.WriteLine(colorLog);
                    break;
                case "err":
                    if (no_color)
                    {
                        colorLog = c_dic["err_color"] + outStr[1] + W_Colors.Reset;
                    }
                    else
                    {
                        colorLog = drawColor(a_type, "err", outStr[1]);
                    }
                    Console.Error.WriteLine(colorLog);
                    return 1;
            }
        }
        // 実行
        if (c_dic.ContainsKey("practice") && !no_practice)
        {
            string p_file = a_file;
            if (c_dic.ContainsKey("practice_ext"))
            {
                p_file += c_dic["practice_ext"];
            }
            else
            {
                p_file += c_dic["extension"];
            }

            string c_file = p_file;
            if (c_dic.ContainsKey("practice_check_ext"))
            {
                c_file += c_dic["practice_check_ext"];
            }

            string[] outStr;
            if (File.Exists(c_file))
            {
                outStr = doProcess(a_type, c_dic["practice"], p_file, doArgs, currentDir, false, !no_stderr, no_realtime, inEnc);
            }
            else
            {
                outStr = new string[] { "sys", "実行ファイル「" + c_file + "」が存在しません" };
            }

            string colorLog = "";
            switch (outStr[0])
            {
                case "none":
                    // なにもしない
                    break;
                case "sys":
                    WriteError(outStr[1]);
                    return 1;
                case "err":
                    if (no_color)
                    {
                        colorLog = c_dic["err_color"] + outStr[1] + W_Colors.Reset;
                    }
                    else
                    {
                        colorLog = drawColor(a_type, "err", outStr[1]);
                    }
                    Console.Error.WriteLine(colorLog);
                    return 1;
            }
        }

        return 0;
    }

    /*
    * コマンドを実行
    */
    public static string[] doProcess(string fileType, string exeF, string file, string args, string currentDir, bool isGetNormalOut, bool isGetErrOut, bool no_realtime, Encoding enc)
    {
        string maF = exeF;
        string arg = args;
        if (maF == string.Empty)
        {
            maF = file;
        }
        else
        {
            arg = file + args;
        }

        if (!Directory.Exists(currentDir))
        {
            return new string[] { "sys", "ディレクトリ「{" + currentDir + "」が存在しません" };
        }

        bool isWarn = false;
        bool isError = false;

        ProcessStartInfo startInfo = new ProcessStartInfo(maF, arg)
        {
            UseShellExecute = false,
            WorkingDirectory = currentDir,
        };
        if (isGetNormalOut)
        {
            startInfo.RedirectStandardOutput = true;
            //startInfo.StandardOutputEncoding = enc;
        }
        if (isGetErrOut)
        {
            startInfo.RedirectStandardError = true;
            startInfo.StandardErrorEncoding = enc;
        }

        using (Process process = new Process())
        {
            bool isTimedOut = false;

            StringBuilder stdout = new StringBuilder();
            StringBuilder stderr = new StringBuilder();

            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;

            if (isGetNormalOut)
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (no_realtime)
                        {
                            stdout.AppendLine(e.Data);
                        }
                        else
                        {
                            string s = drawColor(fileType, "warn", e.Data);
                            Console.Error.WriteLine(s);
                            isWarn = true;
                        }
                    }
                };

            }
            if (isGetErrOut)
            {
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (no_realtime)
                        {
                            stderr.AppendLine(e.Data);
                        }
                        else
                        {
                            string s = drawColor(fileType, "err", e.Data);
                            Console.Error.WriteLine(s);
                            isError = true;
                        }
                    }
                };
            }

            process.Start();

            if (isGetNormalOut)
            {
                process.BeginOutputReadLine();
            }
            if (isGetErrOut)
            {
                process.BeginErrorReadLine();
            }

            if (!process.WaitForExit((int)TimeSpan.FromMinutes(30).TotalMilliseconds))
            {
                isTimedOut = true;
                process.Kill();
            }

            if (isGetNormalOut)
            {
                process.CancelOutputRead();
            }
            if (isGetErrOut)
            {
                process.CancelErrorRead();
            }

            if (isTimedOut)
            {
                return new string[] { "sys", "タイムアウトしました" };
            }
            if (isGetNormalOut)
            {
                string outStr = stdout.ToString();
                if (outStr != "")
                {
                    return new string[] { "out", outStr };
                }
                else if (isWarn)
                {
                    return new string[] { "out", "" };
                }
            }
            if (isGetErrOut)
            {
                string errStr = stderr.ToString();
                if (errStr != "")
                {
                    return new string[] { "err", errStr };
                }
                else if (isError)
                {
                    return new string[] { "err", "" };
                }
            }
        }
        return new string[] { "none", "" };
    }

    public static string drawColor(string fileType, string type, string str)
    {
        Dictionary<string, string> c_dic = co_dict[fileType];

        // 基本色設定
        string baseColor = "";
        switch (type)
        {
            case "err":
                baseColor = c_dic["err_color"];
                break;
            case "warn":
                baseColor = c_dic["warn_color"];
                break;
            default:
                baseColor = W_Colors.Reset;
                break;
        }

        bool isIni = false;
        if (c_dic.ContainsKey("regexp_ini"))
        {
            Match m = Regex.Match(str, c_dic["regexp_ini"]);
            if (m.Value != string.Empty)
            {
                str = str.Replace(m.Value, baseColor + m.Value);
                isIni = true;
            }
        }

        // 正規表現でこねるタイム
        foreach (string key in c_dic.Keys)
        {
            string[] b_spKey = key.Split('_');
            if (b_spKey.Length == 3
                && b_spKey[0] == "regexp"
                && b_spKey[1] == type)
            {
                MatchCollection mc = Regex.Matches(str, c_dic[key]);

                foreach (Match mat in mc)
                {
                    string tmpStr = mat.Value;
                    if (tmpStr == string.Empty)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<string, string> item in c_dic)
                    {
                        string[] i_spKey = item.Key.Split('_');
                        if (i_spKey.Length == 4
                            && i_spKey[0] == "regexp"
                            && i_spKey[1] == type
                            && i_spKey[2] == b_spKey[2])
                        {
                            string r_color = W_Colors.GetColor(i_spKey[3]);

                            Match m = Regex.Match(tmpStr, item.Value);
                            if (m.Value != string.Empty)
                            {
                                string repStr = m.Value;
                                if (c_dic.ContainsKey(item.Key + "_sub"))
                                {
                                    repStr = m.Groups[Int32.Parse(c_dic[item.Key + "_sub"])].Value;
                                }
                                if (repStr != string.Empty)
                                {
                                    tmpStr = tmpStr.Replace(repStr, r_color + repStr + baseColor);
                                }
                            }
                        }
                    }
                    str = str.Replace(mat.Value, tmpStr);
                }
            }
        }
        if (!isIni)
        {
            str = baseColor + str;
        }
        return str + W_Colors.Reset;
    }

    public static void WriteError(string str)
    {
        Console.Error.WriteLine(W_Colors.LRed + str + W_Colors.Reset);
    }

    //shift_jisをutf-8に変換
    public static string ConvertEncoding(Encoding srcEnc, Encoding destEnc, string src)
    {
        byte[] src_temp = srcEnc.GetBytes(src);
        byte[] dest_temp = System.Text.Encoding.Convert(srcEnc, destEnc, src_temp);
        string ret = destEnc.GetString(dest_temp);
        return ret;
    }


    // コンソールの初期化
    private static void setConsoleEnc(Encoding enc)
    {
        Console.OutputEncoding = enc;
        Console.InputEncoding = enc;
    }

    // コンソールの初期化
    private static void InitializeConsole()
    {
        // Windows の場合
        // 現在使用しているコンソールがエスケープコードを解釈しない場合、エスケープコードを解釈するように設定する。


        // コンソールのハンドルを取得する
        IntPtr consoleOutputHandle = INVALID_HANDLE_VALUE;
        if (!Console.IsOutputRedirected)
        {
            // 標準出力がリダイレクトされていない場合は、標準出力がコンソールに紐づけられているとみなす
            consoleOutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            _escapeCodeWriter = Console.Out;

        }
        else if (!Console.IsErrorRedirected)
        {
            // 標準エラー出力がリダイレクトされていない場合は、標準エラー出力がコンソールに紐づけられているとみなす
            consoleOutputHandle = GetStdHandle(STD_ERROR_HANDLE);
            _escapeCodeWriter = Console.Error;
        }
        else
        {
            // その他の場合はコンソールに紐づけられているハンドル/ストリームはない。
            consoleOutputHandle = INVALID_HANDLE_VALUE;
            _escapeCodeWriter = null;
        }

        if (consoleOutputHandle != INVALID_HANDLE_VALUE)
        {
            // 標準出力と標準エラー出力の少なくともどちらかがコンソールに紐づけられている場合

            // 現在のコンソールモード(フラグ)を取得する
            uint mode;
            if (!GetConsoleMode(consoleOutputHandle, out mode))
                throw new Exception("Failed to get console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

            // コンソールモードに ENABLE_VIRTUAL_TERMINAL_PROCESSING がセットされていないのならセットする
            if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == 0)
            {
                if (!SetConsoleMode(consoleOutputHandle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING))
                    throw new Exception("Failed to set console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            }
        }
    }


}

/*
* 色クラス
*/
public class W_Colors
{
    public const string Reset = "\x1b[0m";
    public const string Red = "\x1b[31m";
    public const string Green = "\x1b[32m";
    public const string Yellow = "\x1b[33m";
    public const string Blue = "\x1b[34m";
    public const string Magenta = "\x1b[35m";
    public const string Cyan = "\x1b[36m";
    public const string White = "\x1b[37m";
    public const string LRed = "\x1b[91m";
    public const string LGreen = "\x1b[92m";
    public const string LYellow = "\x1b[93m";
    public const string LBlue = "\x1b[94m";
    public const string LMagenta = "\x1b[95m";
    public const string LCyan = "\x1b[96m";
    public const string LWhite = "\x1b[97m";

    public static string GetColor(string color)
    {
        switch (color)
        {
            case "Red":
                return Red;
            case "Green":
                return Green;
            case "Yellow":
                return Yellow;
            case "Blue":
                return Blue;
            case "Magenta":
                return Magenta;
            case "Cyan":
                return Cyan;
            case "White":
                return White;
            case "LRed":
                return LRed;
            case "LGreen":
                return LGreen;
            case "LYellow":
                return LYellow;
            case "LBlue":
                return LBlue;
            case "LMagenta":
                return LMagenta;
            case "LCyan":
                return LCyan;
            case "LWhite":
                return LWhite;
            default:
                return Reset;
        }
    }
}


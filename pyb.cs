using System;
using System.Diagnostics;


/*
pyb Ver.1.1.1

同階層にある「_colorOutputer.exe」を利用しています。

*/

public class pyb
{
    public static int Main(string[] args)
    {
        string maF = "_colorOutputer.exe";
        string arg = "python ";
        if (args.Length > 0)
        {
            arg += args[0];
            if (args.Length > 1)
            {
                arg += " " + string.Join(" ", args, 1, args.Length - 1);
            }
        }

        ProcessStartInfo startInfo = new ProcessStartInfo(maF, arg)
        {
            UseShellExecute = false,
        };

        using (Process process = new Process())
        {
            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();
        }
        return 0;
    }
}

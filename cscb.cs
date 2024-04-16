using System;
using System.Diagnostics;


/*
cscb Ver.1.2.1

同階層にある「_colorOutputer.exe」を利用しています。

*/

public class cscb
{
    public static int Main(string[] args)
    {
        string maF = "_colorOutputer.exe";
        string arg = "cs ";
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

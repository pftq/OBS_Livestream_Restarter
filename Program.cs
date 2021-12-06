using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace OBS_Livestream_Restarter
{
    class Program
    {

        static string OBSInstallationFolder = @"C:\Program Files\obs-studio\bin\64bit";
        static string OBSFileName = "obs64.exe";

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] int nCmdShow);

        static void Main(string[] args)
        {
            WriteLine("OBS Stream Restarter by pftq ~ www.pftq.com ~ Oct. 2015");
            Thread.Sleep(1000);

            // from https://stackoverflow.com/questions/44675085/minimize-console-at-app-startup-c-sharp
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            MinimizeWindow(handle);

            DateTime lastRestart = DateTime.Now;
            while (true)
            {
                Stack<string> nextDir = new Stack<string>();
                nextDir.Push(OBSInstallationFolder);
                while(nextDir.Any())
                {
                    string dir = nextDir.Pop();

                    WriteLine("Checking folder " + dir, false);
                    foreach(string f in Directory.GetFiles(dir, OBSFileName))
                    {
                        if (f == Process.GetCurrentProcess().MainModule.FileName) continue;
                        string file = Path.GetFileName(f);
                        string process = Path.GetFileNameWithoutExtension(f);
                        WriteLine(" - Checking file " + file, false);
                        try
                        {
                            bool processExists = Process.GetProcesses().Where(p => p.ProcessName == process && p.MainModule.FileName == f).Any();
                            if (!Process.GetProcesses().Where(p => p.ProcessName==process && p.MainModule.FileName ==f).Any())
                            {
                                string arg = " --startstreaming --minimize-to-tray";
                                WriteLine("Restarting: " + f + arg);
                                ProcessStartInfo proc = new ProcessStartInfo();

                                proc.WorkingDirectory = dir;
                                proc.FileName = f;
                                proc.Arguments = arg;
                                Process.Start(proc);
                                lastRestart = DateTime.Now;
                                Thread.Sleep(2000);
                                /*if (Process.GetProcesses().Where(p => p.ProcessName == process && p.MainModule.FileName == f).Any())
                                {
                                    MinimizeWindow(Process.GetProcesses().Where(p => p.ProcessName == process && p.MainModule.FileName == f).First().MainWindowHandle);
                                }*/
                            }
                            else if (DateTime.Now >= lastRestart.AddHours(11).AddMinutes(59))
                            {
                                Process.GetProcesses().Where(p => p.ProcessName == process && p.MainModule.FileName == f).ToList().ForEach(p => p.Kill());
                                WriteLine("Closing: " + f );
                            }
                        }
                        catch (Exception e)
                        {
                            WriteLine("Error for "+f+":\n" + e);
                        }
                    }
                }
                Thread.Sleep(10000);
            }
        }
        static void WriteLine(string s, bool log=true)
        {
            s = DateTime.Now + ": " + s;
            Console.WriteLine(s);
            if(log) File.AppendAllLines("TaskKeepAlive_log.txt", new string[]{s});
        }

        static void MinimizeWindow(IntPtr window)
        {
            ShowWindow(window, 6);
        }
    }
}

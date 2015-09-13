using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Dino.Tools.Reddit.CleanModQueue
{
    static class Program
    {
        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole( int dwProcessId );

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AttachConsole(-1);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

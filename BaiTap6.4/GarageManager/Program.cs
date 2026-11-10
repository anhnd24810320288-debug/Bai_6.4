using System;
using System.Windows.Forms;
using GarageManager.GUI;

namespace GarageManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }
    }
}

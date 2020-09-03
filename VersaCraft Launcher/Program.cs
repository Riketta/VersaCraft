using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaCraft_Launcher
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                App.Main();
            }
            catch (Exception ex)
            {
                FatalError.Exception(ex, true);
            }
        }
    }
}

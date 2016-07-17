using System;
using Microsoft.Owin.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoWebApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // On Windows you must run as Admin for this to work, otherwise use http://localhost:5000
                string baseUrl = "http://*:5000";
                using (WebApp.Start<Startup>(baseUrl))
                {
                    ConsoleKeyInfo c = new ConsoleKeyInfo();
                    do
                    {
                        Console.WriteLine("Press x to quit.");
                        c = Console.ReadKey();
                    }
                    while (c.KeyChar != 'x' && c.KeyChar != 'X');
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running server:" + ex.Message);
            }
            finally
            {
                Console.WriteLine("Exit");
            }
        }
    }
}

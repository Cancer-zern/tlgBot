using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tlgBotConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                TelegraBotHelper hlp = new TelegraBotHelper(token: "1669924939:AAF27M9EcP2gDwbeFCqihmfnPdi0_N6WivU"); //testsrg_bot
                hlp.GetUpdates();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}

using System;
using System.IO;

namespace DGMCheckUp
{
    class Program
    {
        static void Main(string[] args)
        {

            string bash = Environment.GetEnvironmentVariable("BASH");

            if (bash == null)
            {
                Console.WriteLine("Bash not found! Set environment variable 'BASH' first");
                return;
            }

            if (args[0] == "h" || args[0] == "help")
            {
                Console.WriteLine("DGMCheckUp v1.0");
                Console.WriteLine("Available Commands:");
                Console.WriteLine("1. Create DGM-File" + "\r\n" + "Directorypath (of raw dgm files) Outputpath");
                Console.WriteLine("2. Increase gridsize of DGM-File" + "\r\n" + "Filepath targetRasterSize Outputpath");
                Console.WriteLine("3. Cutout specific area of DGM-File" + "\r\n" + "Filepath x1 y1 x2 y2 Outputpath");
                Console.WriteLine("4. Analyze DGM-File" + "\r\n" + "Filepath");
            }

            Dgm dgm = new Dgm();

            //Option 1
            if (args.Length == 2 && Directory.Exists(args[0]))
            {
                dgm.prepareDGM(args[0], args[1]);
                return;
            }
            //Option 2
            if (args.Length == 3 && File.Exists(args[0]) && args[1] != "")
            {
                dgm.convertGrid(args[0], args[1], args[2]);
                return;
            }
            //Option 3  
            if (args.Length == 6 && File.Exists(args[0]) && args[1] != "" && args[2] != "" && args[3] != "" && args[4] != "")
            {
                dgm.cutoutArea(args[0], Int32.Parse(args[1]), Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5]);
                return;
            }

            //Option 4 TODO
            if (args.Length == 1 && File.Exists(args[0]))
            {
                //analyze
                return;
            }
        }
    }
}

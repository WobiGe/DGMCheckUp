using System;
using System.IO;

namespace DGMCheckUp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "h" || args[0] == "help")
            {
                Console.WriteLine("DGMCheckUp v1.0");
                Console.WriteLine("Available Commands:");
                Console.WriteLine("1. Reduce gridsize of DGM-File" + "\r\n" + "Filepath targetRasterSize Outputpath");
                Console.WriteLine("2. Cutout specific area of DGM-File DGM-File" + "\r\n" + "Filepath x1 y1 x2 y2 Outputpath");
                Console.WriteLine("3. Analyze DGM-File" + "\r\n" + "Filepath");
            }
            
            Dgm dgm = new Dgm();

            if (args.Length == 3 && File.Exists(args[0]) && args[1] != "" &&  Directory.Exists(args[2])){    
                dgm.convertGrid(args[0], args[1], args[2]);
                return;
            }

            if (args.Length == 6 && File.Exists(args[0]) && args[1] != "" && args[2] != "" && args[3] != "" && args[4] != "" && Directory.Exists(args[5])){
                dgm.cutoutArea(args[0],Int32.Parse(args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]),Int32.Parse(args[4]), args[5]);
            }
        }
    }
}

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
                Console.WriteLine("1. Create DGM-File" + "\r\n" + "Directorypath (of raw dgm files)");
                Console.WriteLine("2. Increase gridsize of DGM-File" + "\r\n" + "Filepath targetRasterSize Outputpath");
                Console.WriteLine("3. Cutout specific area of DGM-File" + "\r\n" + "Filepath x1 y1 x2 y2 Outputpath");
                Console.WriteLine("4. Analyze DGM-File" + "\r\n" + "Filepath");
                Console.WriteLine("5. Remove duplicates of DGM-File" + "\r\n" + "Filepath r");
            }
            
            Dgm dgm = new Dgm();

            //Option 1
            if(args.Length == 1 && Directory.Exists(args[0])){
                dgm.cleanupDGM(args[0]);
            }
            //Option 2
            if (args.Length == 3 && File.Exists(args[0]) && args[1] != "" &&  Directory.Exists(args[2])){    
                dgm.convertGrid(args[0], args[1], args[2]);
                return;
            }
            //Option 3  
            if (args.Length == 6 && File.Exists(args[0]) && args[1] != "" && args[2] != "" && args[3] != "" && args[4] != "" && Directory.Exists(args[5])){
                dgm.cutoutArea(args[0],Int32.Parse(args[1]),Int32.Parse(args[2]),Int32.Parse(args[3]),Int32.Parse(args[4]), args[5]);
            }

            //Option 4 TODO

            //Option 5
            if (args.Length == 2 && File.Exists(args[0]) && args[1] == "r")
            {
                dgm.checkForDuplicates(args[0]);
            }
        }
    }
}

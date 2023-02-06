using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public class Dgm
{

    //Refactoring:
    /*
    -Merge all files with linux cat
    -Sort cat.xyz y first, x second with linux sort
    -Remove Duplicates if there are any
    -While removing duplicates, collect data bounds, number of coords, raster size, how many duplicates there are...
    */



    public string DgmFile { get; set; }
    public string OutputPath { get; set; }
    public string RasterSize { get; set; }

    public Dgm()
    {
    }

    public void convertGrid(string dgmFile, string targetRasterSize, string outputPath)
    {

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        const Int32 BufferSize = 4096;
        using (var fileStream = File.OpenRead(dgmFile))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        using (var sw = new StreamWriter(outputPath + @"\reduced_" + targetRasterSize + "m" + ".xyz"))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] coords = line.Split(" ");
                var delimeter = 0;
                if (coords[0].Split(".").Length > 1)
                {
                    delimeter = coords[0].Split(".")[1].Length;
                }

                if (Double.Parse(coords[0]) / Math.Pow(10, delimeter) % Int32.Parse(targetRasterSize) == 0 && Double.Parse(coords[1]) / Math.Pow(10, delimeter) % Int32.Parse(targetRasterSize) == 0)
                {
                    sw.WriteLine(line);
                }
            }
        }
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }

    public void cutoutArea(string dgmFile, int x1, int x2, int y1, int y2, string outputPath)
    {

        // go from lower left to upper right
        /*
        |---------x2y2|
        |             |
        |     Map     |
        |             |
        |x1y1---------|
        */
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        const Int32 BufferSize = 4096;
        using (var fileStream = File.OpenRead(dgmFile))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        using (var sw = new StreamWriter(outputPath + @"\cutout" + ".xyz"))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] coords = line.Split(" ");

                if (Double.Parse(coords[0]) / Math.Pow(10, 2) >= x1 && Double.Parse(coords[0]) / Math.Pow(10, 2) <= x2)
                {
                    if (Double.Parse(coords[1]) / Math.Pow(10, 2) >= y1 && Double.Parse(coords[1]) / Math.Pow(10, 2) <= y2)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }

    public void prepareDGM(string dgmFilesPath, string outputPath)
    {
        //1. Step is to merge raw dgm-files
        //2. Step is to sort the dgm file. Sort second column (y coord) as numeric, then first column (x coord)
        //3. Step is to check for duplicate x,y coords and remove them.

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!Directory.Exists(outputPath)){
            Directory.CreateDirectory(outputPath);
        }

        string dgmFileName = outputPath + "\\dgm_clean.xyz";
        //replace backslash with forward slash for bash operations
        string filePathLnx =  dgmFilesPath.Replace("\\","/");
        string outPathLnx = outputPath.Replace("\\", "/");

        //merge files with cat
        Console.WriteLine("Merging xyz-files...");
        this.RunCommandWithBash("cat " + filePathLnx + "/*.xyz" + " > " + outPathLnx + "/cat.xyz");

        //sort coordinates. Y first, X second
        Console.WriteLine("Sorting coordinates...");
        this.RunCommandWithBash("sort -k2 -n -k1 " + outPathLnx + "/cat.xyz -o " + outPathLnx + "/sort.xyz");

        File.Delete(outputPath + "\\cat.xyz");

        //remove duplicates
        Console.WriteLine("Removing duplicates...");
        this.checkForDuplicates(outputPath + "\\sort.xyz", dgmFileName);

        File.Delete(outputPath + "\\sort.xyz");
        
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }

    public void checkForDuplicates(string dgmFile, string dgmFileName){
        string preY = "";
        string preX = "";

        const Int32 BufferSize = 4096;
        using (var fileStream = File.OpenRead(dgmFile))
        using (var streamReader = new StreamReader(fileStream,Encoding.UTF8, true, BufferSize))
        using (var sw = new StreamWriter(dgmFileName))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null){
                string[] coords = line.Split(" ");

                if(preX + preY != coords[0] + coords[1]){
                    sw.WriteLine(line);
                }
                preX = coords[0];
                preY = coords[1];
            }
        } 
    }
    public string RunCommandWithBash(string command)
    {
        //Set beforehand the system variable for bash.exe
        string bashFile = Environment.GetEnvironmentVariable("BASH");

        var psi = new ProcessStartInfo();
        psi.FileName =  bashFile;
        psi.Arguments = $"-c \"{command}\"";
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = false;

        using var process = Process.Start(psi);

        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();

        return output;
    }
}
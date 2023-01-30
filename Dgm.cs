using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public string rasterSize { get; set; }
    public string targetRasterSize { get; set; }

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

    public void cleanupDGM(string dgmFilesPath)
    {

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        //1. Step is to merge raw dgm-files
        //2. Step is to sort the dgm file. Sort second column (y coord) as numeric, then first column (x coord)
        //3. Step is to check for duplicate x,y coords and remove them.
        string mergedFile = @"G:\Dev\TestData\dgm1_kreis_ddorf_clean\clean.xyz";
        this.RunCommandWithBash("sort -k2 -n -k1 'G:\\Dev\\TestData\\dgm1_kreis_ddorf_clean\\clean.xyz' -o 'G:\\Dev\\TestData\\dgm1_kreis_ddorf_clean\\sort.xyz'");
        this.checkForDuplicates(mergedFile);
        
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }

    private void checkForDuplicates(string mergedFile){
        string preY = "";
        string preX = "";

        const Int32 BufferSize = 4096;
        using (var fileStream = File.OpenRead(mergedFile))
        using (var streamReader = new StreamReader(fileStream,Encoding.UTF8, true, BufferSize))
        using (var sw = new StreamWriter(@"G:\Dev\TestData\split\final.xyz"))
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
        //use for cat and sort
        var psi = new ProcessStartInfo();
        psi.FileName =  @"G:\Programme\Git\bin\bash.exe";
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
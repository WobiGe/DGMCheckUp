using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public class Dgm{
    

    public string DgmFile {get; set;}
    public string OutputPath {get; set;}
    public string rasterSize {get; set;}
    public string targetRasterSize {get; set;}

    public Dgm(){
    }

    public void convertGrid(string dgmFile, string targetRasterSize, string outputPath){
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        const Int32 BufferSize = 4096;
        using (var fileStream = File.OpenRead(dgmFile))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        using (var sw = new StreamWriter(outputPath + @"\reduced_" + targetRasterSize + "m" + ".xyz"))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null){
                string[] coords = line.Split(" ");
                var delimeter = 0;
                if(coords[0].Split(".").Length > 1){
                    delimeter = coords[0].Split(".")[1].Length;
                }

                if(Double.Parse(coords[0]) / Math.Pow(10, delimeter) % Int32.Parse(targetRasterSize) == 0 && Double.Parse(coords[1]) / Math.Pow(10, delimeter) % Int32.Parse(targetRasterSize) == 0 ){
                    sw.WriteLine(line);
                }
            }
        }
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }

    public void cutoutArea(string dgmFile, int x1, int x2, int y1, int y2, string outputPath){
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
        using (var sw = new StreamWriter(outputPath + @"\cutout"+ ".xyz"))
        {
            string line;
             while ((line = streamReader.ReadLine()) != null){
                string[] coords = line.Split(" ");

                if(Double.Parse(coords[0]) / Math.Pow(10, 2) >= x1 && Double.Parse(coords[0]) / Math.Pow(10, 2) <= x2){
                    if(Double.Parse(coords[1]) / Math.Pow(10, 2) >= y1 && Double.Parse(coords[1]) / Math.Pow(10, 2) <= y2){
                        sw.WriteLine(line);
                    }
                }
             }
        }
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }
}
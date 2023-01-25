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
    -Dont split dgm file into smaller parts (Removing duplicates is not possible this way)
    -Keep merge method, but merge raw files first, then sort, then remove duplicates
    -track dgm file info (Bounds, filesize, number of coords, raster size, info if there were duplicate coords etc.)
    -Remove split method
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

    public void cleanupDGM(string dgmFile, string outputPath)
    {

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        //1. Step is to split large dgm files into smaller chunks.
        //2. Step is to sort the dgm file(s). Sort second column (y coord) as numeric, then first column (x coord)
        //3. Step is to check for duplicate x,y coords and remove them.
        //this.splitDgmFile(dgmFile, outputPath);
        //this.sortXYZ(outputPath);
        string cleanFile = this.mergeFiles(outputPath);

        this.checkForDuplicates(cleanFile);
        
        stopwatch.Stop();
        var elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString("hh\\:mm\\:ss");
        Console.WriteLine("Elapsed time: " + elapsedTime);
    }

    private void checkForDuplicates(string cleanFile){
        string preY = "";
        string preX = "";

        const Int32 BufferSize = 4096;
        using (var fileStream = File.OpenRead(cleanFile))
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

    private string mergeFiles(string outputPath)
    {
        string cleanFile = outputPath + @"\clean" + ".xyz";

        if( File.Exists(cleanFile)){
            File.Delete(cleanFile);
        }

        const Int32 BufferSize = 4096;
        foreach (var file in Directory.GetFiles(outputPath))
        {
            using (var fileStream = File.OpenRead(file))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            using (var sw = new StreamWriter(cleanFile, append: true))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    sw.WriteLine(line);
                }
                fileStream.Flush();
                fileStream.Close();
                sw.Flush();
                sw.Close();
            }
        }

        return cleanFile;
    }
    //temporary solution. Bit too slow for >1gb files
    private void sortXYZ(string outputPath){
        foreach (var file in Directory.GetFiles(outputPath))
        {
            Console.WriteLine("Sorting coordinates in file: " + Path.GetFileName(file));
            var coords = File.ReadAllLines(file);
            List<string> coordsList = new List<string>(coords);
            coordsList = coordsList.OrderBy(coord => decimal.Parse(coord.Split(" ")[1]))
                      .ThenBy(coord => decimal.Parse(coord.Split(" ")[0]))
                      .ToList();
                      
            File.WriteAllLines(outputPath + "\\" + Path.GetFileName(file) , coordsList);
        }
    }
    
    private void splitDgmFile(string dgmFile, string outputPath)
    {

        Console.WriteLine("Splitting file into chunks...");

        long fileLength = new FileInfo(dgmFile).Length;

        if (fileLength > 1000000000)
        {
            //test 500000
            const long maxLines = 10000000;
            int outFileNumber = 1;
            const Int32 BufferSize = 4096;

            using (var fileStream = File.OpenRead(dgmFile))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                long lineCount = 0;
                //first file
                var sw = File.CreateText(outputPath + Path.GetFileNameWithoutExtension(dgmFile) + "_temp" + outFileNumber + ".xyz");
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (lineCount >= maxLines)
                    {
                        outFileNumber++;
                        sw.Flush();
                        sw.Close();
                        sw = File.CreateText(outputPath + Path.GetFileNameWithoutExtension(dgmFile) + "_temp" + outFileNumber + ".xyz");
                        lineCount = 0;
                    }
                    sw.WriteLine(line);
                    lineCount++;
                }
                sw.Flush();
                sw.Close();
            }
            Console.WriteLine("Created " + outFileNumber + " temporary files...");
        }
        else
        {
            //Copy File as temporary file
            File.Copy(dgmFile,outputPath + "temp.xyz");
            Console.WriteLine("Filesize too small. Skipping splitting into chunks..");
        }
    }
}
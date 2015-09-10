using System;
using System.Drawing;
using System.IO;

namespace ASCConverter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No arguments passed, try calling with \"--help\" for usage information.");
                return;
            }

            string inputFileName;
            string outputFileName;
            if (args.Length == 1)
            {
                if (args[0] == "--help")
                {
                    Console.WriteLine("This program will take a .asc file and attempt to turn it into a raster image.");
                    Console.WriteLine();
                    Console.WriteLine("Run with ASCConverter.exe input [output]");
                    Console.WriteLine();
                    Console.WriteLine("where input is the path to the .asc file to convert,");
                    Console.WriteLine("and output is an optional parameter of where to put the final image.");
                    Console.WriteLine("Not specifiying an output file will result in the final image being");
                    Console.WriteLine("placed in the same location as the input file, but with a .bmp extension.");
                    return;
                }
                else
                {
                    inputFileName = args[0];
                    if (!inputFileName.EndsWith(".asc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("The input file does not end in a .asc extension.");
                    }

                    if (!File.Exists(inputFileName))
                    {
                        Console.WriteLine("The input file specified does not exist.");
                        return;
                    }

                    outputFileName = inputFileName.Substring(0, inputFileName.Length - 4) + ".bmp";
                }
            }
            else if (args.Length == 2)
            {
                inputFileName = args[0];
                if (!inputFileName.EndsWith(".asc", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("The input file does not end in a .asc extension.");
                }

                if (!File.Exists(inputFileName))
                {
                    Console.WriteLine("The input file specified does not exist.");
                    return;
                }

                outputFileName = args[1];
                if (!outputFileName.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("The input file does not end in a .bmp extension.");
                }
            }
            else
            {
                Console.WriteLine("Too many arguments passed, try calling with \"--help\" for usage information.");
                return;
            }

            try
            {
                CreateImage(inputFileName, outputFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has ocurred, probably due to an invalid input file or a bug in the code in being able to read a valid input file.");
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void CreateImage(string inputFileName, string outputFileName)
        {
            int rows = 0;
            int cols = 0;
            var stream = File.OpenText(inputFileName);
            for (int i = 0; i < 6; ++i)
            {
                string line = stream.ReadLine();
                if (line.StartsWith("ncols"))
                {
                    cols = int.Parse(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                }
                else if (line.StartsWith("nrows"))
                {
                    rows = int.Parse(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                }
            }

            Console.WriteLine($"Found {cols} columns and {rows} rows.");
            Console.WriteLine("Calculating high and low.");
            float low = float.MaxValue;
            float high = float.MinValue;
            int currentRow = 0;
            while (currentRow < rows)
            {
                var line = stream.ReadLine();
                var numbers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (numbers.Length < cols)
                {
                    throw new Exception("Invalid number of columns in input file.");
                }

                for (int i = 0; i < cols; ++i)
                {
                    var num = float.Parse(numbers[i]);
                    if (num < low && num > -900)
                    {
                        low = num;
                    }

                    if (num > high)
                    {
                        high = num;
                    }
                }

                if (currentRow % 1000 == 0)
                {
                    Console.WriteLine($"Row: {currentRow}");
                }

                ++currentRow;
            }

            Console.WriteLine("Drawing image.");
            stream = File.OpenText(inputFileName);
            for (int i = 0; i < 6; ++i)
            {
                stream.ReadLine();
            }

            Bitmap image = new Bitmap(cols, rows);
            currentRow = 0;
            low = 0;
            while (currentRow < rows)
            {
                var line = stream.ReadLine();
                var numbers = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < cols; ++i)
                {
                    var num = float.Parse(numbers[i]);
                    int shade = (int)(((num - low) / (high - low)) * 255);
                    shade = Math.Max(shade, 0);
                    image.SetPixel(i, currentRow, Color.FromArgb(shade, shade, shade));
                }

                if (currentRow % 1000 == 0)
                {
                    Console.WriteLine($"Row: {currentRow}");
                }

                ++currentRow;
            }

            image.Save(outputFileName);
        }
    }
}
using SimplePaletteQuantizer.Quantizers;
using System.Drawing;
using System.Drawing.Imaging;
using PixelAvatarGenerator.Extensions;
using Fclp;
using System;

namespace PixelAvatarGenerator
{
    class Program
    {
        static string input_filename;
        static string output_filename;
        static int width = 184;
        static int height = 184;
        static int colors = 16 * 2;

        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser();

            p.Setup<string>('i', "input-file")
            .Callback(item => input_filename = item)
            .Required();

            p.Setup<string>('o', "output-file")
            .Callback(item => output_filename = item)
            .SetDefault("pixelated.png");

            p.Setup<int>('w', "width")
            .Callback(item => width = item)
            .SetDefault(184);

            p.Setup<int>('h', "height")
            .Callback(item => height = item)
            .SetDefault(184);

            p.Setup<int>('c', "colors")
            .Callback(item => colors = item)
            .SetDefault(16 * 2);

            var result = p.Parse(args);
            if (result.HasErrors || result.HelpCalled || result.EmptyArgs)
            {
                PrintHelp();
            }
            else
            {
                Image image = Image.FromFile(input_filename);
                image = image.ToPixelAvatar(width, height, colors);
                image.Save(output_filename, ImageFormat.Png);
                image.Dispose();
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("-i --input-file\tInput filename");
            Console.WriteLine("-o --output-file\tOutput filename");
            Console.WriteLine("-w --width\tOutput width");
            Console.WriteLine("-h --height\tOutput height");
            Console.WriteLine("-c --colors\tAmount of colors (near perfect = 256, otherwise 32)");

            Console.WriteLine("Usage: .\\PixelAvatarGenerator.exe -i input_file.png -o output_file.png -w 1024 -h 1024 -c 256");
            Console.WriteLine("Usage: .\\PixelAvatarGenerator.exe -i input_file.png -o output_file.png");
            Console.WriteLine("Usage: .\\PixelAvatarGenerator.exe -i input_file.png");
        }
    }
}

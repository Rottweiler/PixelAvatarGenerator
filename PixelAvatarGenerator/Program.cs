using SimplePaletteQuantizer.Quantizers;
using System.Drawing;
using System.Drawing.Imaging;
using PixelAvatarGenerator.Extensions;

namespace PixelAvatarGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string fn = args[0];
            Image img1 = Image.FromFile(fn);
            img1 = img1.ToPixelAvatar(184, 184, 16*2);
            img1.Save("out.png", ImageFormat.Png);
        }
    }
}

using SimplePaletteQuantizer.Quantizers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;

namespace PixelAvatarGenerator.Extensions
{
    public static class Bitmap
    {
        public static IColorQuantizer quantizer = new PaletteQuantizer();

        public static System.Drawing.Image ToPixelAvatar(this System.Drawing.Image main, int width, int height, int colors)
        {
            main = main.ReDraw(16, 16);
            main = main.GetQuantizedImage(colors);
            main = main.ReDraw(width, height);
            return main;
        }

        public static Image ReDraw(this Image main, int w, int h,
            CompositingQuality quality = CompositingQuality.Default, //linear?
            SmoothingMode smoothing_mode = SmoothingMode.None,
            InterpolationMode ip_mode = InterpolationMode.NearestNeighbor)
        {
            //size
            double dbl = (double)main.Width / (double)main.Height;

            //preserve size ratio
            if ((int)((double)h * dbl) <= w)
                w = (int)((double)h * dbl);
            else
                h = (int)((double)w / dbl);

            //draw
            Image newImage = new System.Drawing.Bitmap(w, h);

            using(Graphics gfx = Graphics.FromImage(newImage))
            {
                gfx.CompositingQuality = quality;
                gfx.SmoothingMode = smoothing_mode;
                gfx.InterpolationMode = ip_mode;
                gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                gfx.Clear(Color.Transparent);

                gfx.DrawImage(main,
                new System.Drawing.Rectangle(0, 0, w, h),
                new System.Drawing.Rectangle(0, 0, main.Width, main.Height),
                System.Drawing.GraphicsUnit.Pixel);
            }

            return newImage;
        }

        public static Image GetQuantizedImage(this Image image, int palette_size = 256)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot quantize a null image.";
                throw new ArgumentNullException(message);
            }

            // locks the source image data
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)image;
            Rectangle bounds = Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData sourceData = bitmap.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                Int32[] sourceBuffer = new Int32[image.Width];
                Int64 sourceOffset = sourceData.Scan0.ToInt64();

                for (Int32 row = 0; row < image.Height; row++)
                {
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, image.Width);

                    foreach (Color color in sourceBuffer.Select(argb => Color.FromArgb(argb)))
                    {
                        quantizer.AddColor(color);
                    }

                    // increases a source offset by a row
                    sourceOffset += sourceData.Stride;
                }
            }
            catch
            {
                bitmap.UnlockBits(sourceData);
                throw;
            }

            // calculates the palette
            System.Drawing.Bitmap result = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            List<Color> palette = quantizer.GetPalette(palette_size);
            System.Drawing.Imaging.ColorPalette imagePalette = result.Palette;

            //textBox1.Text = string.Format("Original picture: {0} colors", quantizer.GetColorCount());
            //textBox2.Text = string.Format("Quantized picture: {0} colors", palette.Count);

            for (Int32 index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = palette[index];
            }

            result.Palette = imagePalette;

            // locks the target image data
            System.Drawing.Imaging.BitmapData targetData = result.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            try
            {
                Byte[] targetBuffer = new Byte[result.Width];
                Int32[] sourceBuffer = new Int32[image.Width];
                Int64 sourceOffset = sourceData.Scan0.ToInt64();
                Int64 targetOffset = targetData.Scan0.ToInt64();

                for (Int32 row = 0; row < image.Height; row++)
                {
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, image.Width);

                    for (Int32 index = 0; index < image.Width; index++)
                    {
                        Color color = Color.FromArgb(sourceBuffer[index]);
                        targetBuffer[index] = quantizer.GetPaletteIndex(color);
                    }

                    Marshal.Copy(targetBuffer, 0, new IntPtr(targetOffset), result.Width);

                    // increases the offsets by a row
                    sourceOffset += sourceData.Stride;
                    targetOffset += targetData.Stride;
                }
            }
            finally
            {
                // releases the locks on both images
                bitmap.UnlockBits(sourceData);
                result.UnlockBits(targetData);
            }

            return result;
        }
    }
}

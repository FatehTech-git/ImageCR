using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageCR
{
    class Program
    {
        /// <summary>
        /// Main method of the program , here we get source directory , quality percent and image max width from user
        /// then we use CompressImage method for each image in the source directory and and it will create
        /// optimized image in the CompressedAndResizedImages folder in source directory
        /// </summary>
        static void Main()
        {
            GetUserData(out var sourceDirectory, out var destinationPath, out var qualityPercent, out var imageMaxWidth);
            string[] imageFilePaths = GetImageFilePaths(sourceDirectory);

            foreach (var imagePath in imageFilePaths)
            {
                CompressImage(imagePath, destinationPath, qualityPercent, imageMaxWidth);
            }
        }

        /// <summary>
        /// In this method we collect user data within console app
        /// </summary>
        /// <param name="sourceDirectory">source directory of images</param>
        /// <param name="destinationPath">destination for optimized images</param>
        /// <param name="qualityPercent">percent of quality</param>
        /// <param name="imageMaxWidth">max width for optimized images</param>
        public static void GetUserData(out string sourceDirectory, out string destinationPath, out int qualityPercent, out int imageMaxWidth)
        {
            Console.WriteLine("Welcome to Image compress and resize app");
            Console.WriteLine("Enter source path for your images (for current path enter .)");
            Console.WriteLine();
            sourceDirectory = Console.ReadLine();
            if (sourceDirectory != null && sourceDirectory.Equals("."))
                sourceDirectory = Environment.CurrentDirectory;
            if (!Directory.Exists(sourceDirectory))
            {
                throw new Exception("Directory does not exist");
            }

            qualityPercent = -1;
            Console.WriteLine("Enter Quality Percent");
            int.TryParse(Console.ReadLine(), out qualityPercent);
            if (qualityPercent < 0 || qualityPercent > 100)
            {
                throw new Exception("Quality is not valid");
            }


            imageMaxWidth = 4096;
            Console.WriteLine("Enter image max width");
            int.TryParse(Console.ReadLine(), out imageMaxWidth);

            destinationPath = sourceDirectory + "/CompressedAndResizedImages";
            Directory.CreateDirectory(destinationPath);
        }

        /// <summary>
        /// Here we get all image files path within sourceDirectory
        /// </summary>
        /// <param name="sourceDirectory">source directory of images</param>
        /// <returns>Path of images in directory</returns>
        public static string[] GetImageFilePaths(string sourceDirectory)
        {
            string[] files = Directory.GetFiles(sourceDirectory);
            List<string> imageFileList = new List<string>();
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file).ToUpper();
                if (ext == ".PNG" || ext == ".JPG")
                    imageFileList.Add(file);
            }

            return imageFileList.ToArray();
        }


        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        /// <summary>
        /// Main method of compressing images
        /// </summary>
        /// <param name="imagePath">path of image</param>
        /// <param name="destPath">destination path for optimized images</param>
        /// <param name="quality">quality percent</param>
        /// <param name="imageMaxWidth">max width for optimized image</param>
        public static void CompressImage(string imagePath, string destPath, int quality, int imageMaxWidth)
        {
            var fileName = Path.GetFileName(imagePath);
            destPath = destPath + "\\" + fileName;

            var img =
                Image.FromFile(imagePath, true);
            if (img.Width > imageMaxWidth)
            {
                int scaledHeight = img.Height * imageMaxWidth / img.Width;
                using (Bitmap bmp1 = ResizeImage(img, imageMaxWidth, scaledHeight))
                {
                    MakeBitmapCompress(destPath, quality, bmp1);
                }
            }
            else
            {
                using (Bitmap bmp1 = new Bitmap(imagePath))
                {
                    MakeBitmapCompress(destPath, quality, bmp1);
                }
            }


        }

        /// <summary>
        /// optimizing bitmap image
        /// </summary>
        /// <param name="destPath">destination path for image</param>
        /// <param name="quality">image quality percent</param>
        /// <param name="bmp1">bitmap image</param>
        private static void MakeBitmapCompress(string destPath, int quality, Bitmap bmp1)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            var qualityEncoder = Encoder.Quality;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(qualityEncoder, quality);

            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(destPath, jpgEncoder, myEncoderParameters);
        }

        /// <summary>
        /// Get image codec info
        /// </summary>
        /// <param name="format">format of image</param>
        /// <returns>image codec</returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}

using System;
using System.IO;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class ImageProcessor : IImageProcessor
    {
        public byte[] Resize(byte[] source, int targetWidth, int targetHeight, IImageFormat targetFormat)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (targetFormat == null)
                throw new ArgumentNullException(nameof(targetFormat));

            using (var image = Image.Load(source))
            using (var targetStream = new MemoryStream())
            {
                image.Mutate(x => x.Resize(targetWidth, targetHeight));
                image.Save(targetStream, targetFormat);
                return targetStream.ToArray();
            }
        }
    }
}

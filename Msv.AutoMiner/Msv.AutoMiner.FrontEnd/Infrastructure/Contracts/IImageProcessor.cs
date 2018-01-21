using SixLabors.ImageSharp.Formats;

namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IImageProcessor
    {
        byte[] Resize(byte[] source, int targetWidth, int targetHeight, IImageFormat targetFormat);
    }
}

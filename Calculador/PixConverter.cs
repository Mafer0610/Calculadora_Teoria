using System.Drawing;
using System.IO;
using Tesseract;

public static class PixConverter
{
    public static Pix ToPix(Image image)
    {
        using (var ms = new MemoryStream())
        {
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return Pix.LoadFromMemory(ms.ToArray());
        }
    }
}

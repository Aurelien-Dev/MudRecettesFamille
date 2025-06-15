using ImageMagick;
using MudBlazor;

namespace RecettesFamille.Extensions;

public static class ImageExtensions
{
    public static string ToImageSrc(this byte[] imageBytes, string mimeType = "image/webp")
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return string.Empty;

        var base64 = Convert.ToBase64String(imageBytes);
        return $"data:{mimeType};base64,{base64}";
    }

    public static byte[] CompressImage(this byte[] imageBytes)
    {
        const int maxWidth = 1024;

        using (var image = new MagickImage(imageBytes))
        {
            // Redimensionner si l'image est plus large que maxWidth
            if (image.Width > maxWidth)
            {
                image.Resize(maxWidth, 0); // hauteur proportionnelle
            }

            image.Quality = 75;
            image.Format = MagickFormat.WebP;

            using (var ms = new MemoryStream())
            {
                image.Write(ms);

                var compressedBytes = ms.ToArray();

                File.WriteAllBytes("C:\\Users\\acret\\Pictures\\test.webp", compressedBytes);

                return ms.ToArray();
            }
        }
    }

    public static byte[] CropImage(this byte[] imageBytes, decimal? x, decimal? y, decimal? width, decimal? height)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Image bytes cannot be null or empty.", nameof(imageBytes));

        if (x == null || y == null || width == null || height == null)
            throw new ArgumentException("Crop dimensions (x, y, width, height) must not be null.");

        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be greater than zero.");

        using (var image = new MagickImage(imageBytes))
        {
            //if (x < 0 || y < 0 || x + width > image.Width || y + height > image.Height)
            //    throw new ArgumentOutOfRangeException("Crop dimensions are out of the image bounds.");

            uint uWidth = Convert.ToUInt32(width);
            uint uHeight = Convert.ToUInt32(height);
            int uX = Convert.ToInt32(x);
            int uY = Convert.ToInt32(y);

            // Apply the crop
            var geometry = new MagickGeometry(uX, uY, uWidth, uHeight)
            {
                IgnoreAspectRatio = true
            };

            image.Crop(geometry);

            using (var ms = new MemoryStream())
            {
                image.Write(ms);
                var croppedBytes = ms.ToArray();

                File.WriteAllBytes("C:\\Users\\acret\\Pictures\\test-cropped.webp", croppedBytes);

                return croppedBytes;
            }
        }
    }
}
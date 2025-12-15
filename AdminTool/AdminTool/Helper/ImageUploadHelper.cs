using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Net.Http.Headers; 
namespace AdminTool.Helper
{
    public static class ImageUploadHelper
    {
        public static async Task<byte[]> ToPngBytesAsync(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Empty file");

            if (string.Equals(file.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Path.GetExtension(file.FileName), ".svg", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("SVG는 지원하지 않습니다. PNG/JPG/WebP를 업로드하세요.");

            await using var s = file.OpenReadStream();
            using var img = await Image.LoadAsync(s, ct);

            await using var ms = new MemoryStream();
            var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression };
            await img.SaveAsPngAsync(ms, encoder, ct);

            return ms.ToArray();
        }

        public static MultipartFormDataContent BuildUploadForm(string key, byte[] pngBytes)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key is required", nameof(key));

            if (pngBytes is null || pngBytes.Length == 0)
                throw new ArgumentException("pngBytes is empty", nameof(pngBytes));

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(key), "Key");

            var fileContent = new ByteArrayContent(pngBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(fileContent, "File", $"{key}.png");

            return form;
        }
    }
}

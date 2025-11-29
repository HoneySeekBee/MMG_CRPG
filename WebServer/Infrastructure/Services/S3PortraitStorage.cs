using Amazon.S3.Model;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Storage;

namespace Infrastructure.Services
{
    public class S3PortraitStorage : IPortraitStorage
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucketName;

        public S3PortraitStorage(IAmazonS3 s3, string bucketName)
        {
            _s3 = s3;
            _bucketName = bucketName;
        }

        public string GetPublicUrl(string key, int version)
            => $"/api/image/portraits/{key}?v={version}";

        public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct)
        {
            var put = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"portraits/{key}.png",
                InputStream = content,
                ContentType = contentType,
            };

            await _s3.PutObjectAsync(put, ct);
        }

        public async Task<byte[]> LoadAsync(string key, CancellationToken ct)
        {
            var res = await _s3.GetObjectAsync(_bucketName, $"portraits/{key}.png", ct);
            using var ms = new MemoryStream();
            await res.ResponseStream.CopyToAsync(ms, ct);
            return ms.ToArray();
        }

        public async Task DeleteAsync(string key, CancellationToken ct)
        {
            await _s3.DeleteObjectAsync(_bucketName, $"portraits/{key}.png", ct);
        }
    }
}

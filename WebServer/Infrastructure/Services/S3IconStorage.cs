using Amazon.S3;
using Amazon.S3.Model;
using Application.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class S3IconStorage : IIconStorage
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucketName;

        public S3IconStorage(IAmazonS3 s3, string bucketName)
        {
            _s3 = s3;
            _bucketName = bucketName;
        }

        public string GetPublicUrl(string key, int version) // AWS S3에 바로 입장하게 하기 
           => $"https://d3nehzpoo6py80.cloudfront.net/icons/{key}.png?v={version}";

        public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct)
        {
            var put = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"icons/{key}.png",
                InputStream = content,
                ContentType = contentType
            };
            put.Headers.CacheControl = "public, max-age=31536000, immutable";

            await _s3.PutObjectAsync(put, ct);
        }

        public async Task<byte[]> LoadAsync(string key, CancellationToken ct)
        {
            var res = await _s3.GetObjectAsync(_bucketName, $"icons/{key}.png", ct);

            using var ms = new MemoryStream();
            await res.ResponseStream.CopyToAsync(ms, ct);
            return ms.ToArray();
        }

        public async Task DeleteAsync(string key, CancellationToken ct)
        {
            await _s3.DeleteObjectAsync(_bucketName, $"icons/{key}.png", ct);
        }
    }
}

namespace MassTransit.AWS.S3.Tests
{
    using Amazon;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using MessageData;
    using NUnit.Framework;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestFixture]
    public class AWSS3MessageDataRepositoryTestsForPuttingMessageData
    {
        [Test]
        public async Task ThenExpirationIsNotSet()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(nameof(ThenExpirationIsNotSet)));
            var uri = await _repository.Put(stream);
            var objectKey = _resolver.GetObjectKey(uri);

            using var s3Object = await _client.GetObjectAsync(new GetObjectRequest { BucketName = bucketName, Key = objectKey });

            Assert.That(s3Object.Expiration, Is.Null);
        }

        [Test]
        public async Task ThenMessageStoredAsExpected()
        {
            var expectedData = Encoding.UTF8.GetBytes(nameof(ThenExpirationIsNotSet));
            using var stream = new MemoryStream(expectedData);
            var uri = await _repository.Put(stream);
            var objectKey = _resolver.GetObjectKey(uri);

            using var s3Object = await _client.GetObjectAsync(new GetObjectRequest { BucketName = bucketName, Key = objectKey });

            using var memoryStream = new MemoryStream();
            await s3Object.ResponseStream.CopyToAsync(memoryStream);

            byte[] result = memoryStream.ToArray();

            Assert.That(result, Is.EqualTo(expectedData));
        }

        [OneTimeSetUp]
        public async Task GivenAnAWSS3StorageMessageDataRepository_WhenPuttingMessageData()
        {
            _client = new AmazonS3Client(new BasicAWSCredentials(Configuration.AccessKey, Configuration.SecretKey), RegionEndpoint.GetBySystemName(Configuration.Region));
            _repository = new AWSS3StorageMessageDataRepository(_client, bucketName);
            _resolver = new MessageDataResolver();

            if (!await BucketExists())
            {
                try
                {
                    await _client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
                    _didCreateBucket = true;
                }
                catch (AmazonS3Exception)
                {
                    // Try and proceed in case bucket exists and is usable but we don't have
                    // list bucket permissions.
                }
            }
        }

        private async Task<bool> BucketExists()
        {
            try
            {
                var listResponse = await _client.ListBucketsAsync();
                return listResponse.Buckets.Any(b => b.BucketName == bucketName);
            }
            catch
            {
                return false;
            }
        }

        [OneTimeTearDown]
        public async Task Kill()
        {
            if (_didCreateBucket)
            {
                await _client.DeleteBucketAsync(bucketName);
            }
        }

        bool _didCreateBucket;
        AmazonS3Client _client;
        AWSS3StorageMessageDataRepository _repository;
        MessageDataResolver _resolver;
        const string bucketName = "masstransit.aws.s3.tests";
    }
}

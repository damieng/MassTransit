namespace MassTransit.AWS.S3.MessageData
{
    using Amazon.S3;
    using Amazon.S3.Model;
    using Context;
    using MassTransit.MessageData;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;

    public class AWSS3StorageMessageDataRepository :
        IMessageDataRepository,
        IBusObserver
    {
        readonly AmazonS3Client _client;
        readonly string _bucketName;
        readonly IObjectKeyGenerator _keyGenerator;
        readonly IMessageDataResolver _resolver;

        public AWSS3StorageMessageDataRepository(AmazonS3Client client, string bucketName)
            : this(client, bucketName, new NewIdObjectNameGenerator(), new MessageDataResolver())
        {
        }

        public AWSS3StorageMessageDataRepository(AmazonS3Client client, string bucketName, IObjectKeyGenerator keyGenerator, IMessageDataResolver resolver)
        {
            _client = client;
            _bucketName = bucketName;
            _keyGenerator = keyGenerator;
            _resolver = resolver;
        }

        public Task PostCreate(IBus bus)
        {
            return TaskUtil.Completed;
        }

        public Task CreateFaulted(Exception exception)
        {
            return TaskUtil.Completed;
        }

        public async Task PreStart(IBus bus)
        {
            try
            {
                var listBucketsResponse = await _client.ListBucketsAsync().ConfigureAwait(false);
                var containerExists = listBucketsResponse.Buckets.Any(b => b.BucketName == _bucketName);
                if (!containerExists)
                {
                    LogContext.Warning?.Log("AWS S3 Bucket does not exist: {Bucket}", _bucketName);
                }
            }
            catch (Exception exception)
            {
                LogContext.Error?.Log(exception, "AWS S3 storage failure.");
            }
        }

        public Task PostStart(IBus bus, Task<BusReady> busReady)
        {
            return TaskUtil.Completed;
        }

        public Task StartFaulted(IBus bus, Exception exception)
        {
            return TaskUtil.Completed;
        }

        public Task PreStop(IBus bus)
        {
            return TaskUtil.Completed;
        }

        public Task PostStop(IBus bus)
        {
            return TaskUtil.Completed;
        }

        public Task StopFaulted(IBus bus, Exception exception)
        {
            return TaskUtil.Completed;
        }

        public async Task<Stream> Get(Uri address, CancellationToken cancellationToken = default)
        {
            var key = _resolver.GetObjectKey(address);

            var getObjectRequest = new GetObjectRequest { BucketName = _bucketName, Key = key };
            var blob = await _client.GetObjectAsync(getObjectRequest, cancellationToken).ConfigureAwait(false);
            try
            {
                LogContext.Debug?.Log("GET Message Data: {Address} ({Bucket}/{Key})", address, _bucketName, key);
                return blob.ResponseStream;
            }
            catch (AmazonS3Exception exception)
            {
                throw new MessageDataException($"MessageData content not found: {_bucketName}/{key}", exception);
            }
        }

        public async Task<Uri> Put(Stream stream, TimeSpan? timeToLive = default, CancellationToken cancellationToken = default)
        {
            var key = _keyGenerator.GenerateObjectKey();
            var uri = _resolver.GetAddress(key);

            var putObjectRequest = new PutObjectRequest { BucketName = _bucketName, Key = key, InputStream = stream };
            await _client.PutObjectAsync(putObjectRequest, cancellationToken).ConfigureAwait(false);

            await SetObjectExpiration(key, timeToLive, cancellationToken).ConfigureAwait(false);

            LogContext.Debug?.Log("PUT Message Data: {Address} ({Bucket}/{Key})", uri, _bucketName, key);

            return uri;
        }

        private async Task SetObjectExpiration(string objectKey, TimeSpan? timeToLive, CancellationToken cancellationToken)
        {
            if (timeToLive.HasValue)
            {
                var putObjectRetentionRequest = new PutObjectRetentionRequest()
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    Retention = new ObjectLockRetention { RetainUntilDate = DateTime.UtcNow.Add(timeToLive.Value) }
                };

                await _client.PutObjectRetentionAsync(putObjectRetentionRequest, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

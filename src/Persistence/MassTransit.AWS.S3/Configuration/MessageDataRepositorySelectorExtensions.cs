namespace MassTransit
{
    using System;
    using Configuration;
    using MessageData;
    using AWS.S3.MessageData;
    using Amazon.S3;

    public static class MessageDataRepositorySelectorExtensions
    {
        /// <summary>
        /// Use AWS S3 for message data storage.
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="client">A configured <see cref="AmazonS3Client"/> for connecting to S3.</param>
        /// <param name="bucketName">Specify the bucket name (defaults to message-data)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If any non-optional argument is <see langword="null"/>.</exception>
        public static IMessageDataRepository AWSS3(this IMessageDataRepositorySelector selector, AmazonS3Client client,
            string bucketName = default)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            if (client is null)
                throw new ArgumentNullException(nameof(client));

            return new AWSS3StorageMessageDataRepository(client, bucketName ?? "message-data");
        }
    }
}

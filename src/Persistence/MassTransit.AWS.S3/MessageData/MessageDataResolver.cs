namespace MassTransit.AWS.S3.MessageData
{
    using System;

    public class MessageDataResolver :
        IMessageDataResolver
    {
        const string Scheme = "urn";
        const string System = "s3";

        readonly string _format = string.Join(":", Scheme, System);

        public string GetObjectKey(Uri address)
        {
            if (address.Scheme != Scheme)
                throw new UriFormatException($"The scheme did not match the expected value: {Scheme}");

            string[] tokens = address.AbsolutePath.Split(':');

            if (tokens.Length != 2 || !address.AbsoluteUri.StartsWith($"{_format}:"))
                throw new UriFormatException($"Urn is not in the correct format. Use '{_format}:{{resourceId}}'");

            return tokens[tokens.Length - 1];
        }

        public Uri GetAddress(string id)
        {
            return new Uri($"{_format}:{id}");
        }
    }
}

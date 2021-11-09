namespace MassTransit.AWS.S3.Tests
{
    using System;
    using MassTransit.AWS.S3.MessageData;
    using NUnit.Framework;


    [TestFixture]
    public class AWSS3MessageDataResolverTests
    {
        private readonly IMessageDataResolver resolver = new MessageDataResolver();

        [Test]
        [TestCase("abc:s3:gridfs:12345")]
        [TestCase("urn:s3:gridfs:somethingelse:12345")]
        [TestCase("urn:s3:gridfsthing:12345")]
        public void GetObjectKey_GivenBadUri_ThenExceptionIsUriFormatException(Uri address)
        {
            Assert.Throws<UriFormatException>(() => resolver.GetObjectKey(address));
        }


        [Test]
        [TestCase("urn:s3:12345")]
        [TestCase("urn:s3:dfsdfsdfsdf")]
        [TestCase("urn:s3:3cvsd-345")]
        public void GetObjectKey_RoundTrips_X(Uri expectedAddress)
        {
            var objectKey = resolver.GetObjectKey(expectedAddress);
            var actualAddress = resolver.GetAddress(objectKey);

            Assert.AreEqual(expectedAddress, actualAddress);
        }
    }
}

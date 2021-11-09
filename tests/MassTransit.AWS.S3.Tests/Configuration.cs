namespace MassTransit.AWS.S3.Tests
{
    using System;
    using NUnit.Framework;


    public static class Configuration
    {
        public static string AccessKey =>
            TestContext.Parameters.Exists("S3AccessKey")
                ? TestContext.Parameters.Get("S3AccessKey")
                : Environment.GetEnvironmentVariable("MT_S3_ACCESS_KEY");

        public static string SecretKey =>
            TestContext.Parameters.Exists("S3SecretKey")
                ? TestContext.Parameters.Get("S3SecretKey")
                : Environment.GetEnvironmentVariable("MT_S3_SECRET_KEY");

        public static string Region =>
            TestContext.Parameters.Exists("S3Region")
                ? TestContext.Parameters.Get("S3Region")
                : Environment.GetEnvironmentVariable("MT_S3_REGION");
    }
}

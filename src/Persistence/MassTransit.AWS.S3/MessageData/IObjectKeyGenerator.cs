namespace MassTransit.AWS.S3.MessageData
{
    public interface IObjectKeyGenerator
    {
        string GenerateObjectKey();
    }
}

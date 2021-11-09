namespace MassTransit.AWS.S3.MessageData
{
    using Util;


    public class NewIdObjectNameGenerator :
        IObjectKeyGenerator
    {
        public string GenerateObjectKey()
        {
            return FormatUtil.Formatter.Format(NewId.Next().ToSequentialGuid().ToByteArray());
        }
    }
}

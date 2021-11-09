namespace MassTransit.AWS.S3.MessageData
{
    using System;


    public interface IMessageDataResolver
    {
        /// <summary>
        /// Returns the key name for the specified address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        string GetObjectKey(Uri address);

        /// <summary>
        /// Returns the address for the specified key name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Uri GetAddress(string key);
    }
}

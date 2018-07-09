using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class StorageNotImplementedException : Exception
    {
        public StorageNotImplementedException() : base("No storage is implemented for saving and loading data. You must assign a built-in storage to ParbadConfiguration.Storage. You can also implement your custom storage by implementing Parbad.Infrastructure.Data.Storage.")
        {
        }
    }
}
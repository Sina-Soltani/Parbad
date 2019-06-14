// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Parbad.Storage.Cache.Internal
{
    internal static class ObjectSerializer
    {
        public static byte[] SerializeObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();

            byte[] buffer;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);

                buffer = stream.ToArray();
            }

            return buffer;
        }

        public static T DeserializeObject<T>(byte[] buffer)
        {
            IFormatter formatter = new BinaryFormatter();

            T result;

            using (var stream = new MemoryStream(buffer))
            {
                result = (T)formatter.Deserialize(stream);
            }

            return result;
        }
    }
}

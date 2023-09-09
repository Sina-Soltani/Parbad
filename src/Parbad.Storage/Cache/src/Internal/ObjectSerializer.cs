// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Text;
using Newtonsoft.Json;

namespace Parbad.Storage.Cache.Internal;

internal static class ObjectSerializer
{
    public static byte[] SerializeObject(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);

        return Encoding.UTF8.GetBytes(json);
    }

    public static T DeserializeObject<T>(byte[] buffer)
    {
        var json = Encoding.UTF8.GetString(buffer);

        return JsonConvert.DeserializeObject<T>(json);
    }
}

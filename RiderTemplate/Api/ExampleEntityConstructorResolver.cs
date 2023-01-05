using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Domain.Entities;
using HarmonyLib;

namespace Api;

public class ExampleEntityConstructorResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (type == typeof(Entity))
        {
            jsonTypeInfo.CreateObject = () => new Entity();
        }

        return jsonTypeInfo;
    }
}

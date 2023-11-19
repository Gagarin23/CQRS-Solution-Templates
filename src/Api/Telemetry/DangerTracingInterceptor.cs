using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace Api.Telemetry;

public static class DangerTracingInterceptor
{
    private static readonly ConcurrentDictionary<Type, ActivitySource> ActivitySources = new();
    private static readonly ConcurrentDictionary<(object, string), Activity> Activities = new();

    public static void Prefix(object __instance, MethodBase __originalMethod)
    {
        var activitySource = ActivitySources.GetOrAdd(__instance.GetType(), _ =>
                new ActivitySource(__instance.GetType().Name));

        var activity = Activities.GetOrAdd((__instance, __originalMethod.Name), _ =>
            activitySource.StartActivity(__originalMethod.Name));
    }

    public static void Postfix(object __instance, MethodBase __originalMethod, object __result)
    {
        var type = __result.GetType();
        // Асинхронные методы разворачиваются в машину состояний IAsyncStateMachine.
        // Постфикс срабазывает после первого return из машины состояний.
        // Поэтому нам нужно принудительно дождаться завершения всего асинхронного метода через св-во Result.
        // ВАЖНО: мы принудительно встаём в блокировку потока. Код ТОЛЬКО для дебага.
        var result = AccessTools.PropertyGetter(type, "Result")?.Invoke(__result, null);

        if (Activities.TryGetValue((__instance, __originalMethod.Name), out var activity))
        {
            activity.Dispose();
            Activities.TryRemove((__instance, __originalMethod.Name), out _);
        }
    }
}

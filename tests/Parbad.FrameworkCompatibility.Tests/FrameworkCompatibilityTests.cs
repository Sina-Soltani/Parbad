using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parbad.FrameworkCompatibility.Tests;

[TestClass]
public class FrameworkCompatibilityTests
{
    private static readonly string[] ExpectedRuntimeTargets =
    [
        "netcoreapp3.0",
        "netcoreapp3.1",
        "net5.0",
        "net6.0",
        "net7.0",
        "net8.0",
        "net9.0",
        "net10.0"
    ];

    [TestMethod]
    public void Current_runtime_target_is_declared_as_supported()
    {
        CollectionAssert.Contains(ExpectedRuntimeTargets, CurrentTargetFramework);
    }

    [TestMethod]
    public void Runtime_compatible_project_assemblies_can_be_loaded()
    {
        var loadedAssemblies = GetCoveredProjectTypes()
                               .Select(type => type.Assembly.GetName().Name)
                               .Distinct()
                               .OrderBy(name => name)
                               .ToArray();

        CollectionAssert.AreEquivalent(GetExpectedProjectAssemblyNames(), loadedAssemblies);
    }

    private static Type[] GetCoveredProjectTypes()
    {
        var types = new List<Type>
        {
            typeof(Parbad.Options.ParbadOptions),
            typeof(Parbad.AspNetCore.VirtualGateway.ParbadVirtualGatewayMiddleware),
            typeof(Parbad.Storage.Abstractions.IStorage),
            typeof(Parbad.Storage.Cache.MemoryCache.MemoryCacheStorageOptions),
            typeof(Parbad.Gateway.FanAva.FanAvaGatewayOptions),
            typeof(Parbad.Gateway.IdPay.IdPayGatewayOptions),
            typeof(Parbad.Gateway.IranKish.IranKishGatewayOptions),
            typeof(Parbad.Gateway.PayIr.PayIrGatewayOptions),
            typeof(Parbad.Gateway.PayPing.PayPingGatewayOptions),
            typeof(Parbad.Gateway.Sepehr.SepehrGatewayOptions),
            typeof(Parbad.Gateway.YekPay.YekPayGatewayOptions),
            typeof(Parbad.Gateway.ZarinPal.ZarinPalGatewayOptions),
            typeof(Parbad.Gateway.Zibal.ZibalGatewayOptions)
        };

#if NET5_0_OR_GREATER
        types.Add(typeof(Parbad.Storage.EntityFrameworkCore.EntityFrameworkCoreStorage));
#endif

        return types.ToArray();
    }

    private static string[] GetExpectedProjectAssemblyNames()
    {
        var names = new List<string>
        {
            "Parbad",
            "Parbad.AspNetCore",
            "Parbad.Storage.Abstractions",
            "Parbad.Storage.Cache",
            "Parbad.Gateway.FanAva",
            "Parbad.Gateway.IdPay",
            "Parbad.Gateway.IranKish",
            "Parbad.Gateway.PayIr",
            "Parbad.Gateway.PayPing",
            "Parbad.Gateway.Sepehr",
            "Parbad.Gateway.YekPay",
            "Parbad.Gateway.ZarinPal",
            "Parbad.Gateway.Zibal"
        };

#if NET5_0_OR_GREATER
        names.Add("Parbad.Storage.EntityFrameworkCore");
#endif

        return names.OrderBy(name => name).ToArray();
    }

    private static string CurrentTargetFramework
    {
        get
        {
#if NETCOREAPP3_0
            return "netcoreapp3.0";
#elif NETCOREAPP3_1
            return "netcoreapp3.1";
#elif NET5_0
            return "net5.0";
#elif NET6_0
            return "net6.0";
#elif NET7_0
            return "net7.0";
#elif NET8_0
            return "net8.0";
#elif NET9_0
            return "net9.0";
#elif NET10_0
            return "net10.0";
#else
            return "unknown";
#endif
        }
    }
}

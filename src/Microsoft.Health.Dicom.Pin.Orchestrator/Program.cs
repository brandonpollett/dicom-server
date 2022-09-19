// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Dicom.Pin.Orchestrator.Options;

namespace Microsoft.Health.Dicom.Pin.Orchestrator;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddHostedService<Worker>();

                var serviceBusOptions = new ServiceBusOptions();
                hostContext.Configuration?.GetSection(ServiceBusOptions.ServiceBusOptionsSectionName).Bind(serviceBusOptions);

                serviceCollection.AddSingleton(_ => new ServiceBusClient(serviceBusOptions.ConnectionString));
            })
            .RunConsoleAsync();

        return 0;
    }
}

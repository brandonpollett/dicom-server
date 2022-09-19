// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Dicom.Pin.CosmosDb.Registration;
using Microsoft.Health.Dicom.Pin.ServiceBus.Registration;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddHostedService<Worker>();
                serviceCollection.AddCosmosDb(hostContext.Configuration);
                serviceCollection.AddServiceBus(hostContext.Configuration);
                serviceCollection.AddHttpClient();
            })
            .RunConsoleAsync();

        return 0;
    }
}

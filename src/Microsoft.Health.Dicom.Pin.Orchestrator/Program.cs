// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Dicom.Pin.Core.Features.Outputs;
using Microsoft.Health.Dicom.Pin.CosmosDb.Registration;
using Microsoft.Health.Dicom.Pin.ServiceBus.Registration;
using Microsoft.Health.Dicom.Pin.Storage.Registration;
using Microsoft.Health.Extensions.DependencyInjection;

namespace Microsoft.Health.Dicom.Pin.Orchestrator;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddHostedService<IncomingWorker>();
                serviceCollection.AddHostedService<OutgoingWorker>();
                serviceCollection.AddCosmosDb(hostContext.Configuration);
                serviceCollection.AddServiceBus(hostContext.Configuration);
                serviceCollection.AddStorage(hostContext.Configuration);
                serviceCollection.AddHttpClient();

                serviceCollection.Add<UpsRsOutputFactory>()
                    .Singleton()
                    .AsImplementedInterfaces();
            })
            .RunConsoleAsync();

        return 0;
    }
}

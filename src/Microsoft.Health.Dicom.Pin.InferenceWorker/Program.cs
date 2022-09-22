// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Dicom.Pin.CosmosDb.Registration;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;
using Microsoft.Health.Dicom.Pin.ServiceBus.Registration;
using Microsoft.Health.Dicom.Pin.Storage.Registration;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddHostedService<Worker>();
                serviceCollection.AddCosmosDb(hostContext.Configuration);
                serviceCollection.AddServiceBus(hostContext.Configuration);
                serviceCollection.AddStorage(hostContext.Configuration);
                serviceCollection.AddHttpClient();

                serviceCollection.Add<JpegInferenceDataFactory>()
                    .Singleton()
                    .AsImplementedInterfaces();

                serviceCollection.Add<PngInferenceDataFactory>()
                    .Singleton()
                    .AsImplementedInterfaces();

                serviceCollection.Add<DcmInferenceDataFactory>()
                    .Singleton()
                    .AsImplementedInterfaces();

                serviceCollection.Add<UpsRsInputFactory>()
                    .Singleton()
                    .AsImplementedInterfaces();

                serviceCollection
                    .AddFellowOakDicom()
                    .AddImageManager<ImageSharpImageManager>();
            })
            .UseConsoleLifetime();

        IHost host = builder.Build();

        DicomSetupBuilder.UseServiceProvider(host.Services);

        await host.RunAsync();

        return 0;
    }
}

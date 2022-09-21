// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Dicom.Pin.Storage.Features.TempFiles;
using Microsoft.Health.Dicom.Pin.Storage.Options;

namespace Microsoft.Health.Dicom.Pin.Storage.Registration;

public static class PinStorageRegistrationExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<StorageOptions>(configuration?.GetSection(StorageOptions.StorageOptionsSectionName));

        serviceCollection.AddSingleton<BlobContainerClient>(sp =>
        {
            IOptionsMonitor<StorageOptions> dbOptions = sp.GetRequiredService<IOptionsMonitor<StorageOptions>>();
            return new BlobContainerClient(dbOptions.CurrentValue.ConnectionString, dbOptions.CurrentValue.ContainerName);
        });

        serviceCollection.Add<StorageTempFileStore>()
            .Singleton()
            .AsImplementedInterfaces();

        return serviceCollection;
    }
}

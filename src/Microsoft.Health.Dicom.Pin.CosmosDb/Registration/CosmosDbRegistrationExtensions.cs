// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Dicom.Pin.CosmosDb.Options;

namespace Microsoft.Health.Dicom.Pin.CosmosDb.Registration;

public static class CosmosDbRegistrationExtensions
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosDbOptions>(configuration?.GetSection(CosmosDbOptions.CosmosDbOptionsSectionName));

        services.AddSingleton<CosmosClient>(sp =>
        {
            IOptionsMonitor<CosmosDbOptions> dbOptions = sp.GetRequiredService<IOptionsMonitor<CosmosDbOptions>>();
            return new CosmosClient(dbOptions.CurrentValue.ConnectionString);
        });

        return services;
    }
}

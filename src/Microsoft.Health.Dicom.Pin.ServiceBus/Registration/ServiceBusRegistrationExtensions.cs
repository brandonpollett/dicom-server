// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Dicom.Pin.ServiceBus.Options;

namespace Microsoft.Health.Dicom.Pin.ServiceBus.Registration;

public static class ServiceBusRegistrationExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<ServiceBusOptions>(configuration?.GetSection(ServiceBusOptions.ServiceBusOptionsSectionName));

        serviceCollection.AddSingleton<ServiceBusClient>(sp =>
        {
            IOptionsMonitor<ServiceBusOptions> dbOptions = sp.GetRequiredService<IOptionsMonitor<ServiceBusOptions>>();
            return new ServiceBusClient(dbOptions.CurrentValue.ConnectionString);
        });

        return serviceCollection;
    }
}

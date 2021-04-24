// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Dicom.Core.Registration;
using Microsoft.Health.Dicom.Elastic.Features.Query;
using Microsoft.Health.Dicom.Elastic.Features.Store;
using Microsoft.Health.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DicomServerBuilderElasticRegistrationExtensions
    {
        public static IDicomServerBuilder AddElastic(
            this IDicomServerBuilder dicomServerBuilder)
        {
            EnsureArg.IsNotNull(dicomServerBuilder, nameof(dicomServerBuilder));
            IServiceCollection services = dicomServerBuilder.Services;

            services.Add<ElasticQueryStore>()
                .Scoped()
                .AsSelf()
                .AsImplementedInterfaces();

            services.Add<ElasticIndexDataStore>()
                .Scoped()
                .AsSelf()
                .AsImplementedInterfaces();

            services.Add<ElasticIndexDataStoreFactory>()
                .Scoped()
                .AsSelf()
                .AsImplementedInterfaces();

            return dicomServerBuilder;
        }
    }
}

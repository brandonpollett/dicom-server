// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Core.Features.Store;

namespace Microsoft.Health.Dicom.Elastic.Features.Store
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Not production code.")]
    internal class ElasticIndexDataStoreFactory : IIndexDataStoreFactory
    {
        public IIndexDataStore GetInstance()
        {
            return new ElasticIndexDataStore();
        }
    }
}

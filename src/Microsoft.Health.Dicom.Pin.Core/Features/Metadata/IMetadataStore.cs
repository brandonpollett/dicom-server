// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.Core.Features.Metadata;

public interface IMetadataStore
{
    Task<Account> GetAccountAsync(string id, CancellationToken cancellationToken);

    Task<Inference> GetInferenceAsync(string id, CancellationToken cancellationToken);

    Task<IEnumerable<Inference>> GetInferencesAsync(string accountId, CancellationToken cancellationToken);
}

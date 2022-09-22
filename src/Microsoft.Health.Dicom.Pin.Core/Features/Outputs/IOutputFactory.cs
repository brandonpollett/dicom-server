// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.Core.Features.Outputs;

public interface IOutputFactory
{
    OrchestratorDataType OrchestratorDataType { get; }

    Task WriteAsync(UpsRsRequestProperties requestProperties, Stream result, CancellationToken cancellationToken);
}

// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Outputs;

public interface IOutputFactory
{
    OrchestratorDataType OrchestratorDataType { get; }

    Task<DicomInput> WriteAsync(UpsRsRequestProperties requestProperties, CancellationToken cancellationToken);
}

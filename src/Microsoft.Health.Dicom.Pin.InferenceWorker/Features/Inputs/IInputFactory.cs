﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;

public interface IInputFactory
{
    OrchestratorSourceType OrchestratorSourceType { get; }

    Task<DicomInput> RetrieveAsync(UpsRsRequestProperties requestProperties, CancellationToken cancellationToken);
}

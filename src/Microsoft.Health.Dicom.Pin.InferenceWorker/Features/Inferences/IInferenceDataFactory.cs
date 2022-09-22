// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Features.Inputs;
using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;

public interface IInferenceDataFactory
{
    InferenceDataType InferenceDataType { get; }

    Task<Stream> GetDataAsync(DicomInput input, CancellationToken cancellationToken);
}

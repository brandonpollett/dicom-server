// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.Core.Features.Inputs;

public interface IInputFactory
{
    OrchestratorDataType OrchestratorDataType { get; }

    Task<DicomInput> RetrieveAsync(UpsRsRequestProperties requestProperties, CancellationToken cancellationToken);
}

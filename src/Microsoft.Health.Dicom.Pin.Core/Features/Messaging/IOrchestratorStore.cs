// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.Core.Features.Messaging;

public interface IOrchestratorStore
{
    Task<OrchestratorRequest> GetRequestAsync(CancellationToken cancellationToken);

    Task CompleteRequestAsync(OrchestratorRequest request, CancellationToken cancellationToken);

    Task WriteRequestAsync(OrchestratorRequest orchestratorRequest, CancellationToken cancellationToken);
}

// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.Core.Features.Messaging;

public interface IInferenceStore
{
    Task WriteRequestsAsync(IEnumerable<InferenceRequest> inferenceRequests, CancellationToken cancellationToken);

    Task WriteRequestAsync(InferenceRequest inferenceRequest, CancellationToken cancellationToken);

    Task<InferenceRequest> GetRequestAsync(CancellationToken cancellationToken);

    Task DeadLetterRequestAsync(InferenceRequest inferenceRequest, CancellationToken cancellationToken);

    Task CompleteRequestAsync(InferenceRequest request, CancellationToken cancellationToken);

    Task WriteResponseAsync(InferenceResponse inferenceResponse, CancellationToken cancellationToken);

    Task<InferenceResponse> GetResponseAsync(CancellationToken cancellationToken);

    Task DeadLetterResponseAsync(InferenceResponse inferenceResponse, CancellationToken cancellationToken);

    Task CompleteResponseAsync(InferenceResponse request, CancellationToken cancellationToken);
}

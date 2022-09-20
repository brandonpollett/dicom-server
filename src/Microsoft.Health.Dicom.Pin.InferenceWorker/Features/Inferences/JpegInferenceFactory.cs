// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;

public class JpegInferenceFactory : IInferenceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public JpegInferenceFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
    }

    public InferenceInputType InferenceInputType { get; } = InferenceInputType.Jpeg;

    public async Task<InferenceResponse> ExecuteInferenceAsync(DicomInput dicomInput, InferenceRequest inferenceRequest, Inference inference, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(dicomInput, nameof(dicomInput));
        EnsureArg.IsNotNull(inferenceRequest, nameof(inferenceRequest));
        EnsureArg.IsNotNull(inference, nameof(inference));

        using HttpClient client = _httpClientFactory.CreateClient();
        using var content = new StringContent(string.Empty);
        HttpResponseMessage response = await client.PostAsync(inference.Uri, content, cancellationToken);

        return new InferenceResponse
        {
            AccountId = inferenceRequest.AccountId,
            InferenceId = inferenceRequest.InferenceId,
            RequestProperties = inferenceRequest.RequestProperties,
            StatusCode = response.StatusCode.ToString(),
        };
    }
}

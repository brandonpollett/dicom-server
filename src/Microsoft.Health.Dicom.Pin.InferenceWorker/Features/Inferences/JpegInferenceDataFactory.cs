// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Pin.Core.Features.Inputs;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;

public class JpegInferenceDataFactory : IInferenceDataFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public JpegInferenceDataFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
    }

    public InferenceDataType InferenceDataType { get; } = InferenceDataType.Jpeg;
    public Task<Stream> GetDataAsync(DicomInput input, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<T> CreateAsync<T>(Stream stream, CancellationToken cancellationToken) => throw new NotImplementedException();

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
        };
    }
}

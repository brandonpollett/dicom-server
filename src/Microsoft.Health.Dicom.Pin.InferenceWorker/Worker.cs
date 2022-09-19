// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker;

public class Worker : BackgroundService
{
    private readonly IMetadataStore _metadataStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IInferenceStore _inferenceStore;

    public Worker(IMetadataStore metadataStore, IInferenceStore inferenceStore, IHttpClientFactory httpClientFactory)
    {
        _metadataStore = EnsureArg.IsNotNull(metadataStore, nameof(metadataStore));
        _inferenceStore = EnsureArg.IsNotNull(inferenceStore, nameof(inferenceStore));
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            InferenceRequest inferenceRequest = await _inferenceStore.GetRequestAsync(cancellationToken: stoppingToken);

            if (inferenceRequest != null)
            {
                Inference inferenceItem = await _metadataStore.GetInferenceAsync(inferenceRequest.InferenceId, stoppingToken);

                using HttpClient client = _httpClientFactory.CreateClient();
                using var content = new StringContent(string.Empty);
                HttpResponseMessage response = await client.PostAsync(inferenceItem.Uri, content, stoppingToken);

                var inferenceResponse = new InferenceResponse
                {
                    AccountId = inferenceRequest.AccountId,
                    InferenceId = inferenceRequest.InferenceId,
                    InstanceUid = inferenceRequest.InstanceUid,
                    SeriesUid = inferenceRequest.SeriesUid,
                    StudyUid = inferenceRequest.StudyUid,
                    StatusCode = response.StatusCode.ToString(),
                };

                await _inferenceStore.WriteResponseAsync(inferenceResponse, stoppingToken);

                await _inferenceStore.CompleteRequestAsync(inferenceRequest, stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

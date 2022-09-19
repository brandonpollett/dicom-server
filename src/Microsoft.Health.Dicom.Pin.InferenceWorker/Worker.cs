// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.ServiceBus;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker;

public class Worker : BackgroundService
{
    private readonly IMetadataStore _metadataStore;
    private readonly ServiceBusReceiver _inferenceReceiver;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceBusSender _inferenceResponder;

    public Worker(ServiceBusClient serviceBusClient, IMetadataStore metadataStore, IHttpClientFactory httpClientFactory)
    {
        EnsureArg.IsNotNull(serviceBusClient, nameof(serviceBusClient));
        _metadataStore = EnsureArg.IsNotNull(metadataStore, nameof(metadataStore));
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));

        _inferenceReceiver = serviceBusClient.CreateReceiver("InferenceRequest");
        _inferenceResponder = serviceBusClient.CreateSender("InferenceResponse");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ServiceBusReceivedMessage item = await _inferenceReceiver.ReceiveMessageAsync(cancellationToken: stoppingToken);

            if (item != null)
            {
                var inferenceRequest = item.Body.ToObjectFromJson<InferenceRequest>();

                var inferenceItem = await _metadataStore.GetInferenceAsync(inferenceRequest.InferenceId, stoppingToken);

                using HttpClient client = _httpClientFactory.CreateClient();
                using var content = new StringContent(string.Empty);
                var response = await client.PostAsync(inferenceItem.Uri, content, stoppingToken);

                var inferenceResponse = new InferenceResponse
                {
                    AccountId = inferenceRequest.AccountId,
                    InferenceId = inferenceRequest.InferenceId,
                    InstanceUid = inferenceRequest.InstanceUid,
                    SeriesUid = inferenceRequest.SeriesUid,
                    StudyUid = inferenceRequest.StudyUid,
                    StatusCode = response.StatusCode.ToString(),
                };

                await _inferenceResponder.SendMessageAsync(new ServiceBusMessage(BinaryData.FromObjectAsJson(inferenceResponse)), stoppingToken);

                await _inferenceReceiver.CompleteMessageAsync(item, stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

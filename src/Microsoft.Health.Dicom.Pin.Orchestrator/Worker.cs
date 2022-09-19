// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Messaging.ServiceBus;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.Orchestrator;

public class Worker : BackgroundService
{
    private readonly ServiceBusReceiver _orchestratorReceiver;
    private readonly ServiceBusSender _inferenceRequester;
    private readonly IMetadataStore _metadataStore;

    public Worker(ServiceBusClient serviceBusClient, IMetadataStore metadataStore)
    {
        EnsureArg.IsNotNull(serviceBusClient, nameof(serviceBusClient));
        _metadataStore = EnsureArg.IsNotNull(metadataStore, nameof(metadataStore));

        _orchestratorReceiver = serviceBusClient.CreateReceiver("OrchestratorRequest");
        _inferenceRequester = serviceBusClient.CreateSender("InferenceRequest");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ServiceBusReceivedMessage receivedMessage = await _orchestratorReceiver.ReceiveMessageAsync(cancellationToken: stoppingToken);

            if (receivedMessage != null)
            {
                OrchestratorRequest orchestratorRequest = receivedMessage.Body.ToObjectFromJson<OrchestratorRequest>();

                IEnumerable<Inference> inferences = await _metadataStore.GetInferencesAsync(orchestratorRequest.AccountId, stoppingToken);

                var inferenceRequests = inferences.Select(inference => new InferenceRequest
                {
                    AccountId = orchestratorRequest.AccountId,
                    InferenceId = inference.Id,
                    StudyUid = orchestratorRequest.StudyUid,
                    SeriesUid = orchestratorRequest.SeriesUid,
                    InstanceUid = orchestratorRequest.InstanceUid,
                })
                .Select(inferenceRequest => new ServiceBusMessage(BinaryData.FromObjectAsJson(inferenceRequest)))
                .ToList();

                await _inferenceRequester.SendMessagesAsync(inferenceRequests, stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

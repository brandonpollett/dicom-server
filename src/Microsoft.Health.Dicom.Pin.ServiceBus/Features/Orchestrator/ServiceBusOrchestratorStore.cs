// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using EnsureThat;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.ServiceBus.Features.Orchestrator;

public class ServiceBusOrchestratorStore : IOrchestratorStore
{
    private readonly ServiceBusReceiver _orchestratorReceiver;
    private readonly Dictionary<string, ServiceBusReceivedMessage> _processingMessages = new();

    public ServiceBusOrchestratorStore(ServiceBusClient serviceBusClient)
    {
        EnsureArg.IsNotNull(serviceBusClient, nameof(serviceBusClient));

        _orchestratorReceiver = serviceBusClient.CreateReceiver("OrchestratorRequest");
    }

    public async Task<OrchestratorRequest> GetRequestAsync(CancellationToken cancellationToken)
    {
        ServiceBusReceivedMessage receivedMessage = await _orchestratorReceiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
        OrchestratorRequest orchestratorRequest = null;

        if (receivedMessage == null)
        {
            return null;
        }

        orchestratorRequest = receivedMessage.Body.ToObjectFromJson<OrchestratorRequest>();
        orchestratorRequest.MessageId = receivedMessage.MessageId;
        _processingMessages.TryAdd(receivedMessage.MessageId, receivedMessage);

        return orchestratorRequest;
    }

    public async Task CompleteRequestAsync(OrchestratorRequest request, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(request, nameof(request));

        if (_processingMessages.TryGetValue(request.MessageId, out ServiceBusReceivedMessage message))
        {
            await _orchestratorReceiver.CompleteMessageAsync(message, cancellationToken);
        }
    }
}

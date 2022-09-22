// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using EnsureThat;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.ServiceBus.Features.Inference;

public class ServiceBusInferenceStore : IInferenceStore
{
    private readonly ServiceBusReceiver _inferenceRequestReceiver;
    private readonly Dictionary<string, ServiceBusReceivedMessage> _processingMessages = new();
    private readonly ServiceBusSender _inferenceRequestSender;
    private readonly ServiceBusReceiver _inferenceResponseReceiver;
    private readonly ServiceBusSender _inferenceResponseSender;

    public ServiceBusInferenceStore(ServiceBusClient serviceBusClient)
    {
        EnsureArg.IsNotNull(serviceBusClient, nameof(serviceBusClient));

        _inferenceRequestReceiver = serviceBusClient.CreateReceiver("InferenceRequest");
        _inferenceRequestSender = serviceBusClient.CreateSender("InferenceRequest");
        _inferenceResponseReceiver = serviceBusClient.CreateReceiver("InferenceResponse");
        _inferenceResponseSender = serviceBusClient.CreateSender("InferenceResponse");
    }

    public async Task WriteRequestsAsync(IEnumerable<InferenceRequest> inferenceRequests, CancellationToken cancellationToken)
    {
        IEnumerable<ServiceBusMessage> messages = inferenceRequests.Select(x => new ServiceBusMessage(BinaryData.FromObjectAsJson(x)));

        await _inferenceRequestSender.SendMessagesAsync(messages, cancellationToken);
    }

    public async Task WriteRequestAsync(InferenceRequest inferenceRequest, CancellationToken cancellationToken)
    {
        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(inferenceRequest));

        await _inferenceRequestSender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async Task<InferenceRequest> GetRequestAsync(CancellationToken cancellationToken)
    {
        ServiceBusReceivedMessage receivedMessage = await _inferenceRequestReceiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
        InferenceRequest inferenceRequest = null;

        if (receivedMessage == null)
        {
            return null;
        }

        inferenceRequest = receivedMessage.Body.ToObjectFromJson<InferenceRequest>();
        inferenceRequest.MessageId = receivedMessage.MessageId;
        _processingMessages.TryAdd(receivedMessage.MessageId, receivedMessage);

        return inferenceRequest;
    }

    public async Task DeadLetterRequestAsync(InferenceRequest inferenceRequest, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(inferenceRequest, nameof(inferenceRequest));

        if (_processingMessages.TryGetValue(inferenceRequest.MessageId, out ServiceBusReceivedMessage message))
        {
            await _inferenceRequestReceiver.DeadLetterMessageAsync(message, cancellationToken: cancellationToken);
        }
    }

    public async Task CompleteRequestAsync(InferenceRequest request, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(request, nameof(request));

        if (_processingMessages.TryGetValue(request.MessageId, out ServiceBusReceivedMessage message))
        {
            await _inferenceRequestReceiver.CompleteMessageAsync(message, cancellationToken);
        }
    }

    public async Task WriteResponseAsync(InferenceResponse inferenceResponse, CancellationToken cancellationToken)
    {
        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(inferenceResponse));

        await _inferenceResponseSender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async Task<InferenceResponse> GetResponseAsync(CancellationToken cancellationToken)
    {
        ServiceBusReceivedMessage receivedMessage = await _inferenceResponseReceiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
        InferenceResponse inferenceResponse = null;

        if (receivedMessage == null)
        {
            return null;
        }

        inferenceResponse = receivedMessage.Body.ToObjectFromJson<InferenceResponse>();
        inferenceResponse.MessageId = receivedMessage.MessageId;
        _processingMessages.TryAdd(receivedMessage.MessageId, receivedMessage);

        return inferenceResponse;
    }

    public async Task DeadLetterResponseAsync(InferenceResponse inferenceResponse, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(inferenceResponse, nameof(inferenceResponse));

        if (_processingMessages.TryGetValue(inferenceResponse.MessageId, out ServiceBusReceivedMessage message))
        {
            await _inferenceResponseReceiver.DeadLetterMessageAsync(message, cancellationToken: cancellationToken);
        }
    }

    public async Task CompleteResponseAsync(InferenceResponse request, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(request, nameof(request));

        if (_processingMessages.TryGetValue(request.MessageId, out ServiceBusReceivedMessage message))
        {
            await _inferenceResponseReceiver.CompleteMessageAsync(message, cancellationToken);
        }
    }
}

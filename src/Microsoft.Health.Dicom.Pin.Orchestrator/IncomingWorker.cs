// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.Orchestrator;

public class IncomingWorker : BackgroundService
{
    private readonly IMetadataStore _metadataStore;
    private readonly IOrchestratorStore _orchestratorStore;
    private readonly ILogger<IncomingWorker> _logger;
    private readonly IInferenceStore _inferenceStore;

    public IncomingWorker(IMetadataStore metadataStore, IOrchestratorStore orchestratorStore, IInferenceStore inferenceStore, ILogger<IncomingWorker> logger)
    {
        _metadataStore = EnsureArg.IsNotNull(metadataStore, nameof(metadataStore));
        _orchestratorStore = EnsureArg.IsNotNull(orchestratorStore, nameof(orchestratorStore));
        _inferenceStore = EnsureArg.IsNotNull(inferenceStore, nameof(inferenceStore));
        _logger = EnsureArg.IsNotNull(logger, nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            OrchestratorRequest orchestratorRequest = await _orchestratorStore.GetRequestAsync(stoppingToken);

            if (orchestratorRequest != null)
            {
                IEnumerable<Inference> inferences = await _metadataStore.GetInferencesAsync(orchestratorRequest.AccountId, stoppingToken);

                var inferenceRequests = inferences.Select(inference => new InferenceRequest
                {
                    AccountId = orchestratorRequest.AccountId,
                    InferenceId = inference.Id,
                    RequestProperties = orchestratorRequest.RequestProperties,
                })
                .ToList();

                if (inferenceRequests.Any())
                {
                    await _inferenceStore.WriteRequestsAsync(inferenceRequests, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("No inferences found for account {AccountId}", orchestratorRequest.AccountId);
                }


                _logger.LogInformation("Successfully executed orchestration for {AccountId} on {MessageId}", orchestratorRequest.AccountId, orchestratorRequest.MessageId);
                await _orchestratorStore.CompleteRequestAsync(orchestratorRequest, stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

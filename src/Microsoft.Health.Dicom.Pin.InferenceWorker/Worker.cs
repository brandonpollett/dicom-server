// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker;

public class Worker : BackgroundService
{
    private readonly IMetadataStore _metadataStore;
    private readonly IInferenceStore _inferenceStore;
    private readonly Dictionary<OrchestratorSourceType, IInputFactory> _inputFactories;
    private readonly Dictionary<InferenceInputType, IInferenceFactory> _inferenceFactories;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IMetadataStore metadataStore,
        IInferenceStore inferenceStore,
        IEnumerable<IInputFactory> inputFactories,
        IEnumerable<IInferenceFactory> inferenceFactories, ILogger<Worker> logger)
    {
        _metadataStore = EnsureArg.IsNotNull(metadataStore, nameof(metadataStore));
        _inferenceStore = EnsureArg.IsNotNull(inferenceStore, nameof(inferenceStore));
        EnsureArg.IsNotNull(inputFactories, nameof(inputFactories));
        EnsureArg.IsNotNull(inferenceFactories, nameof(inferenceFactories));
        _logger = EnsureArg.IsNotNull(logger, nameof(logger));

        _inputFactories = new Dictionary<OrchestratorSourceType, IInputFactory>();

        foreach (IInputFactory inputFactory in inputFactories)
        {
            _inputFactories.Add(inputFactory.OrchestratorSourceType, inputFactory);
        }

        _inferenceFactories = new Dictionary<InferenceInputType, IInferenceFactory>();

        foreach (IInferenceFactory inferenceFactory in inferenceFactories)
        {
            _inferenceFactories.Add(inferenceFactory.InferenceInputType, inferenceFactory);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            InferenceRequest inferenceRequest = await _inferenceStore.GetRequestAsync(cancellationToken: stoppingToken);

            if (inferenceRequest != null)
            {
                try
                {
                    Inference inferenceItem = await _metadataStore.GetInferenceAsync(inferenceRequest.InferenceId, stoppingToken);

                    if (!_inputFactories.TryGetValue(inferenceRequest.RequestProperties.OrchestratorSourceType, out IInputFactory inputFactory))
                    {
                        throw new ArgumentOutOfRangeException($"No input factory for the type of {inferenceRequest.RequestProperties.OrchestratorSourceType}");
                    }

                    if (!_inferenceFactories.TryGetValue(inferenceItem.InferenceInputType, out IInferenceFactory inferenceFactory))
                    {
                        throw new ArgumentOutOfRangeException($"No inference factory for the type of {inferenceItem.InferenceInputType}");
                    }

                    DicomInput input = await inputFactory.RetrieveAsync(inferenceRequest.RequestProperties, stoppingToken);

                    InferenceResponse inferenceResponse = await inferenceFactory.ExecuteInferenceAsync(input, inferenceRequest, inferenceItem, stoppingToken);

                    await _inferenceStore.WriteResponseAsync(inferenceResponse, stoppingToken);

                    await _inferenceStore.CompleteRequestAsync(inferenceRequest, stoppingToken);
                    _logger.LogInformation("Successfully executed inference for {InferenceId} on {MessageId}", inferenceItem.Id, inferenceRequest.MessageId);
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    _logger.LogError(ex, "Exception encountered while doing inference worker for {InferenceId} on {MessageId}", inferenceRequest.InferenceId, inferenceRequest.MessageId);
                    await _inferenceStore.DeadLetterRequestAsync(inferenceRequest, stoppingToken);
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

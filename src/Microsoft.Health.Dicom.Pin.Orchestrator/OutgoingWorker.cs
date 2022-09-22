// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Features.Outputs;
using Microsoft.Health.Dicom.Pin.Core.Features.TempFiles;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.Orchestrator;

public class OutgoingWorker : BackgroundService
{
    private readonly IInferenceStore _inferenceStore;
    private readonly ITempFileStore _tempFileStore;
    private readonly ILogger<OutgoingWorker> _logger;
    private readonly Dictionary<OrchestratorDataType, IOutputFactory> _outputFactories;

    public OutgoingWorker(
        IInferenceStore inferenceStore,
        ITempFileStore tempFileStore,
        IEnumerable<IOutputFactory> outputFactories,
        ILogger<OutgoingWorker> logger)
    {
        _inferenceStore = EnsureArg.IsNotNull(inferenceStore, nameof(inferenceStore));
        _tempFileStore = EnsureArg.IsNotNull(tempFileStore, nameof(tempFileStore));
        EnsureArg.IsNotNull(outputFactories, nameof(outputFactories));
        _logger = EnsureArg.IsNotNull(logger, nameof(logger));

        _outputFactories = new Dictionary<OrchestratorDataType, IOutputFactory>();
        foreach (IOutputFactory outputFactory in outputFactories)
        {
            _outputFactories.Add(outputFactory.OrchestratorDataType, outputFactory);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            InferenceResponse inferenceResponse = await _inferenceStore.GetResponseAsync(stoppingToken);

            if (inferenceResponse != null)
            {
                try
                {
                    //TODO: Today we know the result is a PNG file, will need to code this up to be dynamic in the future
                    Stream result = await _tempFileStore.RetrieveAsync(inferenceResponse.FileName, stoppingToken);

                    // We know we only support UpsRs outputs today
                    IOutputFactory outputFactory = _outputFactories[OrchestratorDataType.UpsRs];

                    await outputFactory.WriteAsync(inferenceResponse.RequestProperties, result, stoppingToken);

                    _logger.LogInformation("Successfully executed orchestration for {AccountId} on {MessageId}", inferenceResponse.AccountId, inferenceResponse.MessageId);
                    await _inferenceStore.CompleteResponseAsync(inferenceResponse, stoppingToken);
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    await _inferenceStore.DeadLetterResponseAsync(inferenceResponse, stoppingToken);
                    _logger.LogError(ex, "Failed");
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

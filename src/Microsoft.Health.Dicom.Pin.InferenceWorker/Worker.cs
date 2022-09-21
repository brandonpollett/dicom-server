// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Features.TempFiles;
using Microsoft.Health.Dicom.Pin.Core.Messages;
using Microsoft.Health.Dicom.Pin.Core.Models;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;
using Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker;

public class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMetadataStore _metadataStore;
    private readonly IInferenceStore _inferenceStore;
    private readonly ITempFileStore _tempFileStore;
    private readonly Dictionary<OrchestratorDataType, IInputFactory> _inputFactories;
    private readonly Dictionary<InferenceDataType, IInferenceDataFactory> _inferenceDataFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IHttpClientFactory httpClientFactory,
        IMetadataStore metadataStore,
        IInferenceStore inferenceStore,
        ITempFileStore tempFileStore,
        IEnumerable<IInputFactory> inputFactories,
        IEnumerable<IInferenceDataFactory> inferenceFactories,
        ILogger<Worker> logger)
    {
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
        _metadataStore = EnsureArg.IsNotNull(metadataStore, nameof(metadataStore));
        _inferenceStore = EnsureArg.IsNotNull(inferenceStore, nameof(inferenceStore));
        _tempFileStore = EnsureArg.IsNotNull(tempFileStore, nameof(tempFileStore));
        EnsureArg.IsNotNull(inputFactories, nameof(inputFactories));
        EnsureArg.IsNotNull(inferenceFactories, nameof(inferenceFactories));
        _logger = EnsureArg.IsNotNull(logger, nameof(logger));

        _inputFactories = new Dictionary<OrchestratorDataType, IInputFactory>();
        foreach (IInputFactory inputFactory in inputFactories)
        {
            _inputFactories.Add(inputFactory.OrchestratorDataType, inputFactory);
        }

        _inferenceDataFactory = new Dictionary<InferenceDataType, IInferenceDataFactory>();
        foreach (IInferenceDataFactory inferenceFactory in inferenceFactories)
        {
            _inferenceDataFactory.Add(inferenceFactory.InferenceDataType, inferenceFactory);
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

                    if (!_inputFactories.TryGetValue(inferenceRequest.RequestProperties.OrchestratorDataType, out IInputFactory inputFactory))
                    {
                        throw new ArgumentOutOfRangeException($"No input factory for the type of {inferenceRequest.RequestProperties.OrchestratorDataType}");
                    }

                    if (!_inferenceDataFactory.TryGetValue(inferenceItem.InferenceInputDataType, out IInferenceDataFactory inferenceInputFactory))
                    {
                        throw new ArgumentOutOfRangeException($"No inference factory for the type of {inferenceItem.InferenceInputDataType}");
                    }

                    if (!_inferenceDataFactory.TryGetValue(inferenceItem.InferenceOutputDataType, out IInferenceDataFactory inferenceOutputFactory))
                    {
                        throw new ArgumentOutOfRangeException($"No inference factory for the type of {inferenceItem.InferenceOutputDataType}");
                    }

                    DicomInput input = await inputFactory.RetrieveAsync(inferenceRequest.RequestProperties, stoppingToken);

                    var data = await inferenceInputFactory.GetDataAsync(input, stoppingToken);

                    using HttpClient client = _httpClientFactory.CreateClient();

                    byte[] buffer = new byte[data.Length];

                    string fileName;
                    using (var multipartFormContent = new MultipartFormDataContent())
                    {
                        //Load the file and set the file's Content-Type header
                        using var fileStreamContent = new StreamContent(data);
                        //fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        //Add the file
                        multipartFormContent.Add(fileStreamContent, name: "image", fileName: "example.dcm");

                        //Send it
                        var response = await client.PostAsync(inferenceItem.Uri, multipartFormContent, cancellationToken: stoppingToken);

                        fileName = await _tempFileStore.Save(await response.Content.ReadAsStreamAsync(stoppingToken), stoppingToken);
                    }
                    // _ = await data.ReadAsync(buffer, stoppingToken);
                    // using var content = JsonContent.Create(new
                    // {
                    //     Image = buffer
                    // });
                    //
                    // using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, inferenceItem.Uri);
                    // request.Headers.Add("Accept", "application/json");
                    // request.Content = content;
                    // HttpResponseMessage response = await client.SendAsync(request, stoppingToken);



                    var inferenceResponse = new InferenceResponse
                    {
                        AccountId = inferenceRequest.AccountId,
                        FileName = fileName,
                        InferenceId = inferenceRequest.InferenceId,
                        RequestProperties = inferenceRequest.RequestProperties,
                        OutputDataType = inferenceItem.InferenceOutputDataType,
                    };

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

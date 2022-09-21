// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using FellowOakDicom;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inputs;

public class UpsRsInputFactory : IInputFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UpsRsInputFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
    }

    public OrchestratorSourceType OrchestratorSourceType { get; } = OrchestratorSourceType.UpsRs;

    public async Task<DicomInput> RetrieveAsync(UpsRsRequestProperties requestProperties, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(requestProperties, nameof(requestProperties));

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = requestProperties.ServerAddress;

        var dicomWebClient = new DicomWebClient(httpClient);

        DicomWebResponse<DicomDataset> workItemResponse = await dicomWebClient.RetrieveWorkitemAsync(requestProperties.InstanceId, cancellationToken: cancellationToken);

        if (!workItemResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Unable to obtain the ups rs work item");
        }

        DicomDataset workItem = await workItemResponse.GetValueAsync();

        DicomSequence inputSequence = workItem.GetSequence(DicomTag.InputInformationSequence);

        var wadoSequences = inputSequence.FirstOrDefault()?.GetSequence(DicomTag.WADORetrievalSequence);

        if (wadoSequences == null)
        {
            throw new InvalidOperationException("Unable to obtain any WADORetrievalSequence");
        }

        var retrieveUrls = wadoSequences.Items.Select(i => new Uri(i.GetString(DicomTag.RetrieveURI))).ToList();

        if (!retrieveUrls.Any())
        {
            throw new InvalidOperationException("No retrieve urls found in the ups rs work item");
        }

        DicomWebAsyncEnumerableResponse<DicomFile> dicomFileResponse = await dicomWebClient.RetrieveInstancesAsync(retrieveUrls[0], "*", cancellationToken);

        if (!dicomFileResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Unable to obtain the dicom file");
        }

        var enumerator = dicomFileResponse.GetAsyncEnumerator(cancellationToken);
        await enumerator.MoveNextAsync();
        DicomFile dicomFile = enumerator.Current;
        return new DicomInput { Dataset = dicomFile.Dataset };
    }
}

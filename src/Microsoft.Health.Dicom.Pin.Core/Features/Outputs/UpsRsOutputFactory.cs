// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Buffer;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Pin.Core.Features.TempFiles;
using Microsoft.Health.Dicom.Pin.Core.Messages;

namespace Microsoft.Health.Dicom.Pin.Core.Features.Outputs;

public class UpsRsOutputFactory : IOutputFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITempFileStore _tempFileStore;
    public OrchestratorDataType OrchestratorDataType { get; } = OrchestratorDataType.UpsRs;

    public UpsRsOutputFactory(IHttpClientFactory httpClientFactory, ITempFileStore tempFileStore)
    {
        _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
        _tempFileStore = EnsureArg.IsNotNull(tempFileStore, nameof(tempFileStore));
    }

    public async Task WriteAsync(UpsRsRequestProperties requestProperties, Stream result, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(requestProperties, nameof(requestProperties));
        EnsureArg.IsNotNull(result, nameof(result));

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = requestProperties.ServerAddress;

        var dicomWebClient = new DicomWebClient(httpClient);

        DicomWebResponse<DicomDataset> workItemResponse = await dicomWebClient.RetrieveWorkitemAsync(requestProperties.InstanceId, cancellationToken: cancellationToken);

        if (!workItemResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Unable to obtain the ups rs work item");
        }

        DicomDataset workItem = await workItemResponse.GetValueAsync();

        DicomSequence inputSequence = workItem.GetSequence(DicomTag.OutputDestinationSequence);

        var stowSequences = inputSequence.FirstOrDefault()?.GetSequence(DicomTag.STOWRSStorageSequence);

        if (stowSequences == null)
        {
            throw new InvalidOperationException("Unable to obtain any WADORetrievalSequence");
        }

        var stowUrls = stowSequences.Items.Select(i => new Uri(i.GetString(DicomTag.StorageURL))).ToList();

        if (!stowUrls.Any())
        {
            throw new InvalidOperationException("No retrieve urls found in the ups rs work item");
        }

        var referencedRequest = workItem.GetSequence(DicomTag.ReferencedRequestSequence);
        var referencedStudyInstanceUid = referencedRequest.Items.Select(i => i.GetString(DicomTag.StudyInstanceUID)).FirstOrDefault();

        var ds = new DicomDataset(DicomTransferSyntax.ExplicitVRLittleEndian);
        ds.Add(DicomTag.StudyInstanceUID, referencedStudyInstanceUid);
        ds.Add(DicomTag.SeriesInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID());
        ds.Add(DicomTag.SOPInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID());
        ds.Add(DicomTag.SOPClassUID, DicomUIDGenerator.GenerateDerivedFromUUID());
        ds.Add(DicomTag.PatientID, "123456");
        ds.Add(DicomTag.PatientName, workItem.GetString(DicomTag.PatientName));
        ds.Add(DicomTag.Modality, "AIINFERENCE");
        ds.Add(DicomTag.StudyDate, "20211204");
        ds.Add(DicomTag.ContentDate, "20211204");
        ds.Add(DicomTag.PixelSpacing, 1m, 1m);
        ds.Add(DicomTag.SliceThickness, 1m);
        ds.Add(DicomTag.Rows, (ushort)256);
        ds.Add(DicomTag.Columns, (ushort)256);
        ds.Add(DicomTag.NumberOfFrames, 1);
        ds.Add(DicomTag.BitsStored, (ushort)8);
        ds.Add(DicomTag.BitsAllocated, (ushort)8);
        ds.Add(DicomTag.HighBit, (ushort)7);
        ds.Add(DicomTag.PixelRepresentation, (ushort)0);
        ds.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");

        DicomPixelData pixelData = DicomPixelData.Create(ds, true);
        pixelData.BitsStored = 8;
        pixelData.SamplesPerPixel = 1;
        pixelData.HighBit = 7;
        pixelData.PixelRepresentation = 0;

        pixelData.Height = 256;
        pixelData.Width = 256;
        pixelData.PhotometricInterpretation = PhotometricInterpretation.Monochrome2;

        Int32 length = result.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(result.Length);
        Byte[] buffer = new Byte[length];

        _ = await result.ReadAsync(buffer.AsMemory(0, length), cancellationToken);

        MemoryByteBuffer memoryByteBuffer = new MemoryByteBuffer(buffer);
        pixelData.AddFrame(memoryByteBuffer);
        DicomFile dicomFile = new DicomFile(ds);
        dicomFile.FileMetaInfo.TransferSyntax = DicomTransferSyntax.ExplicitVRLittleEndian;

        await dicomWebClient.StoreAsync(dicomFile, referencedStudyInstanceUid, cancellationToken: cancellationToken);
    }
}

// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.Routing;
using Microsoft.Health.Dicom.Core.Features.Workitem;

namespace Microsoft.Health.Dicom.Core.Features;
public class AutoInferenceInitiator : IAutoInferenceInitiator
{
    private readonly IUrlResolver _urlResolver;
    private readonly IWorkitemService _workitemService;

    public AutoInferenceInitiator(IUrlResolver urlResolver, IWorkitemService workitemService)
    {
        _urlResolver = urlResolver;
        _workitemService = workitemService;
    }

    public async void QueueInferenceRequest(DicomDataset dicomDataset)
    {
        // todo check modality and body part before calling below
        EnsureArg.IsNotNull(dicomDataset);
        string workItemInstanceUid = DicomUID.Generate().UID;
        await _workitemService.ProcessAddAsync(CreateWorkItemDataset(dicomDataset), workItemInstanceUid, cancellationToken: CancellationToken.None);

        // todo queue a message with the WI Id
    }

    private DicomDataset CreateWorkItemDataset(DicomDataset inputDataset)
    {
        var ds = new DicomDataset(DicomTransferSyntax.ExplicitVRLittleEndian);

        ds = ds.NotValidated();
        ds.Add(DicomTag.TransactionUID, string.Empty);

        ds.Add(DicomTag.SOPClassUID, DicomUID.Generate().UID);
        ds.Add(DicomTag.SOPInstanceUID, DicomUID.Generate().UID);
        ds.Add(new DicomSequence(DicomTag.ScheduledProcessingParametersSequence));
        ds.Add(new DicomSequence(DicomTag.ScheduledStationNameCodeSequence));
        ds.Add(new DicomSequence(DicomTag.ScheduledStationClassCodeSequence));
        ds.Add(new DicomSequence(DicomTag.ScheduledStationGeographicLocationCodeSequence));
        ds.Add(new DicomSequence(DicomTag.IssuerOfPatientIDQualifiersSequence));
        ds.Add(DicomTag.IssuerOfAdmissionIDSequence, new DicomDataset());
        ds.Add(DicomTag.AdmittingDiagnosesCodeSequence, new DicomDataset
            {
                { DicomTag.CodeMeaning, "CodeMeaning" }
            });
        ds.Add(new DicomSequence(DicomTag.ReferencedRequestSequence));
        ds.Add(new DicomSequence(DicomTag.InputInformationSequence));
        ds.Add(DicomTag.PatientName, inputDataset.GetString(DicomTag.PatientName));
        ds.Add(DicomTag.IssuerOfPatientID, string.Empty);
        ds.Add(new DicomSequence(DicomTag.OtherPatientIDsSequence));
        ds.Add(DicomTag.PatientBirthDate, DateTime.Now);
        ds.Add(DicomTag.PatientSex, "F");
        ds.Add(DicomTag.AdmissionID, "1");
        ds.Add(DicomTag.AdmittingDiagnosesDescription, "SampleDiagnosesDescription");
        ds.Add(DicomTag.ProcedureStepState, "SCHEDULED");
        ds.Add(new DicomSequence(DicomTag.ProcedureStepProgressInformationSequence));
        ds.Add(new DicomSequence(DicomTag.UnifiedProcedureStepPerformedProcedureSequence));


        ds.Add(DicomTag.ScheduledProcedureStepModificationDateTime, DateTime.Now);
        ds.Add(DicomTag.ProcedureStepLabel, Guid.NewGuid().ToString("N"));
        ds.Add(DicomTag.WorklistLabel, "WorklistLabel");
        ds.Add(DicomTag.ScheduledProcedureStepStartDateTime, DateTime.Now);
        ds.Add(DicomTag.CommentsOnTheScheduledProcedureStep, "Comments");
        ds.Add(new DicomSequence(DicomTag.ProcedureStepDiscontinuationReasonCodeSequence));
        ds.Add(new DicomSequence(DicomTag.ActualHumanPerformersSequence));
        ds.Add(new DicomSequence(DicomTag.HumanPerformerCodeSequence));
        ds.Add(DicomTag.HumanPerformerName, "AI");
        ds.Add(DicomTag.TypeOfInstances, "SAMPLETYPEOFINST");
        ds.Add(DicomTag.ReferencedSOPSequence, new DicomDataset
        {
            { DicomTag.ReferencedSOPClassUID, inputDataset.GetString(DicomTag.SOPClassUID) },
            { DicomTag.ReferencedSOPInstanceUID, inputDataset.GetString(DicomTag.SOPInstanceUID) },
        });

        // add ups-rs fields needed for AI
        ds.Add(DicomTag.InputReadinessState, "READY");
        ds.Add(DicomTag.ScheduledProcedureStepPriority, "Normal");
        ds.Add(new DicomSequence(DicomTag.ScheduledWorkitemCodeSequence));

        var instanceIdentifier = inputDataset.ToInstanceIdentifier();
        var wadoRSUrl = _urlResolver.ResolveRetrieveInstanceUri(instanceIdentifier);
        var stowUrl = _urlResolver.ResolveRetrieveStudyUri(instanceIdentifier.StudyInstanceUid);

        DicomDataset inputdataSet = new DicomDataset();
        DicomDataset inputwadodataSet = new DicomDataset();
        inputwadodataSet.Add(DicomTag.RetrieveURI, wadoRSUrl);
        inputdataSet.Add(DicomTag.WADORetrievalSequence, inputwadodataSet);
        ds.Add(new DicomSequence(DicomTag.InputInformationSequence, inputdataSet));

        DicomDataset outputdataSet = new DicomDataset();
        DicomDataset outputstowataSet = new DicomDataset();
        outputstowataSet.Add(DicomTag.StorageURL, stowUrl);
        outputdataSet.Add(DicomTag.STOWRSStorageSequence, outputstowataSet);
        ds.Add(new DicomSequence(DicomTag.OutputDestinationSequence, inputdataSet));

        return ds;
    }
}
// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Dicom;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.Core.Models;

namespace Microsoft.Health.Dicom.Elastic.Features.Store
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA5400", Justification = "Not production code.")]
    public sealed class ElasticIndexDataStore : IIndexDataStore, IDisposable
    {
        private HttpClient _httpClientFactory;
        private HttpClientHandler _handler;

        public ElasticIndexDataStore()
        {
            _handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClientFactory = new HttpClient(_handler);
        }

        public async Task<long> CreateInstanceIndexAsync(DicomDataset dicomDataset, IEnumerable<QueryTag> queryTags,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
            EnsureArg.IsNotNull(queryTags, nameof(queryTags));

            var content = new
            {
                StudyInstanceUID = dicomDataset.GetString(DicomTag.StudyInstanceUID),
                SeriesInstanceUID= dicomDataset.GetString(DicomTag.SeriesInstanceUID),
                SOPInstanceUID=dicomDataset.GetString(DicomTag.SOPInstanceUID),
                PatientID=dicomDataset.GetSingleValueOrDefault<string>(DicomTag.PatientID),
                PatientName=dicomDataset.GetSingleValueOrDefault<string>(DicomTag.PatientName),
                ReferringPhysicianName= dicomDataset.GetSingleValueOrDefault<string>(DicomTag.ReferringPhysicianName),
                StudyDate= dicomDataset.GetStringDateAsDate(DicomTag.StudyDate),
                StudyDescription=dicomDataset.GetSingleValueOrDefault<string>(DicomTag.StudyDescription),
                AccessionNumber= dicomDataset.GetSingleValueOrDefault<string>(DicomTag.AccessionNumber),
                Modality= dicomDataset.GetSingleValueOrDefault<string>(DicomTag.Modality),
                PerformedProcedureStepStartDate=dicomDataset.GetStringDateAsDate(DicomTag.PerformedProcedureStepStartDate),
            };

            _httpClientFactory.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic","YWRtaW46YWRtaW4=");

            var result = await _httpClientFactory.PostAsJsonAsync(new Uri($"https://localhost:9200/instance/_doc/{Guid.NewGuid()}"), content, cancellationToken);

            Console.WriteLine(await result.Content.ReadAsStringAsync(cancellationToken));

            return 1;
        }

        public Task DeleteStudyIndexAsync(string studyInstanceUid, DateTimeOffset cleanupAfter,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteSeriesIndexAsync(string studyInstanceUid, string seriesInstanceUid, DateTimeOffset cleanupAfter,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteInstanceIndexAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid,
            DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateInstanceIndexStatusAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, IndexStatus status,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> RetrieveDeletedInstancesAsync(int batchSize, int maxRetries, CancellationToken cancellationToken = default)
        {
            return (Task<IEnumerable<VersionedInstanceIdentifier>>) Task.CompletedTask;
        }

        public Task DeleteDeletedInstanceAsync(VersionedInstanceIdentifier versionedInstanceIdentifier,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<int> IncrementDeletedInstanceRetryAsync(VersionedInstanceIdentifier versionedInstanceIdentifier,
            DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            return (Task<int>) Task.CompletedTask;
        }

        public void Dispose()
        {
            _httpClientFactory?.Dispose();
            _handler?.Dispose();
        }
    }
}

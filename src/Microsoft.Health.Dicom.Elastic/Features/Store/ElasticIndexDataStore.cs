// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Dicom;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.Core.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Dicom.Elastic.Features.Store
{
    public class ElasticIndexDataStore : IIndexDataStore
    {
        private IHttpClientFactory _httpClientFactory;

        public ElasticIndexDataStore(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
        }

        public async Task<long> CreateInstanceIndexAsync(DicomDataset dicomDataset, IEnumerable<QueryTag> queryTags,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
            EnsureArg.IsNotNull(queryTags, nameof(queryTags));

            JObject jObject = new JObject
            {
                {"StudyInstanceUid", dicomDataset.GetString(DicomTag.StudyInstanceUID)},
                {"SeriesInstanceUID", dicomDataset.GetString(DicomTag.SeriesInstanceUID)},
                {"SOPInstanceUID",dicomDataset.GetString(DicomTag.SOPInstanceUID)},
                {"PatientID",dicomDataset.GetSingleValueOrDefault<string>(DicomTag.PatientID)},
                {"PatientName",dicomDataset.GetSingleValueOrDefault<string>(DicomTag.PatientName)},
                {"ReferringPhysicianName", dicomDataset.GetSingleValueOrDefault<string>(DicomTag.ReferringPhysicianName)},
                {"StudyDate", dicomDataset.GetStringDateAsDate(DicomTag.StudyDate)},
                {"StudyDescription",dicomDataset.GetSingleValueOrDefault<string>(DicomTag.StudyDescription)},
                {"AccessionNumber", dicomDataset.GetSingleValueOrDefault<string>(DicomTag.AccessionNumber)},
                {"Modality", dicomDataset.GetSingleValueOrDefault<string>(DicomTag.Modality)},
                {"PerformedProcedureStepStartDate",dicomDataset.GetStringDateAsDate(DicomTag.PerformedProcedureStepStartDate)},
            };

            using HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://localhost:9200");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic","YWRtaW46YWRtaW4=");

            using var stringContent = new StringContent(jObject.ToString());
            await httpClient.PutAsync(new Uri($"/instance/_doc/{Guid.NewGuid()}"), stringContent, cancellationToken);

            return 1;
        }

        public Task DeleteStudyIndexAsync(string studyInstanceUid, DateTimeOffset cleanupAfter,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSeriesIndexAsync(string studyInstanceUid, string seriesInstanceUid, DateTimeOffset cleanupAfter,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteInstanceIndexAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid,
            DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateInstanceIndexStatusAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, IndexStatus status,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> RetrieveDeletedInstancesAsync(int batchSize, int maxRetries, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeletedInstanceAsync(VersionedInstanceIdentifier versionedInstanceIdentifier,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementDeletedInstanceRetryAsync(VersionedInstanceIdentifier versionedInstanceIdentifier,
            DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

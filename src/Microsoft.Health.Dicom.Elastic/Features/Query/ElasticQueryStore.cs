// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dicom;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Query.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Dicom.Elastic.Features.Query
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA5400", Justification = "Not production code.")]
    public sealed class ElasticQueryStore : IQueryStore, IDisposable
    {
        private HttpClient _httpClientFactory;
        private HttpClientHandler _handler;

        private static readonly IReadOnlyDictionary<DicomTag, string> tagMapping = new Dictionary<DicomTag, string>()
        {
            { DicomTag.StudyInstanceUID, "studyInstanceUID" },
            { DicomTag.StudyDate, "studyDate" },
            { DicomTag.StudyDescription, "studyDescription" },
            { DicomTag.AccessionNumber, "accessionNumber" },
            { DicomTag.PatientID, "patientId" },
            { DicomTag.PatientName, "patientName" },
            { DicomTag.ReferringPhysicianName, "referringPhysicianName" },
            { DicomTag.SeriesInstanceUID, "seriesInstanceUID" },
            { DicomTag.Modality, "modality" },
            { DicomTag.PerformedProcedureStepStartDate, "performedProcedureStepStartDate" },
            { DicomTag.SOPInstanceUID, "sopInstanceUID" },
        };

        public ElasticQueryStore()
        {
            _handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClientFactory = new HttpClient(_handler);
        }

        public async Task<QueryResult> QueryAsync(QueryExpression query, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(query, nameof(query));

            var matchToken = new Dictionary<string, string>();
            var paramsToken = new Dictionary<string, string>();
            foreach (SingleValueMatchCondition<string> filterCondition in query.FilterConditions)
            {
                matchToken.Add(tagMapping[filterCondition.QueryTag.Tag], $"{{{{{tagMapping[filterCondition.QueryTag.Tag]}}}}}");
                paramsToken.Add(tagMapping[filterCondition.QueryTag.Tag], filterCondition.Value);
            }

            var mainObject = new
            {
                Source = new
                {
                    Query = new
                    {
                        Match = matchToken
                    }
                },
                Params = paramsToken,
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                }
            };
            string json = JsonConvert.SerializeObject(mainObject, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            Console.Write(json);

            _httpClientFactory.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic","YWRtaW46YWRtaW4=");

            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:9200/instance/_search/template"),
                Content = stringContent,
            };
            var result = await _httpClientFactory.SendAsync(request, cancellationToken);

            JObject jsonResult = JObject.Parse(await result.Content.ReadAsStringAsync(cancellationToken));

            var resultList = new List<VersionedInstanceIdentifier>();
            if((int)jsonResult.SelectToken("hits.total.value") > 0)
            {
                foreach (JObject hitItem in (JArray)jsonResult.SelectToken("hits.hits"))
                {
                    string studyInstanceUid = (string)hitItem.SelectToken("_source.studyInstanceUID");
                    string seriesInstanceUid = (string)hitItem.SelectToken("_source.seriesInstanceUID");
                    string sopInstanceUid = (string)hitItem.SelectToken("_source.sopInstanceUID");

                    resultList.Add(new VersionedInstanceIdentifier(studyInstanceUid, seriesInstanceUid, sopInstanceUid, 1));
                }
            }
            return new QueryResult(resultList);
        }

        public void Dispose()
        {
            _httpClientFactory?.Dispose();
            _handler?.Dispose();
        }
    }
}

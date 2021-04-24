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
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Query.Model;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Dicom.Elastic.Features.Query
{
    public class ElasticQueryStore : IQueryStore
    {
        private IHttpClientFactory _httpClientFactory;

        public ElasticQueryStore(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));
        }

        public async Task<QueryResult> QueryAsync(QueryExpression query, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(query, nameof(query));

            var matchToken = new JObject();
            var paramsToken = new JObject();
            foreach (SingleValueMatchCondition<string> filterCondition in query.FilterConditions)
            {
                matchToken.Add(filterCondition.QueryTag.Tag.ToString(), $"{{filterCondition.QueryTag.Tag.ToString()}}");
                paramsToken.Add(filterCondition.QueryTag.Tag.ToString(), filterCondition.Value);
            }

            var mainObject = new JObject
            {
                {
                    "source",
                    new JObject
                    {
                        {
                            "query",
                            new JObject
                            {
                                {"match", matchToken}
                            }
                        }
                    }
                },
                {"params", paramsToken}
            };

            using HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://localhost:9200");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic","YWRtaW46YWRtaW4=");

            using var stringContent = new StringContent(mainObject.ToString());
            var result = await httpClient.PutAsync(new Uri("/instance/_search/template"), stringContent , cancellationToken);

            JObject jsonResult = JObject.Parse(await result.Content.ReadAsStringAsync(cancellationToken));

            var resultList = new List<VersionedInstanceIdentifier>();
            if((int)jsonResult.SelectToken("hits.total.value") > 0)
            {
                foreach (JObject hitItem in (JArray)jsonResult.SelectToken("hits.hits"))
                {
                    string studyInstanceUid = (string)hitItem.SelectToken("_source.studyInstanceUid");
                    string seriesInstanceUid = (string)hitItem.SelectToken("_source.seriesInstanceUid");
                    string sopInstanceUid = (string)hitItem.SelectToken("_source.sopInstanceUid");

                    resultList.Add(new VersionedInstanceIdentifier(studyInstanceUid, seriesInstanceUid, sopInstanceUid, 1));
                }
            }
            return new QueryResult(resultList);
        }
    }
}

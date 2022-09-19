// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Health.Dicom.Pin.Core.Features.Metadata;
using Microsoft.Health.Dicom.Pin.Core.Models;
using Microsoft.Health.Dicom.Pin.CosmosDb.Models;
using Microsoft.Health.Dicom.Pin.CosmosDb.Options;

namespace Microsoft.Health.Dicom.Pin.CosmosDb.Features.Metadata;

public class MetadataStore : IMetadataStore
{
    private readonly Container _container;

    public MetadataStore(CosmosClient cosmosClient, IOptionsMonitor<CosmosDbOptions> cosmosDbOptions)
    {
        EnsureArg.IsNotNull(cosmosClient, nameof(cosmosClient));
        EnsureArg.IsNotNull(cosmosDbOptions, nameof(cosmosDbOptions));

        _container = cosmosClient.GetContainer(cosmosDbOptions.CurrentValue.Database, cosmosDbOptions.CurrentValue.Container);
    }

    public async Task<Account> GetAccountAsync(string id, CancellationToken cancellationToken)
    {
        var sqlQueryText = $"SELECT * FROM c WHERE c.Type = '{AccountCosmosDb.DocumentType}' and c.Id = '{id}'";

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        using FeedIterator<AccountCosmosDb> queryResultSetIterator = _container.GetItemQueryIterator<AccountCosmosDb>(queryDefinition);

        List<AccountCosmosDb> accounts = new List<AccountCosmosDb>();

        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<AccountCosmosDb> currentResultSet = await queryResultSetIterator.ReadNextAsync(cancellationToken);
            foreach (AccountCosmosDb account in currentResultSet)
            {
                accounts.Add(account);
            }
        }

        return accounts.FirstOrDefault();
    }

    public async Task<Inference> GetInferenceAsync(string id, CancellationToken cancellationToken)
    {
        var sqlQueryText = $"SELECT * FROM c WHERE c.Type = '{InferenceCosmosDb.DocumentType}' and c.Id = '{id}'";

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        using FeedIterator<InferenceCosmosDb> queryResultSetIterator = _container.GetItemQueryIterator<InferenceCosmosDb>(queryDefinition);

        List<InferenceCosmosDb> inferences = new List<InferenceCosmosDb>();

        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<InferenceCosmosDb> currentResultSet = await queryResultSetIterator.ReadNextAsync(cancellationToken);
            foreach (InferenceCosmosDb inference in currentResultSet)
            {
                inferences.Add(inference);
            }
        }

        return inferences.FirstOrDefault();
    }

    public async Task<IEnumerable<Inference>> GetInferencesAsync(string accountId, CancellationToken cancellationToken)
    {
        var sqlQueryText = $"SELECT * FROM c WHERE c.Type = '{InferenceCosmosDb.DocumentType}' and c.AcountId = '{accountId}'";

        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        using FeedIterator<InferenceCosmosDb> queryResultSetIterator = _container.GetItemQueryIterator<InferenceCosmosDb>(queryDefinition);

        List<InferenceCosmosDb> inferences = new List<InferenceCosmosDb>();

        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<InferenceCosmosDb> currentResultSet = await queryResultSetIterator.ReadNextAsync(cancellationToken);
            foreach (InferenceCosmosDb inference in currentResultSet)
            {
                inferences.Add(inference);
            }
        }

        return inferences;
    }
}

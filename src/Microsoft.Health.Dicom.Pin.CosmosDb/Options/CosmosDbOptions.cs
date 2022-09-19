// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.CosmosDb.Options;

public class CosmosDbOptions
{
    public const string CosmosDbOptionsSectionName = "CosmosDb";

    public string ConnectionString { get; set; }

    public string Database { get; set; }

    public string Container { get; set; }
}

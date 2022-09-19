// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Pin.Core.Models;

namespace Microsoft.Health.Dicom.Pin.CosmosDb.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Used via cosmosdb")]
internal class AccountCosmosDb : Account
{
    public const string DocumentType = "account";

#pragma warning disable CA1051
    public string Type = DocumentType;
#pragma warning restore CA1051
}

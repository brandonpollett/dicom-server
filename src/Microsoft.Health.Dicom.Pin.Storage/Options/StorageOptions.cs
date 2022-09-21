// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.Storage.Options;

public class StorageOptions
{
    public const string StorageOptionsSectionName = "Storage";

    public string ConnectionString { get; set; }

    public string ContainerName { get; set; }
}

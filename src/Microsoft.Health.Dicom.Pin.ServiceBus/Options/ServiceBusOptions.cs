// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.ServiceBus.Options;

public class ServiceBusOptions
{
    public const string ServiceBusOptionsSectionName = "ServiceBus";

    public string ConnectionString { get; set; }
}

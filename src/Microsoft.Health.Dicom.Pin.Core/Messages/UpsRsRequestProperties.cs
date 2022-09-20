// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.Core.Messages;

public class UpsRsRequestProperties : IRequestProperties
{
    public UpsRsRequestProperties(Uri serverAddress, string instanceId)
    {
        ServerAddress = serverAddress;
        InstanceId = instanceId;
    }

    public OrchestratorSourceType OrchestratorSourceType { get; } = OrchestratorSourceType.UpsRs;

    public Uri ServerAddress { get; }

    public string InstanceId { get; }
}

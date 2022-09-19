// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.Core.Messages;

public class InferenceRequest
{
    public string AccountId { get; set; }
    public string InferenceId { get; set; }
    public string StudyUid { get; set; }
    public string SeriesUid { get; set; }
    public string InstanceUid { get; set; }
}

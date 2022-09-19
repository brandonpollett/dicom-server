// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.Core.Models;

public class Inference
{
    public string Id { get; set; }
    public string AccountId { get; set; }
    public string Name { get; set; }
    public Uri Uri { get; set; }
}

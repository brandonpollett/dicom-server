// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.Core.Messages;

public class InferenceResponse
{
    public string MessageId { get; set; }

    public string AccountId { get; set; }

    public string InferenceId { get; set; }

    public UpsRsRequestProperties RequestProperties { get; set; }

    // public IRequestProperties RequestProperties { get; set; }

    /// <summary>
    /// Throw away status code just so there is something in the response.
    /// </summary>
    public string StatusCode { get; set; }
}

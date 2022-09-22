// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Pin.Core.Models;
using Microsoft.Health.Dicom.Pin.Core.Features.Inputs;

namespace Microsoft.Health.Dicom.Pin.InferenceWorker.Features.Inferences;

public class DcmInferenceDataFactory : IInferenceDataFactory
{
    public InferenceDataType InferenceDataType { get; } = InferenceDataType.Dcm;

    public async Task<Stream> GetDataAsync(DicomInput input, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(input, nameof(input));
#pragma warning disable CA2000
        // Another thread may have already gone through this block
        MemoryStream stream = new MemoryStream();
#pragma warning restore CA2000

        await input.DicomFile.SaveAsync(stream);

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    public Task<T> CreateAsync<T>(Stream stream, CancellationToken cancellationToken) => throw new NotImplementedException();
}

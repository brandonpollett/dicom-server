// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnsureThat;
using Microsoft.Health.Dicom.Pin.Core.Features.TempFiles;

namespace Microsoft.Health.Dicom.Pin.Storage.Features.TempFiles;

public class StorageTempFileStore : ITempFileStore
{
    private readonly BlobContainerClient _containerClient;

    public StorageTempFileStore(BlobContainerClient containerClient)
    {
        _containerClient = EnsureArg.IsNotNull(containerClient, nameof(containerClient));
    }

    public async Task<string> Save(Stream stream, string extension, CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid()}.{extension}";

        BlobClient blob = _containerClient.GetBlobClient(fileName);

        await blob.UploadAsync(stream, cancellationToken: cancellationToken);

        return fileName;
    }

    public async Task<Stream> Retrieve(string fileName, CancellationToken cancellationToken)
    {
        BlobClient blob = _containerClient.GetBlobClient(fileName);

        return await blob.OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken);
    }
}

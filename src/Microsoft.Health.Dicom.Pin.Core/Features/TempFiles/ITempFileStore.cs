﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Dicom.Pin.Core.Features.TempFiles;

public interface ITempFileStore
{
    Task<string> SaveAsync(Stream stream, string extension, CancellationToken cancellationToken);

    Task<Stream> RetrieveAsync(string fileName, CancellationToken cancellationToken);
}
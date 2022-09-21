﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Pin.Core.Features.TempFiles;

public interface ITempFileStore
{
    Task<string> Save(Stream stream, CancellationToken cancellationToken);

    Task<Stream> Retrieve(string fileName, CancellationToken cancellationToken);
}

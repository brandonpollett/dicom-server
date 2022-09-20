// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Pin.Core.Features.Messaging;

namespace Microsoft.Health.Dicom.Core.Features.Workitem;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Response builder handles all exceptions.")]
public partial class WorkitemService : IWorkitemService
{
    private readonly IWorkitemResponseBuilder _responseBuilder;
    private readonly IEnumerable<IWorkitemDatasetValidator> _validators;
    private readonly IWorkitemOrchestrator _workitemOrchestrator;
    private readonly ILogger _logger;
    private readonly IOrchestratorStore _serviceBusOrchestratorStore;

    public WorkitemService(
        IWorkitemResponseBuilder responseBuilder,
        IEnumerable<IWorkitemDatasetValidator> dicomDatasetValidators,
        IWorkitemOrchestrator workitemOrchestrator,
        ILogger<WorkitemService> logger,
        IOrchestratorStore serviceBusOrchestratorStore)
    {
        _responseBuilder = EnsureArg.IsNotNull(responseBuilder, nameof(responseBuilder));
        _validators = EnsureArg.IsNotNull(dicomDatasetValidators, nameof(dicomDatasetValidators));
        _workitemOrchestrator = EnsureArg.IsNotNull(workitemOrchestrator, nameof(workitemOrchestrator));
        _logger = EnsureArg.IsNotNull(logger, nameof(logger));
        _serviceBusOrchestratorStore = serviceBusOrchestratorStore;
    }

    private IWorkitemDatasetValidator GetValidator<T>() where T : IWorkitemDatasetValidator
    {
        var validator = _validators.FirstOrDefault(o => string.Equals(o.Name, typeof(T).Name, StringComparison.Ordinal));

        return validator;
    }
}

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Enums;
using OpenFiscalCore.System.Domains.ESDC.Types.Health;
using OpenFiscalCore.System.Domains.ESDC.Types.LocalFiscalization;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Domains.ESDC.Types.Pki;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;
using OpenFiscalCore.System.Domains.ESDC.Types.States;
using OpenFiscalCore.System.Domains.ESIR.Types.Enums;
using OpenFiscalCore.System.Domains.ESIR.Types.Health;
using OpenFiscalCore.System.Domains.ESIR.Types.Routing;
using OpenFiscalCore.System.Domains.ESIR.Types.States;
using OpenFiscalCore.System.Domains.ESIR.Types.Validation;
using OpenFiscalCore.System.Types.Domain;
using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Types.Lpfr;
using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Results;
using OpenFiscalCore.System.Types.Validation;
using OpenFiscalCore.System.Types.Verification;

namespace OpenFiscalCore.System.Qualification;

internal static class QualificationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Run()
    {
        // ── Gate 1: Structural checks (existing) ──────────────────
        RunCoverageChecks();
        RunWrapperValidationChecks();
        RunBoundaryValidationChecks();
        RunJsonRoundTripChecks();
        RunStateTraceabilityChecks();
        RunRuntimeStoreShapeChecks();

        // ── Gate 2: Behavioral checks (system qualification) ──────
        var behavioralResults = BehavioralRunner.RunAll();
        TestReporter.PrintResults(behavioralResults);
        TestReporter.PrintSummary(behavioralResults);

        if (behavioralResults.Any(r => r.Status == TestStatus.Fail))
            throw new InvalidOperationException(
                $"Behavioral qualification failed: {behavioralResults.Count(r => r.Status == TestStatus.Fail)} test(s) failed.");
    }

    private static void RunCoverageChecks()
    {
        var systemRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
        var ledgerPaths = Directory.GetFiles(systemRoot, "TYPE_*_DIAGRAM_TYPE_EXTRACTION_LEDGER.json", SearchOption.AllDirectories);

        AssertEx.True(ledgerPaths.Length >= 3, "Expected system and domain extraction ledgers to be present.");

        var ledger = new List<LedgerEntry>();
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var ledgerPath in ledgerPaths)
        {
            var entries = JsonSerializer.Deserialize<List<LedgerEntry>>(File.ReadAllText(ledgerPath), serializerOptions)
                ?? throw new InvalidOperationException($"Extraction ledger '{ledgerPath}' could not be deserialized.");

            AssertEx.True(entries.Count > 0, $"Extraction ledger '{Path.GetFileName(ledgerPath)}' should not be empty.");
            ledger.AddRange(entries);
        }

        AssertEx.True(ledger.Count >= 150, "Extraction ledgers are missing expected entries.");

        foreach (var assemblyPath in Directory.GetFiles(AppContext.BaseDirectory, "OpenFiscalCore*.dll"))
        {
            Assembly.LoadFrom(assemblyPath);
        }

        var exportedTypeNames = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(static assembly => assembly.GetExportedTypes())
            .Select(static type => type.FullName)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var cluster in ledger.GroupBy(static entry => entry.Cluster))
        {
            AssertEx.True(cluster.Any(), $"Coverage cluster '{cluster.Key}' is empty.");
        }

        foreach (var entry in ledger.Where(static entry => entry.Resolution != "deferred_to_interface_or_adapter"))
        {
            AssertEx.True(
                !string.IsNullOrWhiteSpace(entry.FullTypeName) && exportedTypeNames.Contains(entry.FullTypeName),
                $"Ledger entry '{entry.Token}' does not resolve to an implemented public type.");
        }

        foreach (var entry in ledger.Where(static entry => entry.Resolution == "deferred_to_interface_or_adapter"))
        {
            AssertEx.True(string.IsNullOrWhiteSpace(entry.FullTypeName), $"Deferred entry '{entry.Token}' should not bind to an implementation type.");
        }
    }

    private static void RunWrapperValidationChecks()
    {
        AssertEx.Throws<ArgumentException>(() => new PinPlainText("12A4"), "PinPlainText should reject non-digit input.");
        AssertEx.Equal("****", new PinPlainText("1234").ToString(), "PinPlainText should redact ToString output.");
        AssertEx.Throws<ArgumentException>(() => new Uid("abc"), "Uid should reject invalid length.");
        AssertEx.Throws<ArgumentException>(() => new BuyerIdentification("99:123"), "BuyerIdentification should reject unknown code prefixes.");
        AssertEx.Equal("10", new BuyerIdentification("10:123456789").Code.Value, "BuyerIdentification should cache the parsed code.");
        AssertEx.Equal("123456789", new BuyerIdentification("10:123456789").Identifier, "BuyerIdentification should cache the parsed identifier.");
        AssertEx.Throws<ArgumentException>(() => new RequestId(new string('x', 33)), "RequestId should enforce maximum length.");
        AssertEx.Throws<ArgumentException>(() => new VerificationUrl("not-a-url"), "VerificationUrl should require an absolute URI.");
        AssertEx.Throws<ArgumentException>(() => new EncryptionCertificateBase64(ReadOnlyMemory<byte>.Empty), "EncryptionCertificateBase64 should reject empty payloads.");
        AssertEx.Throws<ArgumentException>(() => new TaxCorePublicKey(new byte[32]), "TaxCorePublicKey should enforce canonical payload length.");
        AssertEx.Throws<ArgumentException>(() => new AuditRequestPayload(new byte[32]), "AuditRequestPayload should enforce canonical payload length.");
        AssertEx.Throws<ArgumentException>(() => new StatusWord("0x900"), "StatusWord should require a 16-bit hexadecimal payload.");
        AssertEx.Equal("0x9000", new StatusWord("9000").ToString(), "StatusWord should normalize raw hexadecimal input.");
        AssertEx.True(new StatusWord("0x9000").IsSuccess, "StatusWord should expose APDU success semantics.");
        AssertEx.True(new AuditDataStatusCode(1).IsRetryable, "AuditDataStatusCode should expose retryable semantics.");
        AssertEx.True(new AuditDataStatusCode(4).ShouldDeleteLocal, "AuditDataStatusCode should expose delete-local semantics.");
        AssertEx.True(new AuditDataStatusCode(78).ShouldHoldLocal, "AuditDataStatusCode should expose hold-local semantics for non-retryable failures.");
    }

    private static void RunBoundaryValidationChecks()
    {
        ExpectValidationFailure(new PaymentItem(-1m, TaxCorePaymentType.Cash), "PaymentItem amount should be validated.");
        ExpectValidationFailure(new InvoiceItem(null, string.Empty, 0m, Array.Empty<string>(), -1m, -1m), "InvoiceItem should reject invalid values.");
        ExpectValidationFailure(new ValidationErrorResponse(string.Empty, Array.Empty<ValidationErrorItem>()), "ValidationErrorResponse should require a message and model state.");
        ExpectValidationFailure(new EnvironmentDescriptorList(Array.Empty<EnvironmentDescriptor>()), "EnvironmentDescriptorList should require at least one item.");
        ExpectValidationFailure(new MediaCommandResults(Array.Empty<MediaCommandResultItem>()), "MediaCommandResults should require at least one item.");
        ExpectValidationFailure(new PreparedTaxBreakdownItem(string.Empty, -1m, -1m, -1m, -1m), "PreparedTaxBreakdownItem should reject invalid values.");
        ExpectValidationFailure(new PreparedInvoiceValidationErrorList(Array.Empty<PreparedInvoiceValidationError>()), "PreparedInvoiceValidationErrorList should require at least one item.");
        ExpectValidationFailure(new InvoiceRequest(
            DateTimeOffset.Parse("2026-04-15T10:15:00+02:00"),
            TaxCoreInvoiceType.Normal,
            TaxCoreTransactionType.Sale,
            [new PaymentItem(-1m, TaxCorePaymentType.Cash)],
            "Operator",
            null,
            null,
            "INV-1",
            null,
            null,
            null,
            [new InvoiceItem(null, "Bread", 1m, ["A"], 100m, 100m)]), "InvoiceRequest should recursively validate nested DTO items.");

        var validInvoiceRequest = new InvoiceRequest(
            DateTimeOffset.Parse("2026-04-15T10:15:00+02:00"),
            TaxCoreInvoiceType.Normal,
            TaxCoreTransactionType.Sale,
            [new PaymentItem(100m, TaxCorePaymentType.Cash)],
            "Operator",
            new BuyerIdentification("10:123456789"),
            "30:099999999",
            "INV-1",
            null,
            null,
            new InvoiceOptions(InvoiceOptionFlag.Generate, InvoiceOptionFlag.Omit),
            [new InvoiceItem(null, "Bread", 1m, ["A"], 100m, 100m)]);

        validInvoiceRequest.EnsureValid();
        ExpectValidationFailure(
            new PreparedInvoiceRequest(
                validInvoiceRequest,
                new PreparedInvoiceTotals(100m, 100m, 20m),
                Array.Empty<PreparedTaxBreakdownItem>()),
            "PreparedInvoiceRequest should require at least one tax-breakdown line.");

        ValidateObject(new TaxCoreConfigurationResponse(
            "Business",
            "Europe/Belgrade",
            "Street 1",
            "Belgrade",
            "RS",
            new EnvironmentEndpoints { TaxCoreApi = "https://api.example.test" },
            "prod",
            "https://logo.example.test/logo.png",
            "https://ntp.example.test",
            ["sr-Latn-RS"]));

        ValidateObject(new ClientCertificateContext(
            "CN=Open Fiscal Core Test",
            "Open Fiscal Core Test",
            "Open Fiscal Core",
            new Uid("ABCDEFG1"),
            DateTimeOffset.Parse("2027-04-15T10:15:00Z")));

        ValidateObject(new MediaRootInspection(
            true,
            [new Uid("ABCDEFG1")]));

        ValidateObject(new MediaCommandFile(
            new Uid("ABCDEFG1"),
            [new Command("00000000-0000-0000-0000-000000000001", CommandsType.SetTaxRates, "{\"revision\":1}", new Uid("ABCDEFG1"))]));

        ValidateObject(new MediaCommandResults(
            [new MediaCommandResultItem("00000000-0000-0000-0000-000000000001", true)]));

        ValidateObject(new ReadinessContext(
            new ReadinessResult(
                ReadinessStatus.Ready,
                [ReadinessReason.EnvironmentContextRefreshPending]),
            IsVsdcConfigured: true,
            IsVsdcCertificateValid: true,
            IsVsdcReachable: true,
            IsEsdcAttentionReachable: true,
            IsPinVerified: true,
            IsAuditBlocking: false,
            IsSecureElementCertificateValid: true));

        ValidateObject(new RouteSelectionResult(
            FiscalizationRoute.Unavailable,
            RouteSelectionFailureCode.RouteUnavailable,
            OnlineRouteUnavailableReason.Unreachable,
            LocalRouteUnavailableReason.PinRequired));

        ValidateObject(new PreparedInvoiceValidationFailure(
            PreparedInvoiceFailureCode.ValidationFailed,
            new PreparedInvoiceValidationErrorList(
                [new PreparedInvoiceValidationError("items[0].labels[0]", "unknown_tax_label", "The supplied tax label is not configured.")])));

        ValidateObject(new PreparedInvoiceRequest(
            validInvoiceRequest,
            new PreparedInvoiceTotals(100m, 100m, 20m),
            [new PreparedTaxBreakdownItem("A", 20m, 83.33m, 16.67m, 100m)]));

        ValidateObject(new BackendSyncResult(BackendSyncOutcome.Synced));
        ValidateObject(new SeDirectiveResult(SecureElementOperationOutcome.Success, new StatusWord("0x9000")));
        ValidateObject(new AuditCycleOutcome(AuditCycleDisposition.AuditCleared, 0, ProofCompletionStatus.Completed));
        ValidateObject(new OpenFiscalCore.System.Domains.ESDC.Types.AuditProof.EndAuditResult(EndAuditStatus.Success, new StatusWord("0x9000")));
        ValidateObject(new MediaDetectionResult(true, true));
        ValidateObject(new MediaCommandResult(2, 2, 0));
        ValidateObject(new ExportSetResult(true, 3, true));
        ValidateObject(new SeProbeResult(ProbeStatus.Pass, new Uid("ABCDEFG1")));
        ValidateObject(new PinVerifyResult(PinVerificationOutcome.PinFailed, new PinTriesLeft(2)));
        ValidateObject(new LocalFiscalizationFailure(LocalFiscalizationFailureCode.SeSigningFailed, new StatusWord("0x6F00")));
    }

    private static void RunJsonRoundTripChecks()
    {
        var invoiceRequest = new InvoiceRequest(
            DateTimeOffset.Parse("2026-04-15T10:15:00+02:00"),
            TaxCoreInvoiceType.Normal,
            TaxCoreTransactionType.Sale,
            [new PaymentItem(100m, TaxCorePaymentType.Cash)],
            "Operator",
            new BuyerIdentification("10:123456789"),
            "30:099999999",
            "INV-1",
            null,
            null,
            new InvoiceOptions(InvoiceOptionFlag.Generate, InvoiceOptionFlag.Omit),
            [new InvoiceItem(null, "Bread", 1m, ["A"], 100m, 100m)]);

        var invoiceRequestJson = JsonSerializer.Serialize(invoiceRequest, JsonOptions);
        var invoiceRequestRoundTrip = JsonSerializer.Deserialize<InvoiceRequest>(invoiceRequestJson, JsonOptions)
            ?? throw new InvalidOperationException("InvoiceRequest round-trip failed.");
        AssertEx.Equal(TaxCoreInvoiceType.Normal, invoiceRequestRoundTrip.InvoiceType, "InvoiceRequest should preserve invoice type.");
        AssertEx.Equal("10:123456789", invoiceRequestRoundTrip.BuyerId?.Value ?? string.Empty, "InvoiceRequest should preserve buyer identification.");

        var commandList = new CommandList(
            [new Command("00000000-0000-0000-0000-000000000001", CommandsType.SetTaxRates, "{\"revision\":1}", new Uid("ABCDEFG1"))]);
        var commandListJson = JsonSerializer.Serialize(commandList, JsonOptions);
        var commandListRoundTrip = JsonSerializer.Deserialize<CommandList>(commandListJson, JsonOptions)
            ?? throw new InvalidOperationException("CommandList round-trip failed.");
        AssertEx.Equal(1, commandListRoundTrip.Items.Count, "CommandList should preserve element count.");
        AssertEx.Equal(CommandsType.SetTaxRates, commandListRoundTrip.Items[0].Type, "CommandList should preserve command type.");

        var proofRequest = new ProofOfAuditRequest(new AuditRequestPayload(new byte[260]), 10UL, 20UL);
        var proofRequestJson = JsonSerializer.Serialize(proofRequest, JsonOptions);
        var proofRequestRoundTrip = JsonSerializer.Deserialize<ProofOfAuditRequest>(proofRequestJson, JsonOptions)
            ?? throw new InvalidOperationException("ProofOfAuditRequest round-trip failed.");
        AssertEx.Equal(260, proofRequestRoundTrip.AuditRequestPayload.Value.Length, "ProofOfAuditRequest should preserve payload size.");
        AssertEx.Equal(20UL, proofRequestRoundTrip.Limit, "ProofOfAuditRequest should preserve limit.");

        var verificationResponse = new InvoiceVerificationResponse(
            new VerifiedInvoiceRequestView(
                DateTimeOffset.Parse("2026-04-15T10:15:00Z"),
                "240799085",
                "Business",
                "Location",
                "Street 1",
                "Belgrade",
                "Belgrade",
                null,
                null,
                "Operator",
                new Uid("ABCDEFG1"),
                null,
                TaxCoreInvoiceType.Normal,
                TaxCoreTransactionType.Sale,
                [new VerifiedPaymentItem(TaxCorePaymentType.Cash, 100m)]),
            new VerifiedInvoiceResultView(100m, 1, 1, "NS", "ABCDEFG1-HIJKLMN2-1", "HIJKLMN2", DateTimeOffset.Parse("2026-04-15T08:15:00Z")),
            "journal",
            true);

        var verificationJson = JsonSerializer.Serialize(verificationResponse, JsonOptions);
        var verificationRoundTrip = JsonSerializer.Deserialize<InvoiceVerificationResponse>(verificationJson, JsonOptions)
            ?? throw new InvalidOperationException("InvoiceVerificationResponse round-trip failed.");
        AssertEx.True(verificationRoundTrip.IsValid, "Verification response should preserve validity flag.");
        AssertEx.Equal("HIJKLMN2", verificationRoundTrip.InvoiceResult.SignedBy, "Verification response should preserve signedBy.");

        var boolBodyJson = JsonSerializer.Serialize(new TaxCoreBooleanFlagBody(true), JsonOptions);
        var boolBody = JsonSerializer.Deserialize<TaxCoreBooleanFlagBody>(boolBodyJson, JsonOptions);
        AssertEx.True(boolBody.Value, "TaxCoreBooleanFlagBody should round-trip as a bare boolean.");

        var auditStatusJson = JsonSerializer.Serialize(new AuditDataStatus(new AuditDataStatusCode(4), commandList.Items), JsonOptions);
        var auditStatus = JsonSerializer.Deserialize<AuditDataStatus>(auditStatusJson, JsonOptions)
            ?? throw new InvalidOperationException("AuditDataStatus round-trip failed.");
        AssertEx.True(auditStatus.ShouldDeleteLocal, "AuditDataStatus should preserve semantic delete-local classification.");
        AssertEx.Equal("invoice_verified", auditStatus.Status?.KnownTitle ?? string.Empty, "AuditDataStatus should preserve the known status mapping.");

        var readinessResultJson = JsonSerializer.Serialize(
            new ReadinessResult(
                ReadinessStatus.Degraded,
                [ReadinessReason.BackendPathLostLocalOnlyMode, ReadinessReason.EnvironmentContextRefreshPending],
                "Local-only mode remains available."),
            JsonOptions);
        var readinessResult = JsonSerializer.Deserialize<ReadinessResult>(readinessResultJson, JsonOptions)
            ?? throw new InvalidOperationException("ReadinessResult round-trip failed.");
        AssertEx.Equal(ReadinessReason.BackendPathLostLocalOnlyMode, readinessResult.Reasons?[0] ?? default, "ReadinessResult should preserve typed readiness reasons.");

        var bootstrapResultJson = JsonSerializer.Serialize(
            new BootstrapResult(
                BootstrapStatus.PendingOperatorAction,
                [BootstrapReason.InitializationCommandsPendingReview],
                "One initialization command needs operator review."),
            JsonOptions);
        var bootstrapResult = JsonSerializer.Deserialize<BootstrapResult>(bootstrapResultJson, JsonOptions)
            ?? throw new InvalidOperationException("BootstrapResult round-trip failed.");
        AssertEx.Equal(BootstrapReason.InitializationCommandsPendingReview, bootstrapResult.Reasons?[0] ?? default, "BootstrapResult should preserve typed bootstrap reasons.");

        var mediaResultsJson = JsonSerializer.Serialize(
            new MediaCommandResults([new MediaCommandResultItem("00000000-0000-0000-0000-000000000001", true)]),
            JsonOptions);
        var mediaResults = JsonSerializer.Deserialize<MediaCommandResults>(mediaResultsJson, JsonOptions)
            ?? throw new InvalidOperationException("MediaCommandResults round-trip failed.");
        AssertEx.True(mediaResults.Items[0].ProcessingSucceeded, "MediaCommandResults should preserve command processing outcomes.");

        var routeSelectionResultJson = JsonSerializer.Serialize(
            new RouteSelectionResult(
                FiscalizationRoute.Unavailable,
                RouteSelectionFailureCode.RouteUnavailable,
                OnlineRouteUnavailableReason.Unreachable,
                LocalRouteUnavailableReason.PinRequired),
            JsonOptions);
        var routeSelectionResult = JsonSerializer.Deserialize<RouteSelectionResult>(routeSelectionResultJson, JsonOptions)
            ?? throw new InvalidOperationException("RouteSelectionResult round-trip failed.");
        AssertEx.Equal(FiscalizationRoute.Unavailable, routeSelectionResult.Route, "RouteSelectionResult should preserve the selected route.");
        AssertEx.Equal(OnlineRouteUnavailableReason.Unreachable, routeSelectionResult.OnlineReason ?? default, "RouteSelectionResult should preserve online unavailability reason.");
        AssertEx.Equal(LocalRouteUnavailableReason.PinRequired, routeSelectionResult.LocalReason ?? default, "RouteSelectionResult should preserve local unavailability reason.");

        var preparedInvoiceRequest = new PreparedInvoiceRequest(
            invoiceRequest,
            new PreparedInvoiceTotals(100m, 100m, 20m),
            [new PreparedTaxBreakdownItem("A", 20m, 83.33m, 16.67m, 100m)]);
        var preparedInvoiceJson = JsonSerializer.Serialize(preparedInvoiceRequest, JsonOptions);
        var preparedInvoiceRoundTrip = JsonSerializer.Deserialize<PreparedInvoiceRequest>(preparedInvoiceJson, JsonOptions)
            ?? throw new InvalidOperationException("PreparedInvoiceRequest round-trip failed.");
        AssertEx.Equal("INV-1", preparedInvoiceRoundTrip.NormalizedRequest.InvoiceNumber ?? string.Empty, "PreparedInvoiceRequest should preserve the normalized invoice reference.");
        AssertEx.Equal(1, preparedInvoiceRoundTrip.TaxBreakdown.Count, "PreparedInvoiceRequest should preserve tax breakdown items.");

        var backendSyncJson = JsonSerializer.Serialize(
            new BackendSyncResult(BackendSyncOutcome.Failed, BackendSyncReason.AuthContextUnavailable),
            JsonOptions);
        var backendSync = JsonSerializer.Deserialize<BackendSyncResult>(backendSyncJson, JsonOptions)
            ?? throw new InvalidOperationException("BackendSyncResult round-trip failed.");
        AssertEx.Equal(BackendSyncOutcome.Failed, backendSync.Outcome, "BackendSyncResult should preserve outcome.");
        AssertEx.Equal(BackendSyncReason.AuthContextUnavailable, backendSync.Reason ?? default, "BackendSyncResult should preserve failure reason.");

        var auditCycleJson = JsonSerializer.Serialize(
            new AuditCycleOutcome(AuditCycleDisposition.ProofPending, 2, ProofCompletionStatus.Pending),
            JsonOptions);
        var auditCycle = JsonSerializer.Deserialize<AuditCycleOutcome>(auditCycleJson, JsonOptions)
            ?? throw new InvalidOperationException("AuditCycleOutcome round-trip failed.");
        AssertEx.Equal(AuditCycleDisposition.ProofPending, auditCycle.State, "AuditCycleOutcome should preserve disposition.");
        AssertEx.Equal(2, auditCycle.PackagesRemaining, "AuditCycleOutcome should preserve remaining package count.");

        var endAuditJson = JsonSerializer.Serialize(
            new OpenFiscalCore.System.Domains.ESDC.Types.AuditProof.EndAuditResult(EndAuditStatus.Error, new StatusWord("0x6985")),
            JsonOptions);
        var endAudit = JsonSerializer.Deserialize<OpenFiscalCore.System.Domains.ESDC.Types.AuditProof.EndAuditResult>(endAuditJson, JsonOptions)
            ?? throw new InvalidOperationException("EndAuditResult round-trip failed.");
        AssertEx.Equal("0x6985", endAudit.StatusWord.Value, "EndAuditResult should preserve the APDU status word.");

        var mediaDetectionJson = JsonSerializer.Serialize(
            new MediaDetectionResult(false, false, MediaDetectionFailureReason.UnsupportedFilesystem),
            JsonOptions);
        var mediaDetection = JsonSerializer.Deserialize<MediaDetectionResult>(mediaDetectionJson, JsonOptions)
            ?? throw new InvalidOperationException("MediaDetectionResult round-trip failed.");
        AssertEx.Equal(MediaDetectionFailureReason.UnsupportedFilesystem, mediaDetection.Reason ?? default, "MediaDetectionResult should preserve the failure reason.");

        var pinVerifyJson = JsonSerializer.Serialize(
            new PinVerifyResult(PinVerificationOutcome.PinFailed, new PinTriesLeft(2)),
            JsonOptions);
        var pinVerify = JsonSerializer.Deserialize<PinVerifyResult>(pinVerifyJson, JsonOptions)
            ?? throw new InvalidOperationException("PinVerifyResult round-trip failed.");
        AssertEx.Equal(PinVerificationOutcome.PinFailed, pinVerify.Outcome, "PinVerifyResult should preserve the verification status.");
        AssertEx.Equal((byte)2, pinVerify.RetriesRemaining?.Value ?? byte.MaxValue, "PinVerifyResult should preserve remaining retries.");

        var localFiscalizationFailureJson = JsonSerializer.Serialize(
            new LocalFiscalizationFailure(LocalFiscalizationFailureCode.SeSigningFailed, new StatusWord("6F00")),
            JsonOptions);
        var localFiscalizationFailure = JsonSerializer.Deserialize<LocalFiscalizationFailure>(localFiscalizationFailureJson, JsonOptions)
            ?? throw new InvalidOperationException("LocalFiscalizationFailure round-trip failed.");
        AssertEx.Equal(LocalFiscalizationFailureCode.SeSigningFailed, localFiscalizationFailure.Code, "LocalFiscalizationFailure should preserve the failure code.");
        AssertEx.Equal("0x6F00", localFiscalizationFailure.Sw?.Value ?? string.Empty, "LocalFiscalizationFailure should preserve the status word.");
    }

    private static void RunStateTraceabilityChecks()
    {
        AssertEnumMembers<EsirServingState>(nameof(EsirServingState), ["Starting", "Accepting", "DegradedOfflineCapable", "Blocked"]);
        AssertEnumMembers<EsirFiscalizationState>(nameof(EsirFiscalizationState), ["RequestReceived", "ValidatingAndPreparing", "RouteDecision", "ExecutingOnline", "ExecutingLocal", "Succeeded", "Failed"]);
        AssertEnumMembers<EsdcServingState>(nameof(EsdcServingState), ["Starting", "Accepting", "DegradedLocalOnly", "Blocked"]);
        AssertEnumMembers<EsdcLocalFiscalizationState>(nameof(EsdcLocalFiscalizationState), ["RequestReceived", "PreparingSignInput", "Signing", "PersistingLocalEvidence", "Succeeded", "Failed"]);
        AssertEnumMembers<EsdcBackendSyncState>(nameof(EsdcBackendSyncState), ["Idle", "EnsuringAuthContext", "AnnouncingStatus", "PullingInitializationCommands", "CapturingCommands", "ExecutingCommands", "ReportingCommandOutcomes", "Succeeded", "Deferred", "Failed"]);
        AssertEnumMembers<EsdcAuditAndProofState>(nameof(EsdcAuditAndProofState), ["Idle", "SubmittingAuditPackage", "ResolvingAuditStatus", "StartingProofCycle", "SubmittingProofRequest", "WaitingForProofCompletion", "CompletingProofCycle", "Succeeded", "Deferred", "Failed"]);
        AssertEnumMembers<EsdcLocalAuditExportState>(nameof(EsdcLocalAuditExportState), ["Idle", "MediaDetected", "ImportingCommands", "DeterminingExportSet", "StagingArtifacts", "WritingMedia", "RecordingMediaResults", "Succeeded", "Deferred", "Failed"]);
    }

    private static void RunRuntimeStoreShapeChecks()
    {
        var runtimeRecords = new[]
        {
            typeof(ApprovedConfigurationRecord),
            typeof(EnvironmentBindingRecord),
            typeof(SharedTaxCoreSnapshotRecord),
            typeof(HostedCoordinationRecord),
            typeof(CounterStateRecord),
            typeof(LocalJournalRecord),
            typeof(AuditBacklogIndex),
            typeof(ProofCycleRecord),
            typeof(ReadinessRecoverySnapshot),
            typeof(CommandIndex),
            typeof(ExportBatchRecord),
            typeof(StagedMediaArtifactRecord),
            typeof(ExportResultRecord)
        };

        foreach (var recordType in runtimeRecords)
        {
            var properties = recordType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            AssertEx.True(properties.Any(static property => property.Name.EndsWith("Key", StringComparison.Ordinal) || property.Name.EndsWith("Ref", StringComparison.Ordinal)),
                $"{recordType.Name} should expose an owning key or reference.");
            AssertEx.True(properties.Any(static property => property.Name == "Meta"),
                $"{recordType.Name} should expose metadata.");
            AssertEx.True(properties.All(static property =>
                    property.PropertyType.FullName is null ||
                    (!property.PropertyType.FullName.Contains("Interface", StringComparison.Ordinal) &&
                     !property.PropertyType.FullName.Contains("Adapter", StringComparison.Ordinal))),
                $"{recordType.Name} should not depend on interface or adapter types.");
        }

        AssertEx.True(typeof(ProofCycleRecord).GetProperty(nameof(ProofCycleRecord.CurrentState)) is not null, "ProofCycleRecord should carry lifecycle state.");
        AssertEx.True(typeof(ReadinessRecoverySnapshot).GetProperty(nameof(ReadinessRecoverySnapshot.StartupStatus)) is not null, "ReadinessRecoverySnapshot should carry startup status.");
        AssertEx.True(typeof(ReadinessRecoverySnapshot).GetProperty(nameof(ReadinessRecoverySnapshot.LivenessStatus)) is not null, "ReadinessRecoverySnapshot should carry liveness status.");
        AssertEx.True(typeof(ReadinessRecoverySnapshot).GetProperty(nameof(ReadinessRecoverySnapshot.ReadinessStatus)) is not null, "ReadinessRecoverySnapshot should carry readiness status.");
        AssertEx.True(typeof(ExportBatchRecord).GetProperty(nameof(ExportBatchRecord.CurrentState)) is not null, "ExportBatchRecord should carry export state.");
        AssertEx.True(typeof(ExportResultRecord).GetProperty(nameof(ExportResultRecord.OutcomeStatus)) is not null, "ExportResultRecord should carry export outcome.");
    }

    private static void AssertEnumMembers<TEnum>(string enumName, IReadOnlyList<string> expected)
        where TEnum : struct, Enum
    {
        var actual = Enum.GetNames<TEnum>();
        AssertEx.Equal(expected.Count, actual.Length, $"{enumName} should not contain extra members.");

        for (var index = 0; index < expected.Count; index++)
        {
            AssertEx.Equal(expected[index], actual[index], $"{enumName} member mismatch at position {index}.");
        }
    }

    private static void ValidateObject(object target)
    {
        ContractValidator.ValidateObjectGraph(target);
    }

    private static void ExpectValidationFailure(object target, string message)
    {
        AssertEx.Throws<ValidationException>(() => ValidateObject(target), message);
    }

    private sealed record LedgerEntry(
        string Id,
        string Cluster,
        string Token,
        string Resolution,
        string? CanonicalType,
        string? FullTypeName,
        string Notes);
}

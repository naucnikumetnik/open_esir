using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Enums;
using OpenFiscalCore.System.Domains.ESDC.Types.Health;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Domains.ESDC.Types.Pki;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;
using OpenFiscalCore.System.Domains.ESDC.Types.States;
using OpenFiscalCore.System.Domains.ESIR.Types.Health;
using OpenFiscalCore.System.Domains.ESIR.Types.States;
using OpenFiscalCore.System.Types.Domain;
using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Types.Lpfr;
using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Results;
using OpenFiscalCore.System.Types.Verification;

namespace OpenFiscalCore.System.Qualification;

internal static class QualificationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Run()
    {
        RunCoverageChecks();
        RunWrapperValidationChecks();
        RunBoundaryValidationChecks();
        RunJsonRoundTripChecks();
        RunStateTraceabilityChecks();
        RunRuntimeStoreShapeChecks();
    }

    private static void RunCoverageChecks()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../types/design/TYPE_SYS_DIAGRAM_TYPE_EXTRACTION_LEDGER.json"));

        var ledger = JsonSerializer.Deserialize<List<LedgerEntry>>(File.ReadAllText(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Extraction ledger could not be deserialized.");

        AssertEx.True(ledger.Count >= 90, "Extraction ledger is missing expected entries.");

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
        AssertEx.Throws<ArgumentException>(() => new Uid("abc"), "Uid should reject invalid length.");
        AssertEx.Throws<ArgumentException>(() => new BuyerIdentification("99:123"), "BuyerIdentification should reject unknown code prefixes.");
        AssertEx.Throws<ArgumentException>(() => new RequestId(new string('x', 33)), "RequestId should enforce maximum length.");
        AssertEx.Throws<ArgumentException>(() => new VerificationUrl("not-a-url"), "VerificationUrl should require an absolute URI.");
        AssertEx.Throws<ArgumentException>(() => new EncryptionCertificateBase64(ReadOnlyMemory<byte>.Empty), "EncryptionCertificateBase64 should reject empty payloads.");
        AssertEx.Throws<ArgumentException>(() => new TaxCorePublicKey(new byte[32]), "TaxCorePublicKey should enforce canonical payload length.");
        AssertEx.Throws<ArgumentException>(() => new AuditRequestPayload(new byte[32]), "AuditRequestPayload should enforce canonical payload length.");
    }

    private static void RunBoundaryValidationChecks()
    {
        ExpectValidationFailure(new PaymentItem(-1m, TaxCorePaymentType.Cash), "PaymentItem amount should be validated.");
        ExpectValidationFailure(new InvoiceItem(null, string.Empty, 0m, Array.Empty<string>(), -1m, -1m), "InvoiceItem should reject invalid values.");
        ExpectValidationFailure(new ValidationErrorResponse(string.Empty, Array.Empty<ValidationErrorItem>()), "ValidationErrorResponse should require a message and model state.");
        ExpectValidationFailure(new EnvironmentDescriptorList(Array.Empty<EnvironmentDescriptor>()), "EnvironmentDescriptorList should require at least one item.");

        ValidateObject(new InvoiceRequest(
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
            [new InvoiceItem(null, "Bread", 1m, ["A"], 100m, 100m)]));

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
        Validator.ValidateObject(target, new ValidationContext(target), validateAllProperties: true);
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

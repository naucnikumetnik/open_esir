using System.Security.Cryptography.X509Certificates;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed record PkiClientContextAdapterConfig(
    string SubjectName,
    StoreName StoreName = StoreName.My,
    StoreLocation StoreLocation = StoreLocation.CurrentUser,
    bool ValidOnly = false,
    Uid? UidOverride = null);

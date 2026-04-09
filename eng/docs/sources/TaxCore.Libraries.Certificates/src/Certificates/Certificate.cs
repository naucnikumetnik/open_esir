using TaxCore.Libraries.Certificates.Extensions;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace TaxCore.Libraries.Certificates
{
    [Serializable]
    public class Certificate : X509Certificate2, ISerializable, IDisposable
    {
        protected CertRequestData _certRequestData = null;
        protected string _uniqueIdentifier;
        private CertificateTypes _certificateType;

        #region Constructors


        public Certificate() : base()
        {
        }

        public Certificate(string fileName, string password, X509KeyStorageFlags keyStorageFlags) :
            base(fileName, password, keyStorageFlags)
        {
        }

        public Certificate(X509Certificate2 certificate) :
            base(certificate)
        {
        }

        public Certificate(byte[] pfx, string password) :
            base(pfx, password)
        {
        }

        public Certificate(string pfxBase64, string password) :
            base(Convert.FromBase64String(pfxBase64), password)
        {
        }

        public Certificate(byte[] rawCert) :
            base(rawCert)
        {
        }

        protected Certificate(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var dict = info.ToDictionary();
            if (dict.ContainsKey(nameof(CertificateId)))
                CertificateId = info.GetInt32(nameof(CertificateId));
            if (dict.ContainsKey(nameof(RevokeReasonDescription)))
                RevokeReasonDescription = info.GetString(nameof(RevokeReasonDescription));
            CertificateRevokeReason = null;
            DateRevoked = null;
            if (dict.ContainsKey(nameof(CertificateRevokeReason)))
            {
                string reason = info.GetString(nameof(CertificateRevokeReason));
                if (reason != null)
                    CertificateRevokeReason = (CertificateRevokeReason)Int32.Parse(reason);
            }
            if (dict.ContainsKey(nameof(DateRevoked)))
            {
                string date = info.GetString(nameof(DateRevoked));
                if (date != null)
                    DateRevoked = DateTime.Parse(info.GetString(nameof(DateRevoked)));
            }
        }

        #endregion Constructors

        #region Properties

        public int CertificateId { get; set; }

        public virtual string UniqueIdentifier
        {
            get
            {
                if (_uniqueIdentifier != null)
                    return _uniqueIdentifier;
                return GetCertRequestData().DeviceSerialNumber;
            }
        }

        public string CommonName
        {
            get
            {
                return GetCertRequestData().CommonName;
            }
        }

        [Obsolete]
        public string CommonNameOTP
        {
            get
            {
                if (!String.IsNullOrEmpty(UniqueIdentifier))
                    return UniqueIdentifier;

                try
                {
                    return CommonName.Substring(GetCertRequestData().CommonName.IndexOf('(') + 1, 6) + "o0"; //suffix for TaxCore applications
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public virtual string RequestedBy
        {
            get
            {
                return GetCertRequestData().Email;
            }
        }

        public virtual string Organization
        {
            get
            {
                return GetCertRequestData().Organization;
            }
        }

        public virtual bool IsAuthorizedPerson
        {
            get
            {
                return (!String.IsNullOrWhiteSpace(this.GivenName) && !String.IsNullOrWhiteSpace(this.SurName));
            }
        }

        private DateTime? _expiryDate;

        public virtual DateTime ExpiryDate
        {
            get
            {
                if (_expiryDate == null) return NotAfter;
                else return (DateTime)_expiryDate;
            }
            set
            {
                _expiryDate = value;
            }
        }

        public string RevokeReasonDescription { get; set; }

        public DateTime? DateRevoked { get; set; } = null;

        public CertificateRevokeReason? CertificateRevokeReason { get; set; } = null;

        public CertificateTypes CertificateType
        {
            get
            {
                if (_certificateType != CertificateTypes.Unknown)
                    return _certificateType;

                return ExtractCertificateType();
            }
            set
            {
                _certificateType = value;
            }
        }

        public int? CardId { get; set; }

        public string GivenName
        {
            get
            {
                return GetCertRequestData().GivenName;
            }
        }

        public string SurName
        {
            get
            {
                return GetCertRequestData().SurName;
            }
        }

        public string OrganizationUnit
        {
            get
            {
                return GetCertRequestData().OrganizationUnit;
            }
        }

        public string StreetAddress
        {
            get
            {
                return GetCertRequestData().StreetAddress;
            }
        }

        public string State
        {
            get
            {
                return GetCertRequestData().State;
            }
        }

        public bool IsEncryption => CertificateType == CertificateTypes.CertificateClassV36;

        public bool IsSigning => CertificateType == CertificateTypes.CertificateClassV33 || CertificateType == CertificateTypes.CertificateClassV35 || CertificateType == CertificateTypes.CertificateClassV38;

        public bool IsAuthentication => CertificateType == CertificateTypes.CertificateClassV32 || CertificateType == CertificateTypes.CertificateClassV34 || CertificateType == CertificateTypes.CertificateClassV37;

        public virtual RSA GetPrivateKeyRSA() => this.GetRSAPrivateKey();
        public virtual RSA GetPublicKeyRSA() => this.GetRSAPublicKey();

        #endregion Properties

        #region Public methods

        public string ExtractTIN()
        {
            foreach (System.Security.Cryptography.X509Certificates.X509Extension ext in Extensions)
            {
                if (ext.Oid.Value.StartsWith("1.3.6.1.4.1.49952.") && ext.Oid.Value.Split('.')[9] == "6")
                {
                    return Encoding.Default.GetString(ext.RawData);
                }
            }
            return string.Empty;
        }

        public string ExtractTaxCoreApiUrl()
        {
            foreach (System.Security.Cryptography.X509Certificates.X509Extension ext in Extensions)
            {
                if (ext.Oid.Value.StartsWith("1.3.6.1.4.1.49952.") && ext.Oid.Value.Split('.')[9] == "5")
                {
                    return Encoding.Default.GetString(ext.RawData);
                }
            }
            return string.Empty;
        }

        public void Dispose()
        {
            this.Reset();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                info.AddValue(nameof(RawData), RawData);
            }
            catch (Exception)
            {
                info.AddValue(nameof(RawData), null);
            }
            info.AddValue(nameof(CertificateId), CertificateId);
            info.AddValue(nameof(RevokeReasonDescription), RevokeReasonDescription);
            info.AddValue(nameof(DateRevoked), DateRevoked);
            info.AddValue(nameof(CertificateRevokeReason), CertificateRevokeReason);
        }

        #endregion Public methods

        #region Private methods

        private CertificateTypes ExtractCertificateType()
        {
            if (this.Handle == IntPtr.Zero)
                return CertificateTypes.Unknown;

            try
            {
                foreach (System.Security.Cryptography.X509Certificates.X509Extension ext in Extensions)
                {
                    if (IsEnhancedKeyUsage(ext))
                    {
                        foreach (var item in (ext as X509EnhancedKeyUsageExtension).EnhancedKeyUsages)
                        {
                            if (item.Value.StartsWith("1.3.6.1.4.1.49952."))
                            {
                                var segments = item.Value.Split('.');
                                return (CertificateTypes)Enum.Parse(typeof(CertificateTypes), segments[9] + segments[10]);
                            }
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                return CertificateTypes.Unknown;
            }

            return CertificateTypes.Unknown;
        }

        private bool IsEnhancedKeyUsage(System.Security.Cryptography.X509Certificates.X509Extension ext)
        {
            return (ext.Oid.Value == OIDs.ExtendedKeyUsage);
        }

        private CertRequestData GetCertRequestData()
        {
            if (_certRequestData == null)
                _certRequestData = ExtractCertRequestData();
            return _certRequestData;
        }

        private CertRequestData ExtractCertRequestData()
        {
            if (Handle == IntPtr.Zero)
                return null;

            var data = new CertRequestData();
            byte[] rawData = base.SubjectName.RawData;
            Asn1Object asn1Object = Asn1Object.FromByteArray(rawData);

            if (asn1Object is Asn1Sequence sequence)
            {
                foreach (Asn1Encodable encodable in sequence)
                {
                    if (encodable is Asn1Set set && set.Count > 0)
                    {
                        if (set[0] is Asn1Sequence keyValueSequence && keyValueSequence.Count > 1)
                        {
                            if (keyValueSequence[0] is DerObjectIdentifier oid)
                            {
                                if (oid.Id == X509Name.CN.Id)
                                {
                                    data.CommonName += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.OU.Id)
                                {
                                    data.OrganizationUnit += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.O.Id)
                                {
                                    data.Organization += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.L.Id)
                                {
                                    data.Locality += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.ST.Id)
                                {
                                    data.State += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.C.Id)
                                {
                                    data.Country += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.SerialNumber.Id)
                                {
                                    data.DeviceSerialNumber += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.DC.Id)
                                {
                                    data.DomainComponent += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.EmailAddress.Id)
                                {
                                    data.Email += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.GivenName.Id)
                                {
                                    data.GivenName += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.Street.Id)
                                {
                                    data.StreetAddress += keyValueSequence[1].ToString();
                                }
                                if (oid.Id == X509Name.Surname.Id)
                                {
                                    data.SurName += keyValueSequence[1].ToString();
                                }
                            }
                        }
                    }
                }
            }

            return data;
        }

        #endregion Private methods
    }
}
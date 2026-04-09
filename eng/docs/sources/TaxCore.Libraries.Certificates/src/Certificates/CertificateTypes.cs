using System;
using System.Security.Cryptography.X509Certificates;

namespace TaxCore.Libraries.Certificates
{
    /// <summary>
    /// Types of certificates used in system
    /// </summary>
    public enum CertificateTypes
    {
        Unknown = 00,
        /// <summary>
        /// web/ssl certificate class 1
        /// </summary>
        CertificateClassV31 = 31,

        /// <summary>
        /// HTTPS auth certificate class 1 used for (V-SDC HTTPS comunnication and POS HTTPS communication)
        /// </summary>
        CertificateClassV32 = 32,

        /// <summary>
        /// Sign data certificate class 2 used for signing data on Secure Element applet
        /// </summary>
        CertificateClassV33 = 33,

        /// <summary>
        /// HTTPS auth certificate class 2 used for PKI Applet on Smart Card
        /// </summary>
        CertificateClassV34 = 34,

        /// <summary>
        /// Sign data certificate class 1, V-SDC certificate with additional options example authorized/unauthorized
        /// </summary>
        CertificateClassV35 = 35,

        /// <summary>
        /// Encrypt data class 1 (used on SE applet for encrypting POA data(TaxCore public Key) and on VSDC for InternalData encrypt/decrypt)
        /// </summary>
        CertificateClassV36 = 36,

        /// <summary>
        /// HTTPS auth certificate class 2 used for Developer
        /// </summary>
        CertificateClassV37 = 37,

        /// <summary>
        /// Sign data certificate class 2 used for signing data on virtual Developer Secure Element
        /// </summary>
        CertificateClassV38 = 38
    }

    public class CertificateClassification : Dictionary<CertificateTypes, CertificateTemplate>
    {

    }

    public class CertificateTemplate
    {
        public string CertificateTypeOID { get; set; }

        public KeyUsage KeyUsage { get; set; } = new KeyUsage();

        public EnhancedKeyUsage EnhancedKeyUsage { get; set; } = new EnhancedKeyUsage();

        public Policy Policy { get; set; } = new Policy();

        public CustomExtension TaxCoreApi { get; set; } = new CustomExtension();

        public CustomExtension TIN { get; set; } = new CustomExtension();

        public CustomExtension VSDCApi { get; set; } = new CustomExtension();
    }

    public class KeyUsage : List<X509KeyUsageFlags>
    {

    }

    public class EnhancedKeyUsage : List<string>
    {

    }

    public class Policy
    {
        public string OID { get; set; }
        public string URL { get; set; }
        public string Notice { get; set; }
    }

    public class CustomExtension
    {
        public string OID { get; set; }
        public string Value { get; set; }
    }

    public static class OIDs
    {
        public static string ExtendedKeyUsage = "2.5.29.37";

        public static string CertificateClassV31 = "3.1";
        public static string CertificateClassV32 = "3.2";
        public static string CertificateClassV33 = "3.3";
        public static string CertificateClassV34 = "3.4";
        public static string CertificateClassV35 = "3.5";
        public static string CertificateClassV36 = "3.6";
        public static string CertificateClassV37 = "3.7";
        public static string CertificateClassV38 = "3.8";
    }

}

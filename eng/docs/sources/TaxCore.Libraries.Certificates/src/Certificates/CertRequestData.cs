using System;

namespace TaxCore.Libraries.Certificates
{
    public class CertRequestData
    {
        public string Email { get; set; } = string.Empty;

        public string CommonName { get; set; } = string.Empty;

        public string DeviceSerialNumber { get; set; } = string.Empty;

        public string GivenName { get; set; } = string.Empty;

        public string SurName { get; set; } = string.Empty;

        public string OrganizationUnit { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;

        public string StreetAddress { get; set; } = string.Empty;

        public string Locality { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string DomainComponent { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string TIN { get; set; } = string.Empty;

        public int? ValidityPeriodUnits { get; set; }

        public string ValidityPeriod { get; set; } = string.Empty;
    }
}

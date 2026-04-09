using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaxCore.Libraries.Certificates
{
    public enum CertificateRevokeReason
    {
        /// <summary>
        /// The cr l_ reaso n_ unspecified
        /// </summary>
        [Display]
        [Description("The reason is not specified")]
        CRL_REASON_UNSPECIFIED = 0,

        /// <summary>
        /// The cr l_ reaso n_ keycompromise
        /// </summary>
        [Display]
        [Description("Private key has been compromised")]
        CRL_REASON_KEY_COMPROMISE = 1,


        /// <summary>
        /// The cr l_ reaso n_ cacompromise
        /// </summary>
        [Description("Certificate authority has been compromised")]
        CRL_REASON_CA_COMPROMISE = 2,


        /// <summary>
        /// The cr l_ reaso n_ affiliationchanged
        /// </summary>
        [Display]
        [Description("Certificate holder changed its affiliation.")]
        CRL_REASON_AFFILIATION_CHANGED = 3,

        /// <summary>
        /// The cr l_ reaso n_ superseded/
        /// </summary>
        [Display]
        [Description("Certificate has been superseded")]
        CRL_REASON_SUPERSEDED = 4,

        /// <summary>
        /// The cr l_ reaso n_ cessationofoperati on
        /// </summary>
        [Display]
        [Description("Certificate holder is decommissioned or retired.")]
        CRL_REASON_CESSATION_OF_OPERATION = 5,

        /// <summary>
        /// The cr l_ reaso n_ certificatehold/
        /// </summary>
        [Description("Certificate is on hold")]
        CRL_REASON_CERTIFICATE_HOLD = 6,

        /// <summary>
        /// The cr l_ reaso n_ PrivilegeWithdrawn/
        /// </summary>
        [Description("Certificate holder no longer have permissions to use certificate")]
        CRL_REASON_PRIVILEGE_WITHDRAWN = 7,

        /// <summary>
        /// The cr l_ reaso n_ ReleaseFromHold/
        /// </summary>
        [Description("Certificate is removed from CRL")]
        CRL_REASON_RELEASE_FROM_HOLD = 8,

        /// <summary>
        /// The cr l_ reaso n_ AACompromise/
        /// </summary>
        [Description("Authorization Authority is compromised")]
        CRL_REASON_AACOMPROMISE = 10
    }
}

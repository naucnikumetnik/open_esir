# Introduction

This publically available Nuget package is used for working with certificates created by the TaxCore solution.

# Usage

This package can be used to export the following certificates data:

- TIN - Taxpayer identification number
- TaxCoreApiUrl - URL of TaxCore API
- UniqueIdentifier (UID) - Unique identifier of the certificate in the system
- ExpiryDate - Date of certificate expiration
- CertificateType - Type of certificate in the system
  - IsEncryption - Type V36 - used for data encryption
  - IsSigning - Type V33, V35 and V38 - used for applying digital signature
  - IsAuthentication - Type V32, V34 and V37 - used for authentication to web services
- DateRevoked - Date of certificate revocation (exists if the certificate is revoked)
- RevokeReasonDescription - Additional information about revocation entered manually 
- CertificateRevokeReason - Reason for revocation selected from the list
- Subject - Certificate's subject
  - CommonName - Certificate Common Name - first 4 characters of the secure element's UID and the Shop name
  - OrganizationUnit - Shop name 
  - Organization - Business name
  - Locality - City/town,
  - State - State, District or Region,
  - Country - Country,
  - DeviceSerialNumber - UID of the taxpayer's secure element,
  - DomainComponent - Not currently in use,
  - Email - Not currently in use,
  - GivenName - Not currently in use,
  - StreetAddress - Physical street address,
  - SurName - Authorized Person first name.
  

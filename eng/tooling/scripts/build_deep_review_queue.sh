#!/usr/bin/env bash
set -euo pipefail

repo_root="${1:-.}"
audit_csv="$repo_root/eng/fiscal_catalog/working/corpus_file_audit.csv"
out_csv="$repo_root/eng/fiscal_catalog/working/deep_review_queue.csv"

printf 'FilePath,ReviewPath,ReviewLines,KeywordHits,QueueReason\n' > "$out_csv"

tail -n +2 "$audit_csv" | while IFS=, read -r raw_path raw_review _kind lines hits _relevance _tier _notes; do
  path=${raw_path//\"/}
  review=${raw_review//\"/}
  reason=""

  case "$path" in
    knowledge_base/Concepts.pdf|\
    knowledge_base/Technical-Documentation.pdf|\
    knowledge_base/Tehnickouputstvo-ESIRiliL-PFR.pdf|\
    knowledge_base/Апликације.pdf|\
    knowledge_base/Обавезе_обвезника_фискализације.pdf|\
    knowledge_base/Пореска\ управа\ __\ Одговори\ на\ најчешћa\ питања.html|\
    knowledge_base/RucnoTestiranjeLPFR-a_v10.pdf|\
    knowledge_base/RunotestiranjeESIRa.pdf|\
    knowledge_base/Pravilnikovrstamafiskalnihraunatipovimatransakcija.pdf|\
    knowledge_base/ifarnikjedinstvenogIDkupca_ver_116_18122023.pdf|\
    knowledge_base/Pravilnik_bezbednosni_element.pdf|\
    knowledge_base/Overview.pdf|\
    knowledge_base/Tehnickivodic.pdf|\
    knowledge_base/Zakonofiskalizaciji.pdf|\
    knowledge_base/Концепти.pdf|\
    knowledge_base/Техничка-документација.pdf|\
    knowledge_base/Pasted\ text\(1\).txt|\
    knowledge_base/efiskalizacija_minimalni_starter_set.md|\
    knowledge_base/efiskalizacija_resursi_sr.md)
      reason="top_level_source"
      ;;

    knowledge_base/Taxcore-MobilePOS-Android/README.md|\
    knowledge_base/Taxcore-MobilePOS-Android/CHANGELOG.md)
      reason="reference_overview"
      ;;

    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/ui/invoice/InvoiceFragment.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/ui/invoice/InvoiceBottomDialog.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/ui/invoice/FiscalInvoiceFragment.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/params/InvoiceRequest.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/models/InvoiceResponse.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/models/StatusResponse.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/services/SdcService.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/local/JournalManager.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/realm/Journal.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/api/ApiService.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/api/ApiServiceESDC.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/PrefService.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/utils/TCUtil.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/extensions/StringExtensions.kt|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/enums/InvoiceOption.kt)
      reason="reference_android_core"
      ;;

    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/res/values/strings.xml|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/res/values-sr/strings.xml|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/res/values/arrays.xml|\
    knowledge_base/Taxcore-MobilePOS-Android/app/src/main/res/values-sr/arrays.xml)
      reason="reference_android_labels"
      ;;

    knowledge_base/Secure-Element-Reader/README.md|\
    knowledge_base/Secure-Element-Reader/README.sr.md|\
    knowledge_base/Secure-Element-Reader/README.fr.md)
      reason="reference_se_readme"
      ;;

    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Services/CardReaderService.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Services/ApduCommandService.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/ViewModels/VerifyPinDialogViewModel.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Proxies/TaxCoreApiProxy.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Interfaces/IApduCommandService.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Interfaces/ICardReaderService.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Interfaces/ITaxCoreApiProxy.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Enums/ApduInstructions.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Enums/ApduClasses.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Enums/ApduP1.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Enums/ApduP2.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Models/VerifyPinModel.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Models/SecureElementAuditRequest.cs|\
    knowledge_base/Secure-Element-Reader/src/SecureElementReader/Enpoints/EndpointUrls.cs)
      reason="reference_se_core"
      ;;

    knowledge_base/TaxCore.Libraries.Certificates/README.md|\
    knowledge_base/TaxCore.Libraries.Certificates/src/Certificates/Certificate.cs|\
    knowledge_base/TaxCore.Libraries.Certificates/src/Certificates/CertificateTypes.cs|\
    knowledge_base/TaxCore.Libraries.Certificates/src/Certificates/CertificateRevokeReason.cs|\
    knowledge_base/TaxCore.Libraries.Certificates/src/Certificates/CertRequestData.cs)
      reason="reference_cert_core"
      ;;

    knowledge_base/VSDCRequestSubmitterV3.0/README.md|\
    knowledge_base/VSDCRequestSubmitterV3.0/VSDCRequestSubmitter/Models/InvoiceRequest.cs|\
    knowledge_base/VSDCRequestSubmitterV3.0/VSDCRequestSubmitter/Proxies/VSDCApiProxy.cs|\
    knowledge_base/VSDCRequestSubmitterV3.0/VSDCRequestSubmitter/VSDCRequestSubmitter.cs)
      reason="reference_vsdc_core"
      ;;

    knowledge_base/Dti.TaxCore.Examples.SESignInvoice/README.md|\
    knowledge_base/Dti.TaxCore.Examples.SESignInvoice/SESignInvoice/ESDCFixture.cs|\
    knowledge_base/Dti.TaxCore.Examples.SESignInvoiceV3.0/README.md|\
    knowledge_base/Dti.TaxCore.Examples.SESignInvoiceV3.0/SESignInvoice/ESDCFixture.cs)
      reason="reference_se_example"
      ;;
  esac

  if [[ -n "$reason" ]]; then
    printf '"%s","%s",%s,%s,%s\n' "$path" "$review" "$lines" "$hits" "$reason" >> "$out_csv"
  fi
done

wc -l "$out_csv"

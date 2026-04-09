# eFiskalizacija — minimalni starter set (zvanični resursi)

Ovo je skraćeni paket sa **12 najbitnijih zvaničnih resursa** za početak razvoja i razumevanja eFiskalizacije u Srbiji.

## 1) Must read prvo

### 1. eFiskalizacija portal
**Šta je ovo:** Glavna ulazna tačka ka zakonima, pravilnicima, FAQ-u, registru odobrenih elemenata i tehničkim materijalima.  
**Zašto ti treba:** Odavde krećeš i odavde proveravaš da li je nešto i dalje aktuelno.  
**Link:** https://www.purs.gov.rs/eFiskalizacija.html

### 2. Zakon o fiskalizaciji
**Šta je ovo:** Krovni zakon.  
**Zašto ti treba:** Da razumeš ko je obveznik, šta je elektronski fiskalni uređaj i gde su granice sistema.  
**Link:** https://www.purs.gov.rs/eFiskalizacija/zakoni/7775/zakon-o-fiskalizaciji.html

### 3. Pravilnik o vrstama fiskalnih računa, tipovima transakcija, načinima plaćanja i ostalim elementima računa
**Šta je ovo:** Glavni domenski pravilnik za račun.  
**Zašto ti treba:** Ovo je osnova za tvoj generator računa — vrste računa, tipovi transakcija, načini plaćanja i elementi računa.  
**Link:** https://www.purs.gov.rs/eFiskalizacija/pravilnici/8275/pravilnik-o-vrstama-fiskalnih-racuna-tipovima-transakcija-nacinima-placanja-pozivanju-na-broj-drugog-dokumenta-i-pojedinostima-ostalih-elemenata-fiskalnog-racuna-.html

### 4. Tehničko uputstvo za administrativni i tehnički pregled funkcionalnosti ESIR-a ili L-PFR-a
**Šta je ovo:** Najbitniji tehnički dokument.  
**Zašto ti treba:** Tu su pitanja za samoprocenu, tipovi proizvoda, operativne funkcije, tekstualni prikaz računa, posebni slučajevi i approval logika.  
**Link (PDF):** https://purs.gov.rs/upload/media/2025/2/4/414144/Tehnickouputstvo-ESIRiliL-PFR.pdf  
**Link (stranica):** https://www.purs.gov.rs/eFiskalizacija/tehnicki-vodic/7343/tehnicko-uputstvo-za-administrativni-i-tehnicki-pregled-funkcionalnosti-esir-a-ili-l-pfr-a-.html

### 5. Obaveze obveznika fiskalizacije
**Šta je ovo:** Kratko, pregledno uputstvo na ljudskom jeziku.  
**Zašto ti treba:** Dobro za brzo slaganje mentalnog modela: BE, SUF, ESF, obveznik, poslovni prostor.  
**Link:** https://purs.gov.rs/upload/media/2025/3/7/759121/Obaveze_obveznika_fiskalizacije_jan.pdf

### 6. Odgovori na najčešća pitanja
**Šta je ovo:** Zvanični FAQ.  
**Zašto ti treba:** Kada zapneš na rubnim slučajevima ili praktičnim tumačenjima.  
**Link:** https://www.purs.gov.rs/eFiskalizacija/odgovori_najcesca_pitanja.html

## 2) Treba za razvoj ESIR-a

### 7. Registracija za Razvojno okruženje
**Šta je ovo:** Onboarding za dobavljače u supplier sandbox.  
**Zašto ti treba:** Bez ovoga nema pristupa razvojnim alatima i approval toku.  
**Link:** https://tap.sandbox.suf.purs.gov.rs/help/view/1813353054/Регистрација-за-Развојно-окружење/sr-Cyrl-RS

### 8. Razvojni L-PFR
**Šta je ovo:** Softverska razvojna verzija lokalnog procesora fiskalnih računa.  
**Zašto ti treba:** Ako prvo razvijaš ESIR, ovo je najbitniji partner za integraciju i test.  
**Link:** https://tap.sandbox.suf.purs.gov.rs/Help/view/1904691288/Развојни-Л-ПФР/sr-Cyrl-RS

### 9. VSDC Request submitter
**Šta je ovo:** Pomoćni alat za simulaciju rada virtuelnog procesora.  
**Zašto ti treba:** Koristan kada razvijaš ESIR i proveravaš interakciju prema državnom sistemu.  
**Link:** https://tap.sandbox.suf.purs.gov.rs/Help/view/1904691288/VSDC-Request-submitter/sr-Cyrl-RS

## 3) Treba za razvoj L-PFR-a

### 10. SE Sign Invoice
**Šta je ovo:** Alat za testiranje slanja APDU komandi ka bezbednosnom elementu na pametnoj kartici.  
**Zašto ti treba:** Ako praviš svoj L-PFR, ovo je direktno bitno za rad sa bezbednosnim elementom.  
**Link:** https://tap.sandbox.suf.purs.gov.rs/Help/view/255104265/SE-Sign-Invoice/sr-Cyrl-RS

### 11. SDC Analyzer Win App
**Šta je ovo:** Zvanični alat za testiranje lokalnog procesora fiskalnih računa.  
**Zašto ti treba:** Deo je approval priče za L-PFR i važan je za formalno testiranje.  
**Link:** https://tap.sandbox.suf.purs.gov.rs/Help/view/255104265/SDC-Analyzer-Win-App/sr-Cyrl-RS

## 4) Treba za approval

### 12. Proces odobravanja
**Šta je ovo:** Zvanični pregled approval toka u supplier okruženju.  
**Zašto ti treba:** Da vidiš kako ide prijava, tehnički deo, administrativni deo i finalizacija odobrenja.  
**Link:** https://tap.sandbox.suf.purs.gov.rs/Help/view/502568074/Процес-одобравања/sr-Cyrl-RS

---

## Preporučeni redosled

1. eFiskalizacija portal  
2. Zakon o fiskalizaciji  
3. Pravilnik o vrstama fiskalnih računa...  
4. Tehničko uputstvo za ESIR / L-PFR  
5. Obaveze obveznika fiskalizacije  
6. FAQ  
7. Registracija za Razvojno okruženje  
8. Razvojni L-PFR i VSDC Request submitter  
9. SE Sign Invoice i SDC Analyzer  
10. Proces odobravanja

---

## Kratka mapa

- **Generator računa** najviše zavisi od:  
  - Zakona  
  - Pravilnika o vrstama fiskalnih računa  
  - Tehničkog uputstva

- **ESIR** najviše zavisi od:  
  - Tehničkog uputstva  
  - Registracije za Razvojno okruženje  
  - Razvojnog L-PFR-a  
  - VSDC Request submitter-a

- **L-PFR** najviše zavisi od:  
  - Tehničkog uputstva  
  - SE Sign Invoice  
  - SDC Analyzer

- **Approval** najviše zavisi od:  
  - Tehničkog uputstva  
  - Procesa odobravanja

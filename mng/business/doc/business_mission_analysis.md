artifact_kind: business_mission_analysis
schema_version: "0.1"

# Fiscal Core Business and Mission Analysis

## card

- **id:** BMA_FISCAL_CORE_RS
- **name:** Poslovna i misijska analiza za modularni fiskalni core za Srbiju
- **system_of_interest:** Modularni ESIR + L-PFR / V-PFR integracioni core
- **viewpoint_ref:** VP-business-mission
- **level:** system
- **scope:** Početna poslovna i misijska analiza za vendor-neutral fiskalni core namenjen integraciji u druge proizvode i poslovne sisteme
- **purpose:** Definisati problem, misiju, zainteresovane strane, ciljeve, uspeh, ograničenja, rizike i preporučeni pravac pre detaljnijih zahteva i arhitekture

---

## executive_summary

### problem_or_opportunity
U Srbiji je fiskalizacija regulatorno standardizovana, ali je tržišno često isporučena kroz zatvorene proizvode, vendor-specifične stack-ove ili usko profilisane servise. To otežava firmama i softverskim integratorima da objedine kompletan poslovni workflow u jednoj platformi, jer fiskalni sloj često nije dostupan kao neutralan, modularan i lako ugradiv core.

### mission_statement
Omogućiti upotrebu fiskalizacije u Srbiji kao modularne infrastrukturne komponente, a ne samo kao zatvorenog krajnjeg proizvoda.

### recommended_direction
Krenuti usko: prvo validirati ESIR jezgro sa jasno definisanim tipovima, pravilima računa i integracionim granicama, uz mogućnost rada prema V-PFR putanji, a tek zatim širiti prema sopstvenom L-PFR sloju i širim adapterima.

---

## business_context

### background
Na tržištu već postoji veliki broj odobrenih fiskalnih elemenata, ali su mnoga rešenja praktično vezana za konkretne uređaje, konkretne POS proizvode ili zatvorene vendor ekosisteme. Za standardnog krajnjeg korisnika to može biti prihvatljivo, ali za firme koje žele da grade sopstvene platforme, vertikalna rešenja ili objedinjene workflow sisteme, fiskalizacija često postaje ograničavajući deo arhitekture.

### current_pain_points
- Fiskalni sloj je često vezan za konkretan proizvod, uređaj ili vendor ekosistem.
- Integracija u sopstvene sisteme ume da bude teška, ograničena ili neprirodna.
- Poslovni workflow-i ostaju razbijeni između više sistema.
- Postoji vendor lock-in i mala prenosivost rešenja.
- Isti tip fiskalnog jezgra se više puta razvija ili obilazi kroz workaround pristupe.
- Podrška za različite deployment modele i uređaje nije dosledno rešena.

### expected_business_value
- Lakša integracija fiskalizacije u sopstvene poslovne sisteme
- Smanjenje vendor lock-in-a
- Jedinstveniji i čistiji poslovni workflow-i
- Veća prenosivost između uređaja, deployment modela i use-case-ova
- Mogućnost da drugi grade svoje proizvode na istom fiskalnom jezgru
- Bolja dugoročna kontrola nad fiskalnim slojem

---

## mission_definition

### mission
Napraviti modularni fiskalni core za Srbiju koji jasno razdvaja receipt issuing, fiskalizaciju, pristup bezbednosnom elementu i izlazne adaptere, tako da ga drugi mogu koristiti kao neutralnu infrastrukturnu komponentu.

### vision_of_success
Postoji proizvod koji se može:
- ugraditi u tuđi poslovni softver,
- koristiti kao lokalni servis,
- koristiti kao hostovani API,
uz isti domenski model i jasno definisane interfejse.

### desired_outcomes
- Jasan i stabilan model fiskalnog domena
- Modularan ESIR sloj
- Mogućnost rada sa V-PFR i kasnije L-PFR režimom
- Jasni interfejsi prema bezbednosnom elementu, poreskom backendu i izlaznim adapterima
- Osnova za prenosivo odobren proizvod
- Osnova za open-source core + zvaničnu sertifikovanu distribuciju

---

## goals_objectives_measures

### mission_goals
- Omogućiti vendor-neutral fiskalizaciju kao softverski core, a ne samo kao gotov krajnji proizvod
- Smanjiti trošak i težinu integracije fiskalizacije u druge sisteme
- Napraviti jezgro koje može da pokrije više deployment modela bez lomljenja domenskog modela
- Postaviti osnovu za dugoročno održiv i approval-friendly proizvod

### mission_objectives
- Definisati jasan model tipova i pravila za receipt issuing sloj
- Definisati jasnu granicu između ESIR odgovornosti i PFR odgovornosti
- Dokazati rad osnovnog toka izdavanja fiskalnog računa kroz uži MVP
- Obezbediti da arhitektura podrži i embedded i API pristup
- Obezbediti da se uređajski i delivery slojevi rešavaju adapterima, a ne haotično u jezgru

### measures_of_success
- Moguće je izdati i isporučiti fiskalni račun kroz definisan tok bez ad hoc logike
- Integrator može da razume i koristi spoljne interfejse bez oslanjanja na vendor-specifične workaround-e
- Fiskalni core može da se koristi u najmanje dva deployment moda bez promene domena
- Arhitektura ostaje dovoljno stabilna da kasnije podrži approval i održavanje
- Početni korisnici vide jasnu vrednost u odnosu na zatvorene ili teško integrabilne alternative

---

## stakeholders

### major_stakeholders
- **Osnivač / sponzor / budući nosilac proizvoda**  
  Zainteresovan za stvaranje održivog i diferenciranog proizvoda.

- **Softverski integrator**  
  Zainteresovan za čist API ili ugrađeni core koji može da uklopi u svoj sistem.

- **Firma koja razvija sopstveni poslovni softver**  
  Zainteresovana da fiskalizaciju ugradi bez vezivanja za tuđi zatvoreni stack.

- **Krajnji korisnik poslovnog sistema (npr. restoran, maloprodaja, vertikalni operater)**  
  Zainteresovan za objedinjeni workflow i pouzdan rad.

- **Poreski/regulatorni okvir**  
  Nije “korisnik” u klasičnom smislu, ali nameće granice koje proizvod mora da poštuje.

- **Budući partneri i developeri oko open-source/core ekosistema**  
  Zainteresovani za transparentnost, stabilnost i jasnoću granica sistema.

---

## problem_definition

### problem_statement
Problem u srpskom fiskalnom softverskom ekosistemu nije odsustvo rešenja za izdavanje fiskalnih računa, već odsustvo dovoljno otvorenog, modularnog i vendor-neutral fiskalnog jezgra koje se lako ugrađuje u druge sisteme i proizvode.

### affected_parties
- softverski integratori
- firme koje žele sopstveni poslovni softver
- firme koje žele objedinjene workflow-e
- tehnički timovi koji održavaju integracije
- krajnji korisnici koji danas rade kroz više nepovezanih sistema

### causes_or_contributing_factors
- istorijsko formiranje rešenja oko gotovih POS/ERP proizvoda
- vendor-specifični ekosistemi
- slaba dostupnost fiskalnog sloja kao neutralne infrastrukturne komponente
- mešanje poslovne logike, uređajskog sloja i fiskalnog jezgra
- fokus tržišta na krajnji proizvod, a ne na ugradive komponente

### consequences_of_inaction
- nastavak fragmentisanih workflow-a
- vendor lock-in
- visoki troškovi integracije i prilagođavanja
- ponavljanje razvoja istih ili sličnih komponenti
- otežano širenje na nove uređaje i use-case-ove
- ograničena mogućnost da se oko fiskalnog sloja razvija širi softverski ekosistem

---

## scope_and_boundaries

### in_scope
- modularni receipt issuing / ESIR sloj
- jasan domenski model fiskalnog računa
- jasna granica prema PFR sloju
- podrška za V-PFR integracionu putanju u ranom stadijumu
- adapter pristup za izlaz računa i uređaje
- approval-friendly razdvajanje odgovornosti
- osnova za open-source core

### out_of_scope
- pun POS proizvod kao početni cilj
- kompletan restoran/retail operativni sistem
- bogati UI kanali kao primarni fokus prve verzije
- masovna hardverska podrška od prvog dana
- širok multi-domain rollout pre validacije jezgra

### assumptions
- postoji realna potreba za neutralnim fiskalnim core-om
- deo tržišta želi ugradivo ili API-first rešenje, ne samo gotov POS
- modularnost i jasni interfejsi imaju tržišnu vrednost
- approval i compliance teret je upravljiv ako se jezgro drži tvrdo i stabilno

### constraints
- proizvod mora ostati usklađen sa regulatornim pravilima Srbije
- approval lifecycle ne sme biti zanemaren
- jezgro mora ostati dovoljno usko da ne postane “još jedan POS”
- uređajski sloj mora biti adapterizovan, ne ugrađen haotično u core

---

## operating_context_summary

### intended_operational_context
Sistem je namenjen za upotrebu u poslovnim softverima i integracionim scenarijima gde je potrebno izdavanje fiskalnih računa u Srbiji uz mogućnost rada preko postojećeg poreskog ekosistema i kasnije širenje na sopstveni lokalni fiskalni procesor.

### lifecycle_concepts_preliminary
- modelovanje računa
- izdavanje zahteva za fiskalizaciju
- fiskalizacija kroz V-PFR ili L-PFR putanju
- dostavljanje / štampa / elektronska isporuka računa
- verzionisanje i approval lifecycle
- održavanje i eventualno povlačenje proizvoda

### external_dependencies_summary
- regulatorni okvir Poreske uprave Srbije
- dostupni protokoli i operativni tokovi poreskog sistema
- bezbednosni element u režimima gde je potreban
- uređaji i izlazni kanali kod krajnjih korisnika
- odluke integratora o tome da li koriste embedded, service ili hosted model

---

## solution_space

### candidate_solution_classes

#### 1. Integracija na postojeće zatvorene vendor proizvode
**Opis:** Osloniti se na postojeće ESIR/LPFR/POS vendore i graditi samo pomoćne slojeve oko njih.  
**Pros:** brži ulazak, manji inicijalni compliance teret  
**Cons:** vendor lock-in, ograničena kontrola, slaba prenosivost

#### 2. Gotov krajnji POS proizvod
**Opis:** Napraviti kompletan krajnji proizvod za restorane, maloprodaju ili slične korisnike.  
**Pros:** lakša direktna prodajna priča ka krajnjem korisniku  
**Cons:** ogromno širenje scope-a, hardverski i operativni teret, rizik da se izgubi suština proizvoda

#### 3. Modularni fiskalni core
**Opis:** Napraviti ugradiv i/ili API-first fiskalni core sa jasnim granicama prema issuing, fiskalizaciji, secure element sloju i delivery adapterima.  
**Pros:** čista diferencijacija, veća prenosivost, bolji fit za integratore i platforme  
**Cons:** teže tržišno objašnjenje, potreba za vrlo disciplinovanom arhitekturom

### preferred_solution_class
Modularni fiskalni core

### rationale_for_preference
Ovaj pravac najbolje odgovara identifikovanom problemu. Cilj nije samo još jedno krajnje rešenje, već infrastrukturna komponenta koja omogućava drugima da grade sopstvene proizvode bez nepotrebnog vezivanja za zatvorene vendor stack-ove.

---

## risks_and_uncertainties

### uncertainties
- kolika je stvarna spremnost integratora da koriste ovakav proizvod
- koliko tržište vrednuje otvoren i modularan pristup naspram gotovih rešenja
- koliko se jasno može objasniti razlika između core proizvoda i običnog POS-a
- koliko approval i lifecycle održavanje utiču na brzinu razvoja

### key_risks
- proizvod sklizne u razvoj kompletnog POS-a i izgubi fokus
- modularnost postane samo arhitektonska lepota bez tržišne vrednosti
- approval-friendly zahtevi budu potcenjeni
- adapter površina prebrzo eksplodira
- tržište traži direktan krajnji proizvod više nego neutralni core
- postojeći vendor-i delimično zatvore ili zaguše integracione prilike

### mitigation_ideas
- držati MVP uzak
- prvo validirati receipt issuing model i jasne interfejse
- ne ulaziti prerano u pun hardverski svet
- razgovarati sa potencijalnim integratorima, ne samo sa krajnjim korisnicima
- držati approval i version lineage kao sastavni deo proizvoda od početka

---

## decision_basis

### proceed_recommendation
proceed_with_narrow_proof

### justification
Problem je realan, smer je diferenciran, a postoji razuman uzak prvi korak: modularni issuing core sa jasno definisanim tipovima, pravilima i granicama, koji se može validirati bez momentalnog ulaska u pun POS ili pun L-PFR proizvod.

### review_triggers
- ako integratori ne vide dovoljno vrednosti u neutralnom core pristupu
- ako proizvod prebrzo sklizne u veliki POS scope
- ako modularnost ne daje praktičnu prednost pri integraciji
- ako approval i lifecycle trošak postanu nesrazmerni očekivanoj vrednosti

---

## relationships

### related_context_refs
- razgovori o Octopos ograničenjima i potrebi za objedinjavanjem workflow-a
- razrada razlika između ESIR, L-PFR, V-PFR i bezbednosnog elementa
- razrada ideje o open-source core-u i zvaničnoj sertifikovanoj distribuciji

### source_refs
- osnivačevo iskustvo sa postojećim zatvorenim rešenjima
- početna analiza tržišta i regulatornog okvira
- radne diskusije o mogućem modularnom fiskalnom proizvodu

---

## notes
- Proizvod ne treba pozicionirati kao “još jedan POS”, već kao fiskalni core.
- Nazivlje treba pažljivo uskladiti između regulatornog jezika i TaxCore tehničkog jezika.
- Open-source pristup ima smisla samo uz jasnu razliku između community code-a i zvanične sertifikovane distribucije.

---

## status

- **lifecycle_state:** draft
- **owner:** Bratislav
- **reviewers:** []
- **version:** "0.1"
- **last_updated:** "2026-04-08"
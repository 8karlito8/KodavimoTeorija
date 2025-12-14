#### Bendroji dalis

Duotas kodas C virš Fq. Modeliuoti jo veikimą:

- užkoduoti informaciją,
- siųsti ją nepatikimu kanalu, t.y. su duota tikimybe pe atsitiktinai joje padaryti klaidų,
- dekoduoti, naudojant nurodytą dekodavimo algoritmą iš duotos literatūros.

Konkretus kodas ir kiti parametrai priklauso nuo užduoties numerio. Šiame puslapyje pateikta lentelė, kurioje prie kiekvienos užduoties nurodytas konkretus kūnas, kodas, dekodavimo algoritmas ir t.t.

#### Programa vartotojo požiūriu

Programos vartotojas turi turėti galimybę nurodyti konkretų kūną (t.y. q, išskyrus tą atvejį, kai q gali įgyti tik vieną reikšmę, pavyzdžiui, q=2), konkretų kodą (t.y. parametrus, išvardintus lentelės stulpelyje "Kodo parametrai") ir klaidos tikimybę pe.

Programos vartotojo požiūriu turėtų būti realizuoti trys scenarijai:

1. Vartotojas užrašo programos nurodyto ilgio vektorių iš kūno Fq elementų. Programa patikrina, ar vektoriaus ilgis korektiškas (išimtis: jei naudojamas sąsūkos kodas, pranešimo ilgis gali būti bet koks, todėl tuo atveju programa neturi nurodyti ilgio ir tikrinti jo korektiškumo), jį užkoduoja kodu C, parodo užkoduotą vektorių, siunčia jį kanalu, parodo iš kanalo išėjusį vektorių, praneša, kiek ir kuriose pozicijose įvyko klaidų, dekoduoja gautą vektorių ir parodo rezultatą. Vartotojas turi turėti galimybę prieš dekodavimą redaguoti iš kanalo išėjusį vektorių, kad galėtų pats nurodyti klaidas ten, kur jam reikia, ir tiek, kiek reikia.
2. Vartotojas užrašo tekstą (tekstas gali būti sudarytas iš kelių eilučių). Programa suskaido duotą tekstą į reikiamo ilgio vektorius iš kūno Fq elementų (jei naudojamas sąsūkos kodas - neskaido).
    - Kiekvieną vektorių siunčia kanalu nenaudodama kodo, iš gautų vektorių atgamina tekstą ir jį parodo.
    - Kiekvieną vektorių užkoduoja, siunčia kanalu, dekoduoja. Iš gautų vektorių atgamina tekstą ir jį parodo.
3. Vartotojas nurodo paveiksliuką (bmp 24 bitų formato, nes jame geriausiai matosi kanalo iškraipymai). Programa atidaro jį ir parodo. Suskaido duotą paveiksliuką į reikiamo ilgio vektorius iš kūno Fq elementų (jei naudojamas sąsūkos kodas - neskaido).
    - Kiekvieną vektorių siunčia kanalu nenaudodama kodo, iš gautų vektorių atgamina paveiksliuką ir jį parodo.
    - Kiekvieną vektorių užkoduoja, siunčia kanalu, dekoduoja. Iš gautų vektorių atgamina paveiksliuką ir jį parodo.

Antrame ir trečiame scenarijuose vienu metu ekrane turėtų būti matomi ir pradinis tekstas (paveiksliukas), ir persiųstas nenaudojant kodo, ir persiųstas naudojant kodą, kad būtų galima įsitikinti klaidas taisančio kodo veiksmingumu.

Pastaba. Antrame ir trečiame scenarijuose pagrindinis tikslas yra įsitikinti klaidas taisančio kodo veiksmingumu. Norėdami geriau įgyvendinti šį tikslą, laikykime, kad tarnybinė informacija yra siunčiama geriau apsaugotu, patikimesniu kanalu. Praktiškai tai reiškia, kad tarnybinės informacijos nereikia siųsti jūsų realizuotu kanalu, jos nereikia iškraipyti. Tokia tarnybinė informacija atsiranda dviejose situacijose. Pirma, jei reikia papildyti duomenų srautą (pavyzdžiui, nuliais) iki reikiamo ilgio (kad būtų galima suskaidyti reikiamo ilgio vektoriais). Tokiu atveju dekodavus kai kada gali būti neaišku, kiek buvo papildyta, t. y. kiek reikia pašalinti. Kad to išvengtume, informaciją apie tai, kiek buvo papildyta, laikykime tarnybine ir jos neiškraipykime. Antra, jei iškraipoma paveiksliukų antraštėse esanti informacija, tai gali būti, kad tų paveiksliukų išvis nepavyks parodyti. Todėl paveiksliukų antraštėse esančią informaciją irgi laikykime tarnybine ir jos neiškraipykime.

#### Programa programuotojo požiūriu

Programuotojo požiūriu turėtų būti realizuoti tokie moduliai (klasės):

1. (Jei q nėra pirminis) Baigtinis kūnas Fq, veiksmai su jo elementais: sudėtis, daugyba ir t.t.
2. (Jei reikia) Veiksmai su matricomis, sudarytomis iš baigtinio kūno Fq elementų.
3. (Jei reikia) Veiksmai su polinomais virš baigtinio kūno Fq.
4. Kodavimas kodu C.
5. Siuntimas kanalu.
6. Dekodavimas, naudojant nurodytą dekodavimo algoritmą iš duotos literatūros (**dėmesio!** Turi būti realizuotas būtent nurodytas dekodavimo algoritmas iš nurodytos literatūros!).
7. (Jei reikia) Teksto (paveiksliuko) skaidymas reikiamo ilgio vektoriais iš kūno Fq elementų. Vektorių iš kūno Fq elementų sujungimas į vieną tekstą (paveiksliuką).

#### Kanalo realizacija

Laikoma, kad kanalas yra q-naris simetrinis kanalas su klaidos tikimybe pe, kur pe yra realusis skaičius, 0<=pe<=1 (turi būti galimybė klaidos tikimybę pe nurodyti pakankamai tiksliai, bent jau 0,0001 tikslumu). Kanale kiekvienas siunčiamas kūno Fq simbolis yra iškraipomas su tikimybe pe nepriklausomai nuo kitų simbolių iškraipymo. (Tai reiškia, kad gali gautis, kad klaidų nebus iš viso, o gali jų gautis ir daug. Jei klaidų skaičius neviršys dekodavimo algoritmo galimybių, klaidos bus ištaisytos, priešingu atveju gali būti dekoduojama klaidingai).

Kanalo realizacija gali būti tokia: kiekvienam siunčiamam kūno Fq elementui traukiamas atsitiktinis skaičius _a_ iš intervalo [0,1]. Jei _a_ mažesnis už klaidos tikimybę pe, siunčiamą elementą kanalas turi iškraipyti, jei ne - neturi. Kaip iškraipyti? Jei q=2, tai aišku: 0 keičiam į 1, o 1 - į 0. Jei q>2, tai iš likusių q-1 kūno Fq elementų reikia atsitiktinai išrinkti kurį nors elementą. Tam galima panaudoti jau gautą atsitiktinį skaičių _a_. Suskaidome intervalą [0,pe] į q-1 lygias dalis, patikriname, kuriai daliai priklauso _a_, ir pagal tai nusprendžiame, kuriuo iš likusių q-1 kūno Fq elementų pakeisti siunčiamą elementą.

Pastaba programuojantiems C# kalba. Atsitiktinių skaičių generatorių inicializuokite ne siuntimo kanalu funkcijoje, t. y. neinicializuokite kiekvienam siunčiamam vektoriui iš naujo, o inicializuokite tik vieną kartą, naudotojui paleidus programą. Jei inicializuosite kiekvienam siunčiamam vektoriui iš naujo, tai, kadangi inicializuoja priklausomai nuo laiko, o vektoriai siunčiami labai greitai, tai keliems iš eilės siunčiamiems vektoriams atsitiktinių skaičių generatorių inicializuos taip pat, ir jis sugeneruos lygiai tokias pačias klaidas. Tai reiškia, kad siunčiant tekstą arba paveikslėlį reikšmės bus iškraipomos ne atskiruose simboliuose ar pikseliuose, o ruožais. Formaliai kalbant, tada kanalas nebebus simetrinis kanalas, nes kanalu siunčiami simboliai nebebus iškraipomi neprikausomai vienas nuo kito.

#### Užduotys

Pateikiame uždavinių lentelę. Stulpelyje "Pastabos" pateikiame numerius pastabų, į kurias reikia atsižvelgti. Pačios pastabos pateiktos po lentele.



|   |   |   |   |   |   |   |
|---|---|---|---|---|---|---|
|**Užd. nr.**|**Kūnas**|**Kodas**|**Dekodavimo algoritmas**|**Kodo parametrai**|**Literatūra**|**Pastabos**|
|A1|q=2|Goppa kodas|§4.7.2, p. 214-217|m, L ir g|[[KTI78, p. 206–217, §4.7, ir p. 125, §2.6.6]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura20.pdf)|2, 5, 7|
|A2|q=2|Preparata kodo plėtinys (extended Preparata code) P(r)|9.5.1 algoritmas, p. 239-240|r|[[HLL91, §9.3–9.5, p. 228–242]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura19.pdf), [klaidos1](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura19-klaidos1.gif), [klaidos2](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura19-klaidos2.gif)|2|
|A3|q=2|(n,1,m) sąsūkos (konvoliucinis, convolutional) kodas|nukirstas (truncated) Viterbi dekodavimo algoritmas (8.4.1, p. 207–208, ir 8.4.12, p. 216–217, algoritmai)|n, m, generatoriai gi(x) ir dekodavimo parametras ("lango dydis") 'tau'|[[HLL91, §8.1–8.4, p. 185–217]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura18.pdf)||
|A4|q=2|(n,1,m) sąsūkos (konvoliucinis, convolutional) kodas|išsemiantis (exhaustive) dekodavimo algoritmas (§8.3)|n, m, generatoriai gi(x) ir dekodavimo parametras ("lango dydis") 'tau'|[[HLL91, §8.1–8.3, p. 185–206]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura17.pdf)||
|A5|q=2|Rydo-Miulerio (Reed-Muller) kodas **RM**(1,m)|dekodavimo algoritmas, naudojantis greitąją Hadamardo transformaciją (Fast Hadamard Transform) (3.9.4 algoritmas, p. 95)|m|[[HLL91, §3.8–3.9, p. 89–95]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura13.pdf)|7|
|A6|q - pirminis|BCH kodų atskiras atvejis|p. 136-137|n ir d|[[Hil91, p. 130–137]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura11.pdf)|1, 7|
|A7|q=2|Rydo-Miulerio (Reed, Muller) kodas **RM**(m,r)|loginės daugumos algoritmas (majority logic decoding)|m ir r|[Sta96, §5.8] arba [Sta02, §2.8] arba [Sta07, §9.7]|7, 9, 12|
|A8|q=2r, r>=2|Rydo-Solomono (Reed-Solomon) kodas|6.3.2 algoritmas, p. 150|m ir delta|[[HLL91, §6.1–6.3, p. 139–153]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura25.pdf)|2, 6, 7|
|A9|q=2|ciklinis kodas|klaidų gaudymo (error trapping) dekodavimo algoritmas (4.3.8 algoritmas, p. 111)|n ir generuojantis polinomas g(x)|[[HLL91, §4.2–4.3, p. 102–113]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura14.pdf)|6, 7, 8|
|A10|q = pm, p - pirminis, m>=1|ciklinis kodas|klaidų gaudymo (error trapping) dekodavimo algoritmas (p. 348)|n ir generuojantis polinomas g(x)|[[Rom92, p. 345–349]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura16.pdf)|3, 6, 7, 8|
|A11|q=2|tiesinis kodas C[n,k]|grandininis (step-by-step) dekodavimas (p. 79)|kodo ilgis n, dimensija k, generuojanti matrica G|[[VO89, §3.7, p. 78–81; ir p. 73]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura21.pdf)|4, 7, 11|
|A12|q=2|Goppa kodas|8.3.8 teorema, p. 403|m, L ir G(x)|[[Rom92, §8.3, p. 386–406]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura26.pdf)|2, 5, 7|
|A13|q=2|Golėjaus (Golay) kodas C23|3.7.1 algoritmas, p. 88||[[HLL91, §3.5–3.7, p. 82–89]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura12.pdf)|7|
|A14|q=2|sąsūkos (konvoliucinis, convolutional) kodas iš [Ber84, Pvz. 15.62, p. 389]|dekodavimas su grįžtamuoju ryšiu (feedback decoder) (§15.63, Pav. 15.11)||[[Ber84, §15.61–15.63, p. 388–391]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura23.pdf)|10|
|A15|q=2|sąsūkos (konvoliucinis, convolutional) kodas iš [Ber84, Pvz. 15.62, p. 389]|tiesioginis (direct) dekodavimas (§15.63, Pav. 15.11)||[[Ber84, §15.61–15.63, p. 388–391]](https://klevas.mif.vu.lt/~skersys/doc/ktkt/literatura23.pdf)|10|

_Pastabos._

1. Kūne Fq, kur q – pirminis, skaičiavimai vyksta tiesiog moduliu q.
2. Reikėtų, kad programa veiktų su visais baigtiniais kūnais F2m, kuriems [čia](https://klevas.mif.vu.lt/~skersys/doc/ktkt/prim_pol.pdf) duoti polinomai. Šiame uždavinyje skaičiavimai vyks virš F2m, todėl reikės prisiminti [baigtinių kūnų Fq sandarą](https://klevas.mif.vu.lt/~skersys/doc/ktkt/baigtiniai_kunai.pdf).
3. Reikėtų, kad programa veiktų su visais baigtiniais kūnais Fq, kur q – pirminis, ir su tais Fq, kur q = pm, p – pirminis, m>1, kuriems [čia](https://klevas.mif.vu.lt/~skersys/doc/ktkt/prim_pol.pdf) duoti polinomai. Plačiau apie baigtinius kūnus galite pasiskaityti [čia](https://klevas.mif.vu.lt/~skersys/doc/ktkt/baigtiniai_kunai.pdf).
4. Jei vartotojas pageidauja, jis turi turėti galimybę suvesti generuojančią matricą pats. Jei vartotojas to nepageidauja, programa turi sugeneruoti **atsitiktinę** generuojančią matricą, atitinkančią vartotojo nurodytus kitus parametrus. Tai reiškia, kad programa turi realizuoti abu generuojančios matricos sudarymo būdus - ir matricos suvedimo, ir matricos atsitiktinio generavimo, - ir turi leisti pačiam vartotojui pasirinkti jam labiausiai tinkantį būdą.
5. Parametras L - tai kūno F2m elementų aibės poaibis. Vartotojas turi turėti galimybę rinktis, kaip nurodyti L: arba pasirinkti visą kūną F2m, arba nurodyti, kurių elementų iš kūno F2m neįtraukti į L, arba kuriuos įtraukti į L.
6. Apie ciklinius kodus plačiau galite pasiskaityti mano konspektuose arba [Sta96, 6 skyrius].
7. Apie tiesinius kodus plačiau galite pasiskaityti mano konspektuose arba [Sta02, §1.1 ir §2.1-2.2] (arba [Sta96, §3.2, §4.1, §5.1-5.3]).
8. Programos testavimui pateikiame [ciklinių kodų generuojančių polinomų pavyzdžius](https://klevas.mif.vu.lt/~skersys/doc/ktkt/gen_pol_pvz.htm).
9. Kasmet pasitaiko, kad studentas loginės daugumos algoritmą (_majority logic decoding_) aiškinasi ir realizuoja pagal [Coo99] straipsnį, o paskui stebisi, kodėl programa neveikia korektiškai, kai r>1. Todėl perspėju, kad tame straipsnyje dekodavimo algoritmas parodytas ne iki galo, trūksta dar vieno žingsnio. Kai r=1, to žingsnio nereikia, todėl viskas veikia korektiškai, bet didesniems r jo reikia. Todėl rekomenduoju visgi gilintis ir realizuoti pagal nurodytą doc. V. Stakėno literatūrą.
10. _Komentaras._
    1. _Dekodavimo pradžia._ Pirmi iš dekodavimo schemos išeinantys bitai neturi reikšmės. Kiekvienas įeinantis bitas išsaugomas pirmajame atminties registre, paskui slenka tolyn per atminties registrus, kol po 6 laiko momentų išeina iš paskutinio atminties registro, sudedamas su sindromo bitu, atėjusiu iš apačios, ir išeina iš dekoderio. Todėl pirmus 6 iš dekoderio išeinančius bitus tiesiog ignoruokite, nes ten išeina ne informacijos bitas, o dekoderio būsenos bitas.
    2. _Kodavimo ir dekodavimo užbaigimas._ Atitinkamai baigti dekodavimą reikia ne tada, kai paskutiniai užkoduoto pranešimo bitai patenka į dekodavimo schemą, o tada, kai jie išeina iš schemos. Kaip tada užbaigti dekodavimą, kai jau visi užkoduoto pranešimo bitai pateko į dekodavimo schemą, bet dar ne visi iš jos išėjo? Įvesti nulius į dekodavimo schemą netinka, nes į ją galima įvesti tik tokias bitų poras, kurios gautos kodavimo metu. Todėl kodavimą užbaikite ne iškart, kai tik pritrūks bitų, bet dar 6 papildomus nulinius bitus užkoduokite, t. y. kol visų registrų turinys pasidarys lygus nuliui. Taip gausite ilgesnę bitų seką, kurią reiks siųsti į kanalą ir dekoduoti. Tada dekodavimą baigsite, kai tik paskutiniai iš kanalo atėję bitai (įskaitant papildomus) pateks į dekodavimo schemą, nes tuo momentu jau visi užkoduoto pranešimo bitai bus išėję iš dekodavimo schemos.
11. Užtenka, kad programa veiktų su standartinio pavidalo generuojančia matrica.
12. Jei _r_ = _m_, tai _t_ bus tuščias rinkinys, ir tokiu atveju dekodavimo algoritme turėsime vienintelį vektorių **w**_t_, sudarytą vien tik iš vienetų.

#### Vertinimas

Viso už programą galite gauti iki 100% balų. Pateikiamas vertinimas yra orientacinis ir gali kažkiek keistis. Vertinimą reikėtų interpretuoti taip: jei, pavyzdžiui, kurios nors dalies vertinimas yra 20% balų, tai už klaidas ar trūkumus toje dalyje galiu atimti iki 20% balų. Tai reiškia, pavyzdžiui, kad jei nurodytas dekodavimo algoritmas nerealizuotas, jūs automatiškai netenkate 50% balų iš 100% galimų.

- Klaidas taisantys kodai (80% balų):
    - Baigtiniai kūnai (20% balų)
    - Kodavimas (20% balų)
    - Kanalas (10% balų)
    - Dekodavimas (50% balų)
- Kita (20% balų):
    - Ataskaita (10% balų)
    - Išeities tekstai: komentarai juose, kodo tvarkingumas ir skaitomumas ir t.t. (10% balų)
    - 1, 2 ir 3 scenarijai (po 10% balų)

**Pastaba dėl darbo organizavimo. Svarbu!** Atkreipkite dėmesį į tai, kad jei nerealizuosite 2 ir 3 scenarijų, neteksite tik 20% balų iš 100% galimų, o jei nerealizuosite dekodavimo algoritmo, neteksite 50% balų. Todėl rekomenduoju pradėti nuo vieno vektoriaus kodavimo, siuntimo kanalu, dekodavimo. Jei tai korektiškai realizuosite, uždirbsite 80% balų. Tik tada, jei liks laiko, imkitės antro ir trečio scenarijų, t.y. teksto/paveiksliuko skaidymo į atskirus vektorius. Nes skaidymo realizavimas jums irgi gali užimti nemažai laiko, kurio gali pritrūkti svarbesnėms dalims.

#### Literatūra

- [Ber84] E.R. Berlekamp. Algebraic Coding Theory. Revised 1984 Edition. Aegean Park Press, Laguna Hills, CA, 1984.
- [Coo99] B. Cooke. Reed Muller Error Correcting Codes. The MIT Undergraduate Journal of Mathematics, Volume1, pp. 21-26, 1999.
- [Hil91] R.Hill. A first course in coding theory. Oxford University Press, New York, 1991.
- [HLL91] D.G.Hoffman, D.A.Leonard, C.C.Lindner, K.T.Phelps, C.A.Rodger, J.R.Wall. Coding Theory: The Essentials. Dekker, New York, 1991.
- [KTI78] T.Kasami, N.Tokura, E.Ivadari, J.Inagaki. Teorija kodirovanija. Mir, Maskva, 1978 (rusu k.).
- [Rom92] S.Roman. Coding and Information Theory. Springer-Verlag, 1992.
- [Sta96] V.Stakėnas. Informacijos kodavimas. VU leidykla, Vilnius, 1996.
- [Sta02] [V.Stakėnas. Kodavimo teorija. Paskaitų kursas, 2002.](http://www.mif.vu.lt/matinf/asm/vs/pask/cdth/cdth.htm)
- [Sta07] [V. Stakėnas. Kodai ir šifrai. Vilnius, 2007.](http://www.mif.vu.lt/lmd/kodai_sifrai.pdf)
- [VO89] S.A.Vanstone, P.C. van Oorschot. An introduction to error correcting codes with applications. Kluwer Academic Publishers, Boston, 1989.

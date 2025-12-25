# Ataskaitos kompiliavimas / Report Compilation

## Failai / Files

- `ataskaita.typ` - Pagrindinė ataskaitos byla Typst formatu
- `grafikas.png` - Grafikas eksperimentų rezultatams (automatiškai sugeneruotas)
- `generate_graph.py` - Python skriptas grafiko generavimui (jei reikia atnaujinti)

## Kaip kompiliuoti / How to Compile

### 1. Naudojant Typst CLI (Rekomenduojama)

**Instaliacijos:**

Arch Linux:
```bash
sudo pacman -S typst
```

Kitos sistemos: https://github.com/typst/typst#installation

**Kompiliavimas:**

```bash
# Sukurti PDF
typst compile ataskaita.typ

# Arba su watch režimu (automatinis perkompiliavimas)
typst watch ataskaita.typ
```

Rezultatas: `ataskaita.pdf`

### 2. Naudojant Typst web editorių

1. Eikite į: https://typst.app
2. Sukurkite naują projektą
3. Įkelkite `ataskaita.typ` ir `grafikas.png`
4. Sistema automatiškai sukompiliuos PDF
5. Atsisiųskite PDF

### 3. Naudojant VS Code (su Typst LSP)

1. Įdiekite Typst LSP plėtinį
2. Atidarykite `ataskaita.typ`
3. Dešiniame kampe paspauskite "Export PDF" arba naudokite Command Palette

## Jei reikia atnaujinti grafiką / Updating the Graph

```bash
# Su virtualenv (rekomenduojama)
python3 -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install matplotlib
python3 generate_graph.py

# Arba tiesiogiai (Arch Linux)
sudo pacman -S python-matplotlib
python3 generate_graph.py
```

## Literatūros sąrašas / Literature List

Ataskaitos pabaigoje yra tuščia "Literatūra" sekcija. Užpildykite ją pagal savo naudotą literatūrą.

Pavyzdys:
```typst
= Literatūra

+ D. G. Hoffman, D. A. Leonard, C. C. Lindner, et al. _Coding Theory: The Essentials._ Marcel Dekker, 1991.

+ R. Hill. _A First Course in Coding Theory._ Oxford University Press, 1986.

+ F. J. MacWilliams, N. J. A. Sloane. _The Theory of Error-Correcting Codes._ North-Holland, 1977.
```

## Eksportavimas dėstytojui / Export for Professor

1. Sukompiliuokite PDF: `typst compile ataskaita.typ`
2. Supakuokite visą projektą su Docker:
   ```bash
   # Projekto šakniniame kataloge
   zip -r KodavimoTeorija.zip . -x "*/node_modules/*" "*/dist/*" "*/.git/*" "*/bin/*" "*/obj/*"
   ```

3. Archyve bus:
   - Docker konfigūracija (docker-compose.yml, Dockerfiles)
   - Visas source code su komentarais
   - Ataskaita PDF formatu
   - Paleidimo instrukcijos (DOCKER_README.md)

## Troubleshooting

**Klaida: "package not found: grafikas.png"**
- Įsitikinkite, kad `grafikas.png` yra tame pačiame kataloge kaip `ataskaita.typ`
- Sugeneruokite grafiką: `python3 generate_graph.py`

**Klaida: "unknown font: New Computer Modern"**
- Typst automatiškai atsisiunčia fontus, bet jei nepavyksta:
  ```typst
  // Pakeiskite pirmą eilutę į:
  #set text(font: "Linux Libertine", size: 11pt, lang: "lt")
  ```

**PDF atrodo keistai formatuotas**
- Patikrinkite, ar naudojate naujausią Typst versiją: `typst --version`
- Turėtų būti ≥ 0.11.0

# Ricostruzione Virtuale 3D - Colle e Duomo di San Ciriaco (Ancona)

Progetto di ricostruzione virtuale interattiva del colle e della cattedrale di San Ciriaco ad Ancona per il corso di *Computer Graphics & Multimedia* (Università Politecnica delle Marche).  
L'ambiente combina modellazione 3D poligonale su misura in **Blender**, generazione asset assistita da IA, assemblaggio e interattività in **Unity**, oltre a una **Guida Turistica Vocale basata su Intelligenza Artificiale** con pipeline 100% in locale.

---

## 📦 Download Build Eseguibile

La build autonoma per Windows è disponibile per il download diretto:

🔗 **[Scarica la Build Eseguibile (Google Drive)](https://drive.google.com/drive/folders/1g_upw3wFaNqULvydZs0q0Gx4JO72Pzcy?usp=drive_link)**

---

## 🎮 Controlli di Gioco

| Azione | Tasto / Comando |
| :--- | :--- |
| **Movimento** | `W` `A` `S` `D` o Frecce direzionali |
| **Rotazione visuale** | Click sinistro (tieni premuto) + movimento mouse |
| **Interazione (Hotspot / Binocolo / Porte)** | `E` (in prossimità del trigger) |
| **Zoom ottico binocolo** | Rotellina del mouse |
| **Chiusura schermate / Uscita binocolo** | `ESC` o click su pulsante `X` |

---

## ✨ Funzionalità Principali

- **Modellazione 3D Custom (Blender)**: Realizzazione poligonale dedicata da zero dei due complessi architettonici più iconici del colle: il **Duomo di San Ciriaco** e la **Torre Campanaria**, curandone geometrie, sezioni, mappatura UV e shading.
- **Generazione e Popolamento della Scena (Meshy & Unity)**: 
  - Ricostruzione modulare di elementi architettonici specifici (Museo Diocesano, cortine murarie in pietra del Conero, edificio storico e fontanella) tramite fotogrammetria/AI generativa Image-to-3D (**Meshy**) rifinita in Blender.
  - Composizione scenica in Unity con vegetazione volumetrica (**Terrain Engine**), arredo urbano, veicoli e quinte sceniche esterne.
- **Binocolo Panoramico Funzionante**: Postazione di osservazione interattiva al belvedere sul porto antico, dotata di maschera visiva, brandeggio a 360° e zoom ottico progressivo.
- **Punti di Interesse Storico-Culturali (Hotspot)**: Rilevamento di prossimità volumetrico per consultare schede descrittive e fotografiche (Campanile, Leoni stilofori, Statua di Papa Giovanni Paolo II, belvedere Fincantieri e Adriatico).
- **Accesso ed Esplorazione Interni**: Meccanica di transizione e teletrasporto istantaneo per visitare il chiostro e il cortile interno del Museo Diocesano.
- **Immersione Audiovisiva**:
  - Skybox panoramico equirettangolare a 360° calibrato sull'orizzonte reale della costa di Ancona.
  - Materiali PBR con Normal e Height Map per esaltare il rilievo materico di mattoni, pietra e asfalto.
  - Audio spazializzato 3D (vento marino, brusio dei passanti, rintocchi delle campane sincronizzati all'oscillazione fisica, passi del visitatore reattivi al movimento).
  - Deambulazione realistica della telecamera in prima persona gestita tramite profili d'onda procedurali (**Cinemachine**).
- **Guida Turistica IA (100% Offline e Vocale)**: Assistente interattivo a comando vocale per dialogare sulla storia del sito senza dipendenze da server cloud esterni.

---

## 🤖 Architettura della Guida Turistica IA

La conversazione vocale con la guida virtuale opera completamente in locale:
1. **Speech-to-Text (STT)**: **OpenAI Whisper (Small)** — trascrizione rapida e a bassa latenza dell'input vocale italiano.
2. **Brain & Dialogo (LLM)**: **Google Gemma 3 (4B)** — eseguito localmente tramite **Ollama** con prompt ingegnerizzato sul contesto storico del colle di San Ciriaco.
3. **Text-to-Speech (TTS)**: **Kokoro-82M (voce italiana)** — sintesi vocale naturale e fluida in tempo reale.

---

## 📁 Struttura della Repository

```text
.
├── Foto_Sfondo_Nuovo/       # Texture panoramiche a 360°, skybox e documentazione visiva
├── Prima_Prova_SanCiriaco/  # Progetto Unity (scene, script C#, controller, prefab, audio e build)
├── prove_blender/           # Modelli sorgente (.blend) del Duomo e del Campanile con prove di shading
├── .gitignore               # Esclusioni per build e file temporanei Unity/Blender
└── README.md                # Documentazione del progetto
```

---

## 🛠️ Tecnologie Utilizzate

- **Engine Grafico**: Unity (Universal Render Pipeline, Cinemachine, Terrain Engine)
- **Modellazione 3D**: Blender, Meshy (Image-to-3D)
- **Rilievo Metrico e Cartografico**: Google Earth Pro & Google Maps
- **AI Stack Vocale/Linguistico**: Ollama, Google Gemma 3 (4B), Whisper, Kokoro TTS
- **Risorse PBR**: Poly Haven, Unity Asset Store

---

## 🚀 Guida all'Apertura del Progetto in Unity

1. Clona la repository:
   ```bash
   git clone [https://github.com/domenicolaporta00/Prima-Prova.git](https://github.com/domenicolaporta00/Prima-Prova.git)
   cd Prima-Prova
   ```
2. Apri **Unity Hub**, clicca su **Add** e seleziona la cartella `Prima_Prova_SanCiriaco`.
3. Apri la scena principale in `Assets/Scenes/Piazzale_SanCiriaco.unity`.

---

## 👥 Autori

- **Domenico Laporta**
- **Stefano Marinucci**
- **Yassir Flavio Suarez Sanchez**
# Beratungszimmer

Eine VR-Anwendung für Meta Quest, entwickelt in Unity 6.3 LTS mit der Universal Render Pipeline (URP).

## Projektstruktur (Assets/)

```
Assets/
├── Brick Project Studio/   # Asset Pack – Möbel/Inneneinrichtung
├── Gogo Casual Pack/       # Asset Pack – Casual 3D-Modelle
├── ProBuilder Data/        # ProBuilder-Geometriedaten (Raum-Meshes)
├── Samples/                # XR Interaction Toolkit Beispiel-Assets
├── Scenes/                 # Unity-Szenen der Anwendung
├── Settings/               # URP-Renderer und Rendering-Settings
├── TextMesh Pro/           # TextMeshPro-Ressourcen (Fonts, Shader)
├── URPDefaultResources/    # Standard-URP-Ressourcen
├── VRTemplateAssets/       # VR-Template Assets (Controller-Modelle etc.)
├── XR/                     # XR Interaction Toolkit Konfiguration
└── XRI/                    # XR Interaction Toolkit Default Input Actions
```

> **Hinweis:** Ordner wie `Library/`, `Temp/`, `Obj/` und `Logs/` werden lokal von Unity generiert und sind nicht im Repository enthalten (siehe `.gitignore`).

## Voraussetzungen

- **Unity 6.3 LTS** (mit Android Build Support / Meta Quest)
- **Universal Render Pipeline (URP)**
- **XR Interaction Toolkit** (via Package Manager)
- **ProBuilder** (via Package Manager)

## Setup

1. Repository klonen:
   ```
   git clone https://gitlab.com/USERNAME/beratungszimmer.git
   ```
2. Projekt in Unity Hub öffnen (Unity 6.3 LTS auswählen)
3. Beim ersten Öffnen generiert Unity die `Library/`-Ordner automatisch – das kann einige Minuten dauern

## Build & Deployment

1. **File → Build Settings** → Platform: Android → Meta Quest
2. **Build And Run** oder APK exportieren
3. APK via **SideQuest** auf das Quest-Headset übertragen (Developer Mode erforderlich)

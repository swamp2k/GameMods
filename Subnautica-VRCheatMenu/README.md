# VR Cheat Menu til Subnautica (SubmersedVR)

En lille BepInEx-mod, der giver en pegbar cheat-menu i VR. Ingen tastatur nødvendigt.

## Installation
Kræver: Subnautica (Steam), BepInEx 5 og SubmersedVR (begge installeret).

1. Kopiér mappen `VRCheatMenu` til `Subnautica\BepInEx\plugins\`
   (så der ligger `...\BepInEx\plugins\VRCheatMenu\VRCheatMenu.dll`).
2. Start spillet i SteamVR som normalt.
3. Tjek `BepInEx\LogOutput.log`. Der skal stå en linje med `VR Cheat Menu indlæst`.

## Brug
- **Åbn/luk:** hold begge grips nede og klik på højre thumbstick.
- Peg på en knap med højre controller og tryk på triggeren.
- Spillet kører videre, mens menuen er åben. Højre/venstre triggers bruger ikke værktøj, mens menuen er åben.
- Oxygen, No Cost og Freecam er **til/fra-knapper** (tryk igen for at slå fra).

## Spawn-menu
Tryk på **Spawn >** nederst i hovedmenuen. Der er flere sider: **Forrige / Næste** bladrer, **Tilbage** går til hovedmenuen. Et klik på et punkt sender `spawn <navn>`, og tingen dukker op foran dig.

Listen ligger i `BepInEx\config\swamp2k.vrcheatmenu.spawn.txt` (oprettes ved første start). Én linje pr. punkt: `Label | TechType-navn`, fx `Cyclops Engine | cyclopshullmodule1`. Sider laves automatisk (8 pr. side).

## Tilføj eller fjern knapper
Første gang spillet starter, oprettes filen
`BepInEx\config\swamp2k.vrcheatmenu.buttons.txt`. Én knap pr. linje:

```
Label | konsolkommando
```

Eksempel: `Spawn Cyclops | spawn cyclops`. Gem filen og start spillet igen. Alle Subnauticas konsolkommandoer kan bruges.

## Indstillinger
`BepInEx\config\swamp2k.vrcheatmenu.cfg`:
- `Layer` – prøv `UI`, hvis panelet ikke kan ses.
- `Distance` – afstand foran hovedet i meter.
- `Scale` – størrelse.

## Bemærk
Kommandoer sendt fra menuen tæller som brug af konsollen (`hasUsedConsole`), ligesom en rigtig konsol.

## Bygge selv
`dotnet build -c Release` i mappen `VRCheatMenu` (sæt `-p:SubnauticaDir=...` hvis spillet ligger et andet sted).

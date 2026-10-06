using System.Collections.Generic;
using System.IO;

namespace VRCheatMenu
{
    internal class MenuEntry
    {
        public string Label;
        public string Command;
        public System.Action OnClick; // overstyrer Command (bruges af interne knapper)
    }

    // Knapperne ligger i en tekstfil: "Label | kommando", én pr. linje. Linjer med # er kommentarer.
    internal static class MenuConfig
    {
        public const string SpawnDefaults =
@"# VR Cheat Menu - spawn-menu:  Label | TechType-navn
# Hver linje bliver til knappen 'spawn <navn>'. Gem og start spillet igen.
Seamoth | seamoth
Cyclops | cyclops
Prawn Suit | exosuit
Mobile Vehicle Bay | constructor
Scanner | scanner
Habitat Builder | builder
Seaglide | seaglide
Repair Tool | welder
Survival Knife | knife
Heat Blade | heatblade
Flashlight | flashlight
Beacon | beacon
Fins | fins
Rebreather | rebreather
High Capacity Tank | highcapacitytank
Battery | battery
Power Cell | powercell
Titanium | titanium
Titanium Ingot | titaniumingot
Copper | copper
Quartz | quartz
Lead | lead
Silver | silver
Gold | gold
Diamond | diamond
Lithium | lithium
Magnetite | magnetite
Salt | salt
Sulphur | sulphur
Uranium | uranium
Kyanite | kyanite
Nickel | nickel
";

        public const string Defaults =
@"# VR Cheat Menu - én knap pr. linje:  Label | konsolkommando
# Gem filen og start spillet igen for at se ændringerne.
Oxygen | oxygen
No Cost | nocost
Day | day
Night | night
Spawn Seamoth | spawn seamoth
Freecam | freecam
";

        public static List<MenuEntry> Load(string path, string defaults = Defaults)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, defaults);
                Plugin.Log.LogInfo($"Oprettede standard-knapper: {path}");
            }

            var list = new List<MenuEntry>();
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                int bar = line.IndexOf('|');
                if (bar < 1)
                {
                    Plugin.Log.LogWarning($"Ignorerer linje uden '|': {line}");
                    continue;
                }
                var label = line.Substring(0, bar).Trim();
                var cmd = line.Substring(bar + 1).Trim();
                if (label.Length == 0 || cmd.Length == 0) continue;
                list.Add(new MenuEntry { Label = label, Command = cmd });
            }
            return list;
        }
    }
}

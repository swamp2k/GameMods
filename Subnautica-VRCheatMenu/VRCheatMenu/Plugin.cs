extern alias SteamVRActions;
extern alias SteamVRRef;
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SteamVRRef.Valve.VR;
using SteamVRActions.Valve.VR;
using SubmersedVR;
using UnityEngine;
using UnityEngine.UI;

namespace VRCheatMenu
{
    [BepInPlugin(GUID, "VR Cheat Menu", "1.0.0")]
    [BepInDependency("SubmersedVR")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "swamp2k.vrcheatmenu";

        internal static ManualLogSource Log;
        internal static bool MenuOpen;

        ConfigEntry<string> panelLayer;
        ConfigEntry<float> panelDistance;
        ConfigEntry<float> panelScale;

        const int SpawnPerPage = 8;
        List<MenuEntry> entries;
        List<MenuEntry> spawnEntries;
        bool spawnPage;
        int spawnIndex;
        bool rebuild;
        GameObject panel;
        bool gripsWereHeld;

        void Awake()
        {
            Log = Logger;
            panelLayer = Config.Bind("Panel", "Layer", "Default", "Unity-lag panelet tegnes på. Prøv \"UI\", hvis panelet ikke kan ses.");
            panelDistance = Config.Bind("Panel", "Distance", 1.1f, "Afstand foran hovedet (meter).");
            panelScale = Config.Bind("Panel", "Scale", 0.0009f, "Størrelse (meter pr. pixel).");

            var harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(Plugin).Assembly);

            var quickSlots = AccessTools.TypeByName("SubmersedVR.VRQuickSlots");
            var activate = quickSlots == null ? null : AccessTools.Method(quickSlots, "Activate");
            if (activate != null)
            {
                harmony.Patch(activate, prefix: new HarmonyMethod(typeof(Plugin), nameof(BlockQuickSlotWheel)));
                Log.LogInfo("Quickslot-hjulet blokeres, mens begge grips holdes.");
            }
            else
            {
                Log.LogWarning("Fandt ikke SubmersedVR.VRQuickSlots.Activate - quickslot-hjulet kan åbne sammen med menuen.");
            }

            entries = MenuConfig.Load(Path.Combine(Paths.ConfigPath, GUID + ".buttons.txt"));
            spawnEntries = MenuConfig.Load(Path.Combine(Paths.ConfigPath, GUID + ".spawn.txt"), MenuConfig.SpawnDefaults);
            foreach (var e in spawnEntries) e.Command = "spawn " + e.Command;
            Log.LogInfo($"{spawnEntries.Count} spawn-punkter indlæst.");
            Log.LogInfo($"VR Cheat Menu indlæst med {entries.Count} knapper. Trigger: begge grips + klik på højre thumbstick.");
        }

        // ---- Trigger -------------------------------------------------------------------------

        static bool GripsHeld()
        {
            try
            {
                var a = SteamVR_Actions.subnautica.Sprint; // begge grips er bundet til Sprint
                return a.GetState(SteamVR_Input_Sources.LeftHand) && a.GetState(SteamVR_Input_Sources.RightHand);
            }
            catch (Exception) { return false; }
        }

        static bool BlockQuickSlotWheel() => !GripsHeld();

        void Update()
        {
            bool grips = GripsHeld();
            if (grips != gripsWereHeld)
            {
                gripsWereHeld = grips;
                Log.LogDebug($"Begge grips holdes: {grips}");
            }

            bool stick = false;
            try { stick = SteamVR_Actions.subnautica_OpenQuickSlotWheel.GetStateDown(SteamVR_Input_Sources.RightHand); }
            catch (Exception) { }

            if (stick)
            {
                Log.LogInfo($"Højre thumbstick-klik registreret (grips={grips}).");
                if (grips)
                {
                    Log.LogInfo("Trigger registreret.");
                    Toggle();
                }
            }

            if (MenuOpen && rebuild)
            {
                rebuild = false;
                Vector3 pos = panel.transform.position;
                Quaternion rot = panel.transform.rotation;
                Destroy(panel);
                panel = BuildPanel();
                panel.transform.SetPositionAndRotation(pos, rot);
            }

            if (MenuOpen) UpdateLaser();
        }

        // ---- Menu ----------------------------------------------------------------------------

        void Toggle()
        {
            if (MenuOpen) Close(); else Open();
        }

        void Open()
        {
            var head = RigAccess.Head;
            if (head == null)
            {
                Log.LogWarning("Kan ikke åbne menuen: SubmersedVR's VR-rig findes ikke endnu.");
                return;
            }

            Vector3 fwd = head.forward; fwd.y = 0f;
            fwd = fwd.sqrMagnitude < 0.01f ? Vector3.forward : fwd.normalized;
            Vector3 pos = head.position + fwd * panelDistance.Value;
            pos.y = head.position.y - 0.1f;

            spawnPage = false;
            spawnIndex = 0;
            rebuild = false;
            panel = BuildPanel();
            panel.transform.position = pos;
            panel.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            MenuOpen = true;
            Log.LogInfo($"Menu åbnet ({entries.Count} knapper) ved {pos}.");
        }

        void Close()
        {
            if (panel != null) Destroy(panel);
            panel = null;
            MenuOpen = false;
            SetLaser(false, 0f);
            Log.LogInfo("Menu lukket.");
        }

        void GoTo(bool spawn, int index)
        {
            spawnPage = spawn;
            spawnIndex = index;
            rebuild = true; // bygges om i næste Update, så knappen ikke slettes midt i sit eget klik
        }

        GameObject BuildPanel()
        {
            const float btnW = 440f, btnH = 90f, gap = 16f, titleH = 70f;
            const int cols = 2;

            List<MenuEntry> items;
            string titleText;
            bool nav = false;
            int pages = 1;
            if (spawnPage)
            {
                pages = Math.Max(1, (spawnEntries.Count + SpawnPerPage - 1) / SpawnPerPage);
                spawnIndex = Math.Max(0, Math.Min(spawnIndex, pages - 1));
                items = spawnEntries.GetRange(spawnIndex * SpawnPerPage, Math.Min(SpawnPerPage, spawnEntries.Count - spawnIndex * SpawnPerPage));
                titleText = $"Spawn ({spawnIndex + 1}/{pages})";
                nav = true;
            }
            else
            {
                items = new List<MenuEntry>(entries);
                items.Add(new MenuEntry { Label = "Spawn  >", OnClick = () => GoTo(true, 0) });
                titleText = "VR Cheat Menu";
            }

            int rows = Math.Max(1, (items.Count + cols - 1) / cols);
            float w = cols * btnW + (cols + 1) * gap;
            float h = titleH + rows * (btnH + gap) + gap + (nav ? btnH + gap : 0f);

            int layer = LayerMask.NameToLayer(panelLayer.Value);
            if (layer < 0) layer = 0;

            var root = new GameObject("VRCheatMenuPanel", typeof(RectTransform)) { layer = layer };
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            rt.localScale = Vector3.one * panelScale.Value;
            var raycaster = root.AddComponent<uGUI_GraphicRaycaster>();
            raycaster.guiCameraSpace = false; // world-space: SubmersedVR bruger højre controller-kamera

            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var bg = NewChild(root, "Background", layer);
            Stretch(bg);
            bg.AddComponent<Image>().color = new Color(0.04f, 0.09f, 0.14f, 0.92f);

            var title = NewChild(root, "Title", layer);
            Place(title, 0f, 0f, w, titleH, w, h);
            AddText(title, font, titleText, 40, Color.white);

            for (int i = 0; i < items.Count; i++)
            {
                var e = items[i];
                int row = i / cols, col = i % cols;
                float x = gap + col * (btnW + gap);
                float y = titleH + gap + row * (btnH + gap);
                string label = e.Label, cmd = e.Command;
                var act = e.OnClick;
                AddButton(root, font, layer, label, x, y, btnW, btnH, w, h, act ?? (() => Run(label, cmd)));
            }

            if (nav)
            {
                float y = titleH + gap + rows * (btnH + gap);
                float nw = (w - 4 * gap) / 3f;
                int prev = (spawnIndex + pages - 1) % pages, next = (spawnIndex + 1) % pages;
                AddButton(root, font, layer, "<  Forrige", gap, y, nw, btnH, w, h, () => GoTo(true, prev));
                AddButton(root, font, layer, "Tilbage", 2 * gap + nw, y, nw, btnH, w, h, () => GoTo(false, 0));
                AddButton(root, font, layer, "Næste  >", 3 * gap + 2 * nw, y, nw, btnH, w, h, () => GoTo(true, next));
            }
            return root;
        }

        static void AddButton(GameObject root, Font font, int layer, string label, float x, float y, float bw, float bh, float w, float h, Action onClick)
        {
            var go = NewChild(root, "Btn " + label, layer);
            Place(go, x, y, bw, bh, w, h);
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = new Color(0.12f, 0.35f, 0.5f, 1f);
            colors.highlightedColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            colors.pressedColor = new Color(0.9f, 0.7f, 0.2f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick());

            var txt = NewChild(go, "Label", layer);
            Stretch(txt);
            AddText(txt, font, label, 34, Color.white);
        }

        static GameObject NewChild(GameObject parent, string name, int layer)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = layer };
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        static void Stretch(GameObject go)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        // x,y er målt fra panelets øverste venstre hjørne.
        static void Place(GameObject go, float x, float y, float w, float h, float panelW, float panelH)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(w, h);
            r.anchoredPosition = new Vector2(x + w / 2f - panelW / 2f, panelH / 2f - (y + h / 2f));
        }

        static void AddText(GameObject go, Font font, string text, int size, Color color)
        {
            var t = go.AddComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
        }

        // ---- Kommando ------------------------------------------------------------------------

        static void Run(string label, string command)
        {
            try
            {
                bool ok = DevConsole.SendConsoleCommand(command);
                Log.LogInfo($"Kommando '{command}' ({label}) sendt, resultat={ok}.");
                if (!ok) ErrorMessage.AddDebug($"VR Cheat Menu: ukendt kommando '{command}'");
            }
            catch (Exception e)
            {
                Log.LogError($"Kommando '{command}' fejlede: {e}");
            }
        }

        // ---- Laser ---------------------------------------------------------------------------

        void UpdateLaser()
        {
            float len = 4f;
            var module = FPSInputModule.current;
            if (module != null)
            {
                var t = Traverse.Create(module);
                float lastHit = t.Field("lastValidRaycastTime").GetValue<float>();
                float dist = t.Field("lastRaycastResult").GetValue<UnityEngine.EventSystems.RaycastResult>().distance;
                if (Time.unscaledTime - lastHit < 0.2f && dist > 0f) len = dist;
            }
            SetLaser(true, len);
        }

        static void SetLaser(bool on, float length)
        {
            var lp = RigAccess.WorldLaser;
            if (lp == null || lp.lineRenderer == null || lp.pointerDot == null) return;
            lp.lineRenderer.enabled = on;
            lp.pointerDot.SetActive(on);
            if (on) lp.SetEnd(lp.transform.position + lp.transform.forward * length);
        }

        internal static bool IsToolButton(GameInput.Button b)
        {
            return b == GameInput.Button.RightHand || b == GameInput.Button.LeftHand
                || b == GameInput.Button.AltTool || b == GameInput.Button.Reload
                || b == GameInput.Button.Deconstruct;
        }
    }

    // SubmersedVR's VRCameraRig er internal, så vi når den med reflection.
    static class RigAccess
    {
        static readonly Type RigType = AccessTools.TypeByName("SubmersedVR.VRCameraRig");

        static UnityEngine.Object Instance
        {
            get
            {
                var o = RigType == null ? null : AccessTools.Field(RigType, "instance")?.GetValue(null) as UnityEngine.Object;
                return o != null ? o : null;
            }
        }

        static T Field<T>(string name) where T : class
        {
            var inst = Instance;
            return inst == null ? null : AccessTools.Field(RigType, name)?.GetValue(inst) as T;
        }

        public static Transform Head => Field<Camera>("vrCamera")?.transform;
        public static LaserPointer WorldLaser => Field<LaserPointer>("laserPointer");
    }

    // Mens menuen er åben, må kontrollernes værktøjs-knapper ikke også bruge/skyde i spillet.
    // (UISubmit - klik på menuen - røres ikke.)
    [HarmonyPatch(typeof(GameInput), nameof(GameInput.GetButtonDown))]
    static class BlockToolDown
    {
        static void Postfix(GameInput.Button __0, ref bool __result)
        {
            if (Plugin.MenuOpen && Plugin.IsToolButton(__0)) __result = false;
        }
    }

    [HarmonyPatch(typeof(GameInput), nameof(GameInput.GetButtonHeld))]
    static class BlockToolHeld
    {
        static void Postfix(GameInput.Button __0, ref bool __result)
        {
            if (Plugin.MenuOpen && Plugin.IsToolButton(__0)) __result = false;
        }
    }

    [HarmonyPatch(typeof(GameInput), nameof(GameInput.GetButtonUp))]
    static class BlockToolUp
    {
        static void Postfix(GameInput.Button __0, ref bool __result)
        {
            if (Plugin.MenuOpen && Plugin.IsToolButton(__0)) __result = false;
        }
    }
}

using GTAG_NotificationLib;
using MalachiTemp.Backend;
using MalachiTemp.Classes;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MalachiTemp.UI
{
    public class ModGUI : MonoBehaviour
    {
        public static bool showGUI = true;
        private Rect windowRect = new Rect(50f, 50f, 460f, 560f);
        private string currentCategory;
        private float scale = 1f;
        private Vector2 modScrollPos;
        private Vector2 catScrollPos;
        private static GUISkin cachedSkin;
        private static readonly Dictionary<string, Texture2D> cachedTex = new Dictionary<string, Texture2D>();
        private int lastAdminCheckFrame = -1;
        private bool lastAdminResult;

        private static readonly Color colBg = new Color(0.12f, 0.12f, 0.12f, 0.97f);
        private static readonly Color colPanel = new Color(0.17f, 0.17f, 0.17f, 0.95f);
        private static readonly Color colBorder = new Color(0.28f, 0.28f, 0.28f, 1f);
        private static readonly Color colAccent = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color colHover = new Color(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color colActive = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color colSelected = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color colToggleOn = new Color(0.35f, 0.65f, 0.35f, 1f);
        private static readonly Color colToggleOff = new Color(0.5f, 0.18f, 0.18f, 1f);

        private void Start()
        {
            currentCategory = GetFirstCategory();
        }

        private void Update()
        {
            if (Keyboard.current[Key.F4].wasPressedThisFrame)
            {
                showGUI = !showGUI;
                if (showGUI && windowRect.x < -100f)
                    windowRect = new Rect(50f, 50f, 460f, 560f);
            }
        }

        private bool IsAdmin()
        {
            int frame = Time.frameCount;
            if (frame == lastAdminCheckFrame) return lastAdminResult;
            lastAdminCheckFrame = frame;
            lastAdminResult = PhotonNetwork.LocalPlayer != null &&
                              ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
            return lastAdminResult;
        }

        private string GetFirstCategory()
        {
            var cats = GetVisibleCategories();
            return cats.Count > 0 ? cats[0] : "Settings";
        }

        private List<string> GetVisibleCategories()
        {
            bool isAdmin = IsAdmin();
            return MenuManager.Categories
                .Select(c => c.Name)
                .Where(n => n != "Main")
                .Where(n => n != "Tripple T")
                .Where(n => isAdmin || (n != "Admin Mods" && n != "Console Settings"))
                .ToList();
        }

        private void OnGUI()
        {
            if (!showGUI) return;

            if (cachedSkin == null) GUI.skin = GetSkin();
            else GUI.skin = cachedSkin;

            Rect scaled = new Rect(windowRect.x / scale, windowRect.y / scale,
                                   windowRect.width / scale, windowRect.height / scale);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            scaled = GUI.Window(GetHashCode(), scaled, DoWindow, "");
            GUI.matrix = oldMatrix;
            windowRect = new Rect(scaled.x * scale, scaled.y * scale,
                                  scaled.width * scale, scaled.height * scale);
        }

        private GUISkin GetSkin()
        {
            if (cachedSkin != null) return cachedSkin;

            var skin = ScriptableObject.CreateInstance<GUISkin>();

            skin.window = new GUIStyle(GUI.skin.window)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white, background = MakeTex("bg", 1, 1, colBg) },
                border = new RectOffset(6, 6, 6, 6),
                padding = new RectOffset(1, 1, 28, 1)
            };

            skin.button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white, background = MakeTex("btn", 1, 1, new Color(0.25f, 0.25f, 0.25f, 1f)) },
                hover = { textColor = Color.white, background = MakeTex("hover", 1, 1, colHover) },
                active = { textColor = Color.white, background = MakeTex("active", 1, 1, colActive) },
                border = new RectOffset(3, 3, 3, 3),
                margin = new RectOffset(2, 2, 1, 1),
                padding = new RectOffset(8, 4, 4, 4)
            };

            skin.box = new GUIStyle(GUI.skin.box)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colAccent, background = MakeTex("panel", 1, 1, colPanel) },
                border = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(2, 2, 1, 1),
                padding = new RectOffset(8, 4, 3, 3)
            };

            skin.label = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f, 1f) },
                padding = new RectOffset(2, 2, 2, 2)
            };

            skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                normal = { background = MakeTex("scrollbg", 1, 1, new Color(0.08f, 0.08f, 0.08f, 0.5f)) },
                border = new RectOffset(1, 1, 1, 1),
                fixedWidth = 6
            };
            skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb)
            {
                normal = { background = MakeTex("scrollthumb", 1, 1, new Color(0.5f, 0.5f, 0.5f, 0.6f)) },
                border = new RectOffset(1, 1, 1, 1),
                fixedWidth = 6
            };
            skin.verticalScrollbarUpButton = new GUIStyle { fixedHeight = 0 };
            skin.verticalScrollbarDownButton = new GUIStyle { fixedHeight = 0 };

            skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider)
            {
                normal = { background = MakeTex("sliderbg", 1, 1, new Color(0.2f, 0.2f, 0.2f, 0.8f)) },
                fixedHeight = 6
            };
            skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                normal = { background = MakeTex("sliderthumb", 1, 1, colAccent) },
                fixedWidth = 12,
                fixedHeight = 12
            };

            return cachedSkin = skin;
        }

        private static Texture2D MakeTex(string key, int w, int h, Color c)
        {
            if (cachedTex.TryGetValue(key, out var existing) && existing != null)
                return existing;

            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = c;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(pix);
            tex.Apply();
            cachedTex[key] = tex;
            return tex;
        }

        private void DrawOutlineRect(Rect r, Color c)
        {
            DrawLine(new Rect(r.x, r.y, r.width, 1), c);
            DrawLine(new Rect(r.x, r.yMax - 1, r.width, 1), c);
            DrawLine(new Rect(r.x, r.y, 1, r.height), c);
            DrawLine(new Rect(r.xMax - 1, r.y, 1, r.height), c);
        }

        private void DrawLine(Rect r, Color c)
        {
            Color orig = GUI.color;
            GUI.color = c;
            Texture2D t = MakeTex("_white", 1, 1, Color.white);
            GUI.DrawTexture(r, t);
            GUI.color = orig;
        }

        private void DoWindow(int windowID)
        {
            float s = scale;
            float w = windowRect.width / s;

            float headerPad = 8;
            float titleH = 24;
            float margin = 6;
            float catW = 130f;

            float catX = margin;
            float catY = headerPad + titleH + 4;
            float catH = windowRect.height / s - catY - margin;

            float modX = catX + catW + margin;
            float modW = w - modX - margin;
            float modY = catY;
            float modH = catH;

            GUI.Label(new Rect(headerPad, 2, w * 0.6f, titleH), "CHUD  MENU");
            GUI.Label(new Rect(headerPad, 20, w * 0.6f, 14), "v1.0  gorilla tag", new GUIStyle(GUI.skin.label) { fontSize = 9, normal = { textColor = colAccent } });

            if (GUI.Button(new Rect(w - 24, 4, 20, 20), "X", new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.8f, 0.3f, 0.3f, 1f) },
                hover = { textColor = Color.white, background = MakeTex("close_hover", 1, 1, new Color(0.5f, 0.15f, 0.15f, 1f)) }
            }))
                showGUI = false;

            float dividerY = catY - 4;
            DrawLine(new Rect(4, dividerY, w - 8, 1), colBorder);
            DrawLine(new Rect(4, dividerY + 1, w - 8, 1), new Color(0.1f, 0.1f, 0.1f, 1f));

            var cats = GetVisibleCategories();
            Texture2D panelTex = MakeTex("_panel", 1, 1, colPanel);
            GUI.DrawTexture(new Rect(catX, catY, catW, catH), panelTex);
            DrawOutlineRect(new Rect(catX, catY, catW, catH), colBorder);

            GUI.Label(new Rect(catX + 4, catY + 2, catW - 8, 16), "CATEGORIES", new GUIStyle(GUI.skin.label)
            { fontSize = 9, fontStyle = FontStyle.Bold, normal = { textColor = colAccent } });

            float catListY = catY + 20;
            float catListH = catH - 22;
            catScrollPos = GUI.BeginScrollView(
                new Rect(catX + 1, catListY, catW - 2, catListH),
                catScrollPos,
                new Rect(0, 0, catW - 14, cats.Count * 28f));

            float yOff = 2;
            foreach (var cat in cats)
            {
                bool selected = cat == currentCategory;
                Rect btnR = new Rect(3, yOff, catW - 20, 25);

                if (selected)
                {
                    GUI.DrawTexture(btnR, MakeTex("_sel", 1, 1, colSelected));
                    DrawLine(new Rect(btnR.x, btnR.y, 3, btnR.height), new Color(0.55f, 0.55f, 0.55f, 1f));
                    GUI.Label(new Rect(btnR.x + 8, btnR.y, btnR.width - 10, btnR.height), cat, new GUIStyle(GUI.skin.label)
                    { fontSize = 12, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
                }
                else
                {
                    if (GUI.Button(btnR, ""))
                        currentCategory = cat;
                    GUI.Label(new Rect(btnR.x + 8, btnR.y, btnR.width - 10, btnR.height), cat, new GUIStyle(GUI.skin.label)
                    { fontSize = 12, fontStyle = FontStyle.Normal, normal = { textColor = Color.white } });
                }

                yOff += 27;
            }
            GUI.EndScrollView();

            var buttons = GetCurrentButtons();
            GUI.DrawTexture(new Rect(modX, modY, modW, modH), panelTex);
            DrawOutlineRect(new Rect(modX, modY, modW, modH), colBorder);

            GUI.Label(new Rect(modX + 4, modY + 2, modW - 8, 16), (currentCategory ?? "").ToUpper(), new GUIStyle(GUI.skin.label)
            { fontSize = 9, fontStyle = FontStyle.Bold, normal = { textColor = colAccent } });

            float btnListY = modY + 20;
            float btnListH = modH - 22;
            modScrollPos = GUI.BeginScrollView(
                new Rect(modX + 1, btnListY, modW - 2, btnListH),
                modScrollPos,
                new Rect(0, 0, modW - 14, buttons.Count * 30f));

            yOff = 2;
            foreach (var btn in buttons)
            {
                if (btn.buttonText.StartsWith("Exit "))
                {
                    yOff += 30;
                    continue;
                }

                bool isEnabled = btn.enabled == true;
                Rect btnR = new Rect(3, yOff, modW - 14, 26);

                DrawOutlineRect(btnR, new Color(0.23f, 0.23f, 0.23f, 1f));

                Color toggleColor = isEnabled ? colToggleOn : colToggleOff;
                DrawLine(new Rect(btnR.x + 1, btnR.y + 1, 4, btnR.height - 2), toggleColor);

                Color origBg = GUI.backgroundColor;
                GUI.backgroundColor = isEnabled ? new Color(0.25f, 0.45f, 0.25f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.3f);

                if (GUI.Button(btnR, ""))
                    HandleToggle(btn);

                GUI.backgroundColor = origBg;

                GUI.Label(new Rect(btnR.x + 10, btnR.y + 1, btnR.width - 16, btnR.height),
                    btn.buttonText, new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 12,
                        fontStyle = isEnabled ? FontStyle.Bold : FontStyle.Normal,
                        normal = { textColor = Color.white }
                    });

                yOff += 30;
            }
            GUI.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, w, catY + 4));
        }

        private List<ButtonInfo> GetCurrentButtons()
        {
            var cat = MenuManager.Categories.Find(c => c.Name == currentCategory);
            return cat?.Buttons ?? new List<ButtonInfo>();
        }

        private void HandleToggle(ButtonInfo btn)
        {
            if (btn.nontoggleable == true)
            {
                try { btn.method?.Invoke(); } catch { }
                return;
            }

            if (currentCategory == "Master Mods" && !PhotonNetwork.IsMasterClient)
            {
                NotifiLib.SendNotification("[<color=red>MASTER</color>] You are not master client!");
                return;
            }

            bool wasEnabled = btn.enabled == true;
            btn.enabled = !wasEnabled;

            if (btn.enabled == true)
            {
                try { btn.method?.Invoke(); } catch { }
            }
            else if (btn.disableMethod != null)
            {
                try { btn.disableMethod.Invoke(); } catch { }
            }

            Mods.AutoSave();
        }
    }
}

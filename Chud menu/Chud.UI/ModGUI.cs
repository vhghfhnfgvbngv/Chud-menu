using System.Collections.Generic;
using System.Linq;
using Chud.Backend;
using Chud.Classes;
using GTAG_NotificationLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Chud.UI;

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
		if (((ButtonControl)Keyboard.current[(Key)97]).wasPressedThisFrame)
		{
			showGUI = !showGUI;
			if (showGUI && windowRect.x < -100f)
			{
				windowRect = new Rect(50f, 50f, 460f, 560f);
			}
		}
	}

	private bool IsAdmin()
	{
		int frameCount = Time.frameCount;
		if (frameCount == lastAdminCheckFrame)
		{
			return lastAdminResult;
		}
		lastAdminCheckFrame = frameCount;
		lastAdminResult = PhotonNetwork.LocalPlayer != null && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId);
		return lastAdminResult;
	}

	private string GetFirstCategory()
	{
		List<string> visibleCategories = GetVisibleCategories();
		return (visibleCategories.Count > 0) ? visibleCategories[0] : "Settings";
	}

	private List<string> GetVisibleCategories()
	{
		bool isAdmin = IsAdmin();
		return (from c in MenuManager.Categories
			select c.Name into n
			where n != "Main"
			where n != "Tripple T"
			where isAdmin || (n != "Admin Mods" && n != "Console Settings")
			select n).ToList();
	}

	private void OnGUI()
	{
		if (showGUI)
		{
			if ((Object)(object)cachedSkin == (Object)null)
			{
				GUI.skin = GetSkin();
			}
			else
			{
				GUI.skin = cachedSkin;
			}
			Rect val = default(Rect);
			val = new Rect(windowRect.x / scale, windowRect.y / scale, windowRect.width / scale, windowRect.height / scale);
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
			val = GUI.Window(98765, val, DoWindow, "");
			GUI.matrix = matrix;
			windowRect = new Rect(val.x * scale, val.y * scale, val.width * scale, val.height * scale);
		}
	}

	private GUISkin GetSkin()
	{
		if ((Object)(object)cachedSkin != (Object)null)
		{
			return cachedSkin;
		}
		GUISkin val = ScriptableObject.CreateInstance<GUISkin>();
		GUIStyle val2 = new GUIStyle(GUI.skin.window)
		{
			fontSize = 13,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)1
		};
		val2.normal.textColor = Color.white;
		val2.normal.background = MakeTex("bg", 1, 1, colBg);
		val2.border = new RectOffset(6, 6, 6, 6);
		val2.padding = new RectOffset(1, 1, 28, 1);
		val.window = val2;
		GUIStyle val3 = new GUIStyle(GUI.skin.button)
		{
			fontSize = 12,
			fontStyle = (FontStyle)0,
			alignment = (TextAnchor)3
		};
		val3.normal.textColor = Color.white;
		val3.normal.background = MakeTex("btn", 1, 1, new Color(0.25f, 0.25f, 0.25f, 1f));
		val3.hover.textColor = Color.white;
		val3.hover.background = MakeTex("hover", 1, 1, colHover);
		val3.active.textColor = Color.white;
		val3.active.background = MakeTex("active", 1, 1, colActive);
		val3.border = new RectOffset(3, 3, 3, 3);
		val3.margin = new RectOffset(2, 2, 1, 1);
		val3.padding = new RectOffset(8, 4, 4, 4);
		val.button = val3;
		GUIStyle val4 = new GUIStyle(GUI.skin.box)
		{
			fontSize = 11,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)3
		};
		val4.normal.textColor = colAccent;
		val4.normal.background = MakeTex("panel", 1, 1, colPanel);
		val4.border = new RectOffset(4, 4, 4, 4);
		val4.margin = new RectOffset(2, 2, 1, 1);
		val4.padding = new RectOffset(8, 4, 3, 3);
		val.box = val4;
		GUIStyle val5 = new GUIStyle(GUI.skin.label)
		{
			fontSize = 12,
			fontStyle = (FontStyle)0,
			alignment = (TextAnchor)3
		};
		val5.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
		val5.padding = new RectOffset(2, 2, 2, 2);
		val.label = val5;
		GUIStyle val6 = new GUIStyle(GUI.skin.verticalScrollbar);
		val6.normal.background = MakeTex("scrollbg", 1, 1, new Color(0.08f, 0.08f, 0.08f, 0.5f));
		val6.border = new RectOffset(1, 1, 1, 1);
		val6.fixedWidth = 6f;
		val.verticalScrollbar = val6;
		GUIStyle val7 = new GUIStyle(GUI.skin.verticalScrollbarThumb);
		val7.normal.background = MakeTex("scrollthumb", 1, 1, new Color(0.5f, 0.5f, 0.5f, 0.6f));
		val7.border = new RectOffset(1, 1, 1, 1);
		val7.fixedWidth = 6f;
		val.verticalScrollbarThumb = val7;
		val.verticalScrollbarUpButton = new GUIStyle
		{
			fixedHeight = 0f
		};
		val.verticalScrollbarDownButton = new GUIStyle
		{
			fixedHeight = 0f
		};
		GUIStyle val8 = new GUIStyle(GUI.skin.horizontalSlider);
		val8.normal.background = MakeTex("sliderbg", 1, 1, new Color(0.2f, 0.2f, 0.2f, 0.8f));
		val8.fixedHeight = 6f;
		val.horizontalSlider = val8;
		GUIStyle val9 = new GUIStyle(GUI.skin.horizontalSliderThumb);
		val9.normal.background = MakeTex("sliderthumb", 1, 1, colAccent);
		val9.fixedWidth = 12f;
		val9.fixedHeight = 12f;
		val.horizontalSliderThumb = val9;
		return cachedSkin = val;
	}

	private static Texture2D MakeTex(string key, int w, int h, Color c)
	{
		if (cachedTex.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Color[] array = (Color[])(object)new Color[w * h];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = c;
		}
		Texture2D val = new Texture2D(w, h, (TextureFormat)4, false);
		val.SetPixels(array);
		val.Apply();
		cachedTex[key] = val;
		return val;
	}

	private void DrawOutlineRect(Rect r, Color c)
	{
		DrawLine(new Rect(r.x, r.y, r.width, 1f), c);
		DrawLine(new Rect(r.x, r.yMax - 1f, r.width, 1f), c);
		DrawLine(new Rect(r.x, r.y, 1f, r.height), c);
		DrawLine(new Rect(r.xMax - 1f, r.y, 1f, r.height), c);
	}

	private void DrawLine(Rect r, Color c)
	{
		Color color = GUI.color;
		GUI.color = c;
		Texture2D val = MakeTex("_white", 1, 1, Color.white);
		GUI.DrawTexture(r, (Texture)(object)val);
		GUI.color = color;
	}

	private void DoWindow(int windowID)
	{
		float num = scale;
		float num2 = windowRect.width / num;
		float num3 = 8f;
		float num4 = 24f;
		float num5 = 6f;
		float num6 = 130f;
		float num7 = num5;
		float num8 = num3 + num4 + 4f;
		float num9 = windowRect.height / num - num8 - num5;
		float num10 = num7 + num6 + num5;
		float num11 = num2 - num10 - num5;
		float num12 = num8;
		float num13 = num9;
		GUI.Label(new Rect(num3, 2f, num2 * 0.6f, num4), "CHUD  MENU");
		Rect val = new Rect(num3, 20f, num2 * 0.6f, 14f);
		GUIStyle val2 = new GUIStyle(GUI.skin.label)
		{
			fontSize = 9
		};
		val2.normal.textColor = colAccent;
		GUI.Label(val, "v1.0  gorilla tag", val2);
		Rect val3 = new Rect(num2 - 24f, 4f, 20f, 20f);
		GUIStyle val4 = new GUIStyle(GUI.skin.button)
		{
			alignment = (TextAnchor)4,
			fontSize = 12,
			fontStyle = (FontStyle)1
		};
		val4.normal.textColor = new Color(0.8f, 0.3f, 0.3f, 1f);
		val4.hover.textColor = Color.white;
		val4.hover.background = MakeTex("close_hover", 1, 1, new Color(0.5f, 0.15f, 0.15f, 1f));
		if (GUI.Button(val3, "X", val4))
		{
			showGUI = false;
		}
		float num14 = num8 - 4f;
		DrawLine(new Rect(4f, num14, num2 - 8f, 1f), colBorder);
		DrawLine(new Rect(4f, num14 + 1f, num2 - 8f, 1f), new Color(0.1f, 0.1f, 0.1f, 1f));
		List<string> visibleCategories = GetVisibleCategories();
		Texture2D val5 = MakeTex("_panel", 1, 1, colPanel);
		GUI.DrawTexture(new Rect(num7, num8, num6, num9), (Texture)(object)val5);
		DrawOutlineRect(new Rect(num7, num8, num6, num9), colBorder);
		Rect val6 = new Rect(num7 + 4f, num8 + 2f, num6 - 8f, 16f);
		GUIStyle val7 = new GUIStyle(GUI.skin.label)
		{
			fontSize = 9,
			fontStyle = (FontStyle)1
		};
		val7.normal.textColor = colAccent;
		GUI.Label(val6, "CATEGORIES", val7);
		float num15 = num8 + 20f;
		float num16 = num9 - 22f;
		catScrollPos = GUI.BeginScrollView(new Rect(num7 + 1f, num15, num6 - 2f, num16), catScrollPos, new Rect(0f, 0f, num6 - 14f, (float)visibleCategories.Count * 28f));
		float num17 = 2f;
		Rect val8 = default(Rect);
		foreach (string item in visibleCategories)
		{
			bool flag = item == currentCategory;
			val8 = new Rect(3f, num17, num6 - 20f, 25f);
			if (flag)
			{
				GUI.DrawTexture(val8, (Texture)(object)MakeTex("_sel", 1, 1, colSelected));
				DrawLine(new Rect(val8.x, val8.y, 3f, val8.height), new Color(0.55f, 0.55f, 0.55f, 1f));
				Rect val9 = new Rect(val8.x + 8f, val8.y, val8.width - 10f, val8.height);
				GUIStyle val10 = new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					fontStyle = (FontStyle)1
				};
				val10.normal.textColor = Color.white;
				GUI.Label(val9, item, val10);
			}
			else
			{
				if (GUI.Button(val8, ""))
				{
					currentCategory = item;
				}
				Rect val11 = new Rect(val8.x + 8f, val8.y, val8.width - 10f, val8.height);
				GUIStyle val12 = new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					fontStyle = (FontStyle)0
				};
				val12.normal.textColor = Color.white;
				GUI.Label(val11, item, val12);
			}
			num17 += 27f;
		}
		GUI.EndScrollView();
		List<ButtonInfo> currentButtons = GetCurrentButtons();
		GUI.DrawTexture(new Rect(num10, num12, num11, num13), (Texture)(object)val5);
		DrawOutlineRect(new Rect(num10, num12, num11, num13), colBorder);
		Rect val13 = new Rect(num10 + 4f, num12 + 2f, num11 - 8f, 16f);
		string text = (currentCategory ?? "").ToUpper();
		GUIStyle val14 = new GUIStyle(GUI.skin.label)
		{
			fontSize = 9,
			fontStyle = (FontStyle)1
		};
		val14.normal.textColor = colAccent;
		GUI.Label(val13, text, val14);
		float num18 = num12 + 20f;
		float num19 = num13 - 22f;
		modScrollPos = GUI.BeginScrollView(new Rect(num10 + 1f, num18, num11 - 2f, num19), modScrollPos, new Rect(0f, 0f, num11 - 14f, (float)currentButtons.Count * 30f));
		num17 = 2f;
		Rect val15 = default(Rect);
		foreach (ButtonInfo item2 in currentButtons)
		{
			if (item2.buttonText.StartsWith("Exit "))
			{
				num17 += 30f;
				continue;
			}
			bool valueOrDefault = item2.enabled == true;
			val15 = new Rect(3f, num17, num11 - 14f, 26f);
			DrawOutlineRect(val15, new Color(0.23f, 0.23f, 0.23f, 1f));
			Color c = (valueOrDefault ? colToggleOn : colToggleOff);
			DrawLine(new Rect(val15.x + 1f, val15.y + 1f, 4f, val15.height - 2f), c);
			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = (valueOrDefault ? new Color(0.25f, 0.45f, 0.25f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.3f));
			if (GUI.Button(val15, ""))
			{
				HandleToggle(item2);
			}
			GUI.backgroundColor = backgroundColor;
			Rect val16 = new Rect(val15.x + 10f, val15.y + 1f, val15.width - 16f, val15.height);
			string buttonText = item2.buttonText;
			GUIStyle val17 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 12,
				fontStyle = (FontStyle)(valueOrDefault ? 1 : 0)
			};
			val17.normal.textColor = Color.white;
			GUI.Label(val16, buttonText, val17);
			num17 += 30f;
		}
		GUI.EndScrollView();
		GUI.DragWindow(new Rect(0f, 0f, num2, num8 + 4f));
	}

	private List<ButtonInfo> GetCurrentButtons()
	{
		return MenuManager.Categories.Find((MenuCategory c) => c.Name == currentCategory)?.Buttons ?? new List<ButtonInfo>();
	}

	private void HandleToggle(ButtonInfo btn)
	{
		if (btn.nontoggleable == true)
		{
			try
			{
				btn.method?.Invoke();
				return;
			}
			catch (System.Exception ex)
			{
				Debug.LogError("[ModGUI] Error invoking non-toggle method: " + ex.Message);
				return;
			}
		}
		if (currentCategory == "Master Mods" && !PhotonNetwork.IsMasterClient)
		{
			NotifiLib.SendNotification("[<color=red>MASTER</color>] You are not master client!");
			return;
		}
		bool valueOrDefault = btn.enabled == true;
		btn.enabled = !valueOrDefault;
		if (btn.enabled == true)
		{
			try
			{
				btn.method?.Invoke();
			}
			catch (System.Exception ex)
			{
				Debug.LogError("[ModGUI] Error invoking toggle method: " + ex.Message);
			}
		}
		else if (btn.disableMethod != null)
		{
			try
			{
				btn.disableMethod();
			}
			catch (System.Exception ex)
			{
				Debug.LogError("[ModGUI] Error invoking disable method: " + ex.Message);
			}
		}
		Mods.AutoSave();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chud.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace GTAG_NotificationLib;

public class NotifiLib : MonoBehaviour
{
	private GameObject hudObj;

	private GameObject hudObjParent;

	private GameObject mainCamera;

	private static Text notificationText;

	private Material textMaterial = new Material(Shader.Find("GUI/Text Shader"));

	private int decayCounter = 200;

	private bool hasInit = false;

	public static bool IsEnabled = true;

	public static int DecayTime = 150;

	private static float lastNotifyTime;

	private static Queue<int> lineDecayTimes = new Queue<int>();

	private static readonly List<DesktopNoti> _desktopNotis = new List<DesktopNoti>();

	private const int MAX_DESKTOP_NOTIS = 4;

	private static GUIStyle _notiStyle;

	private static Texture2D _notiBgTex;

	private struct DesktopNoti
	{
		public string text;
		public float expireTime;
	}

	private static bool IsDesktopMode()
	{
		return !XRSettings.isDeviceActive;
	}

	private static Texture2D GetNotiBgTex()
	{
		if ((Object)(object)_notiBgTex == (Object)null)
		{
			_notiBgTex = new Texture2D(1, 1);
			_notiBgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
			_notiBgTex.Apply();
		}
		return _notiBgTex;
	}

	private void OnGUI()
	{
		if (!IsDesktopMode() || _desktopNotis.Count == 0)
		{
			return;
		}
		if (_notiStyle == null)
		{
			_notiStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 20,
				richText = true,
				wordWrap = true,
				alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(8, 8, 0, 0)
			};
			if ((Object)(object)WristMenu.MenuFont != (Object)null)
			{
				_notiStyle.font = WristMenu.MenuFont;
			}
		}
		float y = 10f;
		float height = 36f;
		for (int i = 0; i < _desktopNotis.Count; i++)
		{
			DesktopNoti noti = _desktopNotis[i];
			float alpha = Mathf.Clamp01((noti.expireTime - Time.time) / 0.5f);
			if (alpha <= 0f) continue;
			string stripped = Regex.Replace(noti.text, "<[^>]*>", "");
			float width = _notiStyle.CalcSize(new GUIContent(stripped)).x + 24f;
			Color prevColor = GUI.color;
			GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, alpha);
			GUI.DrawTexture(new Rect(10f, y, width, height), GetNotiBgTex());
			GUI.Label(new Rect(10f, y, width, height), noti.text, _notiStyle);
			GUI.color = prevColor;
			y += height + 6f;
		}
	}

	private static void CleanDesktopNotis()
	{
		for (int i = _desktopNotis.Count - 1; i >= 0; i--)
		{
			if (Time.time >= _desktopNotis[i].expireTime)
			{
				_desktopNotis.RemoveAt(i);
			}
		}
	}

	private static void AddDesktopNoti(string plainText, int decayMultiplier)
	{
		CleanDesktopNotis();
		if (_desktopNotis.Count >= MAX_DESKTOP_NOTIS)
		{
			_desktopNotis.RemoveAt(0);
		}
		float duration = (DecayTime * decayMultiplier) * 0.02f;
		_desktopNotis.Add(new DesktopNoti
		{
			text = plainText,
			expireTime = Time.time + duration
		});
	}

	private void Init()
	{
		mainCamera = GameObject.Find("Main Camera");
		if (!((Object)(object)mainCamera == (Object)null))
		{
			hudObj = new GameObject("NOTIFICATIONLIB_HUD_OBJ");
			hudObjParent = new GameObject("NOTIFICATIONLIB_HUD_OBJ2");
			Canvas val = hudObj.AddComponent<Canvas>();
			hudObj.AddComponent<CanvasScaler>();
			hudObj.AddComponent<GraphicRaycaster>();
			((Behaviour)val).enabled = true;
			val.renderMode = (RenderMode)2;
			val.worldCamera = mainCamera.GetComponent<Camera>();
			hudObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
			((Transform)hudObj.GetComponent<RectTransform>()).position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
			hudObjParent.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z - 4.6f);
			hudObj.transform.SetParent(hudObjParent.transform);
			((Transform)hudObj.GetComponent<RectTransform>()).localPosition = new Vector3(0f, 0f, 1.6f);
			Quaternion rotation = ((Transform)hudObj.GetComponent<RectTransform>()).rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			eulerAngles.y = -270f;
			hudObj.transform.localScale = Vector3.one;
			((Transform)hudObj.GetComponent<RectTransform>()).rotation = Quaternion.Euler(eulerAngles);
			GameObject val2 = new GameObject();
			val2.transform.parent = hudObj.transform;
			notificationText = val2.AddComponent<Text>();
			notificationText.text = "";
			notificationText.fontSize = 32;
			notificationText.font = WristMenu.MenuFont;
			((Graphic)notificationText).rectTransform.sizeDelta = new Vector2(800f, 200f);
			notificationText.alignment = (TextAnchor)6;
			((Transform)((Graphic)notificationText).rectTransform).localScale = new Vector3(0.0025f, 0.0025f, 1f);
			((Transform)((Graphic)notificationText).rectTransform).localPosition = new Vector3(-0.3f, -0.175f, -0.15f);
			((Graphic)notificationText).material = textMaterial;
		}
	}

	private void FixedUpdate()
	{
		if (!hasInit)
		{
			if ((Object)(object)GameObject.Find("Main Camera") != (Object)null)
			{
				Init();
				hasInit = true;
			}
		}
		else
		{
			if ((Object)(object)mainCamera == (Object)null)
			{
				return;
			}
			hudObjParent.transform.position = mainCamera.transform.position;
			hudObjParent.transform.rotation = mainCamera.transform.rotation;
			if (notificationText.text != "")
			{
				decayCounter++;
				int num = ((lineDecayTimes.Count > 0) ? lineDecayTimes.Peek() : DecayTime);
				if (decayCounter > num)
				{
					string[] array = (from l in notificationText.text.Split(Environment.NewLine.ToCharArray()).Skip(1)
						where l != ""
						select l).ToArray();
					notificationText.text = string.Join("\n", array) + ((array.Length != 0) ? "\n" : "");
					if (lineDecayTimes.Count > 0)
					{
						lineDecayTimes.Dequeue();
					}
					decayCounter = 0;
				}
			}
			else
			{
				decayCounter = 0;
				lineDecayTimes.Clear();
			}
		}
	}

	public static void SendNotification(string text, int decayMultiplier = 1)
	{
		if (!(lastNotifyTime < Time.time))
		{
			return;
		}
		lastNotifyTime = Time.time + 0.05f;
		if (!IsEnabled)
		{
			return;
		}
		int num = text.IndexOf("] ");
		if (num >= 0)
		{
			text = text.Substring(num + 2);
		}
		string plainText = Regex.Replace(text, "<[^>]*>", "").Trim();
		string richText = "<color=green>[Noti]</color> - " + plainText;

		if (IsDesktopMode())
		{
			AddDesktopNoti(richText, decayMultiplier);
		}

		if ((Object)(object)notificationText == (Object)null)
		{
			return;
		}
		string vrText = richText;
		if (!vrText.Contains(Environment.NewLine))
		{
			vrText += Environment.NewLine;
		}
		string[] array = notificationText.text.Split(Environment.NewLine.ToCharArray());
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != "")
			{
				num2++;
			}
		}
		int num3 = 4;
		if (num2 >= num3)
		{
			int num4 = num2 - num3 + 1;
			string[] value = array.Where((string l) => l != "").Skip(num4).ToArray();
			notificationText.text = string.Join("\n", value) + "\n";
			for (int num5 = 0; num5 < num4; num5++)
			{
				if (lineDecayTimes.Count <= 0)
				{
					break;
				}
				lineDecayTimes.Dequeue();
			}
		}
		Text obj = notificationText;
		obj.text += vrText;
		lineDecayTimes.Enqueue(DecayTime * decayMultiplier);
	}

	public static void ClearAllNotifications()
	{
		if ((Object)(object)notificationText != (Object)null)
		{
			notificationText.text = "";
		}
		_desktopNotis.Clear();
	}

	public static void ClearPastNotifications(int amount)
	{
		if (!((Object)(object)notificationText == (Object)null))
		{
			string[] array = (from l in notificationText.text.Split(Environment.NewLine.ToCharArray()).Skip(amount)
				where l != ""
				select l).ToArray();
			notificationText.text = string.Join("\n", array) + ((array.Length != 0) ? "\n" : "");
		}
		for (int i = 0; i < amount && _desktopNotis.Count > 0; i++)
		{
			_desktopNotis.RemoveAt(0);
		}
	}
}

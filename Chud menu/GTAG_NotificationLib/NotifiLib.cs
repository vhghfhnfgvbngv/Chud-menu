using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chud.UI;
using UnityEngine;
using UnityEngine.UI;
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
		if ((Object)(object)notificationText == (Object)null || !(lastNotifyTime < Time.time))
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
		text = Regex.Replace(text, "<[^>]*>", "");
		text = "<color=green>[Noti]</color> - " + text.Trim();
		if (!text.Contains(Environment.NewLine))
		{
			text += Environment.NewLine;
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
		obj.text += text;
		lineDecayTimes.Enqueue(DecayTime * decayMultiplier);
	}

	public static void ClearAllNotifications()
	{
		if ((Object)(object)notificationText != (Object)null)
		{
			notificationText.text = "";
		}
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
	}
}

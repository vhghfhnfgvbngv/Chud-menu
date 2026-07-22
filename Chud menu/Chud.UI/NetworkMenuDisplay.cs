using System.Collections;
using System.Collections.Generic;
using Chud.Backend;
using Chud.Classes;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;

namespace Chud.UI;

internal static class NetworkMenuDisplay
{
	private const float OPEN_ANIMATION_SPEED = 0.3f;

	private const float CLOSE_ANIMATION_SPEED = 0.3f;

	private static Shader CachedUberShader => ShaderCache.Uber;
	private static Shader CachedGuiTextShader => ShaderCache.GuiText;

	public static void Create(Mods.RemoteMenuState state)
	{
		if (!((Object)(object)state.displayObject != (Object)null))
		{
			GameObject val = new GameObject("RemoteMenu_" + state.player.ActorNumber);
			val.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);
			GameObject val3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.Destroy((Object)(object)val3.GetComponent<Rigidbody>());
			Object.Destroy((Object)(object)val3.GetComponent<BoxCollider>());
			val3.transform.parent = val.transform;
			val3.transform.localRotation = Quaternion.identity;
			val3.transform.localScale = new Vector3(0.1f, 1f, 1f);
			val3.transform.position = new Vector3(0.05f, 0f, 0f);
			((Object)val3).name = "chud_bg";
			Renderer component = val3.GetComponent<Renderer>();
			Color bgTop = state.menuColors.NormalColor * 0.35f;
			component.material = WristMenu.MakeGradientMat(bgTop, state.menuColors.NormalColor);
			GameObject val4 = new GameObject("Canvas");
			val4.transform.parent = val.transform;
			Canvas val5 = val4.AddComponent<Canvas>();
			CanvasScaler val6 = val4.AddComponent<CanvasScaler>();
			val4.AddComponent<GraphicRaycaster>();
			val5.renderMode = RenderMode.WorldSpace;
			val6.dynamicPixelsPerUnit = 1900f;
			val6.referencePixelsPerUnit = 100f;
			GameObject val7 = new GameObject("MenuTitle");
			val7.transform.parent = val4.transform;
			Text val8 = val7.AddComponent<Text>();
			val8.font = WristMenu.MenuFont;
			val8.text = WristMenu.MenuTitle;
			val8.fontSize = 200;
			((Graphic)val8).color = state.menuColors.MenuTitleColor;
			val8.fontStyle = FontStyle.Bold;
			val8.alignment = TextAnchor.MiddleCenter;
			val8.resizeTextForBestFit = true;
			val8.resizeTextMinSize = 0;
			val8.resizeTextMaxSize = 200;
			RectTransform component2 = ((Component)val8).GetComponent<RectTransform>();
			((Transform)component2).localPosition = Vector3.zero;
			component2.sizeDelta = new Vector2(0.28f, 0.05f);
			((Transform)component2).position = new Vector3(0.06f, 0f, 0.175f);
			((Transform)component2).localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			GameObject val9 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.Destroy((Object)(object)val9.GetComponent<Rigidbody>());
			((Collider)val9.GetComponent<BoxCollider>()).isTrigger = true;
			val9.transform.parent = val.transform;
			val9.transform.localRotation = Quaternion.identity;
			val9.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
			val9.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
			Renderer component3 = val9.GetComponent<Renderer>();
			Color dcTop = WristMenu.DisconnectButtonColor * 0.35f;
			component3.material = WristMenu.MakeGradientMat(dcTop, WristMenu.DisconnectButtonColor);
			val9.AddComponent<BtnCollider>().relatedText = "DisconnectingButton";
			GameObject val10 = new GameObject();
			val10.transform.parent = val4.transform;
			Text val11 = val10.AddComponent<Text>();
			val11.font = WristMenu.MenuFont;
			val11.text = "Disconnect";
			val11.fontSize = 200;
			((Graphic)val11).color = WristMenu.DisconnectTextColor;
			val11.alignment = TextAnchor.MiddleCenter;
			val11.resizeTextForBestFit = true;
			val11.resizeTextMinSize = 0;
			val11.resizeTextMaxSize = 200;
			val11.fontStyle = FontStyle.Bold;
			RectTransform component4 = ((Component)val11).GetComponent<RectTransform>();
			((Transform)component4).localPosition = Vector3.zero;
			component4.sizeDelta = new Vector2(0.2f, 0.03f);
			((Transform)component4).localPosition = new Vector3(0.064f, 0f, 0.23f);
			((Transform)component4).localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			GameObject val12 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.Destroy((Object)(object)val12.GetComponent<Rigidbody>());
			((Collider)val12.GetComponent<BoxCollider>()).isTrigger = true;
			val12.transform.parent = val.transform;
			val12.transform.localRotation = Quaternion.identity;
			val12.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
			val12.transform.localPosition = new Vector3(0.56f, 0.65f, 0f);
			Renderer component5 = val12.GetComponent<Renderer>();
			Color npTop = state.menuColors.NextPrevButtonColor * 0.35f;
			component5.material = WristMenu.MakeGradientMat(npTop, state.menuColors.NextPrevButtonColor);
			val12.AddComponent<BtnCollider>().relatedText = "PreviousPage";
			GameObject val13 = new GameObject();
			val13.transform.parent = val4.transform;
			Text val14 = val13.AddComponent<Text>();
			val14.font = WristMenu.MenuFont;
			val14.text = "<";
			val14.fontSize = 200;
			((Graphic)val14).color = WristMenu.NextPrevTextColor;
			val14.fontStyle = FontStyle.Bold;
			val14.alignment = TextAnchor.MiddleCenter;
			val14.resizeTextForBestFit = true;
			val14.resizeTextMinSize = 0;
			val14.resizeTextMaxSize = 200;
			RectTransform component6 = ((Component)val14).GetComponent<RectTransform>();
			((Transform)component6).localPosition = Vector3.zero;
			component6.sizeDelta = new Vector2(0.2f, 0.03f);
			((Transform)component6).localPosition = new Vector3(0.064f, 0.195f, 0f);
			((Transform)component6).localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			GameObject val15 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.Destroy((Object)(object)val15.GetComponent<Rigidbody>());
			((Collider)val15.GetComponent<BoxCollider>()).isTrigger = true;
			val15.transform.parent = val.transform;
			val15.transform.localRotation = Quaternion.identity;
			val15.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
			val15.transform.localPosition = new Vector3(0.56f, -0.65f, 0f);
			Renderer component7 = val15.GetComponent<Renderer>();
			component7.material = WristMenu.MakeGradientMat(npTop, state.menuColors.NextPrevButtonColor);
			val15.AddComponent<BtnCollider>().relatedText = "NextPage";
			GameObject val16 = new GameObject();
			val16.transform.parent = val4.transform;
			Text val17 = val16.AddComponent<Text>();
			val17.font = WristMenu.MenuFont;
			val17.text = ">";
			val17.fontSize = 200;
			((Graphic)val17).color = WristMenu.NextPrevTextColor;
			val17.fontStyle = FontStyle.Bold;
			val17.alignment = TextAnchor.MiddleCenter;
			val17.resizeTextForBestFit = true;
			val17.resizeTextMinSize = 0;
			val17.resizeTextMaxSize = 200;
			RectTransform component8 = ((Component)val17).GetComponent<RectTransform>();
			((Transform)component8).localPosition = Vector3.zero;
			component8.sizeDelta = new Vector2(0.2f, 0.03f);
			((Transform)component8).localPosition = new Vector3(0.064f, -0.195f, 0f);
			((Transform)component8).localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			BuildButtonPage(state, val, val4);
		state.displayObject = val;
		UpdatePosition(state);
		val.transform.localScale = Vector3.zero;
			Mods.instance.StartCoroutine(OpenAni(state));
		}
	}

	private static IEnumerator OpenAni(Mods.RemoteMenuState state)
	{
		GameObject root = state.displayObject;
		if ((Object)(object)root == (Object)null)
		{
			yield break;
		}
		if (!state.animationsEnabled)
		{
			float ps = GTPlayer.Instance.scale;
			root.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f) * ps;
			yield break;
		}
		float elapsed = 0f;
		Vector3 startScale = root.transform.localScale;
		float playerScale = GTPlayer.Instance.scale;
		Vector3 targetScale = new Vector3(0.1f, 0.3f, 0.4f) * playerScale;
		while (elapsed < 0.3f)
		{
			if ((Object)(object)root == (Object)null || state.closing)
			{
				yield break;
			}
			float t = elapsed / 0.3f;
			float s = 1.70158f;
			t -= 1f;
			float bounce = t * t * ((s + 1f) * t + s) + 1f;
			root.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if ((Object)(object)root != (Object)null)
		{
			root.transform.localScale = targetScale;
		}
	}

	public static void CloseAndDestroy(Mods.RemoteMenuState state)
	{
		if (!((Object)(object)state.displayObject == (Object)null) && !state.closing)
		{
			state.closing = true;
			Mods.instance.StartCoroutine(CloseAniCoroutine(state));
		}
	}

	private static IEnumerator CloseAniCoroutine(Mods.RemoteMenuState state)
	{
		GameObject root = state.displayObject;
		if ((Object)(object)root == (Object)null)
		{
			yield break;
		}
		if (!state.animationsEnabled)
		{
			if ((Object)(object)root != (Object)null)
			{
				Object.Destroy((Object)(object)root);
			}
			state.displayObject = null;
			Mods.RemoveRemoteMenuState(state.player);
			yield break;
		}
		float elapsed = 0f;
		Vector3 startScale = root.transform.localScale;
		Vector3 targetScale = Vector3.zero;
		while (elapsed < 0.3f)
		{
			if ((Object)(object)root == (Object)null)
			{
				yield break;
			}
			float t = elapsed / 0.3f;
			float s = 1.70158f;
			float bounce = t * t * ((s + 1f) * t - s);
			root.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if ((Object)(object)root != (Object)null)
		{
			Object.Destroy((Object)(object)root);
		}
		state.displayObject = null;
		Mods.RemoveRemoteMenuState(state.player);
	}

	private static void BuildButtonPage(Mods.RemoteMenuState state, GameObject root, GameObject canvasObj)
	{
		List<string> categoryButtons = GetCategoryButtons(state.category);
		int page = state.page;
		List<string> list = new List<string>();
		int num = page * 7;
		for (int i = num; i < num + 7 && i < categoryButtons.Count; i++)
		{
			list.Add(categoryButtons[i]);
		}
		for (int j = 0; j < list.Count; j++)
		{
			float num2 = (float)j * 0.116f;
			string text = list[j];
			GameObject val = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Object.Destroy((Object)(object)val.GetComponent<Rigidbody>());
			((Collider)val.GetComponent<BoxCollider>()).isTrigger = true;
			val.transform.parent = root.transform;
			val.transform.localRotation = Quaternion.identity;
			val.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
			val.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - num2);
			bool value;
			bool flag = state.buttonStates.TryGetValue(text, out value) && value;
			Renderer component = val.GetComponent<Renderer>();
			Color btnBase = flag ? state.menuColors.ButtonColorEnabled : state.menuColors.ButtonColorDisable;
			component.material = WristMenu.MakeGradientMat(btnBase * 0.35f, btnBase);
			val.AddComponent<BtnCollider>().relatedText = text;
			GameObject val2 = new GameObject();
			val2.transform.parent = canvasObj.transform;
			Text val3 = val2.AddComponent<Text>();
			val3.font = WristMenu.MenuFont;
			val3.text = text;
			val3.fontSize = 200;
			val3.supportRichText = true;
			((Graphic)val3).color = (flag ? state.menuColors.EnableTextColor : state.menuColors.DisableTextColor);
			val3.fontStyle = FontStyle.Bold;
			val3.alignment = TextAnchor.MiddleCenter;
			val3.resizeTextForBestFit = true;
			val3.resizeTextMinSize = 0;
			val3.resizeTextMaxSize = 200;
			RectTransform component2 = ((Component)val3).GetComponent<RectTransform>();
			((Transform)component2).localPosition = Vector3.zero;
			component2.sizeDelta = new Vector2(0.2f, 0.03f);
			((Transform)component2).localPosition = new Vector3(0.064f, 0f, 0.111f - num2 / 2.6f);
			((Transform)component2).localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		}
	}

	private static List<string> GetCategoryButtons(string categoryName)
	{
		MenuCategory menuCategory = MenuManager.Categories.Find((MenuCategory c) => c.Name == categoryName);
		if (menuCategory == null || menuCategory.Buttons == null)
		{
			return new List<string>();
		}
		List<string> list = new List<string>();
		foreach (ButtonInfo button in menuCategory.Buttons)
		{
			list.Add(button.buttonText);
		}
		return list;
	}

	public static void UpdateColors(Mods.RemoteMenuState state)
	{
		if ((Object)(object)state.displayObject == (Object)null || state.closing)
		{
			return;
		}
		foreach (Transform item in state.displayObject.transform)
		{
			Transform val2 = item;
			if (((Object)val2).name == "chud_bg")
			{
				Renderer component = ((Component)val2).GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					Color bgTop = state.menuColors.NormalColor * 0.35f;
					component.material = WristMenu.MakeGradientMat(bgTop, state.menuColors.NormalColor);
				}
				continue;
			}
			BtnCollider component2 = ((Component)val2).GetComponent<BtnCollider>();
			if (!((Object)(object)component2 != (Object)null) || string.IsNullOrEmpty(component2.relatedText))
			{
				continue;
			}
			Renderer component3 = ((Component)val2).GetComponent<Renderer>();
			if (!((Object)(object)component3 == (Object)null))
			{
				if (component2.relatedText == "DisconnectingButton")
				{
					Color dcTop = WristMenu.DisconnectButtonColor * 0.35f;
					component3.material = WristMenu.MakeGradientMat(dcTop, WristMenu.DisconnectButtonColor);
				}
				else if (component2.relatedText == "PreviousPage" || component2.relatedText == "NextPage")
				{
					Color npTop = state.menuColors.NextPrevButtonColor * 0.35f;
					component3.material = WristMenu.MakeGradientMat(npTop, state.menuColors.NextPrevButtonColor);
				}
				else
				{
					bool value;
					bool flag = state.buttonStates.TryGetValue(component2.relatedText, out value) && value;
					Color btnBase = flag ? state.menuColors.ButtonColorEnabled : state.menuColors.ButtonColorDisable;
					Color btnTop = btnBase * 0.35f;
					component3.material = WristMenu.MakeGradientMat(btnTop, btnBase);
				}
			}
		}
		foreach (Transform item2 in state.displayObject.transform)
		{
			Transform val3 = item2;
			if (!(((Object)val3).name == "Canvas"))
			{
				continue;
			}
			foreach (Transform item3 in val3)
			{
				Transform val4 = item3;
				Text component4 = ((Component)val4).GetComponent<Text>();
				if (!((Object)(object)component4 == (Object)null))
				{
					bool value2;
					if (component4.text == "Disconnect")
					{
						((Graphic)component4).color = WristMenu.DisconnectTextColor;
					}
					else if (component4.text == "<" || component4.text == ">")
					{
						((Graphic)component4).color = WristMenu.NextPrevTextColor;
					}
					else if (((Object)val4).name == "MenuTitle")
					{
						component4.text = WristMenu.MenuTitle;
						((Graphic)component4).color = state.menuColors.MenuTitleColor;
					}
					else if (state.buttonStates.TryGetValue(component4.text, out value2))
					{
						((Graphic)component4).color = (value2 ? state.menuColors.EnableTextColor : state.menuColors.DisableTextColor);
					}
					else
					{
						((Graphic)component4).color = state.menuColors.DisableTextColor;
					}
				}
			}
		}
		UpdatePosition(state);
	}

	public static void UpdateState(Mods.RemoteMenuState state)
	{
		if ((Object)(object)state.displayObject == (Object)null || state.closing)
		{
			return;
		}
		GameObject displayObject = state.displayObject;
		GameObject val = null;
		foreach (Transform item in displayObject.transform)
		{
			Transform val2 = item;
			if (((Object)val2).name == "Canvas")
			{
				val = ((Component)val2).gameObject;
				break;
			}
		}
		Shader uber = CachedUberShader;
		List<Transform> list = new List<Transform>();
		foreach (Transform item2 in displayObject.transform)
		{
			Transform val3 = item2;
			BtnCollider component = ((Component)val3).GetComponent<BtnCollider>();
			if ((Object)(object)component != (Object)null && component.relatedText != "DisconnectingButton" && component.relatedText != "PreviousPage" && component.relatedText != "NextPage")
			{
				((Component)val3).gameObject.SetActive(false);
				list.Add(val3);
			}
		}
		foreach (Transform item3 in list)
		{
			Object.Destroy((Object)(object)((Component)item3).gameObject);
		}
		if ((Object)(object)val != (Object)null)
		{
			List<Transform> list2 = new List<Transform>();
			foreach (Transform item4 in val.transform)
			{
				Transform val4 = item4;
				Text component2 = ((Component)val4).GetComponent<Text>();
				if ((Object)(object)component2 != (Object)null && ((Object)val4).name != "MenuTitle" && ((Object)val4).name != "PlayerName" && component2.text != "Disconnect" && component2.text != "<" && component2.text != ">")
				{
					((Component)val4).gameObject.SetActive(false);
					list2.Add(val4);
				}
			}
			foreach (Transform item5 in list2)
			{
				Object.Destroy((Object)(object)((Component)item5).gameObject);
			}
			foreach (Transform item6 in val.transform)
			{
				Transform val5 = item6;
				if (((Object)val5).name == "MenuTitle")
				{
					Text component3 = ((Component)val5).GetComponent<Text>();
					if ((Object)(object)component3 != (Object)null)
					{
						component3.text = WristMenu.MenuTitle;
					}
				}
			}
		}
		BuildButtonPage(state, displayObject, ((Object)(object)val != (Object)null) ? val : displayObject);
		UpdateColors(state);
	}

	public static void UpdatePosition(Mods.RemoteMenuState state)
	{
		if (!((Object)(object)state.displayObject == (Object)null) && !state.closing)
		{
			if ((Object)(object)state.cachedRig != (Object)null)
			{
				state.position = state.cachedRig.transform.position + state.rigOffset;
			}
			state.displayObject.transform.position = state.position;
			state.displayObject.transform.rotation = state.rotation;
		}
	}

	public static void SpawnRemoteObject(string objType, Vector3 pos, Vector3 rot, Vector3 scale, Color color)
	{
		GameObject val = GameObject.CreatePrimitive(objType.ToLower() switch
		{
			"sphere" => PrimitiveType.Sphere,
			"capsule" => PrimitiveType.Capsule,
			"cylinder" => PrimitiveType.Cylinder,
			_ => PrimitiveType.Cube,
		});
		Object.Destroy((Object)(object)val.GetComponent<Collider>());
		Object.Destroy((Object)(object)val.GetComponent<Rigidbody>());
		val.transform.position = pos;
		val.transform.rotation = Quaternion.Euler(rot);
		val.transform.localScale = scale;
		Renderer component = val.GetComponent<Renderer>();
		component.material.color = color;
		Shader val2 = CachedGuiTextShader;
		if ((Object)(object)val2 != (Object)null)
		{
			component.material.shader = val2;
		}
		Object.Destroy((Object)(object)val, 30f);
	}
}

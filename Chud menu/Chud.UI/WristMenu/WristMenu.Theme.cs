using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chud.UI;

internal partial class WristMenu
{
	public static Material MakeGradientMat(Color top, Color bot)
	{
		int h = 16;
		Texture2D tex = new Texture2D(2, h, TextureFormat.RGBA32, false);
		Color highlight = Color.Lerp(bot, Color.white, 0.075f);
		for (int y = 0; y < h; y++)
		{
			float t = 1f - Mathf.Abs((float)y / (h - 1) * 2f - 1f);
			tex.SetPixel(0, y, Color.Lerp(bot, highlight, t));
			tex.SetPixel(1, y, Color.Lerp(bot, highlight, t));
		}
		tex.Apply();
		tex.wrapMode = TextureWrapMode.Repeat;
		Material mat = new Material(Shader.Find("Unlit/Texture"));
		mat.mainTexture = tex;
		mat.color = Color.white;
		mat.mainTextureScale = new Vector2(1, 0.5f);
		gradientMaterials.Add(mat);
		return mat;
	}

	public static Material MakePlainMat(Color c)
	{
		Material mat = new Material(Shader.Find("Unlit/Color"));
		mat.color = c;
		return mat;
	}

	internal static void UpdateGradientAnimations(float time)
	{
		float offsetY = time * 0.2f;
		Vector2 offset = new Vector2(0f, offsetY);
		for (int i = gradientMaterials.Count - 1; i >= 0; i--)
		{
			if ((Object)(object)gradientMaterials[i] == (Object)null)
				gradientMaterials.RemoveAt(i);
			else
				gradientMaterials[i].mainTextureOffset = offset;
		}
	}

	public static void RoundGameObject(GameObject obj, string identifier, Color gradientTop, Color gradientBot)
	{
		Renderer component = obj.GetComponent<Renderer>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		Vector3 localScale = obj.transform.localScale;
		Vector3 localPosition = obj.transform.localPosition;
		Transform transform = menu.transform;
		GameObject rounded = new GameObject(identifier + "_rounded");
		rounded.transform.parent = transform;
		rounded.transform.rotation = Quaternion.identity;
		rounded.transform.localPosition = localPosition;
		rounded.transform.localScale = localScale;
		MeshFilter mf = rounded.AddComponent<MeshFilter>();
		MeshRenderer mr = rounded.AddComponent<MeshRenderer>();
		mf.mesh = GenerateRoundedRectMesh(1f, 1f, 0.08f, 6, 0.85f);
		mr.material = MakeGradientMat(gradientTop, gradientBot);
		roundedRenderers[identifier] = new List<Renderer> { mr };
		component.enabled = false;
	}

	// Must be called before destroying the menu/reference GameObjects so nothing leaks.
	private static void DestroyGradientResources()
	{
		foreach (Material mat in gradientMaterials)
		{
			DestroyMaterial(mat);
		}
		gradientMaterials.Clear();
		roundedRenderers.Clear();
		if ((Object)(object)reference != (Object)null)
		{
			Renderer refRenderer = reference.GetComponent<Renderer>();
			if ((Object)(object)refRenderer != (Object)null && (Object)(object)refRenderer.material != (Object)null)
			{
				DestroyMaterial(refRenderer.material);
			}
		}
	}

	private static void DestroyMaterial(Material mat)
	{
		if ((Object)(object)mat == (Object)null)
		{
			return;
		}
		if (mat.HasProperty("_MainTex"))
		{
			Texture mainTex = mat.mainTexture;
			if ((Object)(object)mainTex != (Object)null)
			{
				Object.Destroy((Object)(object)mainTex);
			}
		}
		Object.Destroy((Object)(object)mat);
	}
}
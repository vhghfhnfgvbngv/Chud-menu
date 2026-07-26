using UnityEngine;

namespace Chud.Backend;

internal static class MenuFixer
{
	private static Shader _cachedShader;

	public static void ApplyMenuShader(GameObject obj)
	{
		if (_cachedShader == null)
			_cachedShader = Shader.Find("GorillaTag/UberShader");
		if (_cachedShader != null)
			obj.GetComponent<Renderer>().material.shader = _cachedShader;
	}
}

using System.Collections;
using Chud.Backend;
using Chud.UI;
using UnityEngine;

internal class BtnCollider : MonoBehaviour
{
	public static int framePressCooldown;

	public string relatedText;

	private void OnTriggerEnter(Collider collider)
	{
		if (Time.frameCount >= framePressCooldown + WristMenu.ClickCooldown && ((Object)collider).name == "buttonPresser")
		{
			GorillaTagger.Instance.StartVibration(Mods.right, 0.01f, 0.001f);
			WristMenu.PlayButtonClickSound(Mods.right);
			if (WristMenu.animationsEnabled)
			{
				((MonoBehaviour)this).StartCoroutine(PressAni());
			}
			WristMenu.Toggle(relatedText);
			framePressCooldown = Time.frameCount;
		}
	}

	private IEnumerator PressAni()
	{
		Vector3 original = ((Component)this).transform.localScale;
		Vector3 pressed = original * 0.85f;
		float dur = 0.05f;
		float elapsed = 0f;
		while (elapsed < dur)
		{
			((Component)this).transform.localScale = Vector3.Lerp(original, pressed, elapsed / dur);
			elapsed += Time.deltaTime;
			yield return null;
		}
		((Component)this).transform.localScale = pressed;
		elapsed = 0f;
		while (elapsed < dur)
		{
			((Component)this).transform.localScale = Vector3.Lerp(pressed, original, elapsed / dur);
			elapsed += Time.deltaTime;
			yield return null;
		}
		((Component)this).transform.localScale = original;
	}
}

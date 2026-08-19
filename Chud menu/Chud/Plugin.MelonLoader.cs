#if MELONLOADER
using MelonLoader;
using UnityEngine;
using static Chud.PluginInfo;

namespace Chud;

[assembly: MelonInfo(typeof(PluginMelon), Name, Version)]
[assembly: MelonGame("Gorilla Tag", "Gorilla Tag")]

public class PluginMelon : MelonMod
{
	public override void OnInitializeMelon()
	{
		Bootstrapper.Patch();
		Bootstrapper.Initialize();
	}
}
#endif

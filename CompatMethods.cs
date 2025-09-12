using System;
using BBTimes.CustomComponents;
using BBTimes.Plugin;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;

namespace BBTimes.Compatibility
{
    //	internal static class CompatMethods // This whole class will hold a bunch of methods and patches for specific mods (with try catches to assure everything flows okay)
    //	{
    //		public static void InitializeCompatibilityChecks()
    //		{

    //				TryAction(Storage.guid_HookTweaks, (_) =>
    //				{

    //					GrapplingHookTweaks.Plugin.GrapplingHookPatch.OnWindowBreak += (pm, w) =>
    //					{
    //						if (!w.GetComponent<CustomWindowComponent>()?.unbreakable ?? true)
    //							pm.RuleBreak("breakingproperty", 3f, 0.15f);
    //					};

    //				});



    //				TryAction("pixelguy.pixelmodding.baldiplus.customvendingmachines", (_) => CustomVendingMachines.CustomVendingMachinesPlugin.AddDataFromDirectory(System.IO.Path.Combine(BasePlugin.ModPath, "objects", "VendingMachines")));

    //				TryAction(Storage.guid_CustomMusics, (_) =>
    //				{
    //					BBPlusCustomMusics.CustomMusicPlug.AddMidisFromDirectory(false, BasePlugin.ModPath, "misc", "Audios", "School");
    //					BBPlusCustomMusics.CustomMusicPlug.AddMidisFromDirectory(true, BasePlugin.ModPath, "misc", "Audios", "Elevator");
    //				});

    //		}

    //		static void TryAction(string modGUID, Action<BaseUnityPlugin> act)
    //		{
    //			try
    //			{
    //				if (Chainloader.PluginInfos.ContainsKey(modGUID))
    //					act(Chainloader.PluginInfos[modGUID].Instance);
    //			}
    //#if CHEAT
    //			catch (Exception e)
    //			{
    //				Debug.LogWarning("Error caught during compatibility action");
    //				Debug.LogException(e);
    //			}
    //#elif RELEASE
    //			catch{} // Suppress in release
    //#endif
    //		}


    //	}
}

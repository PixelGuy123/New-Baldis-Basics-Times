using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(ItemManager), "UseItem")]
	internal class ItemManagerPatches
	{
		private static bool Prefix(ItemManager __instance, ItemObject[] ___items, int ___selectedItem, PlayerManager ___pm)
		{
			var item = Object.Instantiate(___items[___selectedItem].item);
			item.gameObject.SetActive(true);
			if (item.Use(___pm))
				__instance.RemoveItem(___selectedItem);

			return false;
		}
	}
}

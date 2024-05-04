// using HarmonyLib;
// using System.Reflection;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_EmptyWaterBottle : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach))
			{
				var water = hit.transform.GetComponent<WaterFountain>();
				if (water)
				{
					this.pm = pm;
					InteractWithFountain(water);
					pm.itm.SetItem(waterBottle, pm.itm.selectedItem);
				}
			}
			return false;
		}

		void InteractWithFountain(WaterFountain fountain)
		{
			//((AudioManager)_waterfountain_audman.GetValue(fountain)).PlaySingle((SoundObject)_waterfountain_audSip.GetValue(fountain));
			fountain.audMan.PlaySingle(fountain.audSip);
		} // will be used to be patched by BB+ Animations aswell

		internal static ItemObject waterBottle;

		//readonly static FieldInfo _waterfountain_audman = AccessTools.Field(typeof(WaterFountain), "audMan");
		//readonly static FieldInfo _waterfountain_audSip = AccessTools.Field(typeof(WaterFountain), "audSip");
	}
}

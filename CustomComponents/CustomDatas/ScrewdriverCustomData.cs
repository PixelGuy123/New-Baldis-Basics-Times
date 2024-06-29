using BBTimes.CustomContent.CustomItems;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ScrewdriverCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => 
			[GetSound("sd_screw.wav", "Vfx_SD_screw", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var comp = GetComponent<ITM_Screwdriver>();
			comp.audScrew = soundObjects[0];
			comp.item = myEnum;
		}
	}
}

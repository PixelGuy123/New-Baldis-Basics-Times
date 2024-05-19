using BBTimes.CustomContent.CustomItems;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class InvisibilityControllerCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => 
			[GetSound("longHighBeep.wav", "InvCon_Active", SoundType.Effect, Color.white),
		GetSound("longDownBeep.wav", "InvCon_Deactive", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var inv = GetComponent<ITM_InvisibilityController>();
			inv.audUse = soundObjects[0];
			inv.audDeuse = soundObjects[1];
		}
	}
}

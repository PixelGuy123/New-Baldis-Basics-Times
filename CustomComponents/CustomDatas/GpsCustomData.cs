using BBTimes.CustomContent.CustomItems;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class GpsCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => 
			[GetSound("gps_beep.wav", "Vfx_GPS_Beep", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_GPS>().aud_beep = soundObjects[0];
		}
	}
}

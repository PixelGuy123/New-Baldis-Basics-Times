using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class GpsCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => 
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "gps_beep.wav")), "Vfx_GPS_Beep", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_GPS>().aud_beep = soundObjects[0];
		}
	}
}

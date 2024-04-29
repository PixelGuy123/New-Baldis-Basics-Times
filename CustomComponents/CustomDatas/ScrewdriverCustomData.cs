using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ScrewdriverCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => 
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "sd_screw.wav")), "Vfx_SD_screw", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_Screwdriver>().audScrew = soundObjects[0];
		}
	}
}

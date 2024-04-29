using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class InvisibilityControllerCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => 
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "longHighBeep.wav")), "InvCon_Active", SoundType.Effect, Color.white),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "longDownBeep.wav")), "InvCon_Deactive", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var inv = GetComponent<ITM_InvisibilityController>();
			inv.audUse = soundObjects[0];
			inv.audDeuse = soundObjects[1];
		}
	}
}

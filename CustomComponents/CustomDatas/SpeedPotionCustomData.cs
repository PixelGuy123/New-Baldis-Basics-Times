using System.IO;
using UnityEngine;
using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SpeedPotionCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => [ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "potion_speedCoilNoises.wav")), string.Empty, SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var speed = GetComponent<ITM_SpeedPotion>();
			speed.audPower = soundObjects[0];
			speed.audPower.subtitle = false;
			speed.audMan = gameObject.CreateAudioManager(gameObject.CreateAudioSource(75f, 75f), true).MakeAudioManagerNonPositional();
		}
	}
}

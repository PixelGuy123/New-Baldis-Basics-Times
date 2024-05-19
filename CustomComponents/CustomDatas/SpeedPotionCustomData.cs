using UnityEngine;
using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SpeedPotionCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() => [GetSound("potion_speedCoilNoises.wav", string.Empty, SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var speed = GetComponent<ITM_SpeedPotion>();
			speed.audPower = soundObjects[0];
			speed.audPower.subtitle = false;
			speed.audMan = gameObject.CreateAudioManager(75f, 75f)
				.MakeAudioManagerNonPositional();
		}
	}
}

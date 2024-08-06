using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BlowDrierCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("blowDrier.wav", SoundType.Effect)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var item = GetComponent<ITM_BlowDrier>();
			item.audMan = gameObject.CreateAudioManager(45, 65).MakeAudioManagerNonPositional();
			item.audBlow = soundObjects[0];
		}
	}
}

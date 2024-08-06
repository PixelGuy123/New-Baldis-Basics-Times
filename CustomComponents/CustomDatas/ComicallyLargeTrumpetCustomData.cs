

using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ComicallyLargeTrumpetCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("hrn_play.wav", SoundType.Effect)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var tr = GetComponent<ITM_ComicallyLargeTrumpet>();
			tr.audMan = gameObject.CreateAudioManager(100f, 110f).MakeAudioManagerNonPositional();
			tr.audBlow = soundObjects[0];
		}
	}
}



using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ComicallyLargeTrumpetCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("hrn_play.wav", "Vfx_ComicLargTrum_Blow", SoundType.Effect, UnityEngine.Color.white),
		GetSound("hrn_inhale.wav", "Vfx_ComicLargTrum_Inhale", SoundType.Effect, UnityEngine.Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var tr = GetComponent<ITM_ComicallyLargeTrumpet>();
			tr.audMan = gameObject.CreateAudioManager(100f, 110f).MakeAudioManagerNonPositional();
			tr.audBlow = soundObjects[0];
			tr.audInhale = soundObjects[1];
		}
	}
}

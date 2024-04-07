using BBTimes.CustomContent.NPCs;

namespace BBTimes.CustomComponents.CustomDatas
{
	internal class SuperintendentCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>

				[MTM101BaldAPI.ObjectCreators.CreateSoundObject(
				MTM101BaldAPI.AssetTools.AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "Superintendent.wav")), // Gets just one audio
				"Vfx_SI_BaldiHere", SoundType.Voice, new(0f, 0f, 0.796875f)
				)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var s = GetComponent<Superintendent>();
			s.audMan = s.GetComponent<AudioManager>();
			s.data = this;
		}
	}
}

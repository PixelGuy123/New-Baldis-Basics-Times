namespace BBTimes.CustomComponents.CustomDatas
{
	public class OfficeChairCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
				
				[MTM101BaldAPI.ObjectCreators.CreateSoundObject(
				MTM101BaldAPI.AssetTools.AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "ChairRolling.wav")), // Gets just one audio
				"Vfx_OFC_Walk", SoundType.Voice, new(0.74609375f, 0.74609375f, 0.74609375f)
				)];
		
	}
}

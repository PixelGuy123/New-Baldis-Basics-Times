using BBTimes.CustomContent.NPCs;
using MTM101BaldAPI.Registers;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HappyHolidaysCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
				
				[MTM101BaldAPI.ObjectCreators.CreateSoundObject(
				MTM101BaldAPI.AssetTools.AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "HappyHolidays.wav")), // Gets just one audio
				"Vfx_HapH_MerryChristmas", SoundType.Voice, new(0.796875f, 0f, 0f)
				)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<HappyHolidays>().objects = ItemMetaStorage.Instance.GetAllWithFlags(ItemFlags.None | ItemFlags.Persists | ItemFlags.CreatesEntity).ToValues();
		}

	}
}

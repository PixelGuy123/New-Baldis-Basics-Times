using BBTimes.CustomContent.NPCs;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ZeroPrizeCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "0thprize_mustsweep.wav")), "Vfx_0TH_Sweep", SoundType.Voice, new(0.99609375f, 0.99609375f, 0.796875f)),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "0thprize_timetosweep.wav")), "Vfx_0TH_WannaSweep", SoundType.Voice, new(0.99609375f, 0.99609375f, 0.796875f))
			];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var prize = (ZeroPrize)Npc;

			prize.audStartSweep = soundObjects[1];
			prize.audSweep = soundObjects[0];

			prize.activeSprite = storedSprites[1];
			prize.deactiveSprite = storedSprites[0];

			prize.audMan = GetComponent<PropagatedAudioManager>();

			((UnityEngine.CapsuleCollider)prize.baseTrigger[0]).radius = 4f; // default radius of Gotta Sweep
		}
	}
}

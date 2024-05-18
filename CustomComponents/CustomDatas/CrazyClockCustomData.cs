using BBTimes.CustomContent.NPCs;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using UnityEngine;
using System.IO;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CrazyClockCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
		[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "clock_tick.wav")), "Vfx_CC_Tick", SoundType.Voice, Color.yellow),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "clock_tack.wav")), "Vfx_CC_Tack", SoundType.Voice, Color.yellow),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "clock_Scream.wav")), "Vfx_CC_Scream", SoundType.Voice, Color.yellow),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "clock_frown.wav")), "Vfx_CC_Frown", SoundType.Voice, Color.yellow)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var clock = (CrazyClock)Npc;
			clock.data = this;
			clock.spriteRenderer[0].material = new(ObjectCreationExtensions.NonBillBoardPrefab.material);
			clock.GetComponents<Collider>().Do(x => x.enabled = false);
			clock.audMan = GetComponent<AudioManager>();
			clock.Navigator.enabled = false; // It's a static npc
			clock.Navigator.Entity.SetActive(false);
			clock.Navigator.Entity.enabled = false;
		}

	}
}

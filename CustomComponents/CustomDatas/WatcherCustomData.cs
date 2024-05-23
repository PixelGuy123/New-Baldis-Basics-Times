using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class WatcherCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
		[GetSound("WCH_ambience.wav", "Vfx_Wch_Idle", SoundType.Voice, new Color(0.8f, 0.8f, 0.8f)),
		GetSoundNoSub("WCH_see.wav", SoundType.Effect),
		GetSound("WCH_angered.wav", "Vfx_Wch_Angry", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f)),
		GetSound("WCH_teleport.wav", "Vfx_Wch_Teleport", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(35f, "watcher.png")];


		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var w = (Watcher)Npc;

			w.audMan = GetComponent<PropagatedAudioManager>();

			w.audAmbience = soundObjects[0];
			w.audSpot = soundObjects[1];
			w.audAngry = soundObjects[2];
			w.audTeleport = soundObjects[3];

			w.spriteToHide = w.spriteRenderer[0];
			w.screenAudMan = gameObject.CreateAudioManager(45f, 75f).MakeAudioManagerNonPositional();
		}
	}
}

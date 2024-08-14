using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using UnityEngine;
using PixelInternalAPI.Classes;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class WatcherCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
		[GetSound("WCH_ambience.wav", "Vfx_Wch_Idle", SoundType.Voice, new Color(0.8f, 0.8f, 0.8f)),
		GetSoundNoSub("WCH_see.wav", SoundType.Effect),
		GetSound("WCH_angered.wav", "Vfx_Wch_Angry", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f)),
		GetSound("WCH_teleport.wav", "Vfx_Wch_Teleport", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f)),
		GetSound("SHDWCH_spawn.wav", "Vfx_Wch_Spawn", SoundType.Effect, new Color(0.6f, 0.6f, 0.6f)),
		GetSound("SHDWCH_ambience.wav", "Vfx_Wch_Idle", SoundType.Effect, new Color(0.6f, 0.6f, 0.6f)),
			];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(2, 1, 35f, "watcher.png");


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

			var hallRender = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1]);
			hallRender.gameObject.layer = LayerMask.NameToLayer("Overlay");
			hallRender.name = "WatcherHallucination";
			hallRender.gameObject.ConvertToPrefab(true);

			var hall = hallRender.gameObject.AddComponent<Hallucinations>();
			hall.audMan = hall.gameObject.CreateAudioManager(45f, 65f);
			hall.audSpawn = soundObjects[4];
			hall.audLoop = soundObjects[5];
			hall.renderer = hallRender;

			w.hallPre = hall;
		}
	}
}

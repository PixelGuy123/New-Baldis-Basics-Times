using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class MughCustomData : CustomNPCData
	{
		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(5, 1, 55f, "muggy.png");
		protected override SoundObject[] GenerateSoundObjects() =>
			[
			GetSound("Mugh_LetsHug.wav", "Vfx_Mugh_Hug1", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_LetsBeFriends.wav", "Vfx_Mugh_Hug2", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_LoveYou.wav", "Vfx_Mugh_Hug3", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_WhyAreYouRunning.wav", "Vfx_Mugh_Left1", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_DontLeaveMe.wav", "Vfx_Mugh_Left2", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_WhatDidYouDo.wav", "Vfx_Mugh_Die1", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_Why.wav", "Vfx_Mugh_Die2", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("Mugh_Revived.wav", "Vfx_Mugh_Revive", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("mugh_noises.wav", "Vfx_Mugh_Noise", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			GetSound("mugh_die.wav", "Vfx_Mugh_Noise", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var mug = (Mugh)Npc;
			mug.audMan = GetComponent<PropagatedAudioManager>();
			mug.walkAudMan = gameObject.CreatePropagatedAudioManager(65, 75);
			mug.renderer = mug.spriteRenderer[0];
			mug.normSprite = storedSprites[0];
			mug.hugSprite = storedSprites[1];
			mug.sadSprite = storedSprites[2];
			mug.holeSprite = storedSprites[3];
			mug.deadSprite = storedSprites[4];

			mug.audFindPlayer = [.. soundObjects.Take(3)];
			mug.audLostPlayer = [.. soundObjects.Skip(3).Take(2)];
			mug.audGetHit = [.. soundObjects.Skip(5).Take(2)];
			mug.audDie = soundObjects[9];
			mug.audWalk = soundObjects[8];
			mug.audRevive = soundObjects[7];
		}
	}
}

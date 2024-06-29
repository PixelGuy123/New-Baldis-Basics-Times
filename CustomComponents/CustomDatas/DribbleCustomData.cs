using BBTimes.CustomComponents.NpcSpecificComponents;
using MTM101BaldAPI;
using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class DribbleCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[
			GetSound("bounce.wav", "BB_Bong", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSoundNoSub("throw.wav", SoundType.Voice),
			GetSound("DRI_Idle1.wav", "Vfx_Dribble_Idle1", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Idle2.wav", "Vfx_Dribble_Idle2", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Chase1.wav", "Vfx_Dribble_Notice1", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Chase2.wav", "Vfx_Dribble_Notice2", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Caught1.wav", "Vfx_Dribble_Caught1", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Caught2.wav", "Vfx_Dribble_Caught2", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Clap.wav", "Vfx_Dribble_Clap", SoundType.Voice, Color.white),
			GetSound("DRI_Instructions.wav", "Vfx_Dribble_Instructions", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)), // 9
			GetSound("DRI_Ready.wav", "Vfx_Dribble_Ready", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Catch.wav", "Vfx_Dribble_Catch", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Praise1.wav", "Vfx_Dribble_Praise1", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Praise2.wav", "Vfx_Dribble_Praise2", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Dismissed.wav", "Vfx_Dribble_Dismissed", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSoundNoSub("punch.wav", SoundType.Voice),
			GetSound("DRI_Disappointed1.wav", "Vfx_Dribble_Disappointed1", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Disappointed2.wav", "Vfx_Dribble_Disappointed2", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Angry1.wav", "Vfx_Dribble_Angry1", SoundType.Voice, new(1f, 0.15f, 0.15f)), // 18
			GetSound("DRI_Angry2.wav", "Vfx_Dribble_Angry2", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			GetSound("DRI_Step1.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_Step2.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			GetSound("DRI_AngryChase1.wav", "Vfx_Dribble_ChaseAngry1", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			GetSound("DRI_AngryChase2.wav", "Vfx_Dribble_ChaseAngry2", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			GetSound("DRI_AngryCaught1.wav", "Vfx_Dribble_CaughtAngry1", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			GetSound("DRI_AngryCaught2.wav", "Vfx_Dribble_CaughtAngry2", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			GetSound("DRI_AngryPush2.wav", "Vfx_Dribble_Punch1", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			GetSound("DRI_AngryPush2.wav", "Vfx_Dribble_Punch2", SoundType.Voice, new(1f, 0.15f, 0.15f))
			];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(pixelsPerUnit, "dri_normal1.png"), GetSprite(pixelsPerUnit, "dri_normal2.png"),
		GetSprite(pixelsPerUnit, "dri_idle1.png"), GetSprite(pixelsPerUnit, "dri_throw1.png"), GetSprite(pixelsPerUnit, "dri_throw2.png"),
		GetSprite(pixelsPerUnit, "dri_win1.png"), GetSprite(pixelsPerUnit, "dri_win2.png"),
		GetSprite(pixelsPerUnit, "dri_sad1.png"), GetSprite(pixelsPerUnit, "dri_sad2.png"),
		GetSprite(pixelsPerUnit, "dri_crazy1.png"), GetSprite(pixelsPerUnit, "dri_crazy2.png"),
		GetSprite(pixelsPerUnit, "dri_angryChase1.png"), GetSprite(pixelsPerUnit, "dri_angryChase2.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var dr = (Dribble)Npc;
			dr.audMan = GetComponent<PropagatedAudioManager>();
			dr.bounceAudMan = gameObject.CreatePropagatedAudioManager(85f, 125f);
			dr.audBounceBall = soundObjects[0];
			dr.audThrow = soundObjects[1];
			dr.audIdle = [soundObjects[2], soundObjects[3]];
			dr.audNotice = [soundObjects[4], soundObjects[5]];
			dr.audCaught = [soundObjects[6], soundObjects[7]];
			dr.audClap = soundObjects[8];
			dr.audInstructions = soundObjects[9];
			dr.audReady = soundObjects[10];
			dr.audCatch = soundObjects[11];
			dr.audPraise = [soundObjects[12], soundObjects[13]];
			dr.audDismissed = soundObjects[14];
			dr.audDisappointed = [soundObjects[16], soundObjects[17]];
			dr.audAngry = [soundObjects[18], soundObjects[19]];
			dr.audStep = [soundObjects[20], soundObjects[21]];
			dr.audChaseAngry = [soundObjects[22], soundObjects[23]];
			dr.audAngryCaught = [soundObjects[24], soundObjects[25]];
			dr.audPunchResponse = [soundObjects[26], soundObjects[27]];
			dr.audPunch = soundObjects[15];

			dr.renderer = dr.spriteRenderer[0];

			dr.idleSprs = [storedSprites[0], storedSprites[1]];
			dr.clapSprs = [storedSprites[5], storedSprites[6]];
			dr.classSprs = [storedSprites[2], storedSprites[3], storedSprites[4]];
			dr.disappointedSprs = [storedSprites[7], storedSprites[8]];
			dr.crazySprs = [storedSprites[9], storedSprites[10]];
			dr.chasingSprs = [storedSprites[11], storedSprites[12]];

			Sprite[] sprites = new Sprite[5];
			for (int i = 0; i < sprites.Length; i++)
				sprites[i] = BBTimesManager.man.Get<Sprite>($"basketBall{i}");

			var basket = new GameObject("DribbleBasketBall");

			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(sprites[0]);
			rendererBase.transform.SetParent(basket.transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.name = "sprite";
			basket.ConvertToPrefab(true);

			var comp = basket.AddComponent<PickableBasketball>();
			comp.gameObject.layer = LayerStorage.iClickableLayer;
			comp.entity = basket.CreateEntity(2f, 2f, basket.transform);
			comp.spriteAnim = sprites;
			comp.audHit = soundObjects[15];

			comp.renderer = rendererBase;

			dr.basketPre = comp;
		}

		const float pixelsPerUnit = 87f;
	}
}

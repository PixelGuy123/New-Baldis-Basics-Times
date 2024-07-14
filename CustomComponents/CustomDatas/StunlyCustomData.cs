using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class StunlyCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("stunly_noises.wav", SoundType.Voice),
		GetSound("stunly_stun.wav", "Vfx_Stunly_Stun", SoundType.Voice, Color.white),
		GetSound("StunlyChaseLaughter.wav", "Vfx_Stunly_Laughter", SoundType.Voice, Color.white)];

		protected override Sprite[] GenerateSpriteOrder() =>
			[.. GetSpriteSheet(7, 1, 35f, "stunly.png"), .. GetSpriteSheet(2, 1, 1f, "stunlyScreen.png"), GetSprite(30f, "StunningStars.png")];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var stunly = (Stunly)Npc;
			stunly.allSprites = [.. storedSprites];
			stunly.allSounds = [.. soundObjects];

			stunly.noiseMan = GetComponent<PropagatedAudioManager>();

			stunly.laughterMan = gameObject.CreatePropagatedAudioManager(75f, 100f);

			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(stunly.transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "stunlyOverlay";

			stunly.image = ObjectCreationExtensions.CreateImage(canvas, storedSprites[7]);

			stunly.stunlyCanvas = canvas;
			stunly.stunlyCanvas.gameObject.SetActive(false);

			var billboard = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[9]);
			billboard.transform.SetParent(stunly.transform);
			billboard.gameObject.SetActive(false);
			stunly.stars = billboard.gameObject.AddComponent<StarObject>();
		}
	}
}

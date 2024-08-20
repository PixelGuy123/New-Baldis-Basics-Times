using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CameraStandCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("photo.wav", SoundType.Voice)];
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(70f, "camStand.png"), BBTimesManager.man.Get<Sprite>("whiteScreen")];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var cs = (CameraStand)Npc;
			cs.audMan = GetComponent<PropagatedAudioManager>();
			cs.audPic = soundObjects[0];

			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(cs.transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "CameraStandOverlay";

			cs.image = ObjectCreationExtensions.CreateImage(canvas, storedSprites[1]);

			cs.stunCanvas = canvas;
			cs.stunCanvas.gameObject.SetActive(false);
		}
	}
}

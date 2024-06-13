using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.CustomContent.Objects;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CameraBuilderCustomData : CustomObjectPrefabData
	{
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(25f, "SecurityCamera.png"),
			GetSprite(15f, "tiledGrid.png")];

		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("alarm.wav", "Vfx_Camera_Alarm", SoundType.Voice, Color.white),
			GetSound("camSwitch.wav", "Vfx_Camera_Switch", SoundType.Voice, Color.white),
			GetSound("spot.wav", "Vfx_Camera_Spot", SoundType.Voice, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var cam = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]).AddSpriteHolder(9f, 0);
			cam.name = "Sprite";
			var camHolder = cam.transform.parent;
			camHolder.name = "SecurityCamera";
			camHolder.gameObject.ConvertToPrefab(true);

			var camComp = camHolder.gameObject.AddComponent<SecurityCamera>();
			camComp.collider = camHolder.gameObject.AddBoxCollider(new(0f, 1f, 5f), new(3f, 10f, 3f), true);

			var visionIndicator = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1], false);
			visionIndicator.gameObject.layer = 0;
			visionIndicator.material.SetTexture("_LightMap", null); // No light affected, it's always bright

			visionIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			visionIndicator.transform.localScale = new(1f, 1.172f, 1f);
			visionIndicator.name = "CameraVisionIndicator";
			visionIndicator.gameObject.ConvertToPrefab(true);

			camComp.visionIndicatorPre = visionIndicator;

			camComp.audMan = camHolder.gameObject.CreatePropagatedAudioManager(55f, 90f);
			camComp.audAlarm = soundObjects[0];
			camComp.audTurn = soundObjects[1];
			camComp.audDetect = soundObjects[2];

			GetComponent<CameraBuilder>().camPre = camHolder;
		}
	}
}

using UnityEngine;
using BBTimes.CustomContent.Events;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class FrozenEventCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			var sd = GetSound("baldi_freeze.wav", "Event_FreezeEvent0", SoundType.Effect, Color.green);
			sd.additionalKeys = [
				new() {time = 1.796f, key = "Event_FreezeEvent1"},
				new() {time = 5.705f, key = "Event_FreezeEvent2"},
				new() {time = 8.998f, key = "Event_FreezeEvent3"},
				new() {time = 11.223f, key = "Event_FreezeEvent4"},
				new() {time = 14.271f, key = "Event_FreezeEvent0"}
				];
			return [GetSoundNoSub("freeze.wav", SoundType.Effect), sd];
		}

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(1f, "icehud.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var v = GetComponent<FrozenEvent>();
			v.eventIntro = soundObjects[1];
			v.audMan = gameObject.CreateAudioManager(65, 85).MakeAudioManagerNonPositional();

			v.audFreeze = soundObjects[0];

			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "iceOverlay";
			ObjectCreationExtensions.CreateImage(canvas, storedSprites[0], true); // stunly stare moment
			canvas.gameObject.SetActive(false);

			v.canvasPre = canvas;
		}
	}
}

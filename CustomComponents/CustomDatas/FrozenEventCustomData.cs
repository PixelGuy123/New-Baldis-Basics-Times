using MTM101BaldAPI;
using UnityEngine;
using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.CustomContent.Events;
using PixelInternalAPI.Extensions;
using UnityEngine.UI;
using BBTimes.Manager;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class FrozenEventCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "freeze.wav")), string.Empty, SoundType.Effect, Color.white)];
			sds[0].subtitle = false;
			return sds;
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var v = GetComponent<FrozenEvent>();
			v.audMan = gameObject.CreateAudioManager(65, 85)
				.SetAudioManagerAsPrefab()
				.MakeAudioManagerNonPositional();

			v.audFreeze = soundObjects[0];

			var canvas = Instantiate(BBTimesManager.man.Get<Canvas>("CanvasPrefab"));
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "iceOverlay";
			canvas.GetComponentInChildren<Image>().sprite = storedSprites[0]; // stunly stare moment

			v.canvasPre = canvas;
		}
	}
}

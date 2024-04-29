using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BBTimes.Extensions.ObjectCreationExtensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class StunlyCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "stunly_noises.wav")), string.Empty, SoundType.Voice, Color.white),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "stunly_stun.wav")), "Vfx_Stunly_Stun", SoundType.Voice, Color.white),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "StunlyChaseLaughter.wav")), "Vfx_Stunly_Laughter", SoundType.Voice, Color.white)];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			soundObjects[0].subtitle = false;
			var stunly = GetComponent<Stunly>();
			stunly.dat = this;

			stunly.noiseMan = GetComponent<PropagatedAudioManager>();

			stunly.laughterMan = gameObject.CreatePropagatedAudioManager(75f, 100f).SetAudioManagerAsPrefab();

			var canvas = Instantiate(BBTimesManager.man.Get<Canvas>("CanvasPrefab"));
			canvas.transform.SetParent(stunly.transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "stunlyOverlay";

			stunly.image = canvas.GetComponentInChildren<Image>();
			stunly.image.sprite = storedSprites[7]; // stunly stare moment

			stunly.stunlyCanvas = canvas;
			stunly.stunlyCanvas.gameObject.SetActive(false);

			var billboard = ObjectCreationExtension.CreateSpriteBillboard(storedSprites[9], false);
			DontDestroyOnLoad(billboard);
			billboard.gameObject.SetActive(false);
			stunly.stars = billboard.gameObject.AddComponent<StarObject>();
		}
	}
}

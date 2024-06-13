using BBTimes.CustomContent.Builders;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SquisherBuilderCustomData : CustomObjectPrefabData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[
			GetSound("squishHit.wav", "Sfx_Doors_StandardShut", SoundType.Voice, Color.white),
			GetSound("squishSquishing.wav", "Vfx_Squish_Running", SoundType.Voice, Color.white),
			GetSound("squisherPrepare.wav", "Vfx_Squish_Running", SoundType.Voice, Color.white)
			];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var squishBase = new GameObject("Squisher").AddComponent<Squisher>();
			squishBase.gameObject.ConvertToPrefab(true);
			var collider = squishBase.gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size = Vector3.one * 5f;
			squishBase.collider = collider;

			collider = squishBase.gameObject.AddComponent<BoxCollider>();
			collider.size = new(8f, 10f, 8f);
			collider.enabled = false;

			squishBase.blockCollider = collider;

			squishBase.audMan = squishBase.gameObject.CreatePropagatedAudioManager(100f, 105f);
			squishBase.audHit = soundObjects[0];
			squishBase.audRun = soundObjects[1];
			squishBase.audPrepare = soundObjects[2];

			// Head
			var squishHead = ObjectCreationExtension.CreateCube(GetTexture("squisherHead.png"));
			Destroy(squishHead.GetComponent<BoxCollider>());
			
			squishHead.transform.SetParent(squishBase.transform);
			squishHead.transform.localPosition = new(-headSize / 2, 0f, -headSize / 2);
			squishHead.transform.localScale = new(headSize, 1f, headSize);

			// Body
			var squishBody = ObjectCreationExtension.CreateCube(GetTexture("sides.png"), false);
			Destroy(squishBody.GetComponent<BoxCollider>());
			squishBody.transform.SetParent(squishBase.transform);
			squishBody.transform.localPosition = Vector3.up * ((bodyHeight / 2) + 1);
			squishBody.transform.localScale = new(headSize / 2, bodyHeight, headSize / 2);

			var builder = GetComponent<SquisherBuilder>();
			builder.squisherPre = squishBase;
			builder.buttonPre = BBTimesManager.man.Get<GameButton>("buttonPre");
		}

		const float headSize = 6f, bodyHeight = 9f;

	}
}

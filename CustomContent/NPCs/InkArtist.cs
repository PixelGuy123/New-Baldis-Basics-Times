using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomContent.NPCs
{
	// TODO: assign the blob sound recently added
	public class InkArtist : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(25f, "Ink_Artist.png");
			audMan = GetComponent<PropagatedAudioManager>();
			audSplash = this.GetSound("splash.wav", "Vfx_InkArt_Splash", SoundType.Effect, Color.white);

			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "InkOverlay";

			image = ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(1f, "ink.png"));

			stunCanvas = canvas;
			stunCanvas.gameObject.SetActive(false);

			var attVisual = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "inkWall.png")).AddSpriteHolder(out var inkWallRenderer, 0f);
			attPre = attVisual.gameObject.AddComponent<VisualAttacher>();
			attPre.gameObject.AddComponent<BillboardRotator>();
			attPre.name = "InkVisual";
			attPre.gameObject.ConvertToPrefab(true);

			inkWallRenderer.transform.localScale = new(2f, 1f, 1f);
			inkWallRenderer.transform.localPosition = Vector3.forward * -0.5f;

			gaugeSprite = this.GetSprite(Storage.GaugeSprite_PixelsPerUnit, "gaugeIcon.png");

			// Blob Setup
			blobImage = ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(20f, "inkBlob.png"), false);
			blobImage.gameObject.SetActive(false); // Will serve as prefab
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }


		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSplash, audBlob;

		[SerializeField]
		internal Canvas stunCanvas;

		[SerializeField]
		internal Image image, blobImage;

		[SerializeField]
		internal VisualAttacher attPre;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float maxInkCooldown = 12f, minRngBlobSize = 0.5f, maxRngBlobSize = 1.5f;
		[SerializeField]
		internal int inkShakeRemovals = 3, minBlobsPerShake = 2, maxBlobsPerShake = 5;
		[SerializeField]
		internal float shakeSensitivity = 120f; // degrees per second
		[SerializeField]
		internal float shakeCooldown = 0.5f; // seconds between shakes
		[SerializeField]
		[Range(0f, 1f)]
		internal float fadeEffectFactor = 0.125f, blobBoundaryFactor = 0.8f;

		HudGauge gauge;

		Coroutine uiInkCooldown;
		readonly List<KeyValuePair<NPCAttributesContainer, ValueModifier>> affectedNpcs = [];
		static internal readonly List<KeyValuePair<PlayerManager, InkArtist>> affectedPlayers = [];

		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = 15f;
			navigator.SetSpeed(15f);
			behaviorStateMachine.ChangeState(new InkArtist_StateBase(this));
		}

		public void InkPlayer(PlayerManager pm)
		{
			audMan.PlaySingle(audSplash);

			if (!affectedPlayers.Exists(x => x.Key == pm))
				affectedPlayers.Add(new(pm, this));

			if (uiInkCooldown != null)
				StopCoroutine(uiInkCooldown);

			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, maxInkCooldown);

			uiInkCooldown = StartCoroutine(InkCamera(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam));
		}

		void CreateRandomBlobAndDisperse(Vector2 boundaries)
		{
			var blob = Instantiate(blobImage);
			blob.transform.SetParent(stunCanvas.transform, false);


			var randPos = new Vector2(Random.Range(-boundaries.x, boundaries.x), Random.Range(-boundaries.y, boundaries.y));
			blob.rectTransform.localPosition = new Vector2(randPos.x, randPos.y);
			blob.rectTransform.localScale = Vector2.one * Random.Range(minRngBlobSize, maxRngBlobSize);

			blob.gameObject.SetActive(true);
			blob.StartCoroutine(BlobDisperse(blob));
		}

		public void InkEntity(NPC ent)
		{
			audMan.PlaySingle(audSplash);

			var att = Instantiate(attPre);
			att.AttachTo(ent.transform, true);
			att.SetOwnerRefToSelfDestruct(gameObject);

			att.StartCoroutine(InkNPC(att.gameObject, ent));
		}

		public void CancelCameraInk()
		{
			stunCanvas.gameObject.SetActive(false);
			if (uiInkCooldown != null)
				StopCoroutine(uiInkCooldown);
			affectedPlayers.RemoveAll(x => x.Value == this);
		}

		public override void Despawn()
		{
			base.Despawn();
			while (affectedNpcs.Count != 0)
			{
				affectedNpcs[0].Key?.RemoveLookerMod(affectedNpcs[0].Value);
				affectedNpcs.RemoveAt(0);
			}

			affectedPlayers.RemoveAll(x => x.Value == this);

			gauge?.Deactivate();
		}

		IEnumerator InkCamera(Camera target)
		{
			stunCanvas.gameObject.SetActive(true);
			stunCanvas.worldCamera = target;
			Vector3 ogSize = Vector3.one;
			Vector3 zero = Vector3.zero;

			float fadeTime = maxInkCooldown * fadeEffectFactor; // x% is used for fade in/out effect
			float displayTime = maxInkCooldown * (1f - fadeEffectFactor * 2f); // The rest of the cooldown is display on screen
			float totalTime = fadeTime * 2f + displayTime;
			float t = 0f;

			image.transform.localScale = Vector3.zero;

			// --- Shake mechanic state ---
			int shakesLeft = inkShakeRemovals;
			Quaternion lastRot = target.transform.rotation;
			float shakeTimer = 0f;

			// Fade-in
			t = 0f;
			while (t < fadeTime)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime;
				image.transform.localScale = Vector3.Lerp(zero, ogSize, t / fadeTime);
				gauge.SetValue(totalTime, totalTime - t);
				yield return null;
			}
			image.transform.localScale = ogSize;

			// Full display with shake detection
			t = 0f;
			totalTime -= fadeTime; // Remove one fadeTime from the total
			while (t < displayTime)
			{
				// --- Shake detection ---
				shakeTimer -= ec.EnvironmentTimeScale * Time.deltaTime;
				Quaternion currentRot = target.transform.rotation;
				float angleDelta = Quaternion.Angle(lastRot, currentRot) / Time.deltaTime;
				lastRot = currentRot;

				if (shakesLeft > 0 && shakeTimer <= 0f && angleDelta > shakeSensitivity)
				{
					shakesLeft--;
					shakeTimer = shakeCooldown;
					// Shrink ink image with bouncy effect
					yield return StartCoroutine(ShrinkInkBouncy(image, ogSize, zero, 0.18f, shakesLeft <= 0));

					// Spawn blobs
					int blobCount = Random.Range(minBlobsPerShake, maxBlobsPerShake + 1);
					Vector2 bounds = new(image.rectTransform.rect.width, image.transform.localScale.x * blobBoundaryFactor);
					for (int i = 0; i < blobCount; i++)
						CreateRandomBlobAndDisperse(bounds);

					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBlob);

					// If no shakes left, break early
					if (shakesLeft <= 0)
					{
						goto finish;
					}
					// Restore ink image to full size for next shake
					image.transform.localScale = ogSize;
				}

				t += ec.EnvironmentTimeScale * Time.deltaTime;
				gauge.SetValue(totalTime, totalTime - t);
				yield return null;
			}

			// Fade-out
			t = 0f;
			totalTime -= displayTime; // Remove display time from the total
			while (t < fadeTime)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime;
				image.transform.localScale = Vector3.Lerp(ogSize, zero, t / fadeTime);
				gauge.SetValue(totalTime, totalTime - t);
				yield return null;
			}
			image.transform.localScale = zero;
			stunCanvas.gameObject.SetActive(false);

		finish:

			gauge.Deactivate();

			affectedPlayers.RemoveAll(x => x.Value == this);

			yield break;
		}

		// --- Helper coroutine for bouncy shrink effect ---
		IEnumerator ShrinkInkBouncy(Image img, Vector3 from, Vector3 to, float duration, bool disableAfterwards)
		{
			float t = 0f;
			while (t < duration)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime;
				// Bouncy effect using Mathf.PingPong
				float progress = t / duration;
				float bounce = Mathf.Sin(progress * Mathf.PI) * 0.2f + 0.8f; // 0.8-1.0 scale
				img.transform.localScale = Vector3.Lerp(from, to, progress) * bounce;
				yield return null;
			}
			img.transform.localScale = to;

			if (disableAfterwards)
				stunCanvas.gameObject.SetActive(false);
		}

		IEnumerator BlobDisperse(Image blob)
		{
			Vector2 startPos = blobImage.rectTransform.localPosition;
			Vector2 endPos = Storage.Const_RefScreenSize * 2.5f; // Expand a bit to actually go off-screen

			// If the blob is to the left, then does the screen size (-x)
			if (startPos.x < 0)
				endPos.x *= -1f;
			// Same for the y position
			if (startPos.y < 0)
				endPos.y *= -1f;

			// Choose between a random x or y direction to exit
			if (Random.value < 0.5f)
				endPos.x = Random.Range(-endPos.x, endPos.x);
			else
				endPos.y = Random.Range(-endPos.y, endPos.y);

			float t = 0f;
			while (t < 1f)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime;
				blob.rectTransform.localPosition = Vector2.Lerp(startPos, endPos, t);
				yield return null;
			}

			Destroy(blob.gameObject);
		}

		IEnumerator InkNPC(GameObject selfDestruct, NPC e)
		{
			var cont = e.GetNPCContainer();
			var valMod = new ValueModifier(0f);

			cont.AddLookerMod(valMod);
			affectedNpcs.Add(new(cont, valMod));

			float delay = maxInkCooldown * 0.95f;
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			cont?.RemoveLookerMod(valMod);
			affectedNpcs.RemoveAll(x => x.Key == cont || !x.Key);

			if (selfDestruct)
				Destroy(selfDestruct);
			yield break;

		}

		internal class InkArtist_StateBase(InkArtist art) : NpcState(art)
		{
			protected InkArtist art = art;
			float inkCooldown = 0f;

			public override void Enter()
			{
				base.Enter();
				ChangeNavigationState(new NavigationState_WanderRandom(art, 0));
			}

			public override void Update()
			{
				base.Update();
				if (inkCooldown > 0f)
					inkCooldown -= art.TimeScale * Time.deltaTime;
			}

			public override void OnStateTriggerEnter(Collider other)
			{
				base.OnStateTriggerEnter(other);
				if (inkCooldown <= 0f && other.isTrigger)
				{
					var isPlayer = other.CompareTag("Player");
					if (isPlayer || other.CompareTag("NPC"))
					{

						if (isPlayer)
						{
							var pm = other.GetComponent<PlayerManager>();
							if (pm) // Only affect Players that haven't seen the Ink Artist to be an actual bump effect
							{
								art.InkPlayer(pm);
								inkCooldown += art.maxInkCooldown;
							}
							return;
						}

						var e = other.GetComponent<NPC>();
						if (e)
						{
							art.InkEntity(e);
							inkCooldown += art.maxInkCooldown * 0.4f;
						}

					}
				}
			}
		}
	}
}

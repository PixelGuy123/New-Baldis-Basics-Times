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
	public class InkArtist : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(25f, "Ink_Artist.png");
			audMan = GetComponent<PropagatedAudioManager>();
			audSplash = this.GetSound("splash.wav", "Vfx_InkArt_Splash", SoundType.Effect, Color.white);
			audBlob = this.GetSoundNoSub("blob.wav", SoundType.Effect);

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
		internal float maxInkCooldown = 20f, minRngBlobSize = 0.15f, maxRngBlobSize = 1.15f;
		[SerializeField]
		internal int minInkShakeRemovals = 3, maxInkShakeRemovals = 6, minBlobsPerShake = 2, maxBlobsPerShake = 5;
		[SerializeField]
		internal float shakeSensitivity = 35f, shakeToleranceVariation = 15f; // degrees per second
		[SerializeField]
		internal float shakeCooldown = 0.25f; // seconds between shakes
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

			uiInkCooldown = StartCoroutine(InkCamera(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber)));
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

			if (ent == null)
				return;

			var att = Instantiate(attPre);
			att.AttachTo(ent.transform, true);
			att.SetOwnerRefToSelfDestruct(gameObject); // InkArtist itself, since its despawn removes the effect entirely

			att.StartCoroutine(InkNPC(att.gameObject, ent));
		}

		public void CancelCameraInk()
		{
			stunCanvas.gameObject.SetActive(false);
			if (uiInkCooldown != null)
				StopCoroutine(uiInkCooldown);
			affectedPlayers.RemoveAll(x => x.Value == this);
			gauge?.Deactivate();
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

		IEnumerator InkCamera(GameCamera target)
		{
			stunCanvas.gameObject.SetActive(true);
			stunCanvas.worldCamera = target.canvasCam;
			Vector3 ogSize = Vector3.one;
			Vector3 zero = Vector3.zero;

			float fadeTime = maxInkCooldown * fadeEffectFactor; // x% is used for fade in/out effect
			float displayTime = maxInkCooldown * (1f - fadeEffectFactor * 2f); // The rest of the cooldown is display on screen
			float totalTime = fadeTime * 2f + displayTime, runTime = totalTime;
			float t = 0f;
			float shakeSensitivity = this.shakeSensitivity;

			image.transform.localScale = Vector3.zero;

			// --- Shake mechanic state ---
			int shakesLeft = Random.Range(minInkShakeRemovals, maxInkShakeRemovals + 1);
			//Quaternion lastRot = target.transform.rotation;
			float lastYaw = target.transform.eulerAngles.y;
			float shakeTimer = 0f;

			// Fade-in
			t = 0f;
			while (t < fadeTime)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime;
				runTime -= ec.EnvironmentTimeScale * Time.deltaTime;
				image.transform.localScale = Vector3.Lerp(zero, ogSize, t / fadeTime);
				gauge.SetValue(totalTime, runTime);
				yield return null;
			}
			image.transform.localScale = ogSize;

			// Full display with shake detection
			t = 0f;
			while (t < displayTime)
			{
				if (Time.timeScale == 0)
				{
					yield return null;
					continue;
				}

				// --- Shake detection ---
				shakeTimer -= ec.EnvironmentTimeScale * Time.deltaTime;

				float currentYaw = target.transform.eulerAngles.y;
				float angleDelta = Mathf.DeltaAngle(lastYaw, currentYaw);
				// Debug.Log("Current angle delta: " + angleDelta + " || Pure Angle Delta: " + Mathf.DeltaAngle(lastYaw, currentYaw));

				lastYaw = currentYaw;

				if (shakesLeft > 0 && shakeTimer <= 0f && Mathf.Abs(angleDelta) > shakeSensitivity)
				{
					shakesLeft--;
					shakeSensitivity = this.shakeSensitivity + (1 - Random.Range(0, 3)) * shakeToleranceVariation;
					shakeTimer = shakeCooldown;
					// Shrink ink image with bouncy effect
					image.transform.localScale = Vector3.one * (1f - 1f / (shakesLeft + 1));

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
				}

				t += ec.EnvironmentTimeScale * Time.deltaTime;
				runTime -= ec.EnvironmentTimeScale * Time.deltaTime;
				// Update gauge value
				gauge.SetValue(totalTime, runTime);
				yield return null;
			}

			// Fade-out
			ogSize = image.transform.localScale;
			t = 0f;
			while (t < fadeTime)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime;
				runTime -= ec.EnvironmentTimeScale * Time.deltaTime;
				image.transform.localScale = Vector3.Lerp(ogSize, zero, t / fadeTime);
				gauge.SetValue(totalTime, runTime);
				yield return null;
			}
			image.transform.localScale = zero;
			stunCanvas.gameObject.SetActive(false);

		finish:

			gauge.Deactivate();

			affectedPlayers.RemoveAll(x => x.Value == this);

			yield break;
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
			if (!cont) yield break;

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
			affectedNpcs.RemoveAll(x => x.Key == cont); // If container is null, it also rmemoves null containers from the list

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

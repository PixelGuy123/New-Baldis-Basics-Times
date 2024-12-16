using BBTimes.Extensions;
using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Components;
using System.Collections.Generic;

namespace BBTimes.CustomContent.NPCs
{
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
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }


		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSplash;

		[SerializeField]
		internal Canvas stunCanvas;

		[SerializeField]
		internal Image image;

		[SerializeField]
		internal VisualAttacher attPre;

		[SerializeField]
		internal float maxInkCooldown = 12f;

		Coroutine uiInkCooldown;
		readonly List<KeyValuePair<NPCAttributesContainer ,ValueModifier>> affectedNpcs = [];

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

			if (uiInkCooldown != null)
				StopCoroutine(uiInkCooldown);

			uiInkCooldown = StartCoroutine(InkCamera(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam));
		}

		public void InkEntity(NPC ent)
		{
			audMan.PlaySingle(audSplash);

			var att = Instantiate(attPre);
			att.AttachTo(ent.transform, true);
			att.SetOwnerRefToSelfDestruct(gameObject);

			att.StartCoroutine(InkNPC(att.gameObject, ent));
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

			cont.RemoveLookerMod(valMod);
			affectedNpcs.RemoveAll(x => x.Key == cont);

			if (selfDestruct)
				Destroy(selfDestruct);
			yield break;

		}

		public override void Despawn()
		{
			base.Despawn();
			while (affectedNpcs.Count != 0)
			{
				affectedNpcs[0].Key?.RemoveLookerMod(affectedNpcs[0].Value);
				affectedNpcs.RemoveAt(0);
			}
		}

		IEnumerator InkCamera(Camera target)
		{
			stunCanvas.gameObject.SetActive(true);
			stunCanvas.worldCamera = target;
			Vector3 ogSize = Vector3.one;
			Vector3 zero = Vector3.zero;

			float t = 0;
			image.transform.localScale = Vector3.zero;
			while (true)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * 12f;
				
				if (t >= 1f)
				{
					image.transform.localScale = ogSize;
					break;
				}
				image.transform.localScale = Vector3.Lerp(zero, ogSize, t);

				yield return null;
			}

			float delay = maxInkCooldown * 0.95f;
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			t = 0;
			while (true)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * 12f;
				if (t >= 1)
				{
					stunCanvas.gameObject.SetActive(false);
					break;
				}
				image.transform.localScale = Vector3.Lerp(ogSize, zero, t);
				yield return null;
			}

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

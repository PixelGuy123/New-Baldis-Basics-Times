using UnityEngine;
using PixelInternalAPI.Extensions;
using BBTimes.Extensions;
using BBTimes.CustomComponents;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_StormInABag : Item, IItemPrefab
	{
		public void SetupPrefab()
		{

			var bagRenderer = ObjectCreationExtensions.CreateSpriteBillboard(ItmObj.itemSpriteLarge);
			bagRenderer.transform.SetParent(transform);
			bagRenderer.transform.localPosition = Vector3.down * 4.37f;
			bagRenderer.name = "Bag";
			var mainContainer = bagRenderer.GetComponent<RendererContainer>();

			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "thunder.png"));
			mainContainer.renderers = mainContainer.renderers.AddToArray(renderer);
			renderer.name = "Storm";
			renderer.gameObject.SetActive(false);
			storm = renderer.transform;
			storm.SetParent(bagRenderer.transform);
			storm.transform.localPosition = Vector3.zero;
			storm.transform.localScale = Vector3.zero;

			audMan = gameObject.CreatePropagatedAudioManager(145f, 165f);
			audAttack = new SoundObject[3];
			for (int i = 0; i < audAttack.Length; i++)
				audAttack[i] = this.GetSoundNoSub($"shoot{i+1}.wav", SoundType.Effect);

			lightningPre = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(11.35f, "visualThunder.png")).GetComponent<RendererContainer>();
			lightningPre.gameObject.ConvertToPrefab(true);
			lightningPre.name = "StormInABag_Lightning";

			lightningPre.gameObject.CreatePropagatedAudioManager(45f, 75f)
				.AddStartingAudiosToAudioManager(false, this.GetSoundNoSub("electrocute.wav", SoundType.Voice));

			entity = gameObject.CreateEntity(2f, 65f, renderer.transform);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			pm.RuleBreak("littering", 2f, 0.8f);
			PlaceDown(pm.transform.position, pm.ec);

			return true;
		}

		public void PlaceDown(Vector3 pos, EnvironmentController ec, float delayBeforeActive = 1.5f)
		{
			if (initialized)
				return;

			this.ec = ec;
			entity.Initialize(ec, pos);
			
			cooldownForActivation = delayBeforeActive;
			stormingCooldown = Random.Range(minCooldownForStorming, maxCooldownForStorming);
			looker = new(transform);
			initialized = true;
			lifeTimeCooldown = lifeTime;
		}

		void Update()
		{
			if (!initialized || dead)
				return;

			if (!_stormActive) 
			{
				cooldownForActivation -= ec.EnvironmentTimeScale * Time.deltaTime;
				if (cooldownForActivation <= 0f)
				{
					_stormActive = true;
					storm.gameObject.SetActive(true);
					StartCoroutine(SpawnStorm());
				}
				return;
			}

			for (int i = 0; i < ec.Npcs.Count; i++) // Search for npcs
			{
				if (ec.Npcs[i].Navigator.isActiveAndEnabled && looker.Raycast(ec.Npcs[i].transform, minDistanceForHitting))
				{
					if (!targets.Contains(ec.Npcs[i].Navigator.Entity))
						targets.Add(ec.Npcs[i].Navigator.Entity);
				}
				else
					targets.Remove(ec.Npcs[i].Navigator.Entity);
			}

			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++) // Search for players
			{
				if (looker.Raycast(ec.Players[i].transform, minDistanceForHitting))
				{
					if (!targets.Contains(ec.Players[i].plm.Entity))
						targets.Add(ec.Players[i].plm.Entity);
				}
				else
					targets.Remove(ec.Players[i].plm.Entity);
			}

			stormingCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (stormingCooldown <= 0f)
			{
				stormingCooldown += Random.Range(minCooldownForStorming, maxCooldownForStorming);

				if (targets.Count != 0)
					audMan.PlayRandomAudio(audAttack);

				foreach (var ent in targets)
                {
					ent.AddForce(new((ent.transform.position - transform.position).normalized, maxForce, -maxForce * 0.8f));

					var lig = Instantiate(lightningPre);
					lig.transform.position = ent.transform.position;
					lig.StartCoroutine(((SpriteRenderer)lig.renderers[0]).FadeOutLightning(ec, 0.5f, 2f));
				}
            }

			lifeTimeCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTimeCooldown <= 0f)
			{
				dead = true;
				StartCoroutine(DespawnSequence());
			}
		}

		IEnumerator SpawnStorm()
		{
			float t = 0f;
			var verticalOffset = Vector3.up * 8f;

			while (true)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * 6f;
				t = Mathf.Clamp01(t);
				storm.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, t);
				storm.localPosition = Vector3.Slerp(Vector3.zero, verticalOffset, t);
				if (t == 1f)
				{
					yield break;
				}
				
				yield return null;
			}
		}

		IEnumerator DespawnSequence()
		{
			float t = 0f;
			Vector3 pos = storm.localPosition;

			while (true)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * 6f;
				t = Mathf.Clamp01(t);
				storm.localScale = Vector3.Slerp(Vector3.one, Vector3.zero, t);
				storm.localPosition = Vector3.Slerp(pos, Vector3.zero, t);
				if (t == 1f)
				{
					storm.gameObject.SetActive(false);
					break;
				}

				yield return null;
			}

			float timer = 5f;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Destroy(gameObject);
		}

		bool _stormActive = false, initialized = false, dead = false;
		float cooldownForActivation = 0f, stormingCooldown, lifeTimeCooldown;
		EnvironmentController ec;
		BasicLookerInstance looker;
		readonly HashSet<Entity> targets = [];

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject[] audAttack;

		[SerializeField]
		internal Transform storm;

		[SerializeField]
		internal RendererContainer lightningPre;

		[SerializeField]
		internal float maxForce = 40f, minDistanceForHitting = 60f, minCooldownForStorming = 0.15f, maxCooldownForStorming = 0.8f, lifeTime = 30f;
		
	}
}

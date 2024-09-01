using UnityEngine;
using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using PixelInternalAPI.Extensions;
using BBTimes.Extensions;
using BBTimes.Manager;
using System.Collections.Generic;
using MTM101BaldAPI;

namespace BBTimes.CustomContent.Events
{
	public class LightningEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("Lightning.wav", "Event_ShuffleChaos1", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [
				new() {time = 0.953f, key = "Event_Lightning1"},
				new() {time = 4.332f, key = "Event_Lightning2"}
				];

			audMan = gameObject.CreateAudioManager(85, 105)
				.MakeAudioManagerNonPositional();
			audThunderstorm = this.GetSoundNoSub("thunderLoop.wav", SoundType.Music);
			audThunder = this.GetSoundNoSub("thunder.wav", SoundType.Effect);
			eletricityPre = BBTimesManager.man.Get<Eletricity>("EletricityPrefab");

			lightningPre = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(20f, "lightning.png"));
			lightningPre.gameObject.ConvertToPrefab(true);
			lightningPre.name = "Lightning";
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------
		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup();
			spots.AddRange(ec.AllCells());
		}
		public override void Begin()
		{
			base.Begin();

			StartCoroutine(FadeOnFog());
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audThunderstorm);
			ec.MaxRaycast = 40f;

			
		}

		public override void End()
		{
			base.End();
			ec.MaxRaycast = float.PositiveInfinity;
			StartCoroutine(FadeOffFog());
			audMan.FadeOut(4f);

			KillAllEletricities(0);
		}

		void KillAllEletricities(int amount)
		{
			int num = 0;
			while (num <= amount && spawnedEletricities.Count != 0)
			{
				Destroy(spawnedEletricities[0].gameObject);
				spawnedEletricities.RemoveAt(0);
				if (amount > 0)
					num++;
			}
		}

		void SpawnEletricity(Cell pos, List<Cell> cells)
		{
			for (int x = -lightningRange; x <= lightningRange; x++)
			{
				for (int z = -lightningRange; z <= lightningRange; z++)
				{
					var cell = ec.CellFromPosition(pos.position.x + x, pos.position.z + z);
					if (!cell.Null && cell.TileMatches(pos.room))
					{
						var el = Instantiate(eletricityPre);
						el.Initialize(gameObject, cell.FloorWorldPosition, 0.4f, ec);
						spawnedEletricities.Add(el);
						cells.Remove(cell);

						var lig = Instantiate(lightningPre);
						lig.transform.position = pos.CenterWorldPosition;
						StartCoroutine(FadeOutLightning(lig));
					}
				}
			}
		}

		void Update()
		{
			if (!active) return;

			thunderCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (thunderCooldown <= 0f)
			{
				if (thunderAnimation != null)
					StopCoroutine(thunderAnimation);
				thunderAnimation = StartCoroutine(ThrowThunderFog());
				thunderCooldown += Random.Range(minLightningDelay, maxLightningDelay);
				audMan.PlaySingle(audThunder);

				KillAllEletricities(eletricityAmount * lightningRange / 2);

				List<Cell> cells = new(spots);
				for (int i = 0; i < eletricityAmount; i++)
				{
					if (cells.Count == 0) return;

					SpawnEletricity(cells[Random.Range(0, cells.Count)], cells);
				}
			}
		}

		IEnumerator FadeOnFog()
		{
			ec.AddFog(fog);
			fog.color = new(0.35f, 0.35f, 0.35f);
			fog.startDist = 4f;
			fog.maxDist = 55f;
			fog.strength = 0f;
			float strength = 0f;
			while (strength < 0.7f)
			{
				strength += 0.45f * Time.deltaTime * ec.EnvironmentTimeScale;
				fog.strength = strength;
				ec.UpdateFog();
				yield return null;
			}

			fog.strength = 0.7f;
			ec.UpdateFog();

			yield break;
		}

		IEnumerator FadeOffFog()
		{
			float strength = fog.strength;
			while (strength > 0f)
			{
				strength -= 0.45f * Time.deltaTime * ec.EnvironmentTimeScale;
				fog.strength = strength;
				ec.UpdateFog();
				yield return null;
			}

			fog.strength = 0f;
			ec.RemoveFog(fog);

			yield break;
		}

		IEnumerator ThrowThunderFog()
		{
			float yellowOffset = 1f;
			float speed = 0f;
			while (true)
			{
				speed -= 0.25f * ec.EnvironmentTimeScale * Time.deltaTime;
				yellowOffset += speed;
				if (yellowOffset <= 0.35f)
				{
					yellowOffset = 0.35f;
					fog.color = new(yellowOffset, yellowOffset, 0.35f);
					ec.UpdateFog();
					break;
				}
				fog.color = new(yellowOffset, yellowOffset, 0.35f);
				ec.UpdateFog();
				yield return null;
			}

			yellowOffset = 0.65f;
			speed = 0f;
			while (true)
			{
				speed -= 0.25f * ec.EnvironmentTimeScale * Time.deltaTime;
				yellowOffset += speed;
				if (yellowOffset <= 0.35f)
				{
					yellowOffset = 0.35f;
					fog.color = new(yellowOffset, yellowOffset, 0.35f);
					ec.UpdateFog();
					yield break;
				}
				fog.color = new(yellowOffset, yellowOffset, 0.35f);
				ec.UpdateFog();
				yield return null;
			}
		}

		IEnumerator FadeOutLightning(SpriteRenderer renderer)
		{
			float delay = 0.5f;
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			Color color = renderer.color;
			while (true)
			{
				color.a -= ec.EnvironmentTimeScale * Time.deltaTime * 2f;
				if (color.a <= 0f)
				{
					Destroy(renderer.gameObject);
					yield break;
				}
				renderer.color = color;

				yield return null;
			}
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audThunderstorm, audThunder;

		[SerializeField]
		internal SpriteRenderer lightningPre;

		[SerializeField]
		internal Eletricity eletricityPre;

		[SerializeField]
		internal int lightningRange = 2, eletricityAmount = 8;

		[SerializeField]
		internal float minLightningDelay = 5f, maxLightningDelay = 15f;

		readonly Fog fog = new();
		Coroutine thunderAnimation;
		float thunderCooldown = 10f;
		readonly List<Cell> spots = [];
		readonly List<Eletricity> spawnedEletricities = [];
	}
}

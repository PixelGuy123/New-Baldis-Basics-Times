using System.Collections.Generic;
using UnityEngine;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using BBTimes.Manager;
using PixelInternalAPI.Components;


namespace BBTimes.CustomContent.Events
{
    public class NatureEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("NatureEvent.wav", "Event_NatureEvent_1", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [
				new() {time = 1.957f, key = "Event_NatureEvent_2"},
				new() {time = 4.907f, key = "Event_NatureEvent_3"},
				new() {time = 7.026f, key = "Event_NatureEvent_4"},
				new() {time = 9.782f, key = "Event_NatureEvent_5"}
				];

			List<WeightedSelection<Plant>> flowers = [];

			var sprites = this.GetSpriteSheet(2, 2, plantsPixPerUnit, "timesFlowersPack.png");

			// Normal flowers
			var speedChanging = CreatePlant<SpeedChangingFlower>(sprites[0], 75);
			speedChanging.audAffect = this.GetSoundNoSub("BluePlantSpeedUp.wav", SoundType.Voice);

			CreatePlant<PurpleFlower>(sprites[1], 110).audPush = this.GetSound("PurplePlantBang.wav", "Vfx_Prize_Bang", SoundType.Voice, Color.white);
			CreatePlant<MysteryFlower>(sprites[2], 75).audTeleport = BBTimesManager.man.Get<SoundObject>("teleportAud");

			speedChanging = CreatePlant<SpeedChangingFlower>(sprites[3], 100);
			speedChanging.audAffect = this.GetSoundNoSub("RedPlantSlowDown.wav", SoundType.Voice);
			speedChanging.moveMultiplier = 0.75f;

			sprites = this.GetSpriteSheet(3, 1, plantsPixPerUnit, "YTPFlowers.png");

			var ytpFlower = CreatePlant<YTPFlower>(sprites[0], 50);
			ytpFlower.value = 15;
			ytpFlower.audPickup = BBTimesManager.man.Get<SoundObject>("tierOnePickup");

			ytpFlower = CreatePlant<YTPFlower>(sprites[1], 25);
			ytpFlower.value = 45;
			ytpFlower.audPickup = BBTimesManager.man.Get<SoundObject>("tierTwoPickup");

			ytpFlower = CreatePlant<YTPFlower>(sprites[2], 5);
			ytpFlower.value = 75;
			ytpFlower.audPickup = BBTimesManager.man.Get<SoundObject>("tierThreePickup");

			CreatePlant<Vines>(this.GetSprite(plantsPixPerUnit, "Vines.png"), 65).audCatch = this.GetSoundNoSub("vines.wav", SoundType.Voice);
			sprites = this.GetSpriteSheet(2, 1, plantsPixPerUnit, "carnivorousPlants.png");

			var catchPlant = CreatePlant<TrapPlant>(sprites[0], 50);
			catchPlant.sprCatch = sprites[1];
			catchPlant.audCatch = this.GetSoundNoSub("plantTrapCatch.wav", SoundType.Voice);
			

			flowerPres = [..flowers];

			var grassPreRenderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(plantsPixPerUnit, "Grass.png")).AddSpriteHolder(0f);
			grassPreRenderer.transform.localPosition = Vector3.forward * 0.1f;
			grassPreRenderer.name = "GrassSprite";

			grassPre = grassPreRenderer.transform.parent;
			grassPre.name = "Grass";
			grassPre.gameObject.ConvertToPrefab(true);

			grassPre.gameObject.AddComponent<BillboardRotator>().invertFace = true;

			T CreatePlant<T>(Sprite sprite, int weight) where T : Plant
			{
				var flowerRend = ObjectCreationExtensions.CreateSpriteBillboard(sprite).AddSpriteHolder(0f, 0);
				var flower = flowerRend.transform.parent.gameObject.AddComponent<T>();

				flower.gameObject.ConvertToPrefab(true);

				flower.name = "Flower_" + sprite.name;
				flowerRend.name = flower.name + "_Renderer";

				flower.audMan = flower.gameObject.CreatePropagatedAudioManager(30f, 50f);
				flower.audSpawn = this.GetSound("plantGrow.wav", "Vfx_Plant_Grow", SoundType.Voice, Color.white);
				flower.audDespawn = this.GetSound("plantDegrow.wav", "Vfx_Plant_DeGrow", SoundType.Voice, Color.white);

				flower.collider = flower.gameObject.AddBoxCollider(Vector3.up * 5f, Vector3.one * 9f, true);

				flower.renderer = flowerRend;

				flower.PrefabSetup(this);

				flowers.Add(new() { selection = flower, weight = weight });

				return flower;
			}
		}
		const float plantsPixPerUnit = 15f;
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------
		public override void Begin()
		{
			base.Begin();
			spots.ForEach(SpawnRandomFlower);
		}

		public override void End()
		{
			base.End();
			while (flowers.Count != 0)
				flowers[0].Despawn(false);
		}

		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup();
			spots = ec.mainHall.GetTilesOfShape([TileShape.Corner, TileShape.Single], CellCoverage.Down, false);
			spots.ForEach(cell =>
			{
				var grass = Instantiate(grassPre, cell.TileTransform);
				grass.transform.localPosition = Vector3.up * defaultYOffsetForFlowers;
				cell.HardCover(CellCoverage.Down);
			});
		}

		public void SpawnRandomFlower(Cell cell)
		{
			var newFlower = Instantiate(WeightedSelection<Plant>.RandomSelection(flowerPres), cell.TileTransform);
			newFlower.transform.localPosition = Vector3.up * defaultYOffsetForFlowers;
			newFlower.renderer.transform.localPosition = Vector3.down * 6f;
			newFlower.Initialize(this, ec, cell, newFlower.renderer.transform.position + Vector3.up * 6);
			flowers.Add(newFlower);
		}

		public void RemoveFlower(Plant flower) =>
			flowers.Remove(flower);

		[SerializeField]
		WeightedSelection<Plant>[] flowerPres;

		[SerializeField]
		internal Transform grassPre;

		[SerializeField]
		internal float defaultYOffsetForFlowers = 3.15f;

		readonly List<Plant> flowers = [];
		List<Cell> spots;


	}
}

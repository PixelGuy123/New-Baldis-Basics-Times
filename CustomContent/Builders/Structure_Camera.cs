using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using HarmonyLib;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class Structure_Camera : StructureBuilder, IBuilderPrefab
	{

		public StructureWithParameters SetupBuilderPrefabs()
		{
			var cam = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "SecurityCamera.png")).AddSpriteHolder(out var renderer, 9f, 0);
			renderer.name = "Sprite";
			cam.name = "SecurityCamera";
			cam.gameObject.ConvertToPrefab(true);

			var camComp = cam.gameObject.AddComponent<SecurityCamera>();
			camComp.collider = cam.gameObject.AddBoxCollider(new(0f, 1f, 5f), new(3f, 10f, 3f), true);

			var visionIndicator = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(15f, "tiledGrid.png"), false);
			visionIndicator.gameObject.layer = 0;
			visionIndicator.material.SetTexture("_LightMap", null); // No light affected, it's always bright

			visionIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			visionIndicator.transform.localScale = new(1f, 1.172f, 1f);
			visionIndicator.name = "CameraVisionIndicator";
			visionIndicator.gameObject.ConvertToPrefab(true);

			camComp.visionIndicatorPre = visionIndicator;

			camComp.audMan = cam.gameObject.CreatePropagatedAudioManager(55f, 90f);
			camComp.audAlarm = this.GetSound("alarm.wav", "Vfx_Camera_Alarm", SoundType.Voice, Color.white);
			camComp.audTurn = this.GetSound("camSwitch.wav", "Vfx_Camera_Switch", SoundType.Voice, Color.white);
			camComp.audDetect = this.GetSound("spot.wav", "Vfx_Camera_Spot", SoundType.Voice, Color.white);

			camPre = cam.transform;

			return new() { prefab = this, parameters = new() { minMax = [new(1, 1), new(5,10)] } }; // 0 = Amount of cameras, 1 = minMax distance for them
		}
		public void SetupPrefab() { }
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");


		// Prefab stuff above ^^
		public override void PostOpenCalcGenerate(LevelGenerator lg, System.Random rng)
		{
			base.PostOpenCalcGenerate(lg, rng);

			var room = lg.Ec.mainHall;
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			var spots = room.GetTilesOfShape(TileShapeMask.Corner | TileShapeMask.Single, false);
			for (int i = 0; i < spots.Count; i++)
				if (!spots[i].HardCoverageFits(CellCoverage.Up))
					spots.RemoveAt(i--);

			if (spots.Count == 0)
			{
				Debug.LogWarning("CameraBuilder has failed to find a good spot for the Security Camera.");
				return;
			}

			int amount = rng.Next(parameters.minMax[0].x, parameters.minMax[0].z + 1);

			for (int i = 0; i < amount; i++)
			{
				if (spots.Count == 0)
					return;

				int s = rng.Next(spots.Count);
				var cam = Instantiate(camPre, spots[s].ObjectBase).GetComponentInChildren<SecurityCamera>();
				cam.Ec = ec;
				cam.GetComponentsInChildren<SpriteRenderer>().Do(spots[s].AddRenderer);
				cam.Setup(spots[s].AllOpenNavDirections, rng.Next(parameters.minMax[1].x, parameters.minMax[1].z + 1));
				ecData.Cameras.Add(cam);

				spots[s].HardCover(CellCoverage.Up);
				spots.RemoveAt(s);
			}

			Finished();
		}
		public override void Load(List<StructureData> data)
		{
			base.Load(data);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			for (int i = 0; i < data.Count; i++)
			{
				var spot = ec.CellFromPosition(data[i].position);
				var cam = Instantiate(camPre, spot.ObjectBase).GetComponentInChildren<SecurityCamera>();
				cam.Ec = ec;
				cam.GetComponentsInChildren<Renderer>().Do(spot.AddRenderer);
				cam.Setup(spot.AllOpenNavDirections, data[i].data);
				ecData.Cameras.Add(cam);

				spot.HardCover(CellCoverage.Up);
			}

			Finished();
		}

		[SerializeField]
		internal Transform camPre;
	}
}

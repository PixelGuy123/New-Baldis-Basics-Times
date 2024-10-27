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
	public class CameraBuilder : ObjectBuilder, IObjectPrefab
	{

		public void SetupPrefab()
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

			GetComponent<CameraBuilder>().camPre = cam.transform;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");


		// Prefab stuff above ^^
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			var spots = room.GetTilesOfShape([TileShape.Corner, TileShape.Single], false);
			for (int i = 0; i < spots.Count; i++)
				if (!spots[i].HardCoverageFits(CellCoverage.Up))
					spots.RemoveAt(i--);

			if (spots.Count == 0)
			{
				Debug.LogWarning("CameraBuilder has failed to find a good spot for the Security Camera.");
				return;
			}

			int s = cRng.Next(spots.Count);
			var cam = Instantiate(camPre, spots[s].ObjectBase).GetComponentInChildren<SecurityCamera>();
			cam.Ec = ec;
			cam.GetComponentsInChildren<SpriteRenderer>().Do(spots[s].AddRenderer);
			cam.Setup(spots[s].AllOpenNavDirections, cRng.Next(7, 14));
			ecData.Cameras.Add(cam);

			spots[s].HardCover(CellCoverage.Up);


		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			base.Load(ec, pos, dir);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			for (int i = 0; i < pos.Count; i++)
			{
				var spot = ec.CellFromPosition(pos[i]);
				var cam = Instantiate(camPre, spot.ObjectBase).GetComponentInChildren<SecurityCamera>();
				cam.Ec = ec;
				cam.GetComponentsInChildren<Renderer>().Do(spot.AddRenderer);
				cam.Setup(spot.AllOpenNavDirections, (int)dir[i]);
				ecData.Cameras.Add(cam);

				spot.HardCover(CellCoverage.Up);
			}
		}

		[SerializeField]
		internal Transform camPre;
	}
}

using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.Builders
{
    public class SquisherBuilder : ObjectBuilder, IObjectPrefab
	{
		public void SetupPrefab()
		{
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
			squishBase.audHit = this.GetSound("squishHit.wav", "Sfx_Doors_StandardShut", SoundType.Voice, Color.white);
			squishBase.audRun = this.GetSound("squishSquishing.wav", "Vfx_Squish_Running", SoundType.Voice, Color.white);
			squishBase.audPrepare = this.GetSound("squisherPrepare.wav", "Vfx_Squish_Running", SoundType.Voice, Color.white);

			// Head
			var squishHead = ObjectCreationExtension.CreateCube(this.GetTexture("squisherHead.png"));
			Destroy(squishHead.GetComponent<BoxCollider>());

			squishHead.transform.SetParent(squishBase.transform);
			squishHead.transform.localPosition = new(-headSize / 2, 0f, -headSize / 2);
			squishHead.transform.localScale = new(headSize, 1f, headSize);

			// Body
			var squishBody = ObjectCreationExtension.CreateCube(this.GetTexture("sides.png"), false);
			Destroy(squishBody.GetComponent<BoxCollider>());
			squishBody.transform.SetParent(squishBase.transform);
			squishBody.transform.localPosition = Vector3.up * ((bodyHeight / 2) + 1);
			squishBody.transform.localScale = new(headSize / 2, bodyHeight, headSize / 2);

			var builder = GetComponent<SquisherBuilder>();
			builder.squisherPre = squishBase;
			builder.buttonPre = BBTimesManager.man.Get<GameButton>("buttonPre");
		}

		public void SetupPrefabPost() { }

		const float headSize = 6f, bodyHeight = 9f;

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");


		// prefab stuff ^^

		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			var spots = room.GetTilesOfShape([TileShape.Corner, TileShape.Single, TileShape.Straight], false);
			for (int i = 0; i < spots.Count; i++)
				if (!spots[i].HardCoverageFits(CellCoverage.Up))
					spots.RemoveAt(i--);

			if (spots.Count == 0)
			{
				Debug.LogWarning("SquisherBuilder has failed to find a good spot for the Squishers.");
				return;
			}
			int am = cRng.Next(minAmount, maxAmount + 1);
			for (int i = 0; i < am; i++)
			{
				if (spots.Count == 0)
					break;

				int idx = cRng.Next(spots.Count);

				var squ = Instantiate(squisherPre, spots[idx].ObjectBase);
				squ.transform.localPosition = Vector3.up * 8.5f;
				squ.Ec = ec;
				squ.Setup(cRng.Next(5, 9));
				squ.GetComponentsInChildren<Renderer>().Do(spots[idx].AddRenderer);
				ecData.Squishers.Add(squ);
				if (cRng.NextDouble() > 0.7f)
					GameButton.BuildInArea(ec, spots[idx].position, spots[idx].position, cRng.Next(4, 7), squ.gameObject, buttonPre, cRng);
				spots[idx].HardCover(CellCoverage.Up);
				spots.RemoveAt(idx);
			}
		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			base.Load(ec, pos, dir);
			for (int i = 0; i < pos.Count; i++)
			{
				var cell = ec.CellFromPosition(pos[i]);
				var squ = Instantiate(squisherPre, cell.ObjectBase);
				squ.transform.localPosition = Vector3.up * 9f;
				squ.Ec = ec;
				squ.Setup((int)dir[i] + 1);
				squ.GetComponentsInChildren<Renderer>().Do(cell.AddRenderer);
				ecData.Squishers.Add(squ);
			}
		}

		[SerializeField]
		internal int minAmount = 2, maxAmount = 4;

		[SerializeField]
		internal Squisher squisherPre;

		[SerializeField]
		internal GameButton buttonPre;
	}
}

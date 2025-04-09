using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class Structure_Squisher : StructureBuilder, IBuilderPrefab
	{
		public StructureWithParameters SetupBuilderPrefabs()
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
			squishBase.audHit = this.GetSound("squishHit.wav", "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
			squishBase.audRun = this.GetSound("squishSquishing.wav", "Vfx_Squish_Running", SoundType.Effect, Color.white);
			squishBase.audPrepare = this.GetSound("squisherPrepare.wav", "Vfx_Squish_Running", SoundType.Effect, Color.white);

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

			var builder = GetComponent<Structure_Squisher>();
			builder.squisherPre = squishBase;
			builder.buttonPre = BBTimesManager.man.Get<GameButton>("buttonPre");

			return new() { prefab = this, parameters = new() { chance = [0.35f], minMax = [new(1, 1), new(5, 9), new(4, 7)] } }; 
			// Chance = chanceForButtons
			// minMax = SquisherAmount, squisherSpeed, button range
		}

		public void SetupPrefab() { }
		public void SetupPrefabPost() { }

		const float headSize = 6f, bodyHeight = 9f;

		public string Name { get; set; }
		public string Category => "objects";



		// prefab stuff ^^
		public override void PostOpenCalcGenerate(LevelGenerator lg, System.Random rng)
		{
			base.PostOpenCalcGenerate(lg, rng);

			var room = lg.Ec.mainHall;
			var spots = room.GetTilesOfShape(TileShapeMask.Corner | TileShapeMask.Single | TileShapeMask.Straight, false);
			for (int i = 0; i < spots.Count; i++)
				if (!spots[i].HardCoverageFits(CellCoverage.Up))
					spots.RemoveAt(i--);

			if (spots.Count == 0)
			{
				Debug.LogWarning("SquisherBuilder has failed to find a good spot for the Squishers.");
				Finished();
				return;
			}

			int amount = rng.Next(parameters.minMax[0].x, parameters.minMax[0].z + 1);

			for (int i = 0; i < amount; i++)
			{
				if (spots.Count == 0)
					break;

				int idx = rng.Next(spots.Count);

				var squ = Instantiate(squisherPre, spots[idx].ObjectBase);
				squ.transform.localPosition = Vector3.up * 8.5f;
				squ.Ec = ec;
				squ.Setup(rng.Next(parameters.minMax[1].x, parameters.minMax[1].z + 1));

				squ.GetComponentsInChildren<Renderer>().Do(spots[idx].AddRenderer);

				if (rng.NextDouble() <= parameters.chance[0])
					GameButton.BuildInArea(ec, spots[idx].position, spots[idx].position, rng.Next(parameters.minMax[2].x, parameters.minMax[2].z + 1), squ.gameObject, buttonPre, rng);

				spots[idx].HardCover(CellCoverage.Up);
				spots.RemoveAt(idx);
			}

			Finished();
		}

		public override void Load(List<StructureData> data)
		{
			base.Load(data);

			Squisher lastBuiltSquisher = null;

			for (int i = 0; i < data.Count; i++)
			{
				if (lastBuiltSquisher && data[i].data == -1) // If there's a last built squisher and id == -1 (button), then link a button to it
				{
					GameButton.Build(buttonPre, ec, data[i].position, data[i].direction).SetUp(lastBuiltSquisher);
					lastBuiltSquisher = null;
					continue;
				}

				var cell = ec.CellFromPosition(data[i].position);
				var squ = Instantiate(squisherPre, cell.ObjectBase);
				squ.transform.localPosition = Vector3.up * 9f;
				squ.Ec = ec;
				squ.Setup(data[i].data);
				squ.GetComponentsInChildren<Renderer>().Do(cell.AddRenderer);

				lastBuiltSquisher = squ;
			}

			Finished();
		}
		

		[SerializeField]
		internal Squisher squisherPre;

		[SerializeField]
		internal GameButton buttonPre;

	}
}

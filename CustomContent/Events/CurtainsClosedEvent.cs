
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using Rewired.UI.ControlMapper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class CurtainsClosedEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{

			eventIntro = this.GetSound("baldi_curtains.wav", "Event_CurtClosed0", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [new() { time = 3.287f, key = "Event_CurtClosed1" }, new() { time = 8.457f, key = "Event_CurtClosed2" }, new() { time = 16.682f, key = "Event_CurtClosed3" }];

			var curt = new GameObject("Curtain").AddComponent<Curtains>();
			curt.gameObject.ConvertToPrefab(true);
			curt.audClose = this.GetSound("curtainClose.wav", "Vfx_Curtain_Slide", SoundType.Voice, Color.white);
			curt.audOpen = this.GetSound("curtainOpen.wav", "Vfx_Curtain_Slide", SoundType.Voice, Color.white);

			var storedSprites = this.GetSpriteSheet(2, 1, 26f, "curtains.png");
			curt.sprClosed = storedSprites[0];
			curt.sprOpen = storedSprites[1];
			curt.audMan = curt.gameObject.CreatePropagatedAudioManager(65, 85);

			var curtFace = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1], false);
			curtFace.transform.SetParent(curt.transform);
			curtFace.transform.localPosition = Vector3.up * 5.14f;
			curtFace.name = "CurtainFace";

			curt.renderer = curtFace;

			var colliderObj = new GameObject("CurtainBlockRaycast")
			{
				layer = LayerStorage.blockRaycast
			};
			colliderObj.transform.SetParent(curt.transform);
			colliderObj.transform.localPosition = Vector3.zero;

			curt.collider = colliderObj.gameObject.AddBoxCollider(Vector3.zero, new(5f, 5f, 1f), true);
			curt.collider.enabled = false;

			curtPre = curt;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup();

			int curtains = crng.Next(minCurtains, maxCurtains + 1);
			List<List<Cell>> list = ec.FindHallways();
			int num = 0;
			while (num < curtains && list.Count > 0)
			{
				int num2 = crng.Next(0, list.Count);
				for (int i = 0; i < list[num2].Count; i++)
					if (list[num2][i].HasAnyHardCoverage || list[num2][i].shape != TileShape.Straight)
						list[num2].RemoveAt(i--);
					
				if (list[num2].Count > 0)
				{
					int num3 = crng.Next(0, list[num2].Count);
					Direction direction = Directions.OpenDirectionsFromBin(list[num2][num3].ConstBin)[crng.Next(0, Directions.OpenDirectionsFromBin(list[num2][num3].ConstBin).Count)];
					var curt = Instantiate(curtPre, list[num2][num3].TileTransform);
					curt.transform.localPosition = direction.ToVector3() * 5f;
					curt.transform.rotation = direction.ToRotation();
					curt.AttachToCell(ec, list[num2][num3], direction);
					this.curtains.Add(curt);

					list[num2][num3].HardCoverWall(direction, true);
					ec.CellFromPosition(list[num2][num3].position + direction.ToIntVector2()).HardCoverWall(direction.GetOpposite(), true);
					num++;
				}
				list.RemoveAt(num2);
			}
		}

		public override void Begin()
		{
			base.Begin();
			StartCoroutine(ClosePerGroup());
		}

		IEnumerator ClosePerGroup()
		{
			yield return null;
			for (int i = 0; i < curtains.Count; i++)
			{
				curtains[i].Close(true);
				if (i % 3 == 0)
					yield return null;
			}
		}

		public override void End()
		{
			base.End();
			curtains.ForEach(x => x.TimedClose(false, Random.Range(1f, 15f)));
		}

		readonly List<Curtains> curtains = [];

		[SerializeField]
		internal Curtains curtPre;

		[SerializeField]
		internal int minCurtains = 9, maxCurtains = 16;
	}
}

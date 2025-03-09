using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using PixelInternalAPI.Classes;
using UnityEngine;
using BBTimes.Extensions;
using BBTimes.CustomComponents.SecretEndingComponents;
using MTM101BaldAPI;

namespace BBTimes.CustomContent.Builders
{
	internal class Structure_SecretButton : StructureBuilder, IBuilderPrefab
	{

		public StructureWithParameters SetupBuilderPrefabs()
		{
			var sprs = this.GetSpriteSheet(3, 1, 25f, "SecretButton.png");
			var but = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0], false)
				.AddSpriteHolder(out var butRenderer, 0f, LayerStorage.iClickableLayer);
			but.name = "SecretButton";
			but.gameObject.ConvertToPrefab(true);
			butRenderer.name = "SecretButtonRenderer";

			secButPre = but.gameObject.AddComponent<SecretButton>();
			secButPre.audMan = secButPre.gameObject.CreatePropagatedAudioManager();
			secButPre.renderer = butRenderer;
			secButPre.sprPressed = sprs[2];
			secButPre.gameObject.AddBoxCollider(Vector3.zero, new(5f, 5f, 1f), true);
			secButPre.audPress = GenericExtensions.FindResourceObject<GameButton>().audPress;
			secButPre.sprReadyToPress = sprs[1];

			return new() { prefab = this, parameters = null };
		}
		public void SetupPrefab() { }
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "objects";

		public override void Generate(LevelGenerator lg, System.Random rng)
		{
			base.Generate(lg, rng);

			if (Singleton<BaseGameManager>.Instance is not MainGameManager || !Singleton<BaseGameManager>.Instance.levelObject.finalLevel) // hard checks because it needs to be specifically in the final level, to override the ending
				return;

			var room = lg.Ec.mainHall;

			var cells = room.AllTilesNoGarbage(false, false);
			cells.RemoveAll(c => !c.HasAllFreeWall);
			if (cells.Count != 0)
			{
				var cell = cells[rng.Next(cells.Count)];
				var but = Instantiate(secButPre, cell.ObjectBase);
				var dir = cell.RandomUncoveredDirection(rng);
				but.transform.position = cell.CenterWorldPosition + dir.ToVector3() * 4.9f;
				but.transform.rotation = dir.ToRotation();
				cell.HardCoverEntirely(); // YES, entirely. To make the spot completely safe lol
			}

			Finished();
		}

		[SerializeField]
		internal SecretButton secButPre;
	}
}

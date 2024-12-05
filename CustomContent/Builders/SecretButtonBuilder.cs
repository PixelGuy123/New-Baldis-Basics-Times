using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using PixelInternalAPI.Classes;
using UnityEngine;
using BBTimes.Extensions;
using BBTimes.CustomComponents.SecretEndingComponents;
using MTM101BaldAPI;

namespace BBTimes.CustomContent.Builders
{
	internal class SecretButtonBuilder : ObjectBuilder, IObjectPrefab
	{

		public void SetupPrefab()
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
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			if (Singleton<BaseGameManager>.Instance is not MainGameManager || !Singleton<BaseGameManager>.Instance.levelObject.finalLevel) // hard checks because it needs to be specifically in the final level, to override the ending
				return;

			var cells = room.AllTilesNoGarbage(false, false);
			cells.RemoveAll(c => !c.HasFreeWall);
			if (cells.Count != 0)
			{
				var but = Instantiate(secButPre, ec.transform);
				var cell = cells[cRng.Next(cells.Count)];
				var dir = cell.RandomUncoveredDirection(cRng);
				but.transform.position = cell.CenterWorldPosition + dir.ToVector3() * 4.9f;
				but.transform.rotation = dir.ToRotation();
				cell.HardCoverEntirely(); // YES, entirely. To make the spot completely safe lol
			}
		}

		[SerializeField]
		internal SecretButton secButPre;
	}
}

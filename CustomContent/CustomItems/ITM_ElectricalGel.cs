using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.Extensions;
using System.Collections;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_ElectricalGel : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			var gelRRenderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "gel.png"));
			gelRRenderer.transform.SetParent(transform);
			gelRRenderer.transform.localPosition = Vector3.zero;
			gelRRenderer.name = "GelRenderer";
			renderer = gelRRenderer.transform;

			entity = gameObject.CreateEntity(3f, rendererBase:gelRRenderer.transform);
			audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");
			elePre = BBTimesManager.man.Get<Eletricity>("EletricityPrefab");

		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			ec = pm.ec;
			pm.RuleBreak("littering", 2f, 0.8f);
			StartCoroutine(Fall());

			entity.Initialize(pm.ec, pm.transform.position);

			direction = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audThrow);
			return true;
		}

		IEnumerator Fall()
		{
			renderer.localPosition = Vector3.zero;
			float fallSpeed = 5f;
			while (true)
			{
				fallSpeed -= ec.EnvironmentTimeScale * Time.deltaTime * 35f;
				renderer.localPosition += Vector3.up * fallSpeed * Time.deltaTime * ec.EnvironmentTimeScale;
				if (renderer.transform.localPosition.y <= fallLimit)
				{
					Destroy(gameObject);
					IntVector2 pos = IntVector2.GetGridPosition(transform.position);
					IntVector2 ogPos = pos;
					IntVector2 max = new(pos.x + 1, pos.z + 1);
					var room = ec.CellFromPosition(pos).room;

					for (pos.x = ogPos.x - 1; pos.x <= max.x; pos.x++) // Fill up 3x3 area
					{
						for (pos.z = ogPos.z - 1; pos.z <= max.z; pos.z++)
						{
							var cell = ec.CellFromPosition(pos);
							if (!cell.Null && cell.TileMatches(room)) 
							{
								var ele = Instantiate(elePre);
								ele.Initialize(null, cell.FloorWorldPosition, 0.15f, ec);
								ele.eletricityForce = 10f;
								ele.StartCoroutine(GameExtensions.TimerToDestroy(ele.gameObject, ec, 15f));
							}
						}
					}

					

					yield break;
				}

				yield return null;
			}
		}

		void Update() =>
			entity.UpdateInternalMovement(direction * throwSpeed);


		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal float throwSpeed = 55f, fallLimit = -4.5f;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Eletricity elePre;

		[SerializeField]
		internal SoundObject audThrow;

		Vector3 direction;
		EnvironmentController ec;
    }
}

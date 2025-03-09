using BBTimes.CustomComponents;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_RottenCheese : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			var storedSprites = this.GetSpriteSheet(3, 2, 35f, "cheese.png");
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			renderer.transform.SetParent(transform);
			renderer.name = "RottenCheeseVisual";

			entity = gameObject.CreateEntity(2f, 2f, renderer.transform);
			entity.SetHeight(1f);
			sprs = [.. storedSprites];
			this.renderer = renderer;
			audPut = this.GetSound("cheesePlace.wav", "Vfx_RotCheese_Place", SoundType.Effect, Color.white);
			audMan = gameObject.CreatePropagatedAudioManager();
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			audMan.PlaySingle(audPut);
			entity.Initialize(pm.ec, pm.transform.position);
			entity.SetHeight(1);

			map = new(pm.ec, PathType.Nav, transform);
			map.QueueUpdate();
			map.Activate();

			StartCoroutine(Rotten());
			return true;
		}

		IEnumerator Rotten()
		{
			while (map.PendingUpdate) yield return null;
			List<NavigationState_WanderFleeOverride> fleeStates = [];
			float cooldown = 10f;

			for (int i = 0; i < pm.ec.Npcs.Count; i++)
			{
				if (pm.ec.Npcs[i].Navigator.isActiveAndEnabled && pm.ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
				{
					var navigationState_WanderFleeOverride = new NavigationState_WanderFleeOverride(pm.ec.Npcs[i], 64, map);
					fleeStates.Add(navigationState_WanderFleeOverride);
					pm.ec.Npcs[i].navigationStateMachine.ChangeState(navigationState_WanderFleeOverride);
				}
			}

			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			
			fleeStates.ForEach(x => x.End());
			map.Deactivate();

			Destroy(gameObject);
			yield break;
		}

		void Update()
		{
			frame += pm.ec.EnvironmentTimeScale * Time.deltaTime * 7f;
			frame %= sprs.Length;
			renderer.sprite = sprs[Mathf.FloorToInt(frame)];
		}

		float frame = 0f;

		DijkstraMap map;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audPut;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] sprs;
	}
}

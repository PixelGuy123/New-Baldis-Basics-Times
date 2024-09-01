using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections.Generic;
using BBTimes.Manager;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_FidgetSpinner : Item, IEntityTrigger, IItemPrefab
	{

		public void SetupPrefab() 
		{ 
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(9f, "SpinnerPlaced.png"), false).AddSpriteHolder(-4.5f, 0);
			var rendererBase = renderer.transform.parent;
			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(1f, 2f, rendererBase);

			audMan = gameObject.CreatePropagatedAudioManager(65, 110);
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");

			this.renderer = renderer.transform;
			renderer.gameObject.layer = 0;
			renderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (!pm.plm.Entity.Override(overrider))
			{
				Destroy(gameObject);
				return false;
			}

			overrider.SetFrozen(true);
			overrider.SetInteractionState(false);
			height = overrider.entity.BaseHeight;
			overrider.SetHeight(height - 2f);
			this.pm = pm;
			entity.Initialize(pm.ec, pm.transform.position);
			ec = pm.ec;
			rotation = renderer.eulerAngles;

			rooms.AddRange(ec.rooms);
			rooms.RemoveAll(x => x.type == RoomType.Hall);

			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (pm.gameObject == other.gameObject)
				return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					audMan.PlaySingle(audHit);
					e.AddForce(new((other.transform.position - transform.position).normalized, 35f, -20f));
					return;
				}
			}
			
		}

		public void EntityTriggerStay(Collider other)
		{

		}

		public void EntityTriggerExit(Collider other)
		{
		}

		void Update()
		{
			if (!pm)
			{
				Destroy(gameObject);
				return;
			}
			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime <= 0f && targets.Count == 0)
			{
				Destroy(gameObject);
				return;
			}

			if (targets.Count == 0)
			{
				while (targets.Count == 0)
				{
					ec.FindPath(ec.CellFromPosition(transform.position), rooms.Count == 0 ? ec.mainHall.TileAtIndex(Random.Range(0, ec.mainHall.TileCount)) : rooms[Random.Range(0, rooms.Count)].RandomEntitySafeCellNoGarbage(), PathType.Nav, out targets, out bool flag);
					if (!flag)
						targets.Clear();
				}
			}
			else
			{
				accel += ec.EnvironmentTimeScale * Time.deltaTime * 9f;
				if (accel > maxFlySpeed)
					accel = maxFlySpeed;

				float speed = accel * Time.deltaTime * ec.EnvironmentTimeScale;
				while (Time.timeScale != 0f && speed > 0f && targets.Count != 0) // Basically how a NPC move, but simplified
				{
					var dist = targets[0].CenterWorldPosition - transform.position;
					var mag = dist.magnitude;
					var dir = dist.normalized;

					float move = mag >= speed ? speed : mag;
					entity.Teleport(transform.position + move * dir);
					speed -= move;

					if (move == mag)
						targets.RemoveAt(0);

					ray.origin = transform.position;
					ray.direction = dir;
					if (Physics.Raycast(ray, out var hit, 5f))
					{
						var door = hit.transform.GetComponent<StandardDoor>();
						if (door)
							door.OpenTimed(door.DefaultTime, false);
						else
						{
							var swingDoor = hit.transform.GetComponent<SwingDoor>();
							if (swingDoor)
								swingDoor.OpenTimed(swingDoor.defaultTime, false);
						}
					}
				}
			}
			pm.Teleport(transform.position);
			rotation.y += 35f * accel * Time.deltaTime * ec.EnvironmentTimeScale;
			renderer.eulerAngles = rotation;

			
		}

		void OnDestroy()
		{
			overrider.SetFrozen(false);
			overrider.SetInteractionState(true);
			overrider.SetHeight(height);
			overrider.Release();
		}

		Ray ray = new();
		Vector3 rotation = default;
		List<Cell> targets = [];
		EnvironmentController ec;
		float height = 5f, accel = 0f;
		readonly List<RoomController> rooms = [];

		[SerializeField]
		internal float maxFlySpeed = 95f, lifeTime = 30f;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal Entity entity;

		readonly EntityOverrider overrider = new();
	}
}

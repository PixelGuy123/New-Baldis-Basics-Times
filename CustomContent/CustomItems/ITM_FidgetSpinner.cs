using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_FidgetSpinner : Item, IItemPrefab
	{

		public void SetupPrefab()
		{
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(9f, "SpinnerPlaced.png"), false).AddSpriteHolder(out var fidgetRenderer, 0.5f, 0);

			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.zero;

			var collider = gameObject.AddComponent<CapsuleCollider>();
			collider.radius = 3.8f;
			collider.isTrigger = true;

			audMan = gameObject.CreatePropagatedAudioManager(65, 110);
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");

			this.renderer = fidgetRenderer.transform;
			fidgetRenderer.gameObject.layer = 0;
			fidgetRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

			nav = gameObject.AddComponent<MomentumNavigator>();
			nav.maxSpeed = 95f;
			nav.accel = 9f;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

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
			transform.position = pm.transform.position;
			ec = pm.ec;
			rotation = renderer.eulerAngles;

			rooms.AddRange(ec.rooms);
			rooms.RemoveAll(x => x.type == RoomType.Hall);

			nav.Initialize(ec);
			nav.OnMove += (pos, dir) =>
			{
				ray.origin = pos;
				ray.direction = dir;
				if (Physics.Raycast(ray, out var hit, 5f))
					CheckForDoor(hit.transform);

				pm.Teleport(pos);
			};

			gaugeSprite = ItmObj.itemSpriteSmall;

			return true;
		}

		void CheckForDoor(Transform t)
		{
			var door = t.GetComponent<StandardDoor>();
			if (door)
				door.OpenTimed(door.DefaultTime, false);
			else
			{
				var swingDoor = t.GetComponent<SwingDoor>();
				if (swingDoor)
					swingDoor.OpenTimed(swingDoor.defaultTime, false);
				else
				{
					var genDoor = t.GetComponent<GenericDoor>();
					if (genDoor)
						genDoor.Open(genDoor.DefaultOpenTimer, false);
				}
			}
		}

		void OnTriggerEnter(Collider other)
		{
			if (pm.gameObject == other.gameObject)
				return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					audMan.PlaySingle(audHit);
					float push = 0.45f * nav.Acceleration;
					e.AddForce(new((other.transform.position - transform.position).normalized, push, -push * 0.95f));
				}
			}

		}

		void Update()
		{
			if (!pm)
			{
				Destroy(gameObject);
				return;
			}
			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime <= 0f && !nav.HasDestination)
			{
				Destroy(gameObject);
				return;
			}

			if (!nav.HasDestination)
			{
				nav.FindPath(
					(rooms.Count == 0 ?
					ec.mainHall.TileAtIndex(Random.Range(0, ec.mainHall.TileCount)) :
					rooms[Random.Range(0, rooms.Count)].RandomEntitySafeCellNoGarbage()
					).FloorWorldPosition);
			}

			rotation.y += 35f * nav.Acceleration * ec.EnvironmentTimeScale * Time.deltaTime;
			rotation.y %= 360f;
			renderer.eulerAngles = rotation;


		}

		void OnDestroy()
		{
			if (overrider.entity)
			{
				overrider.SetFrozen(false);
				overrider.SetInteractionState(true);
				overrider.SetHeight(height);
				overrider.Release();
			}
		}

		Ray ray = new();
		Vector3 rotation = default;
		EnvironmentController ec;
		float height = 5f;
		readonly List<RoomController> rooms = [];

		[SerializeField]
		internal float lifeTime = 15f;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal MomentumNavigator nav;

		[SerializeField]
		internal Sprite gaugeSprite;
		HudGauge gauge;

		readonly EntityOverrider overrider = new();
	}
}

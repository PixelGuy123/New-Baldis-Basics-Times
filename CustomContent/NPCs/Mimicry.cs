using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;


namespace BBTimes.CustomContent.NPCs
{
	public class Mimicry : NPC, INPCPrefab, IClickable<int> // Npc here
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			audJumpscare = this.GetSound("mimi_blindJumpscare.wav", "Vfx_Mimi_Jumpscare", SoundType.Voice, audMan.subtitleColor);
			audLaughter = this.GetSound("mimi_laughAway.wav", "Vfx_Mimi_Laughter", SoundType.Voice, audMan.subtitleColor);
			renderer = spriteRenderer[0];

			var sprs = this.GetSpriteSheet(4, 1, 25f, "mimiCry.png");
			renderer.sprite = sprs[0];
			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = [renderer];
			animComp.speed = 6f;

			sprWalking = [sprs[0], sprs[1]];
			sprLaughing = [sprs[2], sprs[3]];

			jumpscareSprs = this.GetSpriteSheet(4, 3, 1f, "mimiCryCanvas.png");

			canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.name = "MimicryCanvas";
			canvas.gameObject.SetActive(false);

			jumpscareImg = ObjectCreationExtensions.CreateImage(canvas, jumpscareSprs[0]);
			jumpscareImg.name = "MimicryJumpscare";

			itemRenderer = ObjectCreationExtensions.CreateSpriteBillboard(null);
			itemRenderer.name = "ItemRenderer";
			itemRenderer.enabled = false;

			itemRenderer.transform.SetParent(transform);
			itemRenderer.transform.localScale = new(0.95f, 0.95f, 0.95f); // To make it a little more obvious
			itemRenderer.gameObject.AddComponent<CustomPickupBob>().speed = 4.5f; // Simulate exactly like a random item

			spriteRenderer = [renderer, itemRenderer];

			var myCol = (CapsuleCollider)baseTrigger[0];
			var col = this.CreateClickableLink().gameObject.AddComponent<CapsuleCollider>();
			col.isTrigger = true;
			col.height = myCol.height;
			col.direction = myCol.direction;
			col.radius = myCol.radius;

			gaugeSprite = this.GetSprite(Storage.GaugeSprite_PixelsPerUnit, "mimiCryGauge.png");
		}
		public void SetupPrefabPost() =>
			itemsToDisguiseAs = GameExtensions.GetAllShoppingItems();

		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			moveMod = new MovementModifier(Vector3.zero, slownessFactor);
			animComp.Initialize(ec);
			childReference = new GameObject("Mimicry_PositionReference").transform;

			SetWalking(true);
			Undisguise(false);
		}

		public override void Despawn()
		{
			base.Despawn();
			for (int i = 0; i < affectedEntities.Count; i++)
				affectedEntities[i]?.ExternalActivity.moveMods.Remove(moveMod);

			gauge?.Deactivate();
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (disguised && preventivePickup && preventivePickup.gameObject.activeSelf)
			{
				autoDisabledPickup = true;
				preventivePickup.Hide(true); // No pickup in spot until mimicry is done
			}
		}

		public void SetWalking(bool walking) =>
			animComp.animation = walking ? sprWalking : sprLaughing;
		public void RushToRoom()
		{
			navigator.maxSpeed = speedToReachRoom;
			navigator.SetSpeed(speedToReachRoom);
		}
		public void WalkNormally()
		{
			navigator.maxSpeed = normalSpeed;
			navigator.SetSpeed(normalSpeed);
		}

		public Vector3 GetItemSpawnPoint()
		{
			// Will be useful to know how far rooms are from the camera (that is, the player)
			var dijsMap = GameCamera.dijkstraMap;
			bool hasActivityRoom = false;

			// Get all suitable rooms
			_rooms.Clear();
			for (int i = 0; i < ec.rooms.Count; i++)
			{
				var r = ec.rooms[i];
				if (r.type == RoomType.Room && (r.itemSpawnPoints.Count != 0 || r.AvailableItemRespawnPoints != 0))
				{
					_rooms.Add(ec.rooms[i]);
					if (ec.rooms[i].HasIncompleteActivity)
						hasActivityRoom = true;
				}
			}

			if (hasActivityRoom)
				_rooms.RemoveAll(room => !room.HasIncompleteActivity); // Remove every room that is not a classroom, basically lol

			if (_rooms.Count == 0)
				return transform.position; // let's just hide into itself lol

			// Find max distance for normalization
			int maxDist = 1;
			int[] dists = new int[_rooms.Count];
			if (dijsMap != null) // Well, just for safety. The GameCamera does this check as well lol
			{
				for (int i = 0; i < _rooms.Count; i++)
				{
					var room = _rooms[i];
					var gridPos = IntVector2.GetGridPosition(ec.RealRoomMid(room));

					int dist = dijsMap.Value(gridPos);
					dists[i] = dist;

					maxDist = Mathf.Max(maxDist, dist);
				}
			}
			else
			{
				for (int i = 0; i < dists.Length; i++)
					dists[i] = 0;
			}

			// Calculate weights (get the one that's mostly likely the player will want to go to)
			WeightedRoomController[] weights = new WeightedRoomController[_rooms.Count];
			for (int i = 0; i < _rooms.Count; i++)
			{
				var room = _rooms[i];
				int weight = 10;

				if (dijsMap != null)
				{
					// Closer rooms get more weight. Add (maxDist - dist) to weight.
					weight += System.Math.Max(0, maxDist - dists[i]);
				}

				weights[i] = new() { selection = room, weight = weight };
			}

			// Weighted random selection
			var selectedRoom = WeightedRoomController.RandomSelection(weights);

			// Workaround to work with objects to actually give the exact position that Mimicry should go
			if (!childReference)
				childReference = new GameObject("Mimicry_PositionReference").transform;

			_spotsToDisguise.Clear();
			// Get respawn points
			for (int i = 0; i < selectedRoom.pickups.Count; i++)
			{
				if (!selectedRoom.pickups[i].gameObject.activeSelf)
				{
					_spotsToDisguise.Add(new(selectedRoom.pickups[i], selectedRoom.pickups[i].transform.position.ZeroOutY()));
				}
			}

			// Get item spawn points
			for (int i = 0; i < selectedRoom.itemSpawnPoints.Count; i++)
			{
				Vector2 pos = selectedRoom.itemSpawnPoints[i].position;
				Vector3 worldPos = selectedRoom.objectObject.transform.TransformVector(pos.x, 0, pos.y); // Get local position v
				_spotsToDisguise.Add(new(null, worldPos));
			}

			childReference.SetParent(selectedRoom.objectObject.transform);
			var spot = _spotsToDisguise[Random.Range(0, _spotsToDisguise.Count)];
			childReference.position = spot.Value; // It should go to the exact position, since it'll be relative to how the objectObject is placed in world
			preventivePickup = spot.Key; // In case the position Mimicry goes has a Pickup active, it'll automatically disable it until Mimicry is done

			autoDisabledPickup = false;

			return childReference.position;
		}

		public void DisguiseAsRandomItem()
		{
			renderer.enabled = false;
			// Do operation to get a random item to disguise as
			itemRenderer.sprite = itemsToDisguiseAs[Random.Range(0, itemsToDisguiseAs.Count)].itemSpriteLarge;

			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			navigator.Entity.SetGrounded(false);
			itemRenderer.enabled = true;
			disguised = true;
		}

		public void Undisguise(bool laugh)
		{
			renderer.enabled = true;
			itemRenderer.enabled = false;
			navigator.Entity.SetGrounded(true);
			WalkNormally();
			disguised = false;

			if (preventivePickup)
			{
				if (autoDisabledPickup)
					preventivePickup.Hide(false);
				preventivePickup = null;
			}
			autoDisabledPickup = false;

			behaviorStateMachine.ChangeState(new Mimicry_Wander(this));
			if (laugh)
			{
				SetGuilt(5f, "Bullying");
				StartCoroutine(LaughterDelay());
			}
		}

		public bool ClickableHidden() => !disguised;
		public bool ClickableRequiresNormalHeight() => true;
		public void Clicked(int player)
		{
			if (!disguised) return;

			Undisguise(true);

			var pm = Singleton<CoreGameManager>.Instance.GetPlayer(player);
			StartCoroutine(AffectSomeone(pm.plm.Entity, pm));
			StartCoroutine(Jumpscare(pm));
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public void JumpscareNPC(NPC npc)
		{
			Undisguise(true);
			StartCoroutine(AffectSomeone(npc.Entity, null));
		}
		IEnumerator LaughterDelay()
		{
			audMan.PlaySingle(audLaughter);
			animComp.animation = sprLaughing;

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			animComp.animation = sprWalking;
		}

		IEnumerator AffectSomeone(Entity entity, PlayerManager player)
		{
			entity?.ExternalActivity.moveMods.Add(moveMod);
			affectedEntities.Add(entity);

			if (player)
				gauge = Singleton<CoreGameManager>.Instance.GetHud(player.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, entitySlownessCooldown);

			float delay = entitySlownessCooldown;
			while (delay > 0f)
			{
				delay -= TimeScale * Time.deltaTime;
				gauge?.SetValue(entitySlownessCooldown, delay);
				yield return null;
			}

			gauge?.Deactivate();
			entity?.ExternalActivity.moveMods.Remove(moveMod);
			affectedEntities.Remove(entity);
		}

		IEnumerator Jumpscare(PlayerManager pm)
		{
			canvas.gameObject.SetActive(true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audJumpscare);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			float frame = 0f;
			jumpscareImg.sprite = jumpscareSprs[0];

			while (frame < jumpscareSprs.Length)
			{
				frame += Time.deltaTime * TimeScale * 16f;
				jumpscareImg.sprite = jumpscareSprs[Mathf.FloorToInt(Mathf.Min(jumpscareSprs.Length - 1, frame))];
				yield return null;
			}

			canvas.gameObject.SetActive(false);
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audJumpscare, audLaughter;

		[SerializeField]
		internal SpriteRenderer renderer, itemRenderer;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal Sprite[] sprWalking, sprLaughing, jumpscareSprs;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal Image jumpscareImg;

		[SerializeField]
		internal float wanderingCooldown = 30f, waitingDisguisedCooldown = 60f, entitySlownessCooldown = 15f, speedToReachRoom = 35f, normalSpeed = 15f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slownessFactor = 0.12f;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal List<ItemObject> itemsToDisguiseAs;
		readonly List<Entity> affectedEntities = [];
		readonly List<RoomController> _rooms = [];
		readonly List<KeyValuePair<Pickup, Vector3>> _spotsToDisguise = [];
		MovementModifier moveMod;
		Transform childReference;
		HudGauge gauge;
		Pickup preventivePickup;

		bool disguised = false, autoDisabledPickup = false;


	}

	internal class Mimicry_StateBase(Mimicry mimi) : NpcState(mimi) // A default npc state
	{
		protected Mimicry mimi = mimi;
	}

	internal class Mimicry_Wander(Mimicry mimi) : Mimicry_StateBase(mimi)
	{
		float cooldown = mimi.wanderingCooldown;
		public override void Enter()
		{
			base.Enter();
			mimi.WalkNormally();
			ChangeNavigationState(new NavigationState_WanderRandom(mimi, 0));
		}
		public override void Update()
		{
			base.Update();
			cooldown -= mimi.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				mimi.behaviorStateMachine.ChangeState(new Mimicry_TargetItemSpawnPoint(mimi));
		}
	}

	internal class Mimicry_TargetItemSpawnPoint(Mimicry mimi) : Mimicry_StateBase(mimi)
	{
		readonly Vector3 spotToGo = mimi.GetItemSpawnPoint();

		NavigationState_TargetPosition tarPos;
		public override void Enter()
		{
			base.Enter();
			mimi.RushToRoom();
			tarPos = new(mimi, 64, spotToGo);
			ChangeNavigationState(tarPos);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (mimi.transform.position.x == spotToGo.x && mimi.transform.position.z == spotToGo.z)
				mimi.behaviorStateMachine.ChangeState(new Mimicry_Disguise(mimi));
			else
				ChangeNavigationState(tarPos);
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
		}
	}

	internal class Mimicry_Disguise(Mimicry mimi) : Mimicry_StateBase(mimi)
	{
		float activeCooldown = mimi.waitingDisguisedCooldown;
		public override void Enter()
		{
			base.Enter();
			mimi.DisguiseAsRandomItem();
			mimi.Entity.SetFrozen(true);
			ChangeNavigationState(new NavigationState_DoNothing(mimi, 0));
		}

		public override void Update()
		{
			base.Update();
			activeCooldown -= mimi.TimeScale * Time.deltaTime;
			if (activeCooldown < 0f)
			{
				mimi.Undisguise(false);
				return;
			}

			if (!mimi.Blinded)
			{
				for (int i = 0; i < mimi.ec.Npcs.Count; i++)
				{
					if (mimi != mimi.ec.Npcs[i] && mimi.ec.Npcs[i].Navigator.isActiveAndEnabled && mimi.looker.RaycastNPC(mimi.ec.Npcs[i]))
					{
						mimi.JumpscareNPC(mimi.ec.Npcs[i]);
						return;
					}
				}
			}
		}

		public override void Exit()
		{
			base.Exit();
			mimi.Entity.SetFrozen(false);
		}
	}

}

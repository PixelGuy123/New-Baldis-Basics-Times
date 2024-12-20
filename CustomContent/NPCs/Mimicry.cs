using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
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

			var sprs = this.GetSpriteSheet(3, 1, 25f, "mimiCry.png");
			renderer.sprite = sprs[0];
			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = [renderer];
			animComp.speed = 6f;

			sprWalking = [sprs[0], sprs[1]];
			sprLaughing = [sprs[2]];

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
			itemRenderer.gameObject.AddComponent<PickupBob>(); // Simulate exactly like a random item

			spriteRenderer = [renderer, itemRenderer];

			var myCol = (CapsuleCollider)baseTrigger[0];
			var col = this.CreateClickableLink().gameObject.AddComponent<CapsuleCollider>();
			col.isTrigger = true;
			col.height = myCol.height;
			col.direction = myCol.direction;
			col.radius = myCol.radius;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			moveMod = new MovementModifier(Vector3.zero, slownessFactor);
			itemsToDisguiseAs = Resources.FindObjectsOfTypeAll<ItemObject>();
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
		}

		public void SetWalking(bool walking) =>
			animComp.animation = walking ? sprWalking : sprLaughing;

		public Vector3 GetItemSpawnPoint()
		{
			var rooms = new List<RoomController>(ec.rooms);
			rooms.RemoveAll(r => r.type != RoomType.Room || r.itemSpawnPoints.Count == 0);

			if (rooms.Count == 0)
				return transform.position;

			// Workaround to work with objects to actually give the exact position that Mimicry should go
			if (!childReference)
				childReference = new GameObject("Mimicry_PositionReference").transform;


			var selectedRoom = rooms[Random.Range(0, rooms.Count)];
			Vector2 pos = selectedRoom.itemSpawnPoints[Random.Range(0, selectedRoom.itemSpawnPoints.Count)].position;
			childReference.SetParent(selectedRoom.objectObject.transform);
			childReference.localPosition = new(pos.x, 0, pos.y); // It should go to the exact position, since it'll be relative to how the objectObject is placed in world

			return childReference.position;
		}

		public void DisguiseAsRandomItem()
		{
			renderer.enabled = false;

			// Do operation to get a random item to disguise as
			itemRenderer.sprite = itemsToDisguiseAs[Random.Range(0, itemsToDisguiseAs.Length)].itemSpriteLarge;

			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			itemRenderer.enabled = true;
			disguised = true;
		}

		public void Undisguise(bool laugh)
		{
			renderer.enabled = true;
			itemRenderer.enabled = false;
			navigator.maxSpeed = 15f;
			navigator.SetSpeed(15f);
			disguised = false;
			behaviorStateMachine.ChangeState(new Mimicry_Wander(this));
			if (laugh)
				StartCoroutine(LaughterDelay());
		}

		public bool ClickableHidden() => !disguised;
		public bool ClickableRequiresNormalHeight() => true;
		public void Clicked(int player)
		{
			if (!disguised) return;

			Undisguise(true);

			StartCoroutine(AffectSomeone(Singleton<CoreGameManager>.Instance.GetPlayer(player).plm.Entity));
			StartCoroutine(Jumpscare(Singleton<CoreGameManager>.Instance.GetPlayer(player)));
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public void JumpscareNPC(NPC npc)
		{
			Undisguise(true);
			StartCoroutine(AffectSomeone(npc.Navigator.Entity));
		}
		IEnumerator LaughterDelay()
		{
			audMan.PlaySingle(audLaughter);
			animComp.animation = sprLaughing;
			while (audMan.AnyAudioIsPlaying)
				yield return null;
			animComp.animation = sprWalking;
		}

		IEnumerator AffectSomeone(Entity entity)
		{
			entity?.ExternalActivity.moveMods.Add(moveMod);
			affectedEntities.Add(entity);

			float delay = entitySlownessCooldown;
			while (delay > 0f)
			{
				delay -= TimeScale * Time.deltaTime;
				yield return null;
			}

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
		internal float wanderingCooldown = 30f, waitingDisguisedCooldown = 60f, entitySlownessCooldown = 15f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slownessFactor = 0.12f;

		ItemObject[] itemsToDisguiseAs;
		readonly List<Entity> affectedEntities = [];
		MovementModifier moveMod;
		Transform childReference;

		bool disguised = false;


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
			mimi.Navigator.Entity.SetFrozen(true);
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
					if (mimi != mimi.ec.Npcs[i] && mimi.ec.Npcs[i].Navigator.isActiveAndEnabled)
					{
						mimi.looker.Raycast(mimi.ec.Npcs[i].transform, 
							Mathf.Min((mimi.transform.position - mimi.ec.Npcs[i].transform.position).magnitude + mimi.ec.Npcs[i].Navigator.Velocity.magnitude, 
							mimi.looker.distance, 
							mimi.ec.MaxRaycast), out bool flag);
						if (flag)
						{
							mimi.JumpscareNPC(mimi.ec.Npcs[i]);
							return;
						}
					}
				}
			}
		}

		public override void Exit()
		{
			base.Exit();
			mimi.Navigator.Entity.SetFrozen(false);
		}
	}

}

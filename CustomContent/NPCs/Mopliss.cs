using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;
using MTM101BaldAPI;
using System.Collections.Generic;

namespace BBTimes.CustomContent.NPCs
{
	public class Mopliss : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			audRefill = this.GetSoundNoSub("Mopliss_Refill.wav", SoundType.Voice);

			var mop = this.GetSprite(29f, "mopliss.png");
			spriteRenderer[0].sprite = mop;

			slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").SafeDuplicatePrefab(true);
			((SpriteRenderer)slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = this.GetSprite(26.1f, "mopLissWater.png");

			bucketPre = ObjectCreationExtensions.CreateSpriteBillboard(BBTimesManager.man.Get<Sprite>("fieldTripBucket")) // FireFuel_Sheet_0 is bucket
				.AddSpriteHolder(out var renderer, 1.2f);
			renderer.name = "Bucket_Renderer";
			bucketPre.name = "Bucket";
			bucketPre.gameObject.ConvertToPrefab(true);
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// stuff above^^

		public override void Initialize()
		{
			base.Initialize();
			home = ec.CellFromPosition(transform.position);

			var bucket = Instantiate(bucketPre);
			bucket.transform.position = home.FloorWorldPosition;
			home.AddRenderer(bucket.renderers[0]);

			behaviorStateMachine.ChangeState(new Mopliss_Wait(this));
		}

		internal void StartSweeping()
		{
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}
		internal void StopSweeping()
		{
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			accessedRooms.Clear();
		}


		public void SpawnSlipperArea(Cell cell)
		{
			IntVector2 pos = cell.position;
			var room = cell.room;

			for (int x = pos.x - slipperRadius; x < pos.x + slipperRadius; x++)
			{
				for (int z = pos.z - slipperRadius; z < pos.z + slipperRadius; z++)
				{
					if (ec.ContainsCoordinates(x, z))
					{
						var foundCell = ec.CellFromPosition(x, z);
						if (!foundCell.Null && foundCell.TileMatches(room))
							SpawnSlipper(foundCell);
					}
				}
			}
		}

		public void RefillWater() =>
			audMan.QueueAudio(audRefill);

		void SpawnSlipper(Cell cell)
		{
			var slip = Instantiate(slipMatPre);
			slip.SetAnOwner(gameObject);
			slip.transform.position = cell.FloorWorldPosition;
			slip.StartCoroutine(GameExtensions.TimerToDestroy(slip.gameObject, ec, 30f));
		}

		internal bool IsHome => home == ec.CellFromPosition(transform.position);
		internal Cell RandomSpotToGo { get
			{
				var rooms = new List<RoomController>(ec.rooms);
				if (rooms.Count != 1)
				{
					var roomFound = ec.CellFromPosition(transform.position).room;
					rooms.RemoveAll(x => 
					x.type == RoomType.Hall ||
					roomFound == x || 
					accessedRooms.Contains(x));
				}

				if (rooms.Count == 0)
					return null;

				var room = rooms[Random.Range(0, rooms.Count)];
				accessedRooms.Add(room);

				var cells = room.AllEntitySafeCellsNoGarbage();
				return cells[Random.Range(0, cells.Count)];
			}
		}
		internal float WaitCooldown => Random.Range(minWait, maxWait);
		internal Cell home;
		readonly HashSet<RoomController> accessedRooms = [];

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audRefill;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		[SerializeField]
		internal RendererContainer bucketPre;

		[SerializeField]
		internal float minWait = 30f, maxWait = 45f, speed = 35f;

		[SerializeField]
		internal int roomsPerActivation = 10, slipperRadius = 6;
	}

	internal class Mopliss_StateBase(Mopliss mop) : NpcState(mop)
	{
		protected Mopliss mop = mop;
	}

	internal class Mopliss_Wait(Mopliss mop) : Mopliss_StateBase(mop)
	{
		float waitCooldown = mop.WaitCooldown;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(mop, 0));
			mop.StopSweeping();
		}
		public override void Update()
		{
			base.Update();
			waitCooldown -= mop.TimeScale * Time.deltaTime;
			if (waitCooldown <= 0f)
				mop.behaviorStateMachine.ChangeState(new Mopliss_Start(mop));
		}
	}

	internal class Mopliss_Start(Mopliss mop) : Mopliss_StateBase(mop)
	{
		int visitedRooms = 0;
		public override void Initialize()
		{
			base.Initialize();
			mop.StartSweeping();

		}
		public override void Update()
		{
			base.Update();
			if (visitedRooms++ <= mop.roomsPerActivation)
				mop.behaviorStateMachine.ChangeState(new Mopliss_GoForARoom(mop, this));
			else
				mop.behaviorStateMachine.ChangeState(new Mopliss_GoBack(mop));
		}
	}

	internal class Mopliss_GoForARoom(Mopliss mop, Mopliss_StateBase previousState) : Mopliss_StateBase(mop)
	{
		NavigationState_TargetPosition tar;
		Cell spotToGo;
		public override void Enter()
		{
			base.Enter();
			spotToGo = mop.RandomSpotToGo;
			if (spotToGo == null) // if null, all the possible spots were accessed
			{
				mop.behaviorStateMachine.ChangeState(new Mopliss_GoBack(mop));
				return;
			}
			tar = new(mop, 65, spotToGo.FloorWorldPosition);
			ChangeNavigationState(tar);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (spotToGo == null)
				return; // Should avoid exceptions below

			if (mop.ec.CellFromPosition(mop.transform.position) != spotToGo)
				ChangeNavigationState(tar);
			else
			{
				mop.SpawnSlipperArea(spotToGo);
				mop.behaviorStateMachine.ChangeState(new Mopliss_GoBackForRefill(mop, previousState));
			}
		}
	}

	internal class Mopliss_GoBackForRefill(Mopliss mop, Mopliss_StateBase previousState) : Mopliss_StateBase(mop)
	{
		NavigationState_TargetPosition tar;
		readonly Cell cellToGo = mop.home;

		public override void Enter()
		{
			base.Enter();
			tar = new(mop, 65, cellToGo.FloorWorldPosition);
			ChangeNavigationState(tar);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (mop.ec.CellFromPosition(mop.transform.position) != cellToGo)
				ChangeNavigationState(tar);
			else
			{
				mop.RefillWater();
				mop.behaviorStateMachine.ChangeState(previousState);
			}
		}
	}

	internal class Mopliss_GoBack(Mopliss mop) : Mopliss_StateBase(mop)
	{
		NavigationState_TargetPosition tar;
		public override void Enter()
		{
			base.Enter();
			tar = new(mop, 0, mop.home.FloorWorldPosition);
			ChangeNavigationState(tar);
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (mop.IsHome)
			{
				mop.behaviorStateMachine.ChangeState(new Mopliss_Wait(mop));
				return;
			}
			ChangeNavigationState(tar);
		}
	}
}

using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;
using MTM101BaldAPI;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.NPCs
{
    public class MagicalStudent : NPC, INPCPrefab
	{

		public void SetupPrefab()
		{

			// magic prefab
			var mos = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "MGS_Magic.png")).AddSpriteHolder(out var magicRenderer, 0f, LayerStorage.standardEntities);
			magicRenderer.name = "MagicRenderer";
			mos.name = "Magic";

			mos.gameObject.ConvertToPrefab(true);

			var mo = mos.gameObject.AddComponent<MagicObject>();
			mo.entity = mos.gameObject.CreateEntity(4f, 4f, mos.transform).SetEntityCollisionLayerMask(0);
			mo.entity.SetGrounded(false);

			// MGS Setup
			magicPre = mo;
			audMan = GetComponent<PropagatedAudioManager>();
			audThrow = this.GetSound("MGS_Throw.wav", "Vfx_MGS_Magic", SoundType.Effect, new(0f, 0.33203125f, 0.99609375f));
			audPrepare = this.GetSound("MGS_Prep.wav", "Vfx_MGS_PrepMagic", SoundType.Effect, new(0f, 0.33203125f, 0.99609375f));
			throwSprites = this.GetSpriteSheet(3, 1, 65f, "MGS.png");
			spriteRenderer[0].sprite = throwSprites[0];
			renderer = spriteRenderer[0];
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
			magic = Instantiate(magicPre);
			magic.Initialize(this);
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new MagicalStudent_Wander(this, 0f));
		}

		public void ThrowMagic(PlayerManager pm) =>
			StartCoroutine(Throw(pm.transform));
		

		IEnumerator Throw(Transform target)
		{
			audMan.FlushQueue(true);
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audPrepare);
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			float cool = throwMagicDelay;
			renderer.sprite = throwSprites[1];
			float shakeness = 0f;
			Vector3 pos = renderer.transform.localPosition;
			while (cool > 0f)
			{
				cool -= TimeScale * Time.deltaTime;
				shakeness += TimeScale * Time.deltaTime * 0.05f;
				renderer.transform.localPosition = pos + new Vector3(Random.Range(-shakeness, shakeness), Random.Range(-shakeness, shakeness), Random.Range(-shakeness, shakeness));
				yield return null;
			}
			renderer.transform.localPosition = pos;
			cool = 0.3f;
			renderer.sprite = throwSprites[2];
			audMan.FlushQueue(true);
			audMan.PlaySingle(audThrow);

			magic.Throw((target.position - transform.position).normalized);

			while (cool > 0f)
			{
				cool -= TimeScale * Time.deltaTime;
				yield return null;
			}
			renderer.sprite = throwSprites[0];

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new MagicalStudent_Wander(this, Random.Range(minThrowCooldown, maxThrowCooldown)));

			yield break;
		}

		MagicObject magic;

		[SerializeField]
		internal MagicObject magicPre;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] throwSprites;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow, audPrepare;

		[SerializeField]
		internal float minThrowCooldown = 15f, maxThrowCooldown = 25f, throwMagicDelay = 1.5f;

		const float speed = 15f;
	}

	internal class MagicalStudent_StateBase(MagicalStudent mgs) : NpcState(mgs)
	{
		protected MagicalStudent mgs = mgs;

		public override void DoorHit(StandardDoor door)
		{
			if (door.locked)
			{
				door.Unlock();
				door.OpenTimed(5f, false);
				return;
			}
			base.DoorHit(door);
		}

		public override void OnRoomExit(RoomController room)
		{
			base.OnRoomExit(room);
			if (room.ec.timeOut && room.type == RoomType.Room && !room.HasIncompleteActivity)
			{
				room.SetPower(false);
			}
		}
	}

	internal class MagicalStudent_Wander(MagicalStudent mgs, float waitCooldown) : MagicalStudent_StateBase(mgs)
	{
		float cool = waitCooldown;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRounds(mgs, 0));
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(mgs, 0));
		}
		public override void PlayerInSight(PlayerManager player)
		{			
			base.PlayerInSight(player);
			if (cool > 0f) return;

			if (!player.Tagged)
				mgs.behaviorStateMachine.ChangeState(new MagicalStudent_ThrowPrep(mgs, player));
		}
		public override void Update()
		{
			base.Update();
			cool -= mgs.TimeScale * Time.deltaTime;
		}
	}

	internal class MagicalStudent_ThrowPrep(MagicalStudent mgs, PlayerManager pm) : MagicalStudent_StateBase(mgs)
	{
		readonly PlayerManager player = pm;
		Vector3 nextPoint = mgs.transform.position;
		public override void Enter()
		{
			base.Enter();
			mgs.Navigator.FindPath(mgs.transform.position, player.transform.position);
			nextPoint = mgs.Navigator.NextPoint;
			ChangeNavigationState(new NavigationState_TargetPosition(mgs, 63, nextPoint));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (mgs.looker.PlayerInSight(player) && !player.Tagged)
			{
				ChangeNavigationState(new NavigationState_DoNothing(mgs, 0));
				mgs.ThrowMagic(player);
				mgs.behaviorStateMachine.ChangeState(new MagicalStudent_StateBase(mgs)); // Who will change state now is Mgs himself
				return;
			}
			mgs.behaviorStateMachine.ChangeState(new MagicalStudent_Wander(mgs, 0f));
		}

		public override void Update()
		{
			base.Update();
			if (Vector3.Distance(nextPoint, mgs.transform.position) > 10f)
				mgs.behaviorStateMachine.ChangeState(new MagicalStudent_Wander(mgs, 0f));
		}
	}
}

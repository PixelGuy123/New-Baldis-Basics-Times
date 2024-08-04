using BBTimes.CustomComponents.NpcSpecificComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Bubbly : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			

			behaviorStateMachine.ChangeState(new Bubbly_NavigateToASpot(this));
		}

		internal Bubble SpitBubbleAtDirection(Vector3 dir)
		{
			var b = Instantiate(bubPre);
			b.Spawn(ec, navigator.Entity, transform.position, dir, Random.Range(16f, 22f));
			StartCoroutine(FillupBubble(b));
			return b;
		}

		IEnumerator FillupBubble(Bubble b)
		{
			audMan.PlaySingle(audFillUp);
			float scale = 0f;
			b.entity.SetFrozen(true);
			

			float speed = Random.Range(2.6f, 3.5f);
			while (true)
			{
				scale += (1.03f - scale) * speed * TimeScale * Time.deltaTime;
				if (scale >= 1f)
					break;
				b.renderer.transform.localScale = Vector3.one * scale;
				b.entity.Teleport(transform.position);
				yield return null;
			}
			b.renderer.transform.localScale = Vector3.one;
			b.entity.SetFrozen(false);

			b.Initialize();

			yield break;
		}

		internal void TargetRandomSpot()
		{
			List<Cell> spotsToGo = new(ec.mainHall.AllTilesNoGarbage(false, false));

			for (int i = 0; i < spotsToGo.Count; i++)
				if ((spotsToGo[i].shape != TileShape.Corner && spotsToGo[i].shape != TileShape.Single) || (spotsToGo[i].room.type == RoomType.Room && !spotsToGo[i].open)) // Filter to the ones that are corners or singles
					spotsToGo.RemoveAt(i--);

			TargetPosition(spotsToGo[Random.Range(0, spotsToGo.Count)].FloorWorldPosition);
		}
		


		[SerializeField]
		internal Bubble bubPre;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] sprWalkingAnim;

		[SerializeField]
		internal Sprite sprPrepareBub;

		[SerializeField]
		internal SoundObject audFillUp;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		
		const float speed = 17f;
	}

	internal class Bubbly_StateBase(Bubbly bub) : NpcState(bub)
	{
		protected Bubbly bub = bub;
	}

	internal class Bubbly_WalkingStateBase(Bubbly bub) : Bubbly_StateBase(bub)
	{
		float frame = 0f;

		public override void Update()
		{
			base.Update();
			frame += bub.TimeScale * Time.deltaTime * 8.5f;
			frame %= bub.sprWalkingAnim.Length;
			bub.renderer.sprite = bub.sprWalkingAnim[Mathf.FloorToInt(frame)];
		}
	}

	internal class Bubbly_NavigateToASpot(Bubbly bub) : Bubbly_WalkingStateBase(bub)
	{
		public override void Enter()
		{
			base.Enter();
			bub.TargetRandomSpot();
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			bub.behaviorStateMachine.ChangeState(new Bubbly_SpawnBubbles(bub));
		}
	}

	internal class Bubbly_SpawnBubbles(Bubbly bub) : Bubbly_StateBase(bub)
	{
		Vector3 pos;
		float fillUpCooldown = 0f;
		const float minCool = 0.5f, maxCool = 1.5f;
		readonly List<Vector3> dirsToSpit = [];
		Bubble awaitingBubble = null;

		public override void Enter()
		{
			base.Enter();
			bub.renderer.sprite = bub.sprPrepareBub;
			pos = bub.transform.position;
			ChangeNavigationState(new NavigationState_DoNothing(bub, 0));
			Vector3 direction = Direction.North.ToVector3();
			var cell = bub.ec.CellFromPosition(pos);
			var room = cell.room;
			if (cell.open)
			{
				for (int i = 0; i < 8; i++)
				{
					if (bub.ec.CellFromPosition(pos + (direction * 10f)).TileMatches(room))
						dirsToSpit.Add(direction);
					direction = Quaternion.AngleAxis(45, Vector3.up) * direction;
				}
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				if (bub.ec.CellFromPosition(pos + (direction * 10f)).TileMatches(room))
					dirsToSpit.Add(direction);
				direction = Quaternion.AngleAxis(90, Vector3.up) * direction;
			}
		}

		public override void Update()
		{ 
			base.Update();
			if (awaitingBubble)
			{
				if (awaitingBubble.Initialized)
					awaitingBubble = null;
				return;
			}
			if (dirsToSpit.Count == 0 || pos != bub.transform.position)
			{
				bub.behaviorStateMachine.ChangeState(new Bubbly_NavigateToASpot(bub));
				return;
			}

			fillUpCooldown -= bub.TimeScale * Time.deltaTime;
			if (fillUpCooldown < 0f)
			{
				fillUpCooldown += Random.Range(minCool, maxCool);
				int i = Random.Range(0, dirsToSpit.Count);
				awaitingBubble = bub.SpitBubbleAtDirection(dirsToSpit[i]);
				dirsToSpit.RemoveAt(i);
			}
		}


	}
}

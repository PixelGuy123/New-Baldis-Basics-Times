using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.NPCs
{
    public class Bubbly : NPC, INPCPrefab
	{
		public  void SetupPrefab()
		{
			var sprs = this.GetSpriteSheet(3, 3, pixs, "bubblySheet.png");
			spriteRenderer[0].sprite = sprs[0];
			audMan = GetComponent<PropagatedAudioManager>();
			sprWalkingAnim = [..sprs.Take(7)];
			sprPrepareBub = sprs[8];
			renderer = spriteRenderer[0];
			audFillUp = this.GetSound("Bubbly_BubbleSpawn.wav", "Vfx_Bubbly_Fillup", SoundType.Effect, new(1f, 0.345f, 0.886f));

			var bubble = new GameObject("Bubble").AddComponent<Bubble>();
			bubble.gameObject.ConvertToPrefab(true);
			bubble.audPop = BBTimesManager.man.Get<SoundObject>("audPop");
			bubble.audMan = bubble.gameObject.CreatePropagatedAudioManager(85, 105);

			var visual = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(16f, "bubble.png")).AddSpriteHolder(out var bubbleVisual, 0f, 0);
			visual.transform.SetParent(bubble.transform);
			visual.transform.localPosition = Vector3.zero;
			visual.gameObject.AddComponent<BillboardRotator>().invertFace = true;

			bubbleVisual.transform.localPosition = Vector3.forward * 0.5f;

			bubble.renderer = bubbleVisual;
			bubble.gameObject.layer = LayerStorage.standardEntities;
			bubble.entity = bubble.gameObject.CreateEntity(1f, 4f, visual.transform);
			bubble.entity.SetGrounded(false);
			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(bubble.transform);
			ObjectCreationExtensions.CreateImage(canvas, TextureExtensions.CreateSolidTexture(1, 1, new(0f, 0.5f, 0.5f, 0.35f)));
			bubble.bubbleCanvas = canvas;
			canvas.gameObject.SetActive(false);

			bubPre = bubble;
		}
		public void SetupPrefabPost() { }

		const float pixs = 21f;
		public string Name { get; set; } public string Category => "npcs";
		
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }





		// prefab ^^
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
			bubbles.Add(b);

			for (int i = 0; i < bubbles.Count; i++)
				if (!bubbles[i])
					bubbles.RemoveAt(i--); // Bubble is null? Remove it!

			b.Spawn(ec, navigator.Entity, transform.position, dir, Random.Range(16f, 22f));
			StartCoroutine(FillupBubble(b));
			return b;
		}

		public override void Despawn()
		{
			base.Despawn();
			for (int i = 0; i < bubbles.Count; i++)
				bubbles[i]?.Pop();
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
			List<Cell> spotsToGo = new(ec.mainHall.AllTilesNoGarbage(false, true));

			for (int i = 0; i < spotsToGo.Count; i++)
				if (spotsToGo[i] == lastSpotGone || (spotsToGo[i].shape != TileShapeMask.Corner && spotsToGo[i].shape != TileShapeMask.Single)) // Filter to the ones that are corners or singles
					spotsToGo.RemoveAt(i--);

			if (spotsToGo.Count <= 1) // Just one spot is not valid.
			{
				lastSpotGone = null;
				TargetPosition(ec.mainHall.RandomEntitySafeCellNoGarbage().FloorWorldPosition);
				return;
			}
			lastSpotGone = spotsToGo[Random.Range(0, spotsToGo.Count)];
			TargetPosition(lastSpotGone.FloorWorldPosition);
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

		readonly List<Bubble> bubbles = [];

		Cell lastSpotGone = null;
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

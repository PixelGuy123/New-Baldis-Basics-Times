using System.Collections;
using System.Linq;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;


namespace BBTimes.CustomContent.NPCs
{
	public class Glubotrony : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [this.GetSoundNoSub("PrepareWalk.wav", SoundType.Voice),
		this.GetSound("step.wav", "Vfx_Gboy_Walk", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		this.GetSound("GB_There.wav", "Vfx_Gboy_putGlue2", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		this.GetSound("GB_Done.wav", "Vfx_Gboy_putGlue1", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		this.GetSound("GB_PrankingTime.wav", "Vfx_Gboy_PrankTime", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		this.GetSound("GB_Situation.wav", "Vfx_Gboy_Situation", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		this.GetSound("GB_Mischievous.wav", "Vfx_Gboy_Mischiveous", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		this.GetSoundNoSub("glueSplash.wav", SoundType.Voice),
		this.GetSoundNoSub("glueStep.wav", SoundType.Voice)
		];

			audMan = GetComponent<PropagatedAudioManager>();
			stepAudMan = gameObject.CreatePropagatedAudioManager(25f, 90f);
			audPrepareStep = soundObjects[0];
			audStep = soundObjects[1];
			audPutGlue = [soundObjects[2], soundObjects[3]];
			audWander = [soundObjects[4], soundObjects[5], soundObjects[6]];

			Sprite[] storedSprites = [.. this.GetSpriteSheet(8, 1, pixelsPerUnit, "gluebotronyIdle.png"), .. this.GetSpriteSheet(4, 4, pixelsPerUnit, "gluebotronyMoving.png")];
			spriteRenderer[0].sprite = storedSprites[0];

			renderer = spriteRenderer[0].CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Take(8)]),
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Skip(8).Take(8)]),
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Skip(8).Skip(8).Take(8)])
				);

			sprIdle = storedSprites[0];
			sprStep1 = storedSprites[8];
			sprStep2 = storedSprites[16];

			// Glue setup
			var glueRender = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "glue.png"), false).AddSpriteHolder(out var glueRenderer, -4.9f, 0);
			glueRenderer.gameObject.layer = 0;
			glueRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			glueRender.gameObject.ConvertToPrefab(true);

			var glue = glueRender.gameObject.AddComponent<Glue>();
			glue.render = glueRenderer.transform;

			glue.audMan = glue.gameObject.CreatePropagatedAudioManager(45f, 65f).AddStartingAudiosToAudioManager(false, soundObjects[7]);
			glue.audSteppedOn = soundObjects[8];

			glue.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * (LayerStorage.TileBaseOffset / 2), true);

			gluePre = glue;

		}

		const float pixelsPerUnit = 55f;

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			behaviorStateMachine.ChangeState(new GlubotronyState(this));
		}
		public void Step()
		{
			step = !step;
			renderer.targetSprite = step ? sprStep1 : sprStep2;
			stepAudMan.FlushQueue(true);
			stepAudMan.QueueAudio(audPrepareStep);

			StartCoroutine(Walk());
		}
		IEnumerator Walk()
		{
			isWalking = true;

			float stepCool = 0.35f;
			while (stepCool > 0f)
			{
				stepCool -= TimeScale * Time.deltaTime;
				yield return null;
			}
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			stepCool = 0.5f;

			while (stepCool > 0f)
			{
				stepCool -= TimeScale * Time.deltaTime;
				yield return null;
			}
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			isWalking = false;
			renderer.targetSprite = sprIdle;
			stepAudMan.FlushQueue(true);
			stepAudMan.PlaySingle(audStep);
			yield break;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (cooldown <= 0f)
			{
				cooldown += Random.Range(15f, 30f);
				audMan.QueueRandomAudio(audWander);
			}
			cooldown -= TimeScale * Time.deltaTime;
		}

		public void SpillGlue()
		{
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audPutGlue);
			Instantiate(gluePre).Initialize(gameObject, transform.position, 0.08f, ec);
			Directions.ReverseList(navigator.currentDirs);
			behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));
			SetGuilt(3f, "littering");
		}

		[SerializeField]
		internal AnimatedSpriteRotator renderer;

		[SerializeField]
		internal Sprite sprIdle, sprStep1, sprStep2;

		[SerializeField]
		internal PropagatedAudioManager audMan, stepAudMan;

		[SerializeField]
		internal SoundObject audPrepareStep, audStep;

		[SerializeField]
		internal SoundObject[] audWander;

		[SerializeField]
		internal SoundObject[] audPutGlue;

		[SerializeField]
		internal Glue gluePre;

		bool step = false, isWalking = false;
		float cooldown = 15f;
		const float speed = 12f;

		public bool IsWalking => isWalking;
	}

	internal class GlubotronyState(Glubotrony gb) : NpcState(gb)
	{
		readonly Glubotrony gb = gb;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(gb, 0));
		}

		public override void Update()
		{
			base.Update();
			float angle = Vector3.Angle(gb.transform.forward, gb.Navigator.NextPoint - gb.transform.position); // Basically should stop or not to turn
			if (angle <= 5f)
			{
				if (isTurning)
				{
					isTurning = false;
					gb.Entity.ExternalActivity.moveMods.Remove(moveMod);
				}
			}
			else
			{
				if (!isTurning)
				{
					isTurning = true;
					if (!gb.Entity.ExternalActivity.moveMods.Contains(moveMod))
						gb.Entity.ExternalActivity.moveMods.Add(moveMod);
				}
			}

			if (!gb.IsWalking)
			{
				stepCooldown -= gb.TimeScale * Time.deltaTime;
				if (stepCooldown < 0f)
				{
					stepCooldown += 1f;
					gb.Step();
					gb.transform.RotateSmoothlyToNextPoint(gb.Navigator.NextPoint, 10f);
				}
			}

			spillGlueCooldown -= gb.TimeScale * Time.deltaTime;
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (spillGlueCooldown <= 0f && !player.Tagged)
			{
				gb.SpillGlue();
				spillGlueCooldown = 40f;
			}
		}

		float stepCooldown = 1f;

		float spillGlueCooldown = 0f;

		bool isTurning = false;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}

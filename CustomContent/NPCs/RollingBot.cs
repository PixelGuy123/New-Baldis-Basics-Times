using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;



namespace BBTimes.CustomContent.NPCs
{
    public class RollingBot : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [this.GetSound("rol_warning.wav", "Vfx_Rollbot_Warning", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			this.GetSound("rol_error.wav", "Vfx_Rollbot_Error", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			this.GetSoundNoSub("shock.wav", SoundType.Voice),
			this.GetSound("motor.wav", "Sfx_1PR_Motor", SoundType.Voice, new(0.7f, 0.7f, 0.7f))];
			// eletricity creation
			Sprite[] storedSprites = [.. this.GetSpriteSheet(4, 4, 25f, "rollBotSheet.png"), .. this.GetSpriteSheet(2, 2, 25f, "shock.png")];
			Sprite[] anim = [.. storedSprites.Skip(spriteAmount)];
			var eleRender = ObjectCreationExtensions.CreateSpriteBillboard(anim[0], false).AddSpriteHolder(0.1f, 0);
			eleRender.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			eleRender.transform.parent.gameObject.ConvertToPrefab(true);
			eleRender.name = "Sprite";

			var ele = eleRender.transform.parent.gameObject.AddComponent<Eletricity>();
			ele.name = "RollingEletricity";
			var ani = ele.gameObject.AddComponent<AnimationComponent>();
			ani.animation = anim;
			ani.renderer = eleRender;
			ani.speed = 15f;

			ele.ani = ani;

			ele.gameObject.CreatePropagatedAudioManager(5f, 35f).AddStartingAudiosToAudioManager(true, soundObjects[2]);

			ele.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * (LayerStorage.TileBaseOffset / 2), true);


			// npc setup
			audError = soundObjects[1];
			audWarning = soundObjects[0];
			audMan = GetComponent<PropagatedAudioManager>();

			gameObject.CreatePropagatedAudioManager(10f, 115f).AddStartingAudiosToAudioManager(true, soundObjects[3]);

			spriteRenderer[0].CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(spriteAmount, [.. storedSprites.Take(spriteAmount)])
				);
			spriteRenderer[0].sprite = storedSprites[0];

			eletricityPre = ele;
		}

		const int spriteAmount = 16;
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		public Character[] ReplacementNpcs { get; set; }
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = 14f;
			navigator.SetSpeed(14f);
			behaviorStateMachine.ChangeState(new RollingBot_Wandering(this));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			transform.RotateSmoothlyToNextPoint(navigator.NextPoint, 0.7f);
		}

		public override void Despawn()
		{
			base.Despawn();
			while (eletricities.Count != 0)
				DestroyLastEletricity();
			
		}

		internal void AnnounceWarning() =>
			audMan.PlaySingle(audWarning);
		internal void AnnounceError() =>
			audMan.PlaySingle(audError);

		internal void SpawnEletricity(Cell cell)
		{
			var eletricity = Instantiate(eletricityPre);
			eletricity.Initialize(this, cell.FloorWorldPosition, 0.5f);
			eletricities.Add(eletricity.transform);
		}

		internal void DestroyEletricity() =>
			StartCoroutine(EletricityDestroy());
		

		IEnumerator EletricityDestroy()
		{
			
			while (eletricities.Count > 0)
			{
				DestroyLastEletricity();

				float delay = 0.5f;
				while (delay > 0f)
				{
					delay -= TimeScale * Time.deltaTime;
					yield return null;
				}
				yield return null;
			}

			yield break;
		}

		internal void DestroyLastEletricity()
		{
			Destroy(eletricities[0].gameObject);
			eletricities.RemoveAt(0);
		}

		[SerializeField]
		internal SoundObject audError, audWarning;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Eletricity eletricityPre;

		readonly List<Transform> eletricities = [];

		internal int EletricitiesCreated => eletricities.Count;
	}

	internal class RollingBot_StateBase(RollingBot bot) : NpcState(bot)
	{
		protected RollingBot bot = bot;
	}

	internal class RollingBot_Wandering(RollingBot bot) : RollingBot_StateBase(bot)
	{
		float errorCooldown = Random.Range(25f, 40f);
		bool errorAnnounced = false;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(bot, 0));
		}

		public override void Update()
		{
			base.Update();
			errorCooldown -= bot.TimeScale * Time.deltaTime;
			if (errorCooldown <= 10f)
			{
				if (!errorAnnounced)
				{
					bot.AnnounceWarning();
					errorAnnounced = true;
				}
				if (errorCooldown <= 0f)
					bot.behaviorStateMachine.ChangeState(new RollingBot_Error(bot));
			}
		}
	}

	internal class RollingBot_Error(RollingBot bot) : RollingBot_StateBase(bot)
	{
		Cell currentCell = null;
		List<Cell> usedCells = [];

		float cooldown = Random.Range(15f, 30f);

		const int eletricityLimit = 12;
		public override void Initialize()
		{
			base.Initialize();
			bot.audMan.FlushQueue(true);
			bot.AnnounceError();
		}

		public override void Update()
		{
			base.Update();
			var c = bot.ec.CellFromPosition(bot.transform.position);
			if (c != currentCell && !usedCells.Contains(c))
			{
				currentCell = c;
				usedCells.Add(c);
				bot.SpawnEletricity(c);
				if (bot.EletricitiesCreated > eletricityLimit)
				{
					bot.DestroyLastEletricity();
					usedCells.RemoveAt(0);
				}
			}

			cooldown -= bot.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				bot.behaviorStateMachine.ChangeState(new RollingBot_Wandering(bot));
			
		}

		public override void Exit()
		{
			base.Exit();
			bot.DestroyEletricity();
			usedCells = null; // clear it I guess
		}
	}

}

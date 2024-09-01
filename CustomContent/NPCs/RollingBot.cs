using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BBTimes.Manager;



namespace BBTimes.CustomContent.NPCs
{
    public class RollingBot : NPC, INPCPrefab, IItemAcceptor
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [this.GetSound("rol_warning.wav", "Vfx_Rollbot_Warning", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			this.GetSound("rol_error.wav", "Vfx_Rollbot_Error", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			this.GetSound("motor.wav", "Sfx_1PR_Motor", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			this.GetSound("rol_fix.wav", "Vfx_Rollbot_Fix", SoundType.Voice, new(0.7f, 0.7f, 0.7f))];
			Sprite[] storedSprites = this.GetSpriteSheet(4, 4, 25f, "rollBotSheet.png");

			// npc setup
			audError = soundObjects[1];
			audWarning = soundObjects[0];
			audThanks = soundObjects[3];
			audMan = GetComponent<PropagatedAudioManager>();

			gameObject.CreatePropagatedAudioManager(10f, 115f).AddStartingAudiosToAudioManager(true, soundObjects[2]);

			spriteRenderer[0].CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(storedSprites.Length, storedSprites)
				);
			spriteRenderer[0].sprite = storedSprites[0];

			eletricityPre = BBTimesManager.man.Get<Eletricity>("EletricityPrefab");
		}
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
			eletricity.Initialize(gameObject, cell.FloorWorldPosition, 0.5f, ec);
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
		public bool ItemFits(Items itm)
		{
			if (behaviorStateMachine.CurrentState is RollingBot_Error col && col.cooldown > 0f && itemsThatFixMe.Contains(itm))
				return true;
			return false;
		}

		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			if (behaviorStateMachine.CurrentState is RollingBot_Error col)
			{
				col.cooldown = -1f;
				audMan.PlaySingle(audThanks);
			}
		}

		[SerializeField]
		internal SoundObject audError, audWarning, audThanks;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Eletricity eletricityPre;

		readonly List<Transform> eletricities = [];

		internal int EletricitiesCreated => eletricities.Count;

		readonly static HashSet<Items> itemsThatFixMe = [];

		public static void AddFixableItem(Items i) => itemsThatFixMe.Add(i);
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

		internal float cooldown = Random.Range(15f, 30f);

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

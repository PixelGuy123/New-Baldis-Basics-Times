using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using TMPro;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Penny : NPC, INPCPrefab, IClickable<int>
	{
		public void SetupPrefab()
		{
			renderer = spriteRenderer[0];
			audMan = GetComponent<PropagatedAudioManager>();
			stepAudMan = gameObject.CreatePropagatedAudioManager(115, 145);

			this.CreateClickableLink()
				.CopyColliderAttributes((CapsuleCollider)baseTrigger[0]);

			var sprites = this.GetSpriteSheet(4, 3, 34f, "penny.png").ExcludeNumOfSpritesFromSheet(1);
			talkHappySprs = sprites.TakeAPair(0, 2);
			talkSadSprs = sprites.TakeAPair(2, 2);
			madTalkSprs = sprites.TakeAPair(4, 2);
			screamSprs = [sprites[6]];
			idleWalkSprs = sprites.TakeAPair(7, 2);
			madWalkSprs = sprites.TakeAPair(9, 2);

			spriteRenderer[0].sprite = talkHappySprs[0];

			animator = gameObject.AddComponent<SpriteVolumeAnimator>();
			animator.audMan = audMan;
			animator.renderer = spriteRenderer[0];
			animator.sprites = talkHappySprs;

			audSteps = [this.GetSound("highHeels0.wav", "Vfx_Spj_Step", SoundType.Effect, new(1f, 0.15f, 0f)), this.GetSound("highHeels1.wav", "Vfx_Spj_Step", SoundType.Voice, new(1f, 0.15f, 0f))];
			audSpellTheWord = this.GetSound("SpellTheWord.wav", "Vfx_Pen_Spell", SoundType.Voice, new(1f, 0.15f, 0f));
			audAngrySpellTheWord = this.GetSound("Punishment.wav", "Vfx_Pen_AngrySpell", SoundType.Voice, new(1f, 0.15f, 0f));
			audAngryWarning = [
				this.GetSound("Warning1.wav", "Vfx_Pen_Warning1", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Warning2.wav", "Vfx_Pen_Warning2", SoundType.Voice, new(1f, 0.15f, 0f))
				];
			audIncorrectLetterChoice =
				[
				this.GetSound("Incorrect1.wav", "Vfx_Pen_Incorrect1", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Incorrect2.wav", "Vfx_Pen_Incorrect2", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Incorrect3.wav", "Vfx_Pen_Incorrect3", SoundType.Voice, new(1f, 0.15f, 0f))
				];
			audNoticeChase = [
				this.GetSound("Intro1.wav", "Vfx_Pen_Intro1", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Intro2.wav", "Vfx_Pen_Intro2", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Intro3.wav", "Vfx_Pen_Intro3", SoundType.Voice, new(1f, 0.15f, 0f))
				];
			audWinPrize = [
				this.GetSound("Win1.wav", "Vfx_Pen_Win1", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Win2.wav", "Vfx_Pen_Win2", SoundType.Voice, new(1f, 0.15f, 0f)),
				this.GetSound("Win3.wav", "Vfx_Pen_Win3", SoundType.Voice, new(1f, 0.15f, 0f))
				];

			// ------------- Separation for a big array initialization -------------
			easyWords = [
				"Bald",
				"Camp",
				"Cat",
				"Cheese",
				"Class",
				"Dog",
				"Gum",
				"House",
				"Read",
				"Rules"
				];

			hardWords = [
				"Cafeteria",
				"Cemetery",
				"Detention",
				"Education",
				"Emergency",
				"Glubotrony",
				"Language",
				"Mathematics",
				"PhaWillow",
				"Technology"
				];

			SoundObject[] sds = new SoundObject[easyWords.Length];
			for (int i = 0; i < sds.Length; i++)
				sds[i] = this.GetSound($"{easyWords[i]}.wav", HideWords(easyWords[i]), SoundType.Voice, new(1f, 0.15f, 0f));

			easyWordSoundPair = sds;

			sds = new SoundObject[hardWords.Length];
			for (int i = 0; i < sds.Length; i++)
				sds[i] = this.GetSound($"{hardWords[i]}.wav", HideWords(hardWords[i]), SoundType.Voice, new(1f, 0.15f, 0f));
			hardWordSoundPair = sds;

			// ----------------- Done with words -------------------

			audCorrectRing = BBTimesManager.man.Get<SoundObject>("audRing");
			audBuzz = BBTimesManager.man.Get<SoundObject>("audBuzz");

			// Letter fabrication
			var textHolder = new GameObject("TextHolder").AddComponent<BillboardRotator>();
			textHolder.transform.SetParent(transform);
			textHolder.transform.localPosition = Vector3.zero;
			textHolder.gameObject.layer = LayerMask.NameToLayer("NPCs");

			aboveText = new GameObject("AboveText", typeof(BillboardRotator)).AddComponent<TextMeshPro>();
			aboveText.alignment = TextAlignmentOptions.Center;
			aboveText.autoSizeTextContainer = true;
			aboveText.transform.SetParent(textHolder.transform);
			aboveText.transform.localPosition = Vector3.up * 4f;
			aboveText.gameObject.layer = LayerStorage.billboardLayer;

			letters = [ConstructLetter("Letter01", Vector3.left * 3f), ConstructLetter("Letter02", Vector3.right * 3f)];


			FloatingLetter ConstructLetter(string name, Vector3 offset)
			{
				var let = new GameObject(name).AddComponent<FloatingLetter>();
				let.transform.SetParent(textHolder.transform);
				let.transform.localPosition = offset;

				let.renderer = new GameObject($"{name}_renderer", typeof(BillboardRotator)).AddComponent<TextMeshPro>();
				let.renderer.gameObject.layer = LayerStorage.billboardLayer;
				let.renderer.autoSizeTextContainer = true;
				let.renderer.alignment = TextAlignmentOptions.Center;
				let.renderer.transform.SetParent(let.transform);
				let.renderer.transform.localPosition = Vector3.zero;

				let.gameObject.AddComponent<SphereCollider>().radius = 0.8f;
				let.gameObject.layer = LayerMask.NameToLayer("HotSpot");
				let.gameObject.AddStaticRigidBody();

				let.gameObject.SetActive(false);

				return let;
			}

			string HideWords(string str)
			{
				char[] newStr = new char[str.Length];
				for (int i = 0; i < str.Length; i++)
					newStr[i] = i % 2 == 0 ? str[i] : '_';
				return new string(newStr);
			}
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }


		[SerializeField]
		internal AudioManager audMan, stepAudMan;

		[SerializeField]
		internal string[] easyWords, hardWords;

		[SerializeField]
		internal SoundObject[] easyWordSoundPair, hardWordSoundPair;

		[SerializeField]
		internal SoundObject[] audSteps, audNoticeChase, audIncorrectLetterChoice, audAngryWarning, audWinPrize;

		[SerializeField]
		internal SoundObject audSpellTheWord, audAngrySpellTheWord, audCorrectRing, audBuzz;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] talkHappySprs, talkSadSprs, madTalkSprs, screamSprs, idleWalkSprs, madWalkSprs;

		[SerializeField]
		internal FloatingLetter[] letters;

		[SerializeField]
		internal TextMeshPro aboveText;

		[SerializeField]
		internal SpriteVolumeAnimator animator;

		internal readonly MovementModifier moveMod = new(Vector3.zero, 1f);

		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new Penny_Wandering(this));
			for (int i = 0; i < letters.Length; i++)
				letters[i].Initialize(this, ec);

			navigator.Entity.OnTeleport += Teleport;
		}

		public void NormalSpeed()
		{
			navigator.maxSpeed = angry ? 19f : 12f;
			navigator.SetSpeed(navigator.maxSpeed);
		}

		public void NormalChaseSpeed()
		{
			navigator.maxSpeed = angry ? 25f : 18f;
			navigator.SetSpeed(navigator.maxSpeed);
		}

		public void CalloutPlayer() =>
			audMan.PlayRandomAudio(audNoticeChase);

		public void ScreamOnPlayer()
		{
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audAngryWarning);
		}

		public void SetIdleOnMood(byte state)
		{
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);

			animator.sprites = state switch
			{
				1 => talkSadSprs,
				2 => madTalkSprs,
				3 => screamSprs,
				_ => talkHappySprs
			};
			renderer.sprite = animator.sprites[0];
			// 0 => normal, 1 => angry, 2 => screaming
		}

		public void TakeLetter(char c)
		{
			if (c == chosenWord[wordIndex])
			{
				formingWord += c;
				aboveText.text = formingWord;
				wordIndex++;
				if (wordIndex >= chosenWord.Length)
				{
					audMan.FlushQueue(true);
					audMan.PlayRandomAudio(audWinPrize);
					Singleton<CoreGameManager>.Instance.AddPoints(guesses * 25, chosenPlayer.playerNumber, true);
					SetAngry(false);
					SetIdleOnMood(0);
					moveMod.movementMultiplier = 1f;
					behaviorStateMachine.ChangeState(new Penny_Wandering(this, 60f));
					StartCoroutine(FadeAboveTextOut());

					return;
				}

				ScrambleLetters(chosenWord[wordIndex]);
				stepAudMan.PlaySingle(audCorrectRing);
				if (!angry)
				{
					SetIdleOnMood(0);
				}
				return;
			}
			stepAudMan.PlaySingle(audBuzz);
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audIncorrectLetterChoice);
			SetIdleOnMood(1);

			if (--guesses <= 1)
				guesses = 1;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (Time.timeScale > 0f && !stopStep && Navigator.maxSpeed > 0f && Navigator.Velocity.magnitude > 0.1f * Time.deltaTime)
			{
				stepDelay -= Navigator.Velocity.magnitude;
				if (stepDelay <= 0f)
				{
					stepAudMan.PlaySingle(audSteps[step ? 1 : 0]);
					step = !step;
					renderer.sprite = step ? (angry ? madWalkSprs[1] : idleWalkSprs[1]) : (angry ? madWalkSprs[0] : idleWalkSprs[0]);
					stepDelay += stepMax;
				}
			}
			stopStep = false;
		}

		public void MakePunishmentWorse() =>
			moveMod.movementMultiplier *= 0.5f;

		void Teleport(Vector3 v) =>
			stopStep = true;

		bool stopStep = false;

		public void SetAngry(bool angry) =>
			this.angry = angry;

		/* -------------------------------- MINI GAME --------------------------------*/
		public void InitiateMinigame(PlayerManager pm)
		{
			minigame = StartCoroutine(Minigame());
			chosenPlayer = pm;
		}
		public void StopMinigame()
		{
			if (minigame != null)
				StopCoroutine(minigame);
			chosenPlayer = null;
			currentWordIndex = -1;
			EnableLetters(false);
		}
		public void HideAboveText() => aboveText.gameObject.SetActive(false);
		IEnumerator Minigame()
		{
			audMan.FlushQueue(true);
			if (angry)
				SetIdleOnMood(2);
			audMan.QueueAudio(angry ? audAngrySpellTheWord : audSpellTheWord);

			while (audMan.QueuedAudioIsPlaying) yield return null;

			int idx = angry ? Random.Range(0, hardWords.Length) : Random.Range(0, easyWords.Length);
			audMan.QueueAudio(angry ? hardWordSoundPair[idx] : easyWordSoundPair[idx]);
			currentWordIndex = idx;
			chosenWord = angry ? hardWords[idx] : easyWords[idx];
			formingWord = string.Empty;
			wordIndex = 0;

			aboveText.text = string.Empty;
			aboveText.gameObject.SetActive(true);

			EnableLetters(true);
			ScrambleLetters(chosenWord[wordIndex]);
			guesses = 5;
		}

		IEnumerator FadeAboveTextOut()
		{
			Vector3 ogScale = aboveText.transform.localScale;
			float scale = ogScale.magnitude;
			float scaleVel = 1.2f;

			while (true)
			{
				scaleVel -= 13f * ec.EnvironmentTimeScale * Time.deltaTime;
				scale += 0.15f * scaleVel * Time.timeScale;
				if (scale <= 0f)
				{
					HideAboveText();
					aboveText.transform.localScale = ogScale;
					yield break;
				}
				aboveText.transform.localScale = Vector3.one * scale;
				yield return null;
			}
		}

		void EnableLetters(bool active)
		{
			for (int i = 0; i < letters.Length; i++)
				letters[i].gameObject.SetActive(active);
		}

		void ScrambleLetters(char expectedChar)
		{
			int i = Random.Range(0, letters.Length);
			letters[i].PickLetter(expectedChar);

			var normalizedChar = char.ToLower(expectedChar);

			for (int z = 0; z < letters.Length; z++)
			{
				if (z != i)
				{
					char c;
					do
					{
						c = allowedCharacters[Random.Range(0, allowedCharacters.Length)];
					} while (c == normalizedChar);

					letters[z].PickLetter(char.IsLower(expectedChar) ? c : char.ToUpper(c));
				}
			}
		}

		public bool ClickableRequiresNormalHeight() => false;
		public bool ClickableHidden() => currentWordIndex == -1;
		public void Clicked(int player)
		{
			if (currentWordIndex != -1 && Singleton<CoreGameManager>.Instance.GetPlayer(player) == chosenPlayer)
			{
				audMan.FlushQueue(true);
				audMan.QueueAudio(angry ? hardWordSoundPair[currentWordIndex] : easyWordSoundPair[currentWordIndex]);
			}
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }

		PlayerManager chosenPlayer;
		Coroutine minigame;
		bool angry = false, step = false;

		public bool IsAngry => angry;

		private string formingWord;
		private string chosenWord;
		int wordIndex = 0, guesses = 0, currentWordIndex = -1;

		public string FormingWord => formingWord;
		public string SelectedWord => chosenWord;

		float stepDelay = 0f;
		const float stepMax = 8f;
		readonly char[] allowedCharacters = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
	}

	internal class Penny_StateBase(Penny pen) : NpcState(pen)
	{
		protected Penny pen = pen;

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
	}

	internal class Penny_Wandering(Penny pen, float cooldown = 0f, float calmDownCooldown = 0f, PlayerManager target = null) : Penny_StateBase(pen)
	{
		float cooldown = cooldown, calmDownCooldown = calmDownCooldown;
		readonly PlayerManager target = target;
		public override void Enter()
		{
			base.Enter();
			pen.NormalSpeed();
			ChangeNavigationState(new NavigationState_WanderRandom(pen, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (cooldown <= 0f && !player.Tagged && (target == null || player == target))
				pen.behaviorStateMachine.ChangeState(new Penny_NoticeChase(pen, player, this));

		}

		public override void Update()
		{
			base.Update();
			if (cooldown > 0f)
				cooldown -= pen.TimeScale * Time.deltaTime;
			if (calmDownCooldown > 0f)
			{
				calmDownCooldown -= pen.TimeScale * Time.deltaTime;
				if (calmDownCooldown <= 0f && pen.IsAngry)
				{
					pen.SetAngry(false);
					pen.NormalSpeed();
				}
			}
		}
	}

	internal class Penny_NoticeChase(Penny pen, PlayerManager pm, Penny_StateBase prevState) : Penny_StateBase(pen)
	{
		readonly PlayerManager pm = pm;
		NavigationState_TargetPlayer player;
		readonly Penny_StateBase state = prevState;
		public override void Enter()
		{
			base.Enter();
			pen.NormalChaseSpeed();
			if (!pen.IsAngry)
				pen.CalloutPlayer();
			player = new(pen, 63, pm.transform.position);
			ChangeNavigationState(player);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			player.priority = 0;
			pen.behaviorStateMachine.ChangeState(state);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (!pm.Tagged && player == pm)
				this.player.UpdatePosition(player.transform.position);

		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == pm.gameObject && !pm.Tagged)
				pen.behaviorStateMachine.ChangeState(new Penny_ClassTime(pen, pm));
		}
	}

	internal class Penny_ClassTime(Penny pen, PlayerManager pm) : Penny_StateBase(pen)
	{
		readonly PlayerManager pm = pm;

		public override void Enter()
		{
			base.Enter();
			pen.SetIdleOnMood(0);
			if (pen.IsAngry)
			{
				pen.MakePunishmentWorse();
				pm.Am.moveMods.Add(pen.moveMod);
			}
			pen.InitiateMinigame(pm);
		}

		public override void Update()
		{
			base.Update();
			if (Vector3.Distance(pen.transform.position, pm.transform.position) >= 25f)
				pen.behaviorStateMachine.ChangeState(new Penny_Scream(pen, pm));
		}

		public override void Exit()
		{
			base.Exit();
			pm.Am.moveMods.Remove(pen.moveMod);
			pen.StopMinigame();
		}
	}

	internal class Penny_Scream(Penny pen, PlayerManager pm) : Penny_StateBase(pen)
	{
		readonly PlayerManager pm = pm;

		public override void Enter()
		{
			base.Enter();
			pen.SetIdleOnMood(3);
			pen.ScreamOnPlayer();
			pen.SetAngry(true);
			pen.HideAboveText();
		}

		public override void Update()
		{
			base.Update();
			if (!pen.audMan.AnyAudioIsPlaying)
			{
				pen.SetIdleOnMood(2);
				pen.behaviorStateMachine.ChangeState(new Penny_NoticeChase(pen, pm, new Penny_Wandering(pen, calmDownCooldown: 120f, target: pm)));
			}
		}
	}


}

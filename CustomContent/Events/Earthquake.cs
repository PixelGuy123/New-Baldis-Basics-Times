using UnityEngine;
using System.Collections.Generic;
using MTM101BaldAPI.Registers;
using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.Events
{
    public class Earthquake : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("Bal_earthquake.wav", "Event_Earthquake1", SoundType.Voice, Color.green);
			eventIntro.additionalKeys = [new() { key = "Event_Earthquake2", time = 5.661f }];

			// Particles
			var flipperParticle = new GameObject("Earthquake", typeof(ParticleSystem)); // Copypaste from BB+ Animations
			flipperParticle.ConvertToPrefab(true);

			var mat = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = this.GetTexture("shakeness.png") };
			flipperParticle.GetComponent<ParticleSystemRenderer>().material = mat;

			var particleSystem = flipperParticle.GetComponent<ParticleSystem>();
			var anim = particleSystem.textureSheetAnimation;
			anim.enabled = true;
			anim.numTilesX = 1;
			anim.numTilesY = 8;
			anim.animation = ParticleSystemAnimationType.WholeSheet;
			anim.mode = ParticleSystemAnimationMode.Grid;
			anim.cycleCount = 1;
			anim.timeMode = ParticleSystemAnimationTimeMode.FPS;
			anim.fps = 11f;

			var main = particleSystem.main;
			main.gravityModifierMultiplier = 0f;
			main.startLifetimeMultiplier = 0.8f;
			main.startSpeedMultiplier = 0f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = 9f;

			var emission = particleSystem.emission;
			emission.rateOverTimeMultiplier = 16f;

			partPre = particleSystem;
			audMan = gameObject.CreateAudioManager(55f, 65f).MakeAudioManagerNonPositional();
			audTrembling = this.GetSoundNoSub("earthQuakeGoing.wav", SoundType.Effect);
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void PremadeSetup()
		{
			base.PremadeSetup();
			spots.AddRange(ec.AllCells());
		}
		public override void AfterUpdateSetup(System.Random rng)
		{
			base.AfterUpdateSetup(rng);
			spots.AddRange(ec.AllCells());
		}

		public override void Begin()
		{
			base.Begin();

			audMan.QueueAudio(audTrembling);
			audMan.SetLoop(true);
			int am = (int)((ec.levelSize.x + ec.levelSize.z) * 0.5f * particleReduceFactor);
			for (int i = 0; i < am; i++)
				particles.Add(Instantiate(partPre));

			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i].Navigator.isActiveAndEnabled && ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
				{
					var moveMod = new MovementModifier(Vector3.zero, 1f) { forceTrigger = true, ignoreAirborne = true };
					ec.Npcs[i].Navigator.Am.moveMods.Add(moveMod);
					actMods.Add(new(ec.Npcs[i].Navigator.Am, moveMod));
				}
			}

			for (int i = 0; i < ec.Players.Length; i++)
			{
				if (ec.Players[i] != null)
				{
					var moveMod = new MovementModifier(Vector3.zero, 1f) { forceTrigger = true, ignoreAirborne = true };
					ec.Players[i].Am.moveMods.Add(moveMod);
					actMods.Add(new(ec.Players[i].Am, moveMod));
				}
			}
		}

		void Update()
		{
			if (!active) return;

			float strengthConstant = Mathf.Abs(Mathf.Sin(Time.fixedTime * ec.EnvironmentTimeScale * strengthTremblingFactor));

			delay -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (delay <= 0f)
			{
				delay += tremblingFrameDelay;
				for (int i = 0; i < actMods.Count; i++)
					actMods[i].Value.movementAddend = new(crng.Next(-shakeStrength, shakeStrength) * strengthConstant, 0f, crng.Next(-shakeStrength, shakeStrength) * strengthConstant);
				
			}

			for (int i = 0; i < particles.Count; i++)
				particles[i].transform.position = spots[crng.Next(0, spots.Count)].FloorWorldPosition + Vector3.up * 3.2f;
		}

		public override void End()
		{
			base.End();
			audMan.FadeOut(5f);
			while (actMods.Count != 0)
			{
				actMods[0].Key.moveMods.Remove(actMods[0].Value);
				actMods.RemoveAt(0);
			}
			while (particles.Count != 0)
			{
				Destroy(particles[0].gameObject);
				particles.RemoveAt(0);
			}
		}

		readonly List<ParticleSystem> particles = [];

		readonly List<KeyValuePair<ActivityModifier, MovementModifier>> actMods = [];

		readonly List<Cell> spots = [];

		float delay = 0f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audTrembling;

		[SerializeField]
		internal ParticleSystem partPre;

		[SerializeField]
		internal float tremblingFrameDelay = 0.08f;

		[SerializeField]
		internal int shakeStrength = 42;

		[SerializeField]
		[Range(0.0f, 1.0f)]
		internal float particleReduceFactor = 0.85f, strengthTremblingFactor = 0.45f;
	}
}

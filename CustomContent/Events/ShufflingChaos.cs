using BBTimes.Extensions;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
    public class ShufflingChaos : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			var tex = this.GetTexture("teleportParticle.png");
			eventIntro = this.GetSound("Bal_shuffleChaos.wav", "Event_ShuffleChaos1", SoundType.Voice, Color.green);
			eventIntro.additionalKeys = [new() { time = 4.68f, key = "Event_ShuffleChaos2" }];

			var entShuf = new GameObject("EntityShuffler").AddComponent<EntityShuffler>();
			entShuf.gameObject.ConvertToPrefab(true);
			entShuf.audMan = entShuf.gameObject.CreatePropagatedAudioManager(15f, 55f);
			entShuf.audPrep = this.GetSoundNoSub("teleporting.wav", SoundType.Effect);
			entShuf.audTel = BBTimesManager.man.Get<SoundObject>("teleportAud");

			entShuf.parts = SetupParts(entShuf.gameObject.AddComponent<ParticleSystem>());
			entShufPre = entShuf;

			var picShuf = new GameObject("PickupShuffler").AddComponent<PickupShuffler>();
			picShuf.gameObject.ConvertToPrefab(true);
			picShuf.audMan = picShuf.gameObject.CreatePropagatedAudioManager(15f, 45f);
			picShuf.audPrep = entShuf.audPrep;
			picShuf.audTel = entShuf.audTel;

			picShuf.parts = SetupParts(picShuf.gameObject.AddComponent<ParticleSystem>());
			pickShufPre = picShuf;


			ParticleSystem SetupParts(ParticleSystem system)
			{
				system.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = tex };
				var main = system.main;
				main.gravityModifierMultiplier = 0f;
				main.startLifetimeMultiplier = 0.8f;
				main.startSpeedMultiplier = 2f;
				main.simulationSpace = ParticleSystemSimulationSpace.World;
				main.startSize = new(1f, 2f);

				var emission = system.emission;
				emission.rateOverTimeMultiplier = 8f;

				var vel = system.velocityOverLifetime;
				vel.enabled = true;
				vel.space = ParticleSystemSimulationSpace.World;
				vel.x = new(-13f, 13f);
				vel.y = new(-13f, 13f);
				vel.z = new(-13f, 13f);

				return system;
			}
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void Begin()
		{
			base.Begin();
			telCooldown = eventIntro.soundClip.length + 0.15f; // Yeah, let Baldo talk

			foreach (var npc in ec.Npcs)
				if (npc.Navigator.isActiveAndEnabled && npc.GetMeta().flags.HasFlag(NPCFlags.Standard))
					entities.Add(npc.Navigator.Entity);

			foreach (var player in ec.Players)
				if (player != null)
					entities.Add(player.plm.Entity);

			foreach (var item in ec.items)
				if (item.free && item.item.itemType != Items.None)
					pickups.Add(item);
		}

		void Update()
		{
			if (!active) return;

			telCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (telCooldown <= 0f)
			{
				telCooldown += Random.Range(minWaitDelay, maxWaitDelay);
				for (int i = 0; i < entities.Count; i++)
				{
					if (entities[i])
					{
						var t = Instantiate(entShufPre);
						t.Initialize(entities[i], ec, Random.Range(minTeleportDelay, maxTeleportDelay), [.. entities]);
					}
					else entities.RemoveAt(i--);
				}
				for (int i = 0; i < pickups.Count; i++)
				{
					if (pickups[i].item.itemType != Items.None)
					{
						var t = Instantiate(pickShufPre);
						t.Initialize(pickups[i], ec, Random.Range(minTeleportDelay, maxTeleportDelay), [.. pickups]);
					}
					else pickups.RemoveAt(i--);
				}
			}
		}

		public override void End()
		{
			base.End();
			entities.Clear();
			pickups.Clear();
		}

		readonly List<Entity> entities = [];

		readonly List<Pickup> pickups = [];

		float telCooldown;

		[SerializeField]
		internal float minWaitDelay = 15f, maxWaitDelay = 20f, minTeleportDelay = 4f, maxTeleportDelay = 6f;

		[SerializeField]
		internal PickupShuffler pickShufPre;

		[SerializeField]
		internal EntityShuffler entShufPre;
		
	}
}

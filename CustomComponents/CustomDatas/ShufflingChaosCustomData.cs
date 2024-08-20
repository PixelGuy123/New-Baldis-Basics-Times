using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.CustomContent.Events;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ShufflingChaosCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [GetSound("ShuffleBal.wav", "Event_ShuffleChaos1", SoundType.Effect, Color.green), GetSoundNoSub("teleporting.wav", SoundType.Voice), BBTimesManager.man.Get<SoundObject>("teleportAud")];
			sds[0].additionalKeys = [new() { time = 1.434f, key = "Event_ShuffleChaos2" }, new() { time = 6.12f, key = "Event_ShuffleChaos3" }];

			return sds;
		}
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var tex = GetTexture("teleportParticle.png");
			var ev = GetComponent<ShufflingChaos>();
			ev.eventIntro = soundObjects[0];

			var entShuf = new GameObject("EntityShuffler").AddComponent<EntityShuffler>();
			entShuf.gameObject.ConvertToPrefab(true);
			entShuf.audMan = entShuf.gameObject.CreatePropagatedAudioManager(15f, 55f);
			entShuf.audPrep = soundObjects[1];
			entShuf.audTel = soundObjects[2];

			entShuf.parts = SetupParts(entShuf.gameObject.AddComponent<ParticleSystem>());
			ev.entShufPre = entShuf;

			var picShuf = new GameObject("PickupShuffler").AddComponent<PickupShuffler>();
			picShuf.gameObject.ConvertToPrefab(true);
			picShuf.audMan = picShuf.gameObject.CreatePropagatedAudioManager(15f, 45f);
			picShuf.audPrep = soundObjects[1];
			picShuf.audTel = soundObjects[2];

			picShuf.parts = SetupParts(picShuf.gameObject.AddComponent<ParticleSystem>());
			ev.pickShufPre = picShuf;


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
	}
}

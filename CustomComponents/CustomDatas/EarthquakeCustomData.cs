using BBTimes.CustomContent.Events;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class EarthquakeCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [GetSound("Earthquake.wav", "Event_Earthquake1", SoundType.Effect, Color.green), GetSoundNoSub("earthQuakeGoing.wav", SoundType.Effect)];
			sds[0].additionalKeys = [new() { key = "Event_Earthquake2", time = 2.199f }, new() { key = "Event_Earthquake3", time = 6.789f }];

			return sds;
		}
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var ev = GetComponent<Earthquake>();
			ev.eventIntro = soundObjects[0];

			// Particles
			var flipperParticle = new GameObject("Earthquake", typeof(ParticleSystem)); // Copypaste from BB+ Animations
			flipperParticle.ConvertToPrefab(true);

			var mat = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = GetTexture("shakeness.png") };
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

			ev.partPre = particleSystem;
			ev.audMan = gameObject.CreateAudioManager(55f, 65f).MakeAudioManagerNonPositional();
			ev.audTrembling = soundObjects[1];
		}
	}
}

using UnityEngine;
using System.Collections.Generic;
using MTM101BaldAPI.Registers;

namespace BBTimes.CustomContent.Events
{
	public class Earthquake : RandomEvent
	{
		public override void PremadeSetup()
		{
			base.PremadeSetup();
			spots.AddRange(ec.AllCells());
		}
		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup();
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
					var moveMod = new MovementModifier(Vector3.zero, 1f) { forceTrigger = true };
					ec.Npcs[i].Navigator.Am.moveMods.Add(moveMod);
					actMods.Add(new(ec.Npcs[i].Navigator.Am, moveMod));
				}
			}

			for (int i = 0; i < ec.Players.Length; i++)
			{
				if (ec.Players[i] != null)
				{
					var moveMod = new MovementModifier(Vector3.zero, 1f) { forceTrigger = true };
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
					actMods[i].Value.movementAddend = new(Random.Range(-shakeStrength, shakeStrength) * strengthConstant, 0f, Random.Range(-shakeStrength, shakeStrength) * strengthConstant);
				
			}

			for (int i = 0; i < particles.Count; i++)
				particles[i].transform.position = spots[Random.Range(0, spots.Count)].FloorWorldPosition + Vector3.up * 3.2f;
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
		internal float shakeStrength = 42f, tremblingFrameDelay = 0.08f;

		[SerializeField]
		[Range(0.0f, 1.0f)]
		internal float particleReduceFactor = 0.85f, strengthTremblingFactor = 0.45f;
	}
}

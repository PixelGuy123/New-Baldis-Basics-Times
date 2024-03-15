using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ClassicGottaSweepCustomData : CustomNPCData
	{
		public override void SetupPrefab()
		{
			var sweep = GetComponent<GottaSweep>();
			if (sweep == null) return;

			maxActive.SetValue(sweep, 60f);
			minActive.SetValue(sweep, 35f);
			minDelay.SetValue(sweep, 45f);
			maxDelay.SetValue(sweep, 60f);
			speed.SetValue(sweep, 75f);
			moveMod.SetValue(sweep, new MovementModifier(Vector3.zero, 0.5f));
			moveModMultiplier.SetValue(sweep, 1f);
		}

		protected override void Start()
		{
			base.Start();
			GetComponent<AudioManager>().audioDevice.dopplerLevel = 2f; // Sweep always gotta have this lol
		}

		// FieldInfos
		static readonly FieldInfo maxActive = AccessTools.Field(typeof(GottaSweep), "maxActive");
		static readonly FieldInfo minActive = AccessTools.Field(typeof(GottaSweep), "minActive");
		static readonly FieldInfo minDelay = AccessTools.Field(typeof(GottaSweep), "minDelay");
		static readonly FieldInfo maxDelay = AccessTools.Field(typeof(GottaSweep), "maxDelay");
		static readonly FieldInfo speed = AccessTools.Field(typeof(GottaSweep), "speed");
		static readonly FieldInfo moveMod = AccessTools.Field(typeof(GottaSweep), "moveMod");
		static readonly FieldInfo moveModMultiplier = AccessTools.Field(typeof(GottaSweep), "moveModMultiplier");
	}
}

// using System.Reflection;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ClassicGottaSweepCustomData : CustomNPCData
	{
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(26f, "oldsweep.png")];
		public override void SetupPrefab()
		{
			var sweep = (GottaSweep)Npc;
			if (sweep == null) return;

			//maxActive.SetValue(sweep, 60f);
			sweep.maxActive = 60f;
			//minActive.SetValue(sweep, 35f);
			sweep.minActive = 35f;
			//minDelay.SetValue(sweep, 45f);
			sweep.minDelay = 45f;
			//maxDelay.SetValue(sweep, 60f);
			sweep.maxDelay = 60f;
			//speed.SetValue(sweep, 75f);
			sweep.speed = 75f;
			//moveMod.SetValue(sweep, new MovementModifier(Vector3.zero, 0.5f));
			sweep.moveMod = new(Vector3.zero, 0.5f);
			//moveModMultiplier.SetValue(sweep, 1f);
			sweep.moveModMultiplier = 1f;

			sweep.audMan.audioDevice.dopplerLevel = 2f; // I wonder why I didn't set it in here
		}
		

		// FieldInfos
		//static readonly FieldInfo maxActive = AccessTools.Field(typeof(GottaSweep), "maxActive");
		//static readonly FieldInfo minActive = AccessTools.Field(typeof(GottaSweep), "minActive");
		//static readonly FieldInfo minDelay = AccessTools.Field(typeof(GottaSweep), "minDelay");
		//static readonly FieldInfo maxDelay = AccessTools.Field(typeof(GottaSweep), "maxDelay");
		//static readonly FieldInfo speed = AccessTools.Field(typeof(GottaSweep), "speed");
		//static readonly FieldInfo moveMod = AccessTools.Field(typeof(GottaSweep), "moveMod");
		//static readonly FieldInfo moveModMultiplier = AccessTools.Field(typeof(GottaSweep), "moveModMultiplier");
	}
}

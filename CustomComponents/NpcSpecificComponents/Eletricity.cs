using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class Eletricity : GlueObject
	{
		protected override void Initialize()
		{
			base.Initialize();
			ani.Initialize(ec);
		}

		protected override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (++frameDelay >= 3)
			{
				moveMod.movementAddend = new(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
				frameDelay = 0;
			}
		}
		[SerializeField]
		internal AnimationComponent ani;
		int frameDelay = 0;
	}
}

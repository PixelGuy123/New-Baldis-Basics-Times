using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class Eletricity : GlueObject<RollingBot>
	{
		protected override void Initialize()
		{
			base.Initialize();
			ani.Initialize(owner.ec);
		}

		[SerializeField]
		internal AnimationComponent ani;
	}
}

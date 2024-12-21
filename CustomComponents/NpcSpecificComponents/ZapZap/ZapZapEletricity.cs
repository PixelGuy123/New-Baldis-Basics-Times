using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ZapZap
{
	public class ZapZapEletricity : Eletricity
	{
		protected override void ActivityEnter(ActivityModifier actMod)
		{
			base.ActivityEnter(actMod);
			if (eles.Exists(el => el.Overrider == actMod))
				return;

			var ele = Instantiate(compPre);
			ele.AttachTo(actMod, ec, this);
			eles.Add(ele);
		}

		[SerializeField]
		internal ZapZapEletrecutationComponent compPre;


		readonly List<ZapZapEletrecutationComponent> eles = [];
		public List<ZapZapEletrecutationComponent> EletrecutationComponents => eles;
	}
}

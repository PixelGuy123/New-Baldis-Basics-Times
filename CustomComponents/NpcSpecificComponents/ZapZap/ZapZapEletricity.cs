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

			CreateEletricity(actMod);
		}

		public void CreateEletricity(ActivityModifier actMod)
		{
			var ele = Instantiate(compPre);
			ele.name = compPre.name;
			ele.AttachTo(actMod, ec, this);
			eles.Add(ele);
		}

		protected override void Despawn()
		{
			base.Despawn();
			for (int i = 0; i < eles.Count; i++)
				if (eles[i])
					eles[i--].Despawn();
		}

		[SerializeField]
		internal ZapZapEletrecutationComponent compPre;


		readonly List<ZapZapEletrecutationComponent> eles = [];
		public List<ZapZapEletrecutationComponent> EletrecutationComponents => eles;
	}
}

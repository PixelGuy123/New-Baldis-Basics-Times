using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ZapZap
{
	public class ZapZapEletricity : Eletricity
	{
		protected override void ActivityEnter(ActivityModifier actMod)
		{
			base.ActivityEnter(actMod);
			CreateEletricity(actMod);
		}

		public void CreateEletricity(ActivityModifier actMod)
		{
			if (AffectedEntities.Contains(actMod))
				return;
			actMod.entity.AddForce(new((actMod.transform.position - transform.position).normalized, repulsionForce, -repulsionForce));

			var ele = Instantiate(compPre);
			ele.name = compPre.name;
			ele.AttachTo(actMod, ec, this);
			AffectedEntities.Add(actMod);
			eles.Add(ele);
		}

		protected override void Despawn()
		{
			base.Despawn();
			for (int i = 0; i < eles.Count; i++)
			{
				if (eles[i])
					eles[i--].Despawn();
			}
		}

		[SerializeField]
		internal ZapZapEletrecutationComponent compPre;

		public Door AffectedDoor { get; set; }
		public HashSet<ActivityModifier> AffectedEntities { get; } = [];

		[SerializeField]
		internal float repulsionForce = 15f;

		readonly List<ZapZapEletrecutationComponent> eles = [];
		public List<ZapZapEletrecutationComponent> EletrecutationComponents => eles;
	}
}

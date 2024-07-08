using BBTimes.CustomComponents.EventSpecificComponents;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class CurtainsClosedEvent : RandomEvent
	{
		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup();
			foreach (var window in FindObjectsOfType<Window>())
			{
				if (window.aTile.Null || window.bTile.Null) // Avoid these!!
					continue;

				var curt = Instantiate(curtPre);
				curt.transform.SetParent(window.aTile.ObjectBase);
				curt.transform.position = window.transform.position + window.direction.ToVector3() * 5f;
				curt.transform.rotation = window.direction.ToRotation();
				curt.AttachToWindow(window);
				curtains.Add(curt);
			}
		}

		public override void Begin()
		{
			base.Begin();
			ec.FreezeNavigationUpdates(true);
			curtains.ForEach(x => x.Close(true));
			ec.FreezeNavigationUpdates(false);
		}

		public override void End()
		{
			base.End();
			curtains.ForEach(x => x.TimedClose(false, Random.Range(1f, 15f)));
		}

		readonly List<Curtains> curtains = [];

		[SerializeField]
		internal Curtains curtPre;
	}
}

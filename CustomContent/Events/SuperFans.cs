using BBTimes.CustomComponents.EventSpecificComponents;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class SuperFans : RandomEvent
	{
		public override void PremadeSetup()
		{
			base.PremadeSetup();
			foreach (var su in FindObjectsOfType<SuperFan>())
			{
				su.Initialize(ec, IntVector2.GetGridPosition(su.transform.position), ec.CellFromPosition(su.transform.position).RandomConstDirection(crng).GetOpposite(), out _);
				superFans.Add(su);
			}
		}

		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup(); // Copy paste from LockdownDoorEvent
			List<Cell> list = ec.AllCells();

			for (int i = 0; i < list.Count; i++)
				if (!list[i].TileMatches(ec.mainHall)|| list[i].HasAnyHardCoverage || list[i].open || !list[i].HasFreeWall || (list[i].shape != TileShape.Single && list[i].shape != TileShape.Corner))
					list.RemoveAt(i--);



			int fans = crng.Next(minFans, maxFans + 1);
			for (int i = 0; i < fans; i++)
			{
				if (list.Count == 0) return;

				int num3 = crng.Next(list.Count);
				var lockdownDoor = Instantiate(superFanPre, list[num3].TileTransform);
				lockdownDoor.Initialize(ec, list[num3].position, list[num3].RandomConstDirection(crng).GetOpposite(), out var l);
				list[num3].HardCoverEntirely();
				superFans.Add(lockdownDoor);
				list.RemoveAt(num3);
				for (int z = 0; z < l.Count; z++)
					list.Remove(l[z]);
			}
		}

		public override void Begin()
		{
			base.Begin();
			superFans.ForEach(x => x.TurnMe(true));
		}

		public override void End()
		{
			base.End();
			superFans.ForEach(x => x.TurnMe(false));
		}

		[SerializeField]
		internal int minFans = 13, maxFans = 21;

		[SerializeField]
		internal SuperFan superFanPre;

		readonly List<SuperFan> superFans = [];
	}
}

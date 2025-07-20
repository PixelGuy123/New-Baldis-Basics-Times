using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Plugin;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class FreezingRoomFunction : RoomFunction
	{
		public override void OnEntityStay(Entity entity)
		{
			base.OnEntityStay(entity);
			if (immuneEntity == entity)
				return; // Prevent jerry from being frozen as well

			var pm = entity.GetComponent<PlayerAttributesComponent>();
			int entIdx = actMods.IndexOf(entity.ExternalActivity);
			if (entIdx == -1)
			{
				if (!pm || !pm.HasAttribute(Storage.HOTCHOCOLATE_ATTR_TAG))
				{
					entity.ExternalActivity.moveMods.Add(moveMod);
					actMods.Add(entity.ExternalActivity);
				}
			}
			else if (pm && pm.HasAttribute(Storage.HOTCHOCOLATE_ATTR_TAG))
			{
				entity.ExternalActivity.moveMods.Remove(moveMod);
				actMods.RemoveAt(entIdx);
			}
		}

		public override void OnEntityExit(Entity entity)
		{
			base.OnEntityExit(entity);
			actMods.Remove(entity.ExternalActivity);
			entity.ExternalActivity.moveMods.Remove(moveMod);
		}

		void OnDestroy()
		{
			while (actMods.Count != 0)
			{
				actMods[0]?.moveMods.Remove(moveMod);
				actMods.RemoveAt(0);
			}

			while (slippers.Count != 0)
			{
				if (slippers[0])
					Destroy(slippers[0].gameObject);
				slippers.RemoveAt(0);
			}
		}

		public override void Initialize(RoomController room)
		{
			base.Initialize(room);
			var cells = room.AllEntitySafeCellsNoGarbage();

			var immunityCell = immuneEntity ? room.ec.CellFromPosition(immuneEntity.transform.position) : null;
			cells.RemoveAll(x => x == immunityCell);

			int am = Random.Range(minSlippersPerRoom, maxSlippersPerRoom);
			for (int i = 0; i <= am; i++)
			{
				if (cells.Count == 0)
					return;

				int index = Random.Range(0, cells.Count);

				var slip = Instantiate(slipMatPre);
				slip.transform.position = cells[index].FloorWorldPosition;
				slippers.Add(slip);

				cells.RemoveAt(index);
			}
		}

		public void AssignImmunityToEntity(Entity e) =>
			immuneEntity = e;

		readonly List<ActivityModifier> actMods = [];
		readonly MovementModifier moveMod = new(Vector3.zero, 0.75f);
		readonly List<SlippingMaterial> slippers = [];

		Entity immuneEntity;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		[SerializeField]
		internal int minSlippersPerRoom = 4, maxSlippersPerRoom = 7;


	}
}

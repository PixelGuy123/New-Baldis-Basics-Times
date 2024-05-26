using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class MagicObject : MonoBehaviour, IEntityTrigger
	{
		public void Initialize(MagicalStudent student)
		{
			ec = student.ec;
			entity.Initialize(ec, transform.position);
			this.student = student;
			Hide();
		}

		public void Hide()
		{
			hidden = true;
			entity.SetActive(false);
			entity.UpdateInternalMovement(Vector3.zero);
			entity.SetFrozen(true);
		}

		public void Throw(Vector3 direction)
		{
			this.direction = direction;
			leftStudent = false;
			hidden = false;
			entity.SetActive(true);
			entity.SetFrozen(false);
			entity.ExternalActivity.moveMods.Clear(); // In case they still exist
			entity.SetHeight(5f);
			entity.Teleport(student.transform.position);
		}

		void Update()
		{
			if (!hidden)
				entity.UpdateInternalMovement(direction * 45f * ec.EnvironmentTimeScale);
			
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (!hidden)
			{
				bool npc = other.CompareTag("NPC");
				if (other.isTrigger && (other.CompareTag("Player") || npc) && (other.transform != student.transform || leftStudent))
				{
					if (npc) // if it is npc
					{
						var n = other.GetComponent<NPC>();
						if (n)
						{
							int num = Random.Range(0, ec.offices.Count);
							n.Navigator.Entity.Teleport(ec.offices[num].RandomEntitySafeCellNoGarbage().CenterWorldPosition);
							n.SentToDetention();
						}
					}
					else // if it is player
						other.GetComponent<PlayerManager>()?.SendToDetention(15f);
					
					Hide();
					return;
				}
			}
		}
		public void EntityTriggerStay(Collider other)
		{
		}
		public void EntityTriggerExit(Collider other)
		{
			if (other.transform == student.transform)
				leftStudent = true;
		}

		[SerializeField]
		internal Entity entity;

		bool hidden = true;
		EnvironmentController ec;
		bool leftStudent = false;
		MagicalStudent student;
		Vector3 direction;
	}
}

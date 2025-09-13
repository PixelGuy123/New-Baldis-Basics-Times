using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static class NPCExtensions
	{
		public static void MakeCharacterChalklesLike(this NPC npc) // Basically how Chalkles does, that way, nothing can affect it (nothing!!)
		{
			npc.Entity.enabled = false;
			npc.Entity.colliderRadius = 0f;
			npc.Navigator.enabled = false;
			var colliders = npc.GetComponents<Collider>();
			for (int i = 0; i < colliders.Length; i++)
				Object.Destroy(colliders[i]);
			npc.baseTrigger = [];

			var dummy = new GameObject("DummyColliders");
			AddDummyCollider();
			AddDummyCollider();

			dummy.SetActive(false);
			dummy.transform.SetParent(npc.transform);
			dummy.transform.localPosition = Vector3.zero;
			dummy.layer = LayerMask.NameToLayer("Disabled");

			void AddDummyCollider()
			{
				var col = dummy.AddComponent<CapsuleCollider>();
				col.radius = 2f;
				col.height = 8f;
				col.enabled = false;
				col.isTrigger = true;
				npc.Entity.collider = col;
				npc.Entity.trigger = col;
			}
		}
	}
}

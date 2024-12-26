using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static class NPCExtensions
	{
		public static void MakeCharacterChalklesLike(this NPC npc) // Basically how Chalkles does, that way, nothing can affect it (nothing!!)
		{
			npc.Navigator.Entity.enabled = false;
			npc.Navigator.enabled = false;
			for (int i = 0; i < npc.baseTrigger.Length; i++)
				Object.Destroy(npc.baseTrigger[i]);
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
				npc.Navigator.Entity.collider = col;
			}
		}
	}
}

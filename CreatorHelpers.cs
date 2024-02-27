using HarmonyLib;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
	public static class NPCCreator
	{
		public static T CreateNPC<T>(string name, bool disableLooker = false) where T : NPC
		{
			var npc = Instantiate(GetBeans()).gameObject;
			npc.name = name;
			Destroy(npc.GetComponent<Beans>()); // Removes the component already
			var comp = npc.AddComponent<T>();
			var entity = npc.GetComponent<Entity>();
			entity.SetActive(false); // Disable it completely
			npc.SetActive(false);
			DontDestroyOnLoad(npc); // Prefab of course
			Destroy(npc.GetComponent<Animator>()); // Useless


			// Setup the npc component
			AccessTools.Field(typeof(T), "navigator").SetValue(comp, npc.GetComponent<Navigator>());
			//AccessTools.Field(typeof(T), "poster").SetValue(comp, postersmth); This is the actual line, figure out a way to make posters mtm101 lol
			AccessTools.Field(typeof(T), "poster").SetValue(comp, AccessTools.Field(typeof(Beans), "poster").GetValue(beans)); // Temporary method to use Beans poster
			comp.spriteBase = comp.transform.GetChild(0).gameObject; // Only child anyways
			comp.spriteRenderer = [comp.spriteBase.transform.GetChild(0).GetComponent<SpriteRenderer>()]; // Only available sprite render (in an array)
			comp.baseTrigger = npc.GetComponents<CapsuleCollider>(); // More than one collider actually
			comp.looker = npc.GetComponent<Looker>(); // Set looker
			comp.looker.enabled = !disableLooker;
			comp.spawnableRooms = [RoomCategory.Hall]; // Temporary too
			comp.potentialRoomAssets = []; // Temporary too as there aren't custom rooms yet

			// Setup the entity component
			AccessTools.Field(typeof(Entity), "iEntityTrigger").SetValue(entity, new IEntityTrigger[1] { comp }); // Sets the trigger to itself

			// Setup for navigator component
			var nav = npc.GetComponent<Navigator>();
			nav.npc = comp; // Set npc reference
			AccessTools.Field(typeof(Navigator), "entity").SetValue(nav, entity); // hardly set entity reference
			AccessTools.Field(typeof(Navigator), "collider").SetValue(nav, comp.baseTrigger[0]); // Use by default the first trigger

			// Setup for looker component
			AccessTools.Field(typeof(Looker), "npc").SetValue(comp.looker, comp);

			// Additional field changes
			var man = npc.GetComponent<PropagatedAudioManager>();
			// AccessTools.Field(typeof(PropagatedAudioManager), "minDistance").SetValue(man, 1f);
			// AccessTools.Field(typeof(PropagatedAudioManager), "maxDistance").SetValue(man, 2f); // Set the audio distance, temporarily disabled


			return comp;
		}

		private static Beans GetBeans()
		{
			beans ??= Resources.FindObjectsOfTypeAll<Beans>()[0];
			return beans;
		}
 
		static Beans beans;
	}
}

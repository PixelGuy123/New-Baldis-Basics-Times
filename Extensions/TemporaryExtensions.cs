using BepInEx;
using MTM101BaldAPI.Registers;

namespace BBTimes.Extensions
{

	/* 
	 *
	 * AS THE FILE NAME SAYS, THIS IS A TEMPORARY FILE, AND WILL BE DELETED ONCE THE API HAS THE REQUIRED METHODS FOR ME
	 *
	 */
	public static class TemporaryExtensions
	{
		//public static NPCMetadata AddMeta(this NPC npc, BaseUnityPlugin i, string name, NPCFlags flags) 
		//{
		//	var meta = new NPCMetadata(i.Info, [npc], name, flags);
		//	NPCMetaStorage.Instance.Add(meta);
		//	return meta;
		//}

		//public static NPCMetadata AddMetas(BaseUnityPlugin i, string name, NPCFlags flags, params NPC[] npcs)
		//{
		//	var meta = new NPCMetadata(i.Info, npcs, name, flags);
		//	NPCMetaStorage.Instance.Add(meta);
		//	return meta;
		//}

		//public static RandomEventMetadata AddMeta(this RandomEvent e, BaseUnityPlugin i, RandomEventFlags flags)
		//{
		//	var meta = new RandomEventMetadata(i.Info, e, flags); // Temporary, I'll wait for the api to implement it properly
		//	RandomEventMetaStorage.Instance.Add(meta);
		//	return meta;
		//}

		public static ObjectBuilderMeta AddMeta(this ObjectBuilder e, BaseUnityPlugin i)
		{
			var meta = new ObjectBuilderMeta(i.Info, e); // Temporary, I'll wait for the api to implement it properly
			ObjectBuilderMetaStorage.Instance.Add(meta);
			return meta;
		}
	}
}

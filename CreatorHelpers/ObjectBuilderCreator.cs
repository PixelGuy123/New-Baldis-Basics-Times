using MTM101BaldAPI;
using UnityEngine;
using BBTimes.CustomComponents;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		public static StructureWithParameters CreateObjectBuilder<O>(string name, out O builder, string obstacleName = null) where O : StructureBuilder
		{
			builder = new GameObject(name).AddComponent<O>();

			builder.gameObject.ConvertToPrefab(true);
			var data = builder.GetComponent<IBuilderPrefab>();

			data.Name = obstacleName;
			BasePlugin._cstData.Add(data);

			data.SetupPrefab();

			return data.SetupBuilderPrefabs();
		}

	}
}

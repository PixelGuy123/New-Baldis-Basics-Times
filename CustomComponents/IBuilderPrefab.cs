using UnityEngine;

namespace BBTimes.CustomComponents
{
	public interface IBuilderPrefab : IObjectPrefab
	{
		StructureWithParameters SetupBuilderPrefabs();
	}
}

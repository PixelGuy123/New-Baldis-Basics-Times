using UnityEngine;

namespace BBTimes.CustomComponents
{
	public interface IObjectPrefab : IPrefab
	{
		void SetupPrefab();
		void SetupPrefabPost();
	}
}

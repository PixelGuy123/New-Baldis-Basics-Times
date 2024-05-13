using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class PlayerAttributesComponent : MonoBehaviour
	{
		readonly Dictionary<string, int> attributes = [];
		public void AddAttribute(string attribute)
		{
			if (attributes.ContainsKey(attribute)) attributes[attribute]++;

			else attributes.Add(attribute, 1);
		}

		public void RemoveAttribute(string attribute)
		{
			if (!attributes.ContainsKey(attribute)) return;

			int val = --attributes[attribute];
			if (val <= 0)
				attributes.Remove(attribute);
		}

		public bool HasAttribute(string attribute) =>
			attributes.ContainsKey(attribute);

		void Awake() => _pm = GetComponent<PlayerManager>();

		PlayerManager _pm;

		public PlayerManager Pm => _pm;
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class MainGameManagerExtraComponent : MonoBehaviour
	{
		[SerializeField]
		public string[] midis = [];

		[SerializeField]
		public Color outsideLighting = Color.white;

		[SerializeField]
		public Cubemap mapForToday;
	}
}

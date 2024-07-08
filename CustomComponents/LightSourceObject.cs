using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class LightSourceObject : EnvironmentObject
	{
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			ec.GenerateLight(ec.CellFromPosition(transform.position), colorToLight, colorStrength);
		}


		[SerializeField]
		internal Color colorToLight;

		[SerializeField]
		internal int colorStrength;
	}
}

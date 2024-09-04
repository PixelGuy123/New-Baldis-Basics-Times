using BBTimes.CustomContent.NPCs;
using TMPro;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class FloatingLetter : MonoBehaviour, IClickable<int>
	{
		public void Initialize(Penny pen, EnvironmentController ec)
		{
			this.pen = pen;
			this.ec = ec;
		}
		public void PickLetter(char letter) 
		{
			assignedChar = letter;
			renderer.text = $"{letter}";
		}

		void Update()
		{
			if (ec && Time.timeScale != 0)
			{
				Vector3 pos = renderer.transform.localPosition;
				pos.y = Mathf.Sin(Time.fixedTime * ec.EnvironmentTimeScale * 4f) * 1.5f;
				renderer.transform.localPosition = pos;

				if (selected > 0)
				{
					renderer.transform.localScale += (selectedSize - renderer.transform.localScale) * 12f * ec.EnvironmentTimeScale * Time.deltaTime;
					return;
				}
				renderer.transform.localScale += (Vector3.one - renderer.transform.localScale) * 12f * ec.EnvironmentTimeScale * Time.deltaTime;
			}
		}

		public void Clicked(int player)
		{
			if (!pen || char.IsWhiteSpace(assignedChar)) return;
			pen.TakeLetter(assignedChar);
		}
		public void ClickableSighted(int player) =>
			selected++;
		public void ClickableUnsighted(int player) =>
			selected--;
		public bool ClickableHidden() => false;
		public bool ClickableRequiresNormalHeight() => false;
		
		public string Text { get => renderer.text; set => renderer.text = value; }

		[SerializeField]
		internal TextMeshPro renderer;

		[SerializeField]
		internal Vector3 selectedSize = Vector3.one * 1.5f;

		EnvironmentController ec;
		Penny pen;
		char assignedChar = ' ';

		int selected = 0;
	}
}

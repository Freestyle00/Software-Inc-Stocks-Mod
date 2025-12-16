using System;
using System.Linq;
using UnityEngine;

namespace Software_Inc_Stocks_Mod
{
	// Your mod MUST have exactly one class deriving from ModMeta
	public class Main : ModMeta
	{
		private StocksProBehaviour _StocksProBehaviour;

		public static string _Name = "StocksPro";
		public override string Name => _Name;

		public override void Initialize(ModController.DLLMod parentMod)
		{
			// Locate the behaviour instance assigned by ModController

			_StocksProBehaviour = parentMod.Behaviors.OfType<StocksProBehaviour>().First();
		}

		public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
		{
			//Start by spawning a label
			var OptionsText = WindowManager.SpawnLabel();
			OptionsText.text = _Name;
			//Add the label to the mod panel at (0, 0) with 96 width and 32 height, anchored to the top left
			WindowManager.AddElementToElement(OptionsText.gameObject, parent.gameObject, new Rect(0, 0, 96, 32),
				new Rect(0, 0, 0, 0));
			//Option to enable and disable Debug messages
			var DebugCheckbox = WindowManager.SpawnCheckbox();
			//sets the checkbox to off
			//DebugCheckbox.isOn = false; 
			DebugCheckbox.onValueChanged.AddListener(x => _StocksProBehaviour.DebugChange(x));
			DebugCheckbox.GetComponentInChildren<UnityEngine.UI.Text>().text = "Debug";
			WindowManager.AddElementToElement(DebugCheckbox.gameObject, parent.gameObject, new Rect(0, 50, 100, 100), new Rect(0, 0, 0, 0));
		}
	}
}
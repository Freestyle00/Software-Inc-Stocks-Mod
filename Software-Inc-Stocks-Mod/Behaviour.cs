using DevConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Software_Inc_Stocks_Mod
{
	public class StocksProBehaviour : ModBehaviour
	{
		public static Button StockButton;
		public static GameObject StockPanel;
		public static bool Loaded = false;
		private StocksUI _stocksUI;

		public override void OnActivate()
		{
			utils.DebugConsoleWrite("Activated");
			DebugConsoleCommands();
			SceneManager.sceneLoaded += OnLoad;
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToString() == "MainScene")
			{
				InitUI();
			}
		}
		public override void OnDeactivate()
		{
			SceneManager.sceneLoaded -= OnLoad;
			RemoveConsoleCommands();
			DestroyUI();
			utils.DebugConsoleWrite("Deactivated");
		}
		public void Start()
		{
			/*utils.DebugConsoleWrite("Start called");
			SceneManager.sceneLoaded += OnLoad;
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToString() == "MainScene")
			{
				InitUI();
			}*/
		}
		public void DebugConsoleCommands()
		{
			DevConsole.Console.AddCommand(new Command("getstocks", utils.ConsoleGetStocks, "List stocks owned by the player company (verbose)"));
			DevConsole.Console.AddCommand(new Command("dumpmarket", utils.ConsoleDumpMarket, "List all market companies with id, shares and current price"));
			DevConsole.Console.AddCommand(new Command<string, uint, uint>("stocks", utils.ConsoleStocksOps, "(B)uy or (S)ell <shares> of company with <companyId> immediately (verbose)"));
			DevConsole.Console.AddCommand(new Command("quit", utils.ConsoleQuit, "Quit the game"));
		}
		public void RemoveConsoleCommands()
		{
			DevConsole.Console.RemoveCommand("getstocks");
			DevConsole.Console.RemoveCommand("dumpmarket");
			DevConsole.Console.RemoveCommand("stocks");
			DevConsole.Console.RemoveCommand("quit");
		}
		private void OnLoad(Scene scene, LoadSceneMode mode)
		{
			utils.DebugConsoleWrite($"OnLoad called with scene: {scene.name.ToString()} | and mode: {mode.ToString()}");
			if (isActiveAndEnabled)
			{
				Loaded = false;
				switch (scene.name)
				{
					case "MainMenu":
						DestroyUI();
						break;
					case "MainScene":
						InitUI();
						break;
					default:
						goto case "MainMenu";
				}
			}
		}
		private void InitUI()
		{
			utils.DebugConsoleWrite("InitUI called");

			GameObject stocksUIGO = new GameObject("StocksUI", typeof(StocksUI));
			stocksUIGO.hideFlags = HideFlags.HideAndDontSave;
			_stocksUI = stocksUIGO.GetComponent<StocksUI>();

			if (StockButton == null)
			{
				StockButton = StocksButton.stocksButton(() => _stocksUI.Toggle());
				utils.DebugConsoleWrite("Stocks button created and linked to UI toggle");
			}
		}
		private void DestroyUI()
		{
			if (_stocksUI != null)
			{
				Destroy(_stocksUI.gameObject);
				Destroy(_stocksUI);
				_stocksUI = null;
			}
			if (StockButton != null)
			{
				Destroy(StockButton.gameObject);
				StockButton = null;
			}
		}
	}
}


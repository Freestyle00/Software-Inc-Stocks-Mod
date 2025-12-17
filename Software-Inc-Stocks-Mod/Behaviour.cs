using DevConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
			try
			{
				utils.DebugConsoleWrite("OnActivate called");

				DebugConsoleCommands();

				SceneManager.sceneLoaded += OnLoad;
				utils.DebugConsoleWrite("Scene loaded listener added");

				if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainScene")
				{
					InitUI();
				}
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"OnActivate exception: {ex}");
			}
		}

		public override void OnDeactivate()
		{
			try
			{
				utils.DebugConsoleWrite("OnDeactivate called");

				SceneManager.sceneLoaded -= OnLoad;
				utils.DebugConsoleWrite("Scene loaded listener removed");

				RemoveConsoleCommands();
				utils.DebugConsoleWrite("Console commands removed");

				DestroyUI();
				utils.DebugConsoleWrite("UI destroyed");

				utils.DebugConsoleWrite("Deactivated successfully");
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"OnDeactivate exception: {ex}");
			}
		}

		public void DebugConsoleCommands()
		{
			try
			{
				utils.DebugConsoleWrite("Adding console commands");

				DevConsole.Console.AddCommand(new Command("getstocks", utils.ConsoleGetStocks, "List stocks owned by the player company (verbose)"));
				DevConsole.Console.AddCommand(new Command("dumpmarket", utils.ConsoleDumpMarket, "List all market companies with id, shares and current price"));
				DevConsole.Console.AddCommand(new Command<string, uint, uint>("stocks", utils.ConsoleStocksOps, "(B)uy or (S)ell <shares> of company with <companyId> immediately (verbose)"));
				DevConsole.Console.AddCommand(new Command("quit", utils.ConsoleQuit, "Quit the game"));

				utils.DebugConsoleWrite("Console commands added successfully");
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"DebugConsoleCommands exception: {ex}");
			}
		}

		public void RemoveConsoleCommands()
		{
			try
			{
				utils.DebugConsoleWrite("Removing console commands");

				DevConsole.Console.RemoveCommand("getstocks");
				DevConsole.Console.RemoveCommand("dumpmarket");
				DevConsole.Console.RemoveCommand("stocks");
				DevConsole.Console.RemoveCommand("quit");

				utils.DebugConsoleWrite("Console commands removed successfully");
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"RemoveConsoleCommands exception: {ex}");
			}
		}

		private void OnLoad(Scene scene, LoadSceneMode mode)
		{
			try
			{
				utils.DebugConsoleWrite($"OnLoad called with scene: {scene.name}, mode: {mode}");

				if (isActiveAndEnabled)
				{
					Loaded = false;

					switch (scene.name)
					{
						case "MainMenu":
							utils.DebugConsoleWrite("Destroying UI for MainMenu");
							DestroyUI();
							break;
						case "MainScene":
							utils.DebugConsoleWrite("Initializing UI for MainScene");
							InitUI();
							break;
						default:
							utils.DebugConsoleWrite("Unknown scene, destroying UI");
							DestroyUI();
							break;
					}
				}
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"OnLoad exception: {ex}");
				utils.DebugConsoleWrite("Destroying UI due to exception");
				DestroyUI();
			}
		}

		private void InitUI()
		{
			try
			{
				utils.DebugConsoleWrite("InitUI called");

				if (_stocksUI == null)
				{
					GameObject stocksUIGO = new GameObject("StocksUI", typeof(StocksUI));
					stocksUIGO.hideFlags = HideFlags.HideAndDontSave;
					_stocksUI = stocksUIGO.GetComponent<StocksUI>();
					_stocksUI.IsDestroyed = false;
					utils.DebugConsoleWrite("Stocks UI created");
				}

				if (StockButton == null)
				{
					StockButton = StocksButton.stocksButton(() => _stocksUI?.Toggle());
					utils.DebugConsoleWrite("Stocks button created and linked to UI toggle");
				}
				else
				{
					StockButton.onClick.RemoveAllListeners();
					StockButton.onClick.AddListener(() => _stocksUI?.Toggle());
					utils.DebugConsoleWrite("Stocks button callback updated");
				}
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"InitUI exception: {ex}");
			}
		}

		private void DestroyUI()
		{
			try
			{
				if (_stocksUI != null)
				{
					_stocksUI.Deactivate();
					_stocksUI.enabled = false; 
					Destroy(_stocksUI.gameObject);
					utils.DebugConsoleWrite("Stocks UI destroyed");
				}

				if (StockButton != null)
				{
					StockButton.onClick.RemoveAllListeners();
					Destroy(StockButton.gameObject);
					utils.DebugConsoleWrite("Stocks button destroyed");
				}
			}
			catch (Exception ex)
			{
				utils.ConsoleWrite($"DestroyUI exception: {ex}");
			}
		}


	}
}

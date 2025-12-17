using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Software_Inc_Stocks_Mod
{
	public static class utils
	{
		public static bool Debug = false;
		public static void DebugConsoleWrite(string msg)
		{
			if (Debug) DevConsole.Console.Log($"{Main._Name}: {msg}");
		}
		public static void ConsoleWrite(string msg)
		{
			DevConsole.Console.Log($"{Main._Name}: {msg}");
		}
		public static List<NewStock> GetOwnedStocks(Company company)
		{
			if (company == null)
			{
				DebugConsoleWrite("GetOwnedStocks called with null company");
				return new List<NewStock>();
			}

			DebugConsoleWrite($"Got list of Stocks from {company.Name}");
			return company.NewOwnedStock;
		}
		public static void ConsoleGetStocks()
		{
			Company player = GameSettings.Instance?.MyCompany;
			if (player == null)
			{
				ConsoleWrite("Player company not available (GameSettings.Instance.MyCompany is null).");
				return;
			}

			var stocks = GetOwnedStocks(player);
			if (stocks == null || stocks.Count == 0)
			{
				ConsoleWrite($"No owned stocks for player company '{player.Name}'.");
				return;
			}

			ConsoleWrite($"Owned stocks for '{player.Name}' (Company ID {player.ID}):");
			int idx = 0;
			foreach (NewStock stock in stocks)
			{
				Company seller = stock.Seller;
				string sellerName = (seller != null) ? seller.Name : "<unknown>";
				uint sellerId = (seller != null) ? seller.ID : 0;
				uint shares = stock.Shares;
				double pricePerShare = stock.ShareWorth;
				double total = stock.TotalWorth;
				double initial = stock.InitialWorth;
				double change = stock.Change;
				double ownershipPct = stock.Percentage * 100.0;

				ConsoleWrite($"[{idx}] Seller: '{sellerName}' (ID {sellerId})  Shares: {shares}  Price/share: {pricePerShare:0.00}  Total: {total:0.00}  Initial: {initial:0.00}  Change: {change:P2}  Ownership%: {ownershipPct:0.##}%");
				idx++;
			}
		}
		public static void ConsoleDumpMarket()
		{
			if (MarketSimulation.Active == null)
			{
				ConsoleWrite("Market simulation (MarketSimulation.Active) is not available.");
				return;
			}

			var companies = MarketSimulation.Active.GetAllCompanies().ToList();
			if (companies.Count == 0)
			{
				ConsoleWrite("Market contains no companies.");
				return;
			}

			ConsoleWrite($"Dumping market: {companies.Count} companies found.");
			int idx = 0;
			foreach (Company c in companies)
			{
				if (c == null) continue;

				uint id = c.ID;
				string name = c.Name ?? "<unnamed>";
				uint totalShares = c.Shares;
				double sharePrice = c.GetShareWorth();
				float ownShares = c.GetOwnShares();
				int shareholderCount = c.NewStock?.Count ?? 0;

				ConsoleWrite($"[{idx}] ID: {id}  Name: '{name}'  Shares(total): {totalShares}  SharePrice: {sharePrice:0.00}  OwnShares: {ownShares}  Shareholders: {shareholderCount}");
				idx++;
			}
		}
		public static void ConsoleStocksOps(string buySell, uint companyId, uint shares)
		{
			if (shares == 0)
			{
				ConsoleWrite("Argument error: 'shares' must be at least 1.");
				return;
			}

			Company seller = GetCompanyById(companyId);
			if (seller == null)
			{
				ConsoleWrite($"Company with ID {companyId} not found or simulation unavailable.");
				return;
			}

			Company buyer = GameSettings.Instance?.MyCompany;
			if (buyer == null)
			{
				ConsoleWrite("Player company (GameSettings.Instance.MyCompany) not available.");
				return;
			}

			NewStock existing;
			if (buySell.ToLower() == "buy" || buySell.ToLower() == "b")
			{
				existing = FindMarketLot(seller, shares, false);
			}
			else if (buySell.ToLower() == "sell" || buySell.ToLower() == "s")
			{
				existing = FindMarketLot(seller, shares, true);
			}
			else
			{
				ConsoleWrite("Argument error: first argument must be 'buy' or 'sell'.");
				return;
			}

			if (existing != null)
			{
				double pricePerShare = existing.ShareWorth;
				double totalPrice = pricePerShare * shares;
				ConsoleWrite($"Found market lot: Seller '{seller.Name}' Buyer '{existing.BuyerName}' LotShares={existing.Shares}. Buying {shares} @ {pricePerShare:0.00} -> total {totalPrice:0.00}");
				UseStockButtonForPurchase(existing, shares);
			}
			else
			{
				ConsoleWrite("Purchase failed. Possible reasons: insufficient funds or trade rejected by game logic.");
			}
		}
		public static NewStock FindMarketLot(Company seller, uint desiredShares, bool findPlayerLot)
		{
			if (seller == null) return null;

			if (!findPlayerLot)
			{
				try
				{
					if (seller.NewStock == null || seller.NewStock.Count == 0) return null;

					DebugConsoleWrite("Searching market lots");
					NewStock found = seller.NewStock
						.OrderByDescending(n => n.Shares)
						.FirstOrDefault(n => n.Shares >= desiredShares);

					if (found == null)
					{
						DebugConsoleWrite("No lot with sufficient shares found, picking largest available lot.");
						found = seller.NewStock.OrderByDescending(n => n.Shares).FirstOrDefault();
					}

					foreach (NewStock stock in GameSettings.Instance.MyCompany.NewOwnedStock)
					{
						if (found == stock)
						{
							DebugConsoleWrite("FindMarketLot: Skipping own stock lot.");
							found = seller.NewStock.Where(n => n != stock).OrderByDescending(n => n.Shares).Last();
							if (found == stock)
							{
								DebugConsoleWrite("FindMarketLot: No valid market lot found (all owned by player).");
								return null;
							}
						}
					}

					return found;
				}
				catch (Exception ex)
				{
					DebugConsoleWrite("FindMarketLot error: " + ex.ToString());
					return null;
				}
			}
			else
			{
				foreach (NewStock stock in seller.NewStock)
				{
					if (stock.Buyer == GameSettings.Instance.MyCompany)
					{
						DebugConsoleWrite("FindMarketLot: Found player-owned lot.");
						return stock;
					}
				}
				DebugConsoleWrite("FindMarketLot: No player-owned lot found.");
				return null;
			}
		}
		public static void UseStockButtonForPurchase(NewStock stock, uint shares)
		{
			if (stock == null)
			{
				DebugConsoleWrite("UseStockButtonForPurchase: stock is null.");
				return;
			}

			if (GameSettings.Instance == null)
			{
				DebugConsoleWrite("Game not ready for UI-based purchase.");
				return;
			}

			GameObject root = null;
			try
			{
				root = new GameObject("TempStockButtonInvoker");
				root.hideFlags = HideFlags.HideAndDontSave;

				StockButton sb = root.AddComponent<StockButton>();
				Font arial = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Font>("Arial.ttf");

				// CompanyText
				var companyGO = new GameObject("TempCompanyText");
				companyGO.transform.SetParent(root.transform, worldPositionStays: false);
				var companyText = companyGO.AddComponent<UnityEngine.UI.Text>();
				companyText.font = arial;
				companyText.fontSize = 14;
				sb.CompanyText = companyText;

				// SliderText
				var sliderTextGO = new GameObject("TempSliderText");
				sliderTextGO.transform.SetParent(root.transform, worldPositionStays: false);
				var sliderText = sliderTextGO.AddComponent<UnityEngine.UI.Text>();
				sliderText.font = arial;
				sliderText.fontSize = 12;
				sb.SliderText = sliderText;

				// StockSlider
				var sliderGO = new GameObject("TempSlider");
				sliderGO.transform.SetParent(root.transform, worldPositionStays: false);
				var slider = sliderGO.AddComponent<UnityEngine.UI.Slider>();
				slider.minValue = 0f;
				slider.maxValue = Math.Max(1f, (float)stock.Shares);
				slider.value = Mathf.Clamp((float)shares, slider.minValue, slider.maxValue);
				sb.StockSlider = slider;

				// ButtonText
				var btnTextGO = new GameObject("TempButtonText");
				btnTextGO.transform.SetParent(root.transform, worldPositionStays: false);
				var btnText = btnTextGO.AddComponent<UnityEngine.UI.Text>();
				btnText.font = arial;
				btnText.fontSize = 14;
				sb.ButtonText = btnText;

				// ChangeText
				var changeGO = new GameObject("TempChangeText");
				changeGO.transform.SetParent(root.transform, worldPositionStays: false);
				var changeText = changeGO.AddComponent<UnityEngine.UI.Text>();
				changeText.font = arial;
				changeText.fontSize = 12;
				sb.ChangeText = changeText;

				// PayoutText
				var payoutGO = new GameObject("TempPayoutText");
				payoutGO.transform.SetParent(root.transform, worldPositionStays: false);
				var payoutText = payoutGO.AddComponent<UnityEngine.UI.Text>();
				payoutText.font = arial;
				payoutText.fontSize = 12;
				sb.PayoutText = payoutText;

				// Logo
				var logoGO = new GameObject("TempLogo");
				logoGO.transform.SetParent(root.transform, worldPositionStays: false);
				var raw = logoGO.AddComponent<UnityEngine.UI.RawImage>();
				sb.Logo = raw;

				// ButtonImage
				var btnImageGO = new GameObject("TempButtonImage");
				btnImageGO.transform.SetParent(root.transform, worldPositionStays: false);
				var btnImage = btnImageGO.AddComponent<UnityEngine.UI.Image>();
				sb.ButtonImage = btnImage;

				// ActionButton
				var actionBtnGO = new GameObject("TempActionButton");
				actionBtnGO.transform.SetParent(root.transform, worldPositionStays: false);
				var actionBtnImage = actionBtnGO.AddComponent<UnityEngine.UI.Image>();
				var actionBtn = actionBtnGO.AddComponent<UnityEngine.UI.Button>();
				actionBtn.targetGraphic = actionBtnImage;
				sb.ActionButton = actionBtn;

				// CustomAmount
				var customGO = new GameObject("TempCustomAmount");
				customGO.transform.SetParent(root.transform, worldPositionStays: false);
				var textForInput = customGO.AddComponent<UnityEngine.UI.Text>();
				textForInput.font = arial;
				textForInput.fontSize = 12;
				var input = customGO.AddComponent<UnityEngine.UI.InputField>();
				input.textComponent = textForInput;
				sb.CustomAmount = input;

				sb.Init(stock);

				if (sb.StockSlider != null)
				{
					sb.StockSlider.maxValue = Math.Max(1f, stock.Shares);
					sb.StockSlider.minValue = 0f;
					sb.StockSlider.value = Mathf.Clamp((float)shares, sb.StockSlider.minValue, sb.StockSlider.maxValue);
				}

				sb.Action();
			}
			catch (Exception ex)
			{
				DebugConsoleWrite("Exception using StockButton UI path: " + ex.Message);
				DebugConsoleWrite(ex.ToString());
			}
			finally
			{
				if (root != null)
				{
					try
					{
						UnityEngine.Object.DestroyImmediate(root);
					}
					catch { }
				}
			}
		}
		public static  Company GetCompanyById(uint companyId)
		{
			if (GameSettings.Instance == null || GameSettings.Instance.simulation == null)
			{
				DebugConsoleWrite("GetCompanyById: GameSettings or simulation not available.");
				return null;
			}

			return GameSettings.Instance.simulation.GetCompany(companyId);
		}
		public static void DebugChange(bool State)
		{
			Debug = State;
			DebugConsoleWrite("Debug mode activated");
			if (!State) ConsoleWrite("Debug mode Deactivated");
		}
		public static double GetAllDividends(Company company)
		{
			double totalDividends = 0.0;
			foreach (NewStock stock in company.NewStock)
			{
				totalDividends += stock.Payout;
			}
			return totalDividends;
		}
		public static double GetPlayerDividends(Company company)
		{
			NewStock playerStock = GetPlayerShare(company);
			if (playerStock != null)
			{
				return playerStock.Payout;
			}
			else
			{
				return 0.0;
			}
		}
		public static int GetPlayerShares(Company company)
		{
			NewStock playerStock = GetPlayerShare(company);
			if (playerStock != null)
			{
				return (int)playerStock.Shares;
			}
			else
			{
				return 0;
			}
		}
		public static NewStock GetPlayerShare(Company company)
		{
			NewStock playerStock = null;
			foreach (NewStock stock in company.NewStock)
			{
				if (stock.Buyer == GameSettings.Instance.MyCompany)
				{
					playerStock = stock;
				}
			}
			return playerStock;
		}
		public static double GetPlayerPNL(Company company)
		{
			NewStock playerStock = GetPlayerShare(company);
			if (playerStock != null)
			{
				return (playerStock.ShareWorth - playerStock.InitialWorth) * playerStock.Shares;
			}
			else
			{
				return 0.0;
			}
		}
		public static float GetPlayerPortfolioValue()
		{
			int portfolioValue = 0;
			IEnumerable<Company> companies = MarketSimulation.Active.GetAllCompanies();
			foreach (Company comp in companies)
			{
				NewStock share = GetPlayerShare(comp);
				if (share != null)
				{

					portfolioValue = portfolioValue + ((int)share.ShareWorth * (int)share.Shares);
				}
			}
			return (float)portfolioValue;
		}
	}
}

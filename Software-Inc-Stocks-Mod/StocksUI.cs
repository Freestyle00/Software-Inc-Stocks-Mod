using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GUIListView;

namespace Software_Inc_Stocks_Mod
{
	public class StocksUI : MonoBehaviour
	{
		private GUIWindow _stockWindow;
		private GUILineChart _stocksLineChart;
		private GUIListView _stockListView;
		private Text _displayText;
		private System.Random _random;
		private List<string> _randomTexts;
		private const int SHARES_ALL = 2147483647;
		private const float BUTTON_WIDTH = 50f;
		private void Awake()
		{

		}
		private void Update()
		{
			//DevConsole.Console.Log("StocksPro: Window updated");
			RefreshText();
		}
		public void InitStockPanel()
		{
			_stockWindow = WindowManager.SpawnWindow();
			_stockWindow.InitialTitle = _stockWindow.TitleText.text = _stockWindow.NonLocTitle = Main._Name;
			_stockWindow.MinSize.x = 600f;
			_stockWindow.MinSize.y = 500f;
			_stockWindow.name = "StockProInterface";
			_stockWindow.MainPanel.name = "StockProMainPanel";
			Button closeButton = _stockWindow.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "CloseButton");
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(() => _stockWindow.Close());
			}
			InitList();
			WindowManager.AddElementToWindow(_stockListView.gameObject, _stockWindow, new Rect(0f, 200f, 0, -220), new Rect(0f, 0f, 1f, 1f));
			_stockWindow.Show();
		}
		private void InitList()
		{
			CompanyDetailWindow companyDetailWindow;
			GUIListView.ColumnDef companyLogo = new ColumnDefinition<Company>(header: "Logo", label: (Company c) => (object)c, comparison: c => c.Name, vola: true, width: 24f, filterType: GUIListView.FilterType.None, filter: null, typeOverride: GUIColumn.ColumnType.Logo);
			GUIListView.ColumnDef companyNameColumn = new ColumnDefinition<Company>(header: "Company", label: x => x.Name, vola: false, width: 200f);
			GUIListView.ColumnDef companyWorthColumn = new ColumnDefinition<Company>(header: "Worth", label: x => x.GetMoneyWithInsurance(), vola: true, width: 100f);
			GUIListView.ColumnDef companySharesColumn = new ColumnDefinition<Company>(header: "Shares", label: x => x.Shares, vola: false, width: 100f);
			GUIListView.ColumnDef companyDividendsColumn = new ColumnDefinition<Company>(header: "All Dividends", label: x => GetAllDividends(x), vola: true, width: 100f);
			GUIListView.ColumnDef companyPlayerDividendsColumn = new ColumnDefinition<Company>(header: "Your Dividends", label: x => GetPlayerDividends(x), vola: true, width: 100f);
			GUIListView.ColumnDef companySharePriceColumn = new ColumnDefinition<Company>(header: "Shareworth", label: x => x.GetShareWorth(), vola: true, width: 100f);
			GUIListView.ColumnDef companyYourSharesColumn = new ColumnDefinition<Company>(header: "Your Shares", label: x => GetPlayerShares(x), vola: true, width: 100f);
			GUIListView.ColumnDef companyPNL = new ColumnDefinition<Company>(header: "Your PnL", label: x => GetPlayerPNL(x), vola: true, width: 100f);

			GUIListView.ColumnDef buyAll       = new ColumnDefinition<Company>("+ALL",  company => { ConsoleStocksOps("b", company.ID, company.Shares); }, BUTTON_WIDTH);
			GUIListView.ColumnDef buyThousand  = new ColumnDefinition<Company>("+1000", company => { ConsoleStocksOps("b", company.ID, 1000);           }, BUTTON_WIDTH);
			GUIListView.ColumnDef buyHundred   = new ColumnDefinition<Company>("+100",  company => { ConsoleStocksOps("b", company.ID, 100);            }, BUTTON_WIDTH);
			GUIListView.ColumnDef buyTen       = new ColumnDefinition<Company>("+10",   company => { ConsoleStocksOps("b", company.ID, 10);             }, BUTTON_WIDTH);
			GUIListView.ColumnDef sellTen      = new ColumnDefinition<Company>("+10",   company => { ConsoleStocksOps("s", company.ID, 10);             }, BUTTON_WIDTH);
			GUIListView.ColumnDef sellHundred  = new ColumnDefinition<Company>("+100",  company => { ConsoleStocksOps("s", company.ID, 100);            }, BUTTON_WIDTH);
			GUIListView.ColumnDef sellThousand = new ColumnDefinition<Company>("+1000", company => { ConsoleStocksOps("s", company.ID, 1000);           }, BUTTON_WIDTH);
			GUIListView.ColumnDef sellAll      = new ColumnDefinition<Company>("-ALL",  company => { ConsoleStocksOps("s", company.ID, company.Shares); }, BUTTON_WIDTH);

			_stockListView = WindowManager.SpawnList();
			_stockListView.AddColumn(companyLogo);
			_stockListView.AddColumn(companyNameColumn);
			_stockListView.AddColumn(companyWorthColumn);
			_stockListView.AddColumn(companySharesColumn);
			_stockListView.AddColumn(companyDividendsColumn);
			_stockListView.AddColumn(companyPlayerDividendsColumn);
			_stockListView.AddColumn(companyYourSharesColumn);
			_stockListView.AddColumn(companySharePriceColumn);
			_stockListView.AddColumn(companyPNL);
			_stockListView.AddColumn(buyAll);
			_stockListView.AddColumn(buyThousand);
			_stockListView.AddColumn(buyHundred);
			_stockListView.AddColumn(buyTen);
			_stockListView.AddColumn(sellTen);
			_stockListView.AddColumn(sellHundred);
			_stockListView.AddColumn(sellThousand);
			_stockListView.AddColumn(sellAll);

			UpdateListView();
		}
		private void UpdateListView()
		{
			Company playerCompany = GameSettings.Instance.MyCompany;
			IEnumerable<Company> companies = MarketSimulation.Active.GetAllCompanies();
			companies = companies.Where(c => c != playerCompany);
			_stockListView.Items = companies.Cast<object>().ToList();
		}
		private double GetAllDividends(Company company)
		{
			double totalDividends = 0.0;
			foreach (NewStock stock in company.NewStock)
			{
				totalDividends += stock.Payout;
			}
			return totalDividends;
		}
		private double GetPlayerDividends(Company company)
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
		private int GetPlayerShares(Company company)
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
		private NewStock GetPlayerShare(Company company)
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
		private double GetPlayerPNL(Company company)
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
		private string GetRandomText()
		{
			if (_randomTexts == null || _randomTexts.Count == 0)
				return "No text available.";

			int index = _random.Next(_randomTexts.Count);
			return _randomTexts[index];
		}
		public void RefreshText()
		{
			if (_displayText != null)
			{
				_displayText.text = GetRandomText();
			}
		}
		public void Show()
		{
			if (_stockWindow == null)
			{
				InitStockPanel();
			}
			else
			{
				_stockWindow.Show();
			}
		}
		public void Hide()
		{
			if (_stockWindow != null)
			{
				_stockWindow.Close();
			}
		}
		public void Toggle()
		{
			if (_stockWindow == null)
			{
				Show();
			}
			else if (_stockWindow.gameObject.activeSelf)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}
		private void OnDestroy()
		{
			if (_stockWindow != null)
			{
				_stockWindow.Close();
			}
		}
		public void ConsoleStocksOps(string buySell, uint companyId, uint shares)
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
		public NewStock FindMarketLot(Company seller, uint desiredShares, bool findPlayerLot)
		{
			if (seller == null) return null;

			if (!findPlayerLot)
			{
				try
				{
					if (seller.NewStock == null || seller.NewStock.Count == 0) return null;

					ConsoleWrite("Searching market lots");
					NewStock found = seller.NewStock
						.OrderByDescending(n => n.Shares)
						.FirstOrDefault(n => n.Shares >= desiredShares);

					if (found == null)
					{
						ConsoleWrite("No lot with sufficient shares found, picking largest available lot.");
						found = seller.NewStock.OrderByDescending(n => n.Shares).FirstOrDefault();
					}

					foreach (NewStock stock in GameSettings.Instance.MyCompany.NewOwnedStock)
					{
						if (found == stock)
						{
							ConsoleWrite("FindMarketLot: Skipping own stock lot.");
							found = seller.NewStock.Where(n => n != stock).OrderByDescending(n => n.Shares).Last();
							if (found == stock)
							{
								ConsoleWrite("FindMarketLot: No valid market lot found (all owned by player).");
								return null;
							}
						}
					}

					return found;
				}
				catch (Exception ex)
				{
					ConsoleWrite("FindMarketLot error: " + ex.ToString());
					return null;
				}
			}
			else
			{
				foreach (NewStock stock in seller.NewStock)
				{
					if (stock.Buyer == GameSettings.Instance.MyCompany)
					{
						ConsoleWrite("FindMarketLot: Found player-owned lot.");
						return stock;
					}
				}
				ConsoleWrite("FindMarketLot: No player-owned lot found.");
				return null;
			}
		}
		public void UseStockButtonForPurchase(NewStock stock, uint shares)
		{
			if (stock == null)
			{
				ConsoleWrite("UseStockButtonForPurchase: stock is null.");
				return;
			}

			if (GameSettings.Instance == null)
			{
				ConsoleWrite("Game not ready for UI-based purchase.");
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
				ConsoleWrite("Exception using StockButton UI path: " + ex.Message);
				ConsoleWrite(ex.ToString());
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
		public void ConsoleWrite(string msg)
		{
			DevConsole.Console.Log($"{Main._Name}: {msg}");
		}
		public Company GetCompanyById(uint companyId)
		{
			if (GameSettings.Instance == null || GameSettings.Instance.simulation == null)
			{
				ConsoleWrite("GetCompanyById: GameSettings or simulation not available.");
				return null;
			}

			return GameSettings.Instance.simulation.GetCompany(companyId);
		}
	}
	public class StocksButton
	{
		private static string Name = "Stocks Interface";
		private static Sprite StocksButtonImage;

		public static Button stocksButton(UnityAction toggle = null)
		{
			Button button = WindowManager.SpawnButton();
			button.GetComponentInChildren<Text>().text = "Stocks";
			
			if (toggle != null)
			{
				button.onClick.AddListener(toggle);
			}

			button.name = "StocksButton";
			
			WindowManager.AddElementToElement(
				button.gameObject,
				WindowManager.FindElementPath("MainPanel/Holder/MainBottomPanel/Finance/ButtonPanel").gameObject,
				new Rect(0f, 0f, 100f, 50f),
				new Rect(0f, 0f, 0f, 0f));

			return button;
		}
	}
}
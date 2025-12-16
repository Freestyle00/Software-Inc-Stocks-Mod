using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Software_Inc_Stocks_Mod
{
	public class StocksUI : MonoBehaviour
	{
		private GUIWindow _stockWindow;
		private Text _displayText;
		private System.Random _random;
		private List<string> _randomTexts;

		private void InitializeTexts()
		{
			_randomTexts = new List<string>();
			_randomTexts.Add("Stock prices fluctuate daily!");
			_randomTexts.Add("Diversify your portfolio wisely.");
			_randomTexts.Add("Market trends are unpredictable.");
			_randomTexts.Add("Buy low, sell high!");
			_randomTexts.Add("Risk management is key.");
			_randomTexts.Add("Monitor your investments closely.");
			_randomTexts.Add("Long-term growth requires patience.");
			_randomTexts.Add("Market volatility creates opportunities.");
			_randomTexts.Add("Research before you invest.");
			_randomTexts.Add("Compound interest works wonders.");
		}

		private void Awake()
		{
			_random = new System.Random();
			InitializeTexts();
		}

		private void Update()
		{
			DevConsole.Console.Log("StocksPro: Window updated");
			RefreshText();
		}
		public void InitStockPanel()
		{
			_stockWindow = WindowManager.SpawnWindow();
			_stockWindow.InitialTitle = _stockWindow.TitleText.text = _stockWindow.NonLocTitle = Main._Name;
			_stockWindow.MinSize.x = 600f;
			_stockWindow.MinSize.y = 400f;
			_stockWindow.name = "StockProInterface";
			_stockWindow.MainPanel.name = "StockProMainPanel";

			Button closeButton = _stockWindow.GetComponentsInChildren<Button>().SingleOrDefault(x => x.name == "CloseButton");
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(() => _stockWindow.Close());
			}

			CreateTextDisplay();
			_stockWindow.Show();
		}

		private void CreateTextDisplay()
		{
			if (_stockWindow == null || _stockWindow.MainPanel == null)
			{
				Debug.LogError("StocksUI: Window or MainPanel is null");
				return;
			}

			GameObject textContainerGO = new GameObject("TextContainer", typeof(RectTransform));
			textContainerGO.transform.SetParent(_stockWindow.MainPanel.transform, worldPositionStays: false);
			
			RectTransform containerRect = textContainerGO.GetComponent<RectTransform>();
			containerRect.anchorMin = Vector2.zero;
			containerRect.anchorMax = Vector2.one;
			containerRect.offsetMin = new Vector2(10f, 10f);
			containerRect.offsetMax = new Vector2(-10f, -10f);

			Image containerBg = textContainerGO.AddComponent<Image>();
			containerBg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

			GameObject textGO = new GameObject("RandomText", typeof(RectTransform));
			textGO.transform.SetParent(textContainerGO.transform, worldPositionStays: false);

			RectTransform textRect = textGO.GetComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = Vector2.zero;
			textRect.offsetMax = Vector2.zero;

			_displayText = textGO.AddComponent<Text>();
			_displayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			_displayText.fontSize = 18;
			_displayText.fontStyle = FontStyle.Bold;
			_displayText.alignment = TextAnchor.MiddleCenter;
			_displayText.color = Color.white;
			_displayText.text = GetRandomText();

			LayoutElement layoutElement = textGO.AddComponent<LayoutElement>();
			layoutElement.preferredHeight = 100f;
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
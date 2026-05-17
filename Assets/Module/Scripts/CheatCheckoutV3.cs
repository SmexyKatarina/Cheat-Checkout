using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using rnd = UnityEngine.Random;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

public enum WifiGlitchType
{
	NONE = 2,
	TEXT_GLITCH = 1,
	LCD_DISCONNECT = 0
}

public class CheatCheckoutV3 : MonoBehaviour
{

	public KMBombInfo _bomb;
	public KMAudio _audio;
	public KMColorblindMode _colorblindMode;
	bool _colorblindActive;

	public KMSelectable[] _displayButtons;
	public KMSelectable[] _priceButtons;
	public KMSelectable[] _actionsButtons;

	public KMSelectable[] _allButtons;

	public TextMesh[] _displayTexts;
	public TextMesh[] _priceTexts;
	public TextMesh[] _actionTexts;
	public TextMesh[] _colorblindTexts;
	public TextMesh _customerText;

	public TextMesh[] _allTexts;

	public SpriteRenderer[] _wifiSymbols;
	public SpriteRenderer[] _VPNSymbols;
	public SpriteRenderer[] _cryptoSymbols;

	public AudioClip _HIPSound;

	private static int _modIDCount = 1;
	private int _modID;
	private bool _modSolved = false;

	private static readonly int[][] _positionJumbles = new int[][]
	{
		new int[] { 0, 2, 4, 6, 8, 10, 12 },
		new int[] { 1, 3, 5, 7, 9, 11, 13 },
		new int[] { 0, 1, 4, 6 ,8, 11, 14 },
		new int[] { 1, 2, 4, 8, 12, 14, 16 },
		new int[] { 2, 6, 8, 10, 12, 16 }
	};

	private int _positionPointer = 0;

	/// <summary>
	/// The solved string arrays that replace text on buttons on solve
	/// </summary>
	private static readonly string[] _solvedStringArray = new string[] // display1:display2:12pricebuttons:submit:clear:stabilize:patch
    {
		"YOU:DID IT!:YOUSOLVEDTHE:HARDEST:CHECKOUT:MODULE:POGCHAMP!",
		"THAT:WAS NICE:OWOOWOOWOOWO:OWO:OWO:OWO:OWO",
		"EVERYTHING:IS:FINE........:.....:.....:HA HA:GOTTEM",
		"_:CHLOE,:____________:_:_:_:LOOK OUT.",
		"YOU:CAN COUNT:UNLIKEME;KAT:WHO:CAN'T:COUNT:-w-",
		"YOU:GOT:EVERYTHING__:WRONG!:DO:IT:AGAIN",
		"AM I:A MODULE?:ORISTHISALL_:IN:MY:WIRING?:_"
	};

	/// <summary>
	/// The possible websites in the module
	/// </summary>
	private static readonly Website[] _websites =
	{
		new Website("repost.com", 74, WebsiteType.SOCIAL_MEDIA),
		new Website("pointercat.com", 19, WebsiteType.GAME),
		new Website("usb.os", 37, WebsiteType.SEARCH_ENGINE),
		new Website("color.org", 41, WebsiteType.SEARCH_ENGINE),
		new Website("ktane.timwi.de", 95, WebsiteType.INFO),
		new Website("lol.gg", 8, WebsiteType.SOCIAL_MEDIA),
		new Website("velvet.ss", 58, WebsiteType.STREAMING),
		new Website("watch.tv", 61, WebsiteType.STREAMING),
		new Website("onion.co", 88, WebsiteType.SEARCH_ENGINE),
		new Website("flybird.tv", 20, WebsiteType.STREAMING),
		new Website("sellcoin.org", 61, WebsiteType.INFO),
		new Website("collection.com", 59, WebsiteType.INFO),
		new Website("razor.pt", 66, WebsiteType.SEARCH_ENGINE),
		new Website("checkout.kt", 38, WebsiteType.GAME),
		new Website("crunch.bg", 52, WebsiteType.GAME),
		new Website("locco.pt", 67, WebsiteType.SOCIAL_MEDIA),
		new Website("plant.tr", 12, WebsiteType.INFO),
		new Website("cartoon.com", 69, WebsiteType.STREAMING),
		new Website("blogsite.co", 71, WebsiteType.SOCIAL_MEDIA),
		new Website("voila.lc", 20, WebsiteType.SOCIAL_MEDIA),
		new Website("ktane.gov", 94, WebsiteType.INFO),
		new Website("loli.co", 88, WebsiteType.GAME),
		new Website("anime.st", 41, WebsiteType.STREAMING),
		new Website("medicalsite.co", 92, WebsiteType.INFO),
		new Website("recoil.pt", 82, WebsiteType.SEARCH_ENGINE),
		new Website("numerical.ss", 35, WebsiteType.INFO),
		new Website("isight.com", 26, WebsiteType.STREAMING),
		new Website("symbolic.co", 54, WebsiteType.GAME),
		new Website("grocery.st", 58, WebsiteType.GAME),
		new Website("galaxydeliver.com", 40, WebsiteType.SEARCH_ENGINE),
		new Website("vilesight.ei", 86, WebsiteType.SOCIAL_MEDIA),
		new Website("random.site", 100, WebsiteType.SEARCH_ENGINE)
	};

	/// <summary>
	/// The possible cryptos
	/// </summary>
	private static readonly Crypto[] _possibleCryptos = new Crypto[]
	{
		new Crypto("Berr", 4.4),
		new Crypto("Bitdrop", 111),
		new Crypto("Blade", 1234),
		new Crypto("Crane", 25),
		new Crypto("Evol", 69),
		new Crypto("Lapel", 42),
		new Crypto("Linecoin", 420),
		new Crypto("Penpoint", 777),
		new Crypto("Qubit", 0.5),
	};

	/// <summary>
	/// The gerenated string of characters to use on jumbled/hacked text.
	/// </summary>
	private StringPoolGenerator _jumbleString;

	/// <summary>
	/// The current weekday
	/// </summary>
	private DayOfWeek _weekday;

	/// <summary>
	/// The generator for the hacks
	/// </summary>
	private HackGenerator _hackGenerator;

	/// <summary>
	/// The list of chosen websites for the current module
	/// </summary>
	private List<Website> _chosenWebsites;

	/// <summary>
	/// The chosen crypto for the current module.
	/// </summary>
	private Crypto _chosenCrypto;

	/// <summary>
	/// The number of slaps
	/// </summary>
	private int _slaps = 0;

	/// <summary>
	/// The amount being given by the customer
	/// </summary>
	private double _customerPrice = 0.0d;
	/// <summary>
	/// The change being submitted back to the customer
	/// </summary>
	private double _submittingChange = 0.0d;

	/// <summary>
	/// The current index along the hacks
	/// </summary>
	private int _displayHackIndex = 0;
	/// <summary>
	/// The current index of the information cycle of the hack
	/// </summary>
	private int _cycleIndex = -1;
	/// <summary>
	/// The values that are being displayed for tha thack.
	/// </summary>
	private string[] _displayHackValues;

	/// <summary>
	/// All timer functions
	/// </summary>
	private Timers _timers;

	/// <summary>
	/// The status for the wifi
	/// </summary>
	public int _wifiStatus = 2;
	/// <summary>
	/// The glitch type of the display
	/// </summary>
	public WifiGlitchType _wifiGlitchType = WifiGlitchType.NONE;

	/// <summary>
	/// The status for the VPN
	/// </summary>
	public int _vpnStatus = 2;
	/// <summary>
	/// The array of buttons being affected by the patch glitching
	/// </summary>
	public int[] _glitchedButtons;

	/// <summary>
	/// The boolean to check if a hack is currently in progress when VPN is at RED.
	/// </summary>
	public bool _hackInProgress = false;

	private Coroutine _moduleCycleTimer;
	private Coroutine _vpnGlitchTimer;
	private Coroutine _hackTimer;
	private Coroutine _soundPlayer;
	private Coroutine _hackerFunni;

	void Awake()
	{
		_modID = _modIDCount++;
		_weekday = DateTime.Now.DayOfWeek;
		_jumbleString = new StringPoolGenerator(10000, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()");
	}

	void Start()
	{
		_timers = new Timers(this);
		GenerateModule();

		_colorblindActive = _colorblindMode.ColorblindModeActive;
		ShowColorblindText(_colorblindActive);

		foreach (KMSelectable displayButton in _displayButtons)
		{
			displayButton.OnInteract += delegate ()
			{
				if (_modSolved || _hackInProgress) return false;

				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, displayButton)))
				{
					GetComponent<KMBombModule>().HandleStrike();
					Debug.LogFormat("[Cheat Checkout #{0}] Strike! You clicked a glitched button!", _modID);
					return false;
				}

				DisplayButtons(displayButton);
				return false;
			};
		}

		foreach (KMSelectable priceButton in _priceButtons)
		{
			priceButton.OnInteract += delegate ()
			{
				if (_modSolved || _hackInProgress) return false;

				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, priceButton)))
				{
					GetComponent<KMBombModule>().HandleStrike();
					Debug.LogFormat("[Cheat Checkout #{0}] Strike! You clicked a glitched button!", _modID);
					return false;
				}

				PriceButtons(priceButton);
				return false;
			};
		}

		foreach (KMSelectable actionButton in _actionsButtons)
		{
			actionButton.OnInteract += delegate ()
			{
				if (_modSolved) return false;

				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, actionButton)))
				{
					GetComponent<KMBombModule>().HandleStrike();
					Debug.LogFormat("[Cheat Checkout #{0}] Strike! You clicked a glitched button!", _modID);
					return false;
				}

				ActionButtons(actionButton);
				return false;
			};
		}

	}

	private float _fixedUpdateTimer = 0f;
	private float _fixedFrameCycle = 2;

	void FixedUpdate()
	{
		_fixedUpdateTimer++;

		if (_fixedUpdateTimer == _fixedFrameCycle)
		{
			_fixedUpdateTimer -= _fixedFrameCycle;
			if (_vpnStatus == 1)
			{
				foreach (int pos in _glitchedButtons)
				{
					// Get the button that is affected
					KMSelectable affectedButton = _allButtons[pos];
					// Get the text of the affected button
					TextMesh tm = _allTexts[pos];
					// Change the background of the button
					affectedButton.gameObject.GetComponent<MeshRenderer>().materials[0].color = new Color32(255, 209, 26, 255);
					// Generate the jumbled text for the button
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < tm.text.Replace(".", "").Length; i++)
						sb.Append(_jumbleString.GetNextChar());
					tm.text = sb.ToString();
					tm.color = new Color(255, 0, 0, 255);
				}
			}
			if (_wifiGlitchType == WifiGlitchType.TEXT_GLITCH && _cycleIndex != -1)
			{
				int[] jumblePos = _positionJumbles[_positionPointer];
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < _displayHackValues[_cycleIndex].Length; i++)
				{
					sb.Append(jumblePos.Contains(i) && _displayHackValues[_cycleIndex][i] != ' ' ? _jumbleString.GetNextChar() : _displayHackValues[_cycleIndex][i]);
				}
				_displayTexts[2].text = sb.ToString();
			}
			if (_wifiGlitchType == WifiGlitchType.LCD_DISCONNECT)
			{
				_displayButtons[2].gameObject.GetComponent<MeshRenderer>().materials[0].color = new Color32(80, 0, 0, 255);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < _displayTexts[2].text.Length; i++)
					sb.Append(_jumbleString.GetNextChar());
				_displayTexts[2].text = sb.ToString();
				_displayTexts[2].color = new Color(255, 0, 0, 255);
			}
		}
	}

	void GenerateModule()
	{
		StopAllCoroutines();

		// Generate the string and needed initializers.
		_hackGenerator = new HackGenerator();
		_chosenWebsites = new List<Website>();

		// Choose the crypto
		int crypto = rnd.Range(0, _possibleCryptos.Length);
		_chosenCrypto = _possibleCryptos[crypto];
		_cryptoSymbols[crypto].enabled = true;

		Debug.LogFormat("[Cheat Checkout #{0}] The chosen crypto was {1} (Priced at: {2})", _modID, _chosenCrypto.Name, _chosenCrypto.Price);

		// Generate the websites and hacks
		for (int i = 0; i < 5; i++)
		{
			_chosenWebsites.Add(_websites[rnd.Range(0, _websites.Length)]);
			IHack hack = _hackGenerator.Generate(_chosenWebsites.Last(), _weekday, _chosenCrypto, (HackType)rnd.Range(0, 5));
			foreach (string s in hack.GetLogInfo(_modID, i + 1)) Debug.Log(s);
		}

		Debug.LogFormat("[Cheat Checkout #{0}] The totals (after crypto conversion and rounding): {1}", _modID, _hackGenerator.GetHackCryptoTotals());

		// Generate the customers price, sometimes being under.
		_customerPrice = Math.Round(rnd.Range((float)_hackGenerator.GetHackCryptoTotals() * 0.8f, (float)_hackGenerator.GetHackCryptoTotals() * 1.2f), 3);
		_customerText.text = _customerPrice.ToString();

		Debug.LogFormat("[Cheat Checkout #{0}] The customer is offering {1}. This is {2}",
			_modID,
			_customerPrice,
			_customerPrice >= _hackGenerator.GetHackCryptoTotals()
				? string.Format("enough and their change would be {0}.", Math.Round(_customerPrice - _hackGenerator.GetHackCryptoTotals(), 3))
				: "not enough and requires a good ol' lesson."
			);

		_displayHackValues = _hackGenerator.GetHacks()[0].GetDisplayValues();
		_displayTexts[2].text = "Hack #" + (_displayHackIndex + 1);

		_moduleCycleTimer = StartCoroutine(_timers.ModuleCycle(60f, 30f));

		// Add these incase the module resets
		UpdateWifiStatus(2);
		UpdateVPNStatus(2);
	}

	void SolveSequence()
	{
		string solveString = _solvedStringArray[rnd.Range(0, _solvedStringArray.Length)];
		for (int i = 0; i < _cryptoSymbols.Length; i++)
		{
			_cryptoSymbols[i].enabled = false;
		}
		ShowColorblindText(false);
		for (int i = 0; i < 3; i++)
		{
			_wifiSymbols[i].enabled = false;
			_VPNSymbols[i].enabled = false;
		}
		_customerText.color = new Color32(96, 188, 84, 255);
		StartCoroutine(_timers.SolveAnimation(solveString, _customerText, _allTexts));
	}

	void ShowColorblindText(bool colorblindActive)
	{
		for (int i = 0; i < _colorblindTexts.Length; i++)
			_colorblindTexts[i].gameObject.SetActive(colorblindActive);
	}

	/// <summary>
	/// Slap the customer in order to get a better price.
	/// </summary>
	void SlapCustomer()
	{
		// Increase the slaps for logging
		_slaps++;
		// Generate the new customer price by increasing it withing a range.
		_customerPrice = Math.Round(_customerPrice * rnd.Range(1.1f, 1.25f), 3);
		// Update text.
		_customerText.text = _customerPrice.ToString();
		Debug.LogFormat("[Cheat Checkout #{0}] The customer is now offering {1}. This is {2}",
			_modID,
			_customerPrice,
			_customerPrice >= _hackGenerator.GetHackCryptoTotals()
				? string.Format("enough and their change would be {0}.", Math.Round(_customerPrice - _hackGenerator.GetHackCryptoTotals(), 3))
				: "not enough and requires another good ol' lesson."
			);
	}

	/// <summary>
	/// Left -> 0<br/>
	/// Right -> 1<br/>
	/// Display -> 2
	/// </summary>
	/// <param name="btn">The button passed</param>
	void DisplayButtons(KMSelectable btn)
	{
		int index = Array.IndexOf(_displayButtons, btn);
		btn.AddInteractionPunch();
		switch (index)
		{
			case 0:
				if (_displayHackIndex == 0) return;
				_displayHackIndex--;
				_displayHackValues = _hackGenerator.GetHacks()[_displayHackIndex].GetDisplayValues();
				_cycleIndex = -1;
				_displayTexts[2].text = "Hack #" + (_displayHackIndex + 1);
				break;
			case 1:
				if (_displayHackIndex == 4) return;
				_displayHackIndex++;
				_displayHackValues = _hackGenerator.GetHacks()[_displayHackIndex].GetDisplayValues();
				_cycleIndex = -1;
				_displayTexts[2].text = "Hack #" + (_displayHackIndex + 1);
				break;
			case 2:
				if (_wifiGlitchType == WifiGlitchType.LCD_DISCONNECT) return;
				if (++_cycleIndex == _displayHackValues.Length)
				{
					_cycleIndex = -1;
					_displayTexts[2].text = "Hack #" + (_displayHackIndex + 1);
				} else
				{
					if (_wifiGlitchType == WifiGlitchType.NONE)
					{
						_displayTexts[2].text = _displayHackValues[_cycleIndex];
					} else if (_wifiGlitchType == WifiGlitchType.TEXT_GLITCH)
					{
						_positionPointer = _positionPointer + 1 >= _positionJumbles.Length ? 0 : _positionPointer + 1;
					}
				}
				break;
		}
	}

	/// <summary>
	/// Submit -> 0<br/>
	/// Clear -> 1<br/>
	/// Stabilize -> 2<br/>
	/// Patch -> 3
	/// </summary>
	/// <param name="btn">The button passed</param>
	void ActionButtons(KMSelectable btn)
	{
		int index = Array.IndexOf(_actionsButtons, btn);
		if (_hackInProgress && index != 3) return;
		btn.AddInteractionPunch();
		int timePressed = GetBombSeconds();
		int timerLast = timePressed % 10;
		int snSum = _bomb.GetSerialNumberNumbers().Sum();
		int snLast = _bomb.GetSerialNumberNumbers().Last();
		switch (index)
		{
			// Submit button
			case 0:
				double total = _hackGenerator.GetHackCryptoTotals();
				double minRange = Math.Round(_customerPrice - total - 0.01d, 3);
				double maxRange = Math.Round(_customerPrice - total + 0.01d, 3);
				bool inRange = minRange <= _submittingChange && _submittingChange <= maxRange;
				// If the customer needs to be slapped and there is no current change being submitted
				if (_customerPrice < total && _submittingChange == 0.0d)
				{
					Debug.LogFormat("[Cheat Checkout #{0}] Slapping customer, total {1} times.", _modID, _slaps + 1);
					SlapCustomer();
					break;
				}
				// If the customer needs to be slapped and there is changed trying to be submitted
				else if (_customerPrice < total && _submittingChange != 0.0d)
				{
					GetComponent<KMBombModule>().HandleStrike();
					Debug.LogFormat("[Cheat Checkout #{0}] Strike! You must slap the customer due to their price being less than the total. Customer is giving {1} and the total is {2}.", _modID, _customerPrice, total);
					break;
				}
				// If the change being submitted is not within range of the answer
				else if (!inRange)
				{
					GetComponent<KMBombModule>().HandleStrike();
					Debug.LogFormat("[Cheat Checkout #{0}] Strike! You gave {1} change back, but you were supposed to give them back {2} (within +- 0.01 range so {3}-{4}) ", _modID, _submittingChange, Math.Round(_customerPrice - total, 3), minRange, maxRange);
					break;
				}
				// If the correct change is submitted within range.
				else
				{
					GetComponent<KMBombModule>().HandlePass();
					_modSolved = true;
					Debug.LogFormat("[Cheat Checkout #{0}] Solved! Playing solve animation...", _modID);
					StopAllCoroutines();
					SolveSequence();
					break;
				}
			// Clear
			case 1:
				_customerText.color = new Color32(96, 188, 84, 255);
				_customerText.text = _customerPrice.ToString();
				_submittingChange = 0.0d;
				break;
			// Stabilize
			case 2:
				switch (_wifiStatus)
				{
					// Green
					case 2:
						break;
					// Yellow, success if the sum of the serial number's digits and the seconds of the timer match
					// Sum of SN#'s = 16, means timer must equal 16 in order stabilize.
					case 1:
						if (timePressed == snSum)
						{
							Debug.LogFormat("[Cheat Checkout #{0}] Wifi was stabilized.", _modID);
							UpdateWifiStatus(2);
						}
						else
						{
							GetComponent<KMBombModule>().HandleStrike();
							Debug.LogFormat("[Cheat Checkout #{0}] Strike! You tried to stabilize your wifi at {1} but you needed to stablize at {2}.", _modID, timePressed, snSum);
						}
						break;
					// Red, success if the last digit of the timer matches the last digit of the serial number
					// Last SN digit = 6, means last digit of timer equals 6 in order to stabilize
					case 0:
						if (timerLast == snLast)
						{
							Debug.LogFormat("[Cheat Checkout #{0}] Wifi was reset.", _modID);
							UpdateWifiStatus(2);
						}
						else
						{
							GetComponent<KMBombModule>().HandleStrike();
							Debug.LogFormat("[Cheat Checkout #{0}] Strike! You tried to reset your router at {1} but you needed to reset it at {2}", _modID, timerLast, snLast);
						}
						break;
				}
				break;
			// Patch
			case 3:
				switch (_vpnStatus)
				{
					// Green
					case 2:
						break;
					// Yellow, success at any time.
					case 1:
						Debug.LogFormat("[Cheat Checkout #{0}] VPN was repaired.", _modID);
						UpdateVPNStatus(2);
						break;
					// Red, scary mode owo. Patch at the last digit of SN and timer equal the same.
					// Last SN digit = 6, means last digit of timer equals 6 in order to patch.
					case 0:
						if (timerLast == snLast)
						{
							Debug.LogFormat("[Cheat Checkout #{0}] VPN was repaired.", _modID);
							UpdateVPNStatus(2);
						}
						else
						{
							GetComponent<KMBombModule>().HandleStrike();
							_hackInProgress = false;
							Debug.LogFormat("[Cheat Checkout #{0}] Strike! You tried to restart your VPN at {1} but you needed to restart it at {2}. The hackers have taken over the module and have now reset it!", _modID, timerLast, snLast);
							GenerateModule();
						}
						break;
				}
				break;
		}
	}

	/// <summary>
	/// 0.001 -> 0<br/>
	/// 0.01 -> 1<br/>
	/// 0.1 -> 2<br/>
	/// 1 -> 3<br/>
	/// 10 -> 4<br/>
	/// 100 -> 5<br/>
	/// 0.005 -> 6<br/>
	/// 0.05 -> 7<br/>
	/// 0.5 -> 8<br/>
	/// 5 -> 9<br/>
	/// 50 -> 10<br/>
	/// 500 -> 11
	/// </summary>
	/// <param name="btn">The button passed</param>
	void PriceButtons(KMSelectable btn)
	{
		int index = Array.IndexOf(_priceButtons, btn);
		btn.AddInteractionPunch();
		double[] prices = { 0.001, 0.01, 0.1, 1, 10, 100, 0.005, 0.05, 0.5, 5, 50, 500 };
		_submittingChange += prices[index];
		_customerText.text = _submittingChange.ToString();
		_customerText.color = new Color32(180, 60, 100, 255);
	}

	/// <summary>
	/// Update the Wifi status and its various other systems
	/// </summary>
	/// <param name="level">The level 0, 1 or 2 (red, yellow, green)</param>
	public void UpdateWifiStatus(int level)
	{
		_wifiSymbols[_wifiStatus].enabled = false;
		_wifiStatus = level;
		_wifiSymbols[_wifiStatus].enabled = true;
		_wifiGlitchType = (WifiGlitchType)_wifiStatus;
		_colorblindTexts[0].text = new string[] { "R", "Y", "G" }[_wifiStatus];
		FixTexts();
		_displayButtons[2].gameObject.GetComponent<MeshRenderer>().materials[0].color = new Color32(0, 0, 0, 255);
	}

	/// <summary>
	/// Update the VPN status and its various other systems
	/// </summary>
	/// <param name="level">The level 0, 1 or 2 (red, yellow, green)</param>
	public void UpdateVPNStatus(int level)
	{
		_VPNSymbols[_vpnStatus].enabled = false;
		_vpnStatus = level;
		_VPNSymbols[_vpnStatus].enabled = true;
		_colorblindTexts[1].text = new string[] { "R", "Y", "G" }[_vpnStatus];
		_glitchedButtons = new int[] { };
		switch (_vpnStatus)
		{
			case 2:
				if (_vpnGlitchTimer != null) StopCoroutine(_vpnGlitchTimer);
				if (_hackTimer != null) StopCoroutine(_hackTimer);
				if (_soundPlayer != null) StopCoroutine(_soundPlayer);
				if (_hackerFunni != null) StopCoroutine(_hackerFunni);
				if (_moduleCycleTimer == null) StartCoroutine(_timers.ModuleCycle(60f, 30f));
				_hackInProgress = false;
				_customerText.fontSize = 64;
				_customerText.text = _customerPrice.ToString();
				_cryptoSymbols.First(x => x.name.Contains(_chosenCrypto.Name)).enabled = true;
				FixTexts();
				FixButtons();
				break;
			case 1:
				_vpnGlitchTimer = StartCoroutine(_timers.VPN_GlitchCycle(15f, 5f, 8));
				break;
			case 0:
				if (_hackInProgress) return;
				UpdateWifiStatus(2);
				_hackInProgress = true;
				Debug.Log(_moduleCycleTimer);
				if (_moduleCycleTimer != null) StopCoroutine(_moduleCycleTimer);
				if (_vpnGlitchTimer != null) StopCoroutine(_vpnGlitchTimer);
				Debug.Log(_moduleCycleTimer);
				FixTexts();
				FixButtons();
				_cryptoSymbols.First(x => x.name.Contains(_chosenCrypto.Name)).enabled = false;
				_hackTimer = StartCoroutine(_timers.InitiateHack(30f, _displayTexts[2]));
				_hackerFunni = StartCoroutine(_timers.HackerText(2f, _customerText));
				_soundPlayer = StartCoroutine(_timers.WarningSound(2.5f, _audio, _HIPSound, transform));
				break;
		}
	}

	/// <summary>
	/// Get the seconds on the bomb
	/// </summary>
	/// <returns>Returns the seconds remaining on the bomb's seconds side. Regardless if its less than a minute.</returns>
	public int GetBombSeconds()
	{
		Match match = Regex.Match(_bomb.GetFormattedTime(), @"(?:\d+:(\d{2}))|(?:(\d{2})\.\d+)");
		Group sec = match.Groups[1].Success ? match.Groups[1] : match.Groups[2];
		return Convert.ToInt32(sec.Value);
	}

	/// <summary>
	/// Method for hacking into the module after the 30 second countdown
	/// </summary>
	public void HackModule()
	{
		GetComponent<KMBombModule>().HandleStrike();
		_hackInProgress = false;
		Debug.LogFormat("[Cheat Checkout #{0}] Strike! You didn't fix your VPN in time and the hackers have access to module and have reset it!", _modID);
		GenerateModule();
	}

	/// <summary>
	/// Reset the texts on the module incase there are changes to them due to other systems on the module
	/// </summary>
	public void FixTexts()
	{
		foreach (TextMesh t in _allTexts)
		{
			string name = t.name;
			switch (name.ToLowerInvariant())
			{
				case "left_tx":
					t.text = "◀";
					break;
				case "right_tx":
					t.text = "▶";
					break;
				case "lcd_tx":
					if (_cycleIndex != -1)
					{
						t.text = _displayHackValues[_cycleIndex];
					}
					else
					{
						t.text = "Hack #" + (_displayHackIndex + 1);
					}
					break;
				default:
					t.text = name;
					break;
			}
			t.color = new Color32(96, 188, 84, 255);
		}
	}

	/// <summary>
	/// Fix the colors of the buttons
	/// </summary>
	public void FixButtons()
	{
		foreach (MeshRenderer mr in _allButtons.Select(x => x.gameObject.GetComponent<MeshRenderer>()))
		{
			mr.materials[0].color = new Color32(0, 0, 0, 255);
		}
	}
#pragma warning disable 0414
	private readonly string TwitchHelpMessage = @"!{0} hack <1-5> [Goes to 1-5 hack], !{0} lcd/screen/display <delay in between each info, default 2s> [Cycle the display with a default or custom delay], !{0} submit/sub <change> [Submit with no arguments to slap the customer, otherwise add the change to submit. Clears before submitting/slapping.], !{0} clear [Clears the input], !{0} stabilize/stbl <#/##> [Presses 'Stabilize' on the time specified.], !{0} patch <#> [Press 'Patch' on #, leave blank if at anytime], !{0} cb/colorblind/colourblind [Toggles colourblind mode].";
#pragma warning restore 0414

	private IEnumerator ProcessTwitchCommand(string cmd)
	{
		RegexOptions regOpts = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
		Match match;

		int c_wifiStatus = _wifiStatus;
		int c_vpnStatus = _vpnStatus;

		// !{0} hack {1-5}
		if ((match = Regex.Match(cmd, @"^hack\s([1-5])$", regOpts)).Success)
		{
			int toHack = Convert.ToInt32(match.Groups[1].Value) - 1;
			if (toHack == _displayHackIndex) yield break;
			int dir = _displayHackIndex < toHack ? 1 : 0;
			while (_displayHackIndex != toHack)
			{
				yield return "trycancel Cancelled hack change action.";
				yield return null;
				if (c_vpnStatus != _vpnStatus || _wifiStatus != c_wifiStatus)
				{
					yield return "sendtochat Stopped hack change due to status changing";
					yield break;
				}
				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _displayButtons[dir])))
					continue;
				_displayButtons[dir].OnInteract();
			}
			yield break;
		}
		// !{0} lcd/screen/display {float delay}
		else if ((match = Regex.Match(cmd, @"^(?:lcd|screen|display)(?:\s(\d+(?:\.*\d+)?))?$", regOpts)).Success)
		{
			while (_cycleIndex != -1)
			{
				yield return "trycancel Cancelled display cycle action.";
				yield return null;
				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _displayButtons[2])))
					continue;
				_displayButtons[2].OnInteract();
				yield return new WaitForSeconds(0.01f);
			}
			yield return new WaitForSeconds(0.5f);
			if (_wifiGlitchType == WifiGlitchType.LCD_DISCONNECT)	
			{
				yield return "sendtochat LCD is disabled.";
				yield break;
			}
			float delay = match.Groups[1].Success ? Convert.ToSingle(match.Groups[1].Value) : 2f;
			bool started = false;
			while (true)
			{
				yield return "trycancel Cancelled display cycle action.";
				yield return null;

				if (_vpnStatus != c_vpnStatus || _wifiStatus != c_wifiStatus)
				{
					yield return "sendtochat Stopped cycling due to status changing";
					yield break;
				}

				if ((_cycleIndex != -1 || !started) && !_glitchedButtons.Contains(Array.IndexOf(_allButtons, _displayButtons[2])))
				{
					started = true;
					_displayButtons[2].OnInteract();
					if (_cycleIndex != -1) yield return new WaitForSeconds(delay);
					continue;
				}
				yield break;
			}
		}
		// !{0} submit/sub {float change}
		else if ((match = Regex.Match(cmd, @"^(?:submit|sub)(?:\s(\d+(?:\.*\d{1,3})?))?$", regOpts)).Success)
		{
			while (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _actionsButtons[1])))
			{
				yield return "trycancel Cancelled submission action.";
				yield return null;
			}
			_actionsButtons[1].OnInteract();
			if (match.Groups[1].Success)
			{
				float change = Convert.ToSingle(match.Groups[1].Value);
				KMSelectable[] btns = GetPriceButtonArray(change);
				int idx = 0;
				while (true)
				{
					yield return "trycancel Cancelled submission action.";
					yield return null;

					if (_vpnStatus != c_vpnStatus || _wifiStatus != c_wifiStatus)
					{
						yield return "sendtochat Stopped interaction due to status changing";
						yield break;
					}

					if (idx < btns.Length)
					{
						if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, btns[idx])))
							continue;
						btns[idx++].OnInteract();
					} else
					{
						if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _actionsButtons[0])))
							continue;
						_actionsButtons[0].OnInteract();
						yield break;
					}
				}
			}
			else
			{
				while (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _actionsButtons[0])))
				{
					yield return "trycancel Called submission action.";
					yield return null;
				}
				_actionsButtons[0].OnInteract();
				yield break;
			}
		}
		// !{0} clear
		else if (Regex.Match(cmd, @"^clear$", regOpts).Success)
		{
			while (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _actionsButtons[1])))
			{
				yield return "trycancel Cancelled clear action.";
				yield return null;
			}
			_actionsButtons[1].OnInteract();
			yield break;
		}
		// !{0} stabilize stbl {1 digit / 2 digit}
		else if ((match = Regex.Match(cmd, @"^(?:stabilize|stbl)\s(\d{1,2})$", regOpts)).Success)
		{
			int pressAt = Convert.ToInt32(match.Groups[1].Value);
			while (true)
			{
				yield return "trycancel Cancelled stabilize action.";
				yield return null;
				if (_wifiStatus != c_wifiStatus)
				{
					yield return "sendtochat Stopped press due to wifi status changing.";
					yield break;
				}
				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _actionsButtons[2])))
					continue;
				int seconds = GetBombSeconds();
				if ((_wifiStatus == 1 && seconds != pressAt) || (_wifiStatus == 0 && (seconds % 10) != pressAt))
					continue;
				break;
			}
			_actionsButtons[2].OnInteract();
			yield break;
		}
		// !{0} patch / patch {1 digit}
		else if ((match = Regex.Match(cmd, @"^patch(?:\s(\d+))?$", regOpts)).Success)
		{
			int pressAt = match.Groups[1].Success ? Convert.ToInt32(match.Groups[1].Value) : -1;
			while (true)
			{
				yield return "trycancel Cancelled patch action.";
				yield return null;
				if (_vpnStatus != c_vpnStatus)
				{
					yield return "sendtochat Stopped press due to vpn status changing";
					yield break;
				}
				if (_glitchedButtons.Contains(Array.IndexOf(_allButtons, _actionsButtons[3])))
					continue;
				if (pressAt != -1 && (GetBombSeconds() % 10) != pressAt)
					continue;

				break;
			}
			_actionsButtons[3].OnInteract();
			yield break;
		}
		// !{0} cb/colorblind/colourblind
		else if (Regex.Match(cmd, @"^cb|colorblind|colourblind$", regOpts).Success)
		{
			ShowColorblindText(_colorblindActive = !_colorblindActive);
			yield return "sendtochat Colorblind turned " + (_colorblindActive ? "on" : "off");
			yield break;
		}
		// Unknown command
		else
		{
			yield return "sendtochat Unknown command. Use !{1} help for commands.";
			yield break;
		}
	}

	private IEnumerator TwitchHandleForcedSolve()
	{
		StopAllCoroutines();
		yield return null;
		UpdateWifiStatus(2);
		UpdateVPNStatus(2);
		FixTexts();
		_actionsButtons[1].OnInteract();
		while (_hackGenerator.GetHackCryptoTotals() > _customerPrice) { yield return null; _actionsButtons[0].OnInteract(); }
		foreach (KMSelectable km in GetPriceButtonArray(Math.Round(_customerPrice - _hackGenerator.GetHackCryptoTotals(), 3)))
		{
			km.OnInteract();
			yield return true;
			yield return new WaitForSeconds(0.025f);
		}
		_actionsButtons[0].OnInteract();
		yield break;
	}

	private KMSelectable[] GetPriceButtonArray(double price)
	{
		Debug.Log(price);
		List<KMSelectable> list = new List<KMSelectable>();
		double track = price;
		double[] prices = { 500, 100, 50, 10, 5, 1, 0.5, 0.1, 0.05, 0.01, 0.005, 0.001 };
		for (int i = 0; i < prices.Length; i++)
		{
			if (track <= 0) break;
			int count = (int)(track / prices[i]);
			if (count > 0)
			{
				list.AddRange(Enumerable.Repeat(_priceButtons.First(x => x.name == prices[i].ToString()), count));
				track -= count * prices[i];
				track = Math.Round(track, 3);
			}
		}
		return list.ToArray();
	}
}
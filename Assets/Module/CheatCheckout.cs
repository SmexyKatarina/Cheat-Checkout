using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class CheatCheckout : MonoBehaviour
{
	public KMBombInfo _bomb;
	public KMAudio _audio;
	public KMColorblindMode _colorblindMode;

	public KMSelectable[] _priceButtons;
	public KMSelectable[] _actionButtons;

	public TextMesh[] _displayTexts;
	public TextMesh[] _buttonTexts;
	public TextMesh[] _colorblindTexts;

	public SpriteRenderer[] _wifiSymbols;
	public SpriteRenderer[] _shieldSymbols;
	public SpriteRenderer[] _cryptoSymbols;

	public AudioClip _HIPSound;

	// Specifically for Logging
	static int _modIDCount = 1;
	int _modID;
	private bool _modSolved = false;
	//
	string[] _solvedStringArray = new string[] // display1:display2:12pricebuttons:submit:clear:stabilize:patch
	{
		"YOU:DID IT!:YOUSOLVEDTHE:HARDEST:CHECKOUT:MODULE:POGCHAMP!",
		"THAT:WAS NICE:OWOOWOOWOOWO:OWO:OWO:OWO:OWO",
		"EVERYTHING:IS:FINE........:.....:.....:HA HA:GOTTEM",
		"_:CHLOE,:_:_:_:_:LOOK OUT.",
		"YOU:CAN COUNT:UNLIKEME;KAT:WHO:CAN'T:COUNT:-w-",
		"YOU:GOT:EVERYTHING__:WRONG!:DO:IT:AGAIN",
		"AM I:A MODULE?:ORISTHISALL_:IN:MY:WIRING?:_"
	};

	Color _originalColor = new Color32(96, 188, 84, 255);
	Color _inputColor = new Color32(243, 246, 43, 255);
	string[] _possibleWebsites = new string[] {
		"repost.com:74:SM",
		"pointercat.com:19:GA",
		"usb.os:37:SE",
		"color.org:41:SE",
		"ktane.timwi.de:95:IN",
		"lol.gg:8:SM",
		"velvet.ss:58:ST",
		"watch.tv:61:ST",
		"onion.co:88:SE",
		"flybird.tv:20:ST",
		"sellcoin.org:61:IN",
		"collection.com:59:IN",
		"razor.pt:66:SE",
		"checkout.kt:38:GA",
		"crunch.bg:52:GA",
		"locco.pt:67:SM",
		"plant.tr:12:IN",
		"cartoon.com:69:ST",
		"blogsite.co:71:SM",
		"voila.lc:20:SM",
		"ktane.gov:94:IN",
		"loli.co:88:GA",
		"anime.st:41:ST",
		"medicalsite.co:92:IN",
		"recoil.pt:82:SE",
		"numerical.ss:35:IN",
		"isight.com:26:ST",
		"symbolic.co:54:GA",
		"grocery.st:58:GA",
		"galaxydeliver.com:40:SE",
		"vilesight.ei:86:SM",
		"random.site:100:SE"
	};
	string[] _possibleHacks = new string[] { "DSA", "W", "CI", "XSS", "BFA" }; // Denial of Service Attack, Worm, Code Injection, Cross-Site Scripting, Brute Force Attack
	string[] _possibleCryptos = new string[] { "Berr", "Bitdrop", "Blade", "Crane", "Evol", "Lapel", "Linecoin", "Penpoint", "Qubit" };
	float[] _cryptoPrices = new float[] { 4.4f, 111f, 1234f, 25f, 69f, 42f, 420f, 777f, 0.5f };

	List<object[]> _hackInformation = new List<object[]>();
	List<string> _chosenHackNames = new List<string>();
	List<string> _chosenWebsites = new List<string>();
	int _chosenCryptocurrency = 0;
	DayOfWeek _dow;
	float[] _buttonPrices = new float[] { 0.001f, 0.01f, 0.1f, 1f, 10f, 100f, 0.005f, 0.05f, 0.5f, 5f, 50f, 500f };

	int _wifiStatus = 2; // 2 1 0:Green Yellow Red
	int _shieldStatus = 2; // 2 1 0:Green Yellow Red
	int _staticWifiStatus = -1;
	int _staticShieldStatus = -1;
	int _hackIndex = 0;
	int _hackCycle = -1;
	bool _beingHacked, _blockDisplay, _timerStopPlz, _glitchedButtons, _LCDGlitch, _forcedSolve, _statusLightGreen, _colorblindActive;

	float _totalAmount = 0f;
	float _givenChange = -1f;
	float _customerGave = 0f;
	float _staticCustomerGave = 0f;
	int _customerSlaps = 0;

	List<string> _currentHackInfo;

	bool[] _currentGlitched = new bool[15];

	int _moduleTimer = 75;

	void Awake()
	{
		_modID = _modIDCount++;
		_dow = DateTime.Today.DayOfWeek;

		foreach (KMSelectable km in _priceButtons)
		{
			km.OnInteract += delegate () { if (_modSolved) return false; PriceButtons(km); return false; };
		}

		foreach (KMSelectable km in _actionButtons)
		{
			km.OnInteract += delegate () { if (_modSolved) return false; ActionButtons(km); return false; };
		}

	}

	void Start()
	{
		_colorblindActive = _colorblindMode.ColorblindModeActive;

		ShowColorblindText(_colorblindActive);
		GenerateHackInformation();
		GenerateCustomerPrice();
		//StartCoroutine(ModuleTimer());
		_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
		_currentHackInfo = ParseDisplayHackInfo(_hackIndex);
		StartCoroutine(ModuleTimer());
	}

	void ShowColorblindText(bool colorblindActive)
	{
		for (int i = 0; i < _colorblindTexts.Length; i++)
			_colorblindTexts[i].gameObject.SetActive(colorblindActive);
	}

	void PriceButtons(KMSelectable km)
	{
		int index = Array.IndexOf(_priceButtons, km);
		if (_beingHacked && !_forcedSolve) { return; }
		if (_glitchedButtons && BeingGlitched(km) && !_forcedSolve) { GetComponent<KMBombModule>().HandleStrike(); Debug.LogFormat("[Cheat Checkout #{0}]: Module struck due to the buttons being in their glitched state.", _modID); return; }
		if (_givenChange == -1) _givenChange = 0f;
		_givenChange = (float)Math.Round(_buttonPrices[index] + _givenChange, 3);
		_displayTexts[0].color = _inputColor;
		_displayTexts[0].text = _givenChange.ToString();
	}

	void ActionButtons(KMSelectable km)
	{
		int index = Array.IndexOf(_actionButtons, km);
		if (_beingHacked && _actionButtons[index].name.ToLower() != "patch" && !_forcedSolve) { return; }
		if (_glitchedButtons && _actionButtons[index].name.ToLower() != "patch" && BeingGlitched(km) && !_forcedSolve) { GetComponent<KMBombModule>().HandleStrike(); Debug.LogFormat("[Cheat Checkout #{0}]: Module struck due to the buttons being in their glitched state.", _modID); return; }
		switch (index)
		{
			case 0: // LCD
				if (_blockDisplay) { Debug.LogFormat("[Cheat Checkout #{0}]: Display didn't cycle due to display being blocked.", _modID); break; }
				if (_hackCycle == _currentHackInfo.Count() - 1) { _hackCycle = -1; _displayTexts[1].characterSize = 0.02f; _displayTexts[1].text = "Hack #" + (_hackIndex + 1); break; }
				_hackCycle++;
				_displayTexts[1].characterSize = 0.0125f;
				if (rnd.Range(0, 5) == 0 && _LCDGlitch)
				{
					string chars = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
					string s = "";
					for (int i = 0; i <= 5; i++)
					{
						s += chars[rnd.Range(0, chars.Length)];
					}
					_displayTexts[1].text = s;
					break;
				}
				else
				{
					_displayTexts[1].text = _currentHackInfo[_hackCycle];
					break;
				}
			case 1: // Left
				if (_hackIndex == 0) return;
				_hackIndex--;
				_currentHackInfo = ParseDisplayHackInfo(_hackIndex);
				_displayTexts[1].characterSize = 0.02f;
				_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
				_hackCycle = -1;
				break;
			case 2: // Right
				if (_hackIndex == 4) return;
				_hackIndex++;
				_currentHackInfo = ParseDisplayHackInfo(_hackIndex);
				_displayTexts[1].characterSize = 0.02f;
				_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
				_hackCycle = -1;
				break;
			case 3: // submit
				if (_customerGave == -1f && _givenChange == -1f) { Debug.LogFormat("[Cheat Checkout #{0}]: Slapped the customer for more money.", _modID); _customerSlaps++; GenerateCustomerPrice(); break; }
				if (_givenChange == -1f) { _givenChange = 0f; }
				float answer = (float)Math.Round(_staticCustomerGave - _totalAmount, 3);
				float min = answer - 0.011f;
				float max = answer + 0.011f;
				if (!(_givenChange >= min && _givenChange <= max))
				{
					Debug.LogFormat("[Cheat Checkout #{0}]: Incorrect amount of change. Given {1} but expected {2}.", _modID, _givenChange == -1 ? 0 : _givenChange, Math.Round(_customerGave - _totalAmount, 3));
					GetComponent<KMBombModule>().HandleStrike();
					_displayTexts[0].text = _staticCustomerGave.ToString();
					_displayTexts[0].color = _originalColor;
					_givenChange = -1f;
					break;
				}
				else
				{
					StopAllCoroutines();
					ResetButtonTexts();
					StartCoroutine(SolveAnimation());
				}
				break;
			case 4: // clear
				_givenChange = -1f;
				_displayTexts[0].color = _originalColor;
				_displayTexts[0].text = _staticCustomerGave.ToString();
				break;
			case 5: // stabilize
				if (_wifiStatus == 2) { break; }
				_staticWifiStatus = _wifiStatus;
				if (_staticWifiStatus == 1)
				{
					int sum = _bomb.GetSerialNumberNumbers().Sum();
					int digits = (int)_bomb.GetTime() % 60;
					if (sum == digits)
					{
						StopAllCoroutines();
						if (_shieldStatus == 1) { StartCoroutine(GlitchSeparator()); }
						StartCoroutine(ModuleTimer());
						ResetButtonTexts();
						_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
						_displayTexts[1].color = _originalColor;
						Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Wifi status of {1} and was successful.", _modID, _staticWifiStatus);
						_wifiSymbols[_staticWifiStatus].enabled = false;
						_wifiStatus = 2;
						_wifiSymbols[_wifiStatus].enabled = true;
                        _colorblindTexts[0].text = "G";
                        _colorblindTexts[0].color = new Color32(0, 255, 33, 255);
                        _LCDGlitch = false;
						_blockDisplay = false;
						break;
					}
					else
					{
						Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Wifi status of {1} and was incorrect. Pressed on {2} but expected {3}.", _modID, _staticWifiStatus, digits, sum);
						GetComponent<KMBombModule>().HandleStrike();
						StopAllCoroutines();
						if (_shieldStatus == 1) { StartCoroutine(GlitchSeparator()); }
						StartCoroutine(ModuleTimer());
						ResetButtonTexts();
						_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
						_displayTexts[1].color = _originalColor;
						_wifiSymbols[_staticWifiStatus].enabled = false;
						_wifiStatus = 2;
						_wifiSymbols[_wifiStatus].enabled = true;
                        _colorblindTexts[0].text = "G";
                        _colorblindTexts[0].color = new Color32(0, 255, 33, 255);
                        _LCDGlitch = false;
						_blockDisplay = false;
						break;
					}
				}

				if (_staticWifiStatus == 0)
				{
					int last = _bomb.GetSerialNumberNumbers().Last();
					int digit = (int)_bomb.GetTime() % 10;
					if (last == digit)
					{
						StopAllCoroutines();
						if (_shieldStatus == 1) { StartCoroutine(GlitchSeparator()); }
						StartCoroutine(ModuleTimer());
						ResetButtonTexts();
						_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
						_displayTexts[1].color = _originalColor;
						Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Wifi status of {1} and was successful.", _modID, _staticWifiStatus);
						_wifiSymbols[_staticWifiStatus].enabled = false;
						_wifiStatus = 2;
						_wifiSymbols[_wifiStatus].enabled = true;
                        _colorblindTexts[0].text = "G";
                        _colorblindTexts[0].color = new Color32(0, 255, 33, 255);
                        _LCDGlitch = false;
						_blockDisplay = false;
						break;
					}
					else
					{
						Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Wifi status of {1} and was incorrect. Pressed on {2} but expected {3}.", _modID, _staticWifiStatus, digit, last);
						GetComponent<KMBombModule>().HandleStrike();
						StopAllCoroutines();
						if (_shieldStatus == 1) { StartCoroutine(GlitchSeparator()); }
						StartCoroutine(ModuleTimer());
						ResetButtonTexts();
						_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
						_displayTexts[1].color = _originalColor;
						_wifiSymbols[_staticWifiStatus].enabled = false;
						_wifiStatus = 2;
						_wifiSymbols[_wifiStatus].enabled = true;
						_LCDGlitch = false;
						_blockDisplay = false;
						break;
					}
				}
				break;
			case 6: // patch
				if (_shieldStatus == 2) { break; }
				_staticShieldStatus = _shieldStatus;
				if (_staticShieldStatus == 1)
				{
					StopAllCoroutines();
					StartCoroutine(ModuleTimer());
					ResetButtonTexts();
					Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Hacker Shield status of {1} and was successful.", _modID, _staticShieldStatus);
					_shieldSymbols[_staticShieldStatus].enabled = false;
					_shieldStatus = 2;
					_shieldSymbols[_shieldStatus].enabled = true;
                    _colorblindTexts[1].text = "G";
                    _colorblindTexts[1].color = new Color32(0, 255, 33, 255);
                    _currentGlitched = new bool[15];
					_glitchedButtons = false;
					break;
				}

				if (_staticShieldStatus == 0)
				{
					int last = _bomb.GetSerialNumberNumbers().Last();
					int digit = (int)_bomb.GetTime() % 10;
					if (last == digit)
					{
						StopAllCoroutines();
						StartCoroutine(ModuleTimer());
						_timerStopPlz = true;
						ThyBecomeHacked();
						Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Hacker Shield status of {1} and was successful.", _modID, _staticShieldStatus);
						_shieldSymbols[_staticShieldStatus].enabled = false;
						_shieldStatus = 2;
						_shieldSymbols[_shieldStatus].enabled = true;
						_colorblindTexts[1].text = "G";
						_colorblindTexts[1].color = new Color32(0, 255, 33, 255);
						_currentGlitched = new bool[15];
						_glitchedButtons = false;
						_beingHacked = false;
						break;
					}
					else
					{
						Debug.LogFormat("[Cheat Checkout #{0}]: Module was patched on a Hacker Shield status of {1} and was incorrect. Pressed on {2} but expected {3}.", _modID, _staticShieldStatus, last, digit);
						GetComponent<KMBombModule>().HandleStrike();
						StopAllCoroutines();
						StartCoroutine(ModuleTimer());
						_timerStopPlz = true;
						ThyBecomeHacked();
						_shieldSymbols[_staticShieldStatus].enabled = false;
						_shieldStatus = 2;
						_shieldSymbols[_shieldStatus].enabled = true;
						_currentGlitched = new bool[15];
						_glitchedButtons = false;
						_beingHacked = false;
						break;
					}
				}
				break;
			default:
				break;
		}
	}

	bool BeingGlitched(KMSelectable potent)
	{
		List<KMSelectable> _kms = new List<KMSelectable>();
		if (potent.name.ToLower().EqualsAny("hack lcd", "left", "right", "patch")) return false;
		foreach (KMSelectable km in _priceButtons)
		{
			_kms.Add(km);
		}
		foreach (KMSelectable km in _actionButtons)
		{
			if (km.name.ToLower().EqualsAny("hack lcd", "left", "right", "patch")) continue;
			_kms.Add(km);
		}
		if (_currentGlitched[Array.IndexOf(_kms.ToArray(), potent)]) return true;
		return false;
	}

	void GenerateHackInformation()
	{

		_chosenCryptocurrency = rnd.Range(0, _possibleCryptos.Length);
		_cryptoSymbols[_chosenCryptocurrency].enabled = true;
		Debug.LogFormat("[Cheat Checkout #{0}]: The chosen cryptocurrency for this module is: {1} which is {2}/1 {1}.", _modID, _possibleCryptos[_chosenCryptocurrency], _cryptoPrices[_chosenCryptocurrency]);
		Debug.LogFormat("[Cheat Checkout #{0}]: The day of the week that this module has started on is: {1}.", _modID, _dow.ToString());

		for (int loop = 0; loop <= 4; loop++)
		{
			int index = rnd.Range(0, _possibleHacks.Length);
			string hack = _possibleHacks[index];
			_chosenHackNames.Add(hack);

			float result = 0;

			object[] hinfo = HackInformation(hack);
			string[] website = _possibleWebsites[rnd.Range(0, _possibleWebsites.Length)].Split(':');
			_chosenWebsites.Add(website.Join(":"));
			int SL = int.Parse(website[1]);
			foreach (object s in hinfo)
			{
				switch (hack)
				{
					case "DSA":
						result = (float)Math.Round(
							float.Parse(hinfo[0].ToString())
							* int.Parse(hinfo[1].ToString())
							* (SL / 5.0f)
							* float.Parse(hinfo[2].ToString()), 3)
							* float.Parse(hinfo[3].ToString());
						break;
					case "W":
						result = (float)Math.Round(
							float.Parse(hinfo[0].ToString())
							* int.Parse(hinfo[2].ToString())
							* (SL / 10.0f)
							* float.Parse(hinfo[1].ToString()), 3)
							* float.Parse(hinfo[3].ToString());
						break;
					case "CI":
						result = (float)Math.Round(
							float.Parse(hinfo[0].ToString())
							* float.Parse(hinfo[1].ToString())
							* (SL / 20.0f)
							* int.Parse(hinfo[2].ToString()), 3)
							* float.Parse(hinfo[3].ToString());
						break;
					case "XSS":
						result = (float)Math.Round(
							float.Parse(hinfo[0].ToString())
							* float.Parse(hinfo[1].ToString())
							* (SL / 8.0f)
							* (float.Parse(hinfo[2].ToString()) / 2.0f), 3)
							* float.Parse(hinfo[3].ToString());
						break;
					case "BFA":
						result = (float)Math.Round(
							(float.Parse(hinfo[0].ToString())
							* int.Parse(hinfo[1].ToString())
							* SL) / 5, 3)
							* float.Parse(hinfo[2].ToString());
						break;
					default:
						break;
				}

			}
			List<object> temp = hinfo.ToList();
			temp.Add(Math.Round(result, 3));
			temp.Add(Math.Round(result * DayPercentage(website), 3));
			temp.Add(Math.Round(result * DayPercentage(website) / _cryptoPrices[_chosenCryptocurrency], 3));
			_hackInformation.Add(temp.ToArray());
			_totalAmount += (float)Math.Round(result * DayPercentage(website) / _cryptoPrices[_chosenCryptocurrency], 3);
		}

		_totalAmount = (float)Math.Round(_totalAmount, 3);

		for (int i = 0; i <= 4; i++)
		{
			Debug.LogFormat("[Cheat Checkout #{0}]: Hack information of Hack #{1} ({2}) is: {3}", _modID, (i + 1), _chosenHackNames[i], ParseHackLogging(_hackInformation[i], _chosenHackNames[i], _chosenWebsites[i].Split(':')[0]));
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: Total amount for all hacks is: {1}.", _modID, _totalAmount);
	}

	void GenerateCustomerPrice()
	{
		float paid = 0f;
		if (_customerSlaps == 0)
		{
			paid = (float)Math.Round(rnd.Range((float)Math.Round(_totalAmount - (_totalAmount * 0.25), 3), (float)Math.Round(_totalAmount + (_totalAmount * 0.25), 3)), 3);
			_customerGave = paid < _totalAmount ? -1f : paid;
			_staticCustomerGave = paid;
			_displayTexts[0].text = paid.ToString();
			Debug.Log(_customerGave == -1f ? String.Format("[Cheat Checkout #{0}]: The customer doesn't have enough to pay for the hacks. Requesting a slap.", _modID) : String.Format("[Cheat Checkout #{0}]: The customer is paying {1} which is enough to give {2} back as change.", _modID, _customerGave, Math.Round(_customerGave - _totalAmount, 3)));
			return;
		}
		paid = (float)Math.Round(rnd.Range((float)Math.Round(_staticCustomerGave - (_staticCustomerGave * 0.05), 3), (float)Math.Round(_staticCustomerGave + (_staticCustomerGave * 0.5), 3)), 3);
		_customerGave = paid < _totalAmount ? -1f : paid;
		_staticCustomerGave = paid;
		_displayTexts[0].text = paid.ToString();
		Debug.Log(_customerGave == -1f ? String.Format("[Cheat Checkout #{0}]: After {1} slaps, the customer still doesn't have enough to pay for the hacks. Requesting another slap.", _modID, _customerSlaps) : String.Format("[Cheat Checkout #{0}]: After {1} slaps, the customer now has {2} which is enough to give {3} back as change.", _modID, _customerSlaps, _customerGave, Math.Round(_customerGave - _totalAmount, 3)));
	}

	string ParseHackLogging(object[] i, string hackName, string website)
	{
		switch (hackName)
		{
			case "DSA":
				return String.Format("Website: {0}, Base Value: {1}, Amount of PCs: {2}, Hack Duration: {3}, Success/Failed Price Multiplier: {4}, Total Amount before DOTW & Crypto Divide: {5}, Total Amount After DOTW {6}, Total After Crypto Divide and Rounding: {7}.", website, i[0], i[1], i[2], i[3], i[4], i[5], i[6]);
			case "W":
				return String.Format("Website: {0}, Base Value: {1}, Amount of Infected PCs: {2}, Multiplier: {3}, Success/Failed Price Multiplier: {4}, Total Amount before DOTW & Crypto Divide: {5}, Total Amount After DOTW {6}, Total After Crypto Divide and Rounding: {7}.", website, i[0], i[2], i[1], i[3], i[4], i[5], i[6]);
			case "CI":
				return String.Format("Website: {0}, Base Value: {1}, Complexity Multiplier: {2}, Batches: {3}, Success/Failed Price Multiplier: {4}, Total Amount before DOTW & Crypto Divide: {5}, Total Amount After DOTW {6}, Total After Crypto Divide and Rounding: {7}.", website, i[0], i[1], i[2], i[3], i[4], i[5], i[6]);
			case "XSS":
				return String.Format("Website: {0}, Base Value: {1}, Hack Multiplier: {2}, Programs: {3}, Success/Failed Price Multiplier: {4}, Total Amount before DOTW & Crypto Divide: {5}, Total Amount After DOTW {6}, Total After Crypto Divide and Rounding: {7}.", website, i[0], i[1], i[2], i[3], i[4], i[5], i[6]);
			case "BFA":
				return String.Format("Website: {0}, Base Value: {1}, Attempts: {2}, Success/Failed Price Multiplier: {3}, Total Amount before DOTW & Crypto Divide: {4}, Total Amount After DOTW {5}, Total After Crypto Divide and Rounding: {6}.", website, i[0], i[1], i[2], i[3], i[4], i[5]);
			default:
				return null;
		}
	}

	List<string> ParseDisplayHackInfo(int index)
	{
		if (!index.EqualsAny(0, 1, 2, 3, 4)) { return null; }
		object[] hackInfo = _hackInformation[index];
		List<string> displayInfo = new List<string>() { "Website: " + _chosenWebsites[index].Split(':')[0], "Hack Method: " + _chosenHackNames[index] };
		switch (_chosenHackNames[index])
		{
			case "DSA":
				displayInfo.Add("PC Type: " + new string[] { "Basic", "Advanced", "Supercomp.", "Quantum" }[Array.IndexOf(new float[] { 0.8f, 1.2f, 1.6f, 2f }, float.Parse(hackInfo[0].ToString()))]);
				displayInfo.Add("PCs Used: " + int.Parse(hackInfo[1].ToString()));
				displayInfo.Add("Duration: " + float.Parse(hackInfo[2].ToString()));
				displayInfo.Add(float.Parse(hackInfo[3].ToString()).EqualsAny(1f, 1.25f)
					? float.Parse(hackInfo[3].ToString()) == 1f ? "Success: Crashed Temp." : "Success: Crashed Perm."
					: "Failed: " + float.Parse(hackInfo[3].ToString())
					);
				return displayInfo;
			case "W":
				displayInfo.Add("PC Type: " + new string[] { "Defective", "Basic", "Advanced", "Supercomp.", "Quantum" }[Array.IndexOf(new float[] { 0.5f, 0.9f, 1.3f, 1.75f, 2.1f }, float.Parse(hackInfo[0].ToString()))]);
				displayInfo.Add("Worm Type: " + new string[] { "Normal", "Lethal", "Spreader" }[Array.IndexOf(new float[] { 1f, 2f, 0.5f }, float.Parse(hackInfo[1].ToString()))]);
				displayInfo.Add("Infected PCs: " + int.Parse(hackInfo[2].ToString()));
				displayInfo.Add(float.Parse(hackInfo[3].ToString()) == 1f
					? "Successful"
					: "Failed: " + float.Parse(hackInfo[3].ToString())
					);
				return displayInfo;
			case "CI":
				displayInfo.Add("Vulner. Type: " + new string[] { "SQL:", "LDAP", "XPath", "NoSQL" }[Array.IndexOf(new float[] { 0.9f, 1.8f, 1.25f, 2.2f }, float.Parse(hackInfo[0].ToString()))]);
				displayInfo.Add("Complex. Type: " + new string[] { "Simple", "Advanced", "Complex" }[Array.IndexOf(new float[] { 1f, 1.2f, 1.5f }, float.Parse(hackInfo[1].ToString()))]);
				displayInfo.Add("Batches: " + int.Parse(hackInfo[2].ToString()));
				displayInfo.Add(float.Parse(hackInfo[3].ToString()).EqualsAny(1.25f, 1.5f)
					? float.Parse(hackInfo[3].ToString()) == 1.25f ? "Success: Crashed Perm." : "Success: Host Inflitrated"
					: "Failed: " + float.Parse(hackInfo[3].ToString())
					);
				return displayInfo;
			case "XSS":
				displayInfo.Add("Complex. Type: " + new string[] { "Ex. Basic", "Basic", "Advanced", "Complex", "Unintell." }[Array.IndexOf(new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f }, float.Parse(hackInfo[0].ToString()))]);
				displayInfo.Add("Hack Type: " + new string[] { "Non-Persist.", "Persist.", "Mutated XSS" }[Array.IndexOf(new float[] { 1f, 1.25f, 1.5f }, float.Parse(hackInfo[1].ToString()))]);
				displayInfo.Add("Programs: " + int.Parse(hackInfo[2].ToString()));
				displayInfo.Add(float.Parse(hackInfo[3].ToString()) == 1f
					? "Successful"
					: "Failed: " + float.Parse(hackInfo[3].ToString())
					);
				return displayInfo;
			case "BFA":
				displayInfo.Add("Att. Type: " + new string[] { "Str. Inject", "Sneak", "Duplication" }[Array.IndexOf(new float[] { 2.2f, 1.6f, 1.9f }, float.Parse(hackInfo[0].ToString()))]);
				displayInfo.Add("Attempts: " + int.Parse(hackInfo[1].ToString()));
				displayInfo.Add(float.Parse(hackInfo[2].ToString()).EqualsAny(1.2f, 1.4f)
					? float.Parse(hackInfo[2].ToString()) == 1.2f ? "Success: Crashed Perm." : "Success: Host Inflitrated"
					: "Failed: " + float.Parse(hackInfo[2].ToString())
					);
				return displayInfo;
			default:
				return null;
		}
	}

	object[] HackInformation(string hack)
	{
		List<object> list = new List<object>();
		switch (hack)
		{
			case "DSA":
				float DSAPCPrice = new float[] { 0.8f, 1.2f, 1.6f, 2.0f }[rnd.Range(0, 4)];
				int DSAPCAmount = rnd.Range(5, 21);
				float DSADuration = (float)Math.Round(rnd.Range(1.0f, 3.0f), 3);
				float DSASuccess = rnd.Range(0, 2) == 0 ? 1f : 1.25f;
				float DSAFailedPercent = (float)Math.Round(rnd.Range(1.0f, 100.0f)) / 100;
				bool DSAFailed = rnd.Range(0, 2) == 0 ? false : true;
				if (DSAFailed)
				{
					list.Add(DSAPCPrice);
					list.Add(DSAPCAmount);
					list.Add(DSADuration);
					list.Add(DSAFailedPercent);
				}
				else
				{
					list.Add(DSAPCPrice);
					list.Add(DSAPCAmount);
					list.Add(DSADuration);
					list.Add(DSASuccess);
				}
				return list.ToArray();
			case "W":
				float WPCPrice = new float[] { 0.5f, 0.9f, 1.3f, 1.75f, 2.1f }[rnd.Range(0, 5)];
				float WType = new float[] { 1f, 2f, 0.5f }[rnd.Range(0, 3)];
				int WInfected = rnd.Range(5, 21);
				int WSuccess = 1;
				float WFailedPercent = (float)Math.Round(rnd.Range(1.0f, 100.0f)) / 100;
				bool WFailed = rnd.Range(0, 2) == 0 ? false : true;
				if (WFailed)
				{
					list.Add(WPCPrice);
					list.Add(WType);
					list.Add(WInfected);
					list.Add(WFailedPercent);

				}
				else
				{
					list.Add(WPCPrice);
					list.Add(WType);
					list.Add(WInfected);
					list.Add(WSuccess);
				}
				return list.ToArray();
			case "CI":
				float CIVulType = new float[] { 0.9f, 1.8f, 1.25f, 2.2f }[rnd.Range(0, 4)];
				float CIComType = new float[] { 1f, 1.2f, 1.5f }[rnd.Range(0, 3)];
				int CIBatches = rnd.Range(5, 21);
				float CISuccess = rnd.Range(0, 2) == 0 ? 1.25f : 1.5f;
				float CIFailedPercent = (float)Math.Round(rnd.Range(1.0f, 100.0f)) / 100;
				bool CIFailed = rnd.Range(0, 2) == 0 ? false : true;
				if (CIFailed)
				{
					list.Add(CIVulType);
					list.Add(CIComType);
					list.Add(CIBatches);
					list.Add(CIFailedPercent);
				}
				else
				{
					list.Add(CIVulType);
					list.Add(CIComType);
					list.Add(CIBatches);
					list.Add(CISuccess);
				}
				return list.ToArray();
			case "XSS":
				float XSSComType = new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f }[rnd.Range(0, 5)];
				float XSSHackType = new float[] { 1f, 1.25f, 1.5f }[rnd.Range(0, 3)];
				int XSSPrograms = rnd.Range(5, 21);
				int XSSSuccess = 1;
				float XSSFailedPercent = (float)Math.Round(rnd.Range(1.0f, 100.0f)) / 100;
				bool XSSFailed = rnd.Range(0, 2) == 0 ? false : true;
				if (XSSFailed)
				{
					list.Add(XSSComType);
					list.Add(XSSHackType);
					list.Add(XSSPrograms);
					list.Add(XSSFailedPercent);

				}
				else
				{
					list.Add(XSSComType);
					list.Add(XSSHackType);
					list.Add(XSSPrograms);
					list.Add(XSSSuccess);
				}
				return list.ToArray();
			case "BFA":
				float BFAAttType = new float[] { 2.2f, 1.6f, 1.9f }[rnd.Range(0, 3)];
				int BFAAttempts = rnd.Range(5, 21);
				float BFASuccess = rnd.Range(0, 1) == 0 ? 1.2f : 1.4f;
				float BFAFailedPercent = (float)Math.Round(rnd.Range(1.0f, 100.0f)) / 100;
				bool BFAFailed = rnd.Range(0, 2) == 0 ? false : true;
				if (BFAFailed)
				{
					list.Add(BFAAttType);
					list.Add(BFAAttempts);
					list.Add(BFAFailedPercent);
				}
				else
				{
					list.Add(BFAAttType);
					list.Add(BFAAttempts);
					list.Add(BFASuccess);
				}
				return list.ToArray();
			default:
				return null;
		}
	}

	float DayPercentage(string[] website)
	{
		switch (_dow.ToString())
		{
			case "Sunday":
				if (website[2] == "SE") return 0.8f;
				return 1.0f;
			case "Monday":
				return 1.1f;
			case "Tuesday":
				if (website[2] == "GA") return 0.8f;
				return 1.0f;
			case "Wednesday":
				if (website[2] == "IN") return 0.8f;
				return 1.0f;
			case "Thursday":
				if (website[2] == "SM") return 0.8f;
				return 1.0f;
			case "Friday":
				return 0.9f;
			case "Saturday":
				if (website[2] == "ST") return 0.8f;
				return 1.0f;
			default:
				return 1.0f;
		}
	}

	char[] GetCharactersVIAString(int amount, string s)
	{
		List<char> list = new List<char>();
		for (int i = 0; i < amount; i++)
		{
			int rand = rnd.Range(0, s.Length);
			while (list.Contains(s[rand]))
			{
				rand = rnd.Range(0, s.Length);
			}
			list.Add(s[rand]);
		}
		return list.ToArray();
	}

	void ResetButtonTexts()
	{
		foreach (TextMesh tm in _buttonTexts)
		{
			if (tm.name.ToLower().EqualsAny("left_tx", "right_tx")) { tm.color = _originalColor; continue; }
			tm.text = tm.name;
			tm.color = _originalColor;
		}
		_displayTexts[0].text = _staticCustomerGave.ToString();
		_displayTexts[0].color = _originalColor;
		if (_wifiStatus != 0)
		{
			_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
			_displayTexts[1].color = _originalColor;
		}
		return;
	}

	void StartDisplayBlock()
	{
		_blockDisplay = true;
		Debug.LogFormat("[Cheat Checkout #{0}]: Blocking display because of low wifi connection.", _modID);
		StopAllCoroutines();
		_colorblindTexts[0].text = "R";
		_colorblindTexts[0].color = new Color32(255, 36, 36, 255);
        _displayTexts[1].color = new Color32(255, 36, 36, 255);
		_displayTexts[1].text = "-----";
		StartCoroutine(ModuleTimer());
	}

	void StartHackSequence()
	{
		_beingHacked = true;
		Debug.LogFormat("[Cheat Checkout #{0}]: Hacking sequence started, text will now gl-{1} ERROR 404", _modID, GetCharactersVIAString(12, "abcdefghijklmnopqrstuvwxyz1234567890_+!@#$%^&*(),/.';][<?>:{}|").Join(""));
		StopAllCoroutines();
		_displayTexts[0].text = "PAY ATTENTION";
		_displayTexts[1].text = "TO ME";
		_cryptoSymbols[_chosenCryptocurrency].enabled = false;
		foreach (TextMesh tm in _displayTexts)
		{
			tm.color = new Color32(117, 4, 15, 255);
		}
		foreach (TextMesh tm in _buttonTexts)
		{
			tm.color = new Color32(117, 4, 15, 255);
		}
		_colorblindTexts[1].text = "R";
		_colorblindTexts[1].color = new Color32(255, 36, 36, 255);
        StartCoroutine(WarningSound());
		StartCoroutine(ThyHackTimer());
	}

	void ThyBecomeHacked()
	{
		if (_timerStopPlz)
		{
			ResetButtonTexts();
			StopAllCoroutines();
			StartCoroutine(ModuleTimer());
			_beingHacked = false;
			_cryptoSymbols[_chosenCryptocurrency].enabled = true;
			return;
		}
		StopAllCoroutines();
		ResetButtonTexts();
		_displayTexts[0].text = _customerGave.ToString();
		_displayTexts[1].text = "Hack #" + (_hackIndex + 1);
		_shieldSymbols[0].enabled = false;
		_shieldSymbols[1].enabled = false;
		_shieldSymbols[2].enabled = true;
		_wifiSymbols[0].enabled = false;
		_wifiSymbols[1].enabled = false;
		_wifiSymbols[2].enabled = true;
		_cryptoSymbols[_chosenCryptocurrency].enabled = false;
		ModuleResetVars();
		GetComponent<KMBombModule>().HandleStrike();
		Debug.LogFormat("[Cheat Checkout #{0}]: Module was successfully hacked and data has been deleted. Restarting module...", _modID);
		Start();
	}

	void ModuleResetVars()
	{
		_totalAmount = 0f;
		_givenChange = -1f;
		_customerGave = 0f;
		_staticCustomerGave = 0f;
		_customerSlaps = 0;
		_hackCycle = -1;
		_hackIndex = 0;
		_wifiStatus = 2;
		_shieldStatus = 2;
		_staticWifiStatus = -1;
		_staticShieldStatus = -1;
		_moduleTimer = 75;
		_hackInformation.Clear();
		_chosenHackNames.Clear();
		_chosenWebsites.Clear();
		_beingHacked = false;
		_blockDisplay = false;
		_timerStopPlz = false;
		_glitchedButtons = false;
		_LCDGlitch = false;
	}

	void ModuleAction()
	{
		// 0 for wifi, 1 for shield
		StopAllCoroutines();
		int arnd = rnd.Range(0, 2);
		switch (arnd)
		{
			case 0:
				if (_wifiStatus == 0) break;
				_wifiSymbols[_wifiStatus].enabled = false;
				_wifiStatus--;
				_wifiSymbols[_wifiStatus].enabled = true;
				_colorblindTexts[0].text = "RY"[_wifiStatus].ToString();
				_colorblindTexts[0].color = new Color32[] { new Color32(255, 36, 36, 255), new Color32(255, 254, 36, 255) }[_wifiStatus];

				if (_wifiStatus == 1)
				{
					_LCDGlitch = true;
					break;
				}
				else
				if (_wifiStatus == 0)
				{
					StartDisplayBlock();
					break;
				}
				break;
			case 1:
				if (_shieldStatus == 0) break;
				_shieldSymbols[_shieldStatus].enabled = false;
				_shieldStatus--;
				_shieldSymbols[_shieldStatus].enabled = true;
                _colorblindTexts[1].text = "RY"[_shieldStatus].ToString();
                _colorblindTexts[1].color = new Color32[] { new Color32(255, 36, 36, 255), new Color32(255, 254, 36, 255) }[_shieldStatus];
                if (_shieldStatus == 1)
				{
					StartCoroutine(GlitchSeparator());
					break;
				}
				else
				if (_shieldStatus == 0)
				{
					StartHackSequence();
					break;
				}
				break;
			default:
				break;
		}
		if (_shieldStatus == 1) { StartCoroutine(GlitchSeparator()); }
		if (_wifiStatus == 0) { StartDisplayBlock(); }
		StartCoroutine(ModuleTimer());
	}

	IEnumerator GlitchSeparator()
	{
		while (true)
		{
			_currentGlitched = new bool[15];
			List<TextMesh> tms = new List<TextMesh>();
			for (int x = 0; x <= 3; x++)
			{
				int rand = rnd.Range(0, _buttonTexts.Length - 3);
				while (tms.Contains(_buttonTexts[rand]))
				{
					rand = rnd.Range(0, _buttonTexts.Length - 3);
				}
				_currentGlitched[rand] = true;
				tms.Add(_buttonTexts[rand]);
			}
			foreach (TextMesh tm in tms)
			{
				tm.text = "-----";
			}
			_glitchedButtons = true;
			yield return new WaitForSeconds(5f);
			_glitchedButtons = false;
			_currentGlitched = new bool[15];
			ResetButtonTexts();
			yield return new WaitForSeconds(15f);
		}
	}

	IEnumerator ModuleTimer()
	{
		_moduleTimer = 75;
		while (true)
		{
			if (_modSolved) { break; }
			if (_moduleTimer == 0) { _moduleTimer = 75; }
			_moduleTimer--;
			while (_moduleTimer != 0) { _moduleTimer--; yield return new WaitForSeconds(1f); }
			ModuleAction();
		}
	}

	IEnumerator WarningSound()
	{
		while (true)
		{
			_audio.PlaySoundAtTransform(_HIPSound.name, this.transform);
			yield return new WaitForSeconds(_HIPSound.length + 0.25f);
		}
	}

	IEnumerator ThyHackTimer()
	{
		yield return new WaitForSeconds(30f);
		ThyBecomeHacked();
		yield break;
	}

	IEnumerator SolveAnimation()
	{
		_modSolved = true;
		_cryptoSymbols[_chosenCryptocurrency].enabled = false;
		_shieldSymbols[0].enabled = false;
		_shieldSymbols[1].enabled = false;
		_shieldSymbols[2].enabled = true;
		_wifiSymbols[0].enabled = false;
		_wifiSymbols[1].enabled = false;
		_wifiSymbols[2].enabled = true;
		string[] s = _solvedStringArray[rnd.Range(0, _solvedStringArray.Length)].Split(':');
		_displayTexts[0].text = "";
		_displayTexts[1].text = "";
		foreach (TextMesh tm in _buttonTexts)
		{
			tm.text = "";
			yield return new WaitForSeconds(0.10f);
		}
		_displayTexts[0].text = "";
		_displayTexts[1].text = "";
		yield return new WaitForSeconds(0.25f);
		_displayTexts[0].text = s[0].Replace("_", "") ?? "";
		yield return new WaitForSeconds(0.1f);
		_displayTexts[1].text = s[1].Replace("_", "") ?? "";
		int i = 0;
		if (s.Length >= 3)
		{
			foreach (char c in s[2] ?? "")
			{
				_buttonTexts[i].text = c.ToString().Replace("_", "") ?? "";
				_buttonTexts[i].characterSize = 0.25f;
				i++;
				yield return new WaitForSeconds(0.1f);
			}
			for (int x = 12; x <= 15; x++)
			{
				_buttonTexts[x].text = s[x - 9].Replace("_", "") ?? "";
				yield return new WaitForSeconds(0.1f);
			}
		}
		GetComponent<KMBombModule>().HandlePass();
		_statusLightGreen = true;
		Debug.LogFormat("[Cheat Checkout #{0}]: A correct amount of change was given. Module has been solved!", _modID);
		yield break;
	}

	// TP

	bool CorrectArgument(string s)
	{
		string[] valid = new string[] { "hack", "lcd", "screen", "display", "submit", "sub", "stabilize", "stbl", "patch" };
		if (valid.Contains(s))
		{
			return true;
		}
		return false;
	}

	List<KMSelectable> TPPriceButtonSetup(string arg)
	{
		string[] split = arg.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		if (split[1].Length != 3) 
		{
			split[1] += "0";
		}
		string price = String.Concat(split);
		int totalButtons = 0;
		List<KMSelectable> array = new List<KMSelectable>();
		if (price.Length >= 8)
		{
			return null;
		}
		int y = price.Length - 1;
		foreach (char c in price)
		{
			if (y == 6)
			{
				totalButtons += int.Parse(c.ToString()) * 10;
			}
			totalButtons += int.Parse(c.ToString());
			y--;
		}
		int x = 0;
		int index = price.Length - 1;
		int total = 0;
		bool checkedIndex = false;
		foreach (char c in price)
		{
			if (c == '.') { continue; }
			if (c == 0) { x++; index--; continue; }
			while (!checkedIndex && index != price.Length - 1) { index--; }
			if (!checkedIndex) { checkedIndex = true; }
			if (index == 6)
			{
				for (int i = 0; i < int.Parse(price[x].ToString()) * 10; i++)
				{
					array.Add(_priceButtons[index - 1]);
					total++;
				}
			}
			else
			{
				for (int i = 0; i < int.Parse(price[x].ToString()); i++)
				{
					array.Add(_priceButtons[index]);
					total++;
				}
			}
			x++;
			index--;
		}
		array.Add(_actionButtons[3]);
		return array;
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} hack <index> [Goes to <index> hack], !{0} lcd/screen/display <amount/cycle,c/fullcycle,fc> [Goes forward on a specific hack information, cycle the whole lcd or cycle all 5 hacks], !{0} submit/sub [Submits with nothing to slap the customer], !{0} submit/sub <change> [Submits the answer into the module], !{0} stabilize/stbl <#/##> [Presses 'Stabilize' depending on what is needed], !{0} patch <None/#> [Press 'Patch' depending on what is needed], !{0} cb/colorblind/colourblind [Toggles colourblind mode].";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		int stw = _wifiStatus;
		int sts = _shieldStatus;

		if(Regex.IsMatch(command, "cb|colo(u)?rblind"))
        {
			yield return null;
            _colorblindActive = !_colorblindActive;
			ShowColorblindText(_colorblindActive);
			yield break;
		}

		string[] args = command.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (args.Length >= 3)
		{
			yield return "sendtochaterror That is not the correct amount of arguments, please try again.";
			yield break;
		}
		if (!CorrectArgument(args[0]))
		{
			yield return "sendtochaterror That is an incorrect command, please try again.";
			yield break;
		}
		switch (args[0])
		{
			case "hack":
				if (args.Length != 2)
				{
					yield return "sendtochaterror Invalid amount of arguments for this subcommand.";
					yield break;
				}
				int result;
				if (_blockDisplay) yield break;
				if (int.TryParse(args[1], out result))
				{
					if (!result.EqualsAny(1, 2, 3, 4, 5))
					{
						yield return "sendtochaterror Invalid index.";
						yield break;
					}
					while (_glitchedButtons) yield return null;
					while (_hackIndex != result - 1 && _hackIndex > result - 1) { yield return null; _actionButtons[1].OnInteract(); }
					while (_hackIndex != result - 1 && _hackIndex < result - 1) { yield return null; _actionButtons[2].OnInteract(); }
					yield break;
				}
				else
				{
					yield return "sendtochaterror Invalid format for command.";
					yield break;
				}
			case "lcd":
			case "screen":
			case "display":
				if (args.Length != 2)
				{
					yield return "sendtochaterror Invalid amount of arguments for this subcommand.";
					yield break;
				}
				int resulta;
				if (_blockDisplay) yield break;
				if (args[1].EqualsAny("fullcycle", "fc"))
				{
					yield return null;
					while (_hackIndex != 0) { yield return null; _actionButtons[1].OnInteract(); }
					while (_hackCycle != -1) { yield return null; _actionButtons[0].OnInteract(); }
					int count = 0;
					while (count != 5)
					{
						yield return null;
						_actionButtons[0].OnInteract();
						yield return new WaitForSeconds(1.5f);
						while (_hackCycle != -1) { yield return null; _actionButtons[0].OnInteract(); yield return "trywaitcancel 1.5 Cyclng cancelled due to a request to."; }
						_actionButtons[2].OnInteract();
						yield return new WaitForSeconds(0.75f);
						count++;
					}
					while (_hackIndex != 0) { yield return null; _actionButtons[1].OnInteract(); }
					yield break;
				}
				if (args[1].EqualsAny("cycle", "c"))
				{
					yield return null;
					while (_hackCycle != -1) { yield return null; _actionButtons[0].OnInteract(); }
					_actionButtons[0].OnInteract();
					yield return new WaitForSeconds(1.5f);
					while (_hackCycle != -1) { yield return null; _actionButtons[0].OnInteract(); yield return "trywaitcancel 1.5 Cycling cancelled due to a request to."; }
					yield break;
				}
				else if (int.TryParse(args[1], out resulta))
				{
					if (resulta * -1 > 0)
					{
						yield return "sendtochaterror Invalid format for command.";
						yield break;
					}
					for (int i = 0; i < resulta; i++)
					{
						yield return null;
						_actionButtons[0].OnInteract();
						yield return new WaitForSeconds(0.75f);
					}
					yield break;
				}
				else
				{
					yield return "sendtochaterror Invalid format for command.";
					yield break;
				}
			case "submit":
			case "sub":
				if (args.Length >= 3)
				{
					yield return "sendtochaterror Invalid amount of arguments for this subcommand.";
					yield break;
				}
				float resultb;
				if (args.Length == 1)
				{
					yield return null;
					while (BeingGlitched(_actionButtons[4])) { yield return null; if (_shieldStatus == 0) yield break; }
					_actionButtons[4].OnInteract();
					yield return new WaitForSeconds(0.1f);
					while (BeingGlitched(_actionButtons[3])) { yield return null; if (_shieldStatus == 0) yield break; }
					_actionButtons[3].OnInteract();
					yield break;
				}
				else if (args.Length == 2 && args[1].Contains(".") && float.TryParse(args[1], out resultb))
				{
					if (resultb * -1 > 0)
					{
						yield return "sendtochaterror Invalid format for command.";
						yield break;
					}
					yield return null;
					while (BeingGlitched(_actionButtons[4])) { yield return null; if (_shieldStatus == 0) yield break; }
					_actionButtons[4].OnInteract();
					yield return new WaitForSeconds(0.1f);
					List<KMSelectable> butts = TPPriceButtonSetup(resultb.ToString());
					while (butts.Count != 0)
					{
						while (BeingGlitched(butts[0])) { yield return null; if (_shieldStatus == 0) yield break; }
						butts[0].OnInteract();
						butts.RemoveAt(0);
						yield return new WaitForSeconds(0.1f);
					}
					yield return "solve";
					yield break;
				}
				else
				{
					yield return "sendtochaterror Invalid format for command.";
					yield break;
				}
			case "stabilize":
			case "stbl":
				if (args.Length != 2)
				{
					yield return "sendtochaterror Invalid amount of arguments for this subcommand.";
					yield break;
				}
				int resultc;
				if (!int.TryParse(args[1], out resultc))
				{
					yield return "sendtochaterror Invalid format for command.";
					yield break;
				}
				if (resultc * -1 > 0)
				{
					yield return "sendtochaterror Invalid format for command.";
					yield break;
				}
				if (args[1].Length == 1)
				{
					while ((int)_bomb.GetTime() % 10 != resultc || (BeingGlitched(_actionButtons[5]))) { yield return null; if (_wifiStatus != stw) { yield return "sendtochat Press was cancelled because status has changed."; yield break; } yield return "trycancel Press cancelled due to request."; }
					yield return null;
					_actionButtons[5].OnInteract();
					yield break;
				}
				else if (args[1].Length == 2)
				{
					while ((int)_bomb.GetTime() % 60 != resultc || (BeingGlitched(_actionButtons[5]))) { yield return null; if (_wifiStatus != stw) { yield return "sendtochat Press was cancelled because status has changed."; yield break; } yield return "trycancel Press cancelled due to request."; }
					yield return null;
					_actionButtons[5].OnInteract();
					yield break;
				}
				break;
			case "patch":
				if (args.Length >= 3)
				{
					yield return "sendtochaterror Invalid amount of arguments for this subcommand.";
					yield break;
				}
				if (args.Length == 1)
				{
					yield return null;

					_actionButtons[6].OnInteract();
					yield break;
				}
				else if (args.Length == 2)
				{
					int resultd;
					if (!int.TryParse(args[1], out resultd))
					{
						yield return "sendtochaterror Invalid format for command.";
						yield break;
					}
					if (resultd * -1 > 0)
					{
						yield return "sendtochaterror Invalid format for command.";
						yield break;
					}
					if (!resultd.InRange(0, 9))
					{
						yield return "sendtochaterror Invalid format for command.";
						yield break;
					}
					while ((int)_bomb.GetTime() % 10 != resultd) { yield return null; if (_shieldStatus != sts) { yield return "sendtochat Press was cancelled because status has changed."; yield break; } yield return "trycancel Press cancelled due to request."; }
					yield return null;
					_actionButtons[6].OnInteract();
					yield break;
				}
				break;
			default:
				break;
		}
		yield break;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		_forcedSolve = true;
		yield return null;
		if (_givenChange != -1f) { _actionButtons[4].OnInteract(); yield return new WaitForSeconds(0.1f); }
		while (_customerGave == -1) { _actionButtons[3].OnInteract(); yield return new WaitForSeconds(0.1f); }
		foreach (KMSelectable km in TPPriceButtonSetup(((float)Math.Round(_customerGave - _totalAmount, 3)).ToString()))
		{
			km.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		while (!_statusLightGreen) yield return true;
		yield break;
	}

}

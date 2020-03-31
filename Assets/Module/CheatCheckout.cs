using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rand = UnityEngine.Random;

public class CheatCheckout : MonoBehaviour {

	public KMBombInfo bomb;
	public KMAudio bAudio;

	public KMSelectable[] priceButtons;
	public KMSelectable[] actionButtons;
	public KMSelectable[] directionalButtons;

	public TextMesh cryptoDisplay;
	public TextMesh hackLCDDisplay;
	public TextMesh[] priceTexts;
	public TextMesh[] actionTexts;

	public SpriteRenderer[] wifiSprites;
	public SpriteRenderer[] shieldSprites;
	public SpriteRenderer[] cryptoSprites;

	// Specifically for Logging
	static int modIDCount = 1;
	int modID;
	private bool modSolved = false;
	bool starting = true;
	bool countdownStarted = false;
	bool shieldGlitch = false;
	int hackModuleIndex = 0;
	int resets = 0;
	Color customerColor = new Color32(96, 188, 84,255);
	Color changeColor = new Color32(204,0,0,255);

	string day;
	string[] possibleWebsites = new string[] {
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
	string[] possibleHacks = new string[] { "DSA", "W", "CI", "XSS", "BFA" }; // Denial of Service Attack, Worm, Code Injection, Cross-Site Scripting, Brute Force Attack
	string[] possibleCryptos = new string[] { "Bitdrop", "Crane", "Evol", "Linecoin", "Penpoint", "Berr", "Lapel", "Blade", "Qubit" }; // Bitdrop, Crane, Evol, Linecoin, Penpoint, Berr, Lapel, Blade, Qubit
	float[] cryptoPrices = new float[] { 111f, 25f, 69f, 420f, 777f, 4.4f, 42f, 1234f, 0.5f };
	string[] generatedHacks = new string[5];
	string[] generatedWebsites = new string[5];
	string generatedCryptoName;
	float generatedCryptoPrice;
	string[] hackInformation = new string[5];
	float[] hackTotals = new float[5];
	bool isShowing = false;

	int wifiStatus = 2; // WIFI Connection: 1-3 bars ( 0 = red, 1 = yellow, 2 = green )
	int shieldStatus = 2; // Hacker Shield: 0-2 ( 0 = red, 1 = yellow, 2 = green )
	bool hackedState = false;

	float totalFromHacks; // Will be in cryptocurrency
	float customerPaid;
	float correctChange = 0;
	float givenChange = 0;
	int customerSlaps = 0;

	void Awake() {
		modID = modIDCount++;
		day = DateTime.Now.DayOfWeek.ToString();
		foreach (KMSelectable price in priceButtons) {
			price.OnInteract += delegate () {
				price.AddInteractionPunch();
				bAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, price.transform);
				if (!modSolved) {
					PriceButton(price);
				}
				return false;
			};
		}

		foreach (KMSelectable action in actionButtons) {
			action.OnInteract += delegate () {
				action.AddInteractionPunch();
				bAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, action.transform);
				if (!modSolved) {
					ActionButton(action);
				}
				return false;
			};
		}

		foreach (KMSelectable direction in directionalButtons) {
			direction.OnInteract += delegate () {
				direction.AddInteractionPunch();
				bAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, direction.transform);
				if (!modSolved) {
	
					DirectionalButton(direction);
				}
				return false;
			};
		}
	}

	void Start() {
		GenerateModule();
		hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
	}

	void PriceButton(KMSelectable button) {
		if (isShowing) { return; }
		if (shieldStatus == 0) { return; }
		if (hackedState) {
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Cheat Checkout #{0}]: Module struck because it was in hacked state.", modID);
			return;
		}
		float price = GetPrice(priceTexts[Array.IndexOf(priceButtons, button)].text);
		cryptoDisplay.color = changeColor;
		givenChange += price;
		givenChange = (float)Math.Round(givenChange, 3);
		cryptoDisplay.text = String.Concat(givenChange);
	}

	void ActionButton(KMSelectable button) {
		if (isShowing) { return; }
		if (hackedState) {
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Cheat Checkout #{0}]: Module struck because it was in hacked state.", modID);
			return;
		}
		string action = button.name.Equals("Hack LCD") ? hackLCDDisplay.text : actionTexts[(Array.IndexOf(actionButtons, button) - 1)].text;
		if (action.Equals("CLEAR")) {
			if (shieldStatus == 0) { return; }
			cryptoDisplay.text = String.Concat(customerPaid);
			cryptoDisplay.color = customerColor;
			givenChange = 0;
		} else if (action.Equals("SUBMIT")) {
			if (shieldStatus == 0) { return; }
			if (customerPaid < totalFromHacks) {
				GenerateCustomerPrice();
				return;
			}
			if (givenChange == correctChange) {
				modSolved = true;
				GetComponent<KMBombModule>().HandlePass();
				Debug.LogFormat("[Cheat Checkout #{0}]: The correct amount of change was inputted. Module Solved.",modID);
				return;
			}
			GetComponent<KMBombModule>().HandleStrike();
			givenChange = 0;
			cryptoDisplay.text = String.Concat(customerPaid);
			cryptoDisplay.color = customerColor;
			Debug.LogFormat("[Cheat Checkout #{0}]: The inputted amount of change was incorrect. Given {1} expected {2}.", modID, givenChange, correctChange);
		} else if (action.Equals("STABILIZE")) {
			if (shieldStatus == 0) { return; }
			int second = ((int)bomb.GetTime()) % 10;
			int seconds = ((int)bomb.GetTime()) % 60;
			int serial = int.Parse(String.Concat(bomb.GetSerialNumber().ToCharArray()[5]));
			int sum = SumNumbers(bomb.GetSerialNumberNumbers().ToList());
			if (wifiStatus != 2)
			{
				if (wifiStatus == 1 && seconds == sum)
				{
					wifiStatus = 2;
					wifiSprites[1].enabled = false;
					wifiSprites[2].enabled = true;
					Debug.LogFormat("[Cheat Checkout #{0}]: Wifi was stabilized.", modID);
					return;
				}
				else if (wifiStatus == 0 && second == serial)
				{
					wifiStatus = 2;
					wifiSprites[0].enabled = false;
					wifiSprites[2].enabled = true;
					Debug.LogFormat("[Cheat Checkout #{0}]: Wifi was stabilized.", modID);
					return;
				}
				else
				{
					GetComponent<KMBombModule>().HandleStrike();
					wifiSprites[0].enabled = false;
					wifiSprites[1].enabled = false;
					wifiSprites[2].enabled = true;
					wifiStatus = 2;
					Debug.LogFormat("[Cheat Checkout #{0}]: Strike was issued due to incorrect time press on Stabilize. {1}", modID, wifiStatus == 1 ? String.Format("Given {0} but expected {1}.", second, serial) : String.Format("Given {0} but expected {1}.", seconds, sum));
				}
			}
		} else if (action.Equals("PATCH")) {
			int second = ((int)bomb.GetTime()) % 10;
			int serial = int.Parse(String.Concat(bomb.GetSerialNumber().ToCharArray()[5]));
			if (shieldStatus != 2) {
				if (shieldStatus == 1)
				{
					shieldStatus = 2;
					shieldSprites[1].enabled = false;
					shieldSprites[2].enabled = true;
					shieldGlitch = false;
					StopCoroutine(ShieldGlitchButtons());
					Debug.LogFormat("[Cheat Checkout #{0}]: Hacker Shield was patched.", modID);
					return;
				}
				else if (shieldStatus == 0 & second == serial)
				{
					shieldStatus = 2;
					shieldSprites[0].enabled = false;
					shieldSprites[2].enabled = true;
					countdownStarted = false;
					shieldGlitch = false;
					StopCoroutine(ShieldGlitchButtons());
					StopCoroutine(Countdown());
					SetOriginalTexts();
					Debug.LogFormat("[Cheat Checkout #{0}]: Hacker Shield was patched.", modID);
					return;
				} else {
					GetComponent<KMBombModule>().HandleStrike();
					shieldSprites[0].enabled = false;
					shieldSprites[1].enabled = false;
					shieldSprites[2].enabled = true;
					StopCoroutine(ShieldGlitchButtons());
					shieldGlitch = false;
					shieldStatus = 2;
					SetOriginalTexts();
					StopCoroutine(Countdown());
					givenChange = 0;
					cryptoDisplay.text = String.Concat(customerPaid);
					cryptoDisplay.color = customerColor;
					Debug.LogFormat("[Cheat Checkout #{0}]: Strike was issued due to incorrect time press on Patch. Given {1} but expected {2}.", modID, second, serial);
				}
			}
		} else if (button.name.Equals("Hack LCD") && hackLCDDisplay.text.Equals("Hack #" + (hackModuleIndex + 1)) && wifiStatus != 0) {
			if (wifiStatus <= 1) {
				StartCoroutine(GlitchedHackInformation());
				return;
			}
			StartCoroutine(ShowHackInformation());
		}
	}

	void DirectionalButton(KMSelectable button) {
		if (isShowing) { return; }
		if (shieldStatus == 0) { return; }
		if (hackedState) {
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Cheat Checkout #{0}]: Module struck because it was in hacked state.", modID);
			return;
		}
		if (button.name.Equals("Left")) {
			if (hackModuleIndex == 0) {
				return;
			}
			hackModuleIndex--;
			hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
		} else if (button.name.Equals("Right")) {
			if (hackModuleIndex == 4)
			{
				return;
			}
			hackModuleIndex++;
			hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
		}
	}

	void GenerateModule() {
		Debug.LogFormat("[Cheat Checkout #{0}]: After {1} resets, the module information is:",modID,resets);
		GenerateRandomWebsites();
		GenerateRandomHacks();
		generatedCryptoName = possibleCryptos[rand.Range(0, 9)];
		generatedCryptoPrice = cryptoPrices[Array.IndexOf(possibleCryptos, generatedCryptoName)];
		cryptoSprites[Array.IndexOf(possibleCryptos, generatedCryptoName)].enabled = true;
		Debug.LogFormat("[Cheat Checkout #{0}]: The generated cryptocurrency is: {1} with price of {2}.", modID, generatedCryptoName, generatedCryptoPrice);
		GeneratedDiscounts();
		totalFromHacks = (float)Math.Truncate(((hackTotals[0] + hackTotals[1] + hackTotals[2] + hackTotals[3] + hackTotals[4]) / generatedCryptoPrice) * 1000f) / 1000f;
		Debug.LogFormat("[Cheat Checkout #{0}]: The total amount that is being charged, after disconts, is: {1}", modID, totalFromHacks);
		GenerateCustomerPrice();
		StartCoroutine(UpdateFeatures());
	}

	void GenerateRandomHacks() {
		for (int i = 0; i <= 4; i++) {
			generatedHacks[i] = possibleHacks[rand.Range(0, 5)];
			switch (generatedHacks[i]) {
				case "DSA":
					GenerateDSAHack(i);
					break;
				case "W":
					GenerateWHack(i);
					break;
				case "CI":
					GenerateCIHack(i);
					break;
				case "XSS":
					GenerateXSSHack(i);
					break;
				case "BFA":
					GenerateBFAHack(i);
					break;
				default:
					Debug.LogFormat("[Cheat Checkout #{0}]: The hacks are unable to be created.", modID);
					break;
			}
		}
	}

	void GenerateRandomWebsites() {
		string[] temp = new string[5];
		for (int i = 0; i <= 4; i++) {
			string s = possibleWebsites[rand.Range(0, 32)];
			temp[i] = s.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
			generatedWebsites[i] = s;
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: The chosen websites are: '{1}', '{2}', '{3}', '{4}', '{5}' ", modID, temp[0], temp[1], temp[2], temp[3], temp[4]);
	}

	void GenerateDSAHack(int index) {
		// Generated PC Type 
		string[] pcTypes = new string[] { "Basic", "Advance", "Super", "Quantum" };
		float[] pcPrices = new float[] { 0.8f, 1.2f, 1.6f, 2.0f };
		string pcType = pcTypes[rand.Range(0, 4)];
		float pcPrice = pcPrices[Array.IndexOf(pcTypes, pcType)];
		// Amount of PCs
		int amount = rand.Range(5, 21);
		// Duration (in hours)
		float duration = (rand.Range(6, 19) / 10f);
		// Result of the hack
		bool success = rand.Range(0, 2) == 1 ? true : false; // True: Success, False: Failed #% Hacked
		bool result = rand.Range(0, 2) == 1 ? true : false; // True: Permanently, False: Temporarily
		string sResult;
		float percent = (float)Math.Round(rand.Range(0.0f,100.0f),1)/100f;
		if (success) {
			sResult = result ? "Crashed Permanently" : "Crashed Temporarily";
		} else {
			sResult = "Failed! {0}% Hacked";
		}
		// Math Equation: Base Value * Amount of PC * (Security Level / 5)  * Duration
		string[] website = generatedWebsites[index].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		float subTotal = (pcPrice) * (amount) * (float.Parse(website[1]) / 5f) * (duration);
		hackInformation[index] = String.Concat(website[0] + ":DSA:" + pcType + ":" + amount + ":" + duration + ":" + sResult + ":" + (percent * 100f));
		if (success) {
			if (result) {
				hackTotals[index] = (float)Math.Round((subTotal * 1.25f), 3);
			} else {
				hackTotals[index] = (float)Math.Round(subTotal, 3);
			}
		} else {
			hackTotals[index] = (float)Math.Round((subTotal * percent), 3);
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: The DSA hack (Hack #{1}) has information of -> Website: {2}, PC-Type: {3}, Amount of PCs: {4}, Duration: {5} hours, Result: {6} and Total Cost: {7}", modID, index + 1, website[0], pcType, amount, duration, success ? sResult : "Failed: " + (percent * 100) + "% Hacked", hackTotals[index]);
	}

	void GenerateWHack(int index) {
		// Generated PC Type
		string[] pcTypes = new string[] { "Defective", "Basic", "Advance", "Super", "Quantum" };
		float[] pcPrices = new float[] { 0.5f, 0.9f, 1.3f, 1.75f, 2.1f };
		string pcType = pcTypes[rand.Range(0, 5)];
		float pcPrice = pcPrices[Array.IndexOf(pcTypes, pcType)];
		// Type of Worm
		string[] wormTypes = new string[] { "Normal", "Lethal", "Spreader" };
		float[] wormMultipliers = new float[] { 1f, 2f, 0.5f };
		string wormType = wormTypes[rand.Range(0, 3)];
		float wormMulti = wormMultipliers[Array.IndexOf(wormTypes, wormType)];
		// Amount Infected PCs
		int amount = rand.Range(5, 21);
		// Result of Hack NEEDS CHANGE
		bool result = rand.Range(0, 2) == 1 ? true : false; // True: Success, False: Failed #% Hacked
		float percent = (float)Math.Round(rand.Range(0.0f, 100.0f), 1) / 100f;
		string sResult = result ? "Success!" : "Failed! {0}% Hacked";
		// Math Equation: Base Value * Computers Infected * (Security Level/10) * Worm Multiplier
		string[] website = generatedWebsites[index].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		float subTotal = (pcPrice) * (amount) * (float.Parse(website[1]) / 10f) * (wormMulti);
		hackInformation[index] = String.Concat(website[0] + ":W:" + pcType + ":" + wormType + ":" + amount + ":" + sResult + ":" + (percent * 100f));
		if (result) {
			hackTotals[index] = (float)Math.Round(subTotal, 3);
		} else {
			hackTotals[index] = (float)Math.Round((subTotal * percent), 3);
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: The Worm hack (Hack #{1}) has information of -> Website: {2}, PC-Type: {3}, Type of Worm: {4}, Computers Infected: {5}, Result: {6} and Total Cost: {7}", modID, index + 1, website[0], pcType, wormType, amount, result ? sResult : "Failed: " + (percent * 100) + "% Hacked", hackTotals[index]);
	}

	void GenerateCIHack(int index) {
		// Generated Vulnerabilitiy
		string[] vulnerabilityTypes = new string[] { "SQL", "LDAP", "XPath", "NoSQL" };
		float[] vulnerabilityPrices = new float[] { 0.9f, 1.8f, 1.25f, 2.2f };
		string vulnerabilityType = vulnerabilityTypes[rand.Range(0, 4)];
		float vulnerabilityPrice = vulnerabilityPrices[Array.IndexOf(vulnerabilityTypes, vulnerabilityType)];
		// Generated Complexity
		string[] complexityTypes = new string[] { "Simple", "Advance", "Complex" };
		float[] complexityPrices = new float[] { 1f, 1.2f, 1.5f };
		string complexityType = complexityTypes[rand.Range(0, 3)];
		float complexityPrice = complexityPrices[Array.IndexOf(complexityTypes, complexityType)];
		// Generated Batch Amounts
		int amount = rand.Range(5, 21);
		// Generated Result NEEDS CHANGE
		bool success = rand.Range(0, 2) == 1 ? true : false; // True: Success, False: Failed #% Hacked
		bool result = rand.Range(0, 2) == 1 ? true : false; // True: Permanently, False: Infiltrated
		string sResult;
		float percent = (float)Math.Round(rand.Range(0.0f, 100.0f), 1) / 100f;
		if (success) {
			sResult = result ? "Crashed Permanently" : "Infiltrated";
		} else {
			sResult = "Failed! {0}% Hacked";
		}
		// Math Equation: Base Currency * Code Complexity * Batch Amount * (Security Value/20)
		string[] website = generatedWebsites[index].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		float subTotal = (vulnerabilityPrice) * (complexityPrice) * (amount) * (float.Parse(website[1]) / 20f);
		hackInformation[index] = String.Concat(website[0] + ":CI:" + vulnerabilityType + ":" + complexityType + ":" + amount + ":" + sResult + ":" + (percent * 100f));
		if (success) {
			if (result) {
				hackTotals[index] = (float)Math.Round((subTotal * 1.25f), 3);
			} else {
				hackTotals[index] = (float)Math.Round((subTotal * 1.5f));
			}
		} else {
			hackTotals[index] = (float)Math.Round((subTotal * percent), 3);
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: The CI hack (Hack #{1}) has information of -> Website: {2}, Vulnerability Type: {3}, Complexity Type: {4}, Batch Amount: {5}, Result: {6} and Total Cost: {7}", modID, index + 1, website[0], vulnerabilityType, complexityType, amount, success ? sResult : "Failed: " + (percent * 100) + "% Hacked", hackTotals[index]);
	}

	void GenerateXSSHack(int index) {
		// Generated Script Complexity
		string[] complexityTypes = new string[] { "Ex-Basic", "Basic", "Advance", "Complex", "Unintelligible" };
		float[] complexityPrices = new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f };
		string complexityType = complexityTypes[rand.Range(0, 5)];
		float complexityPrice = complexityPrices[Array.IndexOf(complexityTypes, complexityType)];
		// Generated Hack Type
		string[] hackTypes = new string[] { "Non-Persistent", "Persistent", "Mutated XSS" };
		float[] hackPrices = new float[] { 1f, 1.25f, 1.5f };
		string hackType = hackTypes[rand.Range(0, 3)];
		float hackPrice = hackPrices[Array.IndexOf(hackTypes, hackType)];
		// Generated Programs Sent
		float amount = rand.Range(4, 33);
		// Generated Result
		bool result = rand.Range(0, 2) == 1 ? true : false; // True: Success, False: Failed
		float percent = (float)Math.Round(rand.Range(0.0f, 100.0f), 1) / 100f;
		string sResult = result ? "Success!" : "Failed! {0}% Hacked";
		// Math Equation: Base Value * Multiplier * (Security Value/8) * (Program/2);
		string[] website = generatedWebsites[index].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		float subTotal = complexityPrice * hackPrice * (float.Parse(website[1]) / 8f) * (amount / 2f);
		hackInformation[index] = String.Concat(website[0] + ":XSS:" + complexityType + ":" + hackType + ":" + amount + ":" + sResult + ":" + (percent * 100f));
		if (result) {
			hackTotals[index] = (float)Math.Round(subTotal, 3);
		} else {
			hackTotals[index] = (float)Math.Round((subTotal * percent), 3);
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: The XSS hack (Hack #{1}) has information of -> Website: {2}, Complexity Type: {3}, Hack Type: {4}, Programs: {5}, Result: {6} and Total Cost: {7}", modID, index + 1, website[0], complexityType, hackType, amount, result ? sResult : "Failed: " + (percent * 100) + "% Hacked", hackTotals[index]);
	}

	void GenerateBFAHack(int index) {
		// Generated Attack
		string[] attackTypes = new string[] { "Strong Inject", "Sneak", "Duplication" };
		float[] attackPrices = new float[] { 2.2f, 1.6f, 1.9f };
		string attackType = attackTypes[rand.Range(0, 3)];
		float attackPrice = attackPrices[Array.IndexOf(attackTypes, attackType)];
		// Generated Attempts
		float amount = rand.Range(2, 11);
		// Generated Result
		bool success = rand.Range(0, 2) == 1 ? true : false; // True: Success, False: Failed #% Hacked
		bool result = rand.Range(0, 2) == 1 ? true : false; // True: Permanently, False: Infiltrated
		string sResult;
		float percent = (float)Math.Round(rand.Range(0.0f, 100.0f), 1) / 100f;
		if (success) {
			sResult = result ? "Crashed Permanently" : "Infiltrated";
		} else {
			sResult = "Failed! {0}% Hacked";
		}
		// Math Equation: (Base Value * Attempt * Security Level)/5
		string[] website = generatedWebsites[index].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		float subTotal = (attackPrice * amount * float.Parse(website[1])) / 5f;

		hackInformation[index] = String.Concat(website[0] + ":BFA:" + attackType + ":" + amount + ":" + sResult + ":" + (percent * 100f));
		if (success) {
			if (result) {
				hackTotals[index] = (float)Math.Round((subTotal * 1.2f), 3);
			} else {
				hackTotals[index] = (float)Math.Round((subTotal * 1.4f), 3);
			}
		} else {
			hackTotals[index] = (float)Math.Round((subTotal * percent), 3);
		}
		Debug.LogFormat("[Cheat Checkout #{0}]: The BFA hack (Hack #{1}) has information of -> Website: {2}, Attack Type: {3}, Attempts: {4}, Result: {5} and Total Cost: {6}", modID, index + 1, website[0], attackType, amount, success ? sResult : "Failed: " + (percent * 100) + "% Hacked", hackTotals[index]);
	}

	void GenerateCustomerPrice() {
		if (customerSlaps == 0) {
			customerPaid = (float)Math.Round(rand.Range(totalFromHacks * 0.5f, totalFromHacks * 1.5f), 3);
			while (customerPaid < 0) {
				customerPaid = (float)Math.Round(rand.Range(totalFromHacks * 0.5f, totalFromHacks * 1.5f), 3);
			}
			Debug.LogFormat("[Cheat Checkout #{0}]: The customer has tried to pay {1} for the hacks. {2}", modID, customerPaid, totalFromHacks > customerPaid ? "Customer didn't pay enough, slap him." : "Customer paid enough.");
			correctChange = (customerPaid - totalFromHacks) < 0 ? 0 : (float)Math.Round((customerPaid - totalFromHacks), 3);
			Debug.LogFormat("[Cheat Checkout #{0}]: The correct amount of change is: {1}", modID, correctChange == 0 ? "None" : String.Concat(correctChange));
			cryptoDisplay.text = String.Concat(customerPaid);
			customerSlaps++;
			return;
		}
		customerPaid += (float)Math.Round(rand.Range(totalFromHacks * 0.5f, totalFromHacks * 1.5f), 3);
		Debug.LogFormat("[Cheat Checkout #{0}]: The customer has been slapped {1} and now is paying {2} for their hacks.", modID, customerSlaps >= 2 ? String.Format("{0} times", customerSlaps) : "1 time", customerPaid);
		correctChange = (customerPaid - totalFromHacks) < 0 ? 0 : (float)Math.Round((customerPaid - totalFromHacks), 3);
		Debug.LogFormat("[Cheat Checkout #{0}]: The correct amount of change is: {1}", modID, correctChange == 0 ? "None" : String.Concat(correctChange));
		cryptoDisplay.text = String.Concat(customerPaid);
		customerSlaps++;
	}

	void GeneratedDiscounts() {
		switch (day) {
			case "Sunday":
				for (int i = 0; i <= 4; i++) {
					string[] website = generatedWebsites[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (website[2].Equals("SE")) {
						hackTotals[i] = (float)Math.Round(hackTotals[i] * 0.8f, 3);
						Debug.LogFormat("[Cheat Checkout #{0}]: Discount on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
					}
				}
				break;
			case "Monday":
				for (int i = 0; i <= 4; i++) {
					hackTotals[i] = (float)Math.Round(hackTotals[i] * 1.10f, 3);
					Debug.LogFormat("[Cheat Checkout #{0}]: Increase on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
				}
				break;
			case "Tuesday":
				for (int i = 0; i <= 4; i++) {
					string[] website = generatedWebsites[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (website[2].Equals("GA"))
					{
						hackTotals[i] = (float)Math.Round(hackTotals[i] * 0.8f, 3);
						Debug.LogFormat("[Cheat Checkout #{0}]: Discount on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
					}
				}
				break;
			case "Wednesday":
				for (int i = 0; i <= 4; i++) {
					string[] website = generatedWebsites[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (website[2].Equals("IN"))
					{
						hackTotals[i] = (float)Math.Round(hackTotals[i] * 0.8f, 3);
						Debug.LogFormat("[Cheat Checkout #{0}]: Discount on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
					}
				}
				break;
			case "Thursday":
				for (int i = 0; i <= 4; i++) {
					string[] website = generatedWebsites[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (website[2].Equals("SM"))
					{
						hackTotals[i] = (float)Math.Round(hackTotals[i] * 0.8f, 3);
						Debug.LogFormat("[Cheat Checkout #{0}]: Discount on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
					}
				}
				break;
			case "Friday":
				for (int i = 0; i <= 4; i++) {
					hackTotals[i] = (float)Math.Round(hackTotals[i] * 0.9f, 3);
					Debug.LogFormat("[Cheat Checkout #{0}]: Discount on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
				}
				break;
			case "Saturday":
				for (int i = 0; i <= 4; i++) {
					string[] website = generatedWebsites[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (website[2].Equals("ST"))
					{
						hackTotals[i] = (float)Math.Round(hackTotals[i] * 0.8f, 3);
						Debug.LogFormat("[Cheat Checkout #{0}]: Discount on hack {1}! New price: {2}", modID, i + 1, hackTotals[i]);
					}
				}
				break;
			default:
				Debug.LogFormat("[Cheat Checkout #{0}]: Unable to generate discounts.", modID);
				break;
		}
	}

	float GetPrice(string s) {
		switch (s) {
			case "0.001":
				return 0.001f;
			case "0.01":
				return 0.01f;
			case "0.1":
				return 0.1f;
			case "1":
				return 1f;
			case "10":
				return 10f;
			case "100":
				return 100f;
			case "0.005":
				return 0.005f;
			case "0.05":
				return 0.05f;
			case "0.5":
				return 0.5f;
			case "5":
				return 5f;
			case "50":
				return 50f;
			case "500":
				return 500f;
			default:
				break;
		}
		return 0;
	}

	IEnumerator UpdateFeatures() {
		if (starting) {
			starting = false;
			yield return new WaitForSeconds(25f);
		}
		while (!modSolved && !countdownStarted) {
			int chance = rand.Range(1, 11);
			int random = rand.Range(0, 10);
			bool setting = random.EqualsAny(0, 2, 4, 6, 8) ? true : false; // True: Wifi, False: Shield
			if (chance == 1) {
				if (setting) {
					if (wifiStatus != 0) {
						wifiSprites[wifiStatus].enabled = false;
						wifiSprites[wifiStatus - 1].enabled = true;
						wifiStatus--;
					}
					if (wifiStatus == 0) {
						wifiSprites[1].enabled = false;
						wifiSprites[0].enabled = true;
					}
				} else {
					if (shieldStatus != 0) {
						shieldSprites[shieldStatus].enabled = false;
						shieldSprites[shieldStatus - 1].enabled = true;
						shieldStatus--;
					} 
					if (shieldStatus == 1 && !shieldGlitch) {
						shieldGlitch = true;
						StartCoroutine(ShieldGlitchButtons());
					} 
					if (shieldStatus == 0 && !countdownStarted) {
						countdownStarted = true;
						shieldSprites[1].enabled = false;
						shieldSprites[0].enabled = true;
						StartCoroutine(Countdown());
						yield break;
					}
				}
			}
			yield return new WaitForSeconds(10f);
		}
	}

	IEnumerator ShieldGlitchButtons() {
		int[] xIndicies = new int[10];
		int[] yIndicies = new int[10];
		yield return new WaitForSeconds(2f);
		while (!modSolved && !countdownStarted) {
			int chance = rand.Range(1, 26);
			if (chance == 1 && !hackedState && !countdownStarted) {
				hackedState = true;
				for (int i = 0; i <= 9; i++)
				{
					int x = rand.Range(0, 15);
					int y = rand.Range(0, 15);
					while (x == y)
					{
						y = rand.Range(0, 15);
					}
					TextMesh xTM = priceTexts[0];
					TextMesh yTM = priceTexts[0];
					switch (x)
					{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
							xTM = priceTexts[x];
							break;
						case 12:
						case 13:
						case 14:
							xTM = actionTexts[x - 12];
							break;
					}
					switch (y)
					{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
							yTM = priceTexts[y];
							break;
						case 12:
						case 13:
						case 14:
							yTM = actionTexts[y - 12];
							break;
					}
					xTM.text = GlitchInfo("!!!!", 5);
					yTM.text = GlitchInfo("!!!!", 5);
					xIndicies[i] = x;
					yIndicies[i] = y;
				}
				yield return new WaitForSeconds(5f);
				SetOriginalTexts();
				hackedState = false;
			}
			yield return new WaitForSeconds(rand.Range(5f, 15f));
			if (!shieldGlitch) {
				yield break;
			}
		}
		yield break;
	}

	IEnumerator ShowHackInformation() {
		string[] information = hackInformation[hackModuleIndex].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		switch (information[1]) {
			// 64
			case "DSA":
				//hackInformation[index] = String.Concat(website[0]+":DSA:"+pcType+":"+amount+":"+duration+":"+aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Initiated on: " + information[0];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Method: " + information[1];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "PC-Type: " + information[2];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "PCs Used: " + information[3];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Duration: " + information[4] + " Hours";
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = information[5].EqualsAny("Crashed Temporarily", "Crashed Permanently") ? 32 : 36;
				hackLCDDisplay.text = "Result: " + (information[5].EqualsAny("Crashed Temporarily", "Crashed Permanently") ? information[5] : String.Format(information[5], information[6]));
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "W":
				// hackInformation[index] = String.Concat(website[0] + ":W:" + pcType + ":" + wormType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Initiated on: " + information[0];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Method: " + information[1];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "PC-Type: " + information[2];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Worm: " + information[3];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Infected PCs: " + information[4];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 36;
				hackLCDDisplay.text = "Result: " + (information[5].Equals("Success!") ? "Success!" : String.Format(information[5], information[6]));
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "CI":
				// hackInformation[index] = String.Concat(website[0] + ":CI:" + vulnerabilityType + ":" + complexityType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Initiated on: " + information[0];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Method: " + information[1];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Vulnerability: " + information[2];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Complexity: " + information[3];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Batches: " + information[4];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = information[5].EqualsAny("Crashed Permanently", "Infiltrated") ? 32 : 36;
				hackLCDDisplay.text = "Result: " + (information[5].EqualsAny("Crashed Permanently", "Infiltrated") ? information[5] : String.Format(information[5], information[6]));
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "XSS":
				// hackInformation[index] = String.Concat(website[0] + ":CSS:" + complexityType + ":" + hackType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Initiated on: " + information[0];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Method: " + information[1];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Complexity: " + information[2];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Hack Type: " + information[3];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Programs: " + information[4];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 36;
				hackLCDDisplay.text = "Result: " + (information[5].Equals("Success!") ? "Success!" : String.Format(information[5], information[6]));
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "BFA":
				// hackInformation[index] = String.Concat(website[0] + ":BFA:" + attackType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);
				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = "Initiated on: " + information[0];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Method: " + information[1];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 36;
				hackLCDDisplay.text = "Attack Type: " + information[2];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = "Attempts: " + information[3];
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = information[4].EqualsAny("Crashed Permanently", "Infiltrated") ? 32 : 36;
				hackLCDDisplay.text = "Result: " + (information[4].EqualsAny("Crashed Permanently", "Infiltrated") ? information[4] : String.Format(information[4], information[5]));
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			default:
				Debug.LogFormat("[Cheat Checkout #{0}]: Unable to generate text.", modID);
				break;
		}
		yield break;
	}

	IEnumerator GlitchedHackInformation() {
		string[] information = hackInformation[hackModuleIndex].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
		switch (information[1])
		{
			// 64
			case "DSA":
				//hackInformation[index] = String.Concat(website[0]+":DSA:"+pcType+":"+amount+":"+duration+":"+aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Initiated on: " + information[0], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Method: " + information[1], 6);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("PC-Type: " + information[2], 10);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("PCs Used: " + information[3], 10);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Duration: " + information[4] + " Hours", 14);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = information[5].EqualsAny("Crashed Temporarily", "Crashed Permanently") ? 32 : 36;
				hackLCDDisplay.text = GlitchInfo("Result: " + (information[5].EqualsAny("Crashed Temporarily", "Crashed Permanently") ? information[5] : String.Format(information[5], information[6])), 14);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "W":
				// hackInformation[index] = String.Concat(website[0] + ":W:" + pcType + ":" + wormType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Initiated on: " + information[0], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Method: " + information[1], 6);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("PC-Type: " + information[2], 10);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Worm: " + information[3], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Infected PCs: " + information[4], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 36;
				hackLCDDisplay.text = GlitchInfo("Result: " + (information[5].Equals("Success!") ? "Success!" : String.Format(information[5], information[6])), 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "CI":
				// hackInformation[index] = String.Concat(website[0] + ":CI:" + vulnerabilityType + ":" + complexityType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Initiated on: " + information[0], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Method: " + information[1], 6);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Vulnerability: " + information[2], 13);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Complexity: " + information[3], 13);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Batches: " + information[4], 7);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = information[5].EqualsAny("Crashed Permanently", "Infiltrated") ? 32 : 36;
				hackLCDDisplay.text = GlitchInfo("Result: " + (information[5].EqualsAny("Crashed Permanently", "Infiltrated") ? information[5] : String.Format(information[5], information[6])), 14);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "XSS":
				// hackInformation[index] = String.Concat(website[0] + ":CSS:" + complexityType + ":" + hackType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Initiated on: " + information[0], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Method: " + information[1], 6);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Complexity: " + information[2], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Hack Type: " + information[3], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Programs: " + information[4], 7);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 36;
				hackLCDDisplay.text = GlitchInfo("Result: " + (information[5].Equals("Success!") ? "Success!" : String.Format(information[5], information[6])), 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			case "BFA":
				// hackInformation[index] = String.Concat(website[0] + ":BFA:" + attackType + ":" + amount + ":" + aResult);
				isShowing = true;
				yield return new WaitForSeconds(0.5f);
				hackLCDDisplay.fontSize = 32;
				hackLCDDisplay.text = GlitchInfo("Initiated on: " + information[0], 8);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Method: " + information[1], 6);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 36;
				hackLCDDisplay.text = GlitchInfo("Attack Type: " + information[2], 9);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 48;
				hackLCDDisplay.text = GlitchInfo("Attempts: " + information[3], 6);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = information[4].EqualsAny("Crashed Permanently", "Infiltrated") ? 32 : 36;
				hackLCDDisplay.text = GlitchInfo("Result: " + (information[4].EqualsAny("Crashed Permanently", "Infiltrated") ? information[4] : String.Format(information[4], information[5])), 14);
				yield return new WaitForSeconds(2f);

				hackLCDDisplay.fontSize = 64;
				hackLCDDisplay.text = "Hack #" + (hackModuleIndex + 1);
				isShowing = false;
				break;
			default:
				Debug.LogFormat("[Cheat Checkout #{0}]: Unable to generate text.", modID);
				break;
		}
		yield break;
	}

	IEnumerator Countdown() {
		StopCoroutine("ShieldGlitchButtons");
		countdownStarted = true;
		foreach (TextMesh tm in priceTexts) {
			tm.text = "!!!!";
		}
		foreach (TextMesh tm in actionTexts) {
			if (tm.name.Equals("Patch_TX")) { break; }
			tm.text = "!!!!";
		}
		yield return new WaitForSeconds(30f);
		if (!countdownStarted) { yield break; }
		RestartModule();
		yield break;
	}

	void RestartModule() {
		resets++;
		UpdateVars();
		GetComponent<KMBombModule>().HandleStrike();
		Debug.LogFormat("[Cheat Checkout #{0}]: A strike has been issued as well the module has been reset because it has been hacked!", modID);
		GenerateModule();
		SetOriginalTexts();
	}

	string GlitchInfo(string s, int amount) {
		char[] array = s.ToCharArray();
		for (int i = 0; i <= amount; i++) {
			char[] characters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', ' ' };
			char randomCharacter = characters[rand.Range(0, 11)];
			array[rand.Range(0, array.Length)] = randomCharacter;
		}
		return combineCharArray(array);
	}

	string combineCharArray(char[] array) {
		string s = "";
		for (int i = 0; i <= array.Length - 1; i++) {
			s += array[i];
		}
		return s;
	}

	void SetOriginalTexts() {
		int i = 0;
		foreach (TextMesh tm in priceTexts) {
			tm.text = priceButtons[i].name;
			tm.fontSize = 28;
			i++;
		}
		i = 0;
		foreach (TextMesh tm in actionTexts) {
			tm.text = actionButtons[i+1].name;
			tm.fontSize = 32;
			i++;
		}
		hackedState = false;
	}

	int SumNumbers(List<int> list) {
		int ans = 0;
		foreach (int i in list) {
			ans += i;
		}
		return ans;
	}

	void UpdateVars() {
		countdownStarted = false;
		starting = true;
		wifiStatus = 2;
		shieldStatus = 2;
		wifiSprites[0].enabled = false;
		wifiSprites[1].enabled = false;
		wifiSprites[2].enabled = true;
		shieldSprites[0].enabled = false;
		shieldSprites[1].enabled = false;
		shieldSprites[2].enabled = true;
		cryptoSprites[Array.IndexOf(possibleCryptos,generatedCryptoName)].enabled = false;
	}

	// TP Methods

	bool CorrectArgument(string[] a) {
		string[] valid = new string[] { "right", "left", "r", "l", "lcd", "screen", "display", "submit", "stabilize", "patch" };
		foreach (string s in a)
		{
			int result;
			if (int.TryParse(s, out result)) {
				return true;
			}
			if (valid.Contains(s)) {
				return true;
			}
		}
		return false;
	}

	KMSelectable[] TPPriceButtonSetup(string arg)
	{
		string price = String.Concat(arg.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
		int totalButtons = 0;
		KMSelectable[] array;
		if (price.Length >= 8)
		{
			return null;
		}
		foreach (char c in price) {
			totalButtons += int.Parse(c.ToString());
		}
		array = new KMSelectable[totalButtons+1];
		int x = 0;
		int index = 5;
		int total = 0;
		bool checkedIndex = false;
		foreach (char c in price) {
			if (c == '.') { continue; }
			if (c == 0) { x++; index--; continue; }
			while (!checkedIndex && index != price.Length - 1) { index--; }
			if (!checkedIndex) { checkedIndex = true; }
			for (int i = 0; i < int.Parse(price[x].ToString()); i++) {
				array[total] = priceButtons[index];
				total++;
			}
			x++;
			index--;
		}
		array[total] = actionButtons[1];
		return array;
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} right/left/r/l [Presses the arrows either left or right to choose hack], !{0} lcd/screen/display [Presse the LCD screen for hacks], !{0} submit [Submits with nothing to slap the customer], !{0} submit <change> [Submits the answer into the module], !{0} stabilize <#/##> [Presses 'Stabilize' depending on what is needed], !{0} patch <None/#> [Press 'Patch' depending on what is needed]";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] args = command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (args.Length >= 3) {
			yield return "sendtochaterror That is not the correct amount of arguments, please try again.";
		}
		if (!CorrectArgument(args)) {
			yield return "sendtochaterror That is an incorrect command, please try again.";
		}
		if (args[0].ToLower().EqualsAny("left", "l")) {
			yield return null;
			yield return new KMSelectable[] { directionalButtons[0] };
		}
		if (args[0].ToLower().EqualsAny("right", "r"))
		{
			yield return null;
			yield return new KMSelectable[] { directionalButtons[1] };
		}
		if (args[0].ToLower().EqualsAny("lcd", "screen", "display")) {
			yield return null;
			yield return new KMSelectable[] { actionButtons[0] };
		}
		if (args[0].ToLower().Equals("submit") && args.Length == 1) {
			yield return null;
			yield return new KMSelectable[] { actionButtons[1] };
		}
		if (args[0].ToLower().Equals("submit") && args.Length == 2) {
			SetOriginalTexts();
			hackedState = false;
			yield return null;
			yield return TPPriceButtonSetup(args[1]);
		}
		if (args[0].ToLower().Equals("clear") && args.Length == 1) {
			yield return null;
			yield return new KMSelectable[] { actionButtons[2] };
		}
		if (args[0].ToLower().Equals("stabilize") && args.Length == 2) {
			int result;
			if (!int.TryParse(args[1], out result)) {
				yield return "sendtochaterror Incorrect number format.";
			}
			while ((int)bomb.GetTime() % 10 != int.Parse(args[1]) && args[1].Length == 1) yield return "trycancel The button was not pressed due to a request to cancel.";
			while ((int)bomb.GetTime() % 60 != int.Parse(args[1]) && args[1].Length == 2) yield return "trycancel The button was not pressed due to a request to cancel.";
			yield return null;
			yield return new KMSelectable[] { actionButtons[3] };
		} 
		if (args[0].ToLower().Equals("patch") && args.Length == 1) {
			yield return null;
			yield return new KMSelectable[] { actionButtons[4] };
		}
		if (args[0].ToLower().Equals("patch") && args.Length == 2) {
			int result;
			if (args.Length == 2 && !int.TryParse(args[1], out result))
			{
				yield return "sendtochaterror Incorrect number format.";
			}
			while (args.Length == 2 && (int)bomb.GetTime() % 10 != int.Parse(args[1])) yield return "trycancel The button was not pressed due to a request to cancel.";
			yield return null;
			yield return new KMSelectable[] { actionButtons[4] };
		}
		yield break;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		while (hackedState && isShowing) { }
		while ((customerPaid - totalFromHacks) < 0) { yield return null; yield return new KMSelectable[] { actionButtons[1] }; }
		yield return null;
		yield return TPPriceButtonSetup(correctChange.ToString());
		yield break;
	}

}

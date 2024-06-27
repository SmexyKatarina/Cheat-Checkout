using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using rnd = UnityEngine.Random;

public class CheatCheckoutRemake : MonoBehaviour
{

    public KMBombInfo _bomb;
    public KMAudio _audio;
    public KMColorblindMode _colorblindMode;
    bool _colorblindActive;

    public KMSelectable[] _displayButtons;
    public KMSelectable[] _priceButtons;
    public KMSelectable[] _actionButtons;

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

    // Specifically for Logging
    static int _modIDCount = 1;
    int _modID;
    private bool _modSolved = false;


    // Module Variables
    List<IHack> _hackList;
    DayOfWeek _dayOfWeek;

    string[] _solvedStringArray = new string[] // display1:display2:12pricebuttons:submit:clear:stabilize:patch
    {
        "YOU:DID IT!:YOUSOLVEDTHE:HARDEST:CHECKOUT:MODULE:POGCHAMP!",
        "THAT:WAS NICE:OWOOWOOWOOWO:OWO:OWO:OWO:OWO",
        "EVERYTHING:IS:FINE........:.....:.....:HA HA:GOTTEM",
        "_:CHLOE,:____________:_:_:_:LOOK OUT.",
        "YOU:CAN COUNT:UNLIKEME;KAT:WHO:CAN'T:COUNT:-w-",
        "YOU:GOT:EVERYTHING__:WRONG!:DO:IT:AGAIN",
        "AM I:A MODULE?:ORISTHISALL_:IN:MY:WIRING?:_"
    };

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

    string[] _possibleHacks = new string[] { "DSA", "W", "CI", "XSS", "BFA" };

    string[] _possibleCryptos = new string[] { "Berr", "Bitdrop", "Blade", "Crane", "Evol", "Lapel", "Linecoin", "Penpoint", "Qubit" };
    float[] _cryptoPrices = new float[] { 4.4f, 111f, 1234f, 25f, 69f, 42f, 420f, 777f, 0.5f };
    float[] _buttonPrices = new float[] { 0.001f, 0.01f, 0.1f, 1f, 10f, 100f, 0.005f, 0.05f, 0.5f, 5f, 50f, 500f };

    double _customerTotal, _hackTotal, _hackTotalInCrypto, _expectedChange = -1;
    int _slaps;
    int _chosenCrypto;

    float _inputAmt;

    // Display Variables
    public int _hackIndex;
    public int _hackCycle;
    public bool _fastCycle;

    Coroutine _timer, _animation, _hackCountdown, _warningSound;

    public Coroutine _glitchedButtons;

    Timer t;

    float timePressed = 0;
    float timeReleased = 0;

    void Awake()
    {
        _modID = _modIDCount++;
        _dayOfWeek = DateTime.Now.DayOfWeek;
    }

    void Start()
    {
        t = new Timer(GetInstance(), _wifiSymbols, _VPNSymbols, _colorblindTexts);
        GenerateHacks();

        _colorblindActive = _colorblindMode.ColorblindModeActive;
        ShowColorblindText(_colorblindActive);

        foreach (KMSelectable km in _displayButtons) {
            km.OnInteract += delegate () { if (_modSolved || t.DisableDisplay || t.HackingModeEnabled) return false; timePressed = _bomb.GetTime(); return false; };
            km.OnInteractEnded += delegate () 
            { 
                if (_modSolved || t.DisableDisplay || t.HackingModeEnabled) return;
                timeReleased = _bomb.GetTime();
                int index = Array.IndexOf(_displayButtons, km);
                if (timePressed - timeReleased <= 0.5f && index == 2)
                {
                    if (!_fastCycle)
                    {
                        _fastCycle = true;
                        string text = _hackList[_hackIndex].GetDisplayValues(false)[_hackCycle];
                        List<int> chosen = new List<int>();
                        char[] c = text.ToArray();
                        for (int x = 0; x < rnd.Range(2, text.Length + 1); x++)
                        {
                            int i = rnd.Range(0, text.Length);
                            while (chosen.Contains(i)) i = rnd.Range(0, text.Length);
                            c[i] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                        }
                        _displayTexts[2].text = t.GlitchDisplay ? c.Join("") : _hackList[_hackIndex].GetDisplayValues(false)[_hackCycle];
                    }
                    else
                    {
                        _hackCycle++;
                        if (_hackCycle >= _hackList[_hackIndex].GetDisplayValues(false).Length)
                        {
                            _displayTexts[2].text = "Hack #" + (_hackIndex + 1);
                            _fastCycle = false;
                            _hackCycle = 0;
                            return;
                        }
                        string text = _hackList[_hackIndex].GetDisplayValues(false)[_hackCycle];
                        List<int> chosen = new List<int>();
                        char[] c = text.ToArray();
                        for (int x = 0; x < rnd.Range(2, text.Length + 1); x++)
                        {
                            int i = rnd.Range(0, text.Length);
                            while (chosen.Contains(i)) i = rnd.Range(0, text.Length);
                            c[i] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                        }
                        _displayTexts[2].text = t.GlitchDisplay ? c.Join("") : _hackList[_hackIndex].GetDisplayValues(false)[_hackCycle];
                    }
                }
                else 
                {
                    DisplayButtons(km);
                }
                timePressed = 0;
                timeReleased = 0;
                return; 
            };
        }

        foreach (KMSelectable km in _priceButtons)
            km.OnInteract += delegate () { if (_modSolved || t.HackingModeEnabled) return false; PriceButtons(km); return false; };

        foreach (KMSelectable km in _actionButtons)
            km.OnInteract += delegate () { if (_modSolved) return false; ActionButtons(km); return false; };

    }

    void DisplayButtons(KMSelectable b)
    {
        int index = Array.IndexOf(_displayButtons, b);
        b.AddInteractionPunch(1f);
        if (t.IsGlitched(Array.IndexOf(_allButtons, b)))
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Clicked a glitched button!", _modID);
            return;
        }
        if (_hackList.Any(x => x.Animating) || _fastCycle) return;
        _animation = null;
        switch (index) 
        {
            case 0:
                if (--_hackIndex < 0) _hackIndex = 0;
                _displayTexts[2].text = "Hack #" + (_hackIndex+1);
                break;
            case 1:
                if (++_hackIndex >= _hackList.Count) _hackIndex = _hackList.Count - 1;
                _displayTexts[2].text = "Hack #" + (_hackIndex+1);
                break;
            case 2:
                if (_fastCycle) break;
                _animation = StartCoroutine(_hackList[_hackIndex].CycleHack(1f, _displayTexts[2], t.GlitchDisplay));
                break;
        }
    }

    void PriceButtons(KMSelectable b) 
    {
        int index = Array.IndexOf(_priceButtons, b);
        b.AddInteractionPunch(0.25f);
        if (t.IsGlitched(Array.IndexOf(_allButtons, b)))
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Clicked a glitched button!", _modID);
            return;
        }
        float price = _buttonPrices[index];
        _inputAmt = (float)Math.Round(price + _inputAmt, 3);
        _customerText.text = _inputAmt.ToString();
        _customerText.color = new Color32(180, 60, 100, 255);
    }

    void ActionButtons(KMSelectable b)
    {
        int index = Array.IndexOf(_actionButtons, b);
        b.AddInteractionPunch(0.25f);
        int time = (int)_bomb.GetTime();
        int timeLastTwo = time % 60;
        int serialSum = _bomb.GetSerialNumberNumbers().Sum();
        int serialLast = _bomb.GetSerialNumberNumbers().Last();
        int timeLast = time % 10;
        if (t.HackingModeEnabled) 
        {
            if (index == 3) 
            {
                if (timeLast != serialLast)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Incorrect button press on stabilizing VPN, resetting module! Expected {1} but was given {2}.", _modID, serialLast, timeLast);
                    GenerateModule();
                    return;
                }
                else
                {
                    EndHack();
                    Debug.LogFormat("[Cheat Checkout #{0}]: Fixed VPN, status returned to normal. Whew! Close one wasn't it...", _modID);
                    return;
                }
            }
            return;
        }
        if (t.IsGlitched(Array.IndexOf(_allButtons, b))) 
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Clicked a glitched button!", _modID);
            return;
        }
        

        switch (index) 
        {
            case 0:
                if (_hackTotalInCrypto > _customerTotal && _inputAmt == 0)
                {
                    Debug.LogFormat("[Cheat Checkout #{0}]: Slapping customer :)", _modID);
                    Slap();
                    break;
                }
                else if (Math.Round(_expectedChange - 0.01f, 3) <= _inputAmt && _inputAmt <= Math.Round(_expectedChange + 0.01f, 3))
                {
                    GetComponent<KMBombModule>().HandlePass();
                    StopAllCoroutines();
                    _modSolved = true;
                    StartCoroutine(SolveAnimation());
                    Debug.LogFormat("[Cheat Checkout #{0}]: Module solved!", _modID);
                    break;
                }
                else if (_customerTotal >= _hackTotalInCrypto && _inputAmt != 0)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Expected an answer between {1} - {2} (0.01 range, exact being: {3}). The missing amount required is {4}.", _modID, Math.Round(_expectedChange - 0.01, 3), Math.Round(_expectedChange + 0.01, 3), _expectedChange, _expectedChange - _inputAmt);
                    break;
                }
                else 
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Expected a slap and recieved an answer instead.", _modID);
                    break;
                }
            case 1:
                _customerText.color = new Color32(96, 188, 84, 255);
                _customerText.text = _customerTotal.ToString();
                _inputAmt = 0;
                break;
            case 2:
                
                switch (t.WifiStatus) 
                {
                    case 1:
                        if (timeLastTwo != serialSum)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Incorrect button press on stabilizing wifi! Expected {1} but was given {2}.", _modID, serialSum, timeLastTwo);
                            break;
                        }
                        else
                        {
                            t.FixWifi();
                            Debug.LogFormat("[Cheat Checkout #{0}]: Fixed wifi, status returned to normal, LCD will function normally.", _modID);
                            break;
                        }
                    case 0:
                        if (timeLast != serialLast)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Incorrect press on stabilizing wifi! Expected {1} but was given {2}.", _modID, serialLast, timeLast);
                            break;
                        }
                        else 
                        {
                            t.FixWifi();
                            Debug.LogFormat("[Cheat Checkout #{0}]: Fixed wifi, status returned to normal, LCD will be pressable now.", _modID);
                            break;
                        }
                    default:
                        break;
                }
                break;
            case 3:
                switch (t.VpnStatus)
                {
                    case 1:
                        t.FixVPN();
                        Debug.LogFormat("[Cheat Checkout #{0}]: Fixed VPN, status returned to normal. No hackers here :).", _modID);
                        break;
                    case 0:
                        if (timeLast != serialLast)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Cheat Checkout #{0}]: Strike! Incorrect press on stabilizing wifi! Expected {1} but was given {2}.", _modID, serialLast, timeLast);
                            break;
                        }
                        else
                        {
                            t.FixVPN();
                            Debug.LogFormat("[Cheat Checkout #{0}]: Fixed VPN, status returned to normal. No hackers here :).", _modID);
                            break;
                        }
                    default:
                        break;
                }
                break;
        }
    }

    void Slap() 
    {
        _customerTotal = Math.Round(_customerTotal * (++_slaps == 1 ? rnd.Range(1.05f, 1.25f) : rnd.Range(1+(0.1f*_slaps), 1+(0.2f*_slaps))), 3);
        _customerText.text = _customerTotal.ToString();
        Debug.LogFormat("[Cheat Checkout #{0}]: The customer has been slapped and they are updating their price. The new total given is: {1}. {2}",
            _modID, _customerTotal,
            _customerTotal <= _hackTotalInCrypto ? "The customer has to be slapped again." : "The customer has enough. The expected change is: " + (_expectedChange = Math.Round(_customerTotal - _hackTotalInCrypto, 3)));
    }

    public void GenerateModule() 
    {
        StopAllCoroutines();
        t = new Timer(GetInstance(), _wifiSymbols, _VPNSymbols, _colorblindTexts);
        _timer = null; _animation = null; _hackCountdown = null; _warningSound = null; _glitchedButtons = null;
        foreach (IHack h in _hackList) h.Animating = false;
        t.FixVPN(); t.FixWifi();
        foreach (SpriteRenderer r in _cryptoSymbols) r.enabled = false;
        FixTexts();
        GenerateHacks();
    }

    public void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }

    public int GetModId() { return _modID; }

    void GenerateHacks()
    {
        _hackList = new List<IHack>();
        string name, website;
        List<string> websitesUsed = new List<string>();
        for (int i = 0; i < 5; i++) 
        {
            name = _possibleHacks[rnd.Range(0, _possibleHacks.Length)];
            website = _possibleWebsites[rnd.Range(0, _possibleWebsites.Length)];
            while (websitesUsed.Contains(website))
            {
                website = _possibleWebsites[rnd.Range(0, _possibleWebsites.Length)];
            }
            websitesUsed.Add(website);
            int websiteSecurity = int.Parse(website.Split(':')[1]);
            string websiteType = website.Split(':')[2];
            switch (name) 
            {
                case "DSA":
                    _hackList.Add(new DSA(website, websiteSecurity, ChooseValue(new float[] { 0.8f, 1.2f, 1.6f, 2f }), Discount(websiteType), new object[] { rnd.Range(5, 21), Math.Round(rnd.Range(1.0f, 3.0f), 1) }, Convert.ToBoolean(rnd.Range(0, 2))));
                    break;
                case "W":
                    _hackList.Add(new Worm(website, websiteSecurity, ChooseValue(new float[] { 0.5f, 0.9f, 1.3f, 1.75f, 2.1f }), Discount(websiteType), new object[] { new float[] { 1f, 2f, 0.5f }[rnd.Range(0, 3)], rnd.Range(5, 21)}, Convert.ToBoolean(rnd.Range(0, 2))));
                    break;
                case "CI":
                    _hackList.Add(new CodeInjection(website, websiteSecurity, ChooseValue(new float[] { 0.9f, 1.8f, 1.25f, 2.2f }), Discount(websiteType), new object[] { new float[] { 1f, 1.2f, 1.5f }[rnd.Range(0, 3)], rnd.Range(5, 21) }, Convert.ToBoolean(rnd.Range(0, 2))));
                    break;
                case "XSS":
                    _hackList.Add(new CrossSiteScripting(website, websiteSecurity, ChooseValue(new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f }), Discount(websiteType), new object[] { new float[] { 1f, 1.25f, 1.5f }[rnd.Range(0, 3)], rnd.Range(10, 41) }, Convert.ToBoolean(rnd.Range(0, 2))));
                    break;
                default:
                    _hackList.Add(new BruteForceAttempt(website, websiteSecurity, ChooseValue(new float[] { 2.2f, 1.6f, 1.9f }), Discount(websiteType), new object[] { rnd.Range(5, 21) }, Convert.ToBoolean(rnd.Range(0, 2))));
                    break;
            }
        }

        for (int i = 0; i < 5; i++) Debug.LogFormat("[Cheat Checkout #{0}]: Raw hack data for hack {1}: {2}", _modID, i + 1, string.Join(", ", _hackList[i].GetDisplayValues(true)));

        Debug.LogFormat("[Cheat Checkout #{0}]: The chosen crypto currency is {1} (priced at {2}).", _modID, _possibleCryptos[_chosenCrypto = rnd.Range(0, 9)], _cryptoPrices[_chosenCrypto]);
        _cryptoSymbols[_chosenCrypto].enabled = true;
        Debug.LogFormat("[Cheat Checkout #{0}]: The subtotals for each hack (in order, taking the percentage if it failed and first rounding to third decimal): {1}", _modID, _hackList.Select(x => x.SubtotalPrice).Join(", "));
        Debug.LogFormat("[Cheat Checkout #{0}]: The totals (After applying discounts, for {1}, and second rounding to third decimal) for each hack: {2}", _modID, _dayOfWeek.ToString(), _hackList.Select(x => x.TotalPrice).Join(", "));
        Debug.LogFormat("[Cheat Checkout #{0}]: The total before converting to crypto is {1}.", _modID, _hackTotal = Math.Round(_hackList.Select(x => x.TotalPrice).Sum(), 3));
        Debug.LogFormat("[Cheat Checkout #{0}]: The total price for the hacks in crypto ({1} priced at: {2}) is {3} (Apply the final rounding to third decimal)" +
            ".", _modID, _possibleCryptos[_chosenCrypto], _cryptoPrices[_chosenCrypto], _hackTotalInCrypto = Math.Round(_hackTotal / _cryptoPrices[_chosenCrypto], 3));
        Debug.LogFormat("[Cheat Checkout #{0}]: The price that is being given by the customer is {1}. {2}",
            _modID,
            _customerTotal = Math.Round(rnd.Range((float)(_hackTotalInCrypto - _hackTotalInCrypto * 0.25f), (float)(_hackTotalInCrypto + _hackTotalInCrypto * 0.25f)), 3),
            _customerTotal <= _hackTotalInCrypto ? "The customer has to be slapped." : "The customer has enough. The expected change is: " + (_expectedChange = Math.Round(_customerTotal - _hackTotalInCrypto, 3)));
        _customerText.text = _customerTotal.ToString();
        _customerText.color = new Color32(96, 188, 84, 255);
        _displayTexts[2].text = "Hack #1";
        _timer = StartCoroutine(t.StartTimer(70, 90));
    }

    float Discount(string websiteType) 
    {
        switch (_dayOfWeek) 
        {
            case DayOfWeek.Sunday:
                return websiteType == "SE" ? .8f : 1f;
            case DayOfWeek.Monday:
                return 1.1f;
            case DayOfWeek.Tuesday:
                return websiteType == "GA" ? .8f : 1f;
            case DayOfWeek.Wednesday:
                return websiteType == "IN" ? .8f : 1f;
            case DayOfWeek.Thursday:
                return websiteType == "SM" ? .8f : 1f;
            case DayOfWeek.Friday:
                return .9f;
            default:
                return websiteType == "ST" ? .8f : 1f;
        }
    }

    void FixTexts() 
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
                    t.text = "Hack #" + (_hackIndex + 1);
                    break;
                default:
                    t.text = name;
                    break;
            }
            t.color = new Color32(96, 188, 84, 255);
        }
    }

    public void InitializeHack() 
    {
        StopAllCoroutines();
        foreach (IHack h in _hackList) h.Animating = false;
        foreach (TextMesh t in _allTexts.Where(x => x.name != "PATCH")) { t.text = ""; t.color = new Color32(253, 1, 1, 255); }
        _customerText.color = new Color32(253, 1, 1, 255);
        _cryptoSymbols[_chosenCrypto].enabled = false;
        _hackCountdown = StartCoroutine(t.HackTimer(30, _customerText));
        _warningSound = StartCoroutine(WarningSound(2.5f));
    }

    public void EndHack() 
    {
        StopAllCoroutines();
        _timer = null; _animation = null; _hackCountdown = null; _warningSound = null; _glitchedButtons = null;
        _cryptoSymbols[_chosenCrypto].enabled = true;
        _customerText.text = _customerTotal.ToString();
        _customerText.color = new Color32(96, 188, 84, 255);
        _hackIndex = 0;
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
                    t.text = "Hack #" + (_hackIndex + 1);
                    break;
                default:
                    t.text = name;
                    break;
            }
            t.color = new Color32(96, 188, 84, 255);
        }
        t.FixVPN();
        t.FixWifi();
        _timer = StartCoroutine(t.StartTimer(70, 90));
    }

    public void StartGlitched() 
    {
        if (_glitchedButtons != null) return;
        _glitchedButtons = StartCoroutine(t.GlitchedButtonsTimer(5, 10));
        Debug.LogFormat("[Cheat Checkout #{0}]: Buttons are being glitched.", _modID);
    }

    public void EndGlitched() 
    {
        StopCoroutine(_glitchedButtons);
        _glitchedButtons = null;
        FixTexts();
        t._glitchedButtonTexts = new Dictionary<int, string>();
        Debug.LogFormat("[Cheat Checkout #{0}]: Buttons are no longer being glitched.", _modID);
    }

    IEnumerator WarningSound(float delay) 
    {
        while (true) 
        {
            _audio.PlaySoundAtTransform(_HIPSound.name, transform);
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator SolveAnimation() 
    {
        string[] solve = _solvedStringArray[rnd.Range(0, _solvedStringArray.Length)].Split(':');
        bool jump = false;
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
        for (int i = 0; i < solve.Length; i++) 
        {
            if (i == 0) _customerText.text = solve[i] == "_" ? " " : solve[i]; 
            else if (i == 2) 
            {
                for (int x = 0; x < solve[i].Length; x++) 
                {
                    _allTexts[i + x + 1].text = solve[i][x] == '_' ? " " : solve[i][x].ToString();
                    yield return new WaitForSeconds(0.1f);
                }
                jump = true;
            }
            else _allTexts[i + 1 + (jump ? 11 : 0)].text = solve[i] == "_" ? " " : solve[i];
            yield return new WaitForSeconds(0.25f);
        }
        yield break;
    }

    private T ChooseValue<T>(T[] array) 
    {
        return array[rnd.Range(0, array.Length)];
    }

    void ShowColorblindText(bool colorblindActive)
    {
        for (int i = 0; i < _colorblindTexts.Length; i++)
            _colorblindTexts[i].gameObject.SetActive(colorblindActive);
    }

    public bool DisplayStatus(bool AlreadyLogged) 
    {
        if (AlreadyLogged) return true;
        if (t.DisableDisplay)
            Debug.LogFormat("[Cheat Checkout #{0}]: The display is now disabled and unable to be used. It must be patched before it can be used.", _modID);
        else if (t.GlitchDisplay)
            Debug.LogFormat("[Cheat Checkout #{0}]: The display is glitched, some of the letters/numbers will be replaced with symbols.", _modID);
        else
            Debug.LogFormat("[Cheat Checkout #{0}]: The display is now working properly.", _modID);
        return true;
    }

    private CheatCheckoutRemake GetInstance() { return this; }

    // TP

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} hack <#> [Goes to # hack], !{0} lcd/screen/display <slowcycle,sc/fastcycle,fc/fastfull,ff> <delay for fast/full cycle> [Slow cycles, fast cycles or fast cycles all hacks, with an optional custom delay], !{0} submit/sub <change> [Submit with no arguments to slap the customer, otherwise add the change to submit. Clears before submitting/slapping.], !{0} stabilize/stbl <#/##> [Presses 'Stabilize' (remember anything less than ten is ""0#"")], !{0} patch <#> [Press 'Patch' on #, leave blank if needed], !{0} cb/colorblind/colourblind [Toggles colourblind mode].";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string cmd) 
    {
        string[] args = cmd.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        RegexOptions ropts = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        if (Regex.IsMatch(cmd, @"^hack\s([1-5]{1})$", ropts))
        {
            yield return null;
            if (t.DisableDisplay) { yield return "sendtochat Display is disabled because of wifi status!"; yield break; }
            int index = int.Parse(args[1]) - 1;
            while (_hackIndex != index)
            {
                yield return null;
                while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[0])) || t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[1]))) { yield return "trycancel Changing index was cancelled."; yield return null; }
                if (_hackIndex > index) _displayButtons[0].OnInteractEnded();
                else _displayButtons[1].OnInteractEnded();
            }
            yield break;
        }
        else if (Regex.IsMatch(cmd, @"^(lcd|screen|display)\s(slowcycle|sc|fastcycle|fc|fastfull|ff)$", ropts) || Regex.IsMatch(cmd, @"^(lcd|screen|display)\s(slowcycle|sc|fastcycle|fc|fastfull|ff)\s(\d+\.?\d*)$"))
        {
            switch (args[1])
            {
                case "slowcycle":
                case "sc":
                    yield return null;
                    if (t.DisableDisplay) { yield return "sendtochat Display is disabled because of wifi status!"; yield break; }
                    while (_hackCycle != 0)
                    {
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[2].OnInteractEnded();
                    }
                    while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                    yield return _displayButtons[2];
                    yield return new WaitForSeconds(1f);
                    yield return _displayButtons[2];
                    while (_hackList[_hackIndex].Animating) { yield return "trycancel Cycle viewing was cancelled."; yield return null; }
                    yield break;
                case "fastcycle":
                case "fc":
                    yield return null;
                    if (t.DisableDisplay) { yield return "sendtochat Display is disabled because of wifi status!"; yield break; }
                    while (_hackCycle != 0)
                    {
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[2].OnInteractEnded();
                    }
                    for (int i = 0; i < _hackList[_hackIndex].GetDisplayValues(false).Length; i++)
                    {
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[2].OnInteractEnded();
                        yield return "trywaitcancel " + (args.Length == 3 ? args[2] : "0.35") + " Cycle was cancelled";
                    }
                    _displayButtons[2].OnInteractEnded();
                    yield break;
                case "fastfull":
                case "ff":
                    yield return null;
                    if (t.DisableDisplay) { yield return "sendtochat Display is disabled because of wifi status!"; yield break; }
                    while (_hackCycle != 0)
                    {
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[2].OnInteractEnded();
                    }
                    while (_hackIndex != 0)
                    {
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[0]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[0].OnInteractEnded();
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        for (int x = 0; x < _hackList[_hackIndex].GetDisplayValues(false).Length; x++)
                        {
                            while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                            _displayButtons[2].OnInteractEnded();
                            yield return "trywaitcancel " + (args.Length == 3 ? args[2] : "0.35") + " Cycle was cancelled";
                        }
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[2]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[2].OnInteractEnded();
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[1]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[1].OnInteractEnded();
                        yield return new WaitForSeconds(0.35f);
                    }
                    while (_hackIndex != 0)
                    {
                        while (t.IsGlitched(Array.IndexOf(_allButtons, _displayButtons[0]))) { yield return "trycancel Cycle was cancelled."; yield return null; }
                        _displayButtons[0].OnInteractEnded();
                    }
                    yield break;
                default:
                    yield break;
            }
        }
        else if (Regex.IsMatch(cmd, @"^(sub|submit)$") || Regex.IsMatch(cmd, @"^(sub|submit)\s(\d+\.?\d*)$"))
        {
            if (args.Length == 1)
            {
                yield return null;
                while (t.IsGlitched(Array.IndexOf(_allButtons, _actionButtons[1]))) { yield return "trycancel Clearing was cancelled."; yield return null; }
                _actionButtons[1].OnInteract();
                while (t.IsGlitched(Array.IndexOf(_allButtons, _actionButtons[0]))) { yield return "trycancel Slapping was cancelled."; yield return null; }
                _actionButtons[0].OnInteract();
                yield break;
            }
            else if (args.Length == 2)
            {
                yield return null;
                while (t.IsGlitched(Array.IndexOf(_allButtons, _actionButtons[1]))) { yield return "trycancel Clearing was cancelled."; yield return null; }
                _actionButtons[1].OnInteract();
                foreach (KMSelectable km in GetButtonPresses(float.Parse(args[1])))
                {
                    while (t.IsGlitched(Array.IndexOf(_allButtons, km))) { yield return "trycancel Inputting change was cancelled."; yield return null; }
                    km.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                yield return "solve";
                _actionButtons[0].OnInteract();
                yield break;
            }
            yield break;
        }
        else if (Regex.IsMatch(cmd, @"^(stabilize|stbl)\s(\d{1,2})$"))
        {
            if (t.WifiStatus == 2) { yield return "sendtochaterror {0}, the WIFI is already stabilized!"; }
            yield return "strike";
            if (t.WifiStatus == 0)
            {
                while (t.IsGlitched(Array.IndexOf(_allButtons, _actionButtons[2])) || (int)(_bomb.GetTime() % 10) != int.Parse(args[1])) { yield return "trycancel Stablizing was cancelled."; yield return null; }
                _actionButtons[2].OnInteract();
                yield break;
            }
            else if (t.WifiStatus == 1)
            {
                while (t.IsGlitched(Array.IndexOf(_allButtons, _actionButtons[2])) || (int)(_bomb.GetTime() % 60) != int.Parse(args[1])) { yield return "trycancel Stablizing was cancelled."; if (t.WifiStatus != 1) { yield return "sendtochaterror Module status changed. Cancelling button press"; yield break; } yield return null; }
                _actionButtons[2].OnInteract();
                yield break;
            }
            yield break;
        }
        else if (Regex.IsMatch(cmd, @"^(patch)|((patch)\s(\d{1}))$"))
        {
            if (t.VpnStatus == 2) { yield return "sendtochaterror {0}, the VPN is already fixed!"; }
            yield return "strike";
            if (args.Length == 2)
            {
                yield return null;
                while ((int)(_bomb.GetTime() % 10) != int.Parse(args[1])) { yield return "trycancel Fixing was cancelled."; yield return null; }
                _actionButtons[3].OnInteract();
                yield break;
            }
            else if (args.Length == 1)
            {
                yield return null;
                _actionButtons[3].OnInteract();
                yield break;
            }
            yield break;
        }
        else if (Regex.IsMatch(cmd, @"^(cb|colo(u)?rblind)$")) 
        {
            yield return null;
            ShowColorblindText(_colorblindActive = !_colorblindActive);
            yield break;
        }
        else
        {
            yield return "sendtochaterror The command that was given as invalid!";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        StopAllCoroutines();
        yield return null;
        t.FixVPN(); t.FixWifi();
        _actionButtons[1].OnInteract();
        while (_hackTotalInCrypto > _customerTotal) { yield return null; _actionButtons[0].OnInteract(); } 
        foreach (KMSelectable km in GetButtonPresses(float.Parse(_expectedChange.ToString())))
        {
            km.OnInteract();
            yield return true;
            yield return new WaitForSeconds(0.025f);
        }
        _actionButtons[0].OnInteract();
        yield break;
    }

    public KMSelectable[] GetButtonPresses(float input) 
    {
        // 254.749f new int[] { 0, 2, 1, 0, 0, 4, 1, 2, 0, 4, 1, 4 }
        float[] values = new float[] { 500f, 100f, 50f, 10f, 5f, 1f, 0.5f, 0.1f, 0.05f, 0.01f, 0.005f, 0.001f };
        string[] names = new string[] { "500", "100", "50", "10", "5", "1", "0.5", "0.1", "0.05", "0.01", "0.005", "0.001" };
        int[] amt = new int[values.Length];
        float temp = input;
        int index = 0;
        while (temp > 0 && index != 12) 
        {
            while ((temp - values[index]) >= 0) 
            { amt[index]++; temp = (float)Math.Round(temp - values[index], 3); }
            index++;
        }
        List<KMSelectable> buttons = new List<KMSelectable>();
        for (int i = 0; i < amt.Length; i++) 
            for (int x = 0; x < amt[i]; x++)
                buttons.Add(_priceButtons[Array.IndexOf(_priceButtons.Select(y => y.name).ToArray(), names[i])]);
        return buttons.ToArray();
    }

}

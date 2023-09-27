using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rnd = UnityEngine.Random;

public class Timer {


    int wifiStatus = 2;
    public int WifiStatus 
    {
        get { return wifiStatus; }
        set { wifiStatus = value; }
    }

    int vpnStatus = 2;
    public int VpnStatus 
    {
        get { return vpnStatus; }
        set { vpnStatus = value; }
    }

    bool glitchDisplay, disableDisplay;
    public bool GlitchDisplay
    {
        get { return glitchDisplay; }
        set { glitchDisplay = value; }
    }
    public bool DisableDisplay
    {
        get { return disableDisplay; }
        set { disableDisplay = value; }
    }

    bool glitchButtons, hackingModeEnabled;
    public bool GlitchButtons
    {
        get { return glitchButtons; }
        set { glitchButtons = value; }
    }
    public bool HackingModeEnabled
    {
        get { return hackingModeEnabled; }
        set { hackingModeEnabled = value; }
    }

    CheatCheckoutRemake _ccr;

    SpriteRenderer[] _wifiSymbols;
    SpriteRenderer[] _VPNSymbols;
    TextMesh[] _colorblindTexts;

    bool[] _glitched = new bool[19];
    int _sentLogValue = -1;
    public Dictionary<int, string> _glitchedButtonTexts = new Dictionary<int, string>();

    public Timer(CheatCheckoutRemake ccr, SpriteRenderer[] wifiSymbols, SpriteRenderer[] VPNSymbols, TextMesh[] ColorblindTexts)
    {
        _ccr = ccr;
        _wifiSymbols = wifiSymbols;
        _VPNSymbols = VPNSymbols;
        _colorblindTexts = ColorblindTexts;
    }

    public IEnumerator StartTimer(int minDelay, int maxDelay) 
    {
        bool pause = false;
        while (true) 
        {
            if (!pause) { pause = true; yield return new WaitForSeconds(45f); } 
            if (rnd.Range(0, 2) == 1)
                wifiStatus = (wifiStatus-1) < 0 ? wifiStatus : wifiStatus - 1;
            else
                vpnStatus = (vpnStatus-1) < 0 ? vpnStatus : vpnStatus - 1;
            DisableAll();
            _wifiSymbols[wifiStatus].enabled = true;
            _VPNSymbols[vpnStatus].enabled = true;
            RunCheck();
            yield return new WaitForSeconds(rnd.Range(minDelay, maxDelay+1));
        }
    }

    public IEnumerator HackTimer(int delay, TextMesh customerDisplay) 
    {
        int timer = 0;
        bool warning = false;
        while (true) 
        {
            if (++timer >= delay) 
            {
                _ccr.Strike();
                Debug.LogFormat("[Cheat Checkout #{0}]: UH OH! Module hacked! L for you I guess, module reset!", _ccr.GetModId());
                _ccr.GenerateModule();
                break;
            }
            if ((warning = !warning) == false)
                customerDisplay.text = (delay - timer).ToString();
            else
                customerDisplay.text = "!!" + (delay - timer) + "!!";
            yield return new WaitForSeconds(1f);
        }
    }

    public IEnumerator GlitchedButtonsTimer(int minDelay, int maxDelay) 
    {
        while (true) 
        {
            List<int> indexes = new List<int>();
            Color32 glitched = new Color32(198, 1, 1, 255);
            Color32 baseColor = new Color32(96, 188, 84, 255);
            for (int i = 0; i < 5; i++) 
            {
                int x = rnd.Range(0, 18);
                while (_glitchedButtonTexts.ContainsKey(x)) x = rnd.Range(0, 18);
                indexes.Add(x);
                _glitchedButtonTexts.Add(x, _ccr._allTexts[x].text);
                _ccr._allTexts[x].text = "/!\\";
                _ccr._allTexts[x].color = glitched;
                _glitched[x] = true;
            }
            yield return new WaitForSeconds(5);
            foreach (KeyValuePair<int, string> pair in _glitchedButtonTexts) 
            {
                _ccr._allTexts[pair.Key].text = pair.Value;
                _ccr._allTexts[pair.Key].color = baseColor;
            }
            foreach (int i in indexes) _glitched[i] = false;
            _glitchedButtonTexts = new Dictionary<int, string>();
            yield return new WaitForSeconds(rnd.Range(minDelay, maxDelay+1));
        }
    }


    public bool IsGlitched(int index) { return _glitched[index]; }

    void DisableAll() 
    {
        for (int i = 0; i < 3; i++) 
        {
            _wifiSymbols[i].enabled = false;
            _VPNSymbols[i].enabled = false;
        }
    }

    public void FixWifi() 
    {
        WifiStatus = 2;
        DisableDisplay = false; GlitchDisplay = false;
        DisableAll();
        _wifiSymbols[wifiStatus].enabled = true;
        _VPNSymbols[vpnStatus].enabled = true;
        _colorblindTexts[0].text = "G";
    }

    public void FixVPN() 
    {
        VpnStatus = 2;
        if (_ccr._glitchedButtons != null) _ccr.EndGlitched();
        foreach (KeyValuePair<int, string> pair in _glitchedButtonTexts)
        {
            _ccr._allTexts[pair.Key].text = pair.Value;
            _ccr._allTexts[pair.Key].color = new Color32(96, 188, 84, 255);
        }
        _glitchedButtonTexts = new Dictionary<int, string>();
        _glitched = new bool[19];
        HackingModeEnabled = false; GlitchButtons = false;
        DisableAll();
        _wifiSymbols[wifiStatus].enabled = true;
        _VPNSymbols[vpnStatus].enabled = true;
        _colorblindTexts[1].text = "G";
    }

    void RunCheck() 
    {
        switch (WifiStatus) 
        {
            case 0:
                DisableDisplay = true;
                GlitchDisplay = false;
                _colorblindTexts[0].text = "R";
                _ccr.DisplayStatus(_sentLogValue == WifiStatus);
                break;
            case 1:
                DisableDisplay = false;
                GlitchDisplay = true;
                _colorblindTexts[0].text = "Y";
                _ccr.DisplayStatus(_sentLogValue == WifiStatus);
                break;
            case 2:
                DisableDisplay = false;
                GlitchDisplay = false;
                _colorblindTexts[0].text = "G";
                _ccr.DisplayStatus(_sentLogValue == WifiStatus);
                break;
        }
        if (_sentLogValue != wifiStatus) _sentLogValue = wifiStatus;
        switch (VpnStatus)
        {
            case 0:
                HackingModeEnabled = true;
                GlitchButtons = false;
                _ccr.InitializeHack();
                _colorblindTexts[1].text = "R";
                break;
            case 1:
                HackingModeEnabled = false;
                GlitchButtons = true;
                _ccr.StartGlitched();
                _colorblindTexts[1].text = "Y";
                break;
            case 2:
                HackingModeEnabled = false;
                GlitchButtons = false;
                _colorblindTexts[1].text = "G";
                break;
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using rnd = UnityEngine.Random;

interface IHack
{

    string Website { get; set; }
    int WebsiteSecurity { get; set; }
    float BaseValue { get; set; }
    object[] AdditionalInformation { get; set; }
    bool HackSuccessful { get; set; }

    float SubtotalPrice { get; set; }
    float Discount { get; set; }
    float TotalPrice { get; set; }

    bool Animating { get; set; }

    float GenerateHackPrice();

    string[] GetDisplayValues(bool logging);

    IEnumerator CycleHack(float cycleDelay, TextMesh displayMesh, bool glitched);

}

public class DSA : IHack
{

    string website;
    public string Website
    {
        get { return website; }
        set { website = value; }
    }

    int siteSecurity;
    public int WebsiteSecurity
    {
        get { return siteSecurity; }
        set { siteSecurity = value; }
    }

    float baseValue;
    public float BaseValue
    {
        get { return baseValue; }
        set { baseValue = value; }
    }

    object[] additionalInformation;
    public object[] AdditionalInformation
    {
        get { return additionalInformation; }
        set { additionalInformation = value; }
    }

    bool hackSuccessful;
    public bool HackSuccessful
    {
        get { return hackSuccessful; }
        set { hackSuccessful = value; }
    }

    float subtotal;
    public float SubtotalPrice
    {
        get { return subtotal; }
        set { subtotal = value; }
    }

    float discount;
    public float Discount
    {
        get { return discount; }
        set { discount = value; }
    }

    float total;
    public float TotalPrice
    {
        get { return total; }
        set { total = value; }
    }

    bool crashedPermanently;
    public bool IsCrashedPermanently
    {
        get { return crashedPermanently; }
        set { crashedPermanently = value; }
    }

    bool animating;
    public bool Animating
    {
        get { return animating; }
        set { animating = value; }
    }

    float failedPercentage;

    float[] vals = new float[] { 0.8f, 1.2f, 1.6f, 2f };
    string[] names = new string[] { "Basic", "Advanced", "Supercomp.", "Quantum" };

    public DSA(string Website, int WebsiteSecurity, float BaseValue, float Discount, object[] AdditionalInformation, bool HackSuccessful)
    {
        this.Website = Website;
        this.WebsiteSecurity = WebsiteSecurity;
        this.BaseValue = BaseValue;
        this.Discount = Discount;
        this.AdditionalInformation = AdditionalInformation;
        this.HackSuccessful = HackSuccessful;
        GenerateHackPrice();
    }

    public float GenerateHackPrice()
    {
        SubtotalPrice = BaseValue * Convert.ToSingle(AdditionalInformation[0]) * (WebsiteSecurity / 5f) * Convert.ToSingle(AdditionalInformation[1]);
        SubtotalPrice = (float)Math.Round(SubtotalPrice * (HackSuccessful ? ((IsCrashedPermanently = Convert.ToBoolean(rnd.Range(0, 2))) ? 1.25f : 1f) : (failedPercentage = Mathf.Round(rnd.Range(1.0f, 100.0f)) / 100f)), 3);
        return TotalPrice = (float)Math.Round(SubtotalPrice * Discount, 3);
    }

    public string[] GetDisplayValues(bool logging) 
    {
        
        return logging ?
            new string[]
            {
                "Site: " + Website.Split(':')[0] + " (Security Value: " + WebsiteSecurity + ", Website Type: " + Website.Split(':')[2] + ")",
                "Hack Method: " + "Denial of Service Attack",
                "PC-Type: " + names[Array.IndexOf(vals, BaseValue)] + " (" + BaseValue + ")",
                "PCs Used: " + AdditionalInformation[0].ToString(),
                "Duration: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? new string[] { "Result: Crashed Temp.", "Result: Crashed Permanently" }[Convert.ToInt32(IsCrashedPermanently)] : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            } 
        : 
            new string[]
            {
                "Site: " + Website.Split(':')[0],
                "Hack Method: " + "DSA",
                "PC-Type: " + names[Array.IndexOf(vals, BaseValue)],
                "PCs Used: " + AdditionalInformation[0].ToString(),
                "Duration: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? new string[] { "Result: Crashed Temp.", "Result: Crashed Perm." }[Convert.ToInt32(IsCrashedPermanently)] : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            };
    }

    public IEnumerator CycleHack(float cycleDelay, TextMesh displayMesh, bool glitched)
    {
        animating = true;
        string temp = displayMesh.text;
        for (int i = 0; i < GetDisplayValues(false).Length; i++)
        {
            if (glitched)
            {
                List<int> chosen = new List<int>();
                char[] text = GetDisplayValues(false)[i].ToArray();
                for (int x = 0; x < rnd.Range(2, GetDisplayValues(false)[i].Length + 1); x++)
                {
                    int c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    while (chosen.Contains(c)) c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    text[c] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                }
                displayMesh.text = text.Join("");
            }
            else displayMesh.text = GetDisplayValues(false)[i];
            yield return new WaitForSeconds(cycleDelay);
        }
        displayMesh.text = temp;
        animating = false;
    }

}

public class Worm : IHack
{

    string website;
    public string Website
    {
        get { return website; }
        set { website = value; }
    }

    int siteSecurity;
    public int WebsiteSecurity
    {
        get { return siteSecurity; }
        set { siteSecurity = value; }
    }

    float baseValue;
    public float BaseValue
    {
        get { return baseValue; }
        set { baseValue = value; }
    }

    object[] additionalInformation;
    public object[] AdditionalInformation
    {
        get { return additionalInformation; }
        set { additionalInformation = value; }
    }

    bool hackSuccessful;
    public bool HackSuccessful
    {
        get { return hackSuccessful; }
        set { hackSuccessful = value; }
    }

    float subtotal;
    public float SubtotalPrice
    {
        get { return subtotal; }
        set { subtotal = value; }
    }

    float discount;
    public float Discount
    {
        get { return discount; }
        set { discount = value; }
    }

    float total;
    public float TotalPrice
    {
        get { return total; }
        set { total = value; }
    }

    bool animating;
    public bool Animating
    {
        get { return animating; }
        set { animating = value; }
    }

    float failedPercentage;

    float[] vals = new float[] { 0.5f, 0.9f, 1.3f, 1.75f, 2.1f };
    string[] names = new string[] { "Defective", "Basic", "Advanced", "Supercomp.", "Quantum" };

    float[] multis = new float[] { 1f, 2f, 0.5f };
    string[] worms = new string[] { "Normal", "Lethal", "Spreader" };

    public Worm(string Website, int WebsiteSecurity, float BaseValue, float Discount, object[] AdditionalInformation, bool HackSuccessful)
    {
        this.Website = Website;
        this.WebsiteSecurity = WebsiteSecurity;
        this.BaseValue = BaseValue;
        this.Discount = Discount;
        this.AdditionalInformation = AdditionalInformation;
        this.HackSuccessful = HackSuccessful;
        GenerateHackPrice();
    }

    public float GenerateHackPrice()
    {
        SubtotalPrice = BaseValue * Convert.ToSingle(AdditionalInformation[1]) * (WebsiteSecurity / 10f) * Convert.ToSingle(AdditionalInformation[0]);
        SubtotalPrice = (float)Math.Round(SubtotalPrice * (HackSuccessful ? 1f : (failedPercentage = Mathf.Round(rnd.Range(1.0f, 100.0f)) / 100f)), 3);
        return TotalPrice = (float)Math.Round(SubtotalPrice * Discount , 3);
    }

    public string[] GetDisplayValues(bool logging)
    {
        return logging ?
            new string[]
            {
                "Site: " + Website.Split(':')[0] + " (Security Value: " + WebsiteSecurity + ", Website Type: " + Website.Split(':')[2] + ")",
                "Hack Method: " + "Worm",
                "PC-Type: " + names[Array.IndexOf(vals, BaseValue)] + " (" + BaseValue + ")",
                "Worm: " + worms[Array.IndexOf(multis, Convert.ToSingle(AdditionalInformation[0]))] + " (" + AdditionalInformation[0].ToString() + ")",
                "Infected PCs: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? "Successful" : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            }
        :
            new string[]
            {
                "Site: " + Website.Split(':')[0],
                "Hack Method: " + "W",
                "PC-Type: " + names[Array.IndexOf(vals, BaseValue)],
                "Worm: " + worms[Array.IndexOf(multis, Convert.ToSingle(AdditionalInformation[0]))],
                "Infected PCs: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? "Successful" : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            };
    }


    public IEnumerator CycleHack(float cycleDelay, TextMesh displayMesh, bool glitched)
    {
        animating = true;
        string temp = displayMesh.text;
        for (int i = 0; i < GetDisplayValues(false).Length; i++)
        {
            if (glitched)
            {
                List<int> chosen = new List<int>();
                char[] text = GetDisplayValues(false)[i].ToArray();
                for (int x = 0; x < rnd.Range(2, GetDisplayValues(false)[i].Length + 1); x++)
                {
                    int c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    while (chosen.Contains(c)) c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    text[c] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                }
                displayMesh.text = text.Join("");
            }
            else displayMesh.text = GetDisplayValues(false)[i];
            yield return new WaitForSeconds(cycleDelay);
        }
        displayMesh.text = temp;
        animating = false;
    }

}

public class CodeInjection : IHack
{

    string website;
    public string Website
    {
        get { return website; }
        set { website = value; }
    }

    int siteSecurity;
    public int WebsiteSecurity
    {
        get { return siteSecurity; }
        set { siteSecurity = value; }
    }

    float baseValue;
    public float BaseValue
    {
        get { return baseValue; }
        set { baseValue = value; }
    }

    object[] additionalInformation;
    public object[] AdditionalInformation
    {
        get { return additionalInformation; }
        set { additionalInformation = value; }
    }

    bool hackSuccessful;
    public bool HackSuccessful
    {
        get { return hackSuccessful; }
        set { hackSuccessful = value; }
    }

    float subtotal;
    public float SubtotalPrice
    {
        get { return subtotal; }
        set { subtotal = value; }
    }

    float discount;
    public float Discount
    {
        get { return discount; }
        set { discount = value; }
    }

    float total;
    public float TotalPrice
    {
        get { return total; }
        set { total = value; }
    }

    bool hostInflitrated;
    public bool HostInflitrated
    {
        get { return hostInflitrated; }
        set { hostInflitrated = value; }
    }

    bool animating;
    public bool Animating
    {
        get { return animating; }
        set { animating = value; }
    }

    float failedPercentage;

    float[] vals = new float[] { 0.9f, 1.8f, 1.25f, 2.2f };
    string[] names = new string[] { "SQL", "LDAP", "XPath", "NoSQL" };

    float[] multis = new float[] { 1f, 1.2f, 1.5f };
    string[] complexs = new string[] { "Simple", "Advanced", "Complex" };

    public CodeInjection(string Website, int WebsiteSecurity, float BaseValue, float Discount, object[] AdditionalInformation, bool HackSuccessful)
    {
        this.Website = Website;
        this.WebsiteSecurity = WebsiteSecurity;
        this.BaseValue = BaseValue;
        this.Discount = Discount;
        this.AdditionalInformation = AdditionalInformation;
        this.HackSuccessful = HackSuccessful;
        GenerateHackPrice();
    }

    public float GenerateHackPrice()
    {
        SubtotalPrice = BaseValue * Convert.ToSingle(AdditionalInformation[0]) * Convert.ToSingle(AdditionalInformation[1]) * (WebsiteSecurity / 20f);
        SubtotalPrice = (float)Math.Round(SubtotalPrice * (HackSuccessful ? ((HostInflitrated = Convert.ToBoolean(rnd.Range(0, 2))) ? 1.5f : 1.25f) : (failedPercentage = Mathf.Round(rnd.Range(1.0f, 100.0f)) / 100f)), 3);
        return TotalPrice = (float)Math.Round(SubtotalPrice * Discount, 3);
    }

    public string[] GetDisplayValues(bool logging)
    {
        return logging ?
            new string[]
            {
                "Site: " + Website.Split(':')[0] + " (Security Value: " + WebsiteSecurity + ", Website Type: " + Website.Split(':')[2] + ")",
                "Hack Method: " + "Code Injection",
                "Vulnerability: " + names[Array.IndexOf(vals, BaseValue)] + " (" + BaseValue + ")",
                "Complexity: " + complexs[Array.IndexOf(multis, Convert.ToSingle(AdditionalInformation[0]))] + " (" + AdditionalInformation[0].ToString() + ")",
                "Batches: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? new string[] { "Result: Crashed Permanently", "Result: Host Infiltrated" }[Convert.ToInt32(HostInflitrated)] : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            }
        : 
            new string[]
            {
                "Site: " + Website.Split(':')[0],
                "Hack Method: " + "CI",
                "Vulnerability: " + names[Array.IndexOf(vals, BaseValue)],
                "Complexity: " + complexs[Array.IndexOf(multis, Convert.ToSingle(AdditionalInformation[0]))],
                "Batches: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? new string[] { "Result: Crashed Perm.", "Result: Host Infiltrated" }[Convert.ToInt32(HostInflitrated)] : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            };
    }

    public IEnumerator CycleHack(float cycleDelay, TextMesh displayMesh, bool glitched)
    {
        animating = true;
        string temp = displayMesh.text;
        for (int i = 0; i < GetDisplayValues(false).Length; i++)
        {
            if (glitched)
            {
                List<int> chosen = new List<int>();
                char[] text = GetDisplayValues(false)[i].ToArray();
                for (int x = 0; x < rnd.Range(2, GetDisplayValues(false)[i].Length + 1); x++)
                {
                    int c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    while (chosen.Contains(c)) c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    text[c] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                }
                displayMesh.text = text.Join("");
            }
            else displayMesh.text = GetDisplayValues(false)[i];
            yield return new WaitForSeconds(cycleDelay);
        }
        displayMesh.text = temp;
        animating = false;
    }

}

public class CrossSiteScripting : IHack
{

    string website;
    public string Website
    {
        get { return website; }
        set { website = value; }
    }

    int siteSecurity;
    public int WebsiteSecurity
    {
        get { return siteSecurity; }
        set { siteSecurity = value; }
    }

    float baseValue;
    public float BaseValue
    {
        get { return baseValue; }
        set { baseValue = value; }
    }

    object[] additionalInformation;
    public object[] AdditionalInformation
    {
        get { return additionalInformation; }
        set { additionalInformation = value; }
    }

    bool hackSuccessful;
    public bool HackSuccessful
    {
        get { return hackSuccessful; }
        set { hackSuccessful = value; }
    }

    float subtotal;
    public float SubtotalPrice
    {
        get { return subtotal; }
        set { subtotal = value; }
    }

    float discount;
    public float Discount
    {
        get { return discount; }
        set { discount = value; }
    }

    float total;
    public float TotalPrice
    {
        get { return total; }
        set { total = value; }
    }

    bool animating;
    public bool Animating 
    {
        get { return animating; }
        set { animating = value; }
    }

    float failedPercentage;

    float[] vals = new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f };
    string[] names = new string[] { "Ex. Basic", "Basic", "Advanced", "Complex", "Unintell." };

    float[] multis = new float[] { 1f, 1.25f, 1.5f };
    string[] hackTypes = new string[] { "Non-Pers.", "Persist.", "Muta. XSS" };

    public CrossSiteScripting(string Website, int WebsiteSecurity, float BaseValue, float Discount, object[] AdditionalInformation, bool HackSuccessful)
    {
        this.Website = Website;
        this.WebsiteSecurity = WebsiteSecurity;
        this.BaseValue = BaseValue;
        this.Discount = Discount;
        this.AdditionalInformation = AdditionalInformation;
        this.HackSuccessful = HackSuccessful;
        GenerateHackPrice();
    }

    public float GenerateHackPrice()
    {
        SubtotalPrice = BaseValue * Convert.ToSingle(AdditionalInformation[0]) * (WebsiteSecurity / 8f) * (Convert.ToSingle(AdditionalInformation[1]) / 2);
        SubtotalPrice = (float)Math.Round(SubtotalPrice * (HackSuccessful ? 1f : (failedPercentage = Mathf.Round(rnd.Range(1.0f, 100.0f)) / 100f)), 3);
        return TotalPrice = (float)Math.Round(SubtotalPrice * Discount, 3);
    }

    public string[] GetDisplayValues(bool logging)
    {
        return logging ?
            new string[]
            {
                "Site: " + Website.Split(':')[0] + " (Security Value: " + WebsiteSecurity + ", Website Type: " + Website.Split(':')[2] + ")",
                "Hack Method: " + "Cross Site Scripting",
                "Complexity: " + names[Array.IndexOf(vals, BaseValue)] + " (" + BaseValue + ")",
                "Hack Type: " + hackTypes[Array.IndexOf(multis, Convert.ToSingle(AdditionalInformation[0]))] + " (" + AdditionalInformation[0].ToString() + ")",
                "Programs: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? "Successful" : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            }
        :
            new string[]
            {
                "Site: " + Website.Split(':')[0],
                "Hack Method: " + "XSS",
                "Complexity: " + names[Array.IndexOf(vals, BaseValue)],
                "Hack Type: " + hackTypes[Array.IndexOf(multis, Convert.ToSingle(AdditionalInformation[0]))],
                "Programs: " + AdditionalInformation[1].ToString(),
                HackSuccessful ? "Successful" : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            };
    }

    public IEnumerator CycleHack(float cycleDelay, TextMesh displayMesh, bool glitched)
    {
        animating = true;
        string temp = displayMesh.text;
        for (int i = 0; i < GetDisplayValues(false).Length; i++)
        {
            if (glitched)
            {
                List<int> chosen = new List<int>();
                char[] text = GetDisplayValues(false)[i].ToArray();
                for (int x = 0; x < rnd.Range(2, GetDisplayValues(false)[i].Length + 1); x++)
                {
                    int c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    while (chosen.Contains(c)) c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    text[c] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                }
                displayMesh.text = text.Join("");
            }
            else displayMesh.text = GetDisplayValues(false)[i];
            yield return new WaitForSeconds(cycleDelay);
        }
        displayMesh.text = temp;
        animating = false;
    }

}

public class BruteForceAttempt : IHack
{

    string website;
    public string Website
    {
        get { return website; }
        set { website = value; }
    }

    int siteSecurity;
    public int WebsiteSecurity
    {
        get { return siteSecurity; }
        set { siteSecurity = value; }
    }

    float baseValue;
    public float BaseValue
    {
        get { return baseValue; }
        set { baseValue = value; }
    }

    object[] additionalInformation;
    public object[] AdditionalInformation
    {
        get { return additionalInformation; }
        set { additionalInformation = value; }
    }

    bool hackSuccessful;
    public bool HackSuccessful
    {
        get { return hackSuccessful; }
        set { hackSuccessful = value; }
    }

    float subtotal;
    public float SubtotalPrice
    {
        get { return subtotal; }
        set { subtotal = value; }
    }

    float discount;
    public float Discount
    {
        get { return discount; }
        set { discount = value; }
    }

    float total;
    public float TotalPrice
    {
        get { return total; }
        set { total = value; }
    }

    bool hostInflitrated;
    public bool HostInflitrated
    {
        get { return hostInflitrated; }
        set { hostInflitrated = value; }
    }

    bool animating;
    public bool Animating
    {
        get { return animating; }
        set { animating = value; }
    }

    float failedPercentage;

    float[] vals = new float[] { 2.2f, 1.6f, 1.9f };
    string[] names = new string[] { "Str. Inject", "Sneak", "Duplication" };

    public BruteForceAttempt(string Website, int WebsiteSecurity, float BaseValue, float Discount, object[] AdditionalInformation, bool HackSuccessful)
    {
        this.Website = Website;
        this.WebsiteSecurity = WebsiteSecurity;
        this.BaseValue = BaseValue;
        this.Discount = Discount;
        this.AdditionalInformation = AdditionalInformation;
        this.HackSuccessful = HackSuccessful;
        GenerateHackPrice();
    }

    public float GenerateHackPrice()
    {
        SubtotalPrice = BaseValue * Convert.ToSingle(AdditionalInformation[0]) * WebsiteSecurity / 5f;
        SubtotalPrice = (float)Math.Round(SubtotalPrice * (HackSuccessful ? ((HostInflitrated = Convert.ToBoolean(rnd.Range(0, 2))) ? 1.4f : 1.2f) : (failedPercentage = Mathf.Round(rnd.Range(1.0f, 100.0f)) / 100f)), 3);
        return TotalPrice = (float)Math.Round(SubtotalPrice * Discount, 3);
    }

    public string[] GetDisplayValues(bool logging)
    {
        return logging ?
            new string[]
            {
                "Site: " + Website.Split(':')[0] + " (Security Value: " + WebsiteSecurity + ", Website Type: " + Website.Split(':')[2] + ")",
                "Hack Method: " + "Brute Force Attempt",
                "Attack Type: " + names[Array.IndexOf(vals, BaseValue)] + " (" + BaseValue + ")",
                "Attempts: " + AdditionalInformation[0].ToString(),
                HackSuccessful ? new string[] { "Result: Crashed Permanently", "Result: Host Infiltrated" }[Convert.ToInt32(HostInflitrated)] : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            }
        : 
            new string[]
            {
                "Site: " + Website.Split(':')[0],
                "Hack Method: " + "BFA",
                "Att. Type: " + names[Array.IndexOf(vals, BaseValue)],
                "Attempts: " + AdditionalInformation[0].ToString(),
                HackSuccessful ? new string[] { "Result: Crashed Permanently", "Result: Host Infiltrated" }[Convert.ToInt32(HostInflitrated)] : "Failed: " + (failedPercentage * 100f).ToString() + "%"
            };
    }

    public IEnumerator CycleHack(float cycleDelay, TextMesh displayMesh, bool glitched)
    {
        animating = true;
        string temp = displayMesh.text;
        for (int i = 0; i < GetDisplayValues(false).Length; i++)
        {
            if (glitched) 
            {
                List<int> chosen = new List<int>();
                char[] text = GetDisplayValues(false)[i].ToArray();
                for (int x = 0; x < rnd.Range(2, GetDisplayValues(false)[i].Length+1); x++) 
                {
                    int c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    while (chosen.Contains(c)) c = rnd.Range(0, GetDisplayValues(false)[i].Length);
                    text[c] = "!@#$%^&*()[]{};',./\\\"".PickRandom();
                }
                displayMesh.text = text.Join("");
            }
            else displayMesh.text = GetDisplayValues(false)[i];
            yield return new WaitForSeconds(cycleDelay);
        }
        displayMesh.text = temp;
        animating = false;
    }

}

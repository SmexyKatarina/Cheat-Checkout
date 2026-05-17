using System;
using System.Linq;
using System.Collections.Generic;
using rnd = UnityEngine.Random;
using UnityEngine;

/// <summary>
/// Possible hack types
/// </summary>
public enum HackType
{
    DSA = 0,
    WORM = 1,
    CI = 2,
    XSS = 3,
    BFA = 4
}

public interface IHack
{
    /// <summary>
    /// The current weekday
    /// </summary>
    DayOfWeek Weekday { get; }
    /// <summary>
    /// The chosen crypto
    /// </summary>
    Crypto Crypto { get; }
    /// <summary>
    /// The website attached to the hack
    /// </summary>
    Website Website { get; }
    /// <summary>
    /// The values that are generated for calculations
    /// </summary>
    object[] Values { get; }
    /// <summary>
    /// The success value percentage. 0.0 - 0.99 if the hack failed, 1 if it passed, and perhaps a different success value depending on the hack. 
    /// </summary>
    double SuccessValue { get; }

    /// <summary>
    /// The number after doing the base calculations, rounded to 3 decimal places.
    /// </summary>
    double Subtotal { get; }
    /// <summary>
    /// The discount amount based on the current weekday
    /// </summary>
    double Discount { get; }
    /// <summary>
    /// The total after applying the discount, rounded to 3 decimal places.
    /// </summary>
    double Total { get; }
    /// <summary>
    /// The crypto total, rounded to 3 decimal places
    /// </summary>
    double CryptoTotal { get; }

    /// <summary>
    /// Log the hack information.
    /// </summary>
    /// <returns>An array of formatted strings for the logging</returns>
    string[] GetLogInfo(int moduleId, int hackIndex);

    /// <summary>
    /// Get the values that are shown on the LCD Display
    /// </summary>
    /// <returns>An array of strings for the cycling</returns>
    string[] GetDisplayValues();
}

/// <summary>
/// The DSA hack type
/// 
/// Base Value * PCs Used * (Website Security Value / 5) * Duration
/// 
/// Base Values possible: 0.8, 1.2, 1.6, 2
/// PCs Used: RNG 5 - 20.
/// Duration: RNG 1.0 - 3.0
/// 
/// If successful (RNG, 0-1, 0 fails, 1 passes), the value can either be 1x or 1.25x, RNG of 0-1, 0 = 1x and 1 = 1.25x
/// </summary>
public struct DSAHack : IHack
{
    public DayOfWeek Weekday { get; private set;}
    public Crypto Crypto { get; private set; }
    public Website Website { get; private set; }
    public object[] Values { get; private set; }
    public double SuccessValue { get; private set; }

    public double Subtotal { get; private set; }
    public double Discount { get; private set; }
    public double Total { get; private set; }

    public double CryptoTotal { get; private set; }

    public DSAHack(Website website, DayOfWeek weekday, Crypto crypto)
    {
        Website = website;
        Weekday = weekday;
        Crypto = crypto;
        Values = new object[] { rnd.Range(0, 4), rnd.Range(5, 21), Math.Round(rnd.Range(1.0f, 3.0f), 1), rnd.Range(0, 2) };
        SuccessValue = rnd.Range(0, 2) == 0 ? (double)Math.Min(Math.Round(rnd.Range(0.0f, 0.99f), 2), 0.99f) : new double[] { 1d, 1.25d }[(int)Values.Last()];
        Subtotal = Math.Round(
            new double[] { 0.8d, 1.2d, 1.6d, 2d }[(int)Values[0]] *
            Convert.ToDouble(Values[1]) *
            (Website.Security / 5d) *
            Convert.ToDouble(Values[2]) *
            SuccessValue, 3);
        Discount = Website.GetDayDiscount(Weekday);
        Total = Math.Round(Subtotal * Discount, 3);
        CryptoTotal = Math.Round(Total / Crypto.Price, 3);
    }

    public string[] GetDisplayValues()
    {
        return new string[]
        {
            string.Format("Site: {0}", Website.Url),
            "Method: DSA",
            string.Format("PC-Type: {0}", new string[] { "Basic", "Advanced", "Super", "Quantum" }[(int)Values[0]]),
            string.Format("PCs Used: {0}", Values[1]),
            string.Format("Duration: {0}", Values[2]),
            SuccessValue >= 1 ? new string[] { "Success: Crash Temp", "Success: Crash Perm" }[(int)Values[3]] : string.Format("Failed: {0}%", Math.Round(SuccessValue * 100))
        };
    }

    public string[] GetLogInfo(int moduleId, int hackIndex)
    {
        return new string[]
        {
            string.Format(
                "[Cheat Checkout #{0}] The information for hack #{1}:", 
                moduleId, 
                hackIndex
            ),
            string.Format(
                "[Cheat Checkout #{0}] Performing a Denial of Service hack on website '{1}'", 
                moduleId, 
                Website.Url
            ),
            string.Format(
                "[Cheat Checkout #{0}] The computers used for the hack were: {1} (Base Value: {2})", 
                moduleId, 
                new string[] { "Basic PCs", "Advanced PCs", "Supercomputers", "Quantum Computers" }[(int)Values[0]],
                new double[] { 0.8d, 1.2d, 1.6d, 2d }[(int)Values[0]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The number of PCs used were: {1} (PCs used: {2})",
                moduleId,
                Values[1],
                Values[1]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack took {1} hours to finish. (Duration: {2})",
                moduleId,
                Values[2],
                Values[2]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack was {1}{2}(Success rate: {3})",
                moduleId,
                SuccessValue >= 1.0d ? "Successful" : "Unsuccessful",
                SuccessValue >= 1.0d ? string.Format(" and the hack caused the website to crash {0}. ", ((int)Values[3]) == 0 ? "temporarily" : "permanently" ) : string.Format(" and it was {0}% complete. ", Math.Round(SuccessValue * 100)),
                SuccessValue
            ),
            string.Format(
                "[Cheat Checkout #{0}] The calculation to perform: Base Value * PCs Used * (Website Security Value / 5) * Duration",
                moduleId
            ),
            string.Format(
                "[Cheat Checkout #{0}] The discount applied for {1}: {2}",
                moduleId,
                Weekday,
                Discount
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the calculations are: ({1} * {2} * ({3} / 5) * {4}) * {5}, rounded to 3 decimal places, is {6}",
                moduleId,
                new double[] { 0.8d, 1.2d, 1.6d, 2d }[(int)Values[0]],
                Values[1],
                Website.Security,
                Values[2],
                SuccessValue,
                Subtotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] {1} * {2}, rounded to 3 decimal places is: {3}",
                moduleId,
                Subtotal,
                Discount,
                Total
            ),
            string.Format(
                "[Cheat Checkout #{0}] Finally, converting the total price of the hack {1} into {2}: {3} / {4}, rounded to 3 decimal places, is {5}",
                moduleId,
                Total,
                Crypto.Name,
                Total,
                Crypto.Price,
                CryptoTotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the total crypto price for this hack is: {1}",
                moduleId,
                CryptoTotal
            )
        };
    }
}

/// <summary>
/// The Worm hack type
/// 
/// Base Value * Infected PCs * (Website Security Value / 10) * Multiplier
/// 
/// Base Values possible: 0.5, 0.9, 1.3, 1.75, 2.1
/// Infected PCs: RNG 5 - 20
/// Multipliers possible: 1, 2, 0.5
/// 
/// Normal success value (RNG, 0-1, 0 fails, 1 passes).
/// </summary>
public struct WormHack : IHack
{
    public DayOfWeek Weekday { get; private set;}
    public Crypto Crypto { get; private set; }
    public Website Website { get; private set; }
    public object[] Values { get; private set; }
    public double SuccessValue { get; private set; }

    public double Subtotal { get; private set; }
    public double Discount { get; private set; }
    public double Total { get; private set; }

    public double CryptoTotal { get; private set; }

    public WormHack(Website website, DayOfWeek weekday, Crypto crypto)
    {
        Weekday = weekday;
        Crypto = crypto;
        Website = website;
        Values = new object[] { rnd.Range(0, 5), rnd.Range(5, 21), rnd.Range(0, 3) };
        SuccessValue = rnd.Range(0, 2) == 0 ? (double)Math.Min(Math.Round(rnd.Range(0.0f, 0.99f), 2), 0.99f) : 1d;
        Subtotal = Math.Round(
            new double[] { 0.5d, 0.9d, 1.3d, 1.75d, 2.1d }[(int)Values[0]] *
            Convert.ToDouble(Values[1]) *
            (Website.Security / 10d) *
            new double[] { 1d, 2d, 0.5d }[(int)Values[2]] *
            SuccessValue, 3);
        Discount = Website.GetDayDiscount(Weekday);
        Total = Math.Round(Subtotal * Discount, 3);
        CryptoTotal = Math.Round(Total / Crypto.Price, 3);
    }

    public string[] GetDisplayValues()
    {
        return new string[]
        {
            string.Format("Site: {0}", Website.Url),
            "Method: Worm",
            string.Format("PC-Type: {0}", new string[] { "Defective", "Basic", "Advanced ", "Super", "Quantum" }[(int)Values[0]]),
            string.Format("Type: {0}", new string[] { "Normal", "Lethal", "Spreader" }[(int)Values[2]]),
            string.Format("Infected PCs: {0}", Values[1]),
            SuccessValue >= 1 ? "Hack Successful" : string.Format("Failed: {0}%", Math.Round(SuccessValue * 100))
        };
    }

    public string[] GetLogInfo(int moduleId, int hackIndex)
    {
        return new string[]
        {
            string.Format(
                "[Cheat Checkout #{0}] The information for hack #{1}:",
                moduleId,
                hackIndex
            ),
            string.Format(
                "[Cheat Checkout #{0}] Performing a Worm hack on website '{1}'",
                moduleId,
                Website.Url
            ),
            string.Format(
                "[Cheat Checkout #{0}] The computers that can be infected from this attack: {1} (Base Value: {2})",
                moduleId,
                new string[] { "Defective PCs", "Basic PCs", "Advanced PCs", "Supercomputers", "Quantum Computers" }[(int)Values[0]],
                new double[] { 0.5d, 0.9d, 1.3d, 1.75d, 2.1d }[(int)Values[0]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The number of PCs that were infected: {1} (Infected PCs: {2})",
                moduleId,
                Values[1],
                Values[1]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The type of worm used was {1}. (Multiplier: {2})",
                moduleId,
                new string[] { "Normal", "Lethal", "Spreader" }[(int)Values[2]],
                new double[] { 1d, 2d, 0.5d }[(int)Values[2]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack was {1}{2}(Success rate: {3})",
                moduleId,
                SuccessValue >= 1.0d ? "Successful" : "Unsuccessful",
                SuccessValue >= 1.0d ? " " : string.Format(" and it was {0}% complete. ", Math.Round(SuccessValue * 100)),
                SuccessValue
            ),
            string.Format(
                "[Cheat Checkout #{0}] The calculation to perform: Base Value * Infected PCs * (Website Security Value / 10) * Multiplier",
                moduleId
            ),
            string.Format(
                "[Cheat Checkout #{0}] The discount applied for {1}: {2}",
                moduleId,
                Weekday,
                Discount
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the calculations are: ({1} * {2} * ({3} / 10) * {4}) * {5}, rounded to 3 decimal places, is {6}",
                moduleId,
                new double[] { 0.5d, 0.9d, 1.3d, 1.75d, 2.1d }[(int)Values[0]],
                Values[1],
                Website.Security,
                Values[2],
                SuccessValue,
                Subtotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] {1} * {2}, rounded to 3 decimal places is: {3}",
                moduleId,
                Subtotal,
                Discount,
                Total
            ),
            string.Format(
                "[Cheat Checkout #{0}] Finally, converting the total price of the hack {1} into {2}: {3} / {4}, rounded to 3 decimal places, is {5}",
                moduleId,
                Total,
                Crypto.Name,
                Total,
                Crypto.Price,
                CryptoTotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the total crypto price for this hack is: {1}",
                moduleId,
                CryptoTotal
            )
        };
    }
}

/// <summary>
/// The Code Injection hack type
/// 
/// Base Value * Complexity Multiplier * Batches * (Website Security Value / 20)
/// 
/// Base Values possible: 0.9, 1.8, 1.25, 2.2
/// Complexity Multipliers possible: 1, 1.2, 1.5
/// Batches: RNG 5 - 20
/// 
/// If successful (RNG, 0-1, 0 fails, 1 passes), the value can either be 1.25x or 1.5x, RNG of 0-1, 0 = 1.25x and 1 = 1.5x
/// </summary>
public struct CIHack : IHack
{
    public DayOfWeek Weekday { get; private set;}
    public Crypto Crypto { get; private set; }
    public Website Website { get; private set; }
    public object[] Values { get; private set; }
    public double SuccessValue { get; private set; }

    public double Subtotal { get; private set; }
    public double Discount { get; private set; }
    public double Total { get; private set; }

    public double CryptoTotal { get; private set; }

    public CIHack(Website website, DayOfWeek weekday, Crypto crypto)
    {
        Weekday = weekday;
        Crypto = crypto;
        Website = website;
        Values = new object[] { rnd.Range(0, 4), rnd.Range(0, 3), rnd.Range(5, 21), rnd.Range(0, 2) };
        SuccessValue = rnd.Range(0, 2) == 0 ? (double)Math.Min(Math.Round(rnd.Range(0.0f, 0.99f), 2), 0.99f) : new double[] { 1.25d, 1.5d }[(int)Values.Last()];
        Subtotal = Math.Round(
            new double[] { 0.9d, 1.8d, 1.25d, 2.2d }[(int)Values[0]] *
            new double[] { 1d, 1.2d, 1.5d }[(int)Values[1]] *
            Convert.ToDouble(Values[2]) *
            (Website.Security / 20f) *
            SuccessValue, 3);
        Discount = Website.GetDayDiscount(Weekday);
        Total = Math.Round(Subtotal * Discount, 3);
        CryptoTotal = Math.Round(Total / Crypto.Price, 3);
    }

    public string[] GetDisplayValues()
    {
        return new string[]
        {
            string.Format("Site: {0}", Website.Url),
            "Method: Code Inj.",
            string.Format("Vulner.: {0}", new string[] { "SQL", "LDAP", "XPath", "NoSQL" }[(int)Values[0]]),
            string.Format("Complex.: {0}", new string[] { "Simple", "Advanced", "Complex" }[(int)Values[1]]),
            string.Format("Batches: {0}", Values[2]),
            SuccessValue >= 1 ? new string[] { "Success: Crash Perm", "Success: Host Infl" }[(int)Values[3]] : string.Format("Failed: {0}%", Math.Round(SuccessValue * 100))
        };
    }

    public string[] GetLogInfo(int moduleId, int hackIndex)
    {
        return new string[]
        {
            string.Format(
                "[Cheat Checkout #{0}] The information for hack #{1}:",
                moduleId,
                hackIndex
            ),
            string.Format(
                "[Cheat Checkout #{0}] Performing a Code Injection hack on website '{1}'",
                moduleId,
                Website.Url
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack is exploiting a vulnerability with: {1} (Base Value: {2})",
                moduleId,
                new string[] { "SQL", "LDAP", "XPath", "NoSQL" }[(int)Values[0]],
                new double[] { 0.9d, 1.8d, 1.25d, 2.2d }[(int)Values[0]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The complexity of this attack on this website is {1}. (Complexity Multiplier: {2})",
                moduleId,
                new string[] { "simple", "advanced", "complex" }[(int)Values[1]],
                new double[] { 1d, 1.2d, 1.5d }[(int)Values[1]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] They did a total of {1} batches of attacks. (Batches: {2})",
                moduleId,
                Values[2],
                Values[2]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack was {1}{2}(Success rate: {3})",
                moduleId,
                SuccessValue >= 1.0d ? "Successful" : "Unsuccessful",
                SuccessValue >= 1.0d ? string.Format(" and the hack {0} ", ((int)Values[3]) == 0 ? "caused the website to crash permanently" : "granted host access to the website." ) : string.Format(" and it was {0}% complete. ", Math.Round(SuccessValue * 100)),
                SuccessValue
            ),
            string.Format(
                "[Cheat Checkout #{0}] The calculation to perform: Base Value * Complexity Multiplier * Batches * (Website Security Value / 20)",
                moduleId
            ),
            string.Format(
                "[Cheat Checkout #{0}] The discount applied for {1}: {2}",
                moduleId,
                Weekday,
                Discount
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the calculations are: ({1} * {2} * {3} * ({4} / 20)) * {5}, rounded to 3 decimal places, is {6}",
                moduleId,
                new double[] { 0.9d, 1.8d, 1.25d, 2.2d }[(int)Values[0]],
                new double[] { 1d, 1.2d, 1.5d }[(int)Values[1]],
                Values[2],
                Website.Security,
                SuccessValue,
                Subtotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] {1} * {2}, rounded to 3 decimal places is: {3}",
                moduleId,
                Subtotal,
                Discount,
                Total
            ),
            string.Format(
                "[Cheat Checkout #{0}] Finally, converting the total price of the hack {1} into {2}: {3} / {4}, rounded to 3 decimal places, is {5}",
                moduleId,
                Total,
                Crypto.Name,
                Total,
                Crypto.Price,
                CryptoTotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the total crypto price for this hack is: {1}",
                moduleId,
                CryptoTotal
            )
        };
    }
}

/// <summary>
/// The Cross-site Scripting hack type
/// 
/// Base Value * Multiplier * (Website Security Value / 8) * (Programs / 2)
/// 
/// Base Values possible: 0.5, 1, 1.5, 2, 2.5
/// Multipliers possible: 1, 1.25, 1.5
/// Programs: RNG 10-40
/// 
/// Normal success value (RNG, 0-1, 0 fails, 1 passes).
/// </summary>
public struct XSSHack : IHack
{
    public DayOfWeek Weekday { get; private set; }
    public Crypto Crypto { get; private set; }
    public Website Website { get; private set; }
    public object[] Values { get; private set; }
    public double SuccessValue { get; private set; }

    public double Subtotal { get; private set; }
    public double Discount { get; private set; }
    public double Total { get; private set; }

    public double CryptoTotal { get; private set; }

    public XSSHack(Website website, DayOfWeek weekday, Crypto crypto)
    {
        Weekday = weekday;
        Crypto = crypto;
        Website = website;
        Values = new object[] { rnd.Range(0, 5), rnd.Range(0, 3), rnd.Range(10, 41) };
        SuccessValue = rnd.Range(0, 2) == 0 ? (double)Math.Min(Math.Round(rnd.Range(0.0f, 0.99f), 2), 0.99f) : 1d;
        Subtotal = Math.Round(
            new double[] { 0.5d, 1d, 1.5d, 2d, 2.5d }[(int)Values[0]] *
            new double[] { 1d, 1.25d, 1.5d }[(int)Values[1]] *
            (Website.Security / 8d) *
            (Convert.ToDouble(Values[2]) / 2d) *
            SuccessValue, 3);
        Discount = Website.GetDayDiscount(Weekday);
        Total = Math.Round(Subtotal * Discount, 3);
        CryptoTotal = Math.Round(Total / Crypto.Price, 3);
    }

    public string[] GetDisplayValues()
    {
        return new string[]
        {
            string.Format("Site: {0}", Website.Url),
            "Method: Cross-Site",
            string.Format("Complex.: {0}", new string[] { "Ext. Basic", "Basic", "Advanced", "Complex", "Unintell." }[(int)Values[0]]),
            string.Format("Type: {0}", new string[] { "Non-Persist", "Persist", "Mutated" }[(int)Values[1]]),
            string.Format("Programs: {0}", Values[2]),
            SuccessValue >= 1 ? "Hack Successful" : string.Format("Failed: {0}%", Math.Round(SuccessValue * 100))
        };
    }

    public string[] GetLogInfo(int moduleId, int hackIndex)
    {
        return new string[]
        {
            string.Format(
                "[Cheat Checkout #{0}] The information for hack #{1}:",
                moduleId,
                hackIndex
            ),
            string.Format(
                "[Cheat Checkout #{0}] Performing a Cross-Site Scripting hack on website '{1}'",
                moduleId,
                Website.Url
            ),
            string.Format(
                "[Cheat Checkout #{0}] The complexity of this attack on this website is {1}. (Base Value: {2})",
                moduleId,
                new string[] { "extremely basic", "basic", "advanced", "complex", "unintelligible" }[(int)Values[0]],
                new double[] { 0.5d, 1d, 1.5d, 2d, 2.5d }[(int)Values[0]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The type of scripting they are going with is {1}. (Multiplier: {2})",
                moduleId,
                new string[] { "non-persistent", "persistent", "mutated XSS" }[(int)Values[1]],
                new double[] { 1d, 1.25d, 1.5d }[(int)Values[1]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The number of programs/scripts being sent is: {1}. (Programs: {2})",
                moduleId,
                Values[2],
                Values[2]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack was {1}{2}(Success rate: {3})",
                moduleId,
                SuccessValue >= 1.0d ? "Successful" : "Unsuccessful",
                SuccessValue >= 1.0d ? " " : string.Format(" and it was {0}% complete. ", Math.Round(SuccessValue * 100)),
                SuccessValue
            ),
            string.Format(
                "[Cheat Checkout #{0}] The calculation to perform: Base Value * Multiplier * (Website Security Value / 8) * (Programs / 2)",
                moduleId
            ),
            string.Format(
                "[Cheat Checkout #{0}] The discount applied for {1}: {2}",
                moduleId,
                Weekday,
                Discount
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the calculations are: ({1} * {2} * ({3} / 8) * ({4} / 2)) * {5}, rounded to 3 decimal places, is {6}",
                moduleId,
                new double[] { 0.5d, 1d, 1.5d, 2d, 2.5d }[(int)Values[0]],
                new double[] { 1d, 1.25d, 1.5d }[(int)Values[1]],
                Website.Security,
                Values[2],
                SuccessValue,
                Subtotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] {1} * {2}, rounded to 3 decimal places is: {3}",
                moduleId,
                Subtotal,
                Discount,
                Total
            ),
            string.Format(
                "[Cheat Checkout #{0}] Finally, converting the total price of the hack {1} into {2}: {3} / {4}, rounded to 3 decimal places, is {5}",
                moduleId,
                Total,
                Crypto.Name,
                Total,
                Crypto.Price,
                CryptoTotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the total crypto price for this hack is: {1}",
                moduleId,
                CryptoTotal
            )
        };
    }
}

/// <summary>
/// The Brute Force Attempt hack type
/// 
/// (Base Value * Attempts * Website Security Value) / 5
/// 
/// Base Values possible: 2.2, 1.6, 1.9
/// Attempts: RNG 5 - 20
/// 
/// If successful (RNG, 0-1, 0 fails, 1 passes), the value can either be 1.2x or 1.4x, RNG of 0-1, 0 = 1.2x and 1 = 1.4x
/// </summary>
public struct BFAHack : IHack
{
    public DayOfWeek Weekday { get; private set; }
    public Crypto Crypto { get; private set; }
    public Website Website { get; private set; }
    public object[] Values { get; private set; }
    public double SuccessValue { get; private set; }

    public double Subtotal { get; private set; }
    public double Discount { get; private set; }
    public double Total { get; private set; }

    public double CryptoTotal { get; private set; }

    public BFAHack(Website website, DayOfWeek weekday, Crypto crypto)
    {
        Weekday = weekday;
        Crypto = crypto;
        Website = website;
        Values = new object[] { rnd.Range(0, 3), rnd.Range(5, 21), rnd.Range(0, 2) };
        SuccessValue = rnd.Range(0, 2) == 0 ? (double)Math.Min(Math.Round(rnd.Range(0.0f, 0.99f), 2), 0.99f) : new double[] { 1.2d, 1.4d }[(int)Values.Last()];
        Subtotal = Math.Round(
            new double[] { 2.2d, 1.6d, 1.9 }[(int)Values[0]] *
            Convert.ToDouble(Values[1]) *
            Website.Security /
            5d *
            SuccessValue, 3);
        Discount = Website.GetDayDiscount(Weekday);
        Total = Math.Round(Subtotal * Discount, 3);
        CryptoTotal = Math.Round(Total / Crypto.Price, 3);
    }

    public string[] GetDisplayValues()
    {
        return new string[]
        {
            string.Format("Site: {0}", Website.Url),
            "Method: BFA",
            string.Format("Attack: {0}", new string[] { "Strong Inj.", "Sneak", "Duplication" }[(int)Values[0]]),
            string.Format("Attempts: {0}", Values[1]),
            SuccessValue >= 1 ? new string[] { "Success: Crash Perm", "Success: Host Infl" }[(int)Values[2]] : string.Format("Failed: {0}%", Math.Round(SuccessValue * 100))
        };
    }

    public string[] GetLogInfo(int moduleId, int hackIndex)
    {
        return new string[]
        {
            string.Format(
                "[Cheat Checkout #{0}] The information for hack #{1}:",
                moduleId,
                hackIndex
            ),
            string.Format(
                "[Cheat Checkout #{0}] Performing a Brute Force Attempt hack on website '{1}'",
                moduleId,
                Website.Url
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack is using a {1} attack. (Base Value: {2})",
                moduleId,
                new string[] { "Strong Inject", "Sneak", "Duplication" }[(int)Values[0]],
                new double[] { 2.2d, 1.6d, 1.9 }[(int)Values[0]]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hackers did {1} attempts at attacking. (Attempts: {2})",
                moduleId,
                Values[1],
                Values[1]
            ),
            string.Format(
                "[Cheat Checkout #{0}] The hack was {1}{2}(Success rate: {3})",
                moduleId,
                SuccessValue >= 1.0d ? "Successful" : "Unsuccessful",
                SuccessValue >= 1.0d ? string.Format(" and the hack {0} ", ((int)Values[2]) == 0 ? "caused the website to crash permanently" : "granted host access to the website." ) : string.Format(" and it was {0}% complete. ", Math.Round(SuccessValue * 100)),
                SuccessValue
            ),
            string.Format(
                "[Cheat Checkout #{0}] The calculation to perform: (Base Value * Attempts * Website Security Value) / 5",
                moduleId
            ),
            string.Format(
                "[Cheat Checkout #{0}] The discount applied for {1}: {2}",
                moduleId,
                Weekday,
                Discount
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the calculations are: (({1} * {2} * {3}) / 5) * {4}, rounded to 3 decimal places, is {5}",
                moduleId,
                new double[] { 2.2d, 1.6d, 1.9 }[(int)Values[0]],
                Values[1],
                Website.Security,
                SuccessValue,
                Subtotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] {1} * {2}, rounded to 3 decimal places is: {3}",
                moduleId,
                Subtotal,
                Discount,
                Total
            ),
            string.Format(
                "[Cheat Checkout #{0}] Finally, converting the total price of the hack {1} into {2}: {3} / {4}, rounded to 3 decimal places, is {5}",
                moduleId,
                Total,
                Crypto.Name,
                Total,
                Crypto.Price,
                CryptoTotal
            ),
            string.Format(
                "[Cheat Checkout #{0}] Therefore, the total crypto price for this hack is: {1}",
                moduleId,
                CryptoTotal
            )
        };
    }
}

/// <summary>
/// The controller for generating hacks.
/// </summary>
public class HackGenerator
{
    private List<IHack> _hacks = new List<IHack>();

    /// <summary>
    /// Generate a hack
    /// </summary>
    /// <param name="website">The website that this hack uses</param>
    /// <param name="weekday">The weekday for discounts</param>
    /// <param name="crypto">The crypto chosen</param>
    /// <param name="hackType">The type of hack to generate</param>
    /// <returns>The hack that was generated</returns>
    /// <exception cref="ArgumentException">If the hack type does not exist.</exception>
    public IHack Generate(Website website, DayOfWeek weekday, Crypto crypto, HackType hackType)
    {
        switch (hackType)
        {
            case HackType.DSA:
                _hacks.Add(new DSAHack(website, weekday, crypto));
                break;
            case HackType.WORM:
                _hacks.Add(new WormHack(website, weekday, crypto));
                break;
            case HackType.CI:
                _hacks.Add(new CIHack(website, weekday, crypto));
                break;
            case HackType.XSS:
                _hacks.Add(new XSSHack(website, weekday, crypto));
                break;
            case HackType.BFA:
                _hacks.Add(new BFAHack(website, weekday, crypto));
                break;
            default:
                throw new ArgumentException("Unknown hack type provided");
        }
        return _hacks.Last();
    }

    /// <summary>
    /// Get all the generated hacks
    /// </summary>
    /// <returns>The list of generated hacks</returns>
    public List<IHack> GetHacks()
    {
        return _hacks;
    }

    /// <summary>
    /// Get the sum of the totals of the hacks generated
    /// </summary>
    /// <returns>The sum of the totals of generated hacks</returns>
    public double GetHackTotals()
    {
        return _hacks.Sum(x => x.Total);
    }

    /// <summary>
    /// Get the sum of the crypto converted totals
    /// </summary>
    /// <returns>The sum crypto totals of the hacks</returns>
    public double GetHackCryptoTotals()
    {
        return _hacks.Sum(x => x.CryptoTotal);
    }
}
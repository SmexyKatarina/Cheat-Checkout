using UnityEngine;
using System;
using System.Collections;
using rnd = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;

public class Timers
{

    private static readonly string[] _hackerStrings = new string[] {
        "Are you stupid? You think\nyou can get out of this?",
        "HAHAHA\nYour computer is mine",
        "Use a better VPN\nnext time",
        "You call that a firewall?\nCute...",
        "Your mom's recipe\nis mine too",
        "FBI? More lik\nFBI open up. JK it's me",
        "Sending your browser\nhistory to your boss",
        "Err 404\nEscape not found",
        "I am in your walls",
        "Have you tried\nturning yourself off?",
        "Your antivirus said\nYOLO and left",
        "Deleting System32\njust kidding ...or am I?",
        "Uploading to the\ndark web please wait...",
    };

    /// <summary>
    /// The module instance
    /// </summary>
    private CheatCheckoutV3 _instance;

    /// <summary>
    /// A timer controller
    /// </summary>
    /// <param name="instance">The module instance</param>
    public Timers(CheatCheckoutV3 instance)
    {
        _instance = instance;
    }

    /// <summary>
    /// The main timer for the module
    /// </summary>
    /// <param name="actionDelay">The delay between actions counting down</param>
    /// <param name="initialDelay">The initial delay on module start/restart to allow for time at the start to see the module</param>
    /// <returns>A coroutine</returns>
    public IEnumerator ModuleCycle(float actionDelay, float initialDelay)
    {
        // Wait for the initial delay to allow for no status changes right away to prevent issues at the beginning
        yield return new WaitForSeconds(initialDelay);
        while (true)
        {
            // Choose random type to affect (0 = Wifi, 1 = VPN);
            int affectedType = rnd.Range(0, 2);
            if (affectedType == 0)
            {
                _instance.UpdateWifiStatus(Math.Max(_instance._wifiStatus - 1, 0));
            }
            else
            {
                _instance.UpdateVPNStatus(Math.Max(_instance._vpnStatus - 1, 0));
            }
            yield return new WaitForSeconds(actionDelay);
        }
    }

    /// <summary>
    /// The cycle timer for when the VPN reaches level 1 (yellow). This handles glitching the buttons.
    /// </summary>
    /// <param name="cycleDelay">The delay in between each glitch cycle</param>
    /// <param name="glitchDuration">The duration of how long the glitch lasts</param>
    /// <param name="numberOfButtons">The number of buttons to affect</param>
    /// <returns>A coroutine</returns>
    public IEnumerator VPN_GlitchCycle(float cycleDelay, float glitchDuration, int numberOfButtons)
    {
        // Wait for initial delay so that they can register the change
        yield return new WaitForSeconds(cycleDelay);
        while (_instance._vpnStatus != 2)
        {
            // Create a new list of posiitons
            List<int> positions = new List<int>();
            // Generate the positions that will be affected and ensure that all of them are unique
            for (int i = 0; i < numberOfButtons; i++)
            {
                int generated = rnd.Range(0, _instance._allButtons.Length);
                while (positions.Contains(generated))
                    generated = rnd.Range(0, _instance._allButtons.Length);
                positions.Add(generated);
            }
            // Pass it back to the module
            _instance._glitchedButtons = positions.ToArray();
            // Allow it to be glitched for a certain duration
            yield return new WaitForSeconds(glitchDuration);
            // Then return them back to normal
            foreach (MeshRenderer mr in _instance._allButtons.Select(x => x.gameObject.GetComponent<MeshRenderer>()))
                mr.materials[0].color = new Color(0, 0, 0, 255);
            _instance._glitchedButtons = new int[] { };
            _instance.FixTexts();
            // And wait for x time before doing it again.
            yield return new WaitForSeconds(cycleDelay);
        }
        yield break;
    }

    /// <summary>
    /// The hack timer
    /// </summary>
    /// <param name="hackDuration">The duration this hack lasts for</param>
    /// <param name="LCD_Text">The LCD text to update for the timer</param>
    /// <returns>A coroutine</returns>
    public IEnumerator InitiateHack(float hackDuration, TextMesh LCD_Text)
    {
        float timer = hackDuration;
        foreach (TextMesh tm in _instance._allTexts.Where(x => !x.name.Contains("PATCH")))
        {
            tm.text = "!!!";
            tm.color = new Color32(255, 0, 0, 255);
        }
        while (timer-- != 0)
        {
            LCD_Text.text = timer % 2 == 0 ? "!!! " + timer + " !!!" : timer.ToString();
            yield return new WaitForSeconds(1f);
        }
        // Soft check to ensure that a hack is still possible, in any reason that the coroutine is not stopped successfully.
        if (_instance._vpnStatus == 0)
        {
            _instance.HackModule();
        }
        yield break;
    }

    public IEnumerator HackerText(float cycleDuration, TextMesh Customer_Text)
    {
        Customer_Text.fontSize = 32;
        while (true)
        {
            Customer_Text.text = _hackerStrings[rnd.Range(0, _hackerStrings.Length)];
            yield return new WaitForSeconds(cycleDuration);
        }
    }

    /// <summary>
    /// The timer for the warning sound to be played
    /// </summary>
    /// <param name="delay">The delay</param>
    /// <param name="audio">The audio manager</param>
    /// <param name="sound">The clip to play</param>
    /// <param name="transform">The transform</param>
    /// <returns>A coroutine</returns>
    public IEnumerator WarningSound(float delay, KMAudio audio, AudioClip sound, Transform transform)
    {
        while (true)
        {
            audio.PlaySoundAtTransform(sound.name, transform);
            yield return new WaitForSeconds(delay);
        }
    }

    public IEnumerator SolveAnimation(string solvedString, TextMesh customerText, TextMesh[] allTexts)
    {
        string[] solve = solvedString.Split(':');
        bool jump = false;
        for (int i = 0; i < solve.Length; i++)
        {
            if (i == 0) customerText.text = solve[i] == "_" ? " " : solve[i];
            else if (i == 2)
            {
                for (int x = 0; x < solve[i].Length; x++)
                {
                    allTexts[i + x + 1].text = solve[i][x] == '_' ? " " : solve[i][x].ToString();
                    yield return new WaitForSeconds(0.1f);
                }
                jump = true;
            }
            else allTexts[i + 1 + (jump ? 11 : 0)].text = solve[i] == "_" ? " " : solve[i];
            yield return new WaitForSeconds(0.15f);
        }
        yield break;
    }

}
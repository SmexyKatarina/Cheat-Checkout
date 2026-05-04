using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rnd = UnityEngine.Random;

/// <summary>
/// The controller for creating a long jumbled character string
/// </summary>
public class StringPoolGenerator
{

    /// <summary>
    /// The stored generated string
    /// </summary>
    private string _generatedString;
    /// <summary>
    /// The current pointer index along the string
    /// </summary>
    private int _poolIndex;

    /// <summary>
    /// Generates a string upon initialization
    /// </summary>
    /// <param name="length">The length of the string to generate</param>
    /// <param name="alphabet">The alphabet to use for generation</param>
    public StringPoolGenerator(int length, string alphabet)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
            sb.Append(alphabet[rnd.Range(0, alphabet.Length)]);
        _generatedString = sb.ToString();
        _poolIndex = 0;
    }

    /// <summary>
    /// Get the next character in the string
    /// </summary>
    /// <returns>The character in the string at the current pointers position, wrapping if needed.</returns>
    public char GetNextChar()
    {
        if (_poolIndex >= _generatedString.Length) _poolIndex = 0;
        return _generatedString[_poolIndex++];
    }

}
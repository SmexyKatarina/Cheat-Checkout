
/// <summary>
/// The crypto struct
/// </summary>
public struct Crypto
{
    /// <summary>
    /// The name of the crypto
    /// </summary>
    public string Name;
    /// <summary>
    /// The price of the crypto
    /// </summary>
    public double Price;

    /// <summary>
    /// Create a crypto instance
    /// </summary>
    /// <param name="name">The name of the crypto</param>
    /// <param name="price">The price of the crypto</param>
    public Crypto(string name, double price)
    {
        Name = name;
        Price = price;
    }
}
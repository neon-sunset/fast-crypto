namespace FastCrypto.Utils;

public enum HmacAlgorithm
{
    [Obsolete("HMACSHA1 is known to the state of Californa to cause cancer. Please avoid if possible.")]
    HMACSHA1,
    HMACSHA256,
    HMACSHA384,
    HMACSHA512
}

public static class HmacUtils
{
    // TODO
}
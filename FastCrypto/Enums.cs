namespace FastCrypto;

public enum HashAlgorithm
{
    [Obsolete("MD5 is known to the state of California to cause cancer. Please avoid if possible.")]
    MD5 = 1,
    [Obsolete("SHA1 is known to the state of California to cause cancer. Please avoid if possible.")]
    SHA1 = 2,
    SHA256 = 3,
    SHA384 = 4,
    SHA512 = 5,
}

public enum HmacAlgorithm
{
    [Obsolete("HMACSHA1 is known to the state of Californa to cause cancer. Please avoid if possible.")]
    HMACSHA1 = 0,
    HMACSHA256 = 1,
    HMACSHA384 = 2,
    HMACSHA512 = 3
}
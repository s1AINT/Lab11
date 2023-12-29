using System;
using System.Numerics;
using System.Security.Cryptography;

class DSAAlgorithm
{
    static void Main()
    {
        BigInteger p, q, g, x, y;

        GeneratePQ(out p, out q);
        GenerateG(p, q, out g);
        GenerateKeys(p, q, g, out x, out y);

        Console.WriteLine("Generated p: " + p);
        Console.WriteLine("Generated q: " + q);

        string message = "Hello, DSA!";
        BigInteger[] signature = Sign(message, p, q, g, x);

        bool isValid = Verify(message, signature[0], signature[1], p, q, g, y);

        Console.WriteLine("Original Message: " + message);
        Console.WriteLine("Is Signature Valid? " + isValid);
    }

    static void GeneratePQ(out BigInteger p, out BigInteger q)
    {
        q = GenerateProbablePrime(160);

        BigInteger k = BigInteger.Zero;
        BigInteger pTemp;

        do
        {
            k++;
            pTemp = k * q + 1;
        } while (!IsProbablePrime(pTemp, 10));

        p = pTemp;
    }

    static void GenerateG(BigInteger p, BigInteger q, out BigInteger g)
    {
        BigInteger h;
        do
        {
            h = BigInteger.ModPow(RandomBigInteger(2, p - 2), (p - 1) / q, p);
        } while (h == 1);

        g = BigInteger.ModPow(h, (p - 1) / q, p);
    }

    static void GenerateKeys(BigInteger p, BigInteger q, BigInteger g, out BigInteger x, out BigInteger y)
    {
        x = RandomBigInteger(2, q - 1);
        y = BigInteger.ModPow(g, x, p);
    }

    static BigInteger[] Sign(string message, BigInteger p, BigInteger q, BigInteger g, BigInteger x)
    {
        BigInteger k;
        BigInteger[] signature = new BigInteger[2];

        do
        {
            do
            {
                k = RandomBigInteger(2, q - 1);
            } while (k == 0);

            signature[0] = BigInteger.ModPow(g, k, p) % q;
            signature[1] = (ModInverse(k, q) * (Hash(message) + x * signature[0])) % q;

        } while (signature[0] == 0 || signature[1] == 0);

        return signature;
    }

    static bool Verify(string message, BigInteger r, BigInteger s, BigInteger p, BigInteger q, BigInteger g, BigInteger y)
    {
        if (r < 1 || r > q - 1 || s < 1 || s > q - 1)
            return false;

        BigInteger w = ModInverse(s, q);
        BigInteger u1 = (Hash(message) * w) % q;
        BigInteger u2 = (r * w) % q;

        u1 = (u1 + q) % q; // Забезпечення, що u1 знаходиться в діапазоні [0, q - 1]
        u2 = (u2 + q) % q; // Забезпечення, що u2 знаходиться в діапазоні [0, q - 1]

        BigInteger v = ((BigInteger.ModPow(g, u1, p) * BigInteger.ModPow(y, u2, p)) % p) % q;

        return v == r;
    }

    static BigInteger Hash(string message)
    {
        using (SHA1Managed sha1 = new SHA1Managed())
        {
            byte[] hashBytes = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(message));
            Array.Resize(ref hashBytes, hashBytes.Length + 1);
            hashBytes[hashBytes.Length - 1] = 0;
            return new BigInteger(hashBytes.Reverse().ToArray());
        }
    }

    static BigInteger RandomBigInteger(BigInteger minValue, BigInteger maxValue)
    {
        Random random = new Random();

        byte[] bytes = new byte[maxValue.ToByteArray().Length];
        random.NextBytes(bytes);

        BigInteger randomValue = new BigInteger(bytes);
        randomValue = BigInteger.Abs(randomValue % (maxValue - minValue + 1)) + minValue;

        return randomValue;
    }

    static bool IsProbablePrime(BigInteger source, int certainty)
    {
        if (source == 2 || source == 3)
            return true;

        if (source < 2 || source % 2 == 0)
            return false;

        BigInteger d = source - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        Random random = new Random();

        for (int i = 0; i < certainty; i++)
        {
            BigInteger a = RandomBigInteger(2, source - 2);

            BigInteger x = BigInteger.ModPow(a, d, source);
            if (x == 1 || x == source - 1)
                continue;

            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, source);
                if (x == 1)
                    return false;
                if (x == source - 1)
                    break;
            }

            if (x != source - 1)
                return false;
        }

        return true;
    }

    static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m;
        BigInteger x0 = 0;
        BigInteger x1 = 1;

        if (m == 1)
            return 0;

        while (a > 1)
        {
            BigInteger q = a / m;
            BigInteger t = m;

            m = a % m;
            a = t;
            t = x0;

            x0 = x1 - q * x0;
            x1 = t;
        }

        if (x1 < 0)
            x1 += m0;

        return x1;
    }

    static BigInteger GenerateProbablePrime(int bitSize)
    {
        Random random = new Random();

        BigInteger number = RandomBigInteger(BigInteger.Pow(2, bitSize - 1), BigInteger.Pow(2, bitSize) - 1);

        if (number % 2 == 0)
            number++;

        while (!IsProbablePrime(number, 10))
            number += 2;

        return number;
    }
}

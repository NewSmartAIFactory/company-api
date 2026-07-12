using System.Security.Cryptography;
using System.Text;

namespace NewSmartAIFactory.CompanyApi.Services;

public interface IEmbeddingProvider
{
    int Dimensions { get; }
    double[] Embed(string text);
}

public sealed class DeterministicEmbeddingProvider : IEmbeddingProvider
{
    public int Dimensions => 64;

    public double[] Embed(string text)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        var vector = new double[Dimensions];
        for (var i = 0; i < vector.Length; i++) vector[i] = (digest[i % digest.Length] / 127.5d) - 1d;
        var norm = Math.Sqrt(vector.Sum(x => x * x));
        return norm == 0 ? vector : vector.Select(x => x / norm).ToArray();
    }
}

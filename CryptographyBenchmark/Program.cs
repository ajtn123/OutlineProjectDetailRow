using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;

BenchmarkRunner.Run<CryptographyBenchmark>();

public class CryptographyBenchmark
{
    private byte[] data = null!;

    [Params(1 << 10, 1 << 20)]
    public int DataLength { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        data = RandomNumberGenerator.GetBytes(DataLength);
    }

    [Benchmark] public byte[] M5() => MD5.HashData(data);
    [Benchmark] public byte[] S1() => SHA1.HashData(data);
    [Benchmark] public byte[] S256() => SHA256.HashData(data);
    [Benchmark] public byte[] S384() => SHA384.HashData(data);
    [Benchmark] public byte[] S512() => SHA512.HashData(data);
    [Benchmark] public byte[] S3_256() => SHA3_256.HashData(data);
    [Benchmark] public byte[] S3_384() => SHA3_384.HashData(data);
    [Benchmark] public byte[] S3_512() => SHA3_512.HashData(data);
}

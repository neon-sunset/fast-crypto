using System.Runtime.Intrinsics.Arm;

namespace FastCrypto.Benchmarks.Utils;

public class HashUtilsThroughputBenchmark
{
    public int IterationCount { get; init; } = 10000;

    public void Execute()
    {
        Console.WriteLine(
            $"{nameof(ArmBase)}:{ArmBase.IsSupported}, " +
            $"{nameof(ArmBase.Arm64)}:{ArmBase.Arm64.IsSupported}, " +
            $"{nameof(AdvSimd)}:{AdvSimd.IsSupported}, " +
            $"{nameof(Sha256)}:{Sha256.IsSupported}\n");

        var inputString = File.ReadAllText("Input.txt");
        var inputBytes = Encoding.UTF8.GetBytes(inputString);

        Console.WriteLine("\"From Bytes\" loop start!");
        var stopwatch = Stopwatch.StartNew();
        var output = Sha256Loop(inputBytes);
        stopwatch.Stop();
        Console.WriteLine($"Hash: {output} MB/s: {IterationCount / ((decimal)stopwatch.ElapsedMilliseconds / 1000)}");

        Console.WriteLine("\"From Bytes ARM64\" loop start!");
        stopwatch.Restart();
        output = Sha256Arm64Loop(inputBytes);
        stopwatch.Stop();
        Console.WriteLine($"Hash ARM64: {output} MB/s: {IterationCount / ((decimal)stopwatch.ElapsedMilliseconds / 1000)}");

        Console.WriteLine("\"From String With Decode\" loop start!");
        stopwatch.Restart();
        output = DecodeAndSha256Loop(inputString);
        stopwatch.Stop();
        Console.WriteLine($"Decode+Hash: {output} MB/s: {IterationCount / ((decimal)stopwatch.ElapsedMilliseconds / 1000)}");
    }

    private string DecodeAndSha256Loop(string input)
    {
        var output = string.Empty;
        for (var i = 0; i < IterationCount; i++)
        {
            output = HashUtils.ComputeDigest(HashAlgorithm.SHA256, input);
        }

        return output;
    }

    private string Sha256Arm64Loop(ReadOnlySpan<byte> inputBytes)
    {
        var output = string.Empty;
        for (var i = 0; i < IterationCount; i++)
        {
            output = HashUtils.ComputeDigest(HashAlgorithm.SHA256Arm64, inputBytes);
        }

        return output;
    }

    private string Sha256Loop(ReadOnlySpan<byte> inputBytes)
    {
        var output = string.Empty;
        for (var i = 0; i < IterationCount; i++)
        {
            output = HashUtils.ComputeDigest(HashAlgorithm.SHA256, inputBytes);
        }

        return output;
    }
}
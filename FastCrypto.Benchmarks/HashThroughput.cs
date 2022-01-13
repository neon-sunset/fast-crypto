using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;

using FastCrypto.Algorithms;

namespace FastCrypto.Benchmarks;

public class HashThroughput
{
    private readonly string _inputString;
    private readonly byte[] _inputBytes;
    private readonly int _iterationCount;
    private readonly decimal _inputMiBCount;

    public HashThroughput(string? inputFilePath)
    {
        inputFilePath = inputFilePath is null or { Length: 0 }
            ? "Input.txt" : inputFilePath;

        Console.WriteLine(
            "Supported ARM intrinsics: " +
            $"{nameof(ArmBase)}:{ArmBase.IsSupported}, " +
            $"{nameof(ArmBase.Arm64)}:{ArmBase.Arm64.IsSupported}, " +
            $"{nameof(AdvSimd)}:{AdvSimd.IsSupported}, " +
            $"{nameof(Sha1)}:{Sha1.IsSupported} " +
            $"{nameof(Sha256)}:{Sha256.IsSupported}");

        _inputString = File.ReadAllText(inputFilePath);
        _inputBytes = Encoding.UTF8.GetBytes(_inputString);
        _inputMiBCount = (decimal)_inputBytes.Length / 1048576;
        _iterationCount = (int)(5120 / _inputMiBCount);

        Console.WriteLine($"Input size: {_inputMiBCount} MiB(s)");
    }

    public int Benchmark()
    {
        Benchmark(_inputBytes, input => Sha256BuiltIn(input), nameof(Sha256BuiltIn));
        Benchmark(_inputBytes, input => Sha256Arm64(input), nameof(Sha256Arm64));
        Benchmark(_inputBytes, input => HashUtilsSha256(input), nameof(HashUtilsSha256));
        Benchmark(_inputString, input => HashUtilsSha256WithDecode(input), nameof(HashUtilsSha256WithDecode));

        return 0;
    }

    public int Stress()
    {
        Stress(_inputBytes, input => Sha256BuiltIn(input), nameof(Sha256BuiltIn));

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Benchmark<TInput, TOutput>(TInput input, Func<TInput, TOutput> loop, string name)
        where TInput : class
        where TOutput : class
    {
        Console.WriteLine($"\n{name}: ");
        var stopwatch = Stopwatch.StartNew();
        var stResult = loop(input);
        stopwatch.Stop();

        var stThroughput = (float)(_iterationCount / stopwatch.Elapsed.TotalSeconds * (double)_inputMiBCount);
        Console.Write($"ST {stThroughput} MiB/s / ");

        var processorCount = Environment.ProcessorCount;
        var inputs = Enumerable
            .Range(0, processorCount)
            .Select(_ => (input, loop))
            .ToArray();

        stopwatch.Restart();
        var mtResult = Multithreaded(inputs);
        stopwatch.Stop();

        var mtThroughput = (float)(_iterationCount / stopwatch.Elapsed.TotalSeconds * processorCount * (double)_inputMiBCount);
        var scalingFactor = mtThroughput / stThroughput;
        var scalingFactorPerCore = mtThroughput / stThroughput / processorCount;
        var hash = stResult is byte[] hashBytes
            ? Convert.ToHexString(hashBytes).ToLowerInvariant()
            : stResult as string;

        Console.WriteLine($"MT {mtThroughput} MiB/s, scaling factor {scalingFactor}, {scalingFactorPerCore} per core");
        Console.WriteLine($"Hash: {hash!}");
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Stress<TInput, TOutput>(TInput input, Func<TInput, TOutput> loop, string name)
    {
        Console.WriteLine($"Multi-threaded stress test with {name} start.");

        var processorCount = Environment.ProcessorCount;
        var inputs = Enumerable
            .Range(0, processorCount)
            .Select(_ => (input, loop))
            .ToArray();

        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            stopwatch.Restart();
            var mtResult = Multithreaded(inputs);
            stopwatch.Stop();

            var mtThroughput = (float)(_iterationCount / stopwatch.Elapsed.TotalSeconds * processorCount * (double)_inputMiBCount);
            var mtThroughputPerThread = mtThroughput / processorCount;
            Console.WriteLine($"MT {mtThroughput} MiB/s, {mtThroughputPerThread} MiB/s per thread");
        }
    }

    private static bool Multithreaded<TInput, TOutput>((TInput Input, Func<TInput, TOutput> Loop)[] inputs) =>
        Parallel.ForEach(inputs, static item => item.Loop(item.Input)).IsCompleted;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private byte[] Sha256Arm64(ReadOnlySpan<byte> inputBytes)
    {
        Span<byte> destination = stackalloc byte[32];
        for (var i = 0; i < _iterationCount; i++)
        {
            _ = SHA256.HashData(inputBytes, destination);
        }

        return destination.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private byte[] Sha256BuiltIn(ReadOnlySpan<byte> inputBytes)
    {
        Span<byte> destination = stackalloc byte[32];
        for (var i = 0; i < _iterationCount; i++)
        {
            _ = System.Security.Cryptography.SHA256.HashData(inputBytes, destination);
        }

        return destination.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private string HashUtilsSha256(ReadOnlySpan<byte> inputBytes)
    {
        string output = string.Empty;
        for (var i = 0; i < _iterationCount; i++)
        {
            output = Digest.ComputeHex(HashAlgorithm.SHA256, inputBytes, useLowercase: true);
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private string HashUtilsSha256WithDecode(string input)
    {
        string output = string.Empty;
        for (var i = 0; i < _iterationCount; i++)
        {
            output = Digest.ComputeHex(HashAlgorithm.SHA256, input, useLowercase: true);
        }

        return output;
    }
}

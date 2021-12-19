using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;

namespace FastCrypto.Benchmarks.Utils;

public class HashUtilsThroughputBenchmark
{
    public const int IterationCount = 10000;

    public void Execute()
    {
        Console.WriteLine(
            $"{nameof(ArmBase)}:{ArmBase.IsSupported}, " +
            $"{nameof(ArmBase.Arm64)}:{ArmBase.Arm64.IsSupported}, " +
            $"{nameof(AdvSimd)}:{AdvSimd.IsSupported}, " +
            $"{nameof(Sha256)}:{Sha256.IsSupported}\n");

        var inputString = File.ReadAllText("Input.txt");
        var inputBytes = Encoding.UTF8.GetBytes(inputString);

        Benchmark(inputBytes, static input => Sha256Loop(input), nameof(Sha256Loop));
        Benchmark(inputBytes, static input => Sha256Arm64Loop(input), nameof(Sha256Arm64Loop));
        Benchmark(inputBytes, static input => Sha256HashUtilsLoop(input), nameof(Sha256HashUtilsLoop));
        Benchmark(inputString, static input => Sha256HashUtilsDecodeLoop(input), nameof(Sha256HashUtilsDecodeLoop));
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void Benchmark<TInput, TOutput>(TInput input, Func<TInput, TOutput> loop, string name)
        where TInput : class
        where TOutput : class
    {
        Console.Write($"{name} ST! ");
        var stopwatch = Stopwatch.StartNew();
        var output = loop(input);
        stopwatch.Stop();

        var stThroughput = (float)(IterationCount / ((decimal)stopwatch.ElapsedMilliseconds / 1000));
        Console.Write($"MiB/s: {stThroughput}\n");

        var inputs = Enumerable
            .Range(0, Environment.ProcessorCount)
            .Select(num => (input, loop))
            .ToArray();

        Console.Write($"{name} MT! ");
        stopwatch.Restart();
        var mtResult = Multithreaded(inputs);
        stopwatch.Stop();

        var mtThroughput = (float)(IterationCount / ((decimal)stopwatch.ElapsedMilliseconds / 1000) * Environment.ProcessorCount);
        Console.Write($"MiB/s: {mtThroughput} Scaling factor: {mtThroughput / stThroughput}\n\n");
    }

    private static bool Multithreaded<TInput, TOutput>((TInput Input, Func<TInput, TOutput> Loop)[] inputs) =>
        Parallel
            .ForEach(inputs, static item => item.Loop(item.Input))
            .IsCompleted;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static byte[] Sha256Arm64Loop(ReadOnlySpan<byte> inputBytes)
    {
        Span<byte> destination = stackalloc byte[32];
        for (var i = 0; i < IterationCount; i++)
        {
            _ = FastCrypto.SHA256.HashData(inputBytes, destination);
        }

        return destination.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static byte[] Sha256Loop(ReadOnlySpan<byte> inputBytes)
    {
        Span<byte> destination = stackalloc byte[32];
        for (var i = 0; i < IterationCount; i++)
        {
            _ = System.Security.Cryptography.SHA256.HashData(inputBytes, destination);
        }

        return destination.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string Sha256HashUtilsLoop(ReadOnlySpan<byte> inputBytes)
    {
        string output = string.Empty;
        for (var i = 0; i < IterationCount; i++)
        {
            output = HashUtils.ComputeDigest(HashAlgorithm.SHA256, inputBytes, useLowercase: false);
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string Sha256HashUtilsDecodeLoop(string input)
    {
        string output = string.Empty;
        for (var i = 0; i < IterationCount; i++)
        {
            output = HashUtils.ComputeDigest(HashAlgorithm.SHA256, input, useLowercase: false);
        }

        return output;
    }
}
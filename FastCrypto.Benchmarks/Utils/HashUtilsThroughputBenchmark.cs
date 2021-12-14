namespace FastCrypto.Benchmarks.Utils;

public class HashUtilsThroughputBenchmark
{
    public int IterationCount { get; init; } = 1000;

    public void Execute()
    {
        var inputString = File.ReadAllText("Input.txt");
        var inputBytes = Encoding.UTF8.GetBytes(inputString);

        Console.WriteLine("\"From Bytes\" loop start!");
        var stopwatch = Stopwatch.StartNew();
        var output = Sha256Loop(inputBytes);
        stopwatch.Stop();
        Console.WriteLine($"Hash: {output} MB/s: {IterationCount / ((decimal)stopwatch.ElapsedMilliseconds / 1000)}");

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
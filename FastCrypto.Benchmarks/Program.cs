using FastCrypto.Benchmarks;

Console.WriteLine("Specify input file path (default: \"Input.txt\"):");
var throughputBenchmark = new HashThroughput(Console.ReadLine());

SelectCase:
Console.WriteLine("Benchmark or stress-test? B/S: ");
var resultCode = (char)(Console.Read() | ' ') switch // bitwise to lowercase
{
    'b' => throughputBenchmark.Benchmark(),
    's' => throughputBenchmark.Stress(),
    _ => 1
};

if (resultCode is 1)
{
    goto SelectCase;
}
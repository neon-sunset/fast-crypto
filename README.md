# FastCrypto
## This is a work-in-progress library, please do not use until the implementation is finalized.
Fast implementation of cryptographic APIs with intrinsics that falls back to standard .NET APIs in case the former are unavailable.


## Benchmarks
``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.1 (21C52) [Darwin 21.2.0]
Apple M1 Pro, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.2 (42.42.42.42424), Arm64 RyuJIT (LOCAL BRANCH)
  DefaultJob : .NET 6.0.2 (42.42.42.42424), Arm64 RyuJIT (LOCAL BRANCH)


```
|           Method |                Input |       Mean |   Error |  StdDev | Ratio |  Gen 0 | Allocated |
|----------------- |--------------------- |-----------:|--------:|--------:|------:|-------:|----------:|
|           **Sha256** | **72q0(...)ptip [1024]** |   **752.5 ns** | **0.73 ns** | **0.68 ns** |  **1.00** | **0.0725** |     **152 B** |
| Sha256Intrinsics | 72q0(...)ptip [1024] |   679.7 ns | 1.12 ns | 0.88 ns |  0.90 | 0.0725 |     152 B |
|                  |                      |            |         |         |       |        |           |
|           **Sha256** |  **Ak8p(...)NTp5 [128]** |   **299.8 ns** | **0.30 ns** | **0.26 ns** |  **1.00** | **0.0725** |     **152 B** |
| Sha256Intrinsics |  Ak8p(...)NTp5 [128] |   214.3 ns | 0.49 ns | 0.46 ns |  0.71 | 0.0725 |     152 B |
|                  |                      |            |         |         |       |        |           |
|           **Sha256** | **nwMf3(...)3hAcq [32]** |   **195.5 ns** | **0.45 ns** | **0.40 ns** |  **1.00** | **0.0725** |     **152 B** |
| Sha256Intrinsics | nwMf3(...)3hAcq [32] |   142.8 ns | 0.16 ns | 0.15 ns |  0.73 | 0.0725 |     152 B |
|                  |                      |            |         |         |       |        |           |
|           **Sha256** |  **动态网自(...)网自由门 [731]** | **1,190.2 ns** | **1.52 ns** | **1.42 ns** |  **1.00** | **0.0725** |     **152 B** |
| Sha256Intrinsics |  动态网自(...)网自由门 [731] | 1,189.7 ns | 0.90 ns | 0.84 ns |  1.00 | 0.0725 |     152 B |

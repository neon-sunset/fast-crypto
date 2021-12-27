# FastCrypto
## This is a work-in-progress library, please do not use until the implementation is finalized.
Fast implementation of cryptographic APIs with intrinsics that falls back to standard .NET APIs in case the former are unavailable.

## Todo
- [ ] (in progress) Finalize library API surface
- [ ] (in progress) Implement intrinsics-optimized versions of digest computatation methods
- [ ] Implement streaming for hashing and encryption over memory streams and file handles
- [ ] Implement fuzzying and constant-time test suites
- [ ] Add automated benchmark suites to CI pipeline
- [ ] Publish library to nuget.org
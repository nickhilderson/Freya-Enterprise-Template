# 4. Reproducible Builds

Freya validates deterministic publishing by:

1. Performing two separate publish operations
2. Cleaning between builds
3. Comparing SHA256 hashes of output files

Mismatch = pipeline failure.

This ensures:

- No hidden non-deterministic artifacts
- No timestamp-based drift
- Integrity validation of build outputs
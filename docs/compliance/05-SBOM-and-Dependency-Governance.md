# 5. SBOM & Dependency Governance

Freya generates a CycloneDX SBOM per build.

The SBOM includes:

- All direct dependencies
- All transitive dependencies
- Package metadata

Additionally, Freya exports:

- JSON dependency snapshot
- Versioned inventory evidence

This enables:

- Third-party risk review
- Procurement transparency
- Regulatory reporting
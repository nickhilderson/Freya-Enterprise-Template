# 1. Governance Overview

Freya implements an automated, CI-enforced governance framework for .NET software systems.

The framework enforces:

- Deterministic builds
- SBOM generation
- Dependency inventory
- License policy enforcement
- Vulnerability gating
- Reproducibility validation
- Provenance attestation
- Audit-ready evidence export

Governance is not documentation-based.
It is artifact-based.

All controls are executed during CI and produce immutable evidence.
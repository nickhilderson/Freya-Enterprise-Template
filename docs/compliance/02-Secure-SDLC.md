# 2. Secure Software Development Lifecycle (Secure SDLC)

Freya integrates governance directly into the CI pipeline.

## Enforced Controls

- Build must succeed in Release mode
- All tests must pass
- Vulnerability scan fails on High/Critical
- License policy gate enforcement
- Deterministic publish validation
- Supply chain evidence export

This enforces a shift-left security model.

Security is not post-release validation.
It is enforced before merge.
# Titan-ANPR SDK

Titan-ANPR is a native Windows x64 SDK for automatic license plate recognition (ANPR/LPR), combining plate detection and OCR.

## License

- Commercial SDK for production/runtime use
- 30-day trial available for evaluation
- Activation: send hardware ID to `licenses@safetyroad.es`

## Resources

- SDK product page: https://safetyroad.es/anpr-sdk
- OCR online demo: https://safetyroad.es/ocr-demo

## Repository structure

- `include/` native headers (`titan_anpr.h`, `titan_license.h`)
- `docs/` user guide and third-party notices
- `samples/csharp/` C# integration samples
- `samples/python/` Python ctypes samples

## Quick start

1. Build/package your native runtime under `bin/Titan-ANPR/Release/x64`
2. Run sample license flow to activate
3. Run detection samples for C# or Python

For distribution notes and API details, start with `docs/README.md`.

## Packaging (ZIP)

See `PACKAGING.md` for what to include in the downloadable SDK archive and pre-release checks.

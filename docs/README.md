# Titan-ANPR SDK (User Guide)

Titan-ANPR is a native Windows x64 DLL for:

- License plate detection
- OCR text recognition
- Country classification (per plate in unified results, plus optional `CountryDetector_*` API)
- Unified pipeline (`Init` -> `Detect` -> `Dispose`)

This guide matches the public headers in `include/titan_anpr.h` and `titan_license.h`.

## What You Get

After building, use the redistributable folder:

- `bin/Titan-ANPR/Release/x64/`

It includes this structure:

- `bin/`
  - `Titan-ANPR.dll`
  - Required native runtime dependencies (ONNX Runtime / OpenVINO / TBB / OpenCV)
- `include/`
  - `titan_anpr.h`
  - `titan_license.h`
- `lib/`
  - `Titan-ANPR.lib` (import library for linking with the DLL)
- `docs/`
  - `README.md` (this file)
  - `THIRD_PARTY_NOTICES.txt` (third-party runtimes; update per release)

## Platform Requirements

- Windows x64
- Microsoft Visual C++ 2015-2022 Redistributable (x64)

## Licensing

- Titan-ANPR SDK binaries and headers are **commercial**; production/runtime use requires a valid license.
- A **30-day trial** is available for evaluation (see `LICENSE-SDK.txt` in the package root).
- **Activation:** obtain the machine hardware ID (`TitanLicense_GetHardwareId` or the sample tools), then email **licenses@safetyroad.es** to request a key; activate with `TitanLicense_Activate`.
- Sample **source code** under `samples/` may be under a separate license; see `LICENSE-SAMPLES.txt` where provided.

## Third-party components

Bundled native runtimes (OpenCV, ONNX Runtime, OpenVINO, TBB, etc.) remain under **their own licenses**. Keep `THIRD_PARTY_NOTICES.txt` with any redistribution of the `bin` folder.

## Core API (unified)

Main functions:

- `TitanANPR_Init`
- `TitanANPR_Detect`
- `TitanANPR_Dispose`
- `TitanANPR_Clear`
- `TitanANPR_GetSelectedEP`

Optional low-level APIs (same DLL, see `titan_anpr.h`):

- `PlateDetector_*`
- `OcrDetector_*`
- `CountryDetector_*` (`Create`, `Destroy`, `Predict` — image-level country id + confidence)

`TitanANPR_Detect` returns **multiple** `TitanAnprResult` items per call; each item includes **country** fields filled for that detection (numeric id, confidence, and a short name string when available).

Function shape:

- `TitanANPR_Detect(handle, imgData, width, height, stride, outResults, maxResults, returnedCount)`

Each `TitanAnprResult` contains:

- `found`
- `total_confidence`
- `plate_confidence`
- `ocr_confidence`
- `country_id`
- `country_confidence`
- `points[4]` (plate corner points)
- `plate_text`
- `country_name` (short label, e.g. ISO-style; up to 63 chars + null)

## Basic Usage (C/C++)

1. Initialize: `TitanANPR_Init(&handle)`
2. For each frame/image:
   - Allocate an array of `TitanAnprResult` (e.g. 16 or 32 entries).
   - Call `TitanANPR_Detect(handle, imgData, width, height, stride, outResults, maxResults, &returnedCount)`.
   - Process `outResults[0 .. returnedCount-1]`.
3. Release: `TitanANPR_Dispose(handle)`

## Basic Usage (C# / Python)

See `samples/csharp/README.md` and `samples/python/README.md`. The repository ships example projects that load `Titan-ANPR.dll`, call the unified API, and demonstrate license activation.

## Error Handling

- Return `0`: success
- Negative return value: error
- `-10`: license is not valid for runtime use

## Performance Recommendations

- Use `Release` build
- Run x64 only
- Reuse the engine handle (do not initialize per frame)
- Keep image buffers contiguous when possible

## Deployment Checklist

- Copy the full folder tree from `bin/Titan-ANPR/Release/x64`
- Install VC++ Redistributable x64 on target machines
- Verify startup with `TitanANPR_Init`
- Verify inference with `TitanANPR_Detect` on a sample frame
- Include `THIRD_PARTY_NOTICES.txt` with the shipped `bin` folder

## Linux reference tree (optional)

A separate folder `Titan-ANPR-Linux/` may exist in the repository for Linux-oriented builds and samples. It is **not** part of the standard Windows redistributable ZIP unless you explicitly include it. Country-related types and the unified pipeline on Windows are defined in `include/titan_anpr.h` above.

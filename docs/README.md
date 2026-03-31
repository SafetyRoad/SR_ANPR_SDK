# Titan-ANPR SDK (User Guide)

Titan-ANPR is a native Windows x64 DLL for:

- License plate detection
- OCR text recognition
- Single-call pipeline (`Init` -> `Detect` -> `Dispose`)

This guide is focused on SDK consumers.

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
  - `README.md`
  - `THIRD_PARTY_NOTICES.txt` (third-party runtimes shipped with the native `bin`; update per release)
  - `InternalInfo.md` (internal technical notes)

## Third-party components

The redistributable native `bin` folder may include DLLs from projects such as ONNX Runtime, OpenVINO, TBB, OpenCV, and others. Those components are subject to their own licenses and notice requirements. See `THIRD_PARTY_NOTICES.txt` in this folder and keep it in any ZIP or installer you ship to customers.

## Platform Requirements

- Windows x64
- Microsoft Visual C++ 2015-2022 Redistributable (x64)

## License model

- Commercial SDK (production/runtime usage requires a valid paid license).
- 30-day trial is available for evaluation.
- To activate, retrieve the machine hardware ID (see `titan_license.h` API or sample apps) and request a license key at `licenses@safetyroad.es`.

## Online resources

- SDK page: https://safetyroad.es/anpr-sdk
- OCR online demo: https://safetyroad.es/ocr-demo

## Core API

Main functions:

- `TitanANPR_Init`
- `TitanANPR_Detect`
- `TitanANPR_Dispose`
- `TitanANPR_Clear`
- `TitanANPR_GetSelectedEP`

Result returned by `TitanANPR_Detect`:

- `found`
- `plate_text`
- `total_confidence`
- `plate_confidence`
- `ocr_confidence`
- `points[4]` (plate corner points)

## Basic Usage (C/C++)

1. Initialize:
- `TitanANPR_Init(&handle)`

2. Run detection on each frame:
- `TitanANPR_Detect(handle, imgData, width, height, stride, &result)`

3. Release resources:
- `TitanANPR_Dispose(handle)`

## Basic Usage (C# via P/Invoke)

Typical flow:

1. Create engine (calls `Init` once)
2. Call `Detect` for each frame
3. Dispose engine on shutdown

## Error Handling

- Return `0`: success
- Negative return value: error
- `-10`: license is not valid for runtime use

## Performance Recommendations

- Use `Release` build
- Run x64 only
- Reuse detector handle (do not initialize per frame)
- Keep image buffers contiguous when possible

## Deployment Checklist

- Copy the full folder tree from `bin/Titan-ANPR/Release/x64`
- Install VC++ Redistributable x64 on target machines
- Verify startup by calling `TitanANPR_Init`
- Verify inference by calling `TitanANPR_Detect` on a sample frame

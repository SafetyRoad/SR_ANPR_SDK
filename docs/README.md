# Titan-ANPR SDK (User Guide)

Titan-ANPR is a native Windows x64 DLL for:

- License plate detection
- OCR text recognition
- Unified pipeline (`Init` -> `Detect` -> `Dispose`)

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
  - `InternalInfo.md` (internal technical notes)

## Platform Requirements

- Windows x64
- Microsoft Visual C++ 2015-2022 Redistributable (x64)

## Core API

Main functions:

- `TitanANPR_Init`
- `TitanANPR_Detect`
- `TitanANPR_Dispose`
- `TitanANPR_Clear`
- `TitanANPR_GetSelectedEP`

`TitanANPR_Detect` returns a list of `TitanAnprResult` items.

Function shape:

- `TitanANPR_Detect(handle, imgData, width, height, stride, outResults, maxResults, returnedCount)`

Each `TitanAnprResult` contains:

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
- allocate an array of `TitanAnprResult` (for example 16 or 32 items)
- call `TitanANPR_Detect(handle, imgData, width, height, stride, outResults, maxResults, &returnedCount)`
- process the first `returnedCount` entries

3. Release resources:
- `TitanANPR_Dispose(handle)`

## Basic Usage (C# via P/Invoke)

Typical flow:

1. Create engine (calls `Init` once)
2. Call `DetectAll` for each frame (or `Detect` if you only want the first match)
3. Dispose engine on shutdown

If you already use `TitanAnprNativeWrapper.cs`, no additional glue code is required.

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
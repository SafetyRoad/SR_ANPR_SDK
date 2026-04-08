# Titan-ANPR Linux

Separate Linux project to build and test Titan-ANPR outside Visual Studio.

## Contents

- Shared library: `libTitan-ANPR.so` (via CMake).
- Test CLI: `sample_cli`.
- Encrypted runtime models (copied from `Titan-ANPR`):
  - `models/detector_model.enc`
  - `models/ocr_model.enc`
  - `models/country_model.enc`
- Temporary Linux license stub: `src/titan_license_stub.cpp`.

## Requirements (Ubuntu)

- `cmake` 3.20+
- C++17 compiler (`g++` or `clang++`)
- OpenCV development package (`libopencv-dev`)
- OpenSSL development package (`libssl-dev`)
- ONNX Runtime (include + library)

Base install:

```bash
sudo apt update
sudo apt install -y build-essential cmake libopencv-dev libssl-dev
```

## Build

```bash
cd Titan-ANPR-Linux
cmake -S . -B build -DTITAN_ONNXRUNTIME_ROOT=/opt/onnxruntime
cmake --build build -j
```

Output:

- `build/bin/libTitan-ANPR.so`
- `build/bin/sample_cli`

## Runtime models (required)

Linux runtime loads **encrypted models** (`.enc`) from disk.

Set:

```bash
export TITAN_ANPR_MODEL_DIR=/path/to/models
```

Expected files:

- `detector_model.enc`
- `ocr_model.enc`
- `country_model.enc`

## How to regenerate `.enc` files (optional)

From Windows (PowerShell), using the existing script in `Titan-ANPR`:

```powershell
cd Titan-ANPR
powershell -ExecutionPolicy Bypass -File .\encrypt_models.ps1 `
  -PlateModel ..\TestDetectorPose\lic_pose_no_country.onnx `
  -OcrModel ..\VideoLprProcessor\ocr.onnx `
  -CountryModel ..\TestDetectorPose\country.onnx
```

This generates in `Titan-ANPR`:

- `detector_model.enc`
- `ocr_model.enc`
- `country_model.enc`
- `model_keys.h`

Then copy the `.enc` files to `Titan-ANPR-Linux/models` (or to the directory used in `TITAN_ANPR_MODEL_DIR`).

## Quick test

```bash
export TITAN_ANPR_MODEL_DIR=./models
./build/bin/sample_cli ./samples/images/4793_MSB.png 10
```

## Current status

- EP on Linux: CPU baseline.
- Linux license: temporary stub (real implementation still pending).

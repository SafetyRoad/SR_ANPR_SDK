# Titan-ANPR runtime bin folder

This folder is **not tracked in Git** because it contains large native binaries.
When you build or unpack the official SDK ZIP, it should have a structure similar
to the following (example for Windows x64 Release):

```text
bin/
  Titan-ANPR/
    Release/
      x64/
        bin/
          Titan-ANPR.dll
          <onnxruntime DLLs>
          <OpenVINO / oneAPI / TBB DLLs>
          <OpenCV DLLs>
          <other runtime dependencies>
```

## Required DLLs (example, adjust to your build)

At minimum, the following must be present in the runtime folder that you pass to
`nativeBinDirectory` (C#) or `--bin` (Python):

- `Titan-ANPR.dll` — main ANPR/ALPR engine (this SDK)
- All DLLs required by `Titan-ANPR.dll` such as (names will depend on your build):
  - ONNX Runtime (e.g. `onnxruntime.dll`, `onnxruntime_providers_*.dll`)
  - Intel OpenVINO / oneAPI / TBB libraries
  - OpenCV runtime DLLs
  - Any additional vendor-specific runtimes

The **exact list** depends on how you built Titan-ANPR. Before publishing a
release ZIP, you should:

1. Snapshot the final `bin/Titan-ANPR/Release/x64/bin` folder.
2. List all `.dll` files included in the ZIP.
3. Update this README with the **exact DLL names and versions** for that
   release (and keep them in sync with `docs/THIRD_PARTY_NOTICES.txt`).

## How samples use this folder

- C# (`samples/csharp`): leave `nativeBinDirectory` as `null` in `Program.Main`
  and use `TITAN_ANPR_NATIVE_DIR`, or set `nativeBinDirectory` to the folder that
  contains `Titan-ANPR.dll` and its dependencies.
- Python (`samples/python`): pass `--bin` to the scripts so
  `titan_anpr_bindings.load_dll()` can resolve `Titan-ANPR.dll` and load its
  dependent DLLs.

The **GitHub repository** contains only headers, documentation and samples; the
runtime DLLs are distributed as a separate ZIP from the official product page:

- SDK download: https://safetyroad.es/anpr-sdk

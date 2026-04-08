# C# samples — Titan-ANPR SDK

This folder contains a .NET 8 sample solution for integrating the native Titan-ANPR SDK (`Titan-ANPR.dll`) from C#.

## Requirements

- Windows x64
- .NET SDK 8 ([download](https://dotnet.microsoft.com/download))
- Microsoft Visual C++ 2015-2022 Redistributable (x64)
- Native SDK `bin` folder with `Titan-ANPR.dll` and dependencies (from the official ZIP — not stored in Git)
- Valid license for `TitanANPR_Init` / detection (`-10` often means invalid runtime license)
- License model: commercial SDK with a 30-day evaluation trial (see `LICENSE-SDK.txt`)

## Solution layout

| Project | Description |
|---------|-------------|
| `Titan.Anpr.NativeInterop` | P/Invoke library (DLL load, license API, unified detection API) |
| `Titan.Anpr.LicenseConsole` | Console app for hardware ID and activation flow |
| `Titan.Anpr.DetectionConsole` | Console app for detection over image(s) and overlay output |

Solution file: `Titan.Anpr.Samples.sln`.

The detection sample uses the unified multi-result `TitanANPR_Detect` signature (`out_results`, `max_results`, `returned_count`) and draws all returned plates per image. Each `TitanAnprResult` includes `country_id`, `country_confidence`, and `country_name` (see `include/titan_anpr.h`); the console and overlay show them when present.

## License activation (C# sample)

1. Run `Titan.Anpr.LicenseConsole.exe hwid`
2. Send hardware ID to `licenses@safetyroad.es`
3. Activate with `Titan.Anpr.LicenseConsole.exe activate "YOUR-KEY"`

## Native DLL path

In `Program.Main`, set `nativeBinDirectory` to the folder containing `Titan-ANPR.dll`, or leave it `null` and use `TITAN_ANPR_NATIVE_DIR` / automatic discovery (see `NativeDll`).

## See also

- Python samples: `../python/README.md`
- User guide: `../../docs/README.md`

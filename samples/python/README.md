# Python samples — Titan-ANPR SDK

Minimal `ctypes` examples for Windows x64, in the same spirit as the C# projects under `samples/csharp`.

## Requirements

- Windows x64
- Python 3.10+ (3.11 or 3.12 recommended)
- Microsoft Visual C++ Redistributable (x64) on the target machine
- The native SDK `bin` folder with `Titan-ANPR.dll` and its dependencies
- A valid license to run `TitanANPR_Init` / detection (`-10` often means invalid runtime license)

## Setup

```powershell
cd samples\python
python -m venv .venv
.\.venv\Scripts\activate
pip install -r requirements.txt
```

`numpy` and `Pillow` are only required for `example_detect.py`.

## Locating `Titan-ANPR.dll`

1. Environment variable `TITAN_ANPR_NATIVE_DIR`
2. The `--bin` argument
3. Automatic search for `bin/Titan-ANPR/Release/x64/bin` or `.../Debug/x64/bin`
4. Current working directory if the DLL is present there

On Python 3.8+, `titan_anpr_bindings.load_dll` uses `os.add_dll_directory` so dependent DLLs load correctly.

## License model and activation

- Titan-ANPR SDK is commercial for production/runtime use.
- A 30-day trial is available for evaluation.
- Activation flow:
  1. Run `python example_license.py --bin "D:\sdk\bin" hwid`
  2. Send the hardware ID to `licenses@safetyroad.es`
  3. Receive your key and run `python example_license.py --bin "D:\sdk\bin" activate "YOUR-KEY"`

## Usage

### License (`example_license.py`)

```powershell
python example_license.py --bin "D:\sdk\bin" info
python example_license.py --bin "D:\sdk\bin" hwid
python example_license.py --bin "D:\sdk\bin" validate
python example_license.py --bin "D:\sdk\bin" activate "YOUR-KEY"
python example_license.py --bin "D:\sdk\bin" deactivate
```

### Detection (`example_detect.py`)

```powershell
python example_detect.py --bin "D:\sdk\bin" path\to\photo.jpg
```

Prints all returned plate results from `TitanANPR_Detect` (up to `max_results`), including plate text, country fields (`country_id`, `country_confidence`, `country_name`), and confidence, plus approximate detect time (does not include `Init`).

## Files

| File | Description |
|------|-------------|
| `titan_anpr_bindings.py` | `ctypes` structures, path resolution, DLL load |
| `example_license.py` | `info`, `hwid`, `validate`, `activate`, `deactivate` |
| `example_detect.py` | Single image -> `TitanANPR_Init` / `Detect` / `Dispose` |

## Limitations

- Windows only
- Unified API supports multiple results per call (`returnedCount`)
- No overlay GUI in these Python scripts

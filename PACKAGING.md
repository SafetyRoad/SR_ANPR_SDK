# Empaquetado del SDK (ZIP para web)

Checklist antes de subir el ZIP oficial.

## Contenido mínimo del paquete

- `bin/Titan-ANPR/Release/x64/` — DLL principal, dependencias nativas (ONNX, OpenVINO/TBB, OpenCV, etc.)
- `include/` — `titan_anpr.h`, `titan_license.h`
- `lib/` — `Titan-ANPR.lib` (si se distribuye enlace estático/import)
- `docs/README.md` — guía de usuario alineada con los headers Windows
- `docs/THIRD_PARTY_NOTICES.txt` — completado para esa versión de DLLs
- `LICENSE-SDK.txt` y, si aplica, `LICENSE-SAMPLES.txt` en la raíz del ZIP
- `samples/` — C# y Python (sin rutas absolutas de máquina de desarrollo)

## Comprobaciones rápidas

1. **Headers:** `include/titan_anpr.h` coincide con lo documentado en `docs/README.md` (API unificada, `TitanAnprResult` con país, APIs opcionales `CountryDetector_*` / `PlateDetector_*` / `OcrDetector_*`).
2. **Samples C#:** `nativeBinDirectory` en `Program.cs` en `null` o ruta de ejemplo genérica, no `C:\repos\...`.
3. **Samples Python:** sin rutas fijas a un repo local; usar `--bin` o variables de entorno.
4. **Terceros:** lista de DLLs en `THIRD_PARTY_NOTICES.txt` coherente con lo que lleva realmente `bin/`.
5. **Opcional:** excluir `Titan-ANPR-Linux/` del ZIP de clientes Windows si solo publicáis el SDK Windows.

## Nombre del archivo

Convención sugerida: `Titan-ANPR-SDK-<version>-Windows-x64.zip` (ajustar según política de producto).

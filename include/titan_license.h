#pragma once

#ifdef _WIN32
#define TITAN_ANPR_API __declspec(dllexport)
#else
#define TITAN_ANPR_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// ── Estados de licencia ──────────────────────────────────────────────────

#define TITAN_LIC_TRIAL       0
#define TITAN_LIC_ACTIVE      1
#define TITAN_LIC_EXPIRED     2
#define TITAN_LIC_INVALID     3
#define TITAN_LIC_REVOKED     4

// ── Códigos de error ─────────────────────────────────────────────────────

#define TITAN_LIC_OK                0
#define TITAN_LIC_ERR_INVALID_KEY  -1
#define TITAN_LIC_ERR_EXPIRED      -2
#define TITAN_LIC_ERR_HW_MISMATCH  -3
#define TITAN_LIC_ERR_TAMPERED     -4
#define TITAN_LIC_ERR_IO           -5
#define TITAN_LIC_ERR_INTERNAL     -6
#define TITAN_LIC_ERR_CLOCK        -7

// ── Información de licencia ──────────────────────────────────────────────

typedef struct TitanLicenseInfo
{
    int   status;
    int   days_remaining;
    int   days_total;
    char  customer_name[64];
    char  license_key[128];
    char  hardware_id[64];
    char  expiry_date[20];
} TitanLicenseInfo;

// ── API de licencia ──────────────────────────────────────────────────────

// Consulta el estado actual de la licencia (trial o activada).
// Devuelve TITAN_LIC_OK si pudo leer el estado; info se rellena siempre.
TITAN_ANPR_API int TitanLicense_GetInfo(TitanLicenseInfo* out_info);

// Obtiene el hardware ID de esta máquina (para enviarlo al vendor).
TITAN_ANPR_API int TitanLicense_GetHardwareId(char* buffer, int buffer_size);

// Activa la licencia con una clave firmada por el vendor.
// Devuelve TITAN_LIC_OK si la clave es válida y se guardó correctamente.
TITAN_ANPR_API int TitanLicense_Activate(const char* license_key);

// Desactiva/limpia la licencia local (vuelve a estado trial si quedan días).
TITAN_ANPR_API int TitanLicense_Deactivate(void);

// Validación interna (llamada por TitanANPR_Init).
// Devuelve TITAN_LIC_OK si la licencia permite operar.
// Devuelve código de error específico si no.
TITAN_ANPR_API int TitanLicense_Validate(void);

#ifdef __cplusplus
}
#endif

#pragma once

#if defined(_WIN32)
#define TITAN_ANPR_API __declspec(dllexport)
#elif defined(__GNUC__)
#define TITAN_ANPR_API __attribute__((visibility("default")))
#else
#define TITAN_ANPR_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

#define TITAN_LIC_TRIAL       0
#define TITAN_LIC_ACTIVE      1
#define TITAN_LIC_EXPIRED     2
#define TITAN_LIC_INVALID     3
#define TITAN_LIC_REVOKED     4

#define TITAN_LIC_OK                0
#define TITAN_LIC_ERR_INVALID_KEY  -1
#define TITAN_LIC_ERR_EXPIRED      -2
#define TITAN_LIC_ERR_HW_MISMATCH  -3
#define TITAN_LIC_ERR_TAMPERED     -4
#define TITAN_LIC_ERR_IO           -5
#define TITAN_LIC_ERR_INTERNAL     -6
#define TITAN_LIC_ERR_CLOCK        -7

typedef struct TitanLicenseInfo
{
    int status;
    int days_remaining;
    int days_total;
    char customer_name[64];
    char license_key[128];
    char hardware_id[64];
    char expiry_date[20];
} TitanLicenseInfo;

TITAN_ANPR_API int TitanLicense_GetInfo(TitanLicenseInfo* out_info);
TITAN_ANPR_API int TitanLicense_GetHardwareId(char* buffer, int buffer_size);
TITAN_ANPR_API int TitanLicense_Activate(const char* license_key);
TITAN_ANPR_API int TitanLicense_Deactivate(void);
TITAN_ANPR_API int TitanLicense_Validate(void);

#ifdef __cplusplus
}
#endif

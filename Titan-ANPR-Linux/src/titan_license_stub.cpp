#include "titan_license.h"

#include <cstdio>
#include <cstring>

namespace
{
void CopyText(const char* src, char* dst, size_t dstSize)
{
    if (!dst || dstSize == 0)
    {
        return;
    }
    std::snprintf(dst, dstSize, "%s", src ? src : "");
}
}

int TitanLicense_GetInfo(TitanLicenseInfo* out_info)
{
    if (!out_info)
    {
        return TITAN_LIC_ERR_INTERNAL;
    }

    std::memset(out_info, 0, sizeof(TitanLicenseInfo));
    out_info->status = TITAN_LIC_ACTIVE;
    out_info->days_remaining = 3650;
    out_info->days_total = 3650;
    CopyText("Linux Stub", out_info->customer_name, sizeof(out_info->customer_name));
    CopyText("linux-stub", out_info->license_key, sizeof(out_info->license_key));
    CopyText("linux-host", out_info->hardware_id, sizeof(out_info->hardware_id));
    CopyText("2099-12-31", out_info->expiry_date, sizeof(out_info->expiry_date));
    return TITAN_LIC_OK;
}

int TitanLicense_GetHardwareId(char* buffer, int buffer_size)
{
    if (!buffer || buffer_size <= 0)
    {
        return TITAN_LIC_ERR_INTERNAL;
    }
    std::snprintf(buffer, static_cast<size_t>(buffer_size), "%s", "linux-host");
    return TITAN_LIC_OK;
}

int TitanLicense_Activate(const char* /*license_key*/)
{
    return TITAN_LIC_OK;
}

int TitanLicense_Deactivate(void)
{
    return TITAN_LIC_OK;
}

int TitanLicense_Validate(void)
{
    return TITAN_LIC_OK;
}

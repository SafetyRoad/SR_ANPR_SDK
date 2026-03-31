"""
License API sample: info, hardware id, validate, activate, deactivate.

  python example_license.py --bin "C:\\path\\to\\sdk\\bin" hwid
  python example_license.py info
  python example_license.py validate
  python example_license.py activate "YOUR-KEY"
  python example_license.py deactivate

If --bin is omitted, uses TITAN_ANPR_NATIVE_DIR or searches from this folder upward.
"""

from __future__ import annotations

import argparse
import sys
from ctypes import byref, create_string_buffer

from titan_anpr_bindings import (
    TitanLicenseInfo,
    bind_license_api,
    license_info_to_dict,
    load_dll,
)


def main() -> int:
    parser = argparse.ArgumentParser(description="Titan-ANPR license API (ctypes sample)")
    parser.add_argument(
        "--bin",
        default=None,
        help="Directory containing Titan-ANPR.dll (optional if TITAN_ANPR_NATIVE_DIR is set)",
    )
    sub = parser.add_subparsers(dest="cmd", required=True)

    sub.add_parser("info", help="Print license metadata")
    sub.add_parser("hwid", help="Print hardware ID for vendor activation")
    sub.add_parser("validate", help="Run the same validation as TitanANPR_Init")
    p_act = sub.add_parser("activate", help="Activate with a license key")
    p_act.add_argument("key")
    sub.add_parser("deactivate", help="Clear local activation")

    args = parser.parse_args()

    try:
        dll = load_dll(args.bin)
    except (FileNotFoundError, RuntimeError) as e:
        print(e, file=sys.stderr)
        return 1

    bind_license_api(dll)

    if args.cmd == "info":
        info = TitanLicenseInfo()
        rc = dll.TitanLicense_GetInfo(byref(info))
        print("TitanLicense_GetInfo rc:", rc)
        for k, v in license_info_to_dict(info).items():
            print(f"  {k}: {v}")
        return 0 if rc == 0 else 1

    if args.cmd == "hwid":
        buf = create_string_buffer(256)
        rc = dll.TitanLicense_GetHardwareId(buf, len(buf))
        print("TitanLicense_GetHardwareId rc:", rc)
        if rc == 0:
            hw = buf.value.decode("ascii", errors="replace")
            print("Hardware ID:", hw)
        return 0 if rc == 0 else 1

    if args.cmd == "validate":
        rc = dll.TitanLicense_Validate()
        print("TitanLicense_Validate rc:", rc)
        return 0 if rc == 0 else 1

    if args.cmd == "activate":
        rc = dll.TitanLicense_Activate(args.key.encode("utf-8"))
        print("TitanLicense_Activate rc:", rc)
        return 0 if rc == 0 else 1

    if args.cmd == "deactivate":
        rc = dll.TitanLicense_Deactivate()
        print("TitanLicense_Deactivate rc:", rc)
        return 0 if rc == 0 else 1

    return 1


if __name__ == "__main__":
    raise SystemExit(main())

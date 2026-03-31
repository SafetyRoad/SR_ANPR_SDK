"""
ctypes bindings for Titan-ANPR (titan_anpr.h, titan_license.h).

Windows x64 only. Call resolve_bin_dir() or set TITAN_ANPR_NATIVE_DIR before load_dll().
"""

from __future__ import annotations

import os
import sys
from ctypes import (
    CDLL,
    POINTER,
    Structure,
    byref,
    c_char,
    c_char_p,
    c_float,
    c_int,
    c_void_p,
    create_string_buffer,
)
from pathlib import Path
from typing import Optional


def _decode_cstr(buf: bytes) -> str:
    return buf.split(b"\x00", 1)[0].decode("latin-1", errors="replace")


class PlatePoint(Structure):
    _fields_ = [("x", c_float), ("y", c_float)]


class TitanAnprResult(Structure):
    _fields_ = [
        ("found", c_int),
        ("total_confidence", c_float),
        ("plate_confidence", c_float),
        ("ocr_confidence", c_float),
        ("point0", PlatePoint),
        ("point1", PlatePoint),
        ("point2", PlatePoint),
        ("point3", PlatePoint),
        ("plate_text", c_char * 32),
    ]


class TitanLicenseInfo(Structure):
    _fields_ = [
        ("status", c_int),
        ("days_remaining", c_int),
        ("days_total", c_int),
        ("customer_name", c_char * 64),
        ("license_key", c_char * 128),
        ("hardware_id", c_char * 64),
        ("expiry_date", c_char * 20),
    ]


def resolve_bin_dir(explicit: Optional[str | Path] = None) -> Path:
    """
    Folder containing Titan-ANPR.dll.
    Order: explicit argument > TITAN_ANPR_NATIVE_DIR > walk parents for bin/Titan-ANPR/.../bin.
    """
    if explicit is not None:
        p = Path(explicit).resolve()
        if p.is_file() and p.name.lower() == "titan-anpr.dll":
            return p.parent
        if (p / "Titan-ANPR.dll").is_file():
            return p
        raise FileNotFoundError(f"Titan-ANPR.dll not found under {explicit!r}")

    env = os.environ.get("TITAN_ANPR_NATIVE_DIR", "").strip()
    if env:
        p = Path(env).resolve()
        if (p / "Titan-ANPR.dll").is_file():
            return p
        raise FileNotFoundError(f"TITAN_ANPR_NATIVE_DIR={env!r} does not contain Titan-ANPR.dll")

    here = Path(__file__).resolve().parent
    cur = here
    for _ in range(20):
        for rel in (
            ("bin", "Titan-ANPR", "Release", "x64", "bin"),
            ("bin", "Titan-ANPR", "Debug", "x64", "bin"),
        ):
            candidate = cur.joinpath(*rel)
            if (candidate / "Titan-ANPR.dll").is_file():
                return candidate
        cur = cur.parent

    cwd = Path.cwd()
    if (cwd / "Titan-ANPR.dll").is_file():
        return cwd

    raise FileNotFoundError(
        "Could not find Titan-ANPR.dll. Set TITAN_ANPR_NATIVE_DIR or pass bin_dir= to load_dll()."
    )


def load_dll(bin_dir: Optional[str | Path] = None) -> CDLL:
    """
    Load Titan-ANPR.dll from bin_dir. Uses os.add_dll_directory (Python 3.8+) so dependent DLLs load.
    If bin_dir is None, uses resolve_bin_dir().
    """
    if bin_dir is None:
        d = resolve_bin_dir()
    else:
        d = Path(bin_dir).resolve()
        if not (d / "Titan-ANPR.dll").is_file():
            raise FileNotFoundError(f"Titan-ANPR.dll not in {d}")

    if sys.platform != "win32":
        raise RuntimeError("Titan-ANPR Python samples target Windows only.")

    if hasattr(os, "add_dll_directory"):
        os.add_dll_directory(str(d))

    dll_path = d / "Titan-ANPR.dll"
    return CDLL(str(dll_path))


def bind_license_api(dll: CDLL) -> None:
    """Attach ctypes signatures for TitanLicense_* exports."""
    dll.TitanLicense_GetInfo.argtypes = [POINTER(TitanLicenseInfo)]
    dll.TitanLicense_GetInfo.restype = c_int

    dll.TitanLicense_GetHardwareId.argtypes = [c_char_p, c_int]
    dll.TitanLicense_GetHardwareId.restype = c_int

    dll.TitanLicense_Activate.argtypes = [c_char_p]
    dll.TitanLicense_Activate.restype = c_int

    dll.TitanLicense_Deactivate.argtypes = []
    dll.TitanLicense_Deactivate.restype = c_int

    dll.TitanLicense_Validate.argtypes = []
    dll.TitanLicense_Validate.restype = c_int


def bind_anpr_api(dll: CDLL) -> None:
    """Attach ctypes signatures for TitanANPR_* unified detection exports."""
    dll.TitanANPR_Init.argtypes = [POINTER(c_void_p)]
    dll.TitanANPR_Init.restype = c_int

    dll.TitanANPR_Detect.argtypes = [
        c_void_p,
        c_void_p,
        c_int,
        c_int,
        c_int,
        POINTER(TitanAnprResult),
    ]
    dll.TitanANPR_Detect.restype = c_int

    dll.TitanANPR_Dispose.argtypes = [c_void_p]
    dll.TitanANPR_Dispose.restype = None


def license_info_to_dict(info: TitanLicenseInfo) -> dict:
    """Return license fields as plain strings and ints for printing."""
    return {
        "status": info.status,
        "days_remaining": info.days_remaining,
        "days_total": info.days_total,
        "customer_name": _decode_cstr(bytes(info.customer_name)),
        "license_key": _decode_cstr(bytes(info.license_key)),
        "hardware_id": _decode_cstr(bytes(info.hardware_id)),
        "expiry_date": _decode_cstr(bytes(info.expiry_date)),
    }


def anpr_result_plate_text(result: TitanAnprResult) -> str:
    """Decode the fixed-size plate_text field from TitanAnprResult."""
    return _decode_cstr(bytes(result.plate_text))

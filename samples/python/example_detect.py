"""
Detection sample: TitanANPR_Init -> TitanANPR_Detect on one image
using the unified multi-result API.

  python example_detect.py --bin "C:\\path\\to\\bin" plate.jpg

Requires: pip install -r requirements.txt
"""

from __future__ import annotations

import argparse
import sys
import time
from ctypes import byref, c_int, c_void_p

import numpy as np
from PIL import Image

from titan_anpr_bindings import TitanAnprResult, anpr_result_plate_text, bind_anpr_api, load_dll


MAX_RESULTS = 32


def main() -> int:
    parser = argparse.ArgumentParser(description="Titan-ANPR unified detect (multi-result ctypes sample)")
    parser.add_argument("image", help="Path to an image file")
    parser.add_argument("--bin", default=None, help="Directory containing Titan-ANPR.dll")
    args = parser.parse_args()

    try:
        dll = load_dll(args.bin)
    except (FileNotFoundError, RuntimeError) as e:
        print(e, file=sys.stderr)
        return 1

    bind_anpr_api(dll)

    handle = c_void_p()
    rc = dll.TitanANPR_Init(byref(handle))
    if rc != 0 or not handle.value:
        print(f"TitanANPR_Init failed, return={rc}, handle={handle.value!r}", file=sys.stderr)
        return 1

    try:
        im = Image.open(args.image).convert("RGB")
        arr = np.ascontiguousarray(np.asarray(im, dtype=np.uint8))
        height, width, _ = arr.shape
        stride = width * 3

        results = (TitanAnprResult * MAX_RESULTS)()
        returned_count = c_int(0)

        t0 = time.perf_counter()
        rc = dll.TitanANPR_Detect(
            handle,
            arr.ctypes.data_as(c_void_p),
            width,
            height,
            stride,
            results,
            MAX_RESULTS,
            byref(returned_count),
        )
        elapsed_ms = (time.perf_counter() - t0) * 1000.0

        if rc != 0:
            print(f"TitanANPR_Detect failed, rc={rc}", file=sys.stderr)
            return 1

        count = max(0, min(returned_count.value, MAX_RESULTS))
        print(f"Returned plates:  {count}")
        print(f"Engine time:      {elapsed_ms:.2f} ms")

        for i in range(count):
            item = results[i]
            text = anpr_result_plate_text(item)
            label = text if text else "(none)"
            print(f"[{i+1}] Plate: {label!r} | Total confidence: {item.total_confidence:.4f} | Found: {item.found}")

        return 0
    finally:
        dll.TitanANPR_Dispose(handle)


if __name__ == "__main__":
    raise SystemExit(main())

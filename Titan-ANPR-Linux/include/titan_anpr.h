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

typedef void* PlateDetectorHandle;
typedef void* OcrDetectorHandle;
typedef void* CountryDetectorHandle;

typedef struct PlateRect
{
    int x;
    int y;
    int w;
    int h;
} PlateRect;

typedef struct PlatePoint
{
    float x;
    float y;
} PlatePoint;

typedef struct PlateDetection
{
    PlateRect box;
    PlatePoint points[4];
    float score;
    int class_id;
} PlateDetection;

TITAN_ANPR_API int PlateDetector_Create(const char* model_path, PlateDetectorHandle* out_handle);
TITAN_ANPR_API void PlateDetector_Destroy(PlateDetectorHandle handle);
TITAN_ANPR_API int PlateDetector_Detect(
    PlateDetectorHandle handle,
    const unsigned char* img_data,
    int width,
    int height,
    int stride,
    float conf_threshold,
    PlateDetection* detections_out,
    int max_detections,
    int* returned_count);

TITAN_ANPR_API int OcrDetector_Create(const char* model_path, OcrDetectorHandle* out_handle);
TITAN_ANPR_API void OcrDetector_Destroy(OcrDetectorHandle handle);
TITAN_ANPR_API int OcrDetector_GetText(
    OcrDetectorHandle handle,
    const unsigned char* img_data,
    int width,
    int height,
    int stride,
    char* text_buffer,
    int text_buffer_size);
TITAN_ANPR_API int OcrDetector_GetTextEx(
    OcrDetectorHandle handle,
    const unsigned char* img_data,
    int width,
    int height,
    int stride,
    char* text_buffer,
    int text_buffer_size,
    float* ocr_confidence);

TITAN_ANPR_API int CountryDetector_Create(const char* model_path, CountryDetectorHandle* out_handle);
TITAN_ANPR_API void CountryDetector_Destroy(CountryDetectorHandle handle);
TITAN_ANPR_API int CountryDetector_Predict(
    CountryDetectorHandle handle,
    const unsigned char* img_data,
    int width,
    int height,
    int stride,
    int* out_country_id,
    float* out_confidence);

typedef void* TitanAnprHandle;

typedef struct TitanAnprResult
{
    int found;
    float total_confidence;
    float plate_confidence;
    float ocr_confidence;
    int country_id;
    float country_confidence;
    PlatePoint points[4];
    char plate_text[32];
    char country_name[64];
} TitanAnprResult;

TITAN_ANPR_API int TitanANPR_Init(TitanAnprHandle* out_handle);
TITAN_ANPR_API int TitanANPR_Detect(
    TitanAnprHandle handle,
    const unsigned char* img_data,
    int width,
    int height,
    int stride,
    TitanAnprResult* out_results,
    int max_results,
    int* returned_count);
TITAN_ANPR_API void TitanANPR_Dispose(TitanAnprHandle handle);
TITAN_ANPR_API void TitanANPR_Clear(TitanAnprHandle handle);
TITAN_ANPR_API int TitanANPR_GetSelectedEP(char* buffer, int buffer_size);

#ifdef __cplusplus
}
#endif

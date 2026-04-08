#include "titan_anpr.h"

#include <opencv2/opencv.hpp>

#include <cstdlib>
#include <cstring>
#include <iostream>
#include <string>
#include <vector>

namespace
{
void PrintUsage()
{
    std::cout
        << "Usage:\n"
        << "  sample_cli <image_path> [max_results]\n\n"
        << "Environment:\n"
        << "  TITAN_ANPR_MODEL_DIR=/path/to/encrypted/models\n";
}
}

int main(int argc, char** argv)
{
    if (argc < 2)
    {
        PrintUsage();
        return 1;
    }

    const std::string imagePath = argv[1];
    int maxResults = 10;
    if (argc >= 3)
    {
        maxResults = std::atoi(argv[2]);
        if (maxResults <= 0)
        {
            maxResults = 10;
        }
    }

    cv::Mat img = cv::imread(imagePath, cv::IMREAD_COLOR);
    if (img.empty())
    {
        std::cerr << "Error: cannot read image: " << imagePath << "\n";
        return 2;
    }

    TitanAnprHandle handle = nullptr;
    const int initErr = TitanANPR_Init(&handle);
    if (initErr != 0 || !handle)
    {
        std::cerr << "Error: TitanANPR_Init failed with code " << initErr << "\n";
        return 3;
    }

    std::vector<TitanAnprResult> results(static_cast<size_t>(maxResults));
    int returnedCount = 0;
    const int detErr = TitanANPR_Detect(
        handle,
        img.data,
        img.cols,
        img.rows,
        static_cast<int>(img.step),
        results.data(),
        static_cast<int>(results.size()),
        &returnedCount);

    if (detErr != 0)
    {
        std::cerr << "Error: TitanANPR_Detect failed with code " << detErr << "\n";
        TitanANPR_Dispose(handle);
        return 4;
    }

    std::cout << "Detections: " << returnedCount << "\n";
    for (int i = 0; i < returnedCount; ++i)
    {
        const auto& r = results[static_cast<size_t>(i)];
        std::cout
            << "[" << i << "] "
            << "plate=\"" << r.plate_text << "\" "
            << "total=" << r.total_confidence << " "
            << "plate_conf=" << r.plate_confidence << " "
            << "ocr_conf=" << r.ocr_confidence << " "
            << "country_id=" << r.country_id << " "
            << "country=\"" << r.country_name << "\" "
            << "country_conf=" << r.country_confidence
            << "\n";
    }

    TitanANPR_Dispose(handle);
    return 0;
}

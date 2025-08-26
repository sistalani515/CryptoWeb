using CryptoWeb.Shared.Models.Responses.IconCaptcha;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Helper.IconCaptcha
{
    public static class IconCaptchaHelper
    {
        public static string GenerateWidgetId()
        {
            return Guid.NewGuid().ToString();
        }
        public static long GetCurrentUnixTimestampMillis()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return now.ToUnixTimeMilliseconds();
        }
        public static long GenerateTimestamp(long initTimestamp)
        {
            Random random = new Random();
            int millisecondsToAdd = random.Next(500, 1501);
            return initTimestamp + millisecondsToAdd;
        }
        public static string EncodeBase64(string plainText)
        {
            try
            {
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public static string DecodeBase64(string base64EncodedText)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(base64EncodedText);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (FormatException)
            {
                return null!; // Or you might want to throw the exception
            }
            catch (Exception)
            {
                return null!; // Or you might want to throw the exception
            }
        }


        public static int CountIconRegions(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using var ms = new MemoryStream(imageBytes);
            using var bitmap = new Bitmap(ms);
            using var mat = BitmapConverter.ToMat(bitmap);

            // Convert to grayscale
            using var gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

            // Threshold to binary
            using var binary = new Mat();
            Cv2.Threshold(gray, binary, 40, 255, ThresholdTypes.BinaryInv);

            // Optional: Morphological operations to reduce noise
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernel);

            // Find contours
            Cv2.FindContours(binary, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Filter based on expected size (avoid small dots)
            int count = contours.Count(c =>
            {
                var rect = Cv2.BoundingRect(c);
                return rect.Width > 20 && rect.Height > 20;
            });

            return count;
        }

        private const int ExpectedWidth = 320;
        private const int ExpectedHeight = 50;
        private const double CenterCropRatio = 0.7; // Extracting 70% of the section width
        private static int[] GetImagePixels(Bitmap bitmap)
        {
            int[] pixels = new int[bitmap.Width * bitmap.Height];
            int index = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels[index++] = bitmap.GetPixel(x, y).ToArgb();
                }
            }
            return pixels;
        }

        public static (int x, int y)? FindLeastFrequentIcon(string base64String, string outputFolder = "centered_icons")
        {
            try
            {
                int sections = 0;
                try
                {
                    sections = CountIconRegions(base64String);
                }
                catch (Exception)
                {
                    sections = 5;
                }
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using MemoryStream? ms = new(imageBytes!);
                using Bitmap img = new(ms);

                if (img!.Width != ExpectedWidth || img!.Height != ExpectedHeight)
                {
                    return null;
                }

                int sectionWidth = img.Width / sections;
                int centerWidth = (int)(sectionWidth * CenterCropRatio);
                int halfCenterWidth = centerWidth / 2;

                List<int[]> iconDataList = new();
                Dictionary<int, int> iconHashes = new();
                Dictionary<int, int> indexMapping = new();

                for (int i = 0; i < sections; i++)
                {
                    int centerX = (i * sectionWidth) + (sectionWidth / 2);
                    int left = Math.Max(centerX - halfCenterWidth, i * sectionWidth);
                    int right = Math.Min(centerX + halfCenterWidth, (i + 1) * sectionWidth);

                    using Bitmap iconCrop = img.Clone(new Rectangle(left, 0, right - left, img.Height), img.PixelFormat)!;
                    int[] iconPixels = GetImagePixels(iconCrop);
                    iconDataList.Add(iconPixels);

                    int iconHash = iconPixels.Aggregate(0, (hash, val) => hash ^ val); // Simple XOR-based hash

                    if (!iconHashes.ContainsKey(iconHash))
                    {
                        iconHashes[iconHash] = 0;
                        indexMapping[iconHash] = i;
                    }
                    iconHashes[iconHash]++;
                }

                var leastFrequentHash = iconHashes.OrderBy(x => x.Value).FirstOrDefault();
                if (leastFrequentHash.Key == 0) return null;

                int leastFrequentIndex = indexMapping[leastFrequentHash.Key];
                int xCoord = (leastFrequentIndex * sectionWidth) + (sectionWidth / 2);
                int yCoord = img!.Height! / 2;

                return (xCoord, yCoord);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public static async Task<IconCaptchaImageResponse> ResponseToImage(this HttpResponseMessage message, string widgetId)
        {
            try
            {
                message.EnsureSuccessStatusCode();
                var responseText = await message.Content.ReadAsStringAsync();
                if (responseText.Length < 1000) return null!;
                var imageDecode = DecodeBase64(responseText);
                var imageResponse = JsonConvert.DeserializeObject<IconCaptchaImageResponse>(imageDecode);
                if(imageResponse == null) return null!;
                var coordinates = FindLeastFrequentIcon(imageResponse.Challenge!);
                if (coordinates == null || !coordinates.HasValue || coordinates.Value.x == 0) return null;
                imageResponse.WidgetId = widgetId;
                imageResponse.X = coordinates.Value.x;
                imageResponse.Y = coordinates.Value.y;


                return imageResponse;
            }
            catch (Exception)
            {
                return null!;
            }
        }
        public static async Task<IconCaptchaImageResponse> ResponseToImage(this TlsClient.Core.Models.Responses.Response message, string widgetId)
        {
            try
            {
                var responseText = message.Body;
                if (responseText.Length < 1000) return null!;
                var imageDecode = DecodeBase64(responseText);
                var imageResponse = JsonConvert.DeserializeObject<IconCaptchaImageResponse>(imageDecode);
                if (imageResponse == null) return null!;
                var coordinates = FindLeastFrequentIcon(imageResponse.Challenge!);
                if (coordinates == null || !coordinates.HasValue || coordinates.Value.x == 0) return null!;
                imageResponse.WidgetId = widgetId;
                imageResponse.X = coordinates.Value.x;
                imageResponse.Y = coordinates.Value.y;


                return imageResponse;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public static async Task<IconCaptchaImageResultResponse> ImageToResult(this HttpResponseMessage message)
        {
            try
            {
                message.EnsureSuccessStatusCode();
                var responseText = await message.Content.ReadAsStringAsync();
                var imageDecode = DecodeBase64(responseText);
                var imageResponse = JsonConvert.DeserializeObject<IconCaptchaImageResultResponse>(imageDecode);
                if (imageResponse == null) return null!;
                return imageResponse;
            }
            catch (Exception)
            {
                return null!;
            }
        }
        public static async Task<IconCaptchaImageResultResponse> ImageToResult(this TlsClient.Core.Models.Responses.Response message)
        {
            try
            {
                var responseText = message.Body;
                var imageDecode = DecodeBase64(responseText);
                var imageResponse = JsonConvert.DeserializeObject<IconCaptchaImageResultResponse>(imageDecode);
                if (imageResponse == null) return null!;
                return imageResponse;
            }
            catch (Exception)
            {
                return null!;
            }
        }
    }
}

using ImageMagick;
using iTextSharp.text.pdf;
using OnSign.Common.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.CommonBL
{
    public class PdfToImage
    {
        private static readonly string pathTempMagick = System.Web.Hosting.HostingEnvironment.MapPath("~/Temp");
        private static readonly string gsPath = $"{ConfigHelper.RootFolder}gs";

        public PdfToImage()
        {
            if (!Directory.Exists(pathTempMagick))
                Directory.CreateDirectory(pathTempMagick);

            MagickNET.SetTempDirectory(pathTempMagick);
            MagickNET.SetGhostscriptDirectory(gsPath);
        }

        public void ConvertPdfToPNG(string pdfDocName)
        {
            var rootFileName = Path.GetFileName(pdfDocName);
            var DirectoryName = Path.GetDirectoryName(pdfDocName);
            try
            {
                string fullPath = $"{DirectoryName}\\{rootFileName}-{Common.Constants.SMALL_THUMB}_{Common.Constants.THUMB_EXTENSION}";

                //Kiểm tra trang đầu tiên được convert chưa?
                if (!File.Exists(fullPath.Insert(fullPath.LastIndexOf('.'), "1")))
                {
                    //Tạo hàng loạt thumb
                    int countPage = 0;
                    using (PdfReader pdfReader = new PdfReader(pdfDocName))
                    {
                        PdfReader.unethicalreading = true;
                        countPage = pdfReader.NumberOfPages;
                        pdfReader.Close();
                        pdfReader.Dispose();
                    }
                    new Thread(() =>
                    {
                        ConvertToPNG(pdfDocName, fullPath, 1, countPage, 10);
                    }).Start();
                    string big = fullPath.Replace(Common.Constants.SMALL_THUMB, Common.Constants.BIG_THUMB);
                    for (int i = 1; i <= countPage; i++)
                    {
                        ConvertToPNG(pdfDocName, big, i, i, 200);
                    }
                }
            }
            catch (Exception ex)
            {
                ConfigHelper.Instance.WriteLogException("Lỗi chuyển đổi định dạng pdf sang image", ex, MethodBase.GetCurrentMethod().Name, "ConvertImageToPdf");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="startPage"></param>
        /// <param name="endPage"></param>
        /// <param name="density">Chất lượng ảnh (300 là chất lượng tốt, nếu là thumb: density = 20)</param>
        /// <returns></returns>
        public void ConvertToPNG(string inputFile, string outputFile, int? startPage = null, int? endPage = null, double density = 300)
        {
            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(density, density)
                };
                if (startPage != null)
                    settings.FrameIndex = startPage.Value - 1;
                if (endPage != null)
                    settings.FrameCount = endPage.Value - startPage.Value + 1;
                //new Thread(() =>
                //{
                using (var images = new MagickImageCollection())
                {
                    images.Read(inputFile, settings);
                    var page = startPage != null ? startPage : 1;
                    foreach (var image in images)
                    {
                        string filename = outputFile.Insert(outputFile.LastIndexOf('.'), page.ToString());
                        //Resize image
                        var maxWidth = 1100;
                        if (image.Width > maxWidth)
                        {
                            decimal percent = maxWidth / image.Width;
                            var size = new MagickGeometry(maxWidth, (int)(image.Height * percent));
                            image.Resize(size);
                        }
                        image.Write(filename);
                        page++;
                    }
                    images.Dispose();
                }
                //}).Start();
                Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 10 }, i =>
                  {
                  });

            }
            catch (Exception ex)
            {
                ConfigHelper.Instance.WriteLogException("Lỗi chuyển đổi định dạng pdf sang image", ex, MethodBase.GetCurrentMethod().Name, "ConvertToPNG");
                throw;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using iTextSharp.text.pdf;
using SAB.Library.Core.Crypt;
using SAB.Library.Core.FileService;

namespace OnSign.Common.Helpers
{
    public class MethodHelper
    {
        //Config 
        //public static string ProductImageUrl = ConfigurationManager.AppSettings["ProductImageUrl"];
        //public static string NewsImageUrl = ConfigurationManager.AppSettings["NewsImageUrl"];
        //public static string AdvImageUrl = ConfigurationManager.AppSettings["AdvImageUrl"];
        public string ToUpperFirstLetter(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            // convert to char array of the string
            char[] letters = source.ToCharArray();
            // upper case the first char
            letters[0] = char.ToUpper(letters[0]);
            // return the array made of the new char array
            return new string(letters);
        }

        public static string GenerateUUID()
        {
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            Guid g = Guid.NewGuid();
            return $"{g:D}-{unixTimestamp}";
        }

        public static string GenerateReferenceCode()
        {
            string[] str = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C", "V", "B", "N", "M" };
            string timestring = DateTime.Now.ToString("yyyyMdhmsfff");
            long num = Convert.ToInt64(timestring);
            string decode = "";
            do
            {
                long d = num % str.Length;
                num /= str.Length;
                decode += str[d];
            } while (num > 0);

            return decode;
        }

        #region -- Static (implement Singleton pattern) --

        /// <summary>
        /// The instance
        /// </summary>
        private static volatile MethodHelper _instance;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object _syncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static MethodHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new MethodHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion
        public static string StringEncryptPassword(string password, string keySecret, string keySecretLv2)
        {
            return Cryptography.Encrypt(Cryptography.Encrypt(password, keySecret), keySecretLv2);
        }

        public string MergeEventStr(MethodBase objMethodBase)
        {
            return MergeEventStr(objMethodBase.DeclaringType.FullName, objMethodBase.Name);
        }

        public static string GetIPClient()
        {
            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        public string MergeEventStr(string strFullClass, string strMethodName)
        {
            return string.Format("{0} -> {1}", strFullClass, strMethodName);
        }

        public static DateTime Date2Date(DateTime dtDate)
        {
            try
            {
                return DateTime.ParseExact(dtDate.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public static DateTime String2Date(string strDate, string format = "dd/MM/yyyy")
        {
            try
            {
                return string.IsNullOrEmpty(strDate) ? DateTime.Now : DateTime.ParseExact(strDate, format, CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public string ConvertListToString(List<string> lstString = null, List<int> lstInt = null)
        {
            string strConvert = "";
            if (lstString != null)
                strConvert = string.Join(",", lstString);
            if (lstInt != null)
                strConvert = string.Join(",", lstInt);
            return strConvert;
        }

        public string GetTimeFormat(DateTime dtBegin, DateTime dtEnd)
        {

            string result = string.Empty;
            TimeSpan ts = dtEnd - dtBegin;
            if (ts.Days >= 365)
            {
                result += ts.Days / 365 + " năm, ";
                if (ts.Days % 365 > 30)
                    result += (ts.Days % 365) / 30 + " tháng";
                else
                    result += ts.Days % 365 + " ngày ";
            }
            else
            {
                if (ts.Days >= 30)
                {
                    result += ts.Days / 30 + " tháng, ";
                    if (ts.Days % 30 > 0)
                        result += ts.Days % 30 + " ngày ";
                }
                else
                {
                    if (ts.Days > 0)
                    {
                        result += ts.Days + " ngày, ";
                        if (ts.Hours > 0)
                            result += ts.Hours + " giờ ";
                    }
                    else
                    {
                        if (ts.Minutes > 0)
                            result += ts.Minutes + " phút ";
                        //if (ts.Seconds > 0)
                        //    result += ts.Seconds + " giây ";
                    }
                }
            }
            return result;
        }

        public string ConvertBoolToStr(bool bolValue)
        {
            return bolValue ? "1" : "0";
        }

        public string GetErrorMessage(Exception objEx, string strErrorMessage)
        {
            if (objEx.Message.IndexOf("[+") != -1)
            {
                int i = objEx.Message.IndexOf("[+");
                int j = objEx.Message.IndexOf("+]") + 2;
                if (i < j)
                {
                    return objEx.Message.Substring(i + 2, j - i - 4);
                }
            }
            return strErrorMessage;
        }

        public ResultMessageBO FillResultMessage(bool bolIsError, ErrorTypes enErrorType, string strMessage, string strMessageDetail)
        {
            ResultMessageBO objResultMessageBO = new ResultMessageBO
            {
                IsError = bolIsError,
                ErrorType = enErrorType,
                Message = strMessage,
                MessageDetail = strMessageDetail
            };
            return objResultMessageBO;
        }

        public void ConvertToObject(IDataReader reader, dynamic lstObj)
        {
            try
            {
                while (reader.Read())
                {
                    Exception objError = null;
                    dynamic obj = Activator.CreateInstance(lstObj.GetType().GetGenericArguments()[0]);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (!Convert.IsDBNull(reader[i]))
                        {
                            dynamic type = obj.GetType().GetProperty(reader.GetName(i).ToUpper());

                            if (type != null)
                            {
                                //DLL Npgsql mới phân biệt giữa timespan với datetime nên phải trick
                                if (reader[i].GetType().Name == typeof(TimeSpan).Name
                                    && (type.PropertyType.Name == typeof(DateTime).Name
                                    || (type.PropertyType.Name == typeof(Nullable<>).Name
                                        && type.PropertyType.GenericTypeArguments[0].Name == typeof(DateTime).Name))
                                    )
                                {
                                    TimeSpan time = (TimeSpan)reader[i];
                                    type.SetValue(obj, new DateTime() + time, null);

                                }
                                else
                                {
                                    try
                                    {
                                        type.SetValue(obj, reader[i], null);
                                    }
                                    catch (Exception ex)
                                    {
                                        ConfigHelper.Instance.WriteLogException(ex.Message, "Convert value: " + reader[i] + " to " + type.ToString() + " Class " + obj.ToString(), MethodBase.GetCurrentMethod().Name, "ConvertToObject");
                                        objError = ex;
                                    }
                                }
                            }
                        }
                    }

                    if (objError != null)
                        throw objError;

                    lstObj.Add(obj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Hàm tính số ngày giao nhau giữa 2 khoản thời gian
        /// </summary>
        /// <param name="dateInFrom"></param>
        /// <param name="dateInTo"></param>
        /// <param name="dateOutFrom"></param>
        /// <param name="dateOutTo"></param>
        /// <returns></returns>
        public static int CalculatorJoinDate(DateTime dateInFrom, DateTime dateInTo, DateTime dateOutFrom, DateTime dateOutTo)
        {
            List<DateTime> listTimeIn = new List<DateTime>();
            List<DateTime> listTimeOut = new List<DateTime>();

            while (dateInTo.Date >= dateInFrom.Date)
            {
                listTimeIn.Add(dateInFrom.Date);
                dateInFrom = dateInFrom.AddDays(1);
            }

            while (dateOutTo >= dateOutFrom)
            {
                listTimeOut.Add(dateOutFrom.Date);
                dateOutFrom = dateOutFrom.AddDays(1);
            }

            return listTimeIn.Intersect(listTimeOut).Count();
        }

        /// <summary>
        ///  Hàm tính số ngày giao nhau giữa 1 khoản thời gian và ds các khoản thời gian còn lại
        /// </summary>
        /// <param name="dateInFrom"></param>
        /// <param name="dateInTo"></param>
        /// <param name="listDateCompare"></param>
        /// <returns></returns>
        public static int CalculatorJoinDate(DateTime dateInFrom, DateTime dateInTo, List<DateTime[]> listDateCompare)
        {
            List<DateTime> listTimeIn = new List<DateTime>();
            List<DateTime> listTimeOut = new List<DateTime>();

            listTimeIn = Enumerable.Range(0, 1 + dateInTo.Subtract(dateInFrom).Days)
                                      .Select(offset => dateInFrom.AddDays(offset))
                                      .ToList();

            foreach (var date in listDateCompare)
            {
                DateTime dateOutFrom = date.FirstOrDefault();
                DateTime dateOutTo = date.LastOrDefault();

                listTimeOut.AddRange(Enumerable.Range(0, 1 + dateOutTo.Subtract(dateOutFrom).Days)
                                    .Select(offset => dateOutFrom.AddDays(offset)));
            }

            return listTimeIn.Intersect(listTimeOut).Distinct().Count();
        }

        public static string ConvertDataTableToString(DataTable dt, bool bolIsUpperColumnName = false)
        {
            var result = "";
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dt.Rows)
            {
                if (bolIsUpperColumnName)
                {
                    var row = dt.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName.ToUpper(), col => dr[col]);
                    rows.Add(row);
                }
                else
                {
                    var row = dt.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col]);
                    rows.Add(row);
                }
            }

            result = serializer.Serialize(rows);
            return result;
        }

        /// <summary>
        /// Quy đổi số lượng (Vd: Thùng = 24 lon, Lốc = 6 lon), truyền vào 32 long sẽ trả ra '1 Thùng 1 Lốc 2 lon'
        /// </summary>
        /// <param name="strItemIDNeedExchange">Mã ItemID của sản phẩm cần quy đổi</param>
        /// <param name="intQuantityUnitIDNeedExchange">Mã đơn vị tính của sản phẩm cần quy đổi</param>
        /// <param name="strQuantityUnitNeedExchange">Tên đơn vị tính của sản phẩm cần quy đổi</param>
        /// <param name="decQuantityNeedExchange">Số lượng của sản phẩm cần quy đổi</param>
        /// <param name="dtbExchangeQuantityUnit">Datatable ở bảng quy đổi ItemExchangeQuantityUnit</param>
        /// <returns></returns>
        public static string GetExchangeQuantityByQuantity(string strItemIDNeedExchange, int intQuantityUnitIDNeedExchange, string strQuantityUnitNeedExchange, decimal decQuantityNeedExchange, DataTable dtbExchangeQuantityUnit)
        {
            if (dtbExchangeQuantityUnit == null || dtbExchangeQuantityUnit.Rows.Count == 0) return null;
            //-- Tìm những quy đổi theo strItemIDNeedExchange order by theo EXCHANGEQUANTITY để quy đổi những sản phẩm lớn trước
            List<DataRow> lstItem = dtbExchangeQuantityUnit.AsEnumerable().Where(f =>
            Convert.ToString(f[Constants.COL_ITEMID]).Trim() == strItemIDNeedExchange.Trim()).OrderByDescending(f => Convert.ToDecimal(f[Constants.COL_EXCHANGEQUANTITY])).ToList();

            if (lstItem.Count == 0) return null;
            string strExchange = string.Empty;
            foreach (DataRow dr in lstItem)
            {
                decimal decExchangeQuantity = Convert.ToDecimal(dr[Constants.COL_EXCHANGEQUANTITY]);
                int intQuantityUnitID = Convert.ToInt32(dr[Constants.COL_EXCHANGEQUANTITYUNITID]);
                //-- Nếu đơn vị tính trong bảng quy đổi giống với đơn vị tính cần quy đổi thì thoát luôn
                if (intQuantityUnitID == intQuantityUnitIDNeedExchange)
                {
                    break;
                }
                decimal decQuantity = Math.Floor(decQuantityNeedExchange / decExchangeQuantity);
                if (decQuantity > 0)
                {
                    strExchange += string.Format(" {0} {1} ", decQuantity, Convert.ToString(dr[Constants.COL_QUANTITYUNIT]).Trim());
                    decQuantityNeedExchange -= decQuantity * decExchangeQuantity;
                }
            }
            if (strExchange.Length == 0) return null;
            if (decQuantityNeedExchange > 0)
            {
                strExchange += string.Format(" {0} {1} ", decQuantityNeedExchange, strQuantityUnitNeedExchange.Trim());
            }
            return strExchange.Trim();
        }

        public string ConvertNullToStr(object objValue)
        {
            return (objValue == null || objValue == DBNull.Value) ? string.Empty : objValue.ToString();
        }

        /// <summary>
        /// Copy an object to destination object, only matching fields will be copied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">An object with matching fields of the destination object</param>
        /// <param name="destObject">Destination object, must already be created</param>
        public static void CopyObject<T>(object sourceObject, ref T destObject)
        {
            //  If either the source, or destination is null, return
            if (sourceObject == null || destObject == null)
                return;

            //  Get the type of each object
            Type sourceType = sourceObject.GetType();
            Type targetType = destObject.GetType();

            //  Loop through the source properties
            foreach (PropertyInfo p in sourceType.GetProperties())
            {
                //  Get the matching property in the destination object
                PropertyInfo targetObj = targetType.GetProperty(p.Name);
                //  If there is none, skip
                if (targetObj == null)
                    continue;

                //  Set the value in the destination
                targetObj.SetValue(destObject, p.GetValue(sourceObject, null), null);
            }
        }

        public static bool ValidatePassword(string password)
        {
            const int MIN_LENGTH = 8;
            const int MAX_LENGTH = 150;

            if (password == null) throw new ArgumentNullException();

            bool meetsLengthRequirements = password.Length >= MIN_LENGTH && password.Length <= MAX_LENGTH;
            bool hasUpperCaseLetter = false;
            bool hasLowerCaseLetter = false;
            bool hasDecimalDigit = false;

            if (meetsLengthRequirements)
            {
                foreach (char c in password)
                {
                    if (char.IsUpper(c)) hasUpperCaseLetter = true;
                    else if (char.IsLower(c)) hasLowerCaseLetter = true;
                    else if (char.IsDigit(c)) hasDecimalDigit = true;
                }
            }

            bool isValid = meetsLengthRequirements
                        && hasUpperCaseLetter
                        && hasLowerCaseLetter
                        && hasDecimalDigit
                        ;
            return isValid;

        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    var domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsPhoneNumberValid(string phone)
        {
            bool isValid = false;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                isValid = Regex.IsMatch(phone, @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}", RegexOptions.IgnoreCase);
            }
            return isValid;
        }

        /// <summary>
        /// Lấy n chữ số cuối cùng
        /// </summary>
        /// <param name="source">nguồn số điện thoại</param>
        /// <param name="number">số lượng muốn lấy</param>
        /// <returns></returns>
        public static string GetLastPhoneNumberString(string source, int number)
        {
            source = CleanNumber(source);
            if (source.Length < number)
                return "";
            return source.Substring(source.Length - number, number);

        }
        public static string CleanNumber(string phone)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(phone, "");
        }


        //public static byte[] Export2XLS(DataTable dtData)
        //{
        //    using (OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage())
        //    {
        //        OfficeOpenXml.ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

        //        ws.Cells["A1"].LoadFromDataTable(dtData, true);

        //        using (OfficeOpenXml.ExcelRange rng = ws.Cells["A1:BZ1"])
        //        {
        //            rng.Style.Font.Bold = true;
        //            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;   //Set Pattern for the background to Solid
        //            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);  //Set color to dark blue
        //            rng.Style.Font.Color.SetColor(System.Drawing.Color.Black);
        //        }

        //        for (int i = 0; i < dtData.Columns.Count; i++)
        //        {
        //            if (dtData.Columns[i].DataType == typeof(DateTime))
        //            {
        //                using (OfficeOpenXml.ExcelRange col = ws.Cells[2, i + 1, 2 + dtData.Rows.Count, i + 1])
        //                {
        //                    col.Style.Numberformat.Format = "dd/MM/yyyy";
        //                }
        //            }
        //            if (dtData.Columns[i].DataType == typeof(TimeSpan))
        //            {
        //                using (OfficeOpenXml.ExcelRange col = ws.Cells[2, i + 1, 2 + dtData.Rows.Count, i + 1])
        //                {
        //                    col.Style.Numberformat.Format = "d.hh:mm";
        //                    col.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                }
        //            }

        //        }
        //        return pck.GetAsByteArray();
        //    }
        //}

        /// <summary>
        /// Xóa toàn bộ file trong thư mục
        /// truongnv 20200218
        /// </summary>
        /// <param name="path">đường dẫn</param>
        public static void RemoveFileInDirectory(string path)
        {
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch { }
        }

        /// Đọc dữ liệu trong file
        /// truongnv 20200220
        /// </summary>
        /// <param name="fileName">tên file: vd: key.text hoặc key.json</param>
        /// <returns></returns>
        public static string ReadDataFile(string fileName)
        {
            string data;
            try
            {
                //get the Json filepath  
                string file = HttpContext.Current.Server.MapPath($"~/Data/Secret/{fileName}");
                //deserialize JSON from file  
                data = File.ReadAllText(file);
            }
            catch { data = string.Empty; }
            return data;
        }

        public static HttpClient CreateHttpClient(string url, string apiId, string apiSecret)
        {
            HttpClient client;
            client = new HttpClient(new HMACDelegatingHandler(apiId, apiSecret))
            {
                BaseAddress = new Uri(url.Replace(new Uri(url).PathAndQuery, ""))
            };
            return client;
        }

        public static string Base64StringToFile(string base64Content, string filePath)
        {
            string msg = string.Empty;
            try
            {
                byte[] content = Convert.FromBase64String(base64Content);
                File.WriteAllBytes(filePath, content);
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }
            return msg;
        }

        public static string RandomNumber(int size)
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, size).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomString(int size)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, size).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string BetweenStrings(string text, string start, string end)
        {
            int p1 = text.IndexOf(start) + start.Length;
            int p2 = text.IndexOf(end, p1);

            if (end == "") return (text.Substring(p1));
            else return text.Substring(p1, p2 - p1);
        }

        public static bool SaveImage(string base64Image, string distination, int resizeW = 0, int resizeH = 0)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(distination);
                if (!fileInfo.Directory.Exists)
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                byte[] imageBytes = Convert.FromBase64String(base64Image.Split(',')[1]);
                using (Image image = Image.FromStream(new MemoryStream(imageBytes)))
                {
                    int tempW = 820;
                    int tempH = 400;
                    if (resizeW > 0)
                    {
                        tempW = resizeW;
                        tempH = resizeH;
                    }

                    Size sizeDefault = new Size(tempW, tempH);
                    var bm = ScaleImage(image, sizeDefault);
                    bm.Save(distination, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        //Thu nhỏ - phóng to ảnh gốc để bằng với size (trung tâm)
        public static Image ScaleImage(Image img, Size sizeDefault)
        {
            if (img == null || sizeDefault.Height <= 0 || sizeDefault.Width <= 0)
            {
                return null;
            }
            int newWidth = (img.Width * sizeDefault.Height) / (img.Height);
            int newHeight = (img.Height * sizeDefault.Width) / (img.Width);
            int x = 0;
            int y = 0;
            Bitmap bmp = new Bitmap(sizeDefault.Width, sizeDefault.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            if (newWidth > sizeDefault.Width)
            {
                x = (bmp.Width - sizeDefault.Width) / 2;
                y = (bmp.Height - newHeight) / 2;
                g.DrawImage(img, x, y, sizeDefault.Width, newHeight);
            }
            else
            {
                x = (bmp.Width / 2) - (newWidth / 2);
                y = (bmp.Height / 2) - (sizeDefault.Height / 2);
                g.DrawImage(img, x, y, newWidth, sizeDefault.Height);
            }
            return bmp;
        }

        public static Image ScopeImage(Image imgToResize, Size destinationSize)
        {
            var originalWidth = imgToResize.Width;
            var originalHeight = imgToResize.Height;

            //how many units are there to make the original length
            var hRatio = (float)originalHeight / destinationSize.Height;
            var wRatio = (float)originalWidth / destinationSize.Width;

            //get the shorter side
            var ratio = Math.Min(hRatio, wRatio);

            var hScale = Convert.ToInt32(destinationSize.Height * ratio);
            var wScale = Convert.ToInt32(destinationSize.Width * ratio);

            //start cropping from the center
            var startX = (originalWidth - wScale) / 2;
            var startY = (originalHeight - hScale) / 2;

            //crop the image from the specified location and size
            var sourceRectangle = new Rectangle(startX, startY, wScale, hScale);

            //the future size of the image
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);

            //fill-in the whole bitmap
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            //generate the new image
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        public static string GenerateImageSignatureValid(string signedBy)
        {
            using (Bitmap bmp = new Bitmap(492, 240))
            {
                using (Graphics grp = Graphics.FromImage(bmp))
                {
                    Font font = new Font("Arial", 22, FontStyle.Regular, GraphicsUnit.Pixel);
                    grp.Clear(Color.White);
                    grp.DrawRectangle(new Pen(Color.Red, 2), 2, 2, bmp.Width - 5, bmp.Height - 5); //Border cha
                    Image bitmap = Image.FromFile(HostingEnvironment.MapPath("~/Images/check.png")); //Ảnh dấu tích xanh
                    grp.DrawImage(bitmap, new Point(bmp.Width - 135, bmp.Height - 125)); //Vẽ ảnh tích xanh trong border tọa độ 0.0

                    string[] watermarkTexts = new string[] { "Signature valid", $"Ký bởi: {signedBy}", $"Ký ngày: {DateTime.Now:dd/MM/yyyy \"GMT\"zzz}" };
                    List<string> lines = new List<string>();
                    foreach (string watermarkText in watermarkTexts)
                    {
                        double lineHeight = font.Height;
                        string temp = string.Empty;
                        string line = string.Empty;
                        float lineLength = 0;
                        string[] elements = watermarkText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < elements.Length; i++)
                        {
                            temp += elements[i] + " ";
                            lineLength = grp.MeasureString(temp, font).Width;

                            // Nếu chiều dài dòng ngắn hơn chiều rộng của khung
                            if (lineLength < bmp.Width - 5)
                            {
                                line = temp;
                            }
                            // Ngược lại nếu dài hơn thì add dòng hiện tại và thêm dòng mới
                            else
                            {
                                lines.Add(line.Trim());
                                temp = elements[i] + " ";
                                line = temp;
                            }

                            //Nếu đây là chữ cuối cùng và 
                            if (i == elements.Length - 1 && line != string.Empty)
                            {
                                lines.Add(line.Trim());
                            }
                        }
                    }
                    int y = 0;
                    foreach (string line in lines)
                    {
                        SizeF textSize = grp.MeasureString(line, font);
                        y += ((int)textSize.Height + 5);
                        Point position = new Point(10, y);

                        Brush brush = new SolidBrush(Color.Red);
                        grp.DrawString(line, font, brush, position);
                    }
                }

                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Png);
                byte[] imageBytes = stream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static bool ConvertImageToPdf(string fullPath, string pathDestination)
        {
            try
            {
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fullPath);
                using (FileStream fs = new FileStream(pathDestination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    iTextSharp.text.Document doc = new iTextSharp.text.Document();
                    doc.SetPageSize(new iTextSharp.text.Rectangle(0, 0, image.Width, image.Height, 0));
                    doc.NewPage();
                    using (PdfWriter writer = PdfWriter.GetInstance(doc, fs))
                    {
                        doc.Open();
                        image.SetAbsolutePosition(0, 0);
                        writer.DirectContent.AddImage(image);
                        doc.Close();
                        fs.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ConfigHelper.Instance.WriteLogException("Lỗi convert image file to PDF file", ex, MethodBase.GetCurrentMethod().Name, "ConvertImageToPdf");
                return false;
            }
        }

        public static bool CopyFolder(string sourceDirectory, string targetDirectory)
        {

            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);
            return CopyAll(diSource, diTarget);
        }

        private static bool CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            bool copySuccess = false;
            try
            {
                Directory.CreateDirectory(target.FullName);
                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
                copySuccess = true;
            }
            catch (Exception ex)
            {
                string msg = "Lỗi khi copy file, folder";
                ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "CopyAll");
                return copySuccess;
            }
            return copySuccess;
        }

        public static void DeleteAllExcept(string folderPath, List<string> except, bool recursive = true)
        {
            var dir = new DirectoryInfo(folderPath);

            var files = dir.GetFiles();

            foreach (var fi in files.Where(n => !except.Contains(n.Name)))
            {
                fi.Delete();
            }

            if (recursive)
            {
                var dirs = dir.GetDirectories();
                foreach (var di in dirs)
                {
                    DeleteAllExcept(di.FullName, except, recursive);
                }
            }
        }

    }
}

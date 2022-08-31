using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iTextSharp.text;

//for search
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;


namespace Plugin.Common
{
    public class EdPdfSearch
    {
        //tham khảo
        //https://stackoverflow.com/questions/41574267/search-for-a-text-in-a-pdf-file-and-return-the-coordinates-if-the-text-exist/41574367
        //https://stackoverflow.com/questions/23909893/getting-coordinates-of-string-using-itextextractionstrategy-and-locationtextextr
        /*
         * To solve that problem here it is the strategy I have used:

        Split chunks in characters (actually textrenderinfo objects per each char)
        Group chars by line. This is not straight forward as you have to deal with chunk alignment.
        Search the word you need to find for each line
         * */
        public string searchSimple(string strsearch, int inpage, string pdfFile)
        {
            try
            {
                //1. Open pdf file
                PdfReader reader = new PdfReader(pdfFile);

                //2. search some text: need extract text page by page and search
                int pagesNo = reader.NumberOfPages;
                string currentPageText = "";
                string pagesFound = "";
                //iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                List<int> pages = new List<int>(); //contain page found
                if (inpage != 0)
                {
                    currentPageText = PdfTextExtractor.GetTextFromPage(reader, inpage, strategy);
                    if (currentPageText.Contains(strsearch))
                    {
                        pagesFound = inpage.ToString("D");
                        pages.Add(inpage);
                    }
                }
                else
                {
                    for (int page = 1; page <= pagesNo; page++)
                    {
                        currentPageText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                        if (currentPageText.Contains(strsearch))
                        {
                            if (pagesFound != "") { pagesFound = pagesFound + ";"; };
                            pagesFound = pagesFound + ";" + page.ToString();
                            pages.Add(page);
                        }
                    };
                }
                //3. Close reader
                reader.Close();

                return pagesFound;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm", ex);
            }
        }

        /// <summary>
        /// search in file already open by PdfReader
        /// </summary>
        /// <param name="strsearch">xâu cần tìm</param>
        /// <param name="inpage">trang tìm, nếu =0: tìm từ đầu đến hết</param>
        /// <param name="reader">biến tham chiếu đén file đã mở để đọc. không có nhiệm vụ close</param>
        /// <returns></returns>

        public string searchInFile(string strsearch, int inpage, string pdfFile)
        {
            PdfReader reader = null;
            try
            {
                //1. Open pdf file
                reader = new PdfReader(pdfFile);


                return searchInReader(strsearch, inpage, ref reader);

            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm kiếm", ex);
            }
            finally
            {
                //3. close
                reader.Close();
            }
        }

        /// <summary>
        /// Hàm tìm kiếm xâu trong file PDF. Ổn hơn hàm ở trên
        /// </summary>
        /// <param name="strsearch">xâu cần tìm</param>
        /// <param name="inpage">trìm trong trang, nếu =0: tìm từ trang 1 đến khi thấy, nếu >0: chỉ tìm trong trang</param>
        /// <param name="reader">obj PDFreader tạo ra khi mở file PDF</param>
        /// <returns></returns>
        public string searchInReader(string strsearch, int inpage, ref PdfReader reader)
        {
            try
            {
                int pagesNo = reader.NumberOfPages;
                string pagesFound = "";
                //PdfStamper stamper;
                //PdfContentByte cb;
                if (inpage > 0)
                {
                    var strategy = new myLocationTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(reader, inpage, strategy);

                    StringComparison sc = StringComparison.CurrentCultureIgnoreCase;
                    List<iTextSharp.text.Rectangle> matchesFound = strategy.GetTextLocations(strsearch, sc);
                    if (matchesFound.Count > 0)
                    {
                        if (pagesFound != "") { pagesFound = pagesFound + ";"; };
                        //có thể tìm thấy nhiều, nhưng chỉ trả lại vị trí đầu tiên :)
                        pagesFound = pagesFound + inpage.ToString("D") + ";" + matchesFound[0].Left.ToString() + ";" + matchesFound[0].Bottom.ToString();
                    };

                }
                else
                {
                    for (int page = 1; page <= pagesNo; page++)
                    {
                        var strategy = new myLocationTextExtractionStrategy();
                        //cb = stamper.GetUnderContent(page);
                        //cb = stamper.GetUnderContent(page);
                        //strategy.UndercontentCharacterSpacing = cb.CharacterSpacing;
                        //strategy.UndercontentHorizontalScaling = cb.HorizontalScaling;
                        string currentText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);

                        StringComparison sc = StringComparison.CurrentCultureIgnoreCase;
                        List<iTextSharp.text.Rectangle> matchesFound = strategy.GetTextLocations(strsearch, sc);
                        if (matchesFound.Count > 0)
                        {
                            if (pagesFound != "") { pagesFound = pagesFound + ";"; };
                            //có thể tìm thấy nhiều, nhưng chỉ trả lại vị trí đầu tiên :)
                            pagesFound = pagesFound + page.ToString("D") + ";" + matchesFound[0].Left.ToString() + ";" + matchesFound[0].Bottom.ToString();
                        };
                    };
                }
                return pagesFound;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }


    //hàm này tìm kiếm ổn hơn
    public class myLocationTextExtractionStrategy : ITextExtractionStrategy
    {
        public float UndercontentCharacterSpacing
        {
            get { return _undercontentCharacterSpacing; }
            set { _undercontentCharacterSpacing = value; }
        }
        private float _undercontentCharacterSpacing;


        public float UndercontentHorizontalScaling
        {
            get { return _undercontentHorizontalScaling; }
            set { _undercontentHorizontalScaling = value; }
        }
        private float _undercontentHorizontalScaling;


        public SortedList<string, DocumentFont> ThisPdfDocFonts { get; private set; }
        // خلاصه ای از هر آنچه تا به حال پیدا شده
        public bool DUMP_STATE;

        private List<TextChunk> locationalResult = new List<TextChunk>();


        public myLocationTextExtractionStrategy()
        {
            ThisPdfDocFonts = new SortedList<string, DocumentFont>();
        }

        public void BeginTextBlock()
        {
        }

        public void EndTextBlock()
        {
        }


        public string GetResultantText()
        {
            if (DUMP_STATE)
            {
                DumpState();

            }
            locationalResult.Sort();

            var sb = new StringBuilder();
            TextChunk lastChunk = null;
            foreach (TextChunk chunk in locationalResult)
            {
                if (lastChunk == null)
                    sb.Append(chunk.text);
                else
                    if (chunk.SameLine(lastChunk))
                {
                    float dist = chunk.DistanceFromEndOf(lastChunk);
                    if (dist < -chunk.charSpaceWidth)
                    {
                        sb.Append(' ');
                        //فقط وقتی جاخالی اضافه کردیم، که حرف آخر کلمه قبلی جای خالی نباشد و حرف اول این کلمه هم جای خالی نباشد
                    }
                    else if (dist > chunk.charSpaceWidth / 2.0f && !StartsWithSpace(chunk.text) && !EndWithSpace(lastChunk.text))
                    {
                        sb.Append(' ');
                    }

                    sb.Append(chunk.text);
                }
                else
                {
                    sb.Append('\n');
                    sb.Append(chunk.text);
                }
                lastChunk = chunk;
            }
            return sb.ToString();
        }

        private bool StartsWithSpace(string str)
        {
            if (str.Length == 0)
            {
                return false;
            }
            return str[0] == ' ';
        }

        internal List<Rectangle> GetTextLocations(string pSearchString, StringComparison pStrComp)
        {
            var foundMatches = new List<iTextSharp.text.Rectangle>();
            var sb = new StringBuilder();
            var thisLineChunks = new List<TextChunk>();
            bool bStart = false, bEnd = false;
            TextChunk firstChunk = null;
            TextChunk lastChunk = null;
            string sTextInUsedChunks = "";

            //foreach (TextChunk chunk in locationalResult)
            for (int j = 0; j < locationalResult.Count; j++)
            {
                TextChunk chunk = locationalResult[j];
                if (chunk.text.Contains(pSearchString))
                    Thread.Sleep(1);
                // کامنت بالا را با این خطها عوض کرده Boris
                if (thisLineChunks.Count > 0 && (!chunk.SameLine(thisLineChunks.Last()) || j == locationalResult.Count - 1)) //را اضافه کردlocationalResult.Countشرط Boris آقای
                {
                    if (sb.ToString().IndexOf(pSearchString, pStrComp) > -1)
                    {
                        string sLine = sb.ToString();

                        // دیدن تعداد تکرار کلمه در این خط
                        int iCount = 0;
                        int lPos;
                        lPos = sLine.IndexOf(pSearchString, 0, pStrComp);
                        while (lPos > -1)
                        {
                            iCount++;
                            if (lPos + pSearchString.Length > sLine.Length)
                                break;
                            else
                                lPos += pSearchString.Length;
                            lPos = sLine.IndexOf(pSearchString, lPos, pStrComp);
                        }
                        // پردازش موارد پیدا شده در این خط
                        int curPos = 0;
                        for (int i = 1; i <= iCount; i++)
                        {
                            string sCurrentText; int iFromChar; int iToChar;
                            iFromChar = sLine.IndexOf(pSearchString, curPos, pStrComp);
                            curPos = iFromChar;
                            iToChar = iFromChar + pSearchString.Length - 1;
                            sCurrentText = "";
                            sTextInUsedChunks = "";
                            firstChunk = null;
                            lastChunk = null;

                            // در این خط، نسبت به  مورد پیدا شده chunkبدست آوردن آخرین و اولین
                            foreach (TextChunk chk in thisLineChunks)
                            {
                                sCurrentText = string.Concat(sCurrentText, chk.text);
                                // را ذخیره کنیم chunkببینیم که آیا وارد این قسمتی که مورد در آن پیدا شده، شدیم بعد آن
                                if (!bStart && sCurrentText.Length - 1 >= iFromChar)
                                {
                                    firstChunk = chk;
                                    bStart = true;
                                }

                                // همچنان مشغول ذخیره کردن کلمات تا زمانی که در این قسمتی که مورد در آن پیدا شده هستیم، باش
                                if (bStart && !bEnd)
                                    sTextInUsedChunks = sTextInUsedChunks + chk.text;//sCurrentText = string.Concat(sCurrentText, chk.text);

                                // ویژه را ذخیره با در جای خاص ذخیره کنchunk اگر از این قسمتی که مورد در آن پیدا شده، خارج شدیم، این
                                if (!bEnd && sCurrentText.Length - 1 >= iToChar)
                                {
                                    lastChunk = chk;
                                    bEnd = true;
                                }

                                // که دربرگیرنده متنی است که کلمه مورد نظر ما درآن است ، chunkاگر ما اولین و آخرین
                                // در آنجا ما نوشته را مشخص میکنیم و کار را تمام میکنیم، GetRectangleFromText پس حالا موقع این است که مستطیل روی آنها رسم کنیم، با استفاده از تابع
                                if (bStart && bEnd)
                                {
                                    foundMatches.Add(GetRectangleFromText(firstChunk, lastChunk, pSearchString, sTextInUsedChunks, iFromChar, iToChar, pStrComp));
                                    curPos += pSearchString.Length;
                                    bStart = bEnd = false; //vtn add https://stackoverflow.com/questions/6523243/how-to-highlight-a-text-or-word-in-a-pdf-file-using-itextsharp/6527010#6527010
                                    break;
                                }
                            }
                        }
                    }
                    sb.Clear();
                    thisLineChunks.Clear();
                }
                thisLineChunks.Add(chunk);
                sb.Append(chunk.text);
            }
            return foundMatches;
        }

        private Rectangle GetRectangleFromText(TextChunk firstChunk, TextChunk lastChunk, string pSearchString, string sTextinChunks, int iFromChar, int iToChar, StringComparison pStrComp)
        {
            // دارای کاراکترهای اضافه در پس و پیشش هست، ما نمیخواهیم کادر مستطیل ما آنها را بپوشاند chunk مواردی دیده شده که یک
            // برای این باید اینطور رشته ها را از چپ و راست کوتاه کنیم. یعنی باید اندازه این بخش های اضافه را حساب کنیم.
            // به واحد های نمایش نداریم text الان در اینجا تبدیل مستقیم از واحدهای
            // حساب میکنیم textرا در واحدهای نمایش کاربر داریم، حالا اندازه را در واحدهای chunkبرای این جور موارد، ما که اندازه
            // TransformationValue=و از مقایسه این دو یک نسبت را برای جاهای مختلف بدست میاوریم
            //  inchواحد نمایش کاربر * 72 =~ 1

            float lineRealWidth = lastChunk.PosRight - firstChunk.PosLeft; //User Space  طول متن درواحد نمایش

            float lineTextWidth = GetStringWidth(sTextinChunks, lastChunk.curFontSize, lastChunk.charSpaceWidth, ThisPdfDocFonts.Values.ElementAt(lastChunk.FontIndex)); // textطول متن در واحد

            float TransformationValue = lineRealWidth / lineTextWidth;

            int iStart = sTextinChunks.IndexOf(pSearchString, pStrComp);

            int iEnd = iStart + pSearchString.Length - 1;

            string sLeft;
            if (iStart == 0)
            {
                sLeft = null;
            }
            else
            {
                sLeft = sTextinChunks.Substring(0, iStart);
            }
            string sRight;
            if (iEnd == sTextinChunks.Length - 1)
            {
                sRight = null;
            }
            else
            {
                sRight = sTextinChunks.Substring(iEnd + 1, sTextinChunks.Length - iEnd - 1);
            }

            //  اندازه گیری بخش چپ اضافه
            float leftWidth = 0;
            if (iStart > 0)
            {
                leftWidth = GetStringWidth(sLeft, lastChunk.curFontSize, lastChunk.charSpaceWidth, ThisPdfDocFonts.Values.ElementAt(lastChunk.FontIndex));
                leftWidth *= TransformationValue;
            }

            //  اندازه گیری بخش راست اضافه
            float rightWidth = 0;
            if (iEnd < sTextinChunks.Length - 1)
            {
                rightWidth = GetStringWidth(sRight, lastChunk.curFontSize, lastChunk.charSpaceWidth, ThisPdfDocFonts.Values.ElementAt(lastChunk.FontIndex));
                rightWidth *= TransformationValue;
            }
            //جمعش با آنچه ما بدست اوردیم، میشود آنچه دنبالش بودیم ، firstChunk.distParallelStart = چپ صفحه marginفاصله تا 
            float leftOffset = firstChunk.distParallelStart + leftWidth;
            // lastChunk.distParallelEnd = راست صفحه marginفاصله تا 
            float rightOffset = lastChunk.distParallelEnd - rightWidth;

            return new Rectangle(leftOffset, firstChunk.PosBottom, rightOffset, firstChunk.PosTop);
        }

        private float GetStringWidth(string str, float curFontSize, float pSingleSpaceWidth, DocumentFont pFont)
        {
            char[] chars = str.ToCharArray();
            float totalWidth = 0;
            float w = 0;
            foreach (char c in chars)
            {
                w = pFont.GetWidth(c) / 1000;
                totalWidth += (w * curFontSize + UndercontentCharacterSpacing) * UndercontentHorizontalScaling / 100;
            }
            return totalWidth;
        }


        private void DumpState()
        {
            foreach (TextChunk location in locationalResult)
            {
                location.PrintDiagnostics();
                Console.WriteLine();
            }
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            // هیچ کاری نکن
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
            LineSegment segment = renderInfo.GetBaseline();
            var location = new TextChunk(renderInfo.GetText(), segment.GetStartPoint(), segment.GetEndPoint(), renderInfo.GetSingleSpaceWidth());
            //chunk ی x,yموقعیت:
            //Debug.Print(renderInfo.GetText());
            Vector horizonCoordinate = renderInfo.GetDescentLine().GetStartPoint();
            Vector verticalRight = renderInfo.GetAscentLine().GetEndPoint();
            location.PosLeft = horizonCoordinate[Vector.I1];
            location.PosRight = verticalRight[Vector.I1];
            location.PosBottom = horizonCoordinate[Vector.I2];
            location.PosTop = verticalRight[Vector.I2];
            //chunk Font Size: (height)
            location.curFontSize = location.PosTop - segment.GetStartPoint()[Vector.I2];
            // SortedList استفاده از ترکیب نام و اندازه فونت به عنوان کلید در
            string strKey = string.Concat(renderInfo.GetFont().PostscriptFontName, location.curFontSize);
            // ThisPdfDocFonts در کلاسSortedListاضافه کردن این فونت به  
            if (!ThisPdfDocFonts.ContainsKey(strKey))
            {
                ThisPdfDocFonts.Add(strKey, renderInfo.GetFont());
            }
            // را ذخیره کنیم تا بعدا بتوانیم استفاده کنیم SortedListایندکس مربوط به
            location.FontIndex = ThisPdfDocFonts.IndexOfKey(strKey);
            locationalResult.Add(location);
        }

        private bool EndWithSpace(string str)
        {
            if (str.Length == 0)
                return false;
            return str[str.Length - 1] == ' ';
        }

        /// <summary>
        /// اش x,y کلمه به همراه  جهت، موقعیت
        /// </summary>
        private class TextChunk : IComparable<TextChunk>
        {
            public TextChunk(string str, Vector startLocation, Vector endLocation, float charSpaceWidth)
            {
                this.text = str;
                this.startLocation = startLocation;
                this.endLocation = endLocation;
                this.charSpaceWidth = charSpaceWidth;
                Vector oVector = endLocation.Subtract(startLocation);
                if (oVector.Length == 0)
                {
                    oVector = new Vector(1, 0, 0);
                }
                orientationVector = oVector.Normalize();
                orientationMagnitude = (int)Math.Truncate(Math.Atan2(orientationVector[Vector.I2], orientationVector[Vector.I1]) * 1000);
                var origin = new Vector(0, 0, 1);
                distPerpendicular = (int)((startLocation.Subtract(origin)).Cross(orientationVector)[Vector.I3]);
                distParallelStart = orientationVector.Dot(startLocation);
                distParallelEnd = orientationVector.Dot(endLocation);
            }

            internal float charSpaceWidth;
            internal float curFontSize { get; set; }
            internal float distParallelEnd;
            internal float distParallelStart;
            internal string text;
            private Vector startLocation;
            private Vector endLocation;
            private Vector orientationVector;
            private int orientationMagnitude;
            private int distPerpendicular;

            public int FontIndex { get; internal set; }
            public float PosBottom { get; internal set; }
            public float PosLeft { get; internal set; }
            public float PosRight { get; internal set; }
            public float PosTop { get; internal set; }

            public int CompareTo(TextChunk rhs)
            {
                if (this.Equals(rhs))
                {
                    return 0;
                }
                int rslt;
                rslt = CompareInts(orientationMagnitude, rhs.orientationMagnitude);
                if (rslt != 0)
                {
                    return rslt;
                }
                rslt = CompareInts(distPerpendicular, rhs.distPerpendicular);
                if (rslt != 0)
                {
                    return rslt;
                }
                rslt = distParallelStart < rhs.distParallelStart ? -1 : 1;
                return rslt;
            }

            private int CompareInts(int int1, int int2)
            {
                return (int1 == int2 ? 0 : (int1 < int2 ? -1 : 1));
            }

            /// <summary>
            /// تا پایین اونیکی (در جهت نوشته محاسبه میشود)‌‏ chunk محاسبه فاصله بین اول
            /// هایی که هم خط و یا هم جهت نیستند خوب نیست، اما ما دیگر به خاطر کند نشدن ، جهت را نمیبینیم.‏ chunkتوجه: صدا کردن این تابع برای
            /// </summary>
            /// <param name="other">اونیکی</param>
            /// <returns></returns>
            internal float DistanceFromEndOf(TextChunk other)
            {
                float distance = distParallelStart - other.distParallelEnd;
                return distance;
            }

            internal void PrintDiagnostics()
            {
                Console.WriteLine("Text (@" + Convert.ToString(startLocation) + " -> " + Convert.ToString(endLocation) + "): " + text);
                Console.WriteLine("orientationMagnitude: " + orientationMagnitude);
                Console.WriteLine("distPerpendicular: " + distPerpendicular);
                Console.WriteLine("distParallel: " + distParallelStart);
            }

            internal bool SameLine(TextChunk a)
            {
                if (orientationMagnitude != a.orientationMagnitude)
                    return false;
                if (distPerpendicular != a.distPerpendicular)
                    return false;

                return true;
            }
        }
    }

}

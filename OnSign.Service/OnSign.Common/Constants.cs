using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.Common
{

    public static class LOG_TYPE
    {
        public const string INFO = "Thông tin";
        public const string ERROR = "Lỗi";
    }

    public static class Constants
    {
        #region Column name

        public const string COL_OUTPUTVOUCHERDETAILID = "OUTPUTVOUCHERDETAILID";
        public const string COL_OUTPUTVOUCHERID = "OUTPUTVOUCHERID";
        public const string COL_PRODUCTID = "PRODUCTID";
        public const string COL_PRODUCTIDREF = "PRODUCTIDREF";
        public const string COL_PRODUCTNAME = "PRODUCTNAME";
        public const string COL_QUANTITY = "QUANTITY";
        public const string COL_QUANTITYUNIT = "QUANTITYUNIT";
        public const string COL_QUANTITYUNITID = "QUANTITYUNITID";
        public const string COL_RETAILPRICE = "RETAILPRICE";
        public const string COL_SALEPRICE = "SALEPRICE";
        public const string COL_NOWSALEPRICE = "NOWSALEPRICE";
        public const string COL_UNITID = "UNITID";
        public const string COL_UNITNAME = "UNITNAME";
        public const string COL_PROMOTIONPROGRAMID = "PROMOTIONPROGRAMID";
        public const string COL_ISPROMOTEDPRODUCT = "ISPROMOTEDPRODUCT";
        public const string COL_APPLYPRODUCTID = "APPLYPRODUCTID";
        public const string COL_APPLYTYPE = "APPLYTYPE";
        public const string COL_VAT = "VAT";
        public const string COL_PROMOTIONGIFTGROUPID = "PROMOTIONGIFTGROUPID";
        public const string COL_CREATEDDATE = "CREATEDDATE";
        public const string COL_CREATEDUSER = "CREATEDUSER";
        public const string COL_UPDATEDUSER = "UPDATEDUSER";
        public const string COL_TOTALAMOUNT = "TOTALAMOUNT";
        public const string COL_ROUNDMONEY = "ROUNDMONEY";
        public const string COL_TOTALVOUCHERDISCOUNT = "TOTALVOUCHERDISCOUNT";
        public const string COL_TOTALAMOUNTBF = "TOTALAMOUNTBF";
        public const string COL_INPUTVOUCHERDETAILID = "INPUTVOUCHERDETAILID";
        public const string COL_INPUTVOUCHERID = "INPUTVOUCHERID";
        public const string COL_PRICE = "PRICE";
        public const string COL_INPUTPRICE = "INPUTPRICE";
        public const string COL_TAXID = "TAXID";
        public const string COL_TAXCODE = "TAXCODE";
        public const string COL_CUSTOMERID = "CUSTOMERID";
        public const string COL_CUSTOMERCODE = "CUSTOMERCODE";
        public const string COL_CUSTOMERNAME = "CUSTOMERNAME";
        public const string COL_CUSTOMERADDRESS = "CUSTOMERADDRESS";
        public const string COL_CUSTOMERPOINT = "CUSTOMERPOINT";
        public const string COL_CUSTOMERPHONE = "CUSTOMERPHONE";
        public const string COL_CUSTOMERPHONENUM = "CUSTOMERPHONENUM";
        public const string COL_CUSTOMERTAXID = "CUSTOMERTAXID";
        public const string COL_MOBILEPHONE = "MOBILEPHONE";
        public const string COL_ISALLOWDECIMAL = "ISALLOWDECIMAL";
        public const string COL_INPUTDATE = "INPUTDATE";
        public const string COL_INVOICEDATE = "INVOICEDATE";
        public const string COL_INVOICEID = "INVOICEID";
        public const string COL_INPUTTYPENAME = "INPUTTYPENAME";
        public const string COL_NOTE = "NOTE";
        public const string COL_INVOICESYMBOL = "INVOICESYMBOL";
        public const string COL_INVOICEPATTERN = "INVOICEPATTERN";
        public const string COL_DISCOUNT = "DISCOUNT";
        public const string COL_DISCOUNTTYPE = "DISCOUNTTYPE";
        public const string COL_DISCOUNTRATIO = "DISCOUNTRATIO";
        public const string COL_USERNAME = "USERNAME";
        public const string COL_STOREID = "STOREID";
        public const string COL_STORENAME = "STORENAME";
        public const string COL_STOREADDRESS = "STOREADDRESS";
        public const string COL_TAXADDRESS = "TAXADDRESS";
        public const string COL_STOREPHONENUM = "STOREPHONENUM";
        public const string COL_STOREFAX = "STOREFAX";
        public const string COL_OUTPUTTYPEID = "OUTPUTTYPEID";
        public const string COL_OUTPUTUSER = "OUTPUTUSER";
        public const string COL_INPUTTYPEID = "INPUTTYPEID";
        public const string COL_VOUCHERCONCERN = "VOUCHERCONCERN";
        public const string COL_SUBGROUPID = "SUBGROUPID";
        //public const string COL_MAINGROUPID = "MAINGROUPID";
        public const string COL_SUBGROUPNAME = "SUBGROUPNAME";
        public const string COL_APPLYDATE = "APPLYDATE";
        public const string COL_STORESHORTNAME = "STORESHORTNAME";
        public const string COL_OUTPUTDATE = "OUTPUTDATE";
        public const string COL_ISCHECKREALINPUT = "ISCHECKREALINPUT";
        public const string COL_CHECKREALINPUTNOTE = "CHECKREALINPUTNOTE";
        public const string COL_CHECKREALINPUTTIME = "CHECKREALINPUTTIME";
        public const string COL_CHECKREALINPUTUSER = "CHECKREALINPUTUSER";
        public const string COL_INPUTUSER = "INPUTUSER";
        public const string COL_MAXSALEQUANTITY = "MAXSALEQUANTITY";
        public const string COL_PERCENTDISCOUNT = "PERCENTDISCOUNT";
        public const string COL_USEDQUANTITY = "USEDQUANTITY";
        public const string COL_ISDELETED = "ISDELETED";
        public const string COL_DELETEDDATE = "DELETEDDATE";
        public const string COL_DELETEDUSER = "DELETEDUSER";
        public const string COL_NEAREXPIREID = "NEAREXPIREID";
        public const string COL_FROMDATE = "FROMDATE";
        public const string COL_TODATE = "TODATE";
        public const string COL_MAINGROUPID = "MAINGROUPID";
        public const string COL_MAINGROUPNAME = "MAINGROUPNAME";
        public const string COL_DESTINATEPRODUCTID = "DESTINATEPRODUCTID";
        public const string COL_EXCHANGEQUANTITY = "EXCHANGEQUANTITY";
        public const string COL_EXCHANGEQUANTITYUNIT = "EXCHANGEQUANTITYUNIT";
        public const string COL_EXCHANGEQUANTITYUNITID = "EXCHANGEQUANTITYUNITID";
        public const string COL_ORDERID = "ORDERID";
        public const string COL_ORDERDATE = "ORDERDATE";
        public const string COL_PROMOTIONID = "PROMOTIONID";
        public const string COL_PROMOTIONNAME = "PROMOTIONNAME";
        public const string COL_ISAPPLYALLSTORE = "ISAPPLYALLSTORE";
        public const string COL_ISAPPLYSAMEVIP = "ISAPPLYSAMEVIP";
        public const string COL_PROMOTIONDISCOUNT = "PROMOTIONDISCOUNT";
        public const string COL_NEAREXPIREDATEDISCOUNT = "NEAREXPIREDATEDISCOUNT";
        public const string COL_EXCHANGEDISCOUNT = "EXCHANGEDISCOUNT";
        public const string COL_EXCHANGEDATE = "EXCHANGEDATE";
        public const string COL_EXCHANGEUSER = "EXCHANGEUSER";
        public const string COL_VOUCHERDISCOUNT = "VOUCHERDISCOUNT";
        public const string COL_VOUCHERCODE = "VOUCHERCODE";
        public const string COL_ISCREATEOUTVOUCHER = "ISCREATEOUTVOUCHER";
        public const string COL_DISCOUNTVALUE = "DISCOUNTVALUE";
        public const string COL_ISDONATEALL = "ISDONATEALL";
        public const string COL_AREAID = "AREAID";
        public const string COL_ISOUTPUTEINVOICE = "ISOUTPUTEINVOICE";
        public const string COL_ISVATZERO = "ISVATZERO";
        public const string COL_ISALLOWSELECTION = "ISALLOWSELECTION";
        public const string COL_PROMOTIONPRICE = "PROMOTIONPRICE";
        public const string COL_PROMOTIONGIFTTYPE = "PROMOTIONGIFTTYPE";
        public const string COL_DESCRIPTION = "DESCRIPTION";
        public const string COL_FULLNAME = "FULLNAME";
        public const string COL_STORECHANGEORDERID = "STORECHANGEORDERID";
        public const string COL_ISPROCESSED = "ISPROCESSED";
        public const string COL_TOSTOREID = "TOSTOREID";
        public const string COL_PERCENTLOSS = "PERCENTLOSS";
        public const string COL_LOTID = "LOTID";
        public const string COL_MINTOTALMONEY = "MINTOTALMONEY";
        public const string COL_MAXTOTALMONEY = "MAXTOTALMONEY";
        public const string COL_NEWSALEPRICE = "NEWSALEPRICE";
        public const string COL_ISUPDATELOCATION = "ISUPDATELOCATION";
        public const string COL_LOCATIONID = "LOCATIONID";
        public const string COL_ZONETYPE = "ZONETYPE";
        public const string COL_ITEMID = "ITEMID";
        public const string COL_ITEMNAME = "ITEMNAME";
        public const string COL_ISONLINEONLY = "ISONLINEONLY";
        public const string COL_MONEYBANKTRANSFER = "MONEYBANKTRANSFER";
        public const string COL_BANKID = "BANKID";
        public const string COL_SHIPPINGCOST = "SHIPPINGCOST";
        public const string COL_TOTALDISCOUNT = "TOTALDISCOUNT";
        public const string COL_REFUNDMONEY = "REFUNDMONEY";
        public const string COL_MONEYCARDVOUCHERID = "MONEYCARDVOUCHERID";
        public const string COL_MONEYCARDPOSID = "MONEYCARDPOSID";
        public const string COL_MONEYCARDID = "MONEYCARDID";
        public const string COL_MONEYCARD = "MONEYCARD";
        public const string COL_CASHVND = "CASHVND";
        public const string COL_OUTPUTTYPENAME = "OUTPUTTYPENAME";
        public const string COL_COMPANYID = "COMPANYID";
        public const string COL_COMPANYTITLE = "COMPANYTITLE";
        public const string COL_MAXINSTOCK = "MAXINSTOCK";
        public const string COL_MINORDERQUANTITY = "MINORDERQUANTITY";
        public const string COL_ORDERTYPE = "ORDERTYPE";
        public const string COL_ORDERTYPENAME = "ORDERTYPENAME";
        public const string COL_TOTALQUANTITY = "TOTALQUANTITY";
        public const string COL_TOTALMONEY = "TOTALMONEY";
        public const string COL_DISCOUNTPERCENT = "DISCOUNTPERCENT";
        public const string COL_ORDERQUANTITY = "ORDERQUANTITY";
        public const string COL_INPUTEDQUANTITY = "INPUTEDQUANTITY";
        public const string COL_OUTPUTEDQUANTITY = "OUTPUTEDQUANTITY";
        public const string COL_CORRESPONDINGMONNEY = "CORRESPONDINGMONNEY";
        public const string COL_DEVISIONCODE = "DEVISIONCODE";
        public const string COL_ISOUTPUT = "ISOUTPUT";
        public const string COL_ISINPUTSTORE = "ISINPUTSTORE";
        public const string COL_PRODUCTSIZE = "PRODUCTSIZE";
        public const string COL_PRODUCTWEIGHT = "PRODUCTWEIGHT";
        public const string COL_OPTIONINFO = "OPTIONINFO";
        public const string COL_SHELFNO = "SHELFNO";
        public const string COL_TRAYNO = "TRAYNO";
        public const string COL_SKUNO = "SKUNO";
        public const string COL_ISREVIEWED = "ISREVIEWED";
        public const string COL_INPUTSTOREID = "INPUTSTOREID";
        public const string COL_INPUTDATEFROM = "INPUTDATEFROM";
        public const string COL_INPUTDATETO = "INPUTDATETO";
        public const string COL_IMAGENAMES = "IMAGENAMES";
        public const string COL_SELLQUANTITY = "SELLQUANTITY";
        public const string COL_WORKSTOREID = "WORKSTOREID";
        public const string COL_REWARD = "REWARD";
        public const string COL_REVENUEMONTHSTORE = "REVENUEMONTHSTORE";

        public const string COL_PARENTTAXCODE = "PARENTTAXCODE";
        public const string COL_PARENTCOMPANYTITLE = "PARENTCOMPANYTITLE";
        public const string COL_PARENTTAXADDRESS = "PARENTTAXADDRESS";
        public const string COL_PARENTCOMPANYPHONE = "PARENTCOMPANYPHONE";

        public const string COL_ISACTIVED = "ISACTIVED";
        public const string COL_STORECHANGEORDERIDS = "STORECHANGEORDERIDS";
        public const string COL_OUTSTOREID = "OUTSTOREID";
        public const string COL_IMAGELOCATION = "IMAGELOCATION";
        //public const string COL_ = "";

        #endregion

        #region Extend name

        public const string SUFFIX_IT_SUPPORT = ". Vui lòng liên hệ IT để được hỗ trợ";
        public const string SUFFIX_LIST = "LIST";
        public const string COL_BARCODE = "BARCODE";
        public const string COL_BASEPRODUCTID = "BASEPRODUCTID";

        public const string CAPTION_NO = "STT";
        public const string CAPTION_ERRORNOTE = "Ghi chú";

        public const string COL_TMP = "TMP";
        public const string COL_TOTAL = "TOTAL";
        public const string COL_ISSELECT = "ISSELECT";
        public const string COL_ISUPDATED = "ISUPDATED";
        public const string COL_PRINT = "PRINT";
        public const string COL_SELECT = "SELECT";
        public const string COL_ISERROR = "ISERROR";
        public const string COL_ERRORNOTE = "ERRORNOTE";
        public const string COL_NO = "NO";
        public const string COL_ID = "ID";
        public const string COL_VATOFTOTALAMOUNT = "VATOFTOTALAMOUNT";
        public const string COL_OLDSALEPRICE = "OLDSALEPRICE";
        public const string COL_SALEPRICEORGINAL = "SALEPRICEORGINAL";
        public const string COL_TOTALENDQUANTITY = "TOTALENDQUANTITY";
        public const string COL_TOTALQUANTITYEXCHANGE = "TOTALQUANTITYEXCHANGE";
        public const string COL_TOTALAMOUNTDISCOUNT = "TOTALAMOUNTDISCOUNT";
        public const string COL_ITEMQUANTITY = "ITEMQUANTITY";
        public const string COL_RETAILPRICEVAT = "RETAILPRICEVAT";
        public const string COL_SALEPRICEVAT = "SALEPRICEVAT";
        public const string COL_SALEPRICEMERGE = "SALEPRICEMERGE";
        public const string COL_TOTALAMOUNTVAT = "TOTALAMOUNTVAT";
        public const string COL_APPLYPRODUCT = "APPLYPRODUCT";
        public const string COL_QUANTITYNEEDPRODUCT = "QUANTITYNEEDPRODUCT";
        public const string COL_APPLYPRODUCTQUANTITY = "APPLYPRODUCTQUANTITY";
        public const string COL_APPLYSUBGROUP = "APPLYSUBGROUP";
        public const string COL_QUANTITYNEEDSUBGROUP = "QUANTITYNEEDSUBGROUP";
        public const string COL_SALEPRICEAFTERDISCOUNT = "SALEPRICEAFTERDISCOUNT";

        public const string COL_TOTALAMOUNTROUND = "TOTALAMOUNTROUND";
        public const string COL_COMPANYNAME = "COMPANYNAME";
        public const string COL_WEBSITE = "WEBSITE";

        public const string COL_APPLYSUBGROUPQUANTITY = "APPLYSUBGROUPQUANTITY";
        public const string COL_APPLYSUBGROUPID = "APPLYSUBGROUPID";
        public const string COL_VATOFOUTPUT = "VATOFOUTPUT";
        public const string COL_VATOFINPUT = "VATOFINPUT";
        public const string COL_NOTAPPLYPROMOTION = "NOTAPPLYPROMOTION";
        public const string COL_MAXQUANTITY = "MAXQUANTITY";
        public const string COL_NUMBEROFPROMOTION = "DIV";
        public const string COL_PROMOTIONTYPE = "PROMOTIONTYPE";
        public const string COL_APPLYDATEDISPLAY = "APPLYDATEDISPLAY";
        public const string COL_SHOWDETAIL = "SHOWDETAIL";
        public const string COL_SEARCHTYPEID = "SEARCHTYPEID";
        public const string COL_SEARCHTYPENAME = "SEARCHTYPENAME";
        public const string COL_IDLIST = "IDLIST";
        public const string COL_ISACCEPTEDID = "ISACCEPTEDID";
        public const string COL_ISACCEPTEDNAME = "ISACCEPTEDNAME";
        public const string COL_TOTALQUANTITYITEM = "TOTALQUANTITYITEM";
        public const string COL_STATUSID = "STATUSID";
        public const string COL_STATUSNAME = "STATUSNAME";
        public const string COL_STATUS = "STATUS";
        public const string COL_INPUTPRICEOFPO = "INPUTPRICEOFPO";

        public const string COL_STORECHANGEID = "STORECHANGEID";
        public const string COL_REALSTORECHANGEID = "REALSTORECHANGEID";
        public const string COL_STORECHANGEDATE = "STORECHANGEDATE";
        public const string COL_REQUESTUSERFULLNAME = "REQUESTUSERFULLNAME";
        public const string COL_REQUESTTYPENAME = "REQUESTTYPENAME";
        public const string COL_CREATEUSERFULLNAME = "CREATEUSERFULLNAME";
        public const string COL_STORECHANGEUSERFULLNAME = "STORECHANGEUSERFULLNAME";
        public const string COL_TRANPORTTYPENAME = "TRANPORTTYPENAME";
        public const string COL_FROMSTORENAME = "FROMSTORENAME";
        public const string COL_FROMSTOREPHONE = "FROMSTOREPHONE";
        public const string COL_FROMSTOREFAX = "FROMSTOREFAX";
        public const string COL_TOSTORENAME = "TOSTORENAME";
        public const string COL_TOSTOREPHONE = "TOSTOREPHONE";
        public const string COL_TOSTOREFAX = "TOSTOREFAX";
        public const string COL_IMEI = "IMEI";
        public const string COL_INQUANTITY = "INQUANTITY";
        public const string COL_OUTQUANTITY = "OUTQUANTITY";
        public const string COL_QUANTITYDEVISION = "QUANTITYDEVISION";
        public const string COL_SPECIALPRICEPERCENT = "SPECIALPRICEPERCENT";
        public const string COL_ISSALEOFF = "ISSALEOFF";
        public const string COL_APPLYPRODUCTIDREF = "APPLYPRODUCTIDREF";
        public const string COL_IMAGEDEVISIONCODE = "IMAGEDEVISIONCODE";
        public const string COL_ISMAINQUANTITYUNIT = "ISMAINQUANTITYUNIT";
        public const string COL_QUANTITYINPUTED = "QUANTITYINPUTED";
        public const string COL_LOTIDLIST = "LOTIDLIST";
        public const string COL_EXCHANGE = "EXCHANGE";
        public const string COL_ISNOTEXISTINPO = "ISNOTEXISTINPO";
        public const string COL_PRODUCTINPO = "PRODUCTINPO";
        public const string COL_PICKEDQUANTITY = "PICKEDQUANTITY";
        public const string COL_LOCATION = "LOCATION";
        public const string COL_POSITIONNAME = "POSITIONNAME";
        public const string COL_TOTALHOUR = "TOTALHOUR";
        public const string COL_OUTPUTSTOREID = "OUTPUTSTOREID";

        public const string COL_TOTALAMOUNTNONEVAT = "TOTALAMOUNTNONEVAT";
        public const string COL_DISCOUNTMONEY = "DISCOUNTMONEY";
        public const string COL_ORDERDISCOUNTASSIGN = "ORDERDISCOUNTASSIGN";

        public const string COL_TOTALBEGINQUANTITY = "TOTALBEGINQUANTITY";
        public const string COL_TOTALINPUTQUANTITY = "TOTALINPUTQUANTITY";
        public const string COL_TOTALOUTPUTQUANTITY = "TOTALOUTPUTQUANTITY";
        public const string COL_TOTALSTOCKQUANTITY = "TOTALSTOCKQUANTITY";

        public const string COL_LOCATIONBARCODE = "LOCATIONBARCODE";
        public const string COL_IMAGELOCATIONBARCODE = "IMAGELOCATIONBARCODE";
        public const string COL_QRCODE = "QRCODE";
        public const string COL_IMGPATH = "IMGPATH";

        public const string COL_MINTOTALMONEYAPPLY = "MINTOTALMONEYAPPLY";
        public const string COL_MAXTOTALMONEYAPPLY = "MAXTOTALMONEYAPPLY";
        public const string COL_EXTRAAPPLYSUBGROUPID = "EXTRAAPPLYSUBGROUPID";
        public const string COL_EXTRAAPPLYPRODUCTID = "EXTRAAPPLYPRODUCTID";
        public const string COL_QUANTITYPEROBJECT = "QUANTITYPEROBJECT";
        public const string COL_APPLYPRODUCTMAXQUANTITY = "APPLYPRODUCTMAXQUANTITY";
        public const string COL_APPLYSUBGROUPMAXQUANTITY = "APPLYSUBGROUPMAXQUANTITY";
        public const string COL_DISPLAY = "DISPLAY";
        public const string COL_SALEPRICEOFOUTPUTVOUCHER = "SALEPRICEOFOUTPUTVOUCHER";
        public const string COL_RETURNINPUTTYPEID = "RETURNINPUTTYPEID";
        public const string COL_REALPRODUCTNAME = "REALPRODUCTNAME";

        public const string COL_TOTALAMOUNTALL = "TOTALAMOUNTALL";
        public const string COL_INSTOREID = "INSTOREID";
        public const string COL_ISFINISH = "ISFINISH";
        public const string COL_ISINPUTING = "ISINPUTING";
        public const string COL_ENDCASH = "ENDCASH";
        public const string COL_SENDBANKMONEY = "SENDBANKMONEY";
        public const string COL_STOREGROUPNAME = "STOREGROUPNAME";

        public const string GiftProductPrefix = "   Tặng: ";
        public const string SaleGiftProductPrefix = "   Bán kèm: ";
        public const int NumberAfterDecimalSymbol = 3;
        /// <summary>
        /// Ký hiệu đầu tiên của tem cân
        /// </summary>
        public const string PREFIX_BARCODE_PRICE = "21";
        /// <summary>
        /// Chiều dài của tem cân
        /// </summary>
        public const int LENGTH_BARCODE_PRICE = 13;
        /// <summary>
        /// Chiều dài maximum của mã PLU
        /// </summary>
        public const int LENGTH_BARCODE_FRESH = 5;
        /// <summary>
        /// Dòng chứa giá đã trừ khuyến mãi, vd: 65k giảm 49k thì là dòng 49k
        /// </summary>
        public const string COL_ISBREAKPROMOTION = "ISBREAKPROMOTION";
        /// <summary>
        /// Số lượng của sản phẩm (Sum số lượng của những dòng tách)
        /// </summary>
        public const string COL_QUANTITYTOTAL = "QUANTITYTOTAL";
        /// <summary>
        /// Mã khách hàng mặc định
        /// </summary>
        public const string CUSTOMER_DEFAULT_ID = "SAB0";
        /// <summary>
        /// Mã nhóm hàng dùng để khai báo tặng quà không bán (giá 0 đồng trên hóa đơn)
        /// </summary>
        public const int SUBGROUP_ZERO_PRICE = 30;
        /// <summary>
        /// Mã OrgiginalStoreID khi lưu trên web
        /// </summary>
        public const int ORIGINAL_STOREID = 1023;
        #endregion

        #region File name
        public const string FILE_CONNECTION = "connection.ini";
        public const string FILE_STORE = "store.ini";
        public const string FILE_ENCRYPTKEY = "key.ini";
        public const string FILE_ISMAINPOS = "ismainpos.ini";
        public const string FILE_PRICECHANGED = "pricechangedpath.ini";
        public const string THUMB_EXTENSION = ".png";
        public const string SMALL_THUMB = "small_thumb";
        public const string BIG_THUMB = "big_thumb";
        public const string PDF_EXTENSION = ".pdf";
        public const string PDF_SIGNED_EXTENSION = "_signed.pdf";
        public const string TEMPLATE_MAILTYPE_INVITATION = "INVITATION";
        public const string TEMPLATE_MAILTYPE_DOCUMENT = "DOCUMENT";
        public const string TEMPLATE_MAILTYPE_FORGOT_PASSWORD = "FORGOT_PASSWORD";
        #endregion

        public static readonly List<string> LIST_FIRST_NUMBER_OF_PHONE = new List<string> { "03", "05", "07", "08", "09" };

        #region request sign type
        public const string REQUESTSIGNTYPE_ONSIGN = "OnSign";
        public const string REQUESTSIGNTYPE_HSM = "HSM/USB";
        public const string REQUESTSIGNTYPE_ISCC = "ISCC";
        #endregion

        #region Helper

        #endregion

        #region Config key

        public const string CKEY_CONNECTIONDS = "ConnectionStringDS";

        #endregion

        #region Document


        #endregion

        #region SigningDocumentEvent
        public const string EMAIL_SUBJECT_SIGN = "OnSign {0} - {1}:  {2}";

        public const string EMAIL_SUBJECT_SIGNED_FINISH = "OnSign {0} - Hoàn thành: Tài liệu của bạn đã được xem xét và ký";
        public const string EMAIL_SUBJECT_SIGN_WAIT_ONE = "OnSign {0} - Chờ ký: Vui lòng xem xét và ký vào tài liệu";

        public const string DOCUMENT_MESSAGE_FINISH = "Tài liệu đã được ký thông qua OnSign";
        public const string DOCUMENT_MESSAGE_ISCC_SELF = "Bạn nhận được một bản sao";
        public const string DOCUMENT_MESSAGE_ISCC_ONE = "<strong>{0}</strong>(<small style=\"color: yellow;\">{1}</small>) đã gửi cho bạn một bản sao.";

        public const string DOCUMENT_MESSAGE_SIGN_ONLY_ME = "Bạn đã ký một tài liệu thông qua OnSign.";
        public const string DOCUMENT_MESSAGE_SIGN_ONE_SENT = "<strong>{0}</strong>(<small style=\"color: yellow\">{1}</small>) đã gửi tài liệu cho bạn để xem xét và ký tên.";
        public const string DOCUMENT_MESSAGE_DECLINE_SIGN = "<strong>{0}</strong>(<small style=\"color: yellow\">{1}</small>) đã từ chối ký tài liệu.<br />Lý do từ chối: <strong>{2}</strong>";
        public const string DOCUMENT_MESSAGE_CANCEL_SIGN = "<strong>{0}</strong> đã hủy bỏ tài liệu<br />Lý do hủy bỏ: <strong>{1}</strong>";
        #endregion

        #region Invitation
        public const string INVITE_SUBJECT = "OnSign: {0} đã gửi cho bạn một lời mời tham gia thành viên công ty.";
        public const string INVITE_MESSAGE = "{0} địa chỉ email {1} đã gửi cho bạn một lời mời tham gia thành viên công ty. Để xác nhận vui lòng ấn vào liên kết dưới đây";
        #endregion

        #region URL IMG
        public const string SIGN_ICON = "/content/Libraries/pdf.js/img/sign-icon.svg?v=1";
        public const string INITIAL_ICON = "/content/Libraries/pdf.js/img/signimage.svg?v=2";
        #endregion

        #region DOCUMENT STATUS
        public const string DOCUMENT_FAILED = "FAILED";
        public const string DOCUMENT_RENDERING = "RENDERING";
        public const string DOCUMENT_COMPLETED = "COMPLETED";
        #endregion
    }
}

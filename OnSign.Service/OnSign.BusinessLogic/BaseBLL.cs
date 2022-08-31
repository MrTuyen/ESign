using Newtonsoft.Json;
using OnSign.BusinessLogic.CommonBL;
using OnSign.BusinessObject.Document;
using OnSign.BusinessObject.Output;
using OnSign.Common.Helpers;
using SAB.Library.Core.FileService;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OnSign.BusinessLogic
{
    public class BaseBLL
    {
        protected PdfToImage pdfToImage;

        public BaseBLL()
        {
            if (pdfToImage == null)
                pdfToImage = new PdfToImage();
        }
        #region Fields
        protected ResultMessageBO objResultMessageBO = new ResultMessageBO();
        #endregion

        #region Properties

        public ResultMessageBO ResultMessageBO
        {
            get { return objResultMessageBO; }
            set { objResultMessageBO = value; }
        }

        public string ErrorMsg { get; protected set; }

        public string NameSpace
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Namespace; }
        }
        #endregion

        #region Method

        #endregion

        public static string GenerateLinkViewer(string statusRequest, string emailReceiver, long idRequest, bool isCCReceiver, int signIndex = 0, long idEmail = 0)
        {
            string param = $"s={EncryptDecryptHelper.EncryptQueryString(statusRequest)}" +
                $"&e={EncryptDecryptHelper.EncryptQueryString(emailReceiver)}" +
                $"&i={EncryptDecryptHelper.EncryptQueryString(idRequest.ToString())}" +
                $"&c={EncryptDecryptHelper.EncryptQueryString(isCCReceiver.ToString())}" +
                $"&p={EncryptDecryptHelper.EncryptQueryString(signIndex.ToString())}" +
                $"&t={EncryptDecryptHelper.EncryptQueryString(DateTime.Now.ToString("yyyyMMddHHmmss"))}";
            return $"{ConfigHelper.HostEmail}?s={Convert.ToBase64String(Encoding.UTF8.GetBytes(param))}";
        }

        public static LinkViewerBO DecodeLinkViewer(string dataBase64)
        {
            LinkViewerBO descriptionData = new LinkViewerBO();
            var base64ToBytes = Convert.FromBase64String(dataBase64);
            var base64BytesToString = Encoding.UTF8.GetString(base64ToBytes);
            var stringToObject = HttpUtility.ParseQueryString(base64BytesToString);
            var json = JsonConvert.SerializeObject(stringToObject.Cast<string>().ToDictionary(k => k, v => stringToObject[v]));
            var respObj = JsonConvert.DeserializeObject<StarBuildParams>(json);
            try
            {
                descriptionData = new LinkViewerBO
                {
                    id = string.IsNullOrEmpty(respObj.i) ? 0 : long.Parse(EncryptDecryptHelper.DecryptQueryString(respObj.i)),
                    iscc = !string.IsNullOrEmpty(respObj.c) && bool.Parse(EncryptDecryptHelper.DecryptQueryString(respObj.c)),
                    signIndex = string.IsNullOrEmpty(respObj.p) ? 0 : int.Parse(EncryptDecryptHelper.DecryptQueryString(respObj.p)),
                    status = EncryptDecryptHelper.DecryptQueryString(respObj.s).Trim().Replace("\u0000", ""),
                    email = EncryptDecryptHelper.DecryptQueryString(respObj.e).Trim().Replace("\u0000", "").ToLower(),
                    idEmail = string.IsNullOrEmpty(respObj.i_e) ? 0 : long.Parse(EncryptDecryptHelper.DecryptQueryString(respObj.i_e)),
                };
            }
            catch (Exception)
            {
            }
            return descriptionData;
        }

        public static string GenerateLinkInvitation(long invitationId, int createdByUser, string createdByUserName, string createdByUserEmail, string emailTo)
        {
            string param = string.Format("i={0}&u={1}&n={2}&e={3}&m={4}",
                    EncryptDecryptHelper.EncryptQueryString(invitationId.ToString()),
                    EncryptDecryptHelper.EncryptQueryString(createdByUser.ToString()),
                    EncryptDecryptHelper.EncryptQueryString(createdByUserName),
                    EncryptDecryptHelper.EncryptQueryString(createdByUserEmail),
                    EncryptDecryptHelper.EncryptQueryString(emailTo)
                );
            return $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}/invitation?i={Convert.ToBase64String(Encoding.UTF8.GetBytes(param))}";
        }

        public static ExpandoObject DecodeLinkInvitation(string dataBase64)
        {
            dynamic descriptionData = new ExpandoObject();
            try
            {
                var descriptionString = Encoding.UTF8.GetString(Convert.FromBase64String(dataBase64));
                var dict = HttpUtility.ParseQueryString(descriptionString);
                var json = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
                var respObj = JsonConvert.DeserializeObject<InvitationBuildParams>(json);
                descriptionData = new ExpandoObject();
                descriptionData.invitationid = int.Parse(EncryptDecryptHelper.DecryptQueryString(respObj.i));
                descriptionData.createdbyuser = int.Parse(EncryptDecryptHelper.DecryptQueryString(respObj.u));
                descriptionData.createdbyusername = EncryptDecryptHelper.DecryptQueryString(respObj.n).Trim().Replace("\u0000", "").ToLower();
                descriptionData.createdbyuseremail = EncryptDecryptHelper.DecryptQueryString(respObj.e).Trim().Replace("\u0000", "").ToLower();
                descriptionData.emailto = EncryptDecryptHelper.DecryptQueryString(respObj.m).Trim().Replace("\u0000", "").ToLower();
            }
            catch { }
            return descriptionData;
        }


    }
}
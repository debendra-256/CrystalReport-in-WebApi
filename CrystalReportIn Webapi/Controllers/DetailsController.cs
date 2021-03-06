﻿using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CrystalReportIn_Webapi.Models;
using System.IO;
using CrystalDecisions.Shared;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace CrystalReportIn_Webapi.Controllers
{
    [RoutePrefix("api/Details")]
    public class DetailsController : ApiController
    {
        CodeXEntities cX = new CodeXEntities();

        [AllowAnonymous]
        [Route("Report/SendReport")]
        [HttpPost]
        public HttpResponseMessage ExportReport(Users user)
        {
            string EmailTosend = WebUtility.UrlDecode(user.Email);
            List<Users> model = new List<Users>();
            var data = cX.tbl_Registration; 
            var rd = new ReportDocument();
            
            foreach (var details in data)
            {
                Users obj = new Users();
                obj.Email = details.Email;
                obj.FirstName = details.FirstName;
                obj.LastName = details.LastName;
                model.Add(obj);

            }

            rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports"), "UserRegistration.rpt"));
            ConnectionInfo connectInfo = new ConnectionInfo()
            {
                ServerName = "Debendra",
                DatabaseName = "CodeX",
                UserID = "sa",
                Password = "123"
            };
            rd.SetDatabaseLogon("sa", "123");
            foreach (Table tbl in rd.Database.Tables)
            {
                tbl.LogOnInfo.ConnectionInfo = connectInfo;
                tbl.ApplyLogOnInfo(tbl.LogOnInfo);
            }
            rd.SetDataSource(model);
            using (var stream = rd.ExportToStream(ExportFormatType.PortableDocFormat))
            {
                SmtpClient smtp = new SmtpClient
                {
                    Port = 587,
                    UseDefaultCredentials = true,
                    Host = "smtp.gmail.com",
                    EnableSsl = true
                };

                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("debendra256@gmail.com", "9853183297");
                var message = new System.Net.Mail.MailMessage("debendra256@gmail.com", EmailTosend, "User Registration Details", "Hi Please check your Mail  and find the attachement.");
                message.Attachments.Add(new Attachment(stream, "UsersRegistration.pdf"));

                smtp.Send(message);
            }

            var Message = string.Format("Report Created and sended to your Mail.");
            HttpResponseMessage response1 = Request.CreateResponse(HttpStatusCode.OK, Message);
            return response1;
        }



        [Route("user/PostUserImage")]

        public async Task<HttpResponseMessage> PostUserImage()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {

                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {

                           // YourModelProperty.imageurl = userInfo.email_id + extension;
                            //  where you want to attach your imageurl

                            //if needed write the code to update the table

                            var filePath = HttpContext.Current.Server.MapPath("~/Userimage/" +postedFile.FileName + extension);
                            //Userimage myfolder name where i want to save my image
                            postedFile.SaveAs(filePath);

                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Request.CreateErrorResponse(HttpStatusCode.Created, message1); ;
                }
                var res = string.Format("Please Upload a image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                var res = string.Format("some Message");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }
    }
}

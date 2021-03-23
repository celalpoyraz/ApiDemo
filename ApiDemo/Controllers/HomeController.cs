using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApiDemo.Models;
using RestSharp;
using ApiDemo.Model;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Ionic.Zip;
using ApiDemo.Context.Entity;
using ApiDemo.Context;

namespace ApiDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        static RestClient Client = null;
        public static string accessToken = string.Empty;
        public static string EInvoiceUUID = System.Guid.NewGuid().ToString(); // Örnek "198725EA-9E40-4503-996D-7D9ACD6B97AC";
        public static string EArchiveInvoiceUUID = System.Guid.NewGuid().ToString(); // Örnek "65BF1A86-A2E8-4475-9B7B-806409BBC277";
        public static string CustomerVknTckn = "1234567801";
        public static string CustomerAlias = "urn:mail:defaultpk@nesbilgi.com.tr";
        static RestRequest Request = null;

        public const string BaseUrl = "http://apitest.nesbilgi.com.tr/";
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            Client = new RestClient(BaseUrl);
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            TokenRequest model = new TokenRequest();
            model.username = "test01@nesbilgi.com.tr";
            model.password = "V9zH7Hh55LIl";
            GetToken(model);
            var tokenResult = GetToken(new TokenRequest() { username = "test01@nesbilgi.com.tr", password = "V9zH7Hh55LIl" });
            if (tokenResult.ErrorStatus == null) { accessToken = tokenResult.Result.access_token; }

            var getUnAnsweredInvoiceUUIDListResponse = GetUnAnsweredInvoiceUUIDListRequest(); 

            var getUBLXmlContentResponse = GetUBLXmlContentRequest("53ede91b-25c9-45bb-9012-9bae57593263");

            XmlModel xmlModel;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(getUBLXmlContentResponse);
            XmlNode node2 = doc.DocumentElement.ChildNodes[14].ChildNodes[1];
            XmlNode node3 = doc.DocumentElement.ChildNodes[15].ChildNodes[0].ChildNodes[5];
            XmlNode node4 = doc.DocumentElement.ChildNodes[16].ChildNodes[0];
            XmlNode node5 = doc.DocumentElement.ChildNodes[16].ChildNodes[0].ChildNodes[4];
            xmlModel = new XmlModel()
            {
                WebSiteUrl = node2.ChildNodes[0].InnerText,
                PartyIdentifacition = node2.ChildNodes[1].InnerText,
                PartyName = node2.ChildNodes[2].InnerText,
                PostalAddress = node2.ChildNodes[3].InnerText,
                PartyTaxSchema = node2.ChildNodes[4].InnerText,
                Phone = node3.ChildNodes[0].InnerText,
                Fax = node3.ChildNodes[1].InnerText,
                Mail = node3.ChildNodes[2].InnerText,
                CompanyId = node4.ChildNodes[0].InnerText,
                CompanyName = node4.ChildNodes[1].InnerText,
                CompanyAddress = node4.ChildNodes[2].InnerText,
                CompanyTax = node4.ChildNodes[3].InnerText,
                CompanyPhone = node5.ChildNodes[0].InnerText,
                CompanyFax = node5.ChildNodes[1].InnerText

            };
            _context.DbXml.Add(xmlModel);
            _context.SaveChanges();

            foreach (var item in getUnAnsweredInvoiceUUIDListResponse.Result)
            {
                Invoice invoice1 = new Invoice
                {
                    Description = GetInvoiceHtmlRequest(item)
                };
                _context.Invoices.Add(invoice1);
                _context.SaveChanges();
            }
            ViewBag.data = GetInvoiceHtmlRequest("53ede91b-25c9-45bb-9012-9bae57593263");
            return View(xmlModel);
        }
      

        public static GeneralResponse<TokenResponse> GetToken(TokenRequest model)
        {
            var request = new RestRequest("/token", Method.POST);
            request.AddHeader("Content-Type", "application/json"); //istek data tipi
            request.AddParameter("grant_type", "password"); //auth servisi için sabit bu değerin kullanılması gerekmektedir.
            request.AddParameter("username", model.username); //kullanıcı adı
            request.AddParameter("password", model.password); //şifre

            var response = Client.Execute<TokenResponse>(request);
            return new GeneralResponse<TokenResponse>()
            {
                ErrorStatus = response.ErrorException != null ? new Status() { Code = (int)response.StatusCode, Message = response.ErrorException.Message } : null,
                Result = response.Data
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public static RestRequest SetHeaders(string apiPath, string contentType = "application/json")
        {
            Request = new RestRequest();
            Request.Resource = apiPath;
            Request.AddHeader("Content-Type", contentType);
            Request.AddHeader("Authorization", "bearer " + accessToken);
            Request.RequestFormat = DataFormat.Json;
            return Request;
        }

        public static GeneralResponse<List<string>> GetUnAnsweredInvoiceUUIDListRequest()
        {
            var request = SetHeaders("/einvoice/unAnsweredUUIDList");
            request.Method = Method.GET;
            var response = Client.Execute(request);
            return response.Parse<List<string>>();
        }

        public static string GetUBLXmlContentRequest(string invoiceUuid)
        {
            var request = SetHeaders($"/invoicegeneral/ublXmlContent/{invoiceUuid}", "application/xml");
            request.Method = Method.GET;
            var response = Client.Execute(request);
            var responseData = response.Parse<byte[]>();
            string xmlContent = System.Text.Encoding.UTF8.GetString(responseData.Result);
            return xmlContent;
        }
        public static string GetInvoiceHtmlRequest(string invoiceUuid)
        {
            string htmlContent = "";
            try
            {
                var request = SetHeaders($"/invoicegeneral/html/{invoiceUuid}", "text/html");
                request.Method = Method.GET;
                var response = Client.Execute(request);
                var responseData = response.Parse<byte[]>();
                htmlContent = System.Text.Encoding.UTF8.GetString(responseData.Result);

            }
            catch (Exception)
            {
                htmlContent = invoiceUuid + "boş fatura";

            }
            return htmlContent;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Security;
using System.Configuration;
using Microsoft.Rest;
using Microsoft.PowerBI.Api.V1.Models;
using System.Web.Script.Serialization;
using System.Net;

namespace PBIE_RLS_Sample
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private static string workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
        private static string workspaceId = ConfigurationManager.AppSettings["powerbi:WorkspaceId"];
        private static string accessKey = ConfigurationManager.AppSettings["powerbi:AccessKey"];
        private static string apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
        private static string reportID = ConfigurationManager.AppSettings["powerbi:ReportId"];


        private IPowerBIClient CreatePowerBIClient()
        {
            var credentials = new TokenCredentials(accessKey, "AppKey");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(apiUrl)
            };
            return client;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                renderReport();
            }
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            selectedUser.Value = ((LinkButton)sender).Text;
            renderReport();
        }

        private WebRequest CreateRequest()
        {
            return WebRequest.Create(string.Format("https://api.powerbi.com/v1.0/collections/{0}/workspaces/{1}/Reports", workspaceCollection, workspaceId));
        }

        protected void renderReport()
        {
            Report report;
            var request = this.CreateRequest();
            request.Method = "GET";
            request.ContentLength = 0;
            request.Headers.Add("Authorization", String.Format("AppKey {0}", accessKey));

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    var serializer = new JavaScriptSerializer();
                    var reports = serializer.Deserialize<ODataResponseListReport>(reader.ReadToEnd());
                    report = reports.Value.FirstOrDefault(r => r.Id == reportID);

                    string myUserID = ddlUser.SelectedValue;
                    var embedToken = (myUserID != "" ? PowerBIToken.CreateReportEmbedToken(workspaceCollection, workspaceId, report.Id, myUserID, new string[] { "Participante" }) : PowerBIToken.CreateReportEmbedToken(workspaceCollection, workspaceId, report.Id, "0", null));
                    string myTok = embedToken.Generate(accessKey);

                    accessTokenText.Value = myTok;  //input on the report page.
                    embedUrlText.Value = "https://embedded.powerbi.com/appTokenReportEmbed?reportId=" + report.Id; //input on the report page.
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            renderReport();
        }
    }
}
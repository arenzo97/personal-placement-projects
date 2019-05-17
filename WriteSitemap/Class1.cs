
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

using System.Web;
using System.ComponentModel;

namespace CreatingXML.TimerJob
{
    public class TimerClass : SPJobDefinition
    {
        public TimerClass() : base() { }
        public TimerClass(string jobName, SPService service) :
            base(jobName, service, null, SPJobLockType.None)
        {
            this.Title = "A-Sitemap";
        }
        public TimerClass(string jobName, SPWebApplication webApplication) :
            base(jobName, webApplication, null, SPJobLockType.ContentDatabase)
        {
            this.Title = "A-Sitemap";
        }
        string SetXmlString(string elementUrl, string elementDateTime, int index)
        {
            var xmlElement = new StringBuilder();
            string xmlString = xmlElement.AppendLine("  <url>")
                                         .AppendLine("    <loc>" + elementUrl + "</loc>")
                                         .AppendLine("    <lastmod>" + elementDateTime + "</lastmod>")
                                         .AppendLine("  </url>")
                                         .ToString();
            return xmlString;
        }

        string ReturnPagesLibraryListElements(string listName)
        {
            var xmlStringBuilder = new StringBuilder();
            string formatString = "yyyy-MM-dd";
            string theURL = this.WebApplication.Sites[0].Url.ToString();
            int index = 1;
            string xmlItemElement = "";

            Microsoft.SharePoint.SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite elavatedSiteSuite = new SPSite(this.WebApplication.Sites[0].Url))
                {
                    using (SPWeb elavatedWebSuite = elavatedSiteSuite.OpenWeb())
                    {
                        SPList spList = elavatedWebSuite.Lists[listName];

                        SPQuery query = new SPQuery();
                        query.Query = "<Where><Eq><FieldRef Name='ExcludeSitemap' /><Value Type='Choice'>N</Value></Eq></Where>";

                        SPListItemCollection items = spList.GetItems(query);
                        foreach (SPListItem item in items)
                        {

                            xmlItemElement = "works";
                            string itemDate = item["Modified"].ToString();
                            DateTime result = Convert.ToDateTime(itemDate);
                            string xmlDate = XmlConvert.ToString(result, formatString);

                            string xmlitem = SetXmlString(theURL + "/" + listName + "/" + item["Name"], xmlDate, index).ToString();

                            xmlItemElement = xmlStringBuilder.Append(xmlitem).ToString();
                            index++;
                        }
                    }
                }
            });
            return xmlItemElement;
        }

        string ReturnDLListElements(string listName)
        {
            var xmlStringBuilder = new StringBuilder();
            string formatString = "yyyy-MM-dd";
            string theURL = this.WebApplication.Sites[0].Url.ToString();

            int index = 1;
            string xmlItemElement = "";

            Microsoft.SharePoint.SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite elavatedSiteSuite = new SPSite(this.WebApplication.Sites[0].Url))
                {
                    using (SPWeb elavatedWebSuite = elavatedSiteSuite.OpenWeb())
                    {

                        SPList spList = elavatedWebSuite.Lists[listName];

                        SPQuery query = new SPQuery();
                        query.Query = "<Query><OrderBy><FieldRef Name='ID' /></OrderBy></Query>";
                        query.RowLimit = 100;

                        do
                        {
                            SPListItemCollection items = spList.GetItems(query);

                            foreach (SPListItem item in items)
                            {
                                string itemDate = item["Modified"].ToString();
                                DateTime result = Convert.ToDateTime(itemDate);
                                string xmlDate = XmlConvert.ToString(result, formatString);
                                string xmlitem = SetXmlString(theURL + "/" + listName + "/" + item["Name"], xmlDate, index).ToString();

                                xmlItemElement = xmlStringBuilder.Append(xmlitem).ToString();
                                index++;
                            }

                            query.ListItemCollectionPosition = items.ListItemCollectionPosition;

                        } while (query.ListItemCollectionPosition != null);

                    }

                }

            });
            return xmlItemElement;

        }

        public override void Execute(Guid contentDbId)
        {

            string filePath = "<FILEPATH TO>\\sitemap.xml";
            string sharePointSite = "<SHAREPOINT SITELINK>";

            var sb = new StringBuilder();
            string xmltext = sb.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>")
                               .AppendLine("<!--Generated:" + XmlConvert.ToString(DateTime.Now) + "-->")
                               .AppendLine("<urlset xmlns=\"<SITEMAP LINK URLSET>\">")
                               .ToString();
            try
            {
                xmltext = sb.AppendLine(ReturnPagesLibraryListElements("<PAGES LIST>"))
                            .AppendLine(ReturnDLListElements("<DOCUMENT LIBRARY 1>"))
                            .AppendLine(ReturnDLListElements("<DOCUMENT LIBRARY 2>"))
                            .AppendLine(ReturnDLListElements("<DOCUMENT LIBRARY 3>"))
                            .AppendLine("</urlset>").ToString();
                System.IO.File.WriteAllText(@filePath, xmltext);

                //By colleague
                string fileToUpload = @filePath;
                string documentLibraryName = "Sitemap";


                Microsoft.SharePoint.SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (SPSite oSite = new SPSite(this.WebApplication.Sites[0].Url))
                    {
                        using (SPWeb oWeb = oSite.OpenWeb())
                        {
                            if (!System.IO.File.Exists(fileToUpload))
                                throw new FileNotFoundException("File not found.", fileToUpload);

                            SPFolder myLibrary = oWeb.Folders[documentLibraryName];

                            // Prepare to upload
                            Boolean replaceExistingFiles = true;
                            String fileName = System.IO.Path.GetFileName(fileToUpload);
                            FileStream fileStream = File.OpenRead(fileToUpload);

                            // Upload document
                            SPFile spfile = myLibrary.Files.Add(fileName, fileStream, replaceExistingFiles);

                            // Commit 
                            myLibrary.Update();

                        }
                    }
                });

                // End -By colleague           

            }
            catch (Exception ex)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Sitemap-Logging", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message + " URL== " + this.WebApplication.Sites[0].Url, ex.StackTrace);
                });

            }

        }
    }
}
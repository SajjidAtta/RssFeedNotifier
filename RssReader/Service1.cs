using LogWriter;
using RssReader.DataStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RssReader
{
    
    public partial class RssReader : ServiceBase
    {
        XmlDocument rssXmlDoc;
        XmlSerializer serializer;
        [XmlArrayItem()]
        List<FeedItem> FeedItems;
        System.Timers.Timer servicetimer;
        string IOPath, logfile, outputxmlpath;
        string [] urls;


        public RssReader()
        {
     
      
            InitializeComponent();
            //Service will call workfunction after 5 Minutes.   
            servicetimer = new System.Timers.Timer(1000 * 60 *1);
            rssXmlDoc = new XmlDocument();
            serializer = new XmlSerializer(typeof(FeedItem));
            FeedItems = new List<FeedItem>();


        }
        protected override void OnStart(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            //Intializing Variables

            servicetimer.Elapsed += new System.Timers.ElapsedEventHandler(WorkerFunction);
            servicetimer.Enabled = true;


            IOPath = ConfigurationManager.AppSettings["IOPath"];
            urls = ConfigurationManager.AppSettings["urls"].Split(',');
            logfile = Path.Combine(IOPath, "RssFeedService.txt");
            outputxmlpath = Path.Combine(IOPath, "Feeds.xml");

            Logger.Instance.Log(Logger.MessageType.DEBUG,"Service Started!");
            Logger.Instance.Log(Logger.MessageType.INF,"IO Path: {0}" ,IOPath);
            Logger.Instance.Log(Logger.MessageType.INF,"Output File Path: {0}",outputxmlpath);
        }

        private void WorkerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            foreach(string rsslink in urls)
            CrawlandStore(rsslink);//Store in FeedItems List
            FeedItems.Sort();//Sort Feed Items by Time
            WriteFeedsToXml();//Serialize FeedItems List to XML

        }

        private void WriteFeedsToXml()
        {
            Logger.Instance.Log(Logger.MessageType.INF,"Writing To Xml");
            XmlSerializer serializer = new XmlSerializer(FeedItems.GetType());
            StreamWriter writer = new StreamWriter(outputxmlpath);
            serializer.Serialize(writer.BaseStream, FeedItems);
            FeedItems.Clear();
            writer.Close();
        }

        private void CrawlandStore(string url)
        {

            string xmlStr;
            //Got an Error: "The request was aborted: Could not create SSL/TLS secure channel. Fix: https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
            //Fix Start
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //Fix End
            Logger.Instance.Log(Logger.MessageType.INF, "Crawling RSS from: " + url);
            rssXmlDoc = new XmlDocument();
            try
            {
                using (var wc = new WebClient())
                {
                    //wc.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    wc.Headers.Add("user-agent", "MyRSSReader/1.0");
                    xmlStr = wc.DownloadString(url);
                    rssXmlDoc.LoadXml( xmlStr);
                }
            
            
            Logger.Instance.Log(Logger.MessageType.INF, "Deserializing RSS");
            XmlNodeList ItemList = rssXmlDoc.GetElementsByTagName("item");
            for (int i = 0; i < ItemList.Count; i++)
            {
                    FeedItem temp = ((FeedItem)serializer.Deserialize(new XmlNodeReader(ItemList[i]))).FurnishFeedItem();
                FeedItems.Add(temp);

            }

            }
            catch (Exception exp)
            {
                
                Logger.Instance.Log(Logger.MessageType.ERROR, "Could Not Access/Deserialize URL: {0} | Exception: {1}", url, exp.Message);
            }
        }

        protected override void OnStop()
        {
            Logger.Instance.Log(Logger.MessageType.DEBUG,"Service Stopped!");
        }
    }
}

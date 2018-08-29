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
using System.Xml.Serialization;

namespace RssReader
{
    //#TODO:Just Pasted, Need to Update
    public partial class RssReader : ServiceBase
    {
        XmlDocument rssXmlDoc;
        XmlSerializer serializer;
        List<FeedItem> FeedItems;
        System.Timers.Timer servicetimer;
        string IOPath, logfile, outputxmlpath,url;


        public RssReader()
        {
     
      
            InitializeComponent();
            //Service will call workfunction after 5 Minutes.   
            servicetimer = new System.Timers.Timer(1000 * 60 * 5);
            rssXmlDoc = new XmlDocument();
            serializer = new XmlSerializer(typeof(FeedItem));
            FeedItems = new List<FeedItem>();


        }
        public void writetoLog(string msg)
        {
            using (StreamWriter sw = File.AppendText(logfile))
            {
                sw.WriteLine(msg + " " + System.DateTime.Now);
            }
        }
        protected override void OnStart(string[] args)
        {
            //Intializing Variables

            servicetimer.Elapsed += new System.Timers.ElapsedEventHandler(WorkerFunction);
            servicetimer.Enabled = true;


            IOPath = ConfigurationManager.AppSettings["IOPath"];
            

            logfile = Path.Combine(IOPath, "RssFeedService.txt");
            outputxmlpath = Path.Combine(IOPath, "Feeds.xml");

            writetoLog("\nService Started!");
            writetoLog("IO Path:" + IOPath + " ");
            writetoLog("Output File Path:" + outputxmlpath + " ");
        }

        private void WorkerFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            CrawlandStore(url);
            
            writetoLog("Sorting Feed Items");
            FeedItems.Sort();
            WriteFeedsToXml();

        }

        private void WriteFeedsToXml()
        {
            writetoLog("Wrting To Xml");
            XmlSerializer serializer = new XmlSerializer(FeedItems.GetType());
            StreamWriter writer = new StreamWriter(outputxmlpath);
            serializer.Serialize(writer.BaseStream, FeedItems);
            FeedItems.Clear();
            writer.Close();
        }

        private void CrawlandStore(string url)
        {

            string xmlStr;
            using (var wc = new WebClient())
            {
                xmlStr = wc.DownloadString(url);
            }
            var xmlDoc = new XmlDocument();
            writetoLog("Crawling News from: " + url);
            rssXmlDoc.Load(url);
            XmlNodeList channel = (rssXmlDoc.GetElementsByTagName("title"));
            XmlNodeList ItemList = rssXmlDoc.GetElementsByTagName("item");
            for (int i = 0; i < ItemList.Count; i++)
            {
                FeedItem temp = ((FeedItem)serializer.Deserialize(new XmlNodeReader(ItemList[i]))).FurnishFeedItem();
              
                FeedItems.Add(temp);

            }
        }

        protected override void OnStop()
        {
            writetoLog("\nService Stopped!");
        }
    }
}

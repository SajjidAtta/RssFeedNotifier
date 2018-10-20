using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace FeedNotifier
{
    public class Notifier
    {
        XmlSerializer xmlserializer;
        static Notifier _notifier;
        SortedSet<FeedItem> feedItems;
        EmailSender.Emailer emailer;
        public static Notifier Instance{
            get {
                if (_notifier == null)
                    _notifier = new Notifier();
                return _notifier;
                }

}
        public Notifier()
        {
            xmlserializer = new XmlSerializer(typeof(FeedItem),new XmlRootAttribute("FeedItem"));
            feedItems = new SortedSet<FeedItem>();
            emailer = new EmailSender.Emailer();
        }
        public bool AddIfRecentFeed(FeedItem feeditem)
        {
            
           
                return feedItems.Add(feeditem);
           
            
        }
        public void Notify()
        {
            //TODO: To Improve | A lot of work in one method

            StringBuilder MessageSubject=new StringBuilder(""); ;
            StringBuilder MessageBody=new StringBuilder("");
            int itr = 1;

            //Create Message Subject and Body for new Jobs
            foreach(FeedItem fitem in feedItems)
            {
                if (IsRecent(fitem) && fitem.NotificationStatus==SentStatus.NotSent)
                {
                    LogWriter.Logger.Instance.Log(LogWriter.Logger.MessageType.ERROR, "Recent Item: {0} {1} {2}", fitem.Title, fitem.Description, fitem.PublishedDate);
                    MessageSubject = new StringBuilder(String.Format("Upwork Notifier - {0} Jobs", itr++));
                    MessageBody.Append(String.Format("Job Link: {0} {3} Time: {1} {3} Description: {2} {3} {3} {3} ", fitem.Link, fitem.PublishedDate, fitem.Description, Environment.NewLine));
                    fitem.NotificationStatus = SentStatus.Sent;
                }
                else
                    break; //Becuase If it was not recent then next Can Never be Recent Items are already sorted by Time
                
            }

            //Send Email
            if(MessageBody.Length>0)
            SendEmail(MessageSubject.ToString(), MessageBody.ToString());
        }

        private void SendEmail(String messageSubject, String messageBody)
        {
            emailer.CreateMailMessage(messageSubject,messageBody);
            emailer.SendMessage();
        }

        private bool IsRecent(FeedItem fitem)
        {
            //TODO SA: Convert hardcoded minutes to Read from Config
            int minutes = -10;
            if (Convert.ToDateTime(fitem.PublishedDate) > System.DateTime.Now.AddMinutes(minutes))
                return true;
            else
                return false;

        }

        public void StartNotifier()
        {
            ReadFeedsfromXML();
            //TODO SA: Call Asynchronusly
            Notify();

        }

        private void ReadFeedsfromXML()
        {
            String feedItemIdentifier = "FeedItem";
            List<FeedItem> feeditemslst = new List<FeedItem>();
            using (XmlTextReader sr =  new XmlTextReader(@"D:\PersonelSVN\trunk\RssFeedNotifier\IO\Feeds.xml"))
            {
                if(sr.ReadToDescendant(feedItemIdentifier))
                do
                {
                        FeedItem fitem =  (FeedItem) xmlserializer.Deserialize(sr);
                        AddIfRecentFeed(fitem);
                            

                } while (sr.ReadToNextSibling(feedItemIdentifier));

            }
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedNotifier
{
    public class Notifier
    {
        SortedSet<FeedItem> feedItems;
        public Notifier()
        {
            feedItems = new SortedSet<FeedItem>();
        }
        public void PickRecentFeedItems(FeedItem [] feeditems)
        {
            foreach(FeedItem fitem in feeditems)
            {
                feedItems.Add(fitem);
            }
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
                if (IsRecent(fitem))
                {
                    MessageSubject = new StringBuilder( String.Format("Upwork Notifier - {0} Jobs", itr++));
                    MessageBody.Append(String.Format("Job Link: {0} {3} Time: {1} {3} Description: {2} {3}", fitem.Link, fitem.PublishedDate, fitem.Description, Environment.NewLine));
                }
                
            }

            //Send Email
            SendEmail(MessageSubject.ToString(), MessageBody.ToString());
        }

        private void SendEmail(String messageSubject, String messageBody)
        {
            throw new NotImplementedException();
        }

        private bool IsRecent(FeedItem fitem)
        {
            throw new NotImplementedException();
        }
    }
}

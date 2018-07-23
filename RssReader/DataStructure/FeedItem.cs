using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RssReader.DataStructure
{
    class FeedItem
    {
        //#TODO:Sajjid Atta: Updae Class Fields and Methods
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
        [XmlElement("NewsChannel")]
        public string Channel { get; set; }
        [XmlElement("pubDate")]
        public string PublishedDate { get; set; }

        //I have created this function to Remove html from Description
        internal FeedItem FurnishNews()
        {
            FeedItem temp = new FeedItem();
            temp.Description = Regex.Replace(Description, "<.+?>", string.Empty);
            temp.Title = this.Title;
            temp.PublishedDate = PublishedDate;
            return temp;

        }
        public void Print()
        {
            System.Console.WriteLine("Title: " + Title);
            System.Console.WriteLine("Publication Date: " + PublishedDate);
            System.Console.WriteLine("Description: " + Description);
            System.Console.WriteLine("Channel: " + Channel);
            System.Console.WriteLine();
        }
        public void setChannel(string str)
        {
            Channel = str;
        }


        public int CompareTo(FeedItem obj)
        {
            string parseFormat = "ddd, dd MMM yyyy HH:mm:ss zzz";
            DateTime fstPubDate = DateTime.ParseExact(Convert.ToString(PublishedDate), parseFormat, CultureInfo.InvariantCulture);
            DateTime scndPubDate = DateTime.ParseExact(Convert.ToString(obj.PublishedDate), parseFormat, CultureInfo.InvariantCulture);
            return scndPubDate.CompareTo(fstPubDate);
        }
    }
}

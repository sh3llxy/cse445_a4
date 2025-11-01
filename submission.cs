using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;



/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Program
    {
    public static string xmlURL = "https://sh3llxy.github.io/cse445_a4/A4_XML_Files/Hotels.xml";
    public static string xmlErrorURL = "https://sh3llxy.github.io/cse445_a4/A4_XML_Files/HotelsErrors.xml";
    public static string xsdURL = "https://sh3llxy.github.io/cse445_a4/A4_XML_Files/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            var errors = new System.Text.StringBuilder();

            try
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(null, xsdUrl);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schemas;
                settings.ValidationFlags =
                    XmlSchemaValidationFlags.ReportValidationWarnings |
                    XmlSchemaValidationFlags.ProcessInlineSchema |
                    XmlSchemaValidationFlags.ProcessSchemaLocation;

                settings.ValidationEventHandler += (object sender, ValidationEventArgs e) =>
                {
                    // collect all errors and warnings
                    errors.AppendLine($"{e.Severity}: {e.Message}");
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { /* force full validation */ }
                }
            }
            catch (XmlException xe)
            {
                errors.AppendLine($"XmlException: {xe.Message}");
            }
            catch (Exception ex)
            {
                errors.AppendLine($"Exception: {ex.Message}");
            }

            return errors.Length == 0 ? "No errors are found" : errors.ToString().Trim();
        }

        public static string Xml2Json(string xmlUrl)
        {
            // load XML
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlUrl);

            // build Hotels → Hotel[] structure
            JArray hotelArray = new JArray();
            XmlNodeList hotels = doc.SelectNodes("/Hotels/Hotel");

            if (hotels != null)
            {
                foreach (XmlNode h in hotels)
                {
                    JObject jHotel = new JObject();

                    // Name
                    var name = h.SelectSingleNode("Name")?.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(name))
                        jHotel["Name"] = name;

                    // Phone (one or more)
                    var phoneNodes = h.SelectNodes("Phone");
                    if (phoneNodes != null && phoneNodes.Count > 0)
                    {
                        JArray phones = new JArray();
                        foreach (XmlNode p in phoneNodes)
                        {
                            var pv = p.InnerText?.Trim();
                            if (!string.IsNullOrEmpty(pv))
                                phones.Add(pv);
                        }
                        jHotel["Phone"] = phones;
                    }

                    // Address object
                    var addr = h.SelectSingleNode("Address");
                    if (addr != null)
                    {
                        JObject jAddr = new JObject();
                        string number = addr.SelectSingleNode("Number")?.InnerText?.Trim();
                        string street = addr.SelectSingleNode("Street")?.InnerText?.Trim();
                        string city = addr.SelectSingleNode("City")?.InnerText?.Trim();
                        string state = addr.SelectSingleNode("State")?.InnerText?.Trim();
                        string zip = addr.SelectSingleNode("Zip")?.InnerText?.Trim();
                        string airport = addr.SelectSingleNode("NearestAirport")?.InnerText?.Trim();

                        if (!string.IsNullOrEmpty(number)) jAddr["Number"] = number;
                        if (!string.IsNullOrEmpty(street)) jAddr["Street"] = street;
                        if (!string.IsNullOrEmpty(city)) jAddr["City"] = city;
                        if (!string.IsNullOrEmpty(state)) jAddr["State"] = state;
                        if (!string.IsNullOrEmpty(zip)) jAddr["Zip"] = zip;
                        if (!string.IsNullOrEmpty(airport)) jAddr["NearestAirport"] = airport;

                        jHotel["Address"] = jAddr;
                    }

                    // Optional Rating → appears as "_Rating" only if present
                    var ratingAttr = h.Attributes?["Rating"]?.Value?.Trim();
                    if (!string.IsNullOrEmpty(ratingAttr))
                    {
                        jHotel["_Rating"] = ratingAttr;
                    }


                    hotelArray.Add(jHotel);
                }
            }

            JObject root = new JObject
            {
                ["Hotels"] = new JObject
                {
                    ["Hotel"] = hotelArray
                }
            };

            string jsonText = root.ToString(Newtonsoft.Json.Formatting.None);


            // must be de-serializable by Newtonsoft.Json back into XML
            // if this throws, we bubble an exception to help debugging locally
            JsonConvert.DeserializeXmlNode(jsonText);

            return jsonText;
        }

    }

}
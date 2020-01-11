using StatsUI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace StatsUI.StatsXML
{
    class parseXML
    {
        public static List<Spele> Read(String folderPath)
        {
            Spele spele;
            List<Spele> gamesList = new List<Spele>();

            try { 
                XmlSerializer ser = new XmlSerializer(typeof(Spele));
                foreach (string file in Directory.EnumerateFiles(folderPath, "*.xml"))
                {
                    using (XmlReader reader = XmlReader.Create(file))
                   {
                        spele = (Spele)ser.Deserialize(reader);
                        gamesList.Add(spele);
                        Logger.Log("Game on" + spele.Laiks + " in " + spele.Vieta + " read from XML.");
                    }
                }
            }

            catch ( Exception ex)
            {
                Logger.Log(ex.Message);
            }


            return gamesList;
        }
    }
}

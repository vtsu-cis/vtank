using System;
using System.Xml;
using System.Collections.Generic;

namespace Client.src.util.game
{
    /// <summary>
    /// RankLoader dynamically reads from the ranking XML file to read in Rank objects. RankLoader
    /// is a static class.
    /// </summary>
    public static class RankLoader
    {
        public static readonly string RANK_FILE = "Ranks.xml";
        private static bool initialized = false;
        private static Dictionary<int, Rank> rankDictionary;

        static RankLoader()
        {
            rankDictionary = new Dictionary<int, Rank>();
        }

        /// <summary>
        /// Get the rank associated with the given ID, or null if the rank is unavailable.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Rank GetRank(int ID)
        {
            if (!initialized)
            {
                RefreshRanks();
                initialized = true;
            }

            if (rankDictionary.ContainsKey(ID))
                return rankDictionary[ID];
            else
                return null;
        }

        /// <summary>
        /// Refresh the ranking dictionary by re-loading the data from file. Throws an XML
        /// exception if something goes wrong.
        /// </summary>
        public static void RefreshRanks()
        {
            rankDictionary.Clear();

            XmlDocument doc = new XmlDocument();
            doc.Load(RANK_FILE);

            XmlNodeList rankNodes = doc.GetElementsByTagName("rank");
            foreach (XmlNode rankNode in rankNodes)
            {
                /* A Rank has the following properties:
                 * - ID
                 * - Title
                 * - Abbreviation
                 * - Filename
                 */
                int id = -1;
                string title = null;
                string abbreviation = null;
                string filename = null;
                bool valid = true;
                XmlNodeList childrenNodes = rankNode.ChildNodes;
                for (int i = 0; i < childrenNodes.Count; ++i)
                {
                    string nodeValue = childrenNodes[i].InnerText;
                    switch (childrenNodes[i].Name)
                    {
                        case "id":
                            bool successfulParse = int.TryParse(nodeValue, out id);
                            if (!successfulParse)
                            {
                                // Data is malformed, so continue to the next node.
                                Console.Error.WriteLine("Warning: Malformed data in rank ID field: {0}",
                                    nodeValue);
                                valid = false;
                                break;
                            }
                            break;
                        case "title":
                            if (String.IsNullOrEmpty(nodeValue))
                            {
                                Console.Error.WriteLine("Warning: Empty rank title field.");
                            }
                            title = nodeValue;
                            break;
                        case "abbreviation":
                            if (String.IsNullOrEmpty(nodeValue))
                            {
                                Console.Error.WriteLine("Warning: Empty rank abbreviation field.");
                            }
                            abbreviation = nodeValue;
                            break;
                        case "filename":
                            if (String.IsNullOrEmpty(nodeValue))
                            {
                                Console.Error.WriteLine("Warning: Empty rank filename field.");
                            }
                            filename = nodeValue;
                            break;
                        default:
                            Console.Error.WriteLine("Warning: Unknown node name in rank node: {0}",
                                childrenNodes[i].InnerText);
                            break;
                    }
                }

                if (!valid)
                {
                    continue;
                }

                // Now we've collected the node information: store it.
                Rank rank = new Rank(id, title, abbreviation, filename);
                rankDictionary[id] = rank;
            }
        }
    }
}

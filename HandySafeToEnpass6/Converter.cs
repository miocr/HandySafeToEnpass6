using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;

namespace HandySafeConverter
{
    public class Converter
    {

        public Converter()
        {
        }

        internal string Convert(string xmlFile)
        {
            List<string> allFieldNames = new List<string>() {"název","login","e-mail","heslo","url" };
            List<string> headerNames = new List<string>()   {"Název","Login","Email","*Heslo","Website" };
            StringBuilder csvText = new StringBuilder();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            XmlNodeList cards = xmlDoc.SelectNodes("/HandySafe/Folder/Card");
            foreach (XmlNode card in cards)
            {
                foreach (XmlNode field in card.SelectNodes("Field"))
                {
                    string fieldName = field.Attributes["name"].Value.Trim();

                    if (string.IsNullOrEmpty(fieldName))
                        fieldName = field.InnerText.Trim();

                    if (!allFieldNames.Contains(fieldName.ToLower()))
                    {
                        allFieldNames.Add(fieldName.ToLower());
                        // Prefix * in column name for import as secret field
                        fieldName = fieldName.Replace("\"","'");
                        if (field.Attributes["type"] != null && field.Attributes["type"].Value == "6")
                            headerNames.Add("\"*" + fieldName + "\"");
                        else
                            headerNames.Add("\"" + fieldName + "\"");
                    }
                }
            }

            // Create CSV header line
            headerNames.Insert(0, "\"Title\""); //First column 
            headerNames.Add("\"Note\"");  // Last column
            headerNames.Add("\"Tags\"");
            csvText.AppendLine(string.Join(",", headerNames));

            XmlNodeList folders = xmlDoc.SelectNodes("/HandySafe/Folder");
            foreach (XmlNode folder in folders)
            {
                string folderName = folder.Attributes["name"].Value.Trim();
                folderName = folderName.Replace("\"", "'");

                foreach (XmlNode card in folder.SelectNodes("Card"))
                {
                    string cardName = card.Attributes["name"].Value.Trim();
                    // Title value (card name) is necessary
                    if (String.IsNullOrEmpty(cardName))
                        continue;
                    cardName = cardName.Replace("\"", "'");

                    // New array for line item values
                    string[] fieldValues = new string[allFieldNames.Count + 3];

                    // Card name as Title value in first column
                    fieldValues[0] = "\"" + cardName + "\"";

                    // All card values to corresponding column
                    bool isEmpty = true;
                    foreach (XmlNode field in card.SelectNodes("Field"))
                    {
                        if (!String.IsNullOrEmpty(field.InnerText))
                        {
                            string fieldName = field.Attributes["name"].Value.Trim();
                            if (string.IsNullOrEmpty(fieldName))
                                fieldName = field.InnerText.Trim();

                            string fieldValue = field.InnerText.Trim();
                            fieldValue = fieldValue.Replace("\"", "'");
                            fieldValue = fieldValue.Replace("\n", "").Replace("\r", "");
                            // Column index for value
                            int columnIndex = allFieldNames.IndexOf(fieldName.ToLower());
                            fieldValues[columnIndex + 1] = "\"" + fieldValue + "\"";
                            isEmpty = false;
                        }
                    }

                    // Note field as prelast column
                    XmlNode noteNode = card.SelectSingleNode("Note");
                    if (noteNode != null)
                    {
                        string note = noteNode.InnerText.Trim();
                        note = note.Replace("\"", "'");
                        note = note.Replace("\n", " | ").Replace("\r", "");
                        fieldValues[allFieldNames.Count + 1] = "\"" + note + "\"";
                        isEmpty = false;
                    }

                    // Folder name as tag in tags last column
                    fieldValues[allFieldNames.Count + 2] = "\"" + folderName + "\"";

                    if (!isEmpty)
                        csvText.AppendLine(string.Join(",", fieldValues));
                }
            }

            return csvText.ToString();
        }
    }
}

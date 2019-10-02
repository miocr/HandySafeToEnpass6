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

        /* Output CSV Format - first line header, comma delimited items, nothing for feild without value
         *        
         * "Title, "CustomField1","CustomField2","*CustomField3",....,"CustomFieldN","Note","Tags"
         * "Card",  "Value"      ,              ,"secretValue"  ,    ,              ,"note","tag1,tag2"     
         */

        internal string Convert(string xmlFile)
        {
            List<string> allFieldNames = new List<string>() {"název","login","e-mail","heslo","url" };
            List<string> headerNames = new List<string>()   {"Název","Login","Email","*Heslo","Website" };
            StringBuilder csvText = new StringBuilder();

            string importTimeStamp = DateTime.Now.ToString("yyyy-MM-dd");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);

            // Inspect all cards and create name array for all used field names
            // and CSV header line
            int unnamedFieldIndex = 0;
            foreach (XmlNode card in xmlDoc.SelectNodes("/HandySafe/Folder/Card"))
            {
                foreach (XmlNode field in card.SelectNodes("Field"))
                {
                    string fieldName = String.Empty;
                    if (field.Attributes["name"] == null ||  string.IsNullOrEmpty(field.Attributes["name"].Value))
                        fieldName = String.Format("Field{0}",unnamedFieldIndex++); 
                    else
                        fieldName = field.Attributes["name"].Value.Trim();

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

            // Prepare CSV header line Title,Custom1,Custom2,...,CustomX,Note,Tags
            headerNames.Insert(0, "\"Title\""); //First column 
            headerNames.Add("\"Note\"");  // Last columns
            headerNames.Add("\"Tags\"");
            csvText.AppendLine(string.Join(",", headerNames));

            unnamedFieldIndex = 0;
            XmlNodeList folders = xmlDoc.SelectNodes("/HandySafe/Folder");
            foreach (XmlNode folder in folders)
            {
                string folderName = folder.Attributes["name"].Value.Trim();
                folderName = folderName.Replace("\"", "'");

                foreach (XmlNode card in folder.SelectNodes("Card"))
                {
                    string cardName = card.Attributes["name"].Value.Trim();
                    // Title value (card name) is mandatory
                    if (String.IsNullOrEmpty(cardName))
                        continue;
                    cardName = cardName.Replace("\"", "'");

                    // New array for field values
                    string[] fieldValues = new string[allFieldNames.Count + 3];

                    // Card name as Title value in first column
                    fieldValues[0] = "\"" + cardName + "\"";

                    // All card values to corresponding column
                    bool isEmpty = true;
                    foreach (XmlNode field in card.SelectNodes("Field"))
                    {
                        if (!String.IsNullOrEmpty(field.InnerText))
                        {
                            string fieldName = String.Empty;
                            if (field.Attributes["name"] == null || string.IsNullOrEmpty(field.Attributes["name"].Value))
                                fieldName = String.Format("Field{0}", unnamedFieldIndex++);
                            else
                                fieldName = field.Attributes["name"].Value.Trim();

                            string fieldValue = field.InnerText.Trim();
                            fieldValue = fieldValue.Replace("\"", "'");
                            fieldValue = fieldValue.Replace("\n", "").Replace("\r", "");
                            // Column index for value
                            int columnIndex = allFieldNames.IndexOf(fieldName.ToLower());
                            fieldValues[columnIndex + 1] = "\"" + fieldValue + "\"";
                            isEmpty = false;
                        }
                    }

                    // Note field as one from thelast column (Note)
                    XmlNode noteNode = card.SelectSingleNode("Note");
                    if (noteNode != null)
                    {
                        string note = noteNode.InnerText.Trim();
                        note = note.Replace("\"", "'");
                        note = note.Replace("\n", " | ").Replace("\r", "");
                        fieldValues[allFieldNames.Count + 1] = "\"" + note + "\"";
                        isEmpty = false;
                    }

                    // Folder name as tag last column (Tags)
                    // Add import date stamp as Tag for all imported items
                    fieldValues[allFieldNames.Count + 2] = "\"" + folderName + "|.import " + importTimeStamp + "\"";

                    if (!isEmpty)
                        csvText.AppendLine(string.Join(",", fieldValues));
                }
            }

            return csvText.ToString();
        }
    }
}

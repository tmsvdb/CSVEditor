using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVEditor
{
    class CSVData
    {
        public string filePath;
        public string fileName;
        public List<string> headers;
        public List<List<string>> content;
        public bool hasChanged = false;

        public CSVData(string filePath, string[] rawData)
        {
            SetFilePath(filePath);

            if (rawData.Length > 0)
            {
                SetHeaders(rawData[0]);
                string[] contentData = new string[rawData.Length - 1];
                Array.Copy(rawData, 1, contentData, 0, rawData.Length - 1);
                SetContent(contentData);
            }
            else
            {
                SetHeaders("");
                SetContent(new string[] { });
            }
        }

        public void SetFilePath (string filePath)
        {
            this.filePath = filePath;
            this.fileName = System.IO.Path.GetFileName(filePath);
        }

        public void SetHeaders(string headerLine)
        {
            if (headers == null)
                headers = new List<string>();

            headers.Clear();

            foreach (string s in headerLine.Split(','))
                headers.Add(s);
        }

        public void SetContent(string[] contentLines)
        {
            if (content == null)
                content = new List<List<string>>();

            content.Clear();

            foreach (string l in contentLines)
            {
                List<string> contentLine = new List<string>();

                foreach (string s in l.Split(','))
                    contentLine.Add(s);

                content.Add(contentLine);
            }
        }

        public string HeaderAsString()
        {
            string output = "";
            for (int i = 0; i < headers.Count; i++)
                output += (i != 0 ? "," : "") + headers[i];
            return output;
        }

        public List<string> ContentAsStringList()
        {
            List<string> output = new List<string>();
            foreach (List<string> row in content)
            {
                string s = "";
                for (int i = 0; i < row.Count; i++)
                    s += (i != 0 ? "," : "") + row[i];
                output.Add(s);
            }
            return output;
        }

        public string[] ToLines()
        {
            List<string> output = new List<string>();
            output.Add(HeaderAsString());
            foreach(string contentString in ContentAsStringList())
                output.Add(contentString);
            return output.ToArray();
        }
    }
}

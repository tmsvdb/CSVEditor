using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSVEditor
{
    public partial class CSVEditor : Form
    {

        // properties

        private List<CSVData> openDataFiles = new List<CSVData>();


        // Constructor

        public CSVEditor()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);
        }


        // Event Handlers

        void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)       // Ctrl-S Save
            {
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
                saveFile();
            }
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            ShowNewPage(new CSVData("", new string[] { }));
            CSVLinkedToTab().hasChanged = true;
            buttonSaveFile.Enabled = true;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                openFile(openFileDialog1.FileName);
        }
        
        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CSVData csv = CSVLinkedToTab();
            if (csv != null)
                buttonSaveFile.Enabled = csv.hasChanged;

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            saveFile();
            CloseCurrentPage();
        }


        // Local implementation

        private void openFile(string filePath)
        {
            ShowNewPage(new CSVData(filePath, System.IO.File.ReadAllLines(filePath)));
        }

        private void saveFile()
        {
            CSVData csv = CSVLinkedToTab();

            if (csv.hasChanged || csv.filePath == "")
            {
                if (csv.filePath == "")
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        csv.SetFilePath(saveFileDialog1.FileName);
                        tabControl1.SelectedTab.Name = csv.fileName;
                        tabControl1.SelectedTab.Text = csv.fileName;
                    }
                    else throw new Exception("Save File CSV must contain a pathname!");

                System.IO.File.WriteAllLines(csv.filePath, csv.ToLines());
                tabControl1.SelectedTab.Text = tabControl1.SelectedTab.Name;
                csv.hasChanged = false;
                buttonSaveFile.Enabled = false;
            }
        }


        // Tab Control

        private void ShowNewPage(CSVData csvData)
        { 
            openDataFiles.Add(csvData);
            tabControl1.SelectedTab = addTab(csvData.fileName);
            drawTabView(csvData);
            buttonClose.Enabled = true;
        }

        private void CloseCurrentPage()
        {
            CSVData csv = CSVLinkedToTab();
            openDataFiles.Remove(csv);
            tabControl1.TabPages.Remove(TabLinkedToCSV(csv));

            if (openDataFiles.Count == 0)
                buttonClose.Enabled = false;
        }

        public TabPage addTab (string name)
        {
            foreach (TabPage t in tabControl1.TabPages)
                if (name.Equals(t.Name))
                    return t;

            TabPage tab = new TabPage();
                tab.Name = name;
                tab.Text = name == "" ? "<new file>" : name;
                tab.AutoScroll = true;

            tabControl1.TabPages.Add(tab);

            return tab; 
        }


        // Draw Tab
      
        private void drawTabView(CSVData csv)
        {
            for (int i = 0; i < csv.headers.Count; i++)
                CreateTabObject<Label>("header_" + i, csv.headers[i], new Point(30 + (i * 150), 20), new Size(125, 23), true).Click += new EventHandler(label_Edit);

            for (int row = 0; row < csv.content.Count; row++)
                for (int col = 0; col < csv.content[row].Count; col++)
                    CreateTabObject<TextBox>("content_"+row+"_"+col, csv.content[row][col], new Point(30 + (col * 150), 60 + (row * 25)), new Size(125, 23)).TextChanged += new EventHandler(textbox_Changed);

            CreateTabObject<Button>("add header button", "+", new Point(30 + (csv.headers.Count * 150), 20), new Size(125, 23));
            CreateTabObject<Button>("add row button", "+", new Point(30, 60 + (csv.content.Count * 25)), new Size(125, 23));

            SampleCSV(csv);
        }

        private Control CreateTabObject<T> (string name, string text, Point position, Size size, bool bold = false)
        {
            Control tabObj = Activator.CreateInstance(typeof(T)) as Control;

            if (bold) tabObj.Font = new Font(tabObj.Font, FontStyle.Bold);
            tabObj.Location = position;
            tabObj.Name = name;
            tabObj.Size = size;
            tabObj.Text = text;

            tabControl1.SelectedTab.Controls.Add(tabObj);

            return tabObj;
        }

        private void label_Edit(object sender, EventArgs e)
        {
            Label target = sender as Label;
            Control text = CreateTabObject<TextBox>(target.Name, target.Text, new Point(target.Location.X, target.Location.Y - 2), target.Size);
            target.Dispose();

            Control btn = CreateTabObject<Button>("button_" + target.Name, "-", new Point(text.Location.X - text.Size.Height - 2, text.Location.Y - 1), new Size(text.Size.Height + 2, text.Size.Height + 2));
            btn.Click += new System.EventHandler(delegate (object o, EventArgs a)
            {
                btn.Dispose();
                label_Restore(text);
                UpdateCSVData();
            });
        }

        private void label_Restore(Control sender)
        {
            Control target = sender as Control;
            CreateTabObject<Label>(target.Name, target.Text, new Point(target.Location.X, target.Location.Y + 2), target.Size, true);
            target.Dispose();
        }

        private void textbox_Changed(object sender, EventArgs e)
        {
            TextBox target = sender as TextBox;
            UpdateCSVData();       
        }


        // Data management

        private void UpdateCSVData()
        {
            CSVData csv = CSVLinkedToTab();
                csv.headers = GetHeadersfromTab();
                csv.content = GetContentfromTab();
                csv.hasChanged = true;

            SampleCSV(CSVLinkedToTab());
            tabControl1.SelectedTab.Text = tabControl1.SelectedTab.Name + " *";
            buttonSaveFile.Enabled = true;
        }


        // Lookup

        private List<string> GetHeadersfromTab()
        {
            List<string> output = new List<string>();
            List<Control> compList = GetTabComponents("header");
            for (int i = 0; i < compList.Count; i++)
                output.Add(compList[i].Text);
            return output;
        }

        private List<List<string>> GetContentfromTab()
        {
            List<List<string>> output = new List<List<string>>();
            List<Control> rowCompList;
            int row = 0;

            do {
                rowCompList = GetTabComponents("content_"+ row);
                if (rowCompList.Count > 0) 
                {
                    List<string> outputCol = new List<string>();
                    Control control;
                    int col = 0;

                    do {
                        control = GetTabComponent("content_" + row + "_" + col);
                        if (control != null)
                        { 
                            outputCol.Add(control.Text);
                            col++;
                        }
                        
                    } while (control != null);

                    output.Add(outputCol);
                    row++;
                }
            } while (rowCompList.Count > 0);

            return output;
        }

        private List<Control> GetTabComponents(string nameTag)
        {
            List<Control> compList = new List<Control>();
            foreach (Control control in tabControl1.SelectedTab.Controls)
                if (control.Name.Contains(nameTag)) compList.Add(control);
            return compList;
        }

        private Control GetTabComponent(string fullName)
        {
            foreach (Control control in tabControl1.SelectedTab.Controls)
                if (control.Name == fullName) return control;
            return null;
        }

        private CSVData CSVLinkedToTab ()
        {
            foreach (CSVData csv in openDataFiles)
                if (csv.fileName == tabControl1.SelectedTab.Name)
                    return csv;
            return null;
        }

        private TabPage TabLinkedToCSV(CSVData csv)
        {
            foreach (TabPage tab in tabControl1.TabPages)
                if (csv.fileName == tab.Name)
                    return tab;
            return null;
        }

        private void SampleCSV (CSVData csv)
        {
            string outputL = "";
            foreach (string l in csv.headers)
                outputL += "[" + l + "]";
            Console.WriteLine(outputL);

            foreach (List<string> r in csv.content)
            {
                string outputC = "";
                foreach (string s in r)
                    outputC += "[" + s + "]";
                Console.WriteLine(outputC);
            }
        }

        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blaze.Core.Extensions;

namespace Blaze.Finance
{
    public partial class MainForm : Form
    {
        public static string DateTimeFormat = "yyyy/MM/dd";
        public MainForm()
        {
            InitializeComponent();
            InitDataGrid(); 
            InitializeCategorization();
        }

        private void InitDataGrid()
        {
            string[] colNames = { "Date", "Desc", "Amount", "Cat" };
            foreach (string col in colNames)
            {
                var dc = new DataGridViewTextBoxColumn();
                dc.Name = col;
                _DataGridView.Columns.Add(dc);
            }

            _DataGridView.Columns[1].DefaultCellStyle.Format = DateTimeFormat;

            _DataGridView.RowHeadersVisible = false;
            //TODO: _DataGridView.CellEndEdit
        }

        HashSet<string> _Categories;
        
        Dictionary<string, string> _Keywords;
        private void InitializeCategorization()
        {
            _Categories = new HashSet<string>();
            _Categories.AddRange(new[] { "Transportation", "Coffee" , "Clothes", "Groceries", "Eating Out", "Entertainment", "Utilities", "Misc", "Unknown" });

            _Keywords = new Dictionary<string, string>();
            _Keywords.Add("uber", "Transportation");
            _Keywords.Add("INTERMARCHE", "Groceries");
            _Keywords.Add("Public Mobile", "Utilities");
            _Keywords.Add("Second cup", "Coffee");
            _Keywords.Add("Brulerie", "Coffee");
            _Keywords.Add("Presse Cafe", "Eating Out");
            _Keywords.Add("Pub", "Entertainment");
            _Keywords.Add("Bar", "Entertainment");
            _Keywords.Add("Benelux", "Entertainment");
            _Keywords.Add("Netflix", "Entertainment");
            _Keywords.Add("APPT 200", "Entertainment");
            _Keywords.Add("SAQ", "Entertainment");
            _Keywords.Add("RESTAURANT", "Eating Out");
            _Keywords.Add("TSI Internet", "Utilities");
            _Keywords.Add("FOODORA", "Eating Out");
            _Keywords.Add("Boutique 1861", "Clothes");
            _Keywords.Add("P A NATURE MONTREAL", "Groceries");
            _Keywords.Add("Metro ST", "Groceries");
            _Keywords.Add("FIDO", "Utilities");
            _Keywords.Add("Videotron", "Utilities");
            _Keywords.Add("STM", "Transportation");
            _Keywords.Add("Uber", "Transportation");
            _Keywords.Add("VANHOUTTE", "Coffee");
            _Keywords.Add("Starbucks", "Coffee");
        }

        public void LoadInformation()
        {
            var d = new OpenFileDialog();
            var res = d.ShowDialog();
            if (res != System.Windows.Forms.DialogResult.OK)
                return;
            string file = d.FileName;
            string[] data = System.IO.File.ReadAllLines(file);
            DeserializeCsv(data, file.EndsWith("csvf"));
        }

        List<Transaction> _Transactions;

        DateTime _BeginDate, _EndDate;
        Dictionary<string, Tuple<int, float>> _Descriptions;
        
        public void DeserializeCsv(string[] data, bool csvf)
        {
            //"Account Type","Account Number","Transaction Date","Cheque Number","Description 1","Description 2","CAD$","USD$"
            _Transactions = new List<Transaction>(data.Length);
            for (int ii = 1; ii < data.Length; ++ii)
            {
                string[] datasplit = data[ii].Split(',');
                _Transactions.Add(new Transaction(datasplit, csvf));
            }

            Analyze();
            
            _DataGridView.SuspendLayout();
            foreach(var t in _Transactions)
            {
                t.AddRow(_DataGridView);
            }
            _DataGridView.ResumeLayout();
        }

        private void Analyze()
        {
            _Descriptions = new Dictionary<string, Tuple<int, float>>();
            for (int ii = 0; ii < _Transactions.Count; ++ii)
            {
                var transaction = _Transactions[ii];
                if (!_Categories.Contains(transaction.Category) && transaction.Category != "Payment")
                    _Categories.Add(transaction.Category);

                string desc = transaction.Description.ToLower();
                //Try categorize based on keywords
                if (transaction.Category == "Unknown")
                {
                    foreach (string keyword in _Keywords.Keys)
                    {
                        if (desc.Contains(keyword.ToLower()))
                        {
                            transaction.Category = _Keywords[keyword];
                        }
                    }
                }

                //Count expenses based on equal keywords
                Tuple<int, float> descData;
                if (_Descriptions.TryGetValue(desc, out descData))
                    _Descriptions[desc] = new Tuple<int, float>(descData.Item1 + 1, descData.Item2 + transaction.Amount);
                else
                    _Descriptions.Add(desc, new Tuple<int, float>(1, transaction.Amount));
            }

            Dictionary<string, float> categoryExpense = _Categories.ToDictionary(c => c, c=> 0.0f);

            foreach (Transaction t in _Transactions)
            {
                if (_Categories.Contains(t.Category))
                {
                    categoryExpense[t.Category] += t.Amount;
                }
            }

            _Chart.Series[0].Points.Clear();
            foreach (var kvp in categoryExpense)
            {
                _Chart.Series[0].Points.AddXY(string.Format("{0} ({1})", kvp.Key, kvp.Value.ToString("0.00")), kvp.Value);
            }

            tbRecurrents.Text = string.Join(Environment.NewLine,
                _Descriptions
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => string.Format("{0} {1} {2}", 
                    kvp.Key.PadRight(35), 
                    kvp.Value.Item1.ToString().PadRight(6), 
                    kvp.Value.Item2.ToString("0.00") )));
            
            List<Transaction> expenses = _Transactions.Where(t => t.Amount > 0).ToList();

            float total = expenses.Sum(t => t.Amount);
            float average = expenses.Average(t => t.Amount);
            Trace.Assert(Math.Abs(total / expenses.Count - average) < 0.001);

            tbAverageExpense.Text = average.ToString("0.00");

            List<float> dailyExpense = new List<float>();
            DateTime currentDate = _Transactions.First().Date.AddDays(-1);
            foreach (Transaction t in expenses)
            { 
                if (t.Date > currentDate)
                {
                    dailyExpense.Add(t.Amount);
                    currentDate = t.Date;
                }
                else if (t.Date == currentDate)
                {
                    dailyExpense[dailyExpense.Count - 1] += t.Amount;
                }
            }

            Trace.Assert(Math.Abs(total - dailyExpense.Sum()) < 0.001);

            tbDailyAverage.Text = dailyExpense.Average().ToString("0.00");

            Transaction max = expenses.MaxElement(t => t.Amount);
            Transaction min = expenses.MinElement(t => t.Amount);

            tbMax.Text = string.Format("{0} {1}", max.Description.PadRight(20), max.Amount.ToString("0.00"));
            tbMin.Text = string.Format("{0} {1}", min.Description.PadRight(20), min.Amount.ToString("0.00"));

            _BeginDate = _Transactions.First().Date;
            _EndDate = _Transactions.Last().Date;

            tbExpenseBegin.Tag = _BeginDate;
            tbExpenseEnd.Tag = _EndDate;
            UpdateRangeTextBoxesFromTag();
        }

        private string[] SerializeDataToCsv()
        {
            List<Transaction> modTrans = new List<Transaction>(_DataGridView.Rows.Count);

            _Transactions = _DataGridView
                .Rows
                .OfType<DataGridViewRow>()
                .Select(dgvr => new Transaction(dgvr.ToData()))
                .ToList();
            return _Transactions.Select(t => t.ToCsv()).ToArray();
        }

        private void UpdateRangeTextBoxesFromTag()
        {
            tbExpenseBegin.Text = ((DateTime)tbExpenseBegin.Tag).ToString(DateTimeFormat);
            tbExpenseEnd.Text = ((DateTime)tbExpenseEnd.Tag).ToString(DateTimeFormat);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadInformation();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.AddExtension = true;
            d.DefaultExt = ".csvf";
            d.Filter = "csvf files (*.csvf)|*.csvf";
            if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string file = d.FileName;
            string [] data = SerializeDataToCsv();
            System.IO.File.WriteAllLines(file, data);
            Analyze();
        }
    }


    class Transaction
    {
        public string Description { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }

        public Transaction(string[] data, bool csvf)
        {
            if (csvf)
            { 
                Date = DateTime.Parse(data[0]);
                Description = data[1];
                Amount = float.Parse(data[2]);
                Category = data[3];
                return;
            }    
            
            Date = DateTime.Parse(data[2]);
            Description = data[4].Substring(1, data[4].Length - 2);
            Amount = -float.Parse(data[6]);
            Category = Amount > 0 ? "Unknown" : "Payment";
        }

        public Transaction(object[] data)
        {
            Date = (DateTime)data[0];
            Description = (string)data[1];
            Amount = (float)(data[2]);
            Category = (string) data[3];
        }

        public void AddRow(DataGridView grid)
        {
            int rowIx = grid.Rows.Add();
            var r = grid.Rows[rowIx];
            r.Cells[0].Value = Date;//.ToString(DateTimeFormat);
            r.Cells[1].Value = Description;
            r.Cells[2].Value = Amount;
            r.Cells[3].Value = Category;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Description.PadRight(30), Amount.ToString("0.00"));
        }

        public string ToCsv()
        {
            return string.Format("{0},{1},{2},{3}", Date.ToString(MainForm.DateTimeFormat), Description, Amount.ToString("0.00"), Category);
        }
    }

    static class Ex
    {
        static public object[] ToData(this DataGridViewRow row)
        {
            return row.Cells.OfType<DataGridViewCell>().Select(dcvc => dcvc.Value).ToArray();
        }
        
    }
}

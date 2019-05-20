using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Drawing.Imaging;

namespace Invoice_Gen
{
    public partial class Form1 : Form
    {
        public List<Dictionary<string, Customer>> mDictionary = new List<Dictionary<string, Customer>>();
        public Dictionary<string, Customer> mCustomers = new Dictionary<string, Customer>();

        public Form1()
        {
            InitializeComponent();
            ReadCustomers();
            SetupBackground();
        }

        private void SetupComboBox()
        {
            var addresses = new List<string>();
            comboBox1.Items.Clear();

            foreach (var d in mCustomers)
            {
                addresses.Add(d.Key);
            }

            comboBox1.Items.AddRange(addresses.ToArray());
        }

        private double CalculateTotal()
        {
            Customer c = mCustomers[mAddress.Text];
            double total = 0;
            dataGridView1[1,0].Value = c.mCharge;
            dataGridView1[0,0].Value = "Monthly Pool Service";

            for (int i = 0; i <= dataGridView1.RowCount-1; i++)
            {
                if (dataGridView1[1, i].Value == null)
                {
                    break;
                }

                string price = dataGridView1[1, i].Value.ToString();
                double OUTRes;

                bool test = Double.TryParse(price, out OUTRes);

                if (!test)
                    break;

                total += OUTRes;
            }

            return total;
        }

        private void ReadCustomers()
        {
            // TODO Add a json file that will contain customers with proper file path.
            // Program will exit if there is an error
            var list = File.ReadAllText("../../dictionary.json");

            var result = JsonConvert.DeserializeObject<List<Dictionary<string, Customer>>>(list);

            if (result == null) { return; }

            mCustomers= result[0];
            SetupComboBox();
        }

        private int GetInvoiceNumber()
        {
            int counter = 17000;

            foreach (var user in mCustomers)
                counter += user.Value.mInvoiceCounter;

            return counter;
        }

        private void SetupBackground()
        {
            DateTime time = DateTime.Today;
            mDate.Text = time.ToShortDateString();

            // TODO Insert logo on top of price box by uncommenting next line and inserting proper file path.
            // pictureBox1.BackgroundImage = Image.FromFile("../../logo.png");

            groupBox2.BackgroundImageLayout = ImageLayout.Stretch;
            groupBox2.BackColor = Color.GhostWhite;
            groupBox2.Text = ("Invoice " + (GetInvoiceNumber() + 1));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double OUTcostTB;
            Double.TryParse(TBNewCharge.Text, out OUTcostTB);

            if (TBNewAddress.Text == "" || mCustomers.ContainsKey(TBNewAddress.Text) || TBNewName.Text == "" || OUTcostTB < 0)
            {
                MessageBox.Show("Invalid entry");
                return;
            }

            var inCustomer = new Customer(TBNewName.Text, TBNewAddress.Text, OUTcostTB, 0);

            mCustomers.Add(inCustomer.mAddress, inCustomer);
            AddToDictionary();
            UpdateInvoice(inCustomer);
        }

        private void AddToDictionary()
        {
            mDictionary = new List<Dictionary<string, Customer>>();
            mDictionary.Add(mCustomers);

            string str = JsonConvert.SerializeObject(mDictionary);

            File.WriteAllText("../../dictionary.json", str);

            ReadCustomers();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string addressEntered = comboBox1.Text.ToString();

            if (!mCustomers.ContainsKey(addressEntered))
            {
                MessageBox.Show(addressEntered + " does not exist in the records.");
                return;
            }
            
            UpdateInvoice(mCustomers[addressEntered]);
        }

        private void UpdateInvoice(Customer c)
        {
            mCustomerName.Text = c.mName;
            mAddress.Text = c.mAddress;
            totalLabel.Text = ("Total: $" + CalculateTotal());
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            int initWidth = groupBox2.Width;
            groupBox2.Width = 1002;

            mCustomers[mAddress.Text].mInvoiceCounter++;
            AddToDictionary();
            int invoiceNum = GetInvoiceNumber();

            groupBox2.Text = ("Invoice " + invoiceNum);

            Bitmap bm = new Bitmap(groupBox2.Size.Width, groupBox2.Size.Height);
            Rectangle rec = new Rectangle(0,0, groupBox2.Width, groupBox2.Height);
            groupBox2.DrawToBitmap(bm, rec);

            var bmWriter = new StreamWriter(("../../Invoice " + invoiceNum + ".jpg"));

            bm.Save(bmWriter.BaseStream, ImageFormat.Jpeg);

            groupBox2.Text = ("Invoice " + (invoiceNum + 1));

            bm.Dispose();
            groupBox2.Width = initWidth;

            bmWriter.Dispose();
            bmWriter.Close();
        }
    }

    public class Customer
    {
        public string mName { get; set; }
        public string mAddress { get; set; }
        public double mCharge { get; set; }
        public int mInvoiceCounter { get; set; }
        
        public Customer(string name, string address, double charge, int count)
        {
            mName = name;
            mAddress = address;
            mCharge = charge;
            mInvoiceCounter = count;
        }

    }
}

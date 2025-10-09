using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TubeMailGorilla.Entities;

namespace TubeMailGorilla.Forms
{
    public partial class Accounts : Form
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public Accounts()
        {
            InitializeComponent();
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var credientals = context.Credientals.ToList();

                dgvCredentials.DataSource = credientals;
                context.SaveChanges();

                MessageBox.Show("Records updated successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDeleteRecords_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Credientals.ToList();

                foreach (var item in query)
                {
                    context.Credientals.Remove(item);
                    context.SaveChanges();
                }

                MessageBox.Show("Records deleted successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddRecord_Click(object sender, EventArgs e)
        {
            Add_Account add_Account = new Add_Account();
            add_Account.Show();
        }
    }
}

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
    public partial class Proxies : Form
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public Proxies()
        {
            InitializeComponent();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                dgvProxies.DataSource = context.Proxies.ToList();
                context.SaveChanges();

                MessageBox.Show("Records updated successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Proxies.ToList();
                context.Proxies.RemoveRange(query);
                context.SaveChanges();

                dgvProxies.DataSource = context.Emailers.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Entities.Proxies proxy = new Entities.Proxies()
                {
                    Host = txtUrl.Text,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.Proxies.Add(proxy);
                context.SaveChanges();

                MessageBox.Show("Proxy has been added successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Proxies_Load(object sender, EventArgs e)
        {
            var proxies = context.Proxies.ToList();

            dgvProxies.DataSource = proxies;
        }
    }
}

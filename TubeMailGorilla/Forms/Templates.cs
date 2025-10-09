using Microsoft.EntityFrameworkCore;
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
    public partial class Templates : Form
    {
        private ApplicationDbContext appDbContext = new ApplicationDbContext();

        public Templates()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Entities.Templates templates = new Entities.Templates();

                templates.Name = txtName.Text;
                templates.Subject = txtSubject.Text;
                templates.Body = rtxtBody.Text;
                templates.Html = ckHtmlMode.Checked;

                appDbContext.Templates.Add(templates);
                appDbContext.SaveChanges();

                MessageBox.Show("Template has been added successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var query = appDbContext.Templates.Where(x => x.Name == cboTemplate.Text).FirstOrDefault();

                query.Name = txtName.Text;
                query.Subject = txtSubject.Text;
                query.Body = rtxtBody.Text;
                query.Html = ckHtmlMode.Checked;

                appDbContext.SaveChanges();

                MessageBox.Show("Template has been updated successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            try
            {
                appDbContext.Database.ExecuteSqlRaw("Truncate table templates");

                MessageBox.Show("All template have been deleted Successfully...");
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
                var query = appDbContext.Templates.Where(x => x.Name == cboTemplate.Text).FirstOrDefault();

                appDbContext.Templates.Remove(query);
                appDbContext.SaveChanges();

                MessageBox.Show("Template has been deleted successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var query = appDbContext.Templates.Where(x => x.Name == cboTemplate.Text).FirstOrDefault();

                txtName.Text = query.Name;
                txtSubject.Text = query.Subject;
                rtxtBody.Text = query.Body;
                ckHtmlMode.Checked = query.Html;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

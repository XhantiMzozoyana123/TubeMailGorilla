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
    public partial class Add_Account : Form
    {
        private ApplicationDbContext context = new ApplicationDbContext();
        public Add_Account()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Credientals.ToList();

                Credientals credientals = new Credientals();

                credientals.Email = txtEmail.Text;
                credientals.Password = txtPassword.Text;
                credientals.SmtpHost = txtSMTPServer.Text;
                credientals.SmtpPort = Convert.ToInt32(txtSmtpPort.Text);
                credientals.StmpSsl = ckSmtpSSL.Checked;
                credientals.ImapHost = txtImapServer.Text;
                credientals.ImapPost = Convert.ToInt32(txtImapPort.Text);
                credientals.ImapSsl = ckImapSSL.Checked;

                context.Credientals.Add(credientals);
                context.SaveChanges();

                MessageBox.Show("Account added successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

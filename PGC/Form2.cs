using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace PGC
{
    public partial class Form2 : MetroFramework.Forms.MetroForm
    {
        NotifyIcon noi = new NotifyIcon();
        int randnumclicks;
        int numclicks = 0;
        int randtiming;
        int time = 0;
        bool isgaytest;
        string filecheck = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PGC\\gnrsetting.config";

        public Form2()
        {
            InitializeComponent();
        }

        private async void metroLabel3_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(metroLabel3.Text);

            noi.BalloonTipTitle = "Почта скопирована!";
            noi.BalloonTipText = "Вы успешно скопировали почту разработчика!";
            noi.BalloonTipIcon = ToolTipIcon.Info;
            noi.Icon = this.Icon;
            noi.Visible = true;
            noi.ShowBalloonTip(2000);
            await Task.Delay(2000);
            noi.Visible = false;
            noi.Dispose();
        }

        private void metroLabel5_Click(object sender, EventArgs e)
        {
            Process.Start("https://vk.com/artydavl");
        }

        private void metroLabel7_Click(object sender, EventArgs e)
        {
            Process.Start("https://lolz.guru/egozvaliartur/");
        }

        private async void metroLabel9_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(metroLabel9.Text);

            noi.BalloonTipTitle = "Discord скопирован!";
            noi.BalloonTipText = "Вы успешно скопировали дискорд разработчика!";
            noi.BalloonTipIcon = ToolTipIcon.Info;
            noi.Icon = this.Icon;
            noi.Visible = true;
            noi.ShowBalloonTip(2000);
            await Task.Delay(2000);
            noi.Visible = false;
            noi.Dispose();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (!File.Exists(filecheck))
            {
                isgaytest = false;
                timer1.Enabled = true;
                Random rnd = new Random();
                randnumclicks = rnd.Next(30, 150);
                Random rnd2 = new Random();
                randtiming = rnd2.Next(5, 20);
                metroProgressBar1.Maximum = randnumclicks;
                metroProgressBar1.Value = 0;
            }
            else
            {
                isgaytest = false;
                metroLabel1.Text = "Не забывай какой тест ты проходил, я всё ещё помню кто ты >:D";
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (numclicks != randnumclicks)
            {
                numclicks++;
                metroProgressBar1.Value = numclicks;
            }
            else
            {
                isgaytest = true;
                metroPanel2.Visible = false;
                metroPanel1.Visible = true;
            }
        }

        private void metroButton3_MouseEnter(object sender, EventArgs e)
        {
            metroButton3.Text = "Если честно, ДА";
        }

        private void metroButton3_MouseLeave(object sender, EventArgs e)
        {
            metroButton3.Text = "Если честно, НЕТ";
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(filecheck, "Вы выбрали свою сторону, и она довольно тёмная.\nКак говорится 'Welcome to the club budy!'...");
            isgaytest = false;
            metroPanel1.Visible = false;
            metroPanel2.Visible = false;
            metroPanel3.Visible = true;
            Process.Start("https://youtu.be/-BPlwV6mUyw");
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            File.WriteAllText(filecheck, "Вы выбрали свою сторону, и она довольно тёмная");
            isgaytest = false;
            metroLabel12.Text = "Зато честно... Чтож.. Гей значит... Да?\nСпасибо за информацию в статистику!";
            metroPanel1.Visible = false;
            metroPanel2.Visible = false;
            metroPanel3.Visible = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (time != randtiming)
            {
                time++;
            }
            else
            {
                timer1.Enabled = false;
                metroPanel2.Visible = true;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isgaytest)
            {
                e.Cancel = true;
                MetroFramework.MetroMessageBox.Show(this, "Не не, погоди-ка.\nА ну давай отвечай честно!", "СТОЯТЬ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void Form2_MouseEnter(object sender, EventArgs e)
        {
            if (!this.Focused)
            {
                this.Activate();
            }
        }
    }
}

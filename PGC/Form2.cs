using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;

namespace PGC
{
    public partial class Form2 : MetroFramework.Forms.MetroForm
    {
        NotifyIcon noi = new NotifyIcon();

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
        }

        private void metroLabel7_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://lolz.guru/wdude/");
            }
            catch
            {
                MetroFramework.MetroMessageBox.Show(this, "Не удалось открыть ссылку в браузере, возможно недостаточно прав, или не выбран браузер по умолчанию", "Произошла ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            try
            {
                Process.Start("https://discord.com/login");
            }
            catch
            {
                MetroFramework.MetroMessageBox.Show(this, "Не удалось открыть ссылку в браузере, возможно недостаточно прав, или не выбран браузер по умолчанию", "Произошла ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            await Task.Delay(2000);
            noi.Visible = false;
        }

        private void Form2_MouseEnter(object sender, EventArgs e)
        {
            if (!this.Focused)
            {
                this.Activate();
            }
        }

        private void metroLabel11_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/WutADude");
            }
            catch
            {
                MetroFramework.MetroMessageBox.Show(this, "Не удалось открыть ссылку в браузере, возможно недостаточно прав, или не выбран браузер по умолчанию", "Произошла ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void metroLabel1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://my.qiwi.com/Artur-DZ3-5meKt6");
            }
            catch
            {
                MetroFramework.MetroMessageBox.Show(this, "Не удалось открыть ссылку в браузере, возможно недостаточно прав, или не выбран браузер по умолчанию", "Произошла ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace PGC
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PGC";
        string filecheck = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PGC\\gnrsetting.config";
        string head;
        string host;
        string version = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client").GetValue("version").ToString();
        //кол-во прокси
        int proxloadedHT;
        int proxloadedS4;
        int proxloadedS5;
        //старое кол-во
        int oldht;
        int olds4;
        int olds5;
        int threadsCount;
        int badhttp;
        int bads4;
        int bads5;
        int sumofgood;
        int sumofbad;
        int numofthreadshttp;
        int numofthreadss4;
        int numofthreadss5;
        int sumofthreads;
        int iforhttp;
        int ifors4;
        int ifors5;
        int timeout;
        int mnojitel = 3;
        bool messagewas;
        bool warnwas;
        bool waslog;
        bool frameisgood = false;
        bool isclosesaving = false;
        //отсчёт времени
        TimeSpan timework;
        DateTime begin_time;
        //ссылки для граба
        List<string> urlHT = new List<string>();
        List<string> urlS4 = new List<string>();
        List<string> urlS5 = new List<string>();
        //сграбленые прокси
        List<string> gottedHTTP = new List<string>();
        List<string> gottedSOCKS4 = new List<string>();
        List<string> gottedSOCKS5 = new List<string>();
        //хорошие прочеканные прокси
        List<string> goodHTTP = new List<string>();
        List<string> goodSOCKS4 = new List<string>();
        List<string> goodSOCKS5 = new List<string>();
        //проверка актива чекера
        bool ischecking;
        //Потоки
        Thread thread;
        Thread thread2;
        Thread thread3;
        //Папка для сохранения гудов
        string dirforsave = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PGC" + $"\\Goods ({DateTime.Now.Day}.{DateTime.Now.Month} [{DateTime.Now.Hour}.{DateTime.Now.Minute }])";

        public Form1()
        {
            InitializeComponent();
        }

        private async void GrabProxy()
        {
            metroButton1.Enabled = false;
            metroLink1.Enabled = false;
            await Task.Run(() =>
            {
                richTextBox1.AppendText("Пытаюсь достать все типы прокси...\n");
                try
                {
                    using (var request = new HttpRequest())
                    {
                        for (int i = 0; i < urlHT.Count; i++)
                        {
                            request.UserAgent = Http.ChromeUserAgent();
                            try
                            {
                                HttpResponse rsp = request.Get(urlHT[i]);
                                string[] gotHTTP = rsp.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (string prht in gotHTTP)
                                {
                                    if (gottedHTTP.IndexOf(prht.Trim(new char[] { ' ' })) == -1 && !gottedHTTP.Contains(prht) && !Regex.IsMatch(prht, @"^[a-zA-Z]+$"))
                                    {
                                        if (prht.Length > 0)
                                        {
                                            gottedHTTP.Add(prht);
                                        }
                                        proxloadedHT = gottedHTTP.Count;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                richTextBox1.AppendText($"Возникла проблема при граббе HTTP прокси: {ex.Message}\n");

                            }
                        }
                        richTextBox1.AppendText($"Получилось достать {proxloadedHT} HTTP прокси.\n");
                    }
                }
                catch
                {
                    Thread.CurrentThread.Abort();
                }
                try
                {
                    using (var request2 = new HttpRequest())
                    {
                        for (int i = 0; i < urlS4.Count; i++)
                        {
                            request2.UserAgent = Http.ChromeUserAgent();
                            try
                            {
                                HttpResponse rsp2 = request2.Get(urlS4[i]);
                                string[] gotS4 = rsp2.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (string s4 in gotS4)
                                {
                                    if (gottedSOCKS4.IndexOf(s4.Trim(new char[] { ' ' })) == -1 && !gottedSOCKS4.Contains(s4) && !Regex.IsMatch(s4, @"^[a-zA-Z]+$"))
                                    {
                                        if (s4.Length > 0)
                                        {
                                            gottedSOCKS4.Add(s4);
                                        }
                                        proxloadedS4 = gottedSOCKS4.Count;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                richTextBox1.AppendText($"Возникла проблема при граббе SOCKS4 прокси: {ex.Message}\n");
                            }
                        }
                        richTextBox1.AppendText($"Получилось достать {proxloadedS4} SOCKS4 прокси.\n");
                    }
                }
                catch
                {
                    Thread.CurrentThread.Abort();
                }
                try
                {
                    using (var request3 = new HttpRequest())
                    {
                        for (int i = 0; i < urlS4.Count; i++)
                        {
                            request3.UserAgent = Http.ChromeUserAgent();
                            try
                            {
                                HttpResponse rsp3 = request3.Get(urlS5[i]);
                                string[] gotS5 = rsp3.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (string s5 in gotS5)
                                {
                                    if (gottedSOCKS5.IndexOf(s5.Trim(new char[] { ' ' })) == -1 && !gottedSOCKS5.Contains(s5) && !Regex.IsMatch(s5, @"^[a-zA-Z]+$"))
                                    {
                                        if (s5.Length > 0)
                                        {
                                            gottedSOCKS5.Add(s5);
                                        }
                                        proxloadedS5 = gottedSOCKS5.Count;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                richTextBox1.AppendText($"Возникла проблема при граббе SOCKS5 прокси: {ex.Message}\n");
                            }
                        }
                        richTextBox1.AppendText($"Получилось достать {proxloadedS5} SOCKS5 прокси.\n");
                    }
                }
                catch
                {
                    Thread.CurrentThread.Abort();
                }
                Applyhead();
                metroButton1.Enabled = true;
                metroLink1.Enabled = true;
                oldht = proxloadedHT;
                olds4 = proxloadedS4;
                olds5 = proxloadedS5;
                Thread.CurrentThread.Abort();
            });
        }

        private void CheckDirAndUrls()
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    richTextBox1.AppendText($"Запуск программы первый, выполняю необходимые действия...\nСоздаю папку по пути: {directory}...\nРаскидываю необходимые файлы...\n");

                    FirstReqSend();
                    Directory.CreateDirectory(directory);
                    CheckFrameWork();
                    File.WriteAllText(directory + "\\urlsHTTP.txt", "https://api.proxyscrape.com?request=getproxies&proxytype=http&timeout=10000&country=all&anonymity=all&ssl=all" + "\n"
                        + "https://www.proxy-list.download/api/v1/get?type=http" + "\n"
                        + "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt");
                    File.WriteAllText(directory + "\\urlsSOCKS4.txt", "https://api.proxyscrape.com?request=getproxies&proxytype=socks4&timeout=10000&country=all&anonymity=all&ssl=all" + "\n"
                        + "https://www.proxy-list.download/api/v1/get?type=socks4" + "\n"
                        + "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt");
                    File.WriteAllText(directory + "\\urlsSOCKS5.txt", "https://api.proxyscrape.com?request=getproxies&proxytype=socks5&timeout=10000&country=all&anonymity=all&ssl=all" + "\n"
                        + "https://www.proxy-list.download/api/v1/get?type=socks5" + "\n"
                        + "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt");
                    richTextBox1.AppendText($"Все необходимые действия первого запуска выполненны, более они выполняться не будут!\nМожете начинать использование ПО!\n");
                    Thread read = new Thread(ReadUrs);
                    read.Start();
                }
                else
                {
                    CheckFrameWork();
                    ReadUrs();
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Произошла ошибка:" + ex.Message.ToString() + "\n");

            }
        }

        private void ReadUrs()
        {
            try
            {
                StreamReader reader1 = new StreamReader(directory + "\\urlsHTTP.txt");
                string[] http = reader1.ReadToEnd().Trim(new char[] {'\n'}).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string ht in http)
                {
                    if (urlHT.IndexOf(ht.Trim()) == -1)
                    {
                        urlHT.Add(ht);
                    }
                }
                reader1.Close();
                StreamReader reader2 = new StreamReader(directory + "\\urlsSOCKS4.txt");
                string[] so4 = reader2.ReadToEnd().Trim(new char[] { '\n' }).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string soc4 in so4)
                {
                    if (urlS4.IndexOf(soc4.Trim()) == -1)
                    {
                        urlS4.Add(soc4);
                    }
                }
                reader2.Close();
                StreamReader reader3 = new StreamReader(directory + "\\urlsSOCKS5.txt");
                string[] so5 = reader3.ReadToEnd().Trim(new char[] { '\n' }).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string soc5 in so5)
                {
                    if (urlS5.IndexOf(soc5.Trim()) == -1)
                    {
                        urlS5.Add(soc5);
                    }
                }
                reader3.Close();
                richTextBox1.AppendText($"Количество ссылок для импорта HTTP прокси: {urlHT.Count()}\n");

                richTextBox1.AppendText($"Количество ссылок для импорта SOCKS4 прокси: {urlS4.Count()}\n");

                richTextBox1.AppendText($"Количество ссылок для импорта SOCKS5 прокси: {urlS5.Count()}\n");

                Thread grab = new Thread(GrabProxy);
                grab.Start();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Произошла ошибка: " + ex.Message.ToString() + "\n");
            }
        }
        private void metroTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            label1.Text = $"{mnojitel} × ({metroTrackBar1.Value})";
            if (metroTrackBar1.Value >= 15 && metroCheckBox3.Checked | metroCheckBox2.Checked && warnwas == false)
            {
                MetroFramework.MetroMessageBox.Show(this, "При большом количестве потоков программа может вылетать и не сохранять результаты!\nПожалуйста учитывайте производительность своего ПК и скорость интернета!", "ВНИМАНИЕ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                warnwas = true;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread goch = new Thread(CheckDirAndUrls);
            goch.Start();
            ToolTip tp = new ToolTip();
            tp.SetToolTip(metroLabel2, "Он пытался сделать крутой и полезный софт, но у него как обычно не вышло...");
        }

        private void Applyhead()
        {
            head = metroLabel1.Text;
            string[] wrds = metroLabel1.Text.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string newstr = head.Replace(wrds[2], wrds[2].Replace($"[{olds5}]", $"[{proxloadedS5}]")).Replace(wrds[3], wrds[3].Replace($"[{olds4}]", $"[{proxloadedS4}]")).Replace(wrds[4], wrds[4].Replace($"[{oldht}]", $"[{proxloadedHT}]"));
            metroLabel1.Text = newstr;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (ischecking == false)
            {
                timeout = Convert.ToInt32(numericUpDown1.Value);
                timework = TimeSpan.Zero;
                timer1.Enabled = true;
                metroLabel31.Visible = true;
                begin_time = DateTime.Now;
                metroLink1.Enabled = false;
                messagewas = false;
                badhttp = 0;
                bads5 = 0;
                bads4 = 0;
                goodHTTP.Clear();
                goodSOCKS4.Clear();
                goodSOCKS5.Clear();
                iforhttp = 0;
                ifors4 = 0;
                ifors5 = 0;
                numofthreadshttp = 0;
                numofthreadss4 = 0;
                numofthreadss5 = 0;
                tabControl1.Enabled = false;
                metroTrackBar1.Enabled = false;
                ischecking = true;
                UpdateSum();
                threadsCount = metroTrackBar1.Value;
                host = metroTextBox1.Text;
                metroButton1.Text = "Остановить";
                if (metroCheckBox3.Checked && !metroCheckBox2.Checked && !metroCheckBox1.Checked)
                {

                    richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Начинаю проверку всех прокси...\n");

                    for (int i = 0; i < threadsCount; i++)
                    {
                        thread = new Thread(WorkerHttp);
                        thread2 = new Thread(WorkerSocks4);
                        thread3 = new Thread(WorkerSocks5);
                        thread.IsBackground = true;
                        thread2.IsBackground = true;
                        thread3.IsBackground = true;
                        if (ischecking)
                        {
                            thread.Start();
                            thread2.Start();
                            thread3.Start();
                        }
                        else
                        {
                            thread.Abort();
                            thread2.Abort();
                            thread3.Abort();
                        }
                    }
                }
                else if (metroCheckBox2.Checked && !metroCheckBox1.Checked && !metroCheckBox3.Checked)
                {
                    richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Начинаю проверку SOCKS5 & SOCKS4 прокси...\n");

                    for (int i = 0; i < threadsCount; i++)
                    {
                        Thread thread2 = new Thread(WorkerSocks4);
                        Thread thread3 = new Thread(WorkerSocks5);
                        if (ischecking)
                        {
                            thread2.Start();
                            thread3.Start();
                        }
                        else
                        {
                            thread2.Abort();
                            thread3.Abort();
                        }
                    }
                }
                else if (metroCheckBox1.Checked && !metroCheckBox2.Checked && !metroCheckBox3.Checked)
                {
                    richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Начинаю проверку HTTP прокси...\n");

                    for (int i = 0; i < threadsCount; i++)
                    {
                        Thread thread = new Thread(WorkerHttp);
                        if (ischecking)
                        {
                            thread.Start();
                        }
                        else
                        {
                            thread.Abort();
                        }
                    }
                }
                else
                {
                    richTextBox1.AppendText("Выберите тип прокси, который хотите проверить!\n");
                    timework = TimeSpan.Zero;
                    timer1.Enabled = false;
                    metroLabel31.Visible = false;
                    ischecking = false;
                    metroButton1.Text = "Запустить";
                    tabControl1.Enabled = true;
                    metroTrackBar1.Enabled = true;
                    metroLink1.Enabled = true;
                    iforhttp = proxloadedHT;
                    ifors4 = proxloadedS4;
                    ifors5 = proxloadedS5;
                    threadsCount = 0;
                }
            }
            else
            {
                richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Завершаю проверку прокси...\n");
                metroLink1.Enabled = true;
                iforhttp = proxloadedHT;
                ifors4 = proxloadedS4;
                ifors5 = proxloadedS5;
                Thread save = new Thread(SaveProx);
                save.Start();
                tabControl1.Enabled = true;
                metroTrackBar1.Enabled = true;
                ischecking = false;
                metroButton1.Text = "Запустить";
                threadsCount = 0;
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (metroTextBox2.Text.Length > 0 && metroTextBox2.Text.StartsWith("http") && metroComboBox1.Text.Length > 0)
            {
                switch (metroComboBox1.Text)
                {
                    case "http":
                        {
                            if (!urlHT.Contains(metroTextBox2.Text))
                            {
                                File.AppendAllText(directory + "\\urlsHTTP.txt", $"\n{metroTextBox2.Text}");
                                richTextBox1.Text = "";
                                richTextBox1.AppendText("Ссылка успешно добавлена в список для импорта прокси.\n");
                                ReadUrs();
                            }
                            else
                                richTextBox1.AppendText("Не удалось добавить ссылку, так как она уже есть в списке.\n");

                        }
                        break;
                    case "socks4":
                        {
                            if (!urlS4.Contains(metroTextBox2.Text))
                            {
                                File.AppendAllText(directory + "\\urlsSOCKS4.txt", $"\n{metroTextBox2.Text}");
                                richTextBox1.Text = "";
                                richTextBox1.AppendText("Ссылка успешно добавлена в список для импорта прокси.\n");
                                ReadUrs();
                            }
                            else
                                richTextBox1.AppendText("Не удалось добавить ссылку, так как она уже есть в списке.\n");

                        }
                        break;
                    case "socks5":
                        {
                            if (!urlS5.Contains(metroTextBox2.Text))
                            {
                                File.AppendAllText(directory + "\\urlsSOCKS5.txt", $"\n{metroTextBox2.Text}");
                                richTextBox1.Text = "";
                                richTextBox1.AppendText("Ссылка успешно добавлена в список для импорта прокси.\n");
                                ReadUrs();
                            }
                            else
                                richTextBox1.AppendText("Не удалось добавить ссылку, так как она уже есть.\n");

                        }
                        break;
                }

            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this, "Проверьте правильность ввода ссылки!\nВ началессылки обязательно должно быть http:// или https://.\nПроверьте тип прокси.", "Не удалось добавить ссылку!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //открытие корневой папки программы
        private void metroLabel26_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", directory);
        }

        //ПРОВЕРКА HTTP проксей
        private async void WorkerHttp()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var ht = new HttpRequest())
                    {
                        ht.UserAgent = Http.ChromeUserAgent();
                        for (iforhttp = iforhttp; iforhttp < proxloadedHT; iforhttp++)
                        {
                            numofthreadshttp++;
                            try
                            {
                                ht.Proxy = HttpProxyClient.Parse(gottedHTTP[0]);
                                gottedHTTP.RemoveAt(0);
                                label2.Text = $"({gottedHTTP.Count})";
                                ht.ConnectTimeout = timeout;
                                ht.Proxy.ConnectTimeout = timeout;
                                ht.KeepAlive = false;
                                HttpResponse resp = ht.Get(host);
                                if (resp != null)
                                {
                                    goodHTTP.Add(ht.Proxy.ToString());
                                    metroLabel22.Text = goodHTTP.Count.ToString();
                                    if (ischecking)
                                    {
                                        richTextBox1.AppendText($"(HTTP) Прокси: {ht.Proxy} - прошёл проверку!\n");
                                    }
                                    numofthreadshttp--;
                                }
                                else
                                {
                                    badhttp++;
                                    if (ischecking)
                                    {
                                        richTextBox1.AppendText($"(HTTP) Прокси: {ht.Proxy} - не работает!\n");
                                        metroLabel20.Text = badhttp.ToString();
                                    }
                                    numofthreadshttp--;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ischecking)
                                {
                                    badhttp++;
                                    richTextBox1.AppendText($"(HTTP) Прокси: {ht.Proxy} - не работает! ({ex.Message})\n");
                                    metroLabel20.Text = badhttp.ToString();
                                }
                                numofthreadshttp--;
                            }
                        }
                    }
                    if (gottedHTTP.Count == 0 & metroCheckBox1.Checked || gottedHTTP.Count == 0 & gottedSOCKS4.Count == 0 & gottedSOCKS5.Count == 0)
                    {
                        metroButton1.PerformClick();
                    }
                    Thread.CurrentThread.Abort();
                });
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.Abort();
            }
        }

        private async void WorkerSocks4()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var so4 = new HttpRequest())
                    {
                        so4.UserAgent = Http.ChromeUserAgent();
                        for (ifors4 = ifors4; ifors4 < proxloadedS4; ifors4++)
                        {
                            numofthreadss4++;
                            try
                            {
                                so4.Proxy = Socks4ProxyClient.Parse(gottedSOCKS4[0]);
                                gottedSOCKS4.RemoveAt(0);
                                label3.Text = $"({gottedSOCKS4.Count})";
                                so4.ConnectTimeout = timeout;
                                so4.Proxy.ConnectTimeout = timeout;
                                so4.KeepAlive = false;
                                HttpResponse resp = so4.Get(host);
                                if (resp != null)
                                {
                                    goodSOCKS4.Add(so4.Proxy.ToString());
                                    metroLabel17.Text = goodSOCKS4.Count.ToString();
                                    if (ischecking)
                                    {
                                        richTextBox1.AppendText($"(SOCKS4) Прокси: {so4.Proxy} - прошёл проверку!\n");
                                    }
                                    numofthreadss4--;
                                }
                                else
                                {
                                    if (ischecking)
                                    {
                                        bads4++;
                                        richTextBox1.AppendText($"(SOCKS4) Прокси: {so4.Proxy} - не работает!\n");
                                        metroLabel15.Text = bads4.ToString();
                                    }
                                    numofthreadss4--;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ischecking)
                                {
                                    bads4++;
                                    richTextBox1.AppendText($"(SOCKS4) Прокси: {so4.Proxy} - не работает! ({ex.Message})\n");

                                    metroLabel15.Text = bads4.ToString();
                                }
                                numofthreadss4--;
                            }
                        }
                    }
                    if (gottedSOCKS4.Count == 0 & gottedSOCKS5.Count == 0 & metroCheckBox2.Checked || gottedHTTP.Count == 0 & gottedSOCKS4.Count == 0 & gottedSOCKS5.Count == 0)
                    {
                        metroButton1.PerformClick();
                    }
                    Thread.CurrentThread.Abort();
                });
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.Abort();
            }
        }

        private async void WorkerSocks5()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var so5 = new HttpRequest())
                    {
                        so5.UserAgent = Http.ChromeUserAgent();
                        for (ifors5 = ifors5; ifors5 < proxloadedS5; ifors5++)
                        {
                            numofthreadss5++;
                            try
                            {
                                so5.Proxy = Socks5ProxyClient.Parse(gottedSOCKS5[0]);
                                gottedSOCKS5.RemoveAt(0);
                                label4.Text = $"({gottedSOCKS5.Count})";
                                so5.ConnectTimeout = timeout;
                                so5.Proxy.ConnectTimeout = timeout;
                                so5.KeepAlive = false;
                                HttpResponse resp = so5.Get(host);
                                if (resp != null)
                                {
                                    goodSOCKS5.Add(so5.Proxy.ToString());
                                    if (ischecking)
                                    {
                                        metroLabel10.Text = goodSOCKS5.Count.ToString();
                                        richTextBox1.AppendText($"(SOCKS5) Прокси: {so5.Proxy} - прошёл проверку!\n");
                                    }
                                    numofthreadss5--;
                                }
                                else
                                {
                                    if (ischecking)
                                    {
                                        bads5++;
                                        richTextBox1.AppendText($"(SOCKS5) Прокси: {so5.Proxy} - не работает!\n");
                                        metroLabel12.Text = bads5.ToString();
                                    }
                                    numofthreadss5--;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ischecking)
                                {
                                    bads5++;
                                    richTextBox1.AppendText($"(SOCKS5) Прокси: {so5.Proxy} - не работает! ({ex.Message})\n");
                                    metroLabel12.Text = bads5.ToString();
                                }
                                numofthreadss5--;
                            }
                        }
                    }
                    if (gottedSOCKS4.Count == 0 & gottedSOCKS5.Count == 0 & metroCheckBox2.Checked || gottedHTTP.Count == 0 & gottedSOCKS4.Count == 0 & gottedSOCKS5.Count == 0)
                    {
                        metroButton1.PerformClick();
                    }
                    Thread.CurrentThread.Abort();
                });
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.Abort();
            }
        }

        private void metroTextBox1_Leave(object sender, EventArgs e)
        {
            if (metroTextBox1.Text == "")
            {
                metroTextBox1.Text = "https://www.google.com";
            }
            else if (!metroTextBox1.Text.StartsWith("http"))
            {
                MetroFramework.MetroMessageBox.Show(this, "Укажите ссылку на хост (сайт) корректно!\nВ начале обязательно должно быть http:// или https:// (!) ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                metroTextBox1.Text = "https://www.google.com";
            }
        }

        async void UpdateSum()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    sumofbad = badhttp + bads4 + bads5;
                    sumofgood = goodHTTP.Count + goodSOCKS4.Count + goodSOCKS5.Count;
                    sumofthreads = numofthreadshttp + numofthreadss4 + numofthreadss5;
                    metroLabel6.Text = sumofgood.ToString();
                    metroLabel9.Text = sumofbad.ToString();
                    metroLabel28.Text = sumofthreads.ToString();
                    Task.Delay(750);
                }
            });
        }
        //Сэйв прокси
        private void SaveProx()
        {
            Thread clearlists = new Thread(ClearLists);
            if (sumofthreads > 0 && !isclosesaving)
            {
                metroButton1.Enabled = false;
                if (messagewas == false)
                {
                    richTextBox1.AppendText($"Ожидаю завершения работы всех потоков 🕒... (Это может длиться долго, не волнуйтесь!)\n");
                    messagewas = true;
                }
                Thread.Sleep(1000);
                SaveProx();
            }
            else if (sumofthreads == 0 || isclosesaving)
            {
                metroButton1.Enabled = false;
                if (!Directory.Exists(dirforsave) && goodHTTP.Count > 0 | goodSOCKS4.Count > 0 | goodSOCKS5.Count > 0)
                {
                    richTextBox1.AppendText("Создаю папку для сохранения рабочих прокси...\n");
                    Directory.CreateDirectory(dirforsave);
                    richTextBox1.AppendText("Сохраняю рабочие прокси в текстовые документы в специально созданной папке...\n");

                    foreach (var goodhts in goodHTTP)
                    {
                        File.AppendAllText(dirforsave + "\\GoodHttp.txt", goodhts + "\n");
                    }
                    foreach (var goods4s in goodSOCKS4)
                    {
                        File.AppendAllText(dirforsave + "\\GoodSOCKS4.txt", goods4s + "\n");
                    }
                    foreach (var goods5s in goodSOCKS5)
                    {
                        File.AppendAllText(dirforsave + "\\GoodSOCKS5.txt", goods5s + "\n");
                    }
                    timer1.Enabled = false;
                    metroLabel31.Visible = false;
                    richTextBox1.AppendText($"Прошло времени со старта: {metroLabel31.Text}\n");
                    richTextBox1.AppendText("Сохранение завершено, открываю папку с результатами...\n");
                    Process.Start("explorer.exe", dirforsave);
                    if (isclosesaving)
                    {
                        isclosesaving = false;
                        richTextBox1.AppendText("Сейчас программа завершит работу...\n");
                        richTextBox1.ScrollToCaret();
                        Thread.Sleep(2500);
                        Application.Exit();
                    }
                    metroButton1.Enabled = true;
                    clearlists.Start();
                }
                else if (goodHTTP.Count > 0 || goodSOCKS4.Count > 0 || goodSOCKS5.Count > 0)
                {
                    richTextBox1.AppendText("Сохраняю гуды в текстовые документы в папке сегодняшнего дня...\n");

                    foreach (var goodhts in goodHTTP)
                    {
                        File.AppendAllText(dirforsave + "\\GoodHttp.txt", goodhts + "\n");
                    }
                    foreach (var goods4s in goodSOCKS4)
                    {
                        File.AppendAllText(dirforsave + "\\GoodSOCKS4.txt", goods4s + "\n");
                    }
                    foreach (var goods5s in goodSOCKS5)
                    {
                        File.AppendAllText(dirforsave + "\\GoodSOCKS5.txt", goods5s + "\n");
                    }
                    timer1.Enabled = false;
                    metroLabel31.Visible = false;
                    richTextBox1.AppendText($"Прошло времени со старта: {metroLabel31.Text}\n");
                    richTextBox1.AppendText("Сохранение завершено, открываю папку с результатами...\n");
                    Process.Start("explorer.exe", dirforsave);
                    metroButton1.Enabled = true;
                    clearlists.Start();

                }
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            if (!this.Focused)
            {
                this.Activate();
            }
        }

        private void metroLink1_Click(object sender, EventArgs e)
        {
            Thread clear = new Thread(ClearRich);
            clear.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void metroLabel2_Click(object sender, EventArgs e)
        {
            if (numericUpDown1.Value != 2208)
            {
                richTextBox1.AppendText("Открываю страницу криворукого разраба...\n");
                Process.Start("https://lolz.guru/egozvaliartur/");
            }
            else
            {
                Form2 form2 = new Form2();
                if (!File.Exists(filecheck))
                {
                    richTextBox1.AppendText("Что? Откуда ты...? Bruh...\n");
                    await Task.Delay(1000);
                    richTextBox1.AppendText("Как? Ну ладно, открываю доп. информацию о разработчике...\n");
                    await Task.Delay(1000);
                    form2.Show();
                    await Task.Delay(5000);
                    richTextBox1.AppendText("Kek :D\n");
                }
                else
                {
                    richTextBox1.AppendText("Открываю доп. информацию о криворуком разработчике...\n");
                    form2.Show();
                }
            }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Thread closesave = new Thread(SaveProx);
            if (ischecking)
            {
                e.Cancel = true;
                ischecking = false;
                isclosesaving = true;
                richTextBox1.AppendText("Вызвано принудительное сохранение проверенных прокси...\n");
                closesave.Start();
            }
            else
            {
                e.Cancel = false;
                Application.Exit();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime nowtime = DateTime.Now;
            timework = nowtime - begin_time;
            metroLabel31.Text = $"{timework.Hours}:{timework.Minutes}:{timework.Seconds}";
        }

        private void metroCheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBox3.Checked)
            {
                metroCheckBox2.Checked = false;
                metroCheckBox1.Checked = false;
                metroTrackBar1.Maximum = 20;
                mnojitel = 3;
                label1.Text = $"{mnojitel} × ({metroTrackBar1.Value})";
            }
        }

        private void metroCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBox2.Checked)
            {
                metroCheckBox1.Checked = false;
                metroCheckBox3.Checked = false;
                metroTrackBar1.Maximum = 30;
                mnojitel = 2;
                label1.Text = $"{mnojitel} × ({metroTrackBar1.Value})";
            }
        }

        private void metroCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBox1.Checked)
            {
                metroCheckBox2.Checked = false;
                metroCheckBox3.Checked = false;
                metroTrackBar1.Maximum = 90;
                mnojitel = 1;
                label1.Text = $"{mnojitel} × ({metroTrackBar1.Value})";
            }
        }


        private void ClearRich()
        {
            richTextBox1.Clear();
            richTextBox1.AppendText("Лог очищен!\n");
            Thread.Sleep(2000);
            richTextBox1.Clear();
            Thread.CurrentThread.Abort();
        }

        private void ClearLists()
        {
            goodHTTP.Clear();
            goodSOCKS4.Clear();
            goodSOCKS5.Clear();
            Thread.CurrentThread.Abort();
        }


        private async void FirstReqSend()
        {
            richTextBox1.AppendText($"Отправляю информацаю (не содержит конфиденциальных данных, например: логинов, паролей или документов) о новом пользователе ПО...\nСодержит только имя пользователя (Классное имя: {Environment.UserName}), ваш IP, имя компьютера (Прикольное имя компьютера: {Environment.MachineName}), битность системы, количество потоков процессора (Вроде как: {Environment.ProcessorCount})...\n");
            try
            {
                await Task.Run(() =>
                {
                    HttpRequest req = new HttpRequest();
                    req.Referer = $"PGC Software";
                    req.UserAgent = $"New user of the PGC! UserName: {Environment.UserName}. OS ver: {Environment.OSVersion}. Device name: {Environment.MachineName}. System is x64? - {Environment.Is64BitOperatingSystem}. Processor threads: {Environment.ProcessorCount}";
                    HttpResponse resp = req.Get("https://iplogger.org/2SRhX5");
                });
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"При отправке информации возникла проблема: {ex.Message}");
            }
            Thread.CurrentThread.Abort();
        }


        private async void CheckFrameWork()
        {
            try
            {
                this.TopMost = true;
                if (!File.Exists(directory + "\\frwkgood.kk"))
                {
                    richTextBox1.AppendText("Проверяю версию фреймворка...\n");
                    if (version != null && version.StartsWith("4.7.2") | version.StartsWith("4.8") | version.StartsWith("4.9") | version.StartsWith("5") && frameisgood == false)
                    {
                        richTextBox1.AppendText($"Проверка версии фреймворка (Установленная версия: {version}) прошла успешно, программа должна работать правильно...\n");
                        frameisgood = true;
                        File.Create(directory + "\\frwkgood.kk");
                    }
                    else
                    {
                        richTextBox1.AppendText($"Проверка версии фреймворка прошла, версия меньше необходимой, программа может работать некорректно! Предлагаю установить новую версию фреймворка... Нужна версия: как минимум '4.7.2...', у вас {version}. \n");

                        DialogResult result = MessageBox.Show(this, "Необходимо установить новую версию .NET Framework...\nСделать это сейчас?", "Установить новую версию фреймворка?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            Process.Start("https://support.microsoft.com/ru-ru/help/4503548/microsoft-net-framework-4-8-offline-installer-for-windows");
                            Application.Exit();
                        }
                        else
                        {
                            richTextBox1.AppendText($"Учтите, что из-за того что версия фреймворка несовместима (Установленная версия: {version}), программа может работать некорректно или же не работать в принципе!\n");
                            frameisgood = false;
                            this.TopMost = false;
                        }
                    }
                }
                else
                {
                    richTextBox1.AppendText($"Пропускаю проверку версии фреймворка (Установленная версия: {version})...\n");
                    this.TopMost = false;
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"При вроверке версии фреймворка произошла ошибка: {ex.Message}\n");
                this.TopMost = false;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }
    }
}

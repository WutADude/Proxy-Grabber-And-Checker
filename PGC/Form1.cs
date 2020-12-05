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
        string head;
        public string host;
        string version = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client").GetValue("version").ToString();
        public string file_dialog;
        //кол-во прокси
        public int proxloadedHT;
        public int proxloadedS4;
        public int proxloadedS5;
        //старое кол-во
        public int oldht;
        public int olds4;
        public int olds5;
        public int threadsCount;
        public int badhttp;
        public int bads4;
        public int bads5;
        public int sumofgood;
        public int sumofbad;
        public int numofthreadshttp;
        public int numofthreadss4;
        public int numofthreadss5;
        public int sumofthreads;
        public int iforhttp;
        public int ifors4;
        public int ifors5;
        public int timeout;
        public int mnojitel = 3;
        // счёт всех милисекунд
        public int sumofmsechttp;
        public int sumofmsecs4;
        public int sumofmsecs5;
        // расчёт
        public int srtimehttp;
        public int srtimes4;
        public int srtimes5;

        bool messagewas;
        bool warnwas;
        bool frameisgood = false;
        bool isclosesaving = false;
        bool done;
        //отсчёт времени
        TimeSpan timework;
        DateTime begin_time;
        //ссылки для граба
        public List<string> urlHT = new List<string>();
        public List<string> urlS4 = new List<string>();
        public List<string> urlS5 = new List<string>();
        public List<string> bad_urls = new List<string>();
        //сграбленые прокси
        public List<string> gottedHTTP = new List<string>();
        public List<string> gottedSOCKS4 = new List<string>();
        public List<string> gottedSOCKS5 = new List<string>();
        //хорошие прочеканные прокси
        public List<string> goodHTTP = new List<string>();
        public List<string> goodSOCKS4 = new List<string>();
        public List<string> goodSOCKS5 = new List<string>();
        //проверка актива чекера
        public bool ischecking;
        //Папка для сохранения гудов
        string dirforsave = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PGC" + "\\Goods" + $"\\Good ({DateTime.Now.Day}.{DateTime.Now.Month} [{DateTime.Now.Hour}.{DateTime.Now.Minute}])";

        //Рабочие
        Workers workers = new Workers();

        Thread badurlscheck;

        public Form1()
        {
            InitializeComponent();
            workers.mainform = this;
            badurlscheck = new Thread(CheckBadUrls);
            badurlscheck.IsBackground = true;
        }

        private async void GrabProxy()
        {
            metroButton1.Enabled = false;
            metroLink1.Enabled = false;
            metroLink3.Enabled = false;
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
                            request.ConnectTimeout = 3000;
                            request.KeepAlive = false;
                            try
                            {
                                HttpResponse rsp = request.Get(urlHT[i]);
                                string[] gotHTTP = rsp.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (string prht in gotHTTP)
                                {
                                    if (gottedHTTP.IndexOf(prht.Trim(new char[] { ' ' })) == -1 && !gottedHTTP.Contains(prht) && !Regex.IsMatch(prht, @"^[a-zA-Z]+$") && prht != null)
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
                                bad_urls.Add(urlHT[i]);
                                richTextBox1.AppendText($"Возникла проблема при граббе HTTP прокси: {ex.Message} ({urlHT[i]})\n");

                            }
                        }
                        richTextBox1.AppendText($"Получилось достать {proxloadedHT} HTTP прокси.\n");
                    }
                }
                catch
                {

                }
                try
                {
                    using (var request2 = new HttpRequest())
                    {
                        for (int i = 0; i < urlS4.Count; i++)
                        {
                            request2.UserAgent = Http.ChromeUserAgent();
                            request2.ConnectTimeout = 3000;
                            request2.KeepAlive = false;
                            try
                            {
                                HttpResponse rsp2 = request2.Get(urlS4[i]);
                                string[] gotS4 = rsp2.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (string s4 in gotS4)
                                {
                                    if (gottedSOCKS4.IndexOf(s4.Trim(new char[] { ' ' })) == -1 && !gottedSOCKS4.Contains(s4) && !Regex.IsMatch(s4, @"^[a-zA-Z]+$") && s4 != null)
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
                                bad_urls.Add(urlS4[i]);
                                richTextBox1.AppendText($"Возникла проблема при граббе SOCKS4 прокси: {ex.Message} ({urlS4[i]})\n");
                            }
                        }
                        richTextBox1.AppendText($"Получилось достать {proxloadedS4} SOCKS4 прокси.\n");
                    }
                }
                catch
                {

                }
                try
                {
                    using (var request3 = new HttpRequest())
                    {
                        for (int i = 0; i < urlS5.Count; i++)
                        {
                            request3.UserAgent = Http.ChromeUserAgent();
                            request3.ConnectTimeout = 3000;
                            request3.KeepAlive = false;
                            try
                            {
                                HttpResponse rsp3 = request3.Get(urlS5[i]);
                                string[] gotS5 = rsp3.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                                foreach (string s5 in gotS5)
                                {
                                    if (gottedSOCKS5.IndexOf(s5.Trim(new char[] { ' ' })) == -1 && !gottedSOCKS5.Contains(s5) && !Regex.IsMatch(s5, @"^[a-zA-Z]+$") && s5 != null)
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
                                bad_urls.Add(urlS5[i]);
                                richTextBox1.AppendText($"Возникла проблема при граббе SOCKS5 прокси: {ex.Message} ({urlS5[i]})\n");
                            }
                        }
                        richTextBox1.AppendText($"Получилось достать {proxloadedS5} SOCKS5 прокси.\n");
                    }
                }
                catch
                {

                }
                Applyhead();
                badurlscheck.Start();
                metroButton1.Enabled = true;
                metroLink1.Enabled = true;
                oldht = proxloadedHT;
                olds4 = proxloadedS4;
                olds5 = proxloadedS5;
                metroLink3.Enabled = true;
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
                string[] http = reader1.ReadToEnd().Trim(new char[] { '\n' }).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
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
            done = false;
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
                metroLink3.Enabled = false;
                srtimehttp = 0;
                srtimes4 = 0;
                srtimes5 = 0;
                sumofmsechttp = 0;
                sumofmsecs4 = 0;
                sumofmsecs5 = 0;
                timeout = Convert.ToInt32(numericUpDown1.Value);
                timework = TimeSpan.Zero;
                timer1.Enabled = true;
                timer2.Enabled = true;
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
                Thread updsum = new Thread(UpdateSum);
                updsum.IsBackground = true;
                updsum.Start();
                threadsCount = metroTrackBar1.Value;
                host = metroTextBox1.Text;
                metroButton1.Text = "Остановить";
                if (metroCheckBox3.Checked && !metroCheckBox2.Checked && !metroCheckBox1.Checked)
                {

                    richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Начинаю проверку всех прокси...\n");
                    workers.StartWork(3);
                }
                else if (metroCheckBox2.Checked && !metroCheckBox1.Checked && !metroCheckBox3.Checked)
                {
                    richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Начинаю проверку SOCKS5 & SOCKS4 прокси...\n");
                    workers.StartWork(2);
                }
                else if (metroCheckBox1.Checked && !metroCheckBox2.Checked && !metroCheckBox3.Checked)
                {
                    richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Начинаю проверку HTTP прокси...\n");
                    workers.StartWork(1);
                }
                else
                {
                    richTextBox1.AppendText("Выберите тип прокси, который хотите проверить!\n");
                    timework = TimeSpan.Zero;
                    timer1.Enabled = false;
                    timer2.Enabled = false;
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
                    metroLink3.Enabled = true;
                }
            }
            else
            {
                richTextBox1.AppendText($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Завершаю проверку прокси...\n");
                ischecking = false;
                workers.StopWork();
                metroLink1.Enabled = true;
                timer2.Enabled = false;
                iforhttp = proxloadedHT;
                ifors4 = proxloadedS4;
                ifors5 = proxloadedS5;
                Thread save = new Thread(SaveProx);
                save.IsBackground = true;
                save.Start();
                tabControl1.Enabled = true;
                metroTrackBar1.Enabled = true;
                metroButton1.Text = "Запустить";
                threadsCount = 0;
                metroLink3.Enabled = true;
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

        private void UpdateSum()
        {
            while (true || sumofthreads != 0)
            {
                sumofbad = badhttp + bads4 + bads5;
                sumofgood = goodHTTP.Count + goodSOCKS4.Count + goodSOCKS5.Count;
                sumofthreads = workers.threads.Count();
                metroLabel6.Text = sumofgood.ToString();
                metroLabel9.Text = sumofbad.ToString();
                metroLabel28.Text = sumofthreads.ToString();
                Thread.Sleep(500);
            }
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
                    richTextBox1.AppendText($"Ожидаю завершения работы всех потоков 🕒...\n");
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
                        File.AppendAllText(dirforsave + $"\\GoodHttp [~{srtimehttp} ms].txt", goodhts + "\n");
                    }
                    foreach (var goods4s in goodSOCKS4)
                    {
                        File.AppendAllText(dirforsave + $"\\GoodSOCKS4 [~{srtimes4} ms].txt", goods4s + "\n");
                    }
                    foreach (var goods5s in goodSOCKS5)
                    {
                        File.AppendAllText(dirforsave + $"\\GoodSOCKS5 [~{srtimes5} ms].txt", goods5s + "\n");
                    }
                    timer1.Enabled = false;
                    metroLabel31.Visible = false;
                    richTextBox1.AppendText($"Прошло времени со старта: {metroLabel31.Text}\n");
                    richTextBox1.AppendText("Сохранение завершено, открываю папку с результатами...\n");
                    Process.Start("explorer.exe", dirforsave);
                    if (isclosesaving)
                    {
                        isclosesaving = false;
                        richTextBox1.AppendText("Продолжаю завершение работы...\n");
                        richTextBox1.ScrollToCaret();
                        Thread.Sleep(2500);
                        Application.Exit();
                    }
                    metroButton1.Enabled = true;
                    clearlists.Start();
                }
                else if (goodHTTP.Count > 0 || goodSOCKS4.Count > 0 || goodSOCKS5.Count > 0)
                {
                    richTextBox1.AppendText("Сохраняю гуды в текстовые документы...\n");

                    foreach (var goodhts in goodHTTP)
                    {
                        File.AppendAllText(dirforsave + $"\\GoodHttp [~{srtimehttp} ms].txt", goodhts + "\n");
                    }
                    foreach (var goods4s in goodSOCKS4)
                    {
                        File.AppendAllText(dirforsave + $"\\GoodSOCKS4 [~{srtimes4} ms].txt", goods4s + "\n");
                    }
                    foreach (var goods5s in goodSOCKS5)
                    {
                        File.AppendAllText(dirforsave + $"\\GoodSOCKS5 [~{srtimes5} ms].txt", goods5s + "\n");
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

        private void metroLink1_Click(object sender, EventArgs e)
        {
            Thread clear = new Thread(ClearRich);
            clear.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void metroLabel2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Thread closesave = new Thread(SaveProx);
            closesave.IsBackground = true;

            if (ischecking)
            {
                timer1.Enabled = false;
                timer2.Enabled = false;
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


        private void CheckFrameWork()
        {
            try
            {
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
                        }
                    }
                }
                else
                {
                    richTextBox1.AppendText($"Пропускаю проверку версии фреймворка (Установленная версия: {version})...\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"При вроверке версии фреймворка произошла ошибка: {ex.Message}\n");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                srtimehttp = sumofmsechttp / goodHTTP.Count();
                metroLabel35.Text = $"~{srtimehttp} ms";
            }
            catch
            { }
            try
            {
                srtimes4 = sumofmsecs4 / goodSOCKS4.Count();
                metroLabel30.Text = $"~{srtimes4} ms";
            }
            catch
            { }
            try
            {
                srtimes5 = sumofmsecs5 / goodSOCKS5.Count();
                metroLabel34.Text = $"~{srtimes5} ms";
            }
            catch
            { }
        }


        private void CheckBadUrls()
        {
            if (bad_urls.Count > 0)
            {
                DialogResult result = MetroFramework.MetroMessageBox.Show(this, $"{BadUrlsList()}", "Небольшие проблемы", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    foreach (string url in bad_urls)
                    {
                        if (url.Contains("socks4"))
                        {
                            urlS4.Remove(url);
                            try
                            {
                                using (StreamWriter stream = new StreamWriter(directory + "\\urlsSOCKS4.txt", false))
                                {
                                    foreach (string urlka in urlS4)
                                    {
                                        stream.WriteAsync(urlka + "\n");
                                    }
                                    stream.Close();
                                }
                            }
                            catch { }
                        }
                        else if (url.Contains("socks5"))
                        {
                            urlS5.Remove(url);
                            try
                            {
                                using (StreamWriter stream = new StreamWriter(directory + "\\urlsSOCKS5.txt", false))
                                {
                                    foreach (string urlka in urlS5)
                                    {
                                        stream.WriteAsync(urlka + "\n");
                                    }
                                    stream.Close();
                                }
                            }
                            catch { }
                        }
                        else if (url.StartsWith("https") && url.Contains("http"))
                        {
                            urlHT.Remove(url);
                            try
                            {
                                using (StreamWriter stream = new StreamWriter(directory + "\\urlsHTTP.txt", false))
                                {
                                    foreach (string urlka in urlHT)
                                    {
                                        stream.WriteAsync(urlka + "\n");
                                    }
                                    stream.Close();
                                }
                            }
                            catch { }
                        }
                    }
                    richTextBox1.AppendText("Все сломанные ссылки удалены! Сейчас произойдёт перезапуск программы...\n");
                    Thread.Sleep(2000);
                    Application.Restart();
                }
            }
        }

        private string BadUrlsList()
        {
            string done_list = "Во время грабба прокси были обнаружены ошибки связанные с следующими ссылками:\n";
            foreach (string url in bad_urls)
            {
                done_list += url + "\n";
            }
            done_list += "Рекомендуется удалить их из списка ссылок, попытаться сделать удаление автоматически?";
            return done_list;
        }

        private void metroLink2_Click(object sender, EventArgs e)
        {
            using (FileDialog fl = new OpenFileDialog())
            {
                fl.Filter = "Текстовые документы (*.txt)|*.txt";
                fl.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                fl.Title = "Выберите текстовый документ с списком прокси...";
                DialogResult result = fl.ShowDialog();
                if (result == DialogResult.OK && fl.FileName.Length > 0)
                {
                    file_dialog = fl.FileName.ToString();
                    metroLink2.Text = file_dialog;
                    if (file_dialog.ToLower().Contains("http"))
                    {
                        metroRadioButton1.Checked = true;
                    }
                    else if (file_dialog.ToLower().Contains("socks4"))
                    {
                        metroRadioButton2.Checked = true;
                    }
                    else if (file_dialog.ToLower().Contains("socks5"))
                    {
                        metroRadioButton3.Checked = true;
                    }
                    metroButton3.Enabled = true;
                }
                else
                {
                    richTextBox1.AppendText("Вы не выбрали файл с списком прокси!\n");
                    metroButton3.Enabled = false;
                }
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            try
            {
                string[] proxys = File.ReadAllLines(file_dialog);
                if (metroRadioButton1.Checked)
                {
                    foreach (string proxy in proxys)
                    {
                        gottedHTTP.Add(proxy);
                        proxloadedHT++;
                    }
                    richTextBox1.AppendText($"Было добавлено {proxys.Count()} HTTP прокси.\n");
                }
                if (metroRadioButton2.Checked)
                {
                    foreach (string proxy in proxys)
                    {
                        gottedSOCKS4.Add(proxy);
                        proxloadedS4++;
                    }
                    richTextBox1.AppendText($"Было добавлено {proxys.Count()} SOCKS4 прокси.\n");
                }
                if (metroRadioButton3.Checked)
                {
                    foreach (string proxy in proxys)
                    {
                        gottedSOCKS5.Add(proxy);
                        proxloadedS5++;
                    }
                    richTextBox1.AppendText($"Было добавлено {proxys.Count()} SOCKS5 прокси.\n");
                }
                Applyhead();
                richTextBox1.AppendText($"Импорт прокси из файла выполнен успешно!\n");
                file_dialog = "";
                metroLink2.Text = "Выбрать файл 🗒";
                metroButton3.Enabled = false;
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Во время импорта прокси из файла произошла ошибка: {ex.Message}\n");
            }
        }

        private void metroLink3_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Начинаю сохранение непрочеканных прокси...\n");
            string create_dir = directory + $"\\Nochecked\\Proxy[{DateTime.Now.Day}.{DateTime.Now.Month}] [{DateTime.Now.Hour} {DateTime.Now.Minute}]";
            if (Directory.Exists(directory +"\\NoChecked"))
            {
                richTextBox1.AppendText("Сохраняю непрочеканные прокси...\n");
                Directory.CreateDirectory(create_dir);
                if (gottedHTTP.Count > 0)
                {
                    foreach (string ht_prox in gottedHTTP)
                    {
                        File.AppendAllText(create_dir + "\\HttpProxyList.txt", ht_prox + "\n");
                    }
                    richTextBox1.AppendText("Непрочеканные Http прокси сохранены!\n");
                }
                if (gottedSOCKS4.Count > 0)
                {
                    foreach (string s4_prox in gottedSOCKS4)
                    {
                        File.AppendAllText(create_dir + "\\Socks4ProxyList.txt", s4_prox + "\n");
                    }
                    richTextBox1.AppendText("Непрочеканные Socks4 прокси сохранены!\n");
                }
                if (gottedSOCKS5.Count > 0)
                {
                    foreach (string s5_prox in gottedSOCKS5)
                    {
                        File.AppendAllText(create_dir + "\\Socks5ProxyList.txt", s5_prox + "\n");
                    }
                    richTextBox1.AppendText("Непрочеканные Socks5 прокси сохранены!\n");
                }
                richTextBox1.AppendText("Открываю папку с сохранёнными прокси...\n");
                Process.Start("explorer.exe", create_dir);
            }
            else
            {
                richTextBox1.AppendText("Создаю специальную папку для непрочеканных прокси...\n");
                Directory.CreateDirectory(directory + "\\NoChecked");
                metroLink3.PerformClick();
            }
        }
    }
}

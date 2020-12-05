using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace PGC
{
    class Workers
    {
        public Form1 mainform;
        public int our_index;
        public List<Thread> threads = new List<Thread>();

        Thread ccheck;

        public void StartWork(int index)
        {
            ccheck = new Thread(CountChecker);
            ccheck.IsBackground = true;
            ccheck.Start();
            if (index == 1)
            {
                our_index = 1;
                for (int i = 0; i < mainform.threadsCount; i++)
                {
                    Thread starthttp = new Thread(HttpWorker);
                    starthttp.IsBackground = true;
                    starthttp.Start();
                    threads.Add(starthttp);
                }
            }
            if (index == 2)
            {
                our_index = 2;
                for (int i = 0; i < mainform.threadsCount; i++)
                {
                    Thread startsocks4 = new Thread(Socks4Worker);
                    Thread startsocks5 = new Thread(Socks5Worker);
                    startsocks4.IsBackground = true;
                    startsocks5.IsBackground = true;
                    startsocks4.Start();
                    startsocks5.Start();
                    threads.Add(startsocks4);
                    threads.Add(startsocks5);
                }
            }
            if (index == 3)
            {
                our_index = 3;
                for (int i = 0; i < mainform.threadsCount; i++)
                {
                    Thread starthttp = new Thread(HttpWorker);
                    Thread startsocks4 = new Thread(Socks4Worker);
                    Thread startsocks5 = new Thread(Socks5Worker);
                    starthttp.IsBackground = true;
                    startsocks4.IsBackground = true;
                    startsocks5.IsBackground = true;
                    starthttp.Start();
                    startsocks4.Start();
                    startsocks5.Start();
                    threads.Add(starthttp);
                    threads.Add(startsocks4);
                    threads.Add(startsocks5);
                }
            }
        }

        public void StopWork()
        {
            foreach (Thread thread in threads)
            {
                if (thread.ThreadState != System.Threading.ThreadState.Aborted)
                {
                    thread.Abort();
                }
            }
            threads.Clear();
        }

        public void HttpWorker()
        {
            try
            {
                while (mainform.gottedHTTP.Count != 0)
                {
                    if (mainform.gottedHTTP[0] != null && mainform.ischecking)
                    {
                        using (var http_req = new HttpRequest())
                        {
                            Stopwatch stopwatch = new Stopwatch();
                            try
                            {

                                http_req.Proxy = HttpProxyClient.Parse(mainform.gottedHTTP[0].Trim(new char[] { ' ' }));
                                mainform.gottedHTTP.RemoveAt(0);
                                mainform.label2.Text = $"({mainform.gottedHTTP.Count()})";
                                http_req.Proxy.ConnectTimeout = mainform.timeout / 2;
                                http_req.ConnectTimeout = mainform.timeout / 2;
                                http_req.KeepAlive = false;
                                stopwatch.Start();
                                http_req.Get(mainform.host);
                                if (http_req.Response.IsOK)
                                {
                                    stopwatch.Stop();
                                    mainform.sumofmsechttp += Convert.ToInt32(stopwatch.ElapsedMilliseconds);
                                    mainform.goodHTTP.Add(http_req.Proxy.ToString());
                                    mainform.metroLabel22.Text = mainform.goodHTTP.Count().ToString();
                                    mainform.richTextBox1.AppendText($"✔ - (HTTP) {http_req.Proxy} - прошёл проверку! [Время отклика: {stopwatch.ElapsedMilliseconds} ms]\n");
                                }
                                else
                                {
                                    stopwatch.Stop();
                                    mainform.badhttp++;
                                    mainform.richTextBox1.AppendText($"✖ - (HTTP) {http_req.Proxy} - не работает!\n");
                                    mainform.metroLabel20.Text = mainform.badhttp.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                stopwatch.Stop();
                                if (ex.Message != "Поток находился в процессе прерывания." && mainform.ischecking)
                                {
                                    mainform.badhttp++;
                                    mainform.richTextBox1.AppendText($"✖ - (HTTP) {http_req.Proxy} - не работает! ({ex.Message})\n");
                                    mainform.metroLabel20.Text = mainform.badhttp.ToString();
                                }
                            }
                            http_req.Close();
                        }
                    }
                    else //Во избежание нагрузки и вылета чистим список и заканчиваем чек этого типа
                    {
                        mainform.gottedHTTP.Clear();
                        mainform.badhttp++;
                        mainform.label2.Text = $"({mainform.gottedHTTP.Count()})";
                        Thread.CurrentThread.Abort();
                    }
                }
            }
            catch
            {

            }
        }

        public void Socks4Worker()
        {
            try
            {
                while (mainform.gottedSOCKS4.Count != 0)
                {
                    if (mainform.gottedSOCKS4[0] != null && mainform.ischecking)
                    {
                        using (var socks4_req = new HttpRequest())
                        {
                            Stopwatch stopwatch2 = new Stopwatch();
                            try
                            {

                                socks4_req.Proxy = Socks4ProxyClient.Parse(mainform.gottedSOCKS4[0].Trim(new char[] { ' ' }));
                                mainform.gottedSOCKS4.RemoveAt(0);
                                mainform.label3.Text = $"({mainform.gottedSOCKS4.Count()})";
                                socks4_req.Proxy.ConnectTimeout = mainform.timeout / 2;
                                socks4_req.ConnectTimeout = mainform.timeout / 2;
                                socks4_req.KeepAlive = false;
                                stopwatch2.Start();
                                socks4_req.Get(mainform.host);
                                if (socks4_req.Response.IsOK)
                                {
                                    stopwatch2.Stop();
                                    mainform.sumofmsecs4 += Convert.ToInt32(stopwatch2.ElapsedMilliseconds);
                                    mainform.goodSOCKS4.Add(socks4_req.Proxy.ToString());
                                    mainform.metroLabel17.Text = mainform.goodSOCKS4.Count().ToString();
                                    mainform.richTextBox1.AppendText($"✔ - (SOCKS4) {socks4_req.Proxy} - прошёл проверку! [Время отклика: {stopwatch2.ElapsedMilliseconds} ms]\n");
                                }
                                else
                                {
                                    stopwatch2.Stop();
                                    mainform.bads4++;
                                    mainform.richTextBox1.AppendText($"✖ - (SOCKS4) {socks4_req.Proxy} - не работает!\n");
                                    mainform.metroLabel15.Text = mainform.bads4.ToString();
                                }

                            }
                            catch (Exception ex)
                            {
                                stopwatch2.Stop();
                                if (ex.Message != "Поток находился в процессе прерывания." && mainform.ischecking)
                                {
                                    mainform.bads4++;
                                    mainform.richTextBox1.AppendText($"✖ - (SOCKS4) {socks4_req.Proxy} - не работает! ({ex.Message})\n");
                                    mainform.metroLabel15.Text = mainform.bads4.ToString();
                                }
                            }
                            socks4_req.Close();
                        }
                    }
                    else //Во избежание нагрузки и вылета чистим список и заканчиваем чек этого типа
                    {
                        mainform.gottedSOCKS4.Clear();
                        mainform.bads4++;
                        mainform.label3.Text = $"({mainform.gottedSOCKS4.Count()})";
                        Thread.CurrentThread.Abort();
                    }
                }
            }
            catch
            {

            }
        }

        public void Socks5Worker()
        {
            try
            {
                while (mainform.gottedSOCKS5.Count != 0)
                {
                    if (mainform.gottedSOCKS5[0] != null && mainform.ischecking)
                    {
                        using (var socks5_req = new HttpRequest())
                        {
                            Stopwatch stopwatch3 = new Stopwatch();
                            try
                            {

                                socks5_req.Proxy = Socks5ProxyClient.Parse(mainform.gottedSOCKS5[0].Trim(new char[] { ' ' }));
                                mainform.gottedSOCKS5.RemoveAt(0);
                                mainform.label4.Text = $"({mainform.gottedSOCKS5.Count()})";
                                socks5_req.Proxy.ConnectTimeout = mainform.timeout / 2;
                                socks5_req.ConnectTimeout = mainform.timeout / 2;
                                socks5_req.KeepAlive = false;
                                stopwatch3.Start();
                                socks5_req.Get(mainform.host);
                                if (socks5_req.Response.IsOK)
                                {
                                    stopwatch3.Stop();
                                    mainform.sumofmsecs5 += Convert.ToInt32(stopwatch3.ElapsedMilliseconds);
                                    mainform.goodSOCKS5.Add(socks5_req.Proxy.ToString());
                                    mainform.metroLabel10.Text = mainform.goodSOCKS5.Count().ToString();
                                    mainform.richTextBox1.AppendText($"✔ - (SOCKS5) {socks5_req.Proxy} - прошёл проверку! [Время отклика: {stopwatch3.ElapsedMilliseconds} ms]\n");
                                }
                                else
                                {
                                    stopwatch3.Stop();
                                    mainform.bads4++;
                                    mainform.richTextBox1.AppendText($"✖ - (SOCKS5) {socks5_req.Proxy} - не работает!\n");
                                    mainform.metroLabel12.Text = mainform.bads5.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                stopwatch3.Stop();
                                if (ex.Message != "Поток находился в процессе прерывания." && mainform.ischecking)
                                {
                                    mainform.bads5++;
                                    mainform.richTextBox1.AppendText($"✖ - (SOCKS5) {socks5_req.Proxy} - не работает! ({ex.Message})\n");
                                    mainform.metroLabel12.Text = mainform.bads5.ToString();
                                }
                            }
                            socks5_req.Close();
                        }
                    }
                    else //Во избежание нагрузки и вылета чистим список и заканчиваем чек этого типа
                    {
                        mainform.gottedSOCKS5.Clear();
                        mainform.bads5++;
                        mainform.label4.Text = $"({mainform.gottedSOCKS5.Count()})";
                        Thread.CurrentThread.Abort();
                    }
                }
            }
            catch
            { }
        }

        public void CountChecker()
        {
            try
            {
                while (mainform.ischecking)
                {
                    if (our_index == 1 & mainform.gottedHTTP.Count == 0)
                    {
                        mainform.metroButton1.PerformClick();
                    }
                    else if (our_index == 2 & mainform.gottedSOCKS4.Count == 0 && mainform.gottedSOCKS5.Count == 0)
                    {
                        mainform.metroButton1.PerformClick();
                    }
                    else if (our_index == 3 & mainform.gottedHTTP.Count == 0 && mainform.gottedSOCKS4.Count == 0 && mainform.gottedSOCKS5.Count == 0)
                    {
                        mainform.metroButton1.PerformClick();
                    }
                    else
                    {
                        Thread.Sleep(200);
                    }
                }
            }
            catch
            {

            }
        }
    }
}


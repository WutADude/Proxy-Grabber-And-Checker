using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;
using System.Net.Http;
using xNet;

namespace PGC
{
    /*
        !!Идея для чекера!!
        По окончанию работы каждого воркера, после удаления потока из списка потоков сделать проверку на кол-во потоков в списке. Если кол-во потоков равняется 0, то вызывать метод останавливающий работу.
     */
    class Functions // Класс с набором методов, для осуществления функций
    {
        //Формы
        public mainWorkForm _mainForm; //Ссылка на главную форму, для работы с ней

        // Папки
        public string _mainDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\PGC"; // Корневая папка программы
        public string _directoryForSaveGoods = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\PGC\\Goods\\Good ({DateTime.Now.Day}.{DateTime.Now.Month} [{DateTime.Now.Hour}.{DateTime.Now.Minute}])"; // Папка для сохранения гудов

        //Реестр
        public string _registryFrameworkVersion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client").GetValue("version").ToString(); //ветка в реестре  версии фреймворка
        public RegistryKey _mainRegistryDirectory; // Корневая ветка реестра программы

        //Регулярки
        public Regex _proxyGrabbRegularPattern = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}"); // Паттерн регулярного выражения для нахождения прокси

        //Настройки (int)
        public int _typeOfChecking = 5; // Тип прокси, которые будут чекаться, по стандарту чек всех, то есть 5 (1 - Чек HTTP, 2- Чек SOCKS4, 3 - Чек SOCKS5, 4 - Чек Обоих SOCKS'оф, 5 - Чек всех типов)
        public int _threadsCount = 1; // Кол-во потоков, по стандарту 1
        public int _timeoutNumber = 2000; // Значение Timeout регулируемое пользователем, по стандарту равно 2000
        public int _factorOfThreads = 3; // Множитель, то есть число, на которое будет умножено кол-во потоков после старта.

        //Настройки (string)
        public string _hostToCheck = "https://www.google.com"; // Ссылка на ресурс, к которому будут попытки подключения с использованием прокси (По стандарту google)
        public string _requestMethod = "GET"; // Тип отправляемого запроса чекером, по стандарту GET

        //Настройки (bool)
        public bool _useKeepAlive = false; //Использовать параметр keepAlive или нет, по стандарту False, то есть не используется

        //Счётчики Bad'ов
        public int _badHttpProxyCount; // Счётчик плохих Http прокси
        public int _badSocks4ProxyCount; // Счётчик плохих Socks4 прокси
        public int _badSocks5ProxyCount; // Счётчик плохих Socks5 прокси

        //Счётчики затраченных милисекунд
        public int _sumOfElapsedTimeHttp; // Сумма милисекунд на подключение с Http прокси
        public int _sumOfElapsedTimeSocks4; // Сумма милисекунд на подключение с Socks4 прокси
        public int _sumOfElapsedTimeSocks5; // Сумма милисекунд на подключение с Socks5 прокси

        //Списки (string)
        public List<string> _httpProxyUrls = new List<string>(); // Список ссылок для грабба Http прокси
        public List<string> _socks4ProxyUrls = new List<string>(); // Список ссылок для грабба Socks4 прокси
        public List<string> _socks5ProxyUrls = new List<string>(); // Список ссылок для грабба Socks5 прокси
        public List<string> _badUrls = new List<string>(); // Список ссылок, которые в случае чего можно удалить автоматически
        public List<string> _goodHttpProxyList = new List<string>(); // Список отчеканных, хороших Http прокси
        public List<string> _goodSocks4ProxyList = new List<string>(); // Список отчеканных, хороших Socks4 прокси
        public List<string> _goodSocks5ProxyList = new List<string>(); // Список отчеканных, хороших Socks5 прокси

        public List<string> _usableHeaders = new List<string>();

        //Списки (прочие)
        public List<Thread> _threadsList = new List<Thread>(); // Список потоков
        public List<Control> _controlsToRemoteList; // Список элементов интерфейса для управления

        // Очереди
        public Queue<string> _httpProxyList = new Queue<string>(); // Очередь HTTP прокси для чека
        public Queue<string> _socks4ProxyList = new Queue<string>(); // Очередь SOCKS4 прокси для чека
        public Queue<string> _socks5ProxyList = new Queue<string>(); // Очередь SOCKS5 прокси для чека


        // ФУНКЦИИ

        public void CheckFirstRun() // Метод проверки на первый запуск (ГЛАВНЫЙ МЕТОД, СТАРТУЕТ САМЫЙ ПЕРВЫЙ) Также здесь список контролов для управления
        {
            if (!Directory.Exists(_mainDirectory)) // Проверка, существует корневая ли папка или нет
            {
                _mainForm.logRichBox.AppendText($"Произошёл первый запуск программы, выполняю первоначальные настройки и создаю папку по пути: {_mainDirectory}\n");
                Directory.CreateDirectory(_mainDirectory);
                File.WriteAllText($"{_mainDirectory}\\urlsHTTP.txt", "https://api.proxyscrape.com?request=getproxies&proxytype=http&timeout=10000&country=all&anonymity=all&ssl=all" + "\n"
                        + "https://www.proxy-list.download/api/v1/get?type=http" + "\n"
                        + "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt" + "\n"); // Создаю текстовый документ с ссылками для грабба Http прокси
                File.WriteAllText($"{_mainDirectory}\\urlsSOCKS4.txt", "https://api.proxyscrape.com?request=getproxies&proxytype=socks4&timeout=10000&country=all&anonymity=all&ssl=all" + "\n"
                    + "https://www.proxy-list.download/api/v1/get?type=socks4" + "\n"
                    + "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt" + "\n"); // Создаю текстовый документ с ссылками для грабба Socks4 прокси
                File.WriteAllText($"{_mainDirectory}\\urlsSOCKS5.txt", "https://api.proxyscrape.com?request=getproxies&proxytype=socks5&timeout=10000&country=all&anonymity=all&ssl=all" + "\n"
                    + "https://www.proxy-list.download/api/v1/get?type=socks5" + "\n"
                    + "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt" + "\n"); // Создаю текстовый документ с ссылками для грабба Socks5 прокси
                _mainForm.logRichBox.AppendText("Текстовые документы с ссылками для граббера прокси созданы!\n");
            }
            if (!RegistryCreated()) // Проверяю, существует ли путь в реестре или нет
            {
                _mainForm.logRichBox.AppendText("Создаю ветку с настройками программы в реестре...\n");
                Registry.CurrentUser.CreateSubKey("ProxyGrabberAndChecker"); // Создаю ветку в реестре
                _mainRegistryDirectory = Registry.CurrentUser.OpenSubKey("ProxyGrabberAndChecker", true); // Задаю глобальный путь к разделу реестра программы
                _mainRegistryDirectory.SetValue("autoUpdateProxy", 0); // Устанавливаю параметр автообновления прокси
                _mainRegistryDirectory.SetValue("lolzteamInfoShown", 0); // Устанавливаю параметр показанной информации пользователям с лолза
                _mainRegistryDirectory.SetValue("autoGrabProxy", 1); // Устанавливаю параметр автоматического граббера прокси
                _mainForm.logRichBox.AppendText("Ветка в реестре создана!\n");
            }
            else
            {
                _mainRegistryDirectory = Registry.CurrentUser.OpenSubKey("ProxyGrabberAndChecker", true); // Если ветка уже есть, то просто присваиваю переменной путь для дальнейшей работы
                if (_mainRegistryDirectory.GetValueNames().Contains("framework_good")) // Проверка на старую версию
                {
                    _mainForm.logRichBox.AppendText("Вы пользовались старой версией программы, сейчас я удалю старуюю ветку в реестре и перезапущу программу...\n");
                    Registry.CurrentUser.DeleteSubKey("ProxyGrabberAndChecker");
                    Application.Restart();
                }
            }

            _controlsToRemoteList = new List<Control>() // Добавляю элементы управления в список
            {
                _mainForm.checkAllRadioButton, _mainForm.checkBothSocksRadioButton, _mainForm.checkHttpRadioButton, _mainForm.checkSocks4RadioButton,
                _mainForm.checkSocks5RadioButton, _mainForm.timeoutNumberUpDown, _mainForm.hostEditTextBox, _mainForm.saveAllProxysLinkLabel, _mainForm.clearLogLink,
                _mainForm.threadsCounterTrackBar, _mainForm.soloUrlAddButton, _mainForm.severalUrlAddButton, _mainForm.parameterKeepAliveCheckBox,
                _mainForm.useHeaddersCheckBox, _mainForm.requestTypeComboBox, _mainForm.autoGrabCheckBox, _mainForm.autoUpdateProxyCheckBox
            };

            GetUrlsForGrabber();
        }
        public bool RegistryCreated() // Булевая, проверка существует ли ключ или нет
        {
            if (Registry.CurrentUser.OpenSubKey("ProxyGrabberAndChecker") == null)
                return false;
            else
                return true;
        }
        public void CheckRegistryParameters() //Проверка значений параметров в реестре и применение изменений (ТРЕТИЙ ПОСЛЕ ЗАПУСКА МЕТОД)
        {
            if ((int)_mainRegistryDirectory.GetValue("autoUpdateProxy") == 1) // Проверка на авто-обновление прокси
                _mainForm.autoUpdateProxyCheckBox.Checked = true;
            else
                _mainForm.autoUpdateProxyCheckBox.Checked = false;

            if ((int)_mainRegistryDirectory.GetValue("lolzteamInfoShown") == 0 && LoLZFinded()) // Маленькая просьба для пользователей с форума (Проверяю реестр и процессы компьютера)
            {
                MetroFramework.MetroMessageBox.Show(_mainForm, "Привет! Похоже ты скачал чекер с форума социальной инженерии LoLZTeam, не так-ли? Я знаю это, потому что сейчас в браузере у тебя открыт этот форум.\n" +
                    $"Нет, в этом нет никакой проблемы, я просто хочу попросить тебя {Environment.UserName}, оставить своё сообщение в теме с программой!\n" +
                    $"Я не прошу оставлять положительное сообщение, я хочу, чтобы ты выразил своё мнение насчёт данной программы и предложил свою мысль на тему её улучшения :D!\n" +
                    $"Заранее говорю спасибо за твоё сообщение :)! Приятного использования!", "Привет, спасибо за скачивание моего софта!", MessageBoxButtons.OK, MessageBoxIcon.Information); //Вывожу сообщение
                _mainRegistryDirectory.SetValue("lolzteamInfoShown", 1);
                _mainForm.logRichBox.AppendText($"Ещё раз привет, {Environment.UserName}! Смотри, я надеюсь ты внимательно прочитал показанное чуть ранее тебе сообщение, так вот, если ты всётаки решишь оставить свой отзыв, " +
                    $"то напиши в скобках после отзыва следующее словосочетание \"(Я хочу какать :Р)\", чтобы я понял, что ты очень внимательный пользователь. Заранее спасибо! :)\n");
            }

            if ((int)_mainRegistryDirectory.GetValue("autoGrabProxy") == 1) // Проверка значения реестра на автограбб
            {
                GrabProxy();
                _mainForm.autoGrabCheckBox.Checked = true;
            }
            else
            {
                _mainForm.logRichBox.AppendText($"Авто-граббер выключен, перед началом проверки пожалуйста импотрируйте прокси во кладке \"Граббер\" или во вкладке \"Импорт прокси из файла и прочие настройки\"\n");
                _mainForm.autoGrabCheckBox.Checked = false;
                _mainForm.grabProxyButton.Enabled = true;
            }
        }
        public void SaveGoodProxys() // Сохранение всех доступных гудов
        {
            if (_goodHttpProxyList.Count > 0 || _goodSocks4ProxyList.Count > 0 || _goodSocks5ProxyList.Count > 0)
            {
                _mainForm.logRichBox.AppendText("Выполняю сохранение проверенных прокси!\n");
                try
                {
                    Directory.CreateDirectory(_directoryForSaveGoods);
                    if (_goodHttpProxyList.Count > 0)
                        File.WriteAllLines($"{_directoryForSaveGoods}\\[{_goodHttpProxyList.Count}] GoodHttpProxys (~{_sumOfElapsedTimeHttp / _goodHttpProxyList.Count} ms).txt", _goodHttpProxyList);
                    if (_goodSocks4ProxyList.Count > 0)
                        File.WriteAllLines($"{_directoryForSaveGoods}\\[{_goodSocks4ProxyList.Count}] GoodSocks4Proxys (~{_sumOfElapsedTimeSocks4 / _goodSocks4ProxyList.Count} ms).txt", _goodSocks4ProxyList);
                    if (_goodSocks5ProxyList.Count > 0)
                        File.WriteAllLines($"{_directoryForSaveGoods}\\[{_goodSocks5ProxyList.Count}] GoodSocks5Proxys (~{_sumOfElapsedTimeSocks5 / _goodSocks5ProxyList.Count} ms).txt", _goodSocks5ProxyList);
                    _mainForm.logRichBox.AppendText("Все проверенные прокси успешно сохранены! Открываю папку...\n");
                    Process.Start("explorer.exe", _directoryForSaveGoods);
                }
                catch { }
            }
            else
                _mainForm.logRichBox.AppendText("Нет работающих прокси для сохранения!");

        }
        public void UpdateCountersLabels() // Метод обновления счётчиков
        {
            _mainForm.mainTitleLabel.Text = $"Proxy | Grabber and Checker | SOCKS5: [{_socks5ProxyList.Count()}] | SOCKS4: [{_socks4ProxyList.Count()}] | HTTP: [{_httpProxyList.Count()}]";
            _mainForm.factorLabel.Text = $"{_factorOfThreads} × ({_threadsCount})";
            _mainForm.startedThreadsLabel.Text = _threadsList.Count().ToString();
            _mainForm.allGoodsCountLabel.Text = $"{_goodHttpProxyList.Count() + _goodSocks4ProxyList.Count() + _goodSocks5ProxyList.Count()}";
            _mainForm.allBadsCountLabel.Text = $"{_badHttpProxyCount + _badSocks4ProxyCount + _badSocks5ProxyCount}";

            _mainForm.httpGoodProxyCount.Text = _goodHttpProxyList.Count.ToString();
            _mainForm.httpBadProxyCount.Text = _badHttpProxyCount.ToString();
            _mainForm.socks4GoodProxyCount.Text = _goodSocks4ProxyList.Count.ToString();
            _mainForm.socks4BadProxyCount.Text = _badSocks4ProxyCount.ToString();
            _mainForm.socks5GoodProxyCount.Text = _goodSocks5ProxyList.Count.ToString();
            _mainForm.socks5BadProxyCount.Text = _badSocks5ProxyCount.ToString();

            if (_goodSocks5ProxyList.Count != 0)
                _mainForm.middleMsSocks5Num.Text = $"{_sumOfElapsedTimeSocks5 / _goodSocks5ProxyList.Count} ms";
            if (_goodSocks4ProxyList.Count != 0)
                _mainForm.middleMsSocks4Num.Text = $"{_sumOfElapsedTimeSocks4 / _goodSocks4ProxyList.Count} ms";
            if (_goodHttpProxyList.Count != 0)
                _mainForm.middleMsHttpNum.Text = $"{_sumOfElapsedTimeHttp / _goodHttpProxyList.Count} ms";

        }
        public void ProcessBadUrls() // Метод обработки плохих ссылок, а точнее принятие определённых мер с неработающими ссылками
        {
            if (_badUrls.Count > 0) // Проверяю количество плохих ссылок
            {
                DialogResult _badUrlsMessage = MetroFramework.MetroMessageBox.Show(_mainForm, $"При граббе прокси были обнаружены ссылки, которые либо не содержали в себе прокси, либо не загрузились. Количество таких ссылок равно: {_badUrls.Count}. " +
                    $"Вы можете удалить их самостоятельно из текстовых документов в корневой папке программы, а можете нажать кнопку и программа попытается сделать это самостоятельно. Удалить плохие ссылки самостоятельно?", "У вас проблема? У нас решение!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation); // Создаю уведомление
                if (_badUrlsMessage == DialogResult.Yes) // Проверяю ответ на сообщение
                {
                    foreach (var _txtForDelete in Directory.GetFiles(_mainDirectory, "*.txt")) // Удаляю все текстовые документы в корневой папке программы (по логике их всегда 3)
                        File.Delete(_txtForDelete);

                    foreach (string badUrl in _badUrls) // Создаю цикл, для каждой плохой ссылки
                    {
                        if (_httpProxyUrls.Contains(badUrl)) // Проверка списков на содержание плохой ссылки
                            _httpProxyUrls.Remove(badUrl);
                        if (_socks4ProxyUrls.Contains(badUrl))
                            _socks4ProxyUrls.Remove(badUrl);
                        if (_socks5ProxyUrls.Contains(badUrl))
                            _socks5ProxyUrls.Remove(badUrl);
                    }

                    File.WriteAllLines($"{_mainDirectory}\\urlsHTTP.txt", _httpProxyUrls); // Создание файлов по новой с удалёнными ссылками
                    File.WriteAllLines($"{_mainDirectory}\\urlsSOCKS4.txt", _socks4ProxyUrls);
                    File.WriteAllLines($"{_mainDirectory}\\urlsSOCKS5.txt", _socks5ProxyUrls);

                    Application.Restart();
                }
            }
        }
        public void CloseOnChecking(FormClosingEventArgs e, bool _savingDone = false)  // Обработка закрытия формы
        {
            if (_threadsList.Count != 0 && !_savingDone) // Проверка на чекинг
            {
                e.Cancel = true; // Отменяю закрытие
                StopChecking(e, true); // Вызваю метод с аргументами
            }
            else
            {
                e.Cancel = false; // Разрешаю закрытие
                Application.Exit(); // Закрываю программу
            }
        }

        // Методы граббера
        public void GetUrlsForGrabber()// Метод, берёт ссылки для граббера из текстовых документов в корне программы и добавляет в коллекции (ВТОРОЙ ПОСЛЕ ЗАПУСКА МЕТОД)
        {
            string[] _httpUrls = File.ReadAllLines($"{_mainDirectory}\\urlsHTTP.txt");
            string[] _socks4Urls = File.ReadAllLines($"{_mainDirectory}\\urlsSOCKS4.txt");
            string[] _socks5Urls = File.ReadAllLines($"{_mainDirectory}\\urlsSOCKS5.txt");
            foreach (string _urlHttp in _httpUrls)
            {
                if (!_httpProxyUrls.Contains(_urlHttp) && _urlHttp != null && _urlHttp.StartsWith("http"))
                    _httpProxyUrls.Add(_urlHttp);
            }
            foreach (string _urlSocks4 in _socks4Urls)
            {
                if (!_socks4ProxyUrls.Contains(_urlSocks4) && _urlSocks4 != null && _urlSocks4.StartsWith("http"))
                    _socks4ProxyUrls.Add(_urlSocks4);
            }
            foreach (string _urlSocks5 in _socks5Urls)
            {
                if (!_socks5ProxyUrls.Contains(_urlSocks5) && _urlSocks5 != null && _urlSocks5.StartsWith("http"))
                    _socks5ProxyUrls.Add(_urlSocks5);
            }

            _mainForm.logRichBox.AppendText($"Количество ссылок для импорта HTTP прокси: {_httpProxyUrls.Count()}\n");
            _mainForm.logRichBox.AppendText($"Количество ссылок для импорта SOCKS4 прокси: {_socks4ProxyUrls.Count()}\n");
            _mainForm.logRichBox.AppendText($"Количество ссылок для импорта SOCKS5 прокси: {_socks5ProxyUrls.Count()}\n");

            CheckRegistryParameters();
        }
        public void GrabProxy() // Метод граббера прокси из ссылок (ЧЕТВЁРТЫЙ ПОСЛЕ ЗАПУСКА МЕТОД)
        {
            _mainForm.logRichBox.AppendText("Пытаюсь достать все типы прокси...\n");

            foreach (Control _controlToDisable in _controlsToRemoteList) // Делаю недоступными для выбора кнопки и другие элементы
                _controlToDisable.Enabled = false;
            _mainForm.mainStartButton.Enabled = false;

            for (int i = 0; i < _httpProxyUrls.Count(); i++) // Создаю цикл для грабба Http прокси
            {
                Thread _httpGrabberWorker = new Thread(GrabberHttpWorker); // Создаю поток для каждой ссылки
                _httpGrabberWorker.IsBackground = true;
                _httpGrabberWorker.Start(i);
                _threadsList.Add(_httpGrabberWorker);
            }

            for (int i = 0; i < _socks4ProxyUrls.Count(); i++) // Создаю цикл для грабба Socks4 прокси
            {
                Thread _socks4GrabberWorker = new Thread(GrabberSocks4Worker);
                _socks4GrabberWorker.IsBackground = true;
                _socks4GrabberWorker.Start(i);
                _threadsList.Add(_socks4GrabberWorker);
            }

            for (int i = 0; i < _socks5ProxyUrls.Count(); i++) // Создаю цикл для грабба Socks5 прокси
            {
                Thread _socks5GrabberWorker = new Thread(GrabberSocks5Worker);
                _socks5GrabberWorker.IsBackground = true;
                _socks5GrabberWorker.Start(i);
                _threadsList.Add(_socks5GrabberWorker);
            }

            do
            {
                Thread.Sleep(350);
            }
            while (_threadsList.Count != 0); // Цикл, который продолжается до тех пор, пока прокси не будут сграблены до конца
            _mainForm.logRichBox.AppendText($"Удалось достать {_httpProxyList.Count} HTTP, {_socks4ProxyList.Count} SOCKS4 и {_socks5ProxyList.Count} SOCKS5 прокси.\n"); // Финальное сообщение граббера в логе 
            _mainForm.mainStartButton.Enabled = true; // По стандарту кнопка не доступна для нажатия, после получения прокси становится доступной
            _mainForm.mainStartButton.Text = "Запустить"; // Изменяю текст кнопки, после получения прокси
            ProcessBadUrls();
            foreach (Control _controlToDisable in _controlsToRemoteList) // Возвращаю доступность элементов
                _controlToDisable.Enabled = true;
            _mainForm.mainStartButton.Enabled = true;

        }
        public void GrabberHttpWorker(object i)
        {
            try
            {
                HttpWebRequest _httpGrabProxy = WebRequest.CreateHttp(_httpProxyUrls[(int)i]); // Создаю запрос
                _httpGrabProxy.Method = "GET"; // Задаю метод запроса
                _httpGrabProxy.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
                HttpWebResponse _httpGrabResponse = (HttpWebResponse)_httpGrabProxy.GetResponse(); // Получаю ответ
                if (_httpGrabResponse.StatusCode == System.Net.HttpStatusCode.OK) // Проверка кода ответа, если 200 (ок) то иду дальше
                {
                    using (var _httpResponseStream = _httpGrabResponse.GetResponseStream()) // Чтение ответа
                    {
                        StreamReader _httpGrabStreamReader = new StreamReader(_httpResponseStream); // Создаю "считыватель" символов из ответа
                        MatchCollection _httpMatches = _proxyGrabbRegularPattern.Matches(_httpGrabStreamReader.ReadToEnd()); // Создание коллекции найденых регулярным выражением прокси
                        if (_httpMatches.Count > 0) // Проверка на пустоту коллекции
                        {
                            foreach (Match _httpMatch in _httpMatches) // Цикл, для каждого элемента коллекции
                            {
                                if (!_httpProxyList.Contains(_httpMatch.Value)) // Проверка на дубль
                                    _httpProxyList.Enqueue(_httpMatch.Value.Trim(new char[] { ' ', '\r', '\n' })); // Добавляю в очередь найденные прокси
                            }
                        }
                        else
                        {
                            _mainForm.logRichBox.AppendText($"По данной ссылке ({_httpProxyUrls[(int)i]}) не удалось найти HTTP прокси!\n"); // Вывод сообщения в лог о том, что не удалось найти прокси на сайте
                            if (!_badUrls.Contains(_httpProxyUrls[(int)i])) // Проверяю наличие этой ссылки в списке, если нет, то добавляю
                                _badUrls.Add(_httpProxyUrls[(int)i]);
                        }
                        _httpGrabStreamReader.Close();
                        _httpResponseStream.Close();
                    }
                }
                else
                {
                    if (!_badUrls.Contains(_httpProxyUrls[(int)i]))
                        _badUrls.Add(_httpProxyUrls[(int)i]); // Если сервер не ответил, то заносится в так называемый чёрный список
                    _mainForm.logRichBox.AppendText($"{_httpProxyUrls[(int)i]} - не ответил. Возможно сайт не работает.\n");
                }
                _httpGrabResponse.Close();
                _httpGrabProxy.Abort();
            }
            catch (Exception ex)
            {
                _mainForm.logRichBox.AppendText($"Во время грабба HTTP произошла ошибка: {ex.Message}\n");
                if (!_badUrls.Contains(_httpProxyUrls[(int)i]))
                    _badUrls.Add(_httpProxyUrls[(int)i]);
            }
            _threadsList.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();
        } // Граббер Http прокси
        public void GrabberSocks4Worker(object i)
        {
            try
            {
                HttpWebRequest _socks4GrabProxy = WebRequest.CreateHttp(_socks4ProxyUrls[(int)i]); // Создаю запрос
                _socks4GrabProxy.Method = "GET"; // Задаю метод запроса
                _socks4GrabProxy.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
                HttpWebResponse _socks4GrabResponse = (HttpWebResponse)_socks4GrabProxy.GetResponse(); // Получаю ответ
                if (_socks4GrabResponse.StatusCode == System.Net.HttpStatusCode.OK) // Проверка кода ответа, если 200 (ок) то иду дальше
                {
                    using (var _socks4ResponseStream = _socks4GrabResponse.GetResponseStream()) // Чтение ответа
                    {
                        StreamReader _socks4GrabStreamReader = new StreamReader(_socks4ResponseStream); // Создаю "считыватель" символов из ответа
                        MatchCollection _socks4Matches = _proxyGrabbRegularPattern.Matches(_socks4GrabStreamReader.ReadToEnd()); // Создание коллекции найденых регулярным выражением прокси
                        if (_socks4Matches.Count > 0) // Проверка на пустоту коллекции
                        {
                            foreach (Match _socks4Match in _socks4Matches) // Цикл, для каждого элемента коллекции
                            {
                                if (!_socks4ProxyList.Contains(_socks4Match.Value)) // Проверка на дубль
                                    _socks4ProxyList.Enqueue(_socks4Match.Value.Trim(new char[] { ' ', '\r', '\n' })); // Добавляю в очередь найденные прокси
                            }
                        }
                        else
                        {
                            _mainForm.logRichBox.AppendText($"По данной ссылке ({_socks4ProxyUrls[(int)i]}) не удалось найти SOCKS4 прокси!\n"); // Вывод сообщения в лог о том, что не удалось найти прокси на сайте
                            if (!_badUrls.Contains(_socks4ProxyUrls[(int)i]))
                                _badUrls.Add(_socks4ProxyUrls[(int)i]);
                        }
                        _socks4GrabStreamReader.Close();
                        _socks4ResponseStream.Close();
                    }
                }
                else
                {
                    if (!_badUrls.Contains(_socks4ProxyUrls[(int)i]))
                        _badUrls.Add(_socks4ProxyUrls[(int)i]); // Если сервер не ответил, то заносится в так называемый чёрный список
                    _mainForm.logRichBox.AppendText($"{_socks4ProxyUrls[(int)i]} - не ответил. Возможно сайт не работает.\n");
                }
                _socks4GrabResponse.Close();
                _socks4GrabProxy.Abort();
            }
            catch (Exception ex)
            {
                _mainForm.logRichBox.AppendText($"Во время грабба SOCKS4 произошла ошибка: {ex.Message}\n");
                if (!_badUrls.Contains(_socks4ProxyUrls[(int)i]))
                    _badUrls.Add(_socks4ProxyUrls[(int)i]);
            }
            _threadsList.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();

        } // Граббер Socks4 прокси
        public void GrabberSocks5Worker(object i)
        {
            try
            {
                HttpWebRequest _socks5GrabProxy = WebRequest.CreateHttp(_socks5ProxyUrls[(int)i]); // Создаю запрос
                _socks5GrabProxy.Method = "GET"; // Задаю метод запроса
                _socks5GrabProxy.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
                HttpWebResponse _socks5GrabResponse = (HttpWebResponse)_socks5GrabProxy.GetResponse(); // Получаю ответ
                if (_socks5GrabResponse.StatusCode == System.Net.HttpStatusCode.OK) // Проверка кода ответа, если 200 (ок) то иду дальше
                {
                    using (var _socks5ResponseStream = _socks5GrabResponse.GetResponseStream()) // Чтение ответа
                    {
                        StreamReader _sock5GrabStreamReader = new StreamReader(_socks5ResponseStream); // Создаю "считыватель" символов из ответа
                        MatchCollection _socks5Matches = _proxyGrabbRegularPattern.Matches(_sock5GrabStreamReader.ReadToEnd()); // Создание коллекции найденых регулярным выражением прокси
                        if (_socks5Matches.Count > 0) // Проверка на пустоту коллекции
                        {
                            foreach (Match _socks5Match in _socks5Matches) // Цикл, для каждого элемента коллекции
                            {
                                if (!_socks5ProxyList.Contains(_socks5Match.Value)) // Проверка на дубль
                                    _socks5ProxyList.Enqueue(_socks5Match.Value.Trim(new char[] { ' ', '\r', '\n' })); // Добавляю в очередь найденные прокси
                            }
                        }
                        else
                        {
                            _mainForm.logRichBox.AppendText($"По данной ссылке ({_socks5ProxyUrls[(int)i]}) не удалось найти SOCKS5 прокси!\n"); // Вывод сообщения в лог о том, что не удалось найти прокси на сайте
                            if (!_badUrls.Contains(_socks5ProxyUrls[(int)i]))
                                _badUrls.Add(_socks5ProxyUrls[(int)i]);
                        }
                        _sock5GrabStreamReader.Close();
                        _socks5ResponseStream.Close();
                    }
                }
                else
                {
                    if (!_badUrls.Contains(_socks5ProxyUrls[(int)i]))
                        _badUrls.Add(_socks5ProxyUrls[(int)i]); // Если сервер не ответил, то заносится в так называемый чёрный список
                    _mainForm.logRichBox.AppendText($"{_socks5ProxyUrls[(int)i]} - не ответил. Возможно сайт не работает.\n");
                }
                _socks5GrabResponse.Close();
                _socks5GrabProxy.Abort();
            }
            catch (Exception ex)
            {
                _mainForm.logRichBox.AppendText($"Во время грабба SOCKS5 произошла ошибка: {ex.Message}\n");
                if (!_badUrls.Contains(_socks5ProxyUrls[(int)i]))
                    _badUrls.Add(_socks5ProxyUrls[(int)i]);
            }
            _threadsList.Remove(Thread.CurrentThread);
            Thread.CurrentThread.Abort();

        } // Граббер socks5 проски

        // Функции кнопок
        public void StartChecking() // Метод, стартует чек прокси
        {
            foreach (Control _controlToDisable in _controlsToRemoteList) // Дисейблю все элементы на время чека
                _controlToDisable.Enabled = false;
            _mainForm.mainStartButton.Text = "Остановить";
            switch (_typeOfChecking) // Выбор типа прокси, который будет отчекан
            {
                case 1: // Http
                    for (int i = 0; i < _threadsCount; i++) // Создаю цикл, который будет создавать потоки в нужном кол-ве
                    {
                        Thread _checkHttp = new Thread(HttpCheckerWorker); // Создаю поток
                        _checkHttp.IsBackground = true;
                        _threadsList.Add(_checkHttp); // Добавляю поток в список потоков, для последующего удобного управления потоками
                        _checkHttp.Start(); // Стартую поток
                    }
                    break;
                case 2: // Socks4
                    for (int i = 0; i < _threadsCount; i++)
                    {
                        Thread _checkSocks4 = new Thread(Socks4CheckerWorker);
                        _checkSocks4.IsBackground = true;
                        _threadsList.Add(_checkSocks4);
                        _checkSocks4.Start();
                    }
                    break;
                case 3: // Socks5
                    for (int i = 0; i < _threadsCount; i++)
                    {
                        Thread _checkSocks5 = new Thread(Socks5CheckerWorker);
                        _checkSocks5.IsBackground = true;
                        _threadsList.Add(_checkSocks5);
                        _checkSocks5.Start();
                    }
                    break;
                case 4: // Socks4 & 5
                    for (int i = 0; i < _threadsCount; i++)
                    {
                        Thread _checkSocks4 = new Thread(Socks4CheckerWorker);
                        Thread _checkSocks5 = new Thread(Socks5CheckerWorker);
                        _checkSocks4.IsBackground = true;
                        _checkSocks5.IsBackground = true;
                        _threadsList.Add(_checkSocks4);
                        _threadsList.Add(_checkSocks5);
                        _checkSocks4.Start();
                        _checkSocks5.Start();
                    }
                    break;
                case 5: // Все типы прокси
                    for (int i = 0; i < _threadsCount; i++)
                    {
                        Thread _checkHttp = new Thread(HttpCheckerWorker);
                        Thread _checkSocks4 = new Thread(Socks4CheckerWorker);
                        Thread _checkSocks5 = new Thread(Socks5CheckerWorker);
                        _checkHttp.IsBackground = true;
                        _checkSocks4.IsBackground = true;
                        _checkSocks5.IsBackground = true;
                        _threadsList.Add(_checkHttp);
                        _threadsList.Add(_checkSocks4);
                        _threadsList.Add(_checkSocks5);
                        _checkHttp.Start();
                        _checkSocks4.Start();
                        _checkSocks5.Start();
                    }
                    break;
            }
        }
        public void StopChecking(FormClosingEventArgs e = null, bool _closeForm = false) // Метод, заканчивает работу всех потоков и выдаёт результаты
        {
            try
            {
                _mainForm.logRichBox.AppendText("Выполняю завершение чека. Проверяю потоки...\n");
                while (_threadsList.Count != 0) // Пока кол-во потоков != 0 останавливаю каждый поток в списке
                {
                    if (_threadsList[0].ThreadState != System.Threading.ThreadState.Aborted) // Проверка потоков, если поток не остановлен то останавливаю и удаляю из списка
                    {
                        _threadsList[0].Abort();
                        _threadsList.RemoveAt(0);
                    }
                }
                _mainForm.logRichBox.AppendText("Все потоки успешно остановлены!\n");

                foreach (Control _controlToEnable in _controlsToRemoteList)
                    _controlToEnable.Enabled = true;
                SaveGoodProxys();

                if (_closeForm) // после вызова метода при закрытии снова вызываю метод закрытия
                    CloseOnChecking(e, true);
            }
            catch { }
        }
        public void AddSingleUrl() // Добавление еденичной ссылки в выбранный список
        {
            if (_mainForm.soloUrlTextBox.Text.StartsWith("http") && _mainForm.typeProxyUrlComboBox.Text.Length > 1) // Проверка на то, что ссылка начинается верно и выбран тип прокси
            {
                switch (_mainForm.typeProxyUrlComboBox.Text.ToLower())
                {
                    case "http":
                        if (!_httpProxyUrls.Contains(_mainForm.soloUrlTextBox.Text))
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsHTTP.txt", $"\n{_mainForm.soloUrlTextBox.Text}");
                            _mainForm.logRichBox.AppendText($"Ссылка ({_mainForm.soloUrlTextBox.Text}) для грабба HTTP прокси была добавлена успешно!\n");
                            GetUrlsForGrabber();
                        }
                        else
                            _mainForm.logRichBox.AppendText($"Ссылка ({_mainForm.soloUrlTextBox.Text}) для грабба HTTP прокси НЕ была добавлена, так как такая ссылка в списке уже есть!\n");
                        break;
                    case "socks4":
                        if (!_socks4ProxyUrls.Contains(_mainForm.soloUrlTextBox.Text))
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsSOCKS4.txt", $"\n{_mainForm.soloUrlTextBox.Text}");
                            _mainForm.logRichBox.AppendText($"Ссылка ({_mainForm.soloUrlTextBox.Text}) для грабба SOCSK4 прокси была добавлена успешно!\n");
                            GetUrlsForGrabber();
                        }
                        else
                            _mainForm.logRichBox.AppendText($"Ссылка ({_mainForm.soloUrlTextBox.Text}) для грабба SOCKS4 прокси НЕ была добавлена, так как такая ссылка в списке уже есть!\n");
                        break;
                    case "socks5":
                        if (!_socks5ProxyUrls.Contains(_mainForm.soloUrlTextBox.Text))
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsSOCKS5.txt", $"\n{_mainForm.soloUrlTextBox.Text}");
                            _mainForm.logRichBox.AppendText($"Ссылка ({_mainForm.soloUrlTextBox.Text}) для грабба SOCSK4 прокси была добавлена успешно!\n");
                            GetUrlsForGrabber();
                        }
                        else
                            _mainForm.logRichBox.AppendText($"Ссылка ({_mainForm.soloUrlTextBox.Text}) для грабба SOCKS5 прокси НЕ была добавлена, так как такая ссылка в списке уже есть!\n");
                        break;
                }
            }
            else
                MetroFramework.MetroMessageBox.Show(_mainForm, "Пожалуйста введите ссылку для граббера в первое поле (ссылка обязательно должна начинаться с http:// или https://) и выберите тип прокси, которые он будет граббить во втором поле!", "Что-то пошло не так", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void AddMultipleUrls() // Добавление нескольких ссылок
        {
            string _standartString = "Введите ссылки через 'Enter'...";
            if (_mainForm.multipleUrlTextBox.Text.Length > 0 && _mainForm.multipleUrlTextBox.Text != _standartString && _mainForm.multipleUrlTextBox.Text.StartsWith("http")) // Проверка поля ввода
            {
                Queue<string> _writtenUrls = new Queue<string>(_mainForm.multipleUrlTextBox.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); // Создаю очередь и добавляю в неё ссылки
                while (_writtenUrls.Count != 0) // Пока количество ссылок не равно 0 прохожусь по каждому элементу очереди
                {
                    string _url = _writtenUrls.Dequeue();
                    if (_url.StartsWith("http")) // если ссылка начинается правильно то идём дальше
                    {
                        string _trimmedUrl = _url.TrimStart(new char[] { 'h', 't', 't', 'p', 's' }); // убираю начало ссылки для последующего её анализа
                        if (_trimmedUrl.ToLower().Contains("http") && !_httpProxyUrls.Contains(_url)) // если ссылка содержит http, то добавляю её в список http ссылок
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsHTTP.txt", $"\n{_url}");
                            _httpProxyUrls.Add(_url);
                            _mainForm.logRichBox.AppendText($"Ссылка успешно добавлена в список HTTP для импорта прокси. [{_url}]\n");
                        }
                        else
                            _mainForm.logRichBox.AppendText($"Ссылка ({_url}) для грабба HTTP прокси НЕ была добавлена, так как такая ссылка в списке уже есть!\n");

                        if (_trimmedUrl.ToLower().Contains("socks4") && !_socks4ProxyUrls.Contains(_url)) // если ссылка содержит socks4, то добавляю её в список socsk4 ссылок
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsSOCKS4.txt", $"\n{_url}");
                            _socks4ProxyUrls.Add(_url);
                            _mainForm.logRichBox.AppendText($"Ссылка успешно добавлена в список SOCKS4 для импорта прокси. [{_url}]\n");
                        }
                        else
                            _mainForm.logRichBox.AppendText($"Ссылка ({_url}) для грабба SOCKS4 прокси НЕ была добавлена, так как такая ссылка в списке уже есть!\n");

                        if (_trimmedUrl.ToLower().Contains("socks5") && !_socks5ProxyUrls.Contains(_url)) // если ссылка содержит socks5, то добавляю её в список socks5 ссылок
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsSOCKS5.txt", $"\n{_url}");
                            _socks5ProxyUrls.Add(_url);
                            _mainForm.logRichBox.AppendText($"Ссылка успешно добавлена в список SOCKS5 для импорта прокси. [{_url}]\n");
                        }
                        else
                            _mainForm.logRichBox.AppendText($"Ссылка ({_url}) для грабба SOCKS5 прокси НЕ была добавлена, так как такая ссылка в списке уже есть!\n");

                        if (!_httpProxyUrls.Contains(_url) && !_socks4ProxyUrls.Contains(_url) && !_socks5ProxyUrls.Contains(_url)) // Если ссылка не содержала предыдущие варианты, то добавляю во все списки
                        {
                            File.AppendAllText($"{_mainDirectory}\\urlsHTTP.txt", $"\n{_url}");
                            File.AppendAllText($"{_mainDirectory}\\urlsSOCKS4.txt", $"\n{_url}");
                            File.AppendAllText($"{_mainDirectory}\\urlsSOCKS5.txt", $"\n{_url}");
                            _httpProxyUrls.Add(_url);
                            _socks4ProxyUrls.Add(_url);
                            _socks5ProxyUrls.Add(_url);
                        }

                    }
                    else
                        _mainForm.logRichBox.AppendText($"{_url} - не является ссылкой!\n");
                }
                GrabProxy();
            }
            else
                MetroFramework.MetroMessageBox.Show(_mainForm, "Пожалуйста проверьте правильность введёных ссылок!\nСсылка должна начинаться с \"http://\" или \"https://\"", "Ошибка добавления ссылок!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void SaveAllProxyWithoutCheck() // Сохранение всех прокси без проверки
        {
            if (Directory.Exists($"{_mainDirectory}\\NoCheckedProxys")) // Проверяю, есть папка или нет
            {
                _mainForm.logRichBox.AppendText("Начинаю сохранение непрочеканных прокси...\n");
                string _nowDateDirectory = $"{_mainDirectory}\\NoCheckedProxys\\[{DateTime.Now.Hour}.{DateTime.Now.Minute}] AllProxys"; // Создаю в папке ещё одну папку для сохранения
                Directory.CreateDirectory(_nowDateDirectory);
                File.WriteAllLines($"{_nowDateDirectory}\\[{_httpProxyList.Count}] AllHttpProxys.txt", _httpProxyList); // Сохраняю HTTP прокси
                File.WriteAllLines($"{_nowDateDirectory}\\[{_socks4ProxyList.Count}] AllSocks4Proxys.txt", _socks4ProxyList); // Сохраняю SOCKS4 прокси
                File.WriteAllLines($"{_nowDateDirectory}\\[{_socks5ProxyList.Count}] AllSocks5Proxys.txt", _socks5ProxyList); // Сохраняю SOCKS5 прокси
                _mainForm.logRichBox.AppendText($"Непрочеканные прокси были успешно сохранены!\n");
                Process.Start("explorer.exe", _nowDateDirectory);
            }
            else //Если нет, то создаю и вызываю метод по новой
            {
                Directory.CreateDirectory($"{_mainDirectory}\\NoCheckedProxys");
                SaveAllProxyWithoutCheck();
            }
        }
        public void ChoseFileToGrabProxy() // Метод, для выбора файла для грабба прокси из него
        {
            using (FileDialog _choseFileToGrabProxy = new OpenFileDialog())
            {
                _choseFileToGrabProxy.Filter = "Текстовые документы(*.txt)| *.txt";
                _choseFileToGrabProxy.Title = "Выберите файл, который содержит в себе список прокси";
                _choseFileToGrabProxy.InitialDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads";
                DialogResult _chosenResult = _choseFileToGrabProxy.ShowDialog();
                if (_chosenResult == DialogResult.OK && _choseFileToGrabProxy.FileName != null)
                {
                    _mainForm.fileImportChoseButton.Text = _choseFileToGrabProxy.FileName;
                    _mainForm.httpProxyFileChosen.Checked = _choseFileToGrabProxy.FileName.ToLower().Contains("http") ? true : false; // Проверка, содержит ли имя файла тип прокси, если да, то у radioButton появляется галка
                    _mainForm.socks4ProxyFileChosen.Checked = _choseFileToGrabProxy.FileName.ToLower().Contains("socks4") ? true : false; // Спасибо за вариант реализации кода if else ютуб каналу XpucT :) (Да да, разраб Win10Tweaker)
                    _mainForm.socks5ProxyFileChosen.Checked = _choseFileToGrabProxy.FileName.ToLower().Contains("socks5") ? true : false; // Очень многому я научился благодаря данному человеку :) Хэ, скрытая реклама :D
                    _mainForm.importProxyFromFile.Enabled = true;
                }
                else
                {
                    _mainForm.importProxyFromFile.Enabled = false;
                    _mainForm.fileImportChoseButton.Text = "Выберите файл";
                }
            }
        }
        public void GrabProxyFromChosenFile() // Грабб прокси из выбранного файла
        {
            foreach (Control _controlsToDisable in _controlsToRemoteList) // Делаю недоступными для нажатия все кнопки
                _controlsToDisable.Enabled = false;
            _mainForm.mainStartButton.Enabled = false;
            try // отлавливаю ошибки
            {
                if (_mainForm.httpProxyFileChosen.Checked || _mainForm.socks4ProxyFileChosen.Checked || _mainForm.socks5ProxyFileChosen.Checked) // Проверяю, выбран ли тип прокси, который будет импортирован
                {
                    if (_mainForm.httpProxyFileChosen.Checked) // Если выбран HTTP то иду дальше 
                        foreach (string _httpProxy in File.ReadAllLines(_mainForm.fileImportChoseButton.Text)) // Прохожусь по каждой строке в выбранном файле
                            if (!_httpProxyList.Contains(_httpProxy) && _httpProxy != null && _proxyGrabbRegularPattern.IsMatch(_httpProxy)) // Проверяю на дубль, длину и совпадает ли с регулярным выражением
                                _httpProxyList.Enqueue(_httpProxy); // Добавляю в очередь

                    if (_mainForm.socks4ProxyFileChosen.Checked)
                        foreach (string _socks4Proxy in File.ReadAllLines(_mainForm.fileImportChoseButton.Text))
                            if (!_socks4ProxyList.Contains(_socks4Proxy) && _socks4Proxy != null && _proxyGrabbRegularPattern.IsMatch(_socks4Proxy))
                                _socks4ProxyList.Enqueue(_socks4Proxy);

                    if (_mainForm.socks5ProxyFileChosen.Checked)
                        foreach (string _socks5Proxy in File.ReadAllLines(_mainForm.fileImportChoseButton.Text))
                            if (_socks5ProxyList.Contains(_socks5Proxy) && _socks5Proxy != null && _proxyGrabbRegularPattern.IsMatch(_socks5Proxy))
                                _socks5ProxyList.Enqueue(_socks5Proxy);

                    _mainForm.logRichBox.AppendText("Прокси были успешно импортированны из файла!\n");
                }
                else
                    MetroFramework.MetroMessageBox.Show(_mainForm, "Выберите тип прокси, который будет импортирован из файла!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _mainForm.logRichBox.AppendText($"Во время импорта прокси из файла произошла ошибка: {ex.Message}\n"); // вывожу ошибку в лог для пользователя
            }
            foreach (Control _controlsToEnable in _controlsToRemoteList) // Делаю доступными для нажатия все кнопки
                _controlsToEnable.Enabled = true;
            _mainForm.fileImportChoseButton.Text = "Выберите файл";
            _mainForm.mainStartButton.Enabled = true;
            _mainForm.mainStartButton.Text = "Запустить";
        }

        // Методы чекера
        public void HttpCheckerWorker() // Чекер HTTP прокси
        {
            try
            {
                while (_httpProxyList.Count != 0) // Запускаю цикл, пока список прокси не равен 0 выполняю проверку
                {
                    Stopwatch _httpCheckerMSTimer = new Stopwatch(); // Создаю таймер
                    if (_threadsList.Count != 0) // Проверяю прокси
                    {
                        HttpRequest _checkerHttpRequest = new HttpRequest(); // Создаю запрос
                        _checkerHttpRequest.UserAgent = Http.ChromeUserAgent(); // Задаю параметр UserAgent
                        _checkerHttpRequest.KeepAlive = _useKeepAlive;
                        _checkerHttpRequest.ConnectTimeout = _timeoutNumber / 2; // Задаю Timeout подключения к ресурсу делёный на 2, так как таймаут подключения к прокси отдельный
                        _checkerHttpRequest.Proxy = HttpProxyClient.Parse(_httpProxyList.Dequeue()); // Задаю прокси
                        _checkerHttpRequest.Proxy.ConnectTimeout = _timeoutNumber / 2; // Задаю Timeout подключения к прокси
                        _checkerHttpRequest.IgnoreProtocolErrors = true;
                        if (_mainForm.useHeaddersCheckBox.Checked) // Проверка использования header-ов
                            foreach (string _header in _usableHeaders)
                            {
                                string[] _headerWithValue = _header.Split(new char[] { '|' });
                                _checkerHttpRequest.AddHeader(_headerWithValue[0], _headerWithValue[1]);
                            }
                        _httpCheckerMSTimer.Start(); // запускаю отсчёт
                        try
                        {
                            if (_requestMethod == "GET") // Выбор метода отправки запроса
                                _checkerHttpRequest.Get(_hostToCheck);
                            else
                                _checkerHttpRequest.Post(_hostToCheck);
                            _httpCheckerMSTimer.Stop(); // Останавливаю отсчёт
                            _checkerHttpRequest.Close();
                            _checkerHttpRequest.Dispose();
                            if (_checkerHttpRequest.Response.IsOK) // Проверка ответа
                            {
                                _sumOfElapsedTimeHttp += (int)_httpCheckerMSTimer.ElapsedMilliseconds; // Добавляю кол-во милисекунд к общему числу
                                _goodHttpProxyList.Add(_checkerHttpRequest.Proxy.ToString()); // Добавляю проверенный прокси к списку хороших
                                _mainForm.logRichBox.AppendText($"✔ - (HTTP) {_checkerHttpRequest.Proxy} - прошёл проверку! [Время отклика: {_httpCheckerMSTimer.ElapsedMilliseconds} ms]\n"); // Вывожу в лог
                            }
                            else
                            {
                                _badHttpProxyCount++; // Инкрементирую число плохих прокси
                                _mainForm.logRichBox.AppendText($"✖ - (HTTP) {_checkerHttpRequest.Proxy} - не работает!\n");
                            }
                        }
                        catch (ThreadAbortException) { } // Отлавливаю исключения завершения потока
                        catch (Exception ex) // Отлавливаю остальные исключения
                        {
                            _httpCheckerMSTimer.Stop();
                            _badHttpProxyCount++;
                            _mainForm.logRichBox.AppendText($"✖ - (HTTP) {_checkerHttpRequest.Proxy} - не работает! ({ex.Message})\n");
                        }

                    }

                }
                _threadsList.Remove(Thread.CurrentThread); // По окончании кол-ва прокси удаляю этот поток из списка потоков
                if (_threadsList.Count == 0 && _mainForm.mainStartButton.Text != "Запустить") // Если кол-во потоков становится = 0, то выполняю завершение проверки
                {
                    StopChecking();
                }
                else // Если нет, то завершаю этот поток
                    Thread.CurrentThread.Abort();
            }
            catch (ThreadAbortException) { }
            catch 
            {
                _threadsList.Remove(Thread.CurrentThread);
                _mainForm.logRichBox.AppendText($"! - (HTTP) Программа должна была вылететь, но этого удалось избежать пожертвовав потоком.\n");
                Thread.CurrentThread.Abort();
            }
        }
        public void Socks4CheckerWorker() // Чекер SOCKS4 прокси
        {
            try
            {
                while (_socks4ProxyList.Count != 0) // Запускаю цикл, пока список прокси не равен 0 выполняю проверку
                {
                    Stopwatch _socks4CheckerMSTimer = new Stopwatch(); // Создаю таймер
                    if (_threadsList.Count != 0) // Проверяю прокси
                    {
                        HttpRequest _checkerSocks4Request = new HttpRequest(); // Создаю запрос
                        _checkerSocks4Request.UserAgent = Http.ChromeUserAgent(); // Задаю параметр UserAgent
                        _checkerSocks4Request.KeepAlive = _useKeepAlive;
                        _checkerSocks4Request.ConnectTimeout = _timeoutNumber / 2; // Задаю Timeout подключения к ресурсу делёный на 2, так как таймаут подключения к прокси отдельный
                        _checkerSocks4Request.Proxy = Socks4ProxyClient.Parse(_socks4ProxyList.Dequeue()); // Задаю прокси
                        _checkerSocks4Request.Proxy.ConnectTimeout = _timeoutNumber / 2; // Задаю Timeout подключения к прокси
                        _checkerSocks4Request.IgnoreProtocolErrors = true;
                        if (_mainForm.useHeaddersCheckBox.Checked) // Проверка использования header-ов
                            foreach (string _header in _usableHeaders)
                            {
                                string[] _headerWithValue = _header.Split(new char[] { '|' });
                                _checkerSocks4Request.AddHeader(_headerWithValue[0], _headerWithValue[1]);
                            }
                        _socks4CheckerMSTimer.Start(); // запускаю отсчёт
                        try
                        {
                            if (_requestMethod == "GET") // Выбор метода отправки запроса
                                _checkerSocks4Request.Get(_hostToCheck);
                            else
                                _checkerSocks4Request.Post(_hostToCheck);
                            _socks4CheckerMSTimer.Stop(); // Останавливаю отсчёт
                            _checkerSocks4Request.Close();
                            _checkerSocks4Request.Dispose();
                            if (_checkerSocks4Request.Response.IsOK) // Проверка ответа
                            {
                                _sumOfElapsedTimeSocks4 += (int)_socks4CheckerMSTimer.ElapsedMilliseconds; // Добавляю кол-во милисекунд к общему числу
                                _goodSocks4ProxyList.Add(_checkerSocks4Request.Proxy.ToString()); // Добавляю проверенный прокси к списку хороших
                                _mainForm.logRichBox.AppendText($"✔ - (SOCKS4) {_checkerSocks4Request.Proxy} - прошёл проверку! [Время отклика: {_socks4CheckerMSTimer.ElapsedMilliseconds} ms]\n"); // Вывожу в лог
                            }
                            else
                            {
                                _badSocks4ProxyCount++; // Инкрементирую число плохих прокси
                                _mainForm.logRichBox.AppendText($"✖ - (SOCKS4) {_checkerSocks4Request.Proxy} - не работает!\n");
                            }
                        }
                        catch (ThreadAbortException) { }
                        catch (IndexOutOfRangeException) { }
                        catch (Exception ex)
                        {
                            _socks4CheckerMSTimer.Stop();
                            _badSocks4ProxyCount++;
                            _mainForm.logRichBox.AppendText($"✖ - (SOCKS4) {_checkerSocks4Request.Proxy} - не работает! ({ex.Message})\n");
                        }
                    }

                }
                _threadsList.Remove(Thread.CurrentThread); // По окончании кол-ва прокси удаляю этот поток из списка потоков
                if (_threadsList.Count == 0 && _mainForm.mainStartButton.Text != "Запустить") // Если кол-во потоков становится = 0, то выполняю завершение проверки
                {
                    StopChecking();
                }
                else // Если нет, то завершаю этот поток
                    Thread.CurrentThread.Abort();
            }
            catch (ThreadAbortException) { }
            catch
            {
                _threadsList.Remove(Thread.CurrentThread);
                _mainForm.logRichBox.AppendText($"! - (SOCKS4) Программа должна была вылететь, но этого удалось избежать пожертвовав потоком.\n");
                Thread.CurrentThread.Abort();
            }
        }
        public void Socks5CheckerWorker() // Чекер SOCKS5 прокси
        {
            try
            {
                while (_socks5ProxyList.Count != 0) // Запускаю цикл, пока список прокси не равен 0 выполняю проверку
                {
                    Stopwatch _socks5CheckerMSTimer = new Stopwatch(); // Создаю таймер
                    if (_threadsList.Count != 0) // Проверяю прокси
                    {
                        HttpRequest _checkerSocks5Request = new HttpRequest(); // Создаю запрос
                        _checkerSocks5Request.UserAgent = Http.ChromeUserAgent(); // Задаю параметр UserAgent
                        _checkerSocks5Request.KeepAlive = _useKeepAlive;
                        _checkerSocks5Request.ConnectTimeout = _timeoutNumber / 2; // Задаю Timeout подключения к ресурсу делёный на 2, так как таймаут подключения к прокси отдельный
                        _checkerSocks5Request.Proxy = Socks5ProxyClient.Parse(_socks5ProxyList.Dequeue()); // Задаю прокси
                        _checkerSocks5Request.Proxy.ConnectTimeout = _timeoutNumber / 2; // Задаю Timeout подключения к прокси
                        _checkerSocks5Request.IgnoreProtocolErrors = true;
                        if (_mainForm.useHeaddersCheckBox.Checked) // Проверка использования header-ов
                            foreach (string _header in _usableHeaders)
                            {
                                string[] _headerWithValue = _header.Split(new char[] { '|' });
                                _checkerSocks5Request.AddHeader(_headerWithValue[0], _headerWithValue[1]);
                            }
                        _socks5CheckerMSTimer.Start(); // запускаю отсчёт
                        try
                        {
                            if (_requestMethod == "GET") // Выбор метода отправки запроса
                                _checkerSocks5Request.Get(_hostToCheck);
                            else
                                _checkerSocks5Request.Post(_hostToCheck);
                            _socks5CheckerMSTimer.Stop(); // Останавливаю отсчёт
                            _checkerSocks5Request.Close();
                            _checkerSocks5Request.Dispose();
                            if (_checkerSocks5Request.Response.IsOK) // Проверка ответа
                            {
                                _sumOfElapsedTimeSocks5 += (int)_socks5CheckerMSTimer.ElapsedMilliseconds; // Добавляю кол-во милисекунд к общему числу
                                _goodSocks5ProxyList.Add(_checkerSocks5Request.Proxy.ToString()); // Добавляю проверенный прокси к списку хороших
                                _mainForm.logRichBox.AppendText($"✔ - (SOCKS5) {_checkerSocks5Request.Proxy} - прошёл проверку! [Время отклика: {_socks5CheckerMSTimer.ElapsedMilliseconds} ms]\n"); // Вывожу в лог
                            }
                            else
                            {
                                _badSocks5ProxyCount++; // Инкрементирую число плохих прокси
                                _mainForm.logRichBox.AppendText($"✖ - (SOCKS5) {_checkerSocks5Request.Proxy} - не работает!\n");
                            }
                        }
                        catch (IndexOutOfRangeException) { }
                        catch (ThreadAbortException) { }
                        catch (Exception ex)
                        {
                            _socks5CheckerMSTimer.Stop();
                            _badSocks5ProxyCount++;
                            _mainForm.logRichBox.AppendText($"✖ - (SOCKS5) {_checkerSocks5Request.Proxy} - не работает! ({ex.Message})\n");
                        }
                    }
                }
                _threadsList.Remove(Thread.CurrentThread); // По окончании кол-ва прокси удаляю этот поток из списка потоков
                if (_threadsList.Count == 0 && _mainForm.mainStartButton.Text != "Запустить") // Если кол-во потоков становится = 0, то выполняю завершение проверки
                {
                    StopChecking();
                }
                else // Если нет, то завершаю этот поток
                    Thread.CurrentThread.Abort();
            }
            catch (ThreadAbortException) { }
            catch
            {
                _threadsList.Remove(Thread.CurrentThread);
                _mainForm.logRichBox.AppendText($"! - (SOCKS5) Программа должна была вылететь, но этого удалось избежать пожертвовав потоком.\n");
                Thread.CurrentThread.Abort();
            }
        }

        // Пасхалка для пользователей форума
        private bool LoLZFinded()
        {
            Process[] _allProcesses = Process.GetProcesses();
            bool _lolzIsFinded = false;
            foreach (var _oneProcess in _allProcesses)
            {
                if (_oneProcess.MainWindowTitle != null && _oneProcess.MainWindowTitle.ToLower().Contains("lolz.guru"))
                {
                    _lolzIsFinded = true;
                    break;
                }
            }
            return _lolzIsFinded;
        }
    }
}

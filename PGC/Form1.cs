using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;


namespace PGC
{
    public partial class mainWorkForm : MetroFramework.Forms.MetroForm
    {
        //Воркеры для чека прокси
        Functions _functions = new Functions();


        public mainWorkForm() // Инициализация формы
        {
            InitializeComponent();
            _functions._mainForm = this; // Присваиваю переменной в классе то, что эта форма является главной
        }

        private void metroTrackBar1_ValueChanged(object sender, EventArgs e) // Обработчик при скроле трекбара
        {
            _functions._threadsCount = threadsCounterTrackBar.Value;
            factorLabel.Text = $"{_functions._factorOfThreads} × ({_functions._threadsCount})";
        }

        private void Form1_Shown(object sender, EventArgs e) // Обработчик, при первом запуске формы
        {
            Thread _firstMethod = new Thread(_functions.CheckFirstRun);
            _firstMethod.IsBackground = true;
            _firstMethod.Start();
            ToolTip tp = new ToolTip();
            ToolTip mass_proxadd = new ToolTip();
            tp.SetToolTip(metroLabel2, "Он пытался сделать крутой и полезный софт, но у него как обычно не вышло...");
            mass_proxadd.SetToolTip(multipleAddUrlsInfoLabel, "При массовом добавлении ссылок для импорта прокси программа сама попытается определить,\nкакой тип прокси будет импортирован из ссылки. При этом могут возникать ошибки\nлучше добавлять ссылки по одной с ручным выбором типа прокси, которые будут импортированны!");
        }

        private void metroButton1_Click(object sender, EventArgs e) // Кнопка запуска и остановки чекера программы (Доделать)
        {
            if (_functions._threadsList.Count == 0)
                _functions.StartChecking();
            else
            {
                mainStartButton.Text = "Запустить"; // Изменяю название кнопки
                _functions.StopChecking();
            }
        }

        private void metroButton2_Click(object sender, EventArgs e) // Кнопка добавления единичной ссылки
        {
            Thread _addSingleUrl = new Thread(_functions.AddSingleUrl); // Выполняю добавление в новом потоке, так как повторяется процесс граббинга прокси
            _addSingleUrl.Start();
        }
        //открытие корневой папки программы
        private void metroLabel26_Click(object sender, EventArgs e) // Кнопка открытия корневой директории программы
        {
            Process.Start("explorer.exe", _functions._mainDirectory);
        }

        private void metroTextBox1_Leave(object sender, EventArgs e) // При переключении на другой элемент производится проверка
        {
            if (hostEditTextBox.Text == "" || !hostEditTextBox.Text.StartsWith("http"))
            {
                MetroFramework.MetroMessageBox.Show(this, "Укажите ссылку на хост (сайт) корректно!\nВ начале обязательно должно быть http:// или https:// (!) ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                hostEditTextBox.Text = "https://www.google.com";
            }
            else
                _functions._hostToCheck = hostEditTextBox.Text;
        }

        private void metroLink1_Click(object sender, EventArgs e) // Кнопка очистки лога
        {
            logRichBox.Clear();
        }

        private void metroLabel2_Click(object sender, EventArgs e) // Кнопка открытия информации о разработчике
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) // Событие при закрытии формы 
        {
            _functions.CloseOnChecking(e);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) // Прокрутка лога к последнему сообщению
        {
            logRichBox.ScrollToCaret();
        }

        private void metroLink2_Click(object sender, EventArgs e) // Кнопка выбора файла для последующего импорта прокси из него
        {
            _functions.ChoseFileToGrabProxy();
        }

        private void metroButton3_Click(object sender, EventArgs e) // Кнопка импорта прокси из выбранного файла
        {
            _functions.GrabProxyFromChosenFile();
        }

        private void metroLink3_Click(object sender, EventArgs e) // Кнопка сохранения всех прокси
        {
            _functions.SaveAllProxyWithoutCheck();
        }

        private void metroButton4_Click(object sender, EventArgs e) // Кнопка добавления нескольких ссылок для граббера
        {
            Thread _multipleUrlsAdd = new Thread(_functions.AddMultipleUrls); // Создаю новый поток, дабы избежать слишком высокой нагрузки на поток формы
            _multipleUrlsAdd.Start();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e) // Обработчик изменения текста
        {
            if (multipleUrlTextBox.Text.Length > 0) // Если текст есть и начинает с ссылок, то вывожу кол-во ссылок в лейбл
            {
                if (multipleUrlTextBox.Text.StartsWith("http"))
                {
                    string[] _writtenUrls = multipleUrlTextBox.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // Добавляю ссылки в массив
                    urlsCountLabel.Text = $"Кол-во ссылок: {_writtenUrls.Length}";
                }
            }
            else // Если нет, то вывожу стандартный текст
            {
                multipleUrlTextBox.Text = "Введите ссылки через 'Enter'...";
            }
        }

        private void metroCheckBox5_CheckStateChanged(object sender, EventArgs e) // Обработчик чекбокса отвечающего за авто-обновление прокси
        {
            if (autoUpdateProxyCheckBox.Checked)
            {

                _functions._mainRegistryDirectory.SetValue("autoUpdateProxy", 1);
                proxyUpdateTimer.Enabled = true;

            }
            else
            {

                _functions._mainRegistryDirectory.SetValue("autoUpdateProxy", 0);
                proxyUpdateTimer.Enabled = false;
            }

        }

        private void timer3_Tick(object sender, EventArgs e) // Таймер авто-обновления прокси
        {
            if (_functions._threadsList.Count == 0) // Если 0 потоков, значит чекер не запущен, а значит можно попытаться обновить прокси листы
            {
                logRichBox.AppendText("Происходит обновление листов прокси, пожалуйста подождите 🕒...\n");
                Thread _autoUpdateProxy = new Thread(_functions.GrabProxy);
                _autoUpdateProxy.IsBackground = true;
                _autoUpdateProxy.Start();
            }
            else
            {
                logRichBox.AppendText("Не удалось выполнить обновление прокси, так как в данный момент работает чекер!\n");
            }
        }

        private void metroCheckBox4_CheckStateChanged(object sender, EventArgs e) // При нажати на чекбокс с добавлением хеддеров
        {
            if (useHeaddersCheckBox.Checked) // Проверка на чекед
            {
                Form3 _headdersForm = new Form3(); // Создаю форму
                _headdersForm.Show(); // Показываю
                _headdersForm._mainForm = this; // Определяю переменную
            }
            else
                _functions._usableHeaders.Clear();
        }

        private void metroCheckBox6_CheckStateChanged(object sender, EventArgs e) // Изменение параметра keepAlive
        {
            if (parameterKeepAliveCheckBox.Checked) // Если галочка стоит, то использовать.
                _functions._useKeepAlive = true;
            else
                _functions._useKeepAlive = false;
        }

        private void metroComboBox2_SelectedIndexChanged(object sender, EventArgs e) // Изменение типа отправляемого запроса чекером
        {
            if (requestTypeComboBox.SelectedIndex == 0) // Если индекс равен 0, то тип GET
                _functions._requestMethod = "GET";
            else // Если не 0, то POST
                _functions._requestMethod = "POST";
        }


        private void metroCheckBox7_CheckStateChanged(object sender, EventArgs e) // Обработчик клика и изменения состояния чекбокса граббера
        {
            if (autoGrabCheckBox.Checked)
            {
                _functions._mainRegistryDirectory.SetValue("autoGrabProxy", 1);
            }
            else
            {
                _functions._mainRegistryDirectory.SetValue("autoGrabProxy", 0);
            }
        }

        private void metroButton5_Click(object sender, EventArgs e) // Кнопка граббера прокси при отключеном автоматическом граббере
        {
            Thread grabProxy = new Thread(_functions.GrabProxy);
            grabProxy.IsBackground = true;
            grabProxy.Start();
            grabProxyButton.Enabled = false;
        }

        private void metroCheckBox7_Click(object sender, EventArgs e) // Обработчик клика по чекбоксу автограббера
        {
            if (autoGrabCheckBox.Checked)
            {
                logRichBox.AppendText("Переменная автоматического граббинга прокси успешно изменена! После перезапуска программа самостоятельно будет граббить прокси по ссылкам.\n");
            }
            else
            {
                logRichBox.AppendText("Переменная автоматического граббинга прокси успешно изменена! После перезапуска программа НЕ будет самостоятельно граббить прокси по ссылкам.\n");
            }
        }

        private void checkHttpRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (checkHttpRadioButton.Checked)
                _functions._typeOfChecking = 1;
            _functions._factorOfThreads = 1;
        }

        private void checkSocks4RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (checkSocks4RadioButton.Checked)
                _functions._typeOfChecking = 2;
            _functions._factorOfThreads = 1;
        }

        private void checkSocks5RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (checkSocks5RadioButton.Checked)
                _functions._typeOfChecking = 3;
            _functions._factorOfThreads = 1;
        }

        private void checkBothSocksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBothSocksRadioButton.Checked)
                _functions._typeOfChecking = 4;
            _functions._factorOfThreads = 2;
        }

        private void checkAllRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (checkAllRadioButton.Checked)
                _functions._typeOfChecking = 5;
            _functions._factorOfThreads = 3;
        }

        private void timer4_Tick(object sender, EventArgs e) // Таймер обновления счётчиков
        {
            _functions.UpdateCountersLabels();
        }

        private void timeoutNumberUpDown_Leave(object sender, EventArgs e) // Обновление значения Timeout
        {
            _functions._timeoutNumber = (int)timeoutNumberUpDown.Value;
        }
    }
}

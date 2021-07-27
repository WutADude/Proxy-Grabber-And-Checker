using System;
using System.Windows.Forms;

namespace PGC
{
    public partial class Form3 : MetroFramework.Forms.MetroForm
    {

        public string standart_text = 
            "Введите сюда заголовки, которые будут отправляться с запросом." +
            "\n" +
            "\n" +
             "Символ \"  |  \" является разделителем между именем заголовка и значением." +
            "\n" +
            "\n" +
            "ЗАГОЛОВОК|ЗНАЧЕНИЕ ЗАГОЛОВКА" +
            "\n" +
            "\n" +
            "Referer | https://somesite.com/213/23/11 (БЕЗ ПРОБЕЛОВ)";

        ToolTip _infoToolTip = new ToolTip();

        public mainWorkForm _mainForm;

        Functions _functions = new Functions();

        public Form3()
        {
            InitializeComponent();
            _infoToolTip.SetToolTip(infoLabel, "Данная функция сделана для полной иммитации отправки запроса на сервер.\n" +
                "Пожалуйста не трогайте и не сохраняйте ничего, если не понимаете как это работает!\n" +
                "Если программа выдает слишком большое кол-во 'BAD-ов', значит здесь вы указали что-то не верно, либо что-то реально пошло не по плану.\n" +
                "Чтобы скопировать символ разделитель ( | ) нажмите 1 раз по данной кнопке.");
        }

        private void richTextBox1_Leave(object sender, EventArgs e)
        {
            if (headersAndValuesRichTextBox.Text.Length == 0)
                headersAndValuesRichTextBox.Text = standart_text;
        }

        private void richTextBox1_Click(object sender, EventArgs e)
        {
            if (headersAndValuesRichTextBox.Text.Replace(" ", "") == standart_text.Replace(" ", ""))
                headersAndValuesRichTextBox.Clear();
        }

        private void metroButton3_Click(object sender, EventArgs e) // Обработчик нажатия кнопки
        {
            if (headersAndValuesRichTextBox.Text.Replace(" ", "") != standart_text.Replace(" ", "") & headersAndValuesRichTextBox.Text.Length > 0) // Проверка текста кнопки
            {
                _functions._usableHeaders.Clear(); // Очищаю старые заголовки
                string[] _writtenHeaders = headersAndValuesRichTextBox.Text.Split(new char[] {'\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // Разделяю заголовки и добавляю в массив
                for (int i = 0; i < _writtenHeaders.Length; i++) // Создаю цикл по каждому заголовку
                {
                    _functions._usableHeaders.Add(_writtenHeaders[i]); //Добавляю каждый заголовок в массив заголовков в класс функций
                }
                _mainForm.logRichBox.AppendText($"{written_headers()}"); // Вывожу сообщение в логе
                this.Close(); // Закрываю форму
            }
            else
            {
                MetroFramework.MetroMessageBox.Show(this, "Пожалуйста, введите заголовки и значения заголовков корректно!", "Прошу обратить внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public string written_headers() // Сообщение в лог о добавленных заголовках
        {
            string _stringOfAddedHeadders = "Были успешно сохранены следующие заголовки:\n";
            for (int i = 0; i < _functions._usableHeaders.Count; i++)
            {
                string[] _gottedHeadder = _functions._usableHeaders[i].Split(new char[] { '|' });
                _stringOfAddedHeadders += $"[{_gottedHeadder[0]}]:[{_gottedHeadder[1]}]\n";
            }
            return _stringOfAddedHeadders;
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e) // Проверка при закрытии формы, если текст стандартный или слишком мал, то хеддеры не используются
        {
            if (headersAndValuesRichTextBox.Text == standart_text | headersAndValuesRichTextBox.Text.Length == 0)
            {
                _mainForm.useHeaddersCheckBox.Checked = false;
            }
        }

        private void metroLabel1_Click(object sender, EventArgs e) // При нажатии на кнопку информации в буффер обмена вставляется символ разделитель
        {
            Clipboard.SetText("|");
        }
    }
}

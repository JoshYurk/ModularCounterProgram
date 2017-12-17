using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ModularCounterProgram
{
    public partial class Form1 : Form
    {
        private List<NumericUpDown> _counterList = new List<NumericUpDown>();
        private List<Label> _labelList = new List<Label>();
        private List<Button> _disableCounterList = new List<Button>();
        private List<CheckBox> _enableCumulativeModeList = new List<CheckBox>();
        private List<bool> _cumulativeModeList = new List<bool>();
        private List<Button> _removeCounterList = new List<Button>();
        private readonly List<ToolStripMenuItem> _optionItems = new List<ToolStripMenuItem>();

        private readonly MenuStrip _mainMenu = new MenuStrip();
        private Font _labelFont = new Font("Times New Roman", 12);

        private int _savedWidth = 0;
        private int _savedHeight = 0;
        private bool _excessHidden = false;
        private int _counterTop;
        private int _numberOfCounters = 0;
        private string _path;

        public Form1()
        {
            InitializeComponent();

            AddNewOptionItem("options", "Options", false, "");
            AddNewOptionItem("loadCounters", "Load Counters", true, "options");
            AddNewOptionItem("saveCounters", "Save Counters", true, "options");
            AddNewOptionItem("addNewCounter", "Add New Counter", true, "options");
            AddNewOptionItem("showHideExcess", "Show/Hide Excess Area", true, "options");
            AddNewOptionItem("recreateCounters", "Recreate Counters", false, "");

            AddItemToMenuStrip("options");
            Controls.Add(_mainMenu);
        }

        private void AddItemToMenuStrip(string name)
        {
            var toolStripMenuItem = _optionItems.Single(x => x.Name == name);
            _mainMenu.Items.Add(toolStripMenuItem);
        }

        private void AddNewOptionItem(string name, string text, bool isDropdownItem, string nameOfParent)
        {
            var toolStripMenuItem = new ToolStripMenuItem
            {
                Text = text,
                Name = name
            };

            toolStripMenuItem.Click += OptionItemClickHandler;

            if (isDropdownItem)
            {
                var stripMenuItem = _optionItems.SingleOrDefault(x => x.Name == nameOfParent);
                if (stripMenuItem != null)
                {
                    stripMenuItem.DropDownItems.Add(toolStripMenuItem);
                }
            }

            _optionItems.Add(toolStripMenuItem);
        }

        private void OptionItemClickHandler(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (clickedItem.Name == "addNewCounter")
            {
                AddNewCounter();
            }
        }

        private void AddNewCounter()
        {
            var counterName = "Name";
            var dialogResultName = InputBox("New Counter", "Add Name for New Counter", ref counterName);
            if (dialogResultName != DialogResult.OK) return;

            var counterLabel = "Label Text";
            var dialogResultLabel = InputBox("New Counter", "Add Label Text For New Counter", ref counterLabel);
            if (dialogResultLabel != DialogResult.OK) return;

            var counterValue = "100";
            var dialogResultValue = InputBox("New Counter", "Add Maximum Value For New Counter", ref counterValue);
            if (dialogResultValue != DialogResult.OK) return;

            //ToDo add counter logic
            int.TryParse(counterValue, out var maxValue);

            var numericUpDown = new NumericUpDown { Name = counterName, Maximum = maxValue, Top = _counterTop, Width = 50, Left = 315 };
            _counterList.Add(numericUpDown);
        }

        private static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}

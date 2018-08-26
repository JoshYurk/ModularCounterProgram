using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ModularCounterProgram
{
    public partial class Form1 : Form
    {
        private readonly List<NumericUpDown> _counterList = new List<NumericUpDown>();
        private readonly List<Label> _labelList = new List<Label>();
        private readonly List<Button> _disableCounterList = new List<Button>();
        private readonly List<CheckBox> _enableCumulativeModeList = new List<CheckBox>();
        private readonly List<bool> _cumulativeModeList = new List<bool>();
        private readonly List<Button> _removeCounterList = new List<Button>();
        private readonly List<ToolStripMenuItem> _optionItems = new List<ToolStripMenuItem>();

        private readonly MenuStrip _mainMenu = new MenuStrip();
        private readonly Font _labelFont = new Font("Times New Roman", 12);
        private readonly ToolTip _cumulativeToolTip = new ToolTip();

        private readonly int _savedWidth;
        private readonly int _savedHeight;
        private bool _excessHidden;
        private int _counterTop;
        private int _numberOfCounters;
        private string _path;
        private string _screenString = "0";

        private double _currentVersion = 2.1;
        private double _oldVersion = 1.5;

        public Form1()
        {
            InitializeComponent();
            if (_numberOfCounters > 0)
            {
                _counterTop = (_numberOfCounters * 25) + (_mainMenu.Bottom + 5);

            }
            else
            {
                _counterTop = _mainMenu.Bottom + 5;
            }
            Width = 600;
            _savedWidth = Width;
            _savedHeight = Height;
            Text = @"Modular Counter Program";

            _cumulativeToolTip.AutoPopDelay = 0;

            if (Screen.AllScreens.Length > 1)
            {
                var screens = new List<int>();

                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screens.Add(i);
                }

                InputBox("Change Selected Screen", "Choose Screen Number (" + string.Join(",", screens) + ")" , ref _screenString);
            }

            AddMenuItems();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        //Add Item Methods

        private void AddMenuItems()
        {
            AddNewOptionItem("options", "Options", false, "");
            AddNewOptionItem("loadCounters", "Load Counters", true, "options");
            AddNewOptionItem("saveCounters", "Save Counters", true, "options");
            AddNewOptionItem("addNewCounter", "Add New Counter", true, "options");
            AddNewOptionItem("showHideExcess", "Show/Hide Excess Area", true, "options");
            AddNewOptionItem("recreateCounters", "Recreate Counters", false, "");

            if (Screen.AllScreens.Length > 1)
            {
                AddNewOptionItem("changeScreen", "Change Selected Screen", true, "options");
            }

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

            toolStripMenuItem.Click += MenuItemClickHandler;

            if (isDropdownItem)
            {
                var stripMenuItem = _optionItems.SingleOrDefault(x => x.Name == nameOfParent);
                stripMenuItem?.DropDownItems.Add(toolStripMenuItem);
            }

            _optionItems.Add(toolStripMenuItem);
        }

        private void AddNewCounter()
        {

            var counterLabelText = "Label Text";
            var dialogResultLabel = InputBox("New Counter", "Add Label Text For New Counter", ref counterLabelText);
            if (dialogResultLabel != DialogResult.OK) return;

            var counterValue = "100";
            var dialogResultValue = InputBox("New Counter", "Add Maximum Value For New Counter", ref counterValue);
            if (dialogResultValue != DialogResult.OK) return;

            var counterName = char.ToLowerInvariant(counterLabelText[0]) + counterLabelText.Substring(1);

            counterName = counterName.Replace(" ", "_");

            var tryParse = int.TryParse(counterValue, out var maxValue);

            while (!tryParse)
            {
                counterValue = "100";
                dialogResultValue = InputBox("New Counter", "Invalid Number, Please Add Maximum Value For New Counter", ref counterValue);
                if (dialogResultValue != DialogResult.OK) return;

                tryParse = int.TryParse(counterValue, out maxValue);
            }

            AddCounterSubObjects(counterName, maxValue, counterLabelText);

            AddActionEvents();

            _cumulativeToolTip.SetToolTip(_enableCumulativeModeList[_numberOfCounters], "Cumulative value of the counters above it.");

            AddCounterToForm();
        }

        private void AddCounterSubObjects(string counterName, int maxValue, string counterLabelText)
        {
            _counterList.Add(new NumericUpDown
            {
                Name = counterName,
                Maximum = maxValue,
                Top = _counterTop,
                Width = 50,
                Left = 315
            });
            _labelList.Add(new Label
            {
                Name = counterName + "Label",
                Text = counterLabelText,
                Top = _counterTop,
                Left = 365,
                Font = _labelFont,
                AutoSize = true
            });
            _disableCounterList.Add(new Button
            {
                Name = counterName + "DisableButton",
                Text = @"Disable Counter",
                Top = _counterTop,
                Left = 105,
                Width = 100,
                Height = 22,
                TabStop = false
            });
            _enableCumulativeModeList.Add(new CheckBox
            {
                Name = counterName + "CheckBox",
                Text = @"Cumulative Value",
                Top = _counterTop,
                Left = 210,
                Width = 120,
                Checked = false,
                TabStop = false,
                Visible = true
            });
            _removeCounterList.Add(new Button
            {
                Name = counterName + "RemovalButton",
                Text = @"Remove Counter",
                Top = _counterTop,
                Left = 0,
                Width = 100,
                Height = 22,
                TabStop = false
            });
            _cumulativeModeList.Add(false);
            _enableCumulativeModeList[_numberOfCounters].Checked = false;
        }

        private void AddCounterToForm()
        {
            Controls.Add(_counterList[_numberOfCounters]);
            Controls.Add(_labelList[_numberOfCounters]);
            Controls.Add(_disableCounterList[_numberOfCounters]);
            Controls.Add(_enableCumulativeModeList[_numberOfCounters]);
            Controls.Add(_removeCounterList[_numberOfCounters]);
            _counterList[_numberOfCounters].Focus();

            _counterTop += 25;
            _numberOfCounters++;
        }

        private void AddActionEvents()
        {
            _removeCounterList[_numberOfCounters].Click += RemovalButtonClick;
            _counterList[_numberOfCounters].ValueChanged += CounterValueChanged;
            _enableCumulativeModeList[_numberOfCounters].CheckedChanged += CumulativeModeChanged;
            _enableCumulativeModeList[_numberOfCounters].MouseHover += CheckBoxHover;
            _disableCounterList[_numberOfCounters].Click += DisableCounterButtonClick;
        }

        private void HideExcess()
        {
            var toolStripMenuItem = _optionItems.Single(x => x.Name == "options");
            var index = _optionItems.IndexOf(toolStripMenuItem);

            _excessHidden = true;
            FormBorderStyle = FormBorderStyle.None;
            Width = GetWidth();
            Height = (_numberOfCounters * 25) + (_mainMenu.Bottom + 5);
            for (var i = 0; i < _labelList.Count; i++)
            {
                _labelList[i].ForeColor = Color.White;
                _disableCounterList[i].ForeColor = Color.White;
                _removeCounterList[i].ForeColor = Color.White;
                _enableCumulativeModeList[i].ForeColor = Color.White;

                _removeCounterList[i].Left -= 105;
                _disableCounterList[i].Left -= 105;
                _counterList[i].Left -= 105;
                _enableCumulativeModeList[i].Left -= 105;
                _labelList[i].Left -= 105;
            }
            BackColor = Color.Black;
            _mainMenu.BackColor = Color.Black;
            _optionItems[index].ForeColor = Color.White;
            TopMost = true;
            PlaceUpperRight();
        }

        private void ShowExcess()
        {
            var toolStripMenuItem = _optionItems.Single(x => x.Name == "options");
            var index = _optionItems.IndexOf(toolStripMenuItem);

            _excessHidden = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            Width = _savedWidth;
            Height = _savedHeight;
            for (var i = 0; i < _labelList.Count; i++)
            {
                _labelList[i].ForeColor = Color.Black;
                _disableCounterList[i].ForeColor = Color.Black;
                _removeCounterList[i].ForeColor = Color.Black;
                _enableCumulativeModeList[i].ForeColor = Color.Black;

                _removeCounterList[i].Left += 105;
                _disableCounterList[i].Left += 105;
                _counterList[i].Left += 105;
                _enableCumulativeModeList[i].Left += 105;
                _labelList[i].Left += 105;
            }
            BackColor = Color.White;
            _mainMenu.BackColor = Color.White;
            _optionItems[index].ForeColor = Color.Black;
            TopMost = false;
            PlaceUpperRight();
        }

        private void RecreateCounters()
        {
            var optionsItem = _optionItems.Single(x => x.Name == "options");
            var optionsSubItem = _optionItems.Single(x => x.Name == "recreateCounters");

            if (_oldVersion < 2.0 && _oldVersion > 1.5)
            {
                for (var i = 0; i < _counterList.Count; i++)
                {
                    _counterList[i].Left += 105;
                    _labelList[i].Left += 105;
                    _disableCounterList[i].Left += 105;
                    _enableCumulativeModeList[i].Left += 105;
                    _removeCounterList.Add(new Button { Name = _counterList[i].Name + "RemovalButton", Text = "Remove Counter", Top = _counterList[i].Top, Left = 0, Width = 100, Height = 22, TabStop = false });
                    _removeCounterList[i].Click += RemovalButtonClick;

                    Controls.Add(_removeCounterList[i]);
                }
            }
            SaveCounters();
            for (var j = 0; j < optionsItem.DropDownItems.Count; j++)
            {
                optionsItem.DropDownItems[j].Enabled = true;
            }
            optionsItem.DropDownItems.Remove(optionsSubItem);
            _mainMenu.Items.Clear();
            _mainMenu.Items.Add(optionsItem);
            Refresh();
        }

        //Action Events

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;

            if (clickedItem.Name == "addNewCounter")
            {
                AddNewCounter();
            }
            if (clickedItem.Name == "loadCounters")
            {
                LoadCounters();
            }
            if (clickedItem.Name == "saveCounters")
            {
                SaveCounters();
            }
            if (clickedItem.Name == "showHideExcess")
            {
                if (_excessHidden && _counterList.Count > 0)
                {
                    ShowExcess();
                }
                else if (!_excessHidden && _counterList.Count > 0)
                {
                    HideExcess();
                }
            }
            if (clickedItem.Name == "recreateCounters")
            {
                RecreateCounters();
            }
            if (clickedItem.Name == "changeScreen")
            {
                var screens = new List<int>();

                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screens.Add(i);
                }

                InputBox("Change Selected Screen", "Choose Screen Number (" + string.Join(",", screens) + ")", ref _screenString);
                PlaceUpperRight();
            }
        }

        private void CounterValueChanged(object sender, EventArgs e)
        {
            var start = -1;

            foreach (var unused in _cumulativeModeList)
            {
                var index = _cumulativeModeList.IndexOf(true, start + 1);
                var end = index;

                if (index == -1) continue;
                decimal value = 0;

                for (var k = start + 1; k < end; k++)
                {
                    value += _counterList[k].Value;
                }

                start = end;
                _counterList[index].Value = value;
            }
        }

        private void CheckBoxHover(object sender, EventArgs e)
        {
            var itemHovered = (CheckBox)sender;
            _cumulativeToolTip.GetToolTip(itemHovered);
            _cumulativeToolTip.Active = true;
        }

        private void DisableCounterButtonClick(object sender, EventArgs e)
        {
            var clickedItem = (Button)sender;
            var index = _disableCounterList.IndexOf(clickedItem);

            if (_counterList[index].Enabled)
            {
                _counterList[index].Enabled = false;
                clickedItem.Text = @"Enable Counter";
            }
            else
            {
                _counterList[index].Enabled = true;
                clickedItem.Text = @"Disable Counter";
            }
        }

        private void CumulativeModeChanged(object sender, EventArgs e)
        {
            var clickedItem = (CheckBox)sender;
            var index = _enableCumulativeModeList.IndexOf(clickedItem);

            _cumulativeModeList[index] = !_cumulativeModeList[index];
            _disableCounterList[index].Visible = !_disableCounterList[index].Visible;
            _counterList[index].Enabled = !_counterList[index].Enabled;

            if (clickedItem.Checked)
            {
                var start = -1;
                foreach (var unused in _cumulativeModeList)
                {
                    var cIndex = _cumulativeModeList.IndexOf(true, start + 1);
                    if (cIndex == -1) continue;
                    for (var i = start + 1; i < cIndex; i++)
                    {
                        _enableCumulativeModeList[i].Enabled = false;
                        _enableCumulativeModeList[i].Visible = false;
                        start = cIndex;
                    }
                }
            }
            else
            {
                _disableCounterList[index].Text = @"Disable Counter";
                for (var i = 0; i < index; i++)
                {
                    _enableCumulativeModeList[i].Enabled = true;
                    _enableCumulativeModeList[i].Visible = true;
                }
            }
        }

        private void RemovalButtonClick(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            var index = _removeCounterList.IndexOf(clickedButton);

            if (_excessHidden)
            {
                MessageBox.Show(@"Unable to remove counter when excess is hidden", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (_cumulativeModeList[index])
            {
                MessageBox.Show(@"Unable to remove counter when cumulative mode is enabled", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Controls.Remove(_enableCumulativeModeList[index]);
                Controls.Remove(_counterList[index]);
                Controls.Remove(_labelList[index]);
                Controls.Remove(_disableCounterList[index]);
                Controls.Remove(_removeCounterList[index]);

                _enableCumulativeModeList.RemoveAt(index);
                _counterList.RemoveAt(index);
                _disableCounterList.RemoveAt(index);
                _cumulativeModeList.RemoveAt(index);
                _labelList.RemoveAt(index);
                _removeCounterList.RemoveAt(index);

                if (_counterList.Count > index)
                {
                    for (var i = index; i < _counterList.Count; i++)
                    {
                        _counterList[i].Top -= 25;
                        _disableCounterList[i].Top -= 25;
                        _enableCumulativeModeList[i].Top -= 25;
                        _labelList[i].Top -= 25;
                        _removeCounterList[i].Top -= 25;
                    }
                }

                _counterTop -= 25;
                _numberOfCounters--;
                Refresh();
            }
        }

        private void SaveCounters()
        {
            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = @"Text Files (*.txt)|*.txt",
                Title = @"Save Counters",
                FilterIndex = 2
            };

            var result = saveDialog.ShowDialog();
            if (result != DialogResult.OK) return;

            _path = saveDialog.FileName;
            var fileWriter = new StreamWriter(_path);
            fileWriter.WriteLine(_currentVersion);
            for (var i = 0; i < _counterList.Count; i++)
            {
                fileWriter.WriteLine(_counterList[i].Name);
                fileWriter.WriteLine(_counterList[i].Value);
                fileWriter.WriteLine(_counterList[i].Maximum);
                fileWriter.WriteLine(_counterList[i].Top);
                fileWriter.WriteLine(_counterList[i].Left);
                fileWriter.WriteLine(_counterList[i].Width);
                fileWriter.WriteLine(_counterList[i].Enabled);

                fileWriter.WriteLine(_labelList[i].Name);
                fileWriter.WriteLine(_labelList[i].Text);
                fileWriter.WriteLine(_labelList[i].Width);
                if (_excessHidden)
                {
                    fileWriter.WriteLine(_labelList[i].Left + 105);
                }
                else
                {
                    fileWriter.WriteLine(_labelList[i].Left);
                }
                fileWriter.WriteLine(_labelList[i].Top);

                fileWriter.WriteLine(_disableCounterList[i].Name);
                fileWriter.WriteLine(_disableCounterList[i].Visible);
                fileWriter.WriteLine(_disableCounterList[i].Left);

                fileWriter.WriteLine(_removeCounterList[i].Name);
                fileWriter.WriteLine(_removeCounterList[i].Left);

                fileWriter.WriteLine(_cumulativeModeList[i]);

                fileWriter.WriteLine(_enableCumulativeModeList[i].Name);
                fileWriter.WriteLine(_enableCumulativeModeList[i].Enabled);
                fileWriter.WriteLine(_enableCumulativeModeList[i].Checked);
                fileWriter.WriteLine(_enableCumulativeModeList[i].CheckState);
                fileWriter.WriteLine(_enableCumulativeModeList[i].Left);
            }
            MessageBox.Show(@"Counters Successfully Saved", @"Success");
            fileWriter.Close();
        }

        private void LoadCounters()
        {
            var open = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = @"Text Files (*.txt)|*.txt",
                Title = @"Load Counters",
                FilterIndex = 2
            };
            var openingResult = open.ShowDialog();

            if (openingResult != DialogResult.OK) return;

            _path = open.FileName;

            if (!File.Exists(_path)) return;

            _counterList.Clear();
            _labelList.Clear();
            _enableCumulativeModeList.Clear();
            _cumulativeModeList.Clear();
            _disableCounterList.Clear();
            _numberOfCounters = 0;
            Controls.Clear();
            Controls.Add(_mainMenu);

            var fileReader = new StreamReader(_path);

            var savedVersion = fileReader.ReadLine();
            _oldVersion = double.Parse(savedVersion ?? "0.0");

            while (!fileReader.EndOfStream)
            {
                var buttonText = "";
                var counterSTop = "";

                if (_oldVersion >= 1.0)
                {
                    var counterName = fileReader.ReadLine();
                    var counterValue = fileReader.ReadLine();
                    var counterMax = fileReader.ReadLine();
                    counterSTop = fileReader.ReadLine();
                    var counterLeft = "315";
                    if (_oldVersion >= 2.1)
                    {
                        counterLeft = fileReader.ReadLine();
                    }
                    var counterWidth = fileReader.ReadLine();
                    var counterEnable = fileReader.ReadLine();

                    var labelName = fileReader.ReadLine();
                    var labelText = fileReader.ReadLine();
                    var labelWidth = fileReader.ReadLine();
                    var labelLeft = fileReader.ReadLine();
                    var labelTop = fileReader.ReadLine();

                    _counterList.Add(new NumericUpDown { Name = counterName, Maximum = int.Parse(counterMax), Top = int.Parse(counterSTop), Width = int.Parse(counterWidth), Enabled = bool.Parse(counterEnable), Left = int.Parse(counterLeft) });
                    _counterList[_numberOfCounters].Value = int.Parse(counterValue);
                    _labelList.Add(new Label { Name = labelName, Text = labelText, Width = int.Parse(labelWidth), Left = int.Parse(labelLeft), Top = int.Parse(labelTop), Font = _labelFont, ForeColor = Color.Black });

                    if (bool.Parse(counterEnable))
                    {
                        buttonText = "Disable Counter";
                    }
                    else if (bool.Parse(counterEnable) == false)
                    {
                        buttonText = "Enable Counter";
                    }

                    _counterList[_numberOfCounters].ValueChanged += CounterValueChanged;

                    Controls.Add(_counterList[_numberOfCounters]);
                    Controls.Add(_labelList[_numberOfCounters]);
                }

                if (_oldVersion >= 1.5)
                {
                    var buttonName = fileReader.ReadLine();
                    var buttonVisibility = fileReader.ReadLine();
                    var buttonLeft = "105";
                    if (_oldVersion >= 2.1)
                    {
                        buttonLeft = fileReader.ReadLine();
                    }

                    _disableCounterList.Add(new Button { Name = buttonName, Text = buttonText, Top = int.Parse(counterSTop), Left = int.Parse(buttonLeft), Width = 100, Height = 22, TabStop = false, Visible = bool.Parse(buttonVisibility) });
                    _disableCounterList[_numberOfCounters].Click += DisableCounterButtonClick;

                    Controls.Add(_disableCounterList[_numberOfCounters]);
                }

                if (_oldVersion >= 2.0)
                {
                    var removalButtonName = fileReader.ReadLine();
                    var removalButtonLeft = "0";
                    if (_oldVersion >= 2.1)
                    {
                        removalButtonLeft = fileReader.ReadLine();
                    }
                    _removeCounterList.Add(new Button { Name = removalButtonName, Text = @"Remove Counter", Top = int.Parse(counterSTop), Left = int.Parse(removalButtonLeft), Width = 100, Height = 22, TabStop = false });
                    _removeCounterList[_numberOfCounters].Click += RemovalButtonClick;

                    Controls.Add(_removeCounterList[_numberOfCounters]);
                }

                if (_oldVersion >= 1.5)
                {
                    var cumulativeValue = fileReader.ReadLine();

                    var checkBoxName = fileReader.ReadLine();
                    var checkBoxEnabled = fileReader.ReadLine();
                    var checkBoxChecked = fileReader.ReadLine();
                    var checkBoxCheckStatus = fileReader.ReadLine();
                    var checkBoxLeft = "210";
                    if (_oldVersion >= 2.1)
                    {
                        checkBoxLeft = fileReader.ReadLine();
                    }
                    var checkState = CheckState.Indeterminate;
                    if (checkBoxCheckStatus == "Checked")
                    {
                        checkState = CheckState.Checked;
                    }
                    else if (checkBoxCheckStatus == "Unchecked")
                    {
                        checkState = CheckState.Unchecked;
                    }
                    _enableCumulativeModeList.Add(new CheckBox { Name = checkBoxName, Text = @"Cumulative Value", Top = int.Parse(counterSTop), Left = int.Parse(checkBoxLeft), Width = 120, Checked = bool.Parse(checkBoxChecked), TabStop = false, CheckState = checkState, Enabled = bool.Parse(checkBoxEnabled) });

                    _cumulativeModeList.Add(bool.Parse(cumulativeValue));

                    _enableCumulativeModeList[_numberOfCounters].CheckedChanged += CumulativeModeChanged;
                    _cumulativeToolTip.SetToolTip(_enableCumulativeModeList[_numberOfCounters], "Cumulative value of the counters above it.");

                    Controls.Add(_enableCumulativeModeList[_numberOfCounters]);
                }

                _counterTop = int.Parse(counterSTop) + 25;
                _numberOfCounters++;
            }
            _counterList[0].Focus();
            if (_oldVersion >= 1.5)
            {
                foreach (var unused in _cumulativeModeList)
                {
                    if (_cumulativeModeList.IndexOf(true) == -1) continue;
                    for (var i = 0; i < _cumulativeModeList.IndexOf(true); i++)
                    {
                        _enableCumulativeModeList[i].Enabled = false;
                        _enableCumulativeModeList[i].Visible = false;
                    }
                }
            }
            if (_oldVersion < _currentVersion)
            {
                var optionsItem = _optionItems.Single(x => x.Name == "options");
                var optionsSubItem = _optionItems.Single(x => x.Name == "recreateCounters");

                for (var i = 0; i < optionsItem.DropDownItems.Count; i++)
                {
                    optionsItem.DropDownItems[i].Enabled = false;
                }
                optionsItem.DropDownItems.Add(optionsSubItem);
                _mainMenu.Items.Clear();
                _mainMenu.Items.Add(optionsItem);
                MessageBox.Show(@"Your save is from an older version, please recreate your counters. \n\nThere are new additions that will not get loaded until you get a more recent save", @"Outdated Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            fileReader.Close();
        }

        //Misc

        private int GetWidth()
        {
            const int calculatedWidth = 210;
            var width = 0;

            if (_counterList.Count > 0)
            {
                width = _counterList.Select((t, i) => t.Width + _labelList[i].Width + calculatedWidth).Concat(new[] { width }).Max();
            }

            return width;
        }

        private void PlaceUpperRight()
        {
            var screen = Screen.AllScreens[int.Parse(_screenString)];

            Left = screen.WorkingArea.Right - Width;
            Top = screen.WorkingArea.Top;
        }

        private static DialogResult InputBox(string title, string promptText, ref string value)
        {
            var form = new Form();
            var label = new Label();
            var textBox = new TextBox();
            var buttonOk = new Button();
            var buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = @"OK";
            buttonCancel.Text = @"Cancel";
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

            var dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}
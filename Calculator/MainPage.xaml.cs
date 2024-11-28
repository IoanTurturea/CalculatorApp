
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Calculator;


// TODO: add "," between hundreds as separation

public partial class MainPage : ContentPage
{
    double RetainedInput = 0;
    string Operation = string.Empty;
    const string ADD = "+";
    const string SUB = "-";
    const string MUL = "*";
    const string DIV = "/";
    const string DivisionByZeroText = "Error: division by 0.";
    const string CHART_VIEW = "Chart";
    const string TEXT_VIEW = "Text";
    const string SCIENTIFIC_VIEW = "Scientific";
    readonly ObservableCollection<double> LVCValues = new();
    const int WindowLength = 100;

    public ISeries[] Series { get; set; }

    public MainPage()
    {
        InitializeComponent();

        Series = [
            new LineSeries<double>
            {
                Values = LVCValues,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Blue, 1)
            }
        ];

        pickerView.SelectedIndex = 0;

        entryResult.Text = "0";

        pickerView.ItemsSource = new List<string>() { TEXT_VIEW, CHART_VIEW, SCIENTIFIC_VIEW };

        BindingContext = this;
    }

    private void btnBack_Clicked(object sender, EventArgs e)
    {
        if (entryResult.Text == DivisionByZeroText)
        {
            EnableAllButtons(true);
        }

        int length = entryResult.Text.Length;
        string text;

        if (length > 1)
        {
            text = entryResult.Text.Remove(length - 1, 1);
        }
        else
        {
            text = "0";
        }

        entryResult.Text = text;
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        if (entryResult.Text == DivisionByZeroText)
        {
            EnableAllButtons(true);
        }
        
        entryCalculation.Text = string.Empty;
        entryResult.Text = "0";
        RetainedInput = 0;
        Operation = string.Empty;
        LVCValues.Clear();
    }

    private void btn1PerX_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"1/{entryResult.Text}";

        if (double.TryParse(entryResult.Text, out double temp))
        {
            if (temp != 0)
            {
                entryResult.Text = $"{1 / temp}";
            }
            else
            {
                DivisionByZero();
            }
        }
    }

    private void btn7_Clicked(object sender, EventArgs e) => AddDigit("7");

    private void btn8_Clicked(object sender, EventArgs e) => AddDigit("8");

    private void btn9_Clicked(object sender, EventArgs e) => AddDigit("9");

    private void btn4_Clicked(object sender, EventArgs e) => AddDigit("4");

    private void btn5_Clicked(object sender, EventArgs e) => AddDigit("5");

    private void btn6_Clicked(object sender, EventArgs e) => AddDigit("6");

    private void btn1_Clicked(object sender, EventArgs e) => AddDigit("1");

    private void btn2_Clicked(object sender, EventArgs e) => AddDigit("2");

    private void btn3_Clicked(object sender, EventArgs e) => AddDigit("3");

    private void btn0_Clicked(object sender, EventArgs e) => AddDigit("0");

    private void btnPlus_Clicked(object sender, EventArgs e) => BuildOperation(ADD);

    private void btnDivision_Clicked(object sender, EventArgs e) => BuildOperation(DIV);

    private void btnMult_Clicked(object sender, EventArgs e) => BuildOperation(MUL);

    private void btnMinus_Clicked(object sender, EventArgs e) => BuildOperation(SUB);

    private void btnDot_Clicked(object sender, EventArgs e)
    {
        if (!entryResult.Text.Contains('.'))
        {
            entryResult.Text += '.';
        }
    }

    private void btnEqual_Clicked(object sender, EventArgs e)
    {
        if (double.TryParse(entryResult.Text, out double parsedText))
        {
            double result = 0;

            switch (Operation)
            {
                case ADD:
                    result = RetainedInput + parsedText;
                    entryResult.Text = $"{result}";

                    if (!entryCalculation.Text.Contains('='))
                    {
                        entryCalculation.Text += $" {parsedText} =";
                    }
                    break;

                case SUB:
                    result = RetainedInput - parsedText;
                    entryResult.Text = $"{result}";
                    entryCalculation.Text += $" {parsedText} =";
                    break;

                case MUL:
                    result = RetainedInput * parsedText;
                    entryResult.Text = $"{result}";
                    entryCalculation.Text += $" {parsedText} =";
                    break;

                case DIV:
                    if (parsedText != 0)
                    {
                        result = RetainedInput / parsedText;
                        entryResult.Text = $"{result}";
                        entryCalculation.Text += $" {parsedText} =";
                    }
                    else
                    {
                        DivisionByZero();
                    }
                    break;

                case "":
                    break;
            }

            RetainedInput = 0;
            Operation = string.Empty;

            LVCValues.Add(result);

            if(LVCValues.Count > WindowLength)
            {
                LVCValues.RemoveAt(0);
            }
        }

        // the else case happens if user changes operator (+, -, *, /) and presses equal
        // which does nothing
    }

    private void BuildOperation(string in_operator)
    {
        if (double.TryParse(entryResult.Text, out double temp))
        {
            // this means equal was not pressed and another input is added
            if (Operation != string.Empty)
            {
                switch (Operation)
                {
                    case ADD: entryResult.Text = $"{RetainedInput += temp}"; break;
                    case SUB: entryResult.Text = $"{RetainedInput -= temp}"; break;
                    case MUL: entryResult.Text = $"{RetainedInput *= temp}"; break;
                    case DIV: entryResult.Text = $"{RetainedInput /= temp}"; break;
                }
            }
            else
            {
                RetainedInput = temp;
            }

            entryCalculation.Text = $"{entryResult.Text} {in_operator}";
            entryResult.Text = string.Empty;
        }
        else
        {
            // means user changed mind about operator
            if (entryCalculation.Text.Contains(" "))
            {
                int length = entryCalculation.Text.Length;
                entryCalculation.Text = entryCalculation.Text.Remove(length - 1) + in_operator;
            }
        }

        Operation = in_operator;
    }

    private void AddDigit(string digit)
    {
        if (entryResult.Text == "0")
        {
            entryResult.Text = digit;
        }            
        else if(entryResult.Text == DivisionByZeroText)
        {
            btnClear_Clicked(null, null);
            // must be before the clear click event 
            entryResult.Text = digit;
        }
        else
        {
            entryResult.Text += digit;
        }
    }

    private void DivisionByZero()
    {
        entryResult.Text = DivisionByZeroText;

        EnableAllButtons(false);
    }

    private void EnableAllButtons(bool in_enable)
    {
        var buttons = new List<Button>
        {
            btn1PerX, btnDivision, btnDot, btnEqual, btnPlus, btnMinus, btnMult, btnDivision
        };

        foreach(var button in buttons)
        {
            button.IsEnabled = in_enable;

            if(in_enable)
            {
                button.Opacity = 1;
            }
            else
            {
                button.Opacity = 0.5;
            }
        }
    }

    private void pickerView_SelectedIndexChanged(object sender, EventArgs e)
    {
        var item = pickerView.ItemsSource[pickerView.SelectedIndex];

        switch(item)
        {
            case CHART_VIEW:
                entryResult.IsVisible = false;
                gridChart.IsVisible = true;

                rowDisplay.Height = new GridLength(1.5, GridUnitType.Star);
                rowInput.Height = new GridLength(1, GridUnitType.Star);
                break;

            case TEXT_VIEW:
                entryResult.IsVisible = true;
                gridChart.IsVisible = false;

                rowDisplay.Height = new GridLength(1, GridUnitType.Star);
                rowInput.Height = new GridLength(1.5, GridUnitType.Star);
                break;

            case SCIENTIFIC_VIEW:
                break;
        }
    }

    private void entryResult_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;

        if(entry?.Text == "0")
        {
            btn1PerX.IsEnabled = false;
            btnBack.IsEnabled = false;
            btn1PerX.Opacity = 0.5;
            btnBack.Opacity = 0.5;
        }
        else
        {
            btn1PerX.IsEnabled = true;
            btnBack.IsEnabled = true;
            btn1PerX.Opacity = 1;
            btnBack.Opacity = 1;
        }
    }
}


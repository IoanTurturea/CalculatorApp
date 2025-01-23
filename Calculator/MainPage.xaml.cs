
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Globalization;
namespace Calculator;


// TODO: other feature - better implement them after first release:
// another chart view
// both chart and scientific
// programmer mode would take too much time to build, better keep it simple
// replace combobox with TabView
// TODO: optional rename input arguments with "in" before
public partial class MainPage : ContentPage
{
    double RetainFirstOperand = 0;
    double RetainSecondOperand = 0;
    string Operation = string.Empty;
    const string ADD = "+";
    const string SUB = "-";
    const string MUL = "*";
    const string DIV = "/";
    const string POW = "^";
    const string MOD = "modulo";
    const string DivisionByZeroText = "Error: division by 0.";
    const string CHART_VIEW = "Chart";
    const string SIMPLE_VIEW = "Simple";
    const string SCIENTIFIC_VIEW = "Scientific";
    ObservableCollection<double> LVCValues = new() { 0 };
    const int WindowLength = 100;
    const double EntryCalculationChartFont = 20;
    const double EntryResultChartFont = 30;
    const double EntryCalculationNormalFont = 20;
    const double EntryResultNormalFont = 50;

    /// <summary>
    /// Binding property for the chart
    /// </summary>
    public ISeries[] Series { get; set; }

    private readonly BindableProperty FontSizeFunction = BindableProperty.Create(nameof(StandardButtonFontSize), typeof(double), typeof(MainPage), (double)20);
    public double StandardButtonFontSize
    {
        get => (double)GetValue(FontSizeFunction);
        set => SetValue(FontSizeFunction, value);
    }

    bool ClearEntryText = false;

    const double ChartViewRowInput = 1;
    const double ChartViewRowInputWhenLandscape = ChartViewRowInput / 2;
    const double ChartViewRowDisplayText = 0.5;
    const double ChartViewRowDisplayChart = 1;
    const double ChartViewRowDisplayTextWhenLandScape = 0;
    const double ChartViewRowDisplayChartWhenLandScape = ChartViewRowDisplayChart / 2;

    const double OtherViewsRowInput = 1.5;
    const double OtherViewsRowDisplayText = 1;
    const double OtherViewsRowDisplayTextWhenLandScape = OtherViewsRowDisplayText / 2;
    const double OtherViewsRowDisplayChart = 0;



    public MainPage()
    {
        InitializeComponent();

        InitializeChart();

        // must be set here not in UI
        entryResult.Text = "0";

        pickerView.ItemsSource = new List<string>() { SIMPLE_VIEW, SCIENTIFIC_VIEW, CHART_VIEW };
        pickerView.SelectedIndex = 0;

        BindingContext = this;
    }

    private void InitializeChart()
    {
        Series = [
            new LineSeries<double>
            {
                Values = LVCValues,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Blue, 1)
            }
        ];
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

        entryCalculation.Text = "0";
        entryResult.Text = "0";
        RetainFirstOperand = 0;
        RetainSecondOperand = 0;
        Operation = string.Empty;
        ClearEntryText = false;

        InitializeChart();
        LVCValues.Clear();
        LVCValues.Add(0);
    }

    private void btn1PerX_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"1/{entryResult.Text}";

        if (double.TryParse(entryResult.Text, out double temp))
        {
            if (temp != 0)
            {
                entryResult.Text = $"{ToCustomString(1 / temp)}";
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

    private void btnPlus_Clicked(object sender, EventArgs e)     => BuildOperation(ADD);

    private void btnDivision_Clicked(object sender, EventArgs e) => BuildOperation(DIV);

    private void btnMult_Clicked(object sender, EventArgs e)     => BuildOperation(MUL);

    private void btnMinus_Clicked(object sender, EventArgs e)    => BuildOperation(SUB);

    private void btnDot_Clicked(object sender, EventArgs e)
    {
        if (!entryResult.Text.Contains('.'))
        {
            entryResult.Text += '.';
        }
    }

    private void btnEqual_Clicked(object sender, EventArgs e)
    {
        if (double.TryParse(entryResult.Text, out double result))
        {
            if (string.Empty == Operation)
            {
                // means only simple equal press on a number, nothing to compute
                RetainSecondOperand = result;

                entryCalculation.Text = $" {result} =";
            }
            else
            {
                if(entryCalculation.Text.Contains("="))
                {
                    RetainFirstOperand = result;
                }
                else
                {
                    RetainSecondOperand = result;
                }

                entryCalculation.Text = $"{RetainFirstOperand} {Operation} {RetainSecondOperand} =";
            }

            DuplicateCodeAboutOperations();
        }

        // the else case happens if user changes operator (+, -, *, /, ^) and presses equal
        // which does nothing

        ClearEntryText = true;
    }

    private void BuildOperation(string in_operator)
    {
        if (double.TryParse(entryResult.Text, out double result))
        {
            // this means equal button was not pressed
            // and another input is added. Must compute the old and the coming function
            if (Operation != string.Empty)
            {
                if(!entryCalculation.Text.Contains("="))
                {
                    RetainSecondOperand = result;
                }

                result = DuplicateCodeAboutOperations();
            }

            RetainFirstOperand = result;

            entryCalculation.Text = $"{result} {in_operator}";
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
        ClearEntryText = true;
    }

    private double DuplicateCodeAboutOperations()
    {
        double result = 0;

        switch (Operation)
        {
            case ADD:
                result = RetainFirstOperand + RetainSecondOperand;
                break;

            case SUB:
                result = RetainFirstOperand - RetainSecondOperand;
                break;

            case MUL:
                result = RetainFirstOperand * RetainSecondOperand;
                break;

            case DIV:
                if (RetainSecondOperand != 0)
                {
                    result = RetainFirstOperand / RetainSecondOperand;
                }
                else
                {
                    DivisionByZero();
                }
                break;

            case POW:

                result = Math.Pow(RetainFirstOperand, RetainSecondOperand);
                break;

            case MOD:

                result = RetainFirstOperand % RetainSecondOperand;
                break;

            case "":
                result = RetainSecondOperand;
                break;
        }

        if (Operation != DIV || RetainSecondOperand != 0)
        {
            entryResult.Text = ToCustomString(result);
        }

        if (string.Empty == Operation)
        {
            LVCValues.Add(RetainSecondOperand);
        }
        else
        {
            LVCValues.Add(result);
        }

        if (LVCValues.Count > WindowLength)
        {
            LVCValues.RemoveAt(0);
        }

        return result;
    }

    private void DivisionByZero()
    {
        entryResult.Text = DivisionByZeroText;

        EnableAllButtons(false);
    }

    private void EnableAllButtons(bool in_enable)
    {
        var buttons = new List<VisualElement>
        {
            btn1PerX, btnDivision, btnDot, btnEqual, btnPlus, btnMinus, btnMult, btnDivision
        };

        foreach (var button in buttons)
        {
            button.IsEnabled = in_enable;

            if (in_enable) button.Opacity = 1;
            else           button.Opacity = 0.5;
        }
    }

    private void pickerView_SelectedIndexChanged(object sender, EventArgs e)
    {
        var item = pickerView.ItemsSource[pickerView.SelectedIndex];

        switch (item)
        {
            case CHART_VIEW:

                // setup chart view
                switch (DeviceDisplay.Current.MainDisplayInfo.Orientation)
                {
                    case DisplayOrientation.Landscape:
                        rowDisplayText.Height = new GridLength(ChartViewRowDisplayTextWhenLandScape, GridUnitType.Star);
                        rowDisplayChart.Height = new GridLength(ChartViewRowDisplayChartWhenLandScape, GridUnitType.Star);
                        rowInput.Height = new GridLength(ChartViewRowInputWhenLandscape, GridUnitType.Star);
                        break;

                    case DisplayOrientation.Portrait:
                        rowDisplayText.Height = new GridLength(ChartViewRowDisplayText, GridUnitType.Star);
                        rowDisplayChart.Height = new GridLength(ChartViewRowDisplayChart, GridUnitType.Star);
                        ArrangeEntrys(EntryCalculationChartFont, EntryResultChartFont, LayoutOptions.End);
                        rowInput.Height = new GridLength(ChartViewRowInput, GridUnitType.Star);
                        break;
                }

                gridChart.IsVisible = true;

                // reset scientific view
                rowScientific0.Height = new GridLength(0);
                rowScientific1.Height = new GridLength(0);
                columnScientificLeft.Width = new GridLength(0);

                break;

            case SIMPLE_VIEW:

                // reset chart view
                rowInput.Height = new GridLength(OtherViewsRowInput, GridUnitType.Star);
                rowDisplayText.Height = new GridLength(OtherViewsRowDisplayText, GridUnitType.Star);
                rowDisplayChart.Height = new GridLength(OtherViewsRowDisplayChart, GridUnitType.Star);
                gridChart.IsVisible = false;

                // reset scientific view
                rowScientific0.Height = new GridLength(0);
                rowScientific1.Height = new GridLength(0);
                columnScientificLeft.Width = new GridLength(0);

                ArrangeEntrys(EntryCalculationNormalFont, EntryResultNormalFont, LayoutOptions.Fill);
                break;

            case SCIENTIFIC_VIEW:

                // reset chart view
                switch (DeviceDisplay.Current.MainDisplayInfo.Orientation)
                {
                    case DisplayOrientation.Landscape:
                        rowDisplayText.Height = new GridLength(OtherViewsRowDisplayTextWhenLandScape, GridUnitType.Star);
                        ArrangeEntrys(EntryCalculationNormalFont / 2, EntryResultNormalFont / 2, LayoutOptions.Fill);
                        break;

                    case DisplayOrientation.Portrait:
                        rowDisplayText.Height = new GridLength(OtherViewsRowDisplayText, GridUnitType.Star);
                        ArrangeEntrys(EntryCalculationNormalFont, EntryResultNormalFont, LayoutOptions.Fill);
                        break;
                }
                // setup scientific view
                rowScientific0.Height = new GridLength(1, GridUnitType.Star);
                rowScientific1.Height = new GridLength(1, GridUnitType.Star);
                columnScientificLeft.Width = new GridLength(1, GridUnitType.Star);

                rowInput.Height = new GridLength(OtherViewsRowInput, GridUnitType.Star);
                rowDisplayChart.Height = new GridLength(OtherViewsRowDisplayChart, GridUnitType.Star);
                gridChart.IsVisible = false;

                break;
        }
    }

    private void ArrangeEntrys(double fontSizeCalculation, 
                               double fontSizeResult,
                               LayoutOptions layoutOptions)
    {
        // modify the entries
        entryCalculation.FontSize = fontSizeCalculation;
        entryCalculation.HorizontalOptions = layoutOptions;

        entryResult.FontSize = fontSizeResult;
        entryResult.HorizontalOptions = layoutOptions;
    }

    private void AddDigit(string in_digit)
    {
        const int thousandDigits = 3;

        if (entryResult.Text == "0")
        {
            entryResult.Text = in_digit;
        }
        else if (entryResult.Text == DivisionByZeroText)
        {
            btnClear_Clicked(null, null);
            // must be after the clear click event 
            entryResult.Text = string.Empty;
        }
        else
        {
            if (ClearEntryText)
            {
                entryResult.Text = string.Empty;
                ClearEntryText = false;
            }

            string concat = entryResult.Text + in_digit;
            int index = concat.IndexOf('.');
            string newConcat = string.Empty;
            if (index != -1)
            {
                newConcat = concat.Remove(index);
            }

            if (newConcat.Length > thousandDigits)
            {
                entryResult.Text = concat;
            }
            else if (!concat.Contains('.') && (concat.Length > thousandDigits))
            {
                entryResult.Text = $"{decimal.Parse(concat):N0}";
            }
            else
            {
                entryResult.Text = concat;
            }
        }
    }

    private void entryResult_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;

        if (entry?.Text == "0")
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

    private void btnE_Clicked(object sender, EventArgs e)
    {
        entryResult.Text = ToCustomString(Math.E);
    }

    private void btnLn_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"ln({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Log(result));
        }
    }

    private void btnLog2_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"lb({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Log2(result));
        }
    }

    private void btnLog10_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"log({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Log10(result));
        }
    }

    private void btnExp_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"exp({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Exp(result));
        }
    }

    private void btnPi_Clicked(object sender, EventArgs e)
    {
        entryResult.Text = ToCustomString(Math.PI);
    }

    private void btnSin_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"sin({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Sin(result));
        }
    }

    private void btnCos_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"cos({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Cos(result));
        }
    }

    private void btnTg_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"tg({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Tan(result));
        }
    }

    private void btnCtg_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"ctg({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Atan(result));
        }
    }

    private void btn10LaX_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"10^{entryResult.Text}";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Pow(10, result));
        }
    }

    private void btn2LaX_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"2^{entryResult.Text}";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Pow(2, result));
        }
    }

    private void btnSqrt_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"sqrt({entryResult.Text})";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Sqrt(result));
        }
    }

    private void btnXla2_Clicked(object sender, EventArgs e)
    {
        entryCalculation.Text = $"{entryResult.Text}^2";

        if (double.TryParse(entryResult.Text, out double result))
        {
            entryResult.Text = ToCustomString(Math.Pow(result, 2));
        }
    }

    private void btnXlaY_Clicked(object sender, EventArgs e)
    {
        BuildOperation(POW);
    }

    private string ToCustomString(double input)
    {
        if (input < 1e12)
            return input.ToString("#,##0.##########");
        else
            return input.ToString("E", CultureInfo.InvariantCulture);
    }

    private void btnModulo_Clicked(object sender, EventArgs e)
    {
        BuildOperation(MOD);
    }

    private void btnAbout_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AboutPage());
    }

    private void ContentPage_SizeChanged(object sender, EventArgs e)
    {
        if(null == pickerView.ItemsSource[pickerView.SelectedIndex])
        {
            return;
        }

        switch (DeviceDisplay.Current.MainDisplayInfo.Orientation)
        {
            case DisplayOrientation.Landscape:
                StandardButtonFontSize = 10;

                if ((string)pickerView.ItemsSource[pickerView.SelectedIndex] == CHART_VIEW)
                {
                    rowDisplayText.Height = new GridLength(ChartViewRowDisplayTextWhenLandScape, GridUnitType.Star);
                    rowDisplayChart.Height = new GridLength(ChartViewRowDisplayChartWhenLandScape, GridUnitType.Star);
                    rowInput.Height = new GridLength(ChartViewRowInputWhenLandscape, GridUnitType.Star);
                }
                else if ((string)pickerView.ItemsSource[pickerView.SelectedIndex] == SCIENTIFIC_VIEW)
                {
                    rowDisplayText.Height = new GridLength(OtherViewsRowDisplayTextWhenLandScape, GridUnitType.Star);
                    ArrangeEntrys(EntryCalculationNormalFont / 2, EntryResultNormalFont / 2, LayoutOptions.Fill);
                    rowInput.Height = new GridLength(OtherViewsRowInput, GridUnitType.Star);
                }
                else // means simple
                {
                    rowInput.Height = new GridLength(OtherViewsRowInput, GridUnitType.Star);
                }
                break;

            case DisplayOrientation.Portrait:
                StandardButtonFontSize = 20;

                if ((string)pickerView.ItemsSource[pickerView.SelectedIndex] == CHART_VIEW)
                {
                    rowDisplayText.Height = new GridLength(ChartViewRowDisplayText, GridUnitType.Star);
                    rowDisplayChart.Height = new GridLength(ChartViewRowDisplayChart, GridUnitType.Star);
                    rowInput.Height = new GridLength(ChartViewRowInput, GridUnitType.Star);
                }
                else if ((string)pickerView.ItemsSource[pickerView.SelectedIndex] == SCIENTIFIC_VIEW)
                {
                    rowDisplayText.Height = new GridLength(OtherViewsRowDisplayText, GridUnitType.Star);
                    ArrangeEntrys(EntryCalculationNormalFont, EntryResultNormalFont, LayoutOptions.Fill);
                    rowInput.Height = new GridLength(OtherViewsRowInput, GridUnitType.Star);
                }
                else // means simple
                {
                    rowInput.Height = new GridLength(OtherViewsRowInput, GridUnitType.Star);
                }
                break;
        }
    }

    private void btnCancelLabel_Clicked(object sender, EventArgs e)
    {
        rowLabelInformation.Height = new GridLength(0, GridUnitType.Star);
    }
}


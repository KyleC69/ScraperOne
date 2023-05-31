using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ScraperOne.Controls;

public class Indicator : TemplatedControl
{
    public Indicator()
    {
    PseudoClasses.Add("active");
    PseudoClasses.Add("inactive");
    }
    
    
    
    
    
    
    public static readonly StyledProperty<IBrush> IndicatorBackgroundProperty = AvaloniaProperty.Register<Indicator, IBrush>(
        "IndicatorBackground");

    public IBrush IndicatorBackground
    {
        get => GetValue(IndicatorBackgroundProperty);
        set => SetValue(IndicatorBackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> ActiveIndicatorColorProperty = AvaloniaProperty.Register<Indicator, IBrush>(
        "ActiveIndicatorColor");

    public IBrush ActiveIndicatorColor
    {
        get => GetValue(ActiveIndicatorColorProperty);
        set => SetValue(ActiveIndicatorColorProperty, value);
    }

    public static readonly StyledProperty<IBrush> InActiveIndicatorColorProperty = AvaloniaProperty.Register<Indicator, IBrush>(
        "InActiveIndicatorColor");

    public IBrush InActiveIndicatorColor
    {
        get => GetValue(InActiveIndicatorColorProperty);
        set => SetValue(InActiveIndicatorColorProperty, value);
    }

    public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<Indicator, bool>(
        "IsActive");

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }


    public static readonly StyledProperty<string> IndicatorTextProperty = AvaloniaProperty.Register<Indicator, string>(
        "IndicatorText");

    public string IndicatorText
    {
        get => GetValue(IndicatorTextProperty);
        set => SetValue(IndicatorTextProperty, value);
    }
    
    
    
    
    
    
    
    
}
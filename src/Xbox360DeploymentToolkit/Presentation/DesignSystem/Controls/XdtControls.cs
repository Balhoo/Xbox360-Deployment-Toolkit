using System.Windows;
using System.Windows.Controls;

namespace Xbox360DeploymentToolkit.Presentation.DesignSystem.Controls;

public enum XdtButtonVariant { Primary, Secondary, Success, Warning, Danger, Ghost, Outline, Text }
public enum XdtControlSize { Small, Medium, Large }
public enum XdtStatusVariant { Success, Warning, Error, Pending, Offline, Completed, InProgress }
public enum XdtCardType { Default, Metric, Progress, Status, Warning, Report, Game }
public enum XdtBannerVariant { Info, Warning, Success, Error }

public sealed class XdtButton : Button
{
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(XdtButtonVariant), typeof(XdtButton), new PropertyMetadata(XdtButtonVariant.Secondary));
    public static readonly DependencyProperty ControlSizeProperty = DependencyProperty.Register(nameof(ControlSize), typeof(XdtControlSize), typeof(XdtButton), new PropertyMetadata(XdtControlSize.Medium));
    public XdtButtonVariant Variant { get => (XdtButtonVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public XdtControlSize ControlSize { get => (XdtControlSize)GetValue(ControlSizeProperty); set => SetValue(ControlSizeProperty, value); }
}

public class XdtCard : ContentControl
{
    public static readonly DependencyProperty CardTypeProperty = DependencyProperty.Register(nameof(CardType), typeof(XdtCardType), typeof(XdtCard), new PropertyMetadata(XdtCardType.Default));
    public XdtCardType CardType { get => (XdtCardType)GetValue(CardTypeProperty); set => SetValue(CardTypeProperty, value); }
}

public class XdtStatusBadge : ContentControl
{
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(XdtStatusVariant), typeof(XdtStatusBadge), new PropertyMetadata(XdtStatusVariant.Pending));
    public XdtStatusVariant Variant { get => (XdtStatusVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
}

public class XdtBanner : HeaderedContentControl
{
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(XdtBannerVariant), typeof(XdtBanner), new PropertyMetadata(XdtBannerVariant.Info));
    public XdtBannerVariant Variant { get => (XdtBannerVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
}

public class XdtAppShell : ContentControl { }
public class XdtSidebar : ContentControl { }
public class XdtPageHeader : HeaderedContentControl { }
public class XdtTextBox : TextBox { }
public class XdtProgressBar : ProgressBar { }
public class XdtMetricCard : XdtCard { }
public class XdtDialog : HeaderedContentControl { }
public class XdtGameCard : XdtCard { }
public class XdtChecklistItem : ContentControl { }
public class XdtInspectorPanel : ContentControl { }
public class XdtTransferQueue : ItemsControl { }
public class XdtToast : HeaderedContentControl { }
public class XdtEmptyState : HeaderedContentControl { }

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConvertTests.cs" company="">
//   
// </copyright>
// <summary>
//   The convert tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SharpTpCppConverterTests
{
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Converters;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using StyleCop;
    using StyleCop.CSharp;
    using System.Collections.Generic;

    /// <summary>
    /// The convert tests.
    /// </summary>
    [TestClass]
    public class ConvertTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert file.
        /// </summary>
        /// <param name="code">
        /// The code.
        /// </param>
        public void ConvertFile(string code)
        {
            var filePath = Path.Combine(
                @"G:\Builds\tests\", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".cs");
            var streamWriter = new StreamWriter(filePath);
            streamWriter.Write(code);
            streamWriter.Close();

            ////SharpToCppConverter.DoNotSuppressExceptions = true;
            SharpToCppConverter.ConvertFile(
                filePath,
                @"G:\Builds\tests\",
                // Path.GetDirectoryName(filePath),
                @"C:\Users\Alexander\Documents\Visual Studio 2012\Projects\App1\App1",
                @"C:\Program Files (x86)\Windows Kits\8.0\References\CommonConfiguration\Neutral\;C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.VCLibs\11.0\References\CommonConfiguration\neutral\");

            File.Delete(filePath);
        }

        public SharpToCppInterpreter GetInterpreter(string code)
        {
            var filePath = Path.Combine(
                @"G:\Builds\tests\", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".cs");
            var streamWriter = new StreamWriter(filePath);
            streamWriter.Write(code);
            streamWriter.Close();

            var interpreters = SharpToCppConverter.GetCsharpInterpretersFromFiles(
                new[] { filePath },
                @"G:\Builds\tests\",
                // Path.GetDirectoryName(filePath),
                @"C:\Users\Alexander\Documents\Visual Studio 2012\Projects\App1\App1",
                @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.VCLibs\11.0\References\CommonConfiguration\neutral\;C:\Program Files (x86)\Windows Kits\8.0\References\CommonConfiguration\Neutral\");

            File.Delete(filePath);

            return interpreters.FirstOrDefault();
        }

        /// <summary>
        /// The convert header to cs dummay.
        /// </summary>
        /// <param name="code">
        /// The code.
        /// </param>
        public void ConvertHeaderToCsDummay(string code)
        {
            var filePath = Path.Combine(
                @"G:\Builds\tests\", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".h");
            var streamWriter = new StreamWriter(filePath);
            streamWriter.Write(code);
            streamWriter.Close();

            SharpToCppConverter.DoNotSuppressExceptions = true;
            SharpToCppConverter.ParseHeaderFile(filePath, SharpToCppConverter.CreateProject(Path.GetDirectoryName(filePath)));

            File.Delete(filePath);
        }

        /// <summary>
        /// The convert_ app_ xaml.
        /// </summary>
        [TestMethod]
        public void ConvertAppXaml()
        {
            this.ConvertFile(@"
            #pragma include ""App.g.h""
            #pragma include ""Common\SuspensionManager.h""
            #pragma include ""GroupedItemsPage.xaml.h""

            using App1.Common;

            //using Concurrency;
            //using Platform;
            using Windows.ApplicationModel;
            using Windows.ApplicationModel.Activation;
            using Windows.Foundation;
            using Windows.Foundation.Collections;
            using Windows.UI.Xaml;
            using Windows.UI.Xaml.Controls;
            using Windows.UI.Xaml.Controls.Primitives;
            using Windows.UI.Xaml.Data;
            using Windows.UI.Xaml.Input;
            using Windows.UI.Xaml.Media;
            using Windows.UI.Xaml.Navigation;

            // The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

            namespace App1
            {
                /// <summary>
                /// Provides application-specific behavior to supplement the default Application class.
                /// </summary>
                sealed partial class App : Application
                {
                    /// <summary>
                    /// Initializes the singleton Application object.  This is the first line of authored code
                    /// executed, and as such is the logical equivalent of main() or WinMain().
                    /// </summary>
                    public App()
                    {
                        this.InitializeComponent();
                        //this.Suspending += OnSuspending;
                        this.Suspending += new SuspendingEventHandler(this, ref App.OnSuspending);
                    }

                    /// <summary>
                    /// Invoked when the application is launched normally by the end user.  Other entry points
                    /// will be used when the application is launched to open a specific file, to display
                    /// search results, and so forth.
                    /// </summary>
                    /// <param name=""args"">Details about the launch request and process.</param>
                    protected override async void OnLaunched(LaunchActivatedEventArgs args)
                    {
                        Frame rootFrame = Window.Current.Content as Frame;

                        // Do not repeat app initialization when the Window already has content,
                        // just ensure that the window is active
            
                        if (rootFrame == null)
                        {
                            // Create a Frame to act as the navigation context and navigate to the first page
                            rootFrame = new Frame();
                            //Associate the frame with a SuspensionManager key                                
                            SuspensionManager.RegisterFrame(rootFrame, ""AppFrame"");

                            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                            {
                                // Restore the saved session state only when appropriate
                                try
                                {
                                    await SuspensionManager.RestoreAsync();
                                }
                                catch (SuspensionManagerException)
                                {
                                    //Something went wrong restoring state.
                                    //Assume there is no state and continue
                                }
                            }

                            // Place the frame in the current Window
                            Window.Current.Content = rootFrame;
                        }
                        if (rootFrame.Content == null)
                        {
                            // When the navigation stack isn't restored navigate to the first page,
                            // configuring the new page by passing required information as a navigation
                            // parameter
                            if (!rootFrame.Navigate(typeof(GroupedItemsPage), ""AllGroups""))
                            {
                                throw new Exception(""Failed to create initial page"");
                            }
                        }
                        // Ensure the current window is active
                        Window.Current.Activate();
                    }

                    /// <summary>
                    /// Invoked when application execution is being suspended.  Application state is saved
                    /// without knowing whether the application will be terminated or resumed with the contents
                    /// of memory still intact.
                    /// </summary>
                    /// <param name=""sender"">The source of the suspend request.</param>
                    /// <param name=""e"">Details about the suspend request.</param>
                    private async void OnSuspending(object sender, SuspendingEventArgs e)
                    {
                        var deferral = e.SuspendingOperation.GetDeferral();
                        //await SuspensionManager.SaveAsync();
                        SuspensionManager.SaveAsync().then(() => deferral.Complete());
                        //deferral.Complete();
                    }
                }
            }");
        }

        /// <summary>
        /// The convert_ app_ xaml.
        /// </summary>
        [TestMethod]
        public void BindableBase()
        {
            this.ConvertFile(@"
                using Platform;
                using Windows.UI.Xaml.Data;

                namespace App1.Common
                {
                    /// <summary>
                    /// Implementation of <see cref=""INotifyPropertyChanged""/> to simplify models.
                    /// </summary>
                    [Windows.Foundation.Metadata.WebHostHidden]
                    public abstract class BindableBase_ : Windows.UI.Xaml.DependencyObject, 
                                                          Windows.UI.Xaml.Data.INotifyPropertyChanged, 
                                                          Windows.UI.Xaml.Data.ICustomPropertyProvider
                    {
                        /// <summary>
                        /// Multicast event for property change notifications.
                        /// </summary>
                        public virtual event PropertyChangedEventHandler PropertyChanged;

	                    // ICustomPropertyProvider
		                public virtual Windows.UI.Xaml.Data.ICustomProperty GetCustomProperty(string name)
                        {
                            return null;
                        }
		
                        public virtual Windows.UI.Xaml.Data.ICustomProperty GetIndexedProperty(string name, Windows.UI.Xaml.Interop.TypeName type)
                        {
                            return null;
                        }   
		
                        public virtual string GetStringRepresentation()
                        {
                            return this.ToString();
                        }

		                public virtual Windows.UI.Xaml.Interop.TypeName ElementType
		                {
			                get { return this.GetType(); }
		                }

                        /// <summary>
                        /// Notifies listeners that a property value has changed.
                        /// </summary>
                        /// <param name=""propertyName"">Name of the property used to notify listeners.
                        protected void OnPropertyChanged(string propertyName)
                        {
                            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }");
        }

        [TestMethod]
        public void BooleanNegationConverter()
        {
            this.ConvertFile(@"
                using Platform;
                using Windows.UI.Xaml.Data;

                namespace App1.Common
                {
                    /// <summary>
                    /// Value converter that translates true to false and vice versa.
                    /// </summary>
                    public sealed class BooleanNegationConverter_ : IValueConverter
                    {
                        public object Convert(object value, Windows.UI.Xaml.Interop.TypeName targetType, object parameter, string language)
                        {
	                        var boxedBool = value as Box<bool>;
	                        var boolValue = (boxedBool != null && boxedBool.Value);
	                        return !boolValue;
                        }

                        public object ConvertBack(object value, Windows.UI.Xaml.Interop.TypeName targetType, object parameter, string language)
                        {
                            var boxedBool = value as Box<bool>;
                            var boolValue = (boxedBool != null && boxedBool.Value);
                            return !boolValue;
                        }
                    }
                }");
        }

        [TestMethod]
        public void BooleanToVisibilityConverter()
        {
            this.ConvertFile(@"
                using Platform;
                using Windows.Foundation;
                using Windows.Foundation.Collections;
                using Windows.Graphics.Display;
                using Windows.UI.ViewManagement;
                using Windows.UI.Xaml;
                using Windows.UI.Xaml.Controls;
                using Windows.UI.Xaml.Data;

                namespace App1.Common
                {
                    /// <summary>
                    /// Value converter that translates true to <see cref=""Visibility.Visible""/> and false to
                    /// <see cref=""Visibility.Collapsed""/>.
                    /// </summary>
                    public sealed class BooleanToVisibilityConverter : IValueConverter
                    {
                        public virtual object Convert(object value, Windows.UI.Xaml.Interop.TypeName targetType, object parameter, string language)
                        {
	                        var boxedBool = value as Box<bool>;
	                        var boolValue = (boxedBool != null && boxedBool.Value);
	                        return (boolValue ? Visibility.Visible : Visibility.Collapsed);
                        }

                        public virtual object ConvertBack(object value, Windows.UI.Xaml.Interop.TypeName targetType, object parameter, string language)
                        {
	                        var visibility = value as Box<Visibility>;
	                        return (visibility != null && visibility.Value == Visibility.Visible);
                        }
                    }
                }");
        }

        [TestMethod]
        public void GroupDetailPage()
        {
            this.ConvertFile(
              @"#pragma include ""Common\LayoutAwarePage.h""
                #pragma include ""GroupDetailPage.g.h""
                #pragma include ""DataModel\SampleDataSource.h""

                using App1.Data;

                using Platform;
                using Windows.Foundation;
                using Windows.Foundation.Collections;
                using Windows.UI.Xaml;
                using Windows.UI.Xaml.Controls;
                using Windows.UI.Xaml.Controls.Primitives;
                using Windows.UI.Xaml.Data;
                using Windows.UI.Xaml.Input;
                using Windows.UI.Xaml.Media;
                using Windows.UI.Xaml.Navigation;

                // The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229

                namespace App1
                {
                    /// <summary>
                    /// A page that displays an overview of a single group, including a preview of the items
                    /// within the group.
                    /// </summary>
                    public sealed partial class GroupDetailPage
                    {
                        public GroupDetailPage()
                        {
                            this.InitializeComponent();
                        }

                        /// <summary>
                        /// Populates the page with content passed during navigation.  Any saved state is also
                        /// provided when recreating a page from a prior session.
                        /// </summary>
                        /// <param name=""navigationParameter"">The parameter value passed to
                        /// <see cref=""Frame.Navigate(ElementType, Object)""/> when this page was initially requested.
                        /// </param>
                        /// <param name=""pageState"">A dictionary of state preserved by this page during an earlier
                        /// session.  This will be null the first time a page is visited.</param>
                        protected override void LoadState(Object navigationParameter, IMap<String, Object> pageState)
                        {
                            // TODO: Create an appropriate data model for your problem domain to replace the sample data
                            var group = SampleDataSource.GetGroup((String)navigationParameter);
                            this.DefaultViewModel.Insert(""Group"", group);
                            this.DefaultViewModel.Insert(""Items"", group.Items);
                        }

                        /// <summary>
                        /// Invoked when an item is clicked.
                        /// </summary>
                        /// <param name=""sender"">The GridView (or ListView when the application is snapped)
                        /// displaying the item clicked.</param>
                        /// <param name=""e"">Event data that describes the item clicked.</param>
                        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
                        {
                            // Navigate to the appropriate destination page, configuring the new page
                            // by passing required information as a navigation parameter
                            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
                            this.Frame.Navigate(typeof(ItemDetailPage), itemId);
                        }
                    }
                }");
        }

        [TestMethod]
        public void ItemDetailPage()
        {
            this.ConvertFile(
              @"#pragma include ""Common\LayoutAwarePage.h""
                #pragma include ""Common\RichTextColumns.h""
                #pragma include ""ItemDetailPage.g.h""

                using App1;

                using App1.Data;
                using App1.Common;

                using Platform;
                using Windows.Foundation;
                using Windows.Foundation.Collections;
                using Windows.UI.Xaml;
                using Windows.UI.Xaml.Controls;
                using Windows.UI.Xaml.Controls.Primitives;
                using Windows.UI.Xaml.Data;
                using Windows.UI.Xaml.Input;
                using Windows.UI.Xaml.Interop;
                using Windows.UI.Xaml.Media;
                using Windows.UI.Xaml.Navigation;

                // The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

                namespace App1
                {
                    /// <summary>
                    /// A page that displays details for a single item within a group while allowing gestures to
                    /// flip through other items belonging to the same group.
                    /// </summary>
                    public sealed partial class ItemDetailPage
                    {
                        public ItemDetailPage()
                        {
                            this.InitializeComponent();
                        }

                        /// <summary>
                        /// Populates the page with content passed during navigation.  Any saved state is also
                        /// provided when recreating a page from a prior session.
                        /// </summary>
                        /// <param name=""navigationParameter"">The parameter value passed to
                        /// <see cref=""Frame.Navigate(ElementType, Object)""/> when this page was initially requested.
                        /// </param>
                        /// <param name=""pageState"">A dictionary of state preserved by this page during an earlier
                        /// session.  This will be null the first time a page is visited.</param>
                        protected override void LoadState(object navigationParameter, IMap<String, Object> pageState)
                        {
                            // Allow saved page state to override the initial item to display
                            if (pageState != null && pageState.HasKey(""SelectedItem""))
                            {
                                navigationParameter = pageState.Lookup(""SelectedItem"");
                            }

                            // TODO: Create an appropriate data model for your problem domain to replace the sample data
                            var item = SampleDataSource.GetItem((String)navigationParameter);
                            this.DefaultViewModel.Insert(""Group"", item.Group);
                            this.DefaultViewModel.Insert(""Items"", item.Group.Items);
                            this.flipView.SelectedItem = item;
                        }

                        /// <summary>
                        /// Preserves state associated with this page in case the application is suspended or the
                        /// page is discarded from the navigation cache.  Values must conform to the serialization
                        /// requirements of <see cref=""SuspensionManager.SessionState""/>.
                        /// </summary>
                        /// <param name=""pageState"">An empty dictionary to be populated with serializable state.</param>
                        protected override void SaveState(IMap<String, Object> pageState)
                        {
                            var selectedItem = (SampleDataItem)this.flipView.SelectedItem;
                            pageState.Insert(""SelectedItem"", selectedItem.UniqueId);
                        }
                    }
                }");
        }

        [TestMethod]
        public void RichTextColumns()
        {
            this.ConvertFile(
              @"#pragma include <collection.h>

                using Platform;
                using Platform.Collections;
                using Windows.Foundation;
                using Windows.Foundation.Collections;
                using Windows.UI.Xaml;
                using Windows.UI.Xaml.Controls;
                using Windows.UI.Xaml.Data;
                using Windows.UI.Xaml.Documents;
                using Windows.UI.Xaml.Interop;

                namespace App1.Common
                {
                    /// <summary>
                    /// Wrapper for <see cref=""RichTextBlock""/> that creates as many additional overflow
                    /// columns as needed to fit the available content.
                    /// </summary>
                    /// <example>
                    /// The following creates a collection of 400-pixel wide columns spaced 50 pixels apart
                    /// to contain arbitrary data-bound content:
                    /// <code>
                    /// <RichTextColumns>
                    ///     <RichTextColumns.ColumnTemplate>
                    ///         <DataTemplate>
                    ///             <RichTextBlockOverflow Width=""400"" Margin=""50,0,0,0""/>
                    ///         </DataTemplate>
                    ///     </RichTextColumns.ColumnTemplate>
                    ///     
                    ///     <RichTextBlock Width=""400"">
                    ///         <Paragraph>
                    ///             <Run Text=""{Binding Content}""/>
                    ///         </Paragraph>
                    ///     </RichTextBlock>
                    /// </RichTextColumns>
                    /// </code>
                    /// </example>
                    /// <remarks>Typically used in a horizontally scrolling region where an unbounded amount of
                    /// space allows for all needed columns to be created.  When used in a vertically scrolling
                    /// space there will never be any additional columns.</remarks>
                    [Windows.UI.Xaml.Markup.ContentProperty(Name = ""RichTextContent"")]
                    [Windows.Foundation.Metadata.WebHostHidden]
                    public sealed class RichTextColumns : Panel
                    {
                        /// <summary>
                        /// Identifies the <see cref=""RichTextContent""/> dependency property.
                        /// </summary>
                        private static readonly DependencyProperty _richTextContentProperty =
                            DependencyProperty.Register(""RichTextContent"", typeof(RichTextBlock),
                            typeof(RichTextColumns), new PropertyMetadata(null, new PropertyChangedCallback(ref RichTextColumns.ResetOverflowLayout)));

                        /// <summary>
                        /// Identifies the <see cref=""ColumnTemplate""/> dependency property.
                        /// </summary>
                        private static readonly DependencyProperty _columnTemplateProperty =
                            DependencyProperty.Register(""ColumnTemplate"", typeof(DataTemplate),
                            typeof(RichTextColumns), new PropertyMetadata(null, new PropertyChangedCallback(ref RichTextColumns.ResetOverflowLayout)));

                        /// <summary>
                        /// Initializes a new instance of the <see cref=""RichTextColumns""/> class.
                        /// </summary>
                        public RichTextColumns()
                        {
                            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                        }

                        public static DependencyProperty RichTextContentProperty
                        {
                            get { return _richTextContentProperty; }
                        }

                        public static DependencyProperty ColumnTemplateProperty
                        {
                            get { return _columnTemplateProperty; }
                        }

                        /// <summary>
                        /// Gets or sets the initial rich text content to be used as the first column.
                        /// </summary>
                        public RichTextBlock RichTextContent
                        {
                            get { return (RichTextBlock)GetValue(RichTextContentProperty); }
                            set { SetValue(RichTextContentProperty, value); }
                        }

                        /// <summary>
                        /// Gets or sets the template used to create additional
                        /// <see cref=""RichTextBlockOverflow""/> instances.
                        /// </summary>
                        public DataTemplate ColumnTemplate
                        {
                            get { return (DataTemplate)GetValue(ColumnTemplateProperty); }
                            set { SetValue(ColumnTemplateProperty, value); }
                        }

                        /// <summary>
                        /// Invoked when the content or overflow template is changed to recreate the column layout.
                        /// </summary>
                        /// <param name=""d"">Instance of <see cref=""RichTextColumns""/> where the change
                        /// occurred.</param>
                        /// <param name=""e"">Event data describing the specific change.</param>
                        private static void ResetOverflowLayout(DependencyObject d, DependencyPropertyChangedEventArgs e)
                        {
                            // When dramatic changes occur, rebuild the column layout from scratch
                            var target = d as RichTextColumns;
                            if (target != null)
                            {
                                target._overflowColumns = null;
                                target.Children.Clear();
                                target.InvalidateMeasure();
                            }
                        }

                        /// <summary>
                        /// Lists overflow columns already created.  Must maintain a 1:1 relationship with
                        /// instances in the <see cref=""Panel.Children""/> collection following the initial
                        /// RichTextBlock child.
                        /// </summary>
                        private IVector<RichTextBlockOverflow> _overflowColumns = null;

                        /// <summary>
                        /// Determines whether additional overflow columns are needed and if existing columns can
                        /// be removed.
                        /// </summary>
                        /// <param name=""availableSize"">The size of the space available, used to constrain the
                        /// number of additional columns that can be created.</param>
                        /// <returns>The resulting size of the original content plus any extra columns.</returns>
                        protected override Size MeasureOverride(Size availableSize)
                        {
                            if (this.RichTextContent == null) return Size(0, 0);

                            // Make sure the RichTextBlock is a child, using the lack of
                            // a list of additional columns as a sign that this hasn't been
                            // done yet
                            if (this._overflowColumns == null)
                            {
                                Children.Append(this.RichTextContent);
                                this._overflowColumns = new Vector<RichTextBlockOverflow>();
                            }

                            // Start by measuring the original RichTextBlock content
                            this.RichTextContent.Measure(availableSize);
                            var maxWidth = this.RichTextContent.DesiredSize.Width;
                            var maxHeight = this.RichTextContent.DesiredSize.Height;
                            var hasOverflow = this.RichTextContent.HasOverflowContent;

                            // Make sure there are enough overflow columns
                            int overflowIndex = 0;
                            while (hasOverflow && maxWidth < availableSize.Width && this.ColumnTemplate != null)
                            {
                                // Use existing overflow columns until we run out, then create
                                // more from the supplied template
                                RichTextBlockOverflow overflow;
                                if (this._overflowColumns.Size > overflowIndex)
                                {
                                    overflow = this._overflowColumns.GetAt(overflowIndex);
                                }
                                else
                                {
                                    overflow = (RichTextBlockOverflow)this.ColumnTemplate.LoadContent();
                                    this._overflowColumns.Append(overflow);
                                    this.Children.Append(overflow);
                                    if (overflowIndex == 0)
                                    {
                                        this.RichTextContent.OverflowContentTarget = overflow;
                                    }
                                    else
                                    {
                                        this._overflowColumns.GetAt(overflowIndex - 1).OverflowContentTarget = overflow;
                                    }
                                }

                                // Measure the new column and prepare to repeat as necessary
                                overflow.Measure(Size(availableSize.Width - maxWidth, availableSize.Height));
                                maxWidth += overflow.DesiredSize.Width;
                                maxHeight = Math.Max(maxHeight, overflow.DesiredSize.Height);
                                hasOverflow = overflow.HasOverflowContent;
                                overflowIndex++;
                            }

                            // Disconnect extra columns from the overflow chain, remove them from our private list
                            // of columns, and remove them as children
                            if (this._overflowColumns.Count > overflowIndex)
                            {
                                if (overflowIndex == 0)
                                {
                                    this.RichTextContent.OverflowContentTarget = null;
                                }
                                else
                                {
                                    this._overflowColumns.GetAt(overflowIndex - 1).OverflowContentTarget = null;
                                }
                                while (this._overflowColumns.Size > overflowIndex)
                                {
                                    this._overflowColumns.RemoveAt(overflowIndex);
                                    this.Children.RemoveAt(overflowIndex + 1);
                                }
                            }

                            // Report final determined size
                            return Size(maxWidth, maxHeight);
                        }

                        /// <summary>
                        /// Arranges the original content and all extra columns.
                        /// </summary>
                        /// <param name=""finalSize"">Defines the size of the area the children must be arranged
                        /// within.</param>
                        /// <returns>The size of the area the children actually required.</returns>
                        protected override Size ArrangeOverride(Size finalSize)
                        {
                            double maxWidth = 0;
                            double maxHeight = 0;
                            foreach (var child in Children)
                            {
                                child.Arrange(new Rect(maxWidth, 0, child.DesiredSize.Width, finalSize.Height));
                                maxWidth += child.DesiredSize.Width;
                                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
                            }
                            return new Size(maxWidth, maxHeight);
                        }
                    }
                }");
        }

        [TestMethod]
        public void SuspensionManager()
        {
            this.ConvertFile(@"
                #pragma include <ppltasks.h>
                #pragma include <collection.h>

                using App1.Common;

                using Concurrency;
                using Platform;
                using Platform.Collections;
                using Windows.Foundation;
                using Windows.Foundation.Collections;
                using Windows.Storage;
                using Windows.Storage.FileProperties;
                using Windows.Storage.Streams;
                using Windows.UI.Xaml;
                using Windows.UI.Xaml.Controls;
                using Windows.UI.Xaml.Interop;

                namespace App1.Common
                {
                    /// <summary>
                    /// Wrap a WeakReference as a reference object for use in a collection.
                    /// </summary>
                    internal sealed class WeakFrame
                    {
                        private Platform.WeakReference _frameReference;

                        internal WeakFrame(Frame frame) { _frameReference = frame; }

                        internal Frame ResolvedFrame
                        {
                            get { return _frameReference.Resolve<Frame>(); }
                        }
                    }

                    /// <summary>
                    /// SuspensionManager captures global session state to simplify process lifetime management
                    /// for an application.  Note that session state will be automatically cleared under a variety
                    /// of conditions and should only be used to store information that would be convenient to
                    /// carry across sessions, but that should be discarded when an application crashes or is
                    /// upgraded.
                    /// </summary>
                    internal sealed class SuspensionManager
                    {
                        private static IMap<string, object> _sessionState = new Map<string, object>();
                        private static string sessionStateFilename = ""_sessionState.dat"";

                        /// <summary>
                        /// Provides access to global session state for the current session.  This state is
                        /// serialized by <see cref=""SaveAsync""/> and restored by
                        /// <see cref=""RestoreAsync""/>, so values must be serializable by
                        /// <see cref=""DataContractSerializer""/> and should be as compact as possible.  Strings
                        /// and other self-contained data types are strongly recommended.
                        /// </summary>
                        public static IMap<string, object> SessionState
                        {
                            get { return _sessionState; }
                        }

                        /// <summary>
                        /// Save the current <see cref=""SessionState""/>.  Any <see cref=""Frame""/> instances
                        /// registered with <see cref=""RegisterFrame""/> will also preserve their current
                        /// navigation stack, which in turn gives their active <see cref=""Page""/> an opportunity
                        /// to save its state.
                        /// </summary>
                        /// <returns>An asynchronous task that reflects when session state has been saved.</returns>
                        internal static Concurrency.task<void> SaveAsync()
                        {
                            // Save the navigation state for all registered frames
                            foreach (var weakFrame in _registeredFrames)
                            {
                                var frame = weakFrame.Value.ResolvedFrame;
                                if (frame != null) SaveFrameNavigationState(frame);
                            }

                            // Serialize the session state synchronously to avoid asynchronous access to shared
                            // state
                            var sessionData = new InMemoryRandomAccessStream();
                            var sessionDataWriter = new DataWriter(sessionData.GetOutputStreamAt(0));
                            WriteObject(sessionDataWriter, _sessionState);

                            // Once session state has been captured synchronously, begin the asynchronous process
                            // of writing the result to disk
                            return task<uint>(sessionDataWriter.StoreAsync()).then((uint bytes) =>
                            {
                                return sessionDataWriter.FlushAsync();
                            }).then((bool flushSucceeded) =>
                            {
                                return ApplicationData.Current.LocalFolder.CreateFileAsync(sessionStateFilename, CreationCollisionOption.ReplaceExisting);
                            }).then((StorageFile createdFile) =>
                            {
                                return createdFile.OpenAsync(FileAccessMode.ReadWrite);
                            }).then((IRandomAccessStream newStream) =>
                            {
                                return RandomAccessStream.CopyAndCloseAsync(
                                    sessionData.GetInputStreamAt(0), newStream.GetOutputStreamAt(0));
                            }).then((UINT64 copiedBytes) =>
                            {
                                return;
                            });
                        }

                        /// <summary>
                        /// Restores previously saved <see cref=""SessionState""/>.  Any <see cref=""Frame""/> instances
                        /// registered with <see cref=""RegisterFrame""/> will also restore their prior navigation
                        /// state, which in turn gives their active <see cref=""Page""/> an opportunity restore its
                        /// state.
                        /// </summary>
                        /// <returns>An asynchronous task that reflects when session state has been read.  The
                        /// content of <see cref=""SessionState""/> should not be relied upon until this task
                        /// completes.</returns>
                        internal static Concurrency.task<void> RestoreAsync()
                        {
                            _sessionState.Clear();

                            return task<StorageFile>(ApplicationData.Current.LocalFolder.GetFileAsync(sessionStateFilename)).then((StorageFile stateFile) =>
                            {
                                return task<BasicProperties>(stateFile.GetBasicPropertiesAsync()).then((BasicProperties stateFileProperties) =>
                                {
                                    var size = (uint)stateFileProperties.Size;
                                    if (size != stateFileProperties.Size) throw new FailureException(""Session state larger than 4GB"");
                                    return task<IRandomAccessStreamWithContentType>(stateFile.OpenReadAsync()).then((IRandomAccessStreamWithContentType stateFileStream) =>
                                    {
                                        var stateReader = new DataReader(stateFileStream);
                                        return task<uint>(stateReader.LoadAsync(size)).then((uint bytesRead) =>
                                        {
                                            // Deserialize the Session State
                                            object content = ReadObject(stateReader);
                                            _sessionState = (IMap<string, object>)content;

                                            // Restore any registered frames to their saved state
                                            foreach (var weakFrame in _registeredFrames)
                                            {
                                                var frame = weakFrame.Value.ResolvedFrame;
                                                if (frame != null)
                                                {
                                                    frame.ClearValue(FrameSessionStateProperty);
                                                    RestoreFrameNavigationState(frame);
                                                }
                                            }
                                        }, task_continuation_context.use_current());
                                    });
                                });
                            });
                        }

                        private static DependencyProperty FrameSessionStateKeyProperty =
                            DependencyProperty.RegisterAttached(""_FrameSessionStateKey"", typeof(string), typeof(SuspensionManager), null);
                        private static DependencyProperty FrameSessionStateProperty =
                            DependencyProperty.RegisterAttached(""_FrameSessionState"", typeof(IMap<string, object>), typeof(SuspensionManager), null);
                        private static IMap<string, WeakFrame> _registeredFrames = new Map<string, WeakFrame>();
                        /// <summary>
                        /// Registers a <see cref=""Frame""/> instance to allow its navigation history to be saved to
                        /// and restored from <see cref=""SessionState""/>.  Frames should be registered once
                        /// immediately after creation if they will participate in session state management.  Upon
                        /// registration if state has already been restored for the specified key
                        /// the navigation history will immediately be restored.  Subsequent invocations of
                        /// <see cref=""RestoreAsync""/> will also restore navigation history.
                        /// </summary>
                        /// <param name=""frame"">An instance whose navigation history should be managed by
                        /// <see cref=""SuspensionManager""/></param>
                        /// <param name=""sessionStateKey"">A unique key into <see cref=""SessionState""/> used to
                        /// store navigation-related information.</param>
                        public static void RegisterFrame(Frame frame, String sessionStateKey)
                        {
                            if (frame.GetValue(FrameSessionStateKeyProperty) != null)
                            {
                                throw new FailureException(""Frames can only be registered to one session state key"");
                            }

                            if (frame.GetValue(FrameSessionStateProperty) != null)
                            {
                                throw new FailureException(""Frames must be either be registered before accessing frame session state, or not registered at all"");
                            }

                            // Use a dependency property to associate the session key with a frame, and keep a IVector of frames whose
                            // navigation state should be managed
                            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
                            _registeredFrames.Insert(sessionStateKey, new WeakFrame(frame));

                            // Check to see if navigation state can be restored
                            RestoreFrameNavigationState(frame);
                        }

                        /// <summary>
                        /// Disassociates a <see cref=""Frame""/> previously registered by <see cref=""RegisterFrame""/>
                        /// from <see cref=""SessionState""/>.  Any navigation state previously captured will be
                        /// removed.
                        /// </summary>
                        /// <param name=""frame"">An instance whose navigation history should no longer be
                        /// managed.</param>
                        public static void UnregisterFrame(Frame frame)
                        {
                            // Remove session state and remove the frame from the list of frames whose navigation
                            // state will be saved (along with any weak references that are no longer reachable)
                            var key = (string)(frame.GetValue(FrameSessionStateKeyProperty));
                            if (SessionState.HasKey(key)) SessionState.Remove(key);
                            if (_registeredFrames.HasKey(key)) _registeredFrames.Remove(key);
                        }

                        /// <summary>
                        /// Provides storage for session state associated with the specified <see cref=""Frame""/>.
                        /// Frames that have been previously registered with <see cref=""RegisterFrame""/> have
                        /// their session state saved and restored automatically as a part of the global
                        /// <see cref=""SessionState""/>.  Frames that are not registered have transient state
                        /// that can still be useful when restoring pages that have been discarded from the
                        /// navigation cache.
                        /// </summary>
                        /// <remarks>Apps may choose to rely on <see cref=""LayoutAwarePage""/> to manage
                        /// page-specific state instead of working with frame session state directly.</remarks>
                        /// <param name=""frame"">The instance for which session state is desired.</param>
                        /// <returns>A collection of state subject to the same serialization mechanism as
                        /// <see cref=""SessionState""/>.</returns>
                        public static IMap<string, object> SessionStateForFrame(Frame frame)
                        {
                            var frameState = (IMap<string, object>)(frame.GetValue(FrameSessionStateProperty));

                            if (frameState == null)
                            {
                                var frameSessionKey = (string)(frame.GetValue(FrameSessionStateKeyProperty));
                                if (frameSessionKey != null)
                                {
                                    // Registered frames reflect the corresponding session state
                                    if (!_sessionState.HasKey(frameSessionKey))
                                    {
                                        _sessionState.Insert(frameSessionKey, new Map<string, object>());
                                    }

                                    frameState = (IMap<string, object>)(_sessionState.Lookup(frameSessionKey));
                                }
                                else
                                {
                                    // Frames that aren't registered have transient state
                                    frameState = new Map<string, object>();
                                }

                                frame.SetValue(FrameSessionStateProperty, frameState);
                            }
                            return frameState;
                        }

                        private static void RestoreFrameNavigationState(Frame frame)
                        {
                            var frameState = SessionStateForFrame(frame);
                            if (frameState.HasKey(""Navigation""))
                            {
                                frame.SetNavigationState((string)(frameState.Lookup(""Navigation"")));
                            }
                        }

                        private static void SaveFrameNavigationState(Frame frame)
                        {
                            var frameState = SessionStateForFrame(frame);
                            frameState.Insert(""Navigation"", frame.GetNavigationState());
                        }

                        // Codes used for identifying serialized types
                        enum StreamTypes
                        {
                            NullPtrType = 0,

                            // Supported IPropertyValue types
                            UInt8Type, UInt16Type, UInt32Type, UInt64Type, Int16Type, Int32Type, Int64Type,
                            SingleType, DoubleType, BooleanType, Char16Type, GuidType, StringType,

                            // Additional supported types
                            StringToObjectMapType,

                            // Marker values used to ensure stream integrity
                            MapEndMarker
                        };

                        static void WriteString(DataWriter writer, string _string)
                        {
                            writer.WriteByte(StreamTypes.StringType);
                            writer.WriteUInt32(writer.MeasureString(_string));
                            writer.WriteString(_string);
                        }

                        static void WriteProperty(DataWriter writer, IPropertyValue propertyValue)
                        {
                            switch (propertyValue.Type)
                            {
                                case PropertyType.UInt8:
                                    writer.WriteByte(StreamTypes.UInt8Type);
                                    writer.WriteByte(propertyValue.GetUInt8());
                                    return;
                                case PropertyType.UInt16:
                                    writer.WriteByte(StreamTypes.UInt16Type);
                                    writer.WriteUInt16(propertyValue.GetUInt16());
                                    return;
                                case PropertyType.UInt32:
                                    writer.WriteByte(StreamTypes.UInt32Type);
                                    writer.WriteUInt32(propertyValue.GetUInt32());
                                    return;
                                case PropertyType.UInt64:
                                    writer.WriteByte(StreamTypes.UInt64Type);
                                    writer.WriteUInt64(propertyValue.GetUInt64());
                                    return;
                                case PropertyType.Int16:
                                    writer.WriteByte(StreamTypes.Int16Type);
                                    writer.WriteUInt16(propertyValue.GetInt16());
                                    return;
                                case PropertyType.Int32:
                                    writer.WriteByte(StreamTypes.Int32Type);
                                    writer.WriteUInt32(propertyValue.GetInt32());
                                    return;
                                case PropertyType.Int64:
                                    writer.WriteByte(StreamTypes.Int64Type);
                                    writer.WriteUInt64(propertyValue.GetInt64());
                                    return;
                                case PropertyType.Single:
                                    writer.WriteByte(StreamTypes.SingleType);
                                    writer.WriteSingle(propertyValue.GetSingle());
                                    return;
                                case PropertyType.Double:
                                    writer.WriteByte(StreamTypes.DoubleType);
                                    writer.WriteDouble(propertyValue.GetDouble());
                                    return;
                                case PropertyType.Boolean:
                                    writer.WriteByte(StreamTypes.BooleanType);
                                    writer.WriteBoolean(propertyValue.GetBoolean());
                                    return;
                                case PropertyType.Char16:
                                    writer.WriteByte(StreamTypes.Char16Type);
                                    writer.WriteUInt16(propertyValue.GetChar16());
                                    return;
                                case PropertyType.Guid:
                                    writer.WriteByte(StreamTypes.GuidType);
                                    writer.WriteGuid(propertyValue.GetGuid());
                                    return;
                                case PropertyType.String:
                                    WriteString(writer, propertyValue.GetString());
                                    return;
                                default:
                                    throw new InvalidArgumentException(""Unsupported property type"");
                            }
                        }

                        static void WriteStringToObjectMap(DataWriter writer, IMap<string, object> map)
                        {
                            writer.WriteByte(StreamTypes.StringToObjectMapType);
                            writer.WriteUInt32(map.Size);
                            foreach (var pair in map)
                            {
                                WriteObject(writer, pair.Key);
                                WriteObject(writer, pair.Value);
                            }
                            writer.WriteByte(StreamTypes.MapEndMarker);
                        }

                        static void WriteObject(DataWriter writer, object _object)
                        {
                            if (_object == null)
                            {
                                writer.WriteByte(StreamTypes.NullPtrType);
                                return;
                            }

                            var propertyObject = _object as IPropertyValue;
                            if (propertyObject != null)
                            {
                                WriteProperty(writer, propertyObject);
                                return;
                            }

                            var mapObject = _object as IMap<string, object>;
                            if (mapObject != null)
                            {
                                WriteStringToObjectMap(writer, mapObject);
                                return;
                            }

                            throw new InvalidArgumentException(""Unsupported data type"");
                        }

                        static string ReadString(DataReader reader)
                        {
                            int length = reader.ReadUInt32();
                            string _string = reader.ReadString(length);
                            return _string;
                        }

                        static IMap<string, object> ReadStringToObjectMap(DataReader reader)
                        {
                            var map = new Map<string, object>();
                            var size = reader.ReadUInt32();
                            for (uint index = 0; index < size; index++)
                            {
                                var key = (string)(ReadObject(reader));
                                var value = ReadObject(reader);
                                map.Insert(key, value);
                            }
                            if (reader.ReadByte() != StreamTypes.MapEndMarker)
                            {
                                throw new InvalidArgumentException(""Invalid stream"");
                            }
                            return map;
                        }

                        static object ReadObject(DataReader reader)
                        {
                            var type = reader.ReadByte();
                            switch (type)
                            {
                                case StreamTypes.NullPtrType:
                                    return null;
                                case StreamTypes.UInt8Type:
                                    return reader.ReadByte();
                                case StreamTypes.UInt16Type:
                                    return reader.ReadUInt16();
                                case StreamTypes.UInt32Type:
                                    return reader.ReadUInt32();
                                case StreamTypes.UInt64Type:
                                    return reader.ReadUInt64();
                                case StreamTypes.Int16Type:
                                    return reader.ReadInt16();
                                case StreamTypes.Int32Type:
                                    return reader.ReadInt32();
                                case StreamTypes.Int64Type:
                                    return reader.ReadInt64();
                                case StreamTypes.SingleType:
                                    return reader.ReadSingle();
                                case StreamTypes.DoubleType:
                                    return reader.ReadDouble();
                                case StreamTypes.BooleanType:
                                    return reader.ReadBoolean();
                                case StreamTypes.Char16Type:
                                    return (char16_t)reader.ReadUInt16();
                                case StreamTypes.GuidType:
                                    return reader.ReadGuid();
                                case StreamTypes.StringType:
                                    return ReadString(reader);
                                case StreamTypes.StringToObjectMapType:
                                    return ReadStringToObjectMap(reader);
                                default:
                                    throw new InvalidArgumentException(""Unsupported property type"");
                            }
                        }

                    }
                }");

        }

        /// <summary>
        /// The convert_ class declaration.
        /// </summary>
        [TestMethod]
        public void ConvertClassDeclaration()
        {
            this.ConvertFile(@"
                namespace App1
                {
	                namespace Common
	                {
		                class SuspensionManager
		                {
		                }
	                }
                }");
        }

        /// <summary>
        /// The convert_ cpp header file.
        /// </summary>
        [TestMethod]
        public void ConvertCppHeaderFileSuspensionManager()
        {
            this.ConvertHeaderToCsDummay(@"
            //
            // SuspensionManager.h
            // Declaration of the SuspensionManager class
            //

            #pragma once

            #include <ppltasks.h>

            namespace App1
            {
	            namespace Common
	            {
		            /// <summary>
		            /// SuspensionManager captures global session state to simplify process lifetime management
		            /// for an application.  Note that session state will be automatically cleared under a variety
		            /// of conditions and should only be used to store information that would be convenient to
		            /// carry across sessions, but that should be disacarded when an application crashes or is
		            /// upgraded.
		            /// </summary>
		            ref class SuspensionManager sealed
		            {
		            internal:
			            static void RegisterFrame(Windows::UI::Xaml::Controls::Frame^ frame, Platform::String^ sessionStateKey);
			            static void UnregisterFrame(Windows::UI::Xaml::Controls::Frame^ frame);
			            static Concurrency::task<void> SaveAsync(void);
			            static Concurrency::task<void> RestoreAsync(void);
			            static property Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ SessionState
			            {
				            Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ get(void);
			            };
			            static Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ SessionStateForFrame(
				            Windows::UI::Xaml::Controls::Frame^ frame);

		            private:
			            static void RestoreFrameNavigationState(Windows::UI::Xaml::Controls::Frame^ frame);
			            static void SaveFrameNavigationState(Windows::UI::Xaml::Controls::Frame^ frame);
		            };
	            }
            }");
        }

        /// <summary>
        /// The convert to cpp header file.
        /// </summary>
        [TestMethod]
        public void ConvertCppHeaderFileGroupDetailPageGenHeader()
        {
            this.ConvertHeaderToCsDummay(@"
                #pragma once
                //------------------------------------------------------------------------------
                //     This code was generated by a tool.
                //
                //     Changes to this file may cause incorrect behavior and will be lost if
                //     the code is regenerated.
                //------------------------------------------------------------------------------

                namespace Windows {
                    namespace UI {
                        namespace Xaml {
                            namespace Data {
                                ref class CollectionViewSource;
                            }
                        }
                    }
                }
                namespace Windows {
                    namespace UI {
                        namespace Xaml {
                            namespace Controls {
                                ref class GridView;
                                ref class ListView;
                                ref class Button;
                                ref class TextBlock;
                            }
                        }
                    }
                }
                namespace Windows {
                    namespace UI {
                        namespace Xaml {
                            ref class VisualStateGroup;
                            ref class VisualState;
                        }
                    }
                }

                namespace App1
                {
                    partial ref class GroupDetailPage : public ::App1::Common::LayoutAwarePage, 
                        public ::Windows::UI::Xaml::Markup::IComponentConnector
                    {
                    public:
                        void InitializeComponent();
                        virtual void Connect(int connectionId, ::Platform::Object^ target);
    
                    private:
                        bool _contentLoaded;
    
                        private: ::Windows::UI::Xaml::Data::CollectionViewSource^ itemsViewSource;
                        private: ::Windows::UI::Xaml::Controls::GridView^ itemGridView;
                        private: ::Windows::UI::Xaml::Controls::ListView^ itemListView;
                        private: ::Windows::UI::Xaml::Controls::Button^ backButton;
                        private: ::Windows::UI::Xaml::Controls::TextBlock^ pageTitle;
                        private: ::Windows::UI::Xaml::VisualStateGroup^ ApplicationViewStates;
                        private: ::Windows::UI::Xaml::VisualState^ FullScreenLandscape;
                        private: ::Windows::UI::Xaml::VisualState^ Filled;
                        private: ::Windows::UI::Xaml::VisualState^ FullScreenPortrait;
                        private: ::Windows::UI::Xaml::VisualState^ Snapped;
                    };
                }");
        }

        [TestMethod]
        public void ConvertCppHeaderFileLayoutAwarePage()
        {
            this.ConvertHeaderToCsDummay(@"
                #pragma once

                #include <collection.h>

                namespace App1
                {
	                namespace Common
	                {
		                /// <summary>
		                /// Typical implementation of Page that provides several important conveniences:
		                /// <list type=""bullet"">
		                /// <item>
		                /// <description>Application view state to visual state mapping</description>
		                /// </item>
		                /// <item>
		                /// <description>GoBack, GoForward, and GoHome event handlers</description>
		                /// </item>
		                /// <item>
		                /// <description>Mouse and keyboard shortcuts for navigation</description>
		                /// </item>
		                /// <item>
		                /// <description>State management for navigation and process lifetime management</description>
		                /// </item>
		                /// <item>
		                /// <description>A default view model</description>
		                /// </item>
		                /// </list>
		                /// </summary>
		                [Windows::Foundation::Metadata::WebHostHidden]
		                public ref class LayoutAwarePage : Windows::UI::Xaml::Controls::Page
		                {
		                internal:
			                LayoutAwarePage();

		                public:
			                void StartLayoutUpdates(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                void StopLayoutUpdates(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                void InvalidateVisualState();
			                static property Windows::UI::Xaml::DependencyProperty^ DefaultViewModelProperty
			                {
				                Windows::UI::Xaml::DependencyProperty^ get();
			                };
			                property Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ DefaultViewModel
			                {
				                Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ get();
				                void set(Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ value);
			                }

		                protected:
			                virtual void GoHome(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                virtual void GoBack(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                virtual void GoForward(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                virtual Platform::String^ DetermineVisualState(Windows::UI::ViewManagement::ApplicationViewState viewState);
			                virtual void OnNavigatedTo(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e) override;
			                virtual void OnNavigatedFrom(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e) override;
			                virtual void LoadState(Platform::Object^ navigationParameter,
				                Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ pageState);
			                virtual void SaveState(Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ pageState);

		                private:
			                Platform::String^ _pageKey;
			                bool _navigationShortcutsRegistered;
			                Platform::Collections::Map<Platform::String^, Platform::Object^>^ _defaultViewModel;
			                Windows::Foundation::EventRegistrationToken _windowSizeEventToken,
				                _acceleratorKeyEventToken, _pointerPressedEventToken;
			                Platform::Collections::Vector<Windows::UI::Xaml::Controls::Control^>^ _layoutAwareControls;
			                void WindowSizeChanged(Platform::Object^ sender, Windows::UI::Core::WindowSizeChangedEventArgs^ e);
			                void OnLoaded(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                void OnUnloaded(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
			                void CoreDispatcher_AcceleratorKeyActivated(Windows::UI::Core::CoreDispatcher^ sender,
				                Windows::UI::Core::AcceleratorKeyEventArgs^ args);
			                void CoreWindow_PointerPressed(Windows::UI::Core::CoreWindow^ sender,
				                Windows::UI::Core::PointerEventArgs^ args);
			                LayoutAwarePage^ _this; // Strong reference to self, cleaned up in OnUnload
		                };
	                }
                }");
        }

        /// <summary>
        /// The convert_ cpp header file.
        /// </summary>
        [TestMethod]
        public void ConvertCppHeaderFileSampleDataSource()
        {
            this.ConvertHeaderToCsDummay(@"//
                // SampleDataSource.h
                // Declaration of the SampleDataSource, SampleDataGroup, SampleDataItem, and SampleDataCommon classes
                //

                #pragma once

                #include <collection.h>
                #include ""Common\BindableBase.h""

                // The data model defined by this file serves as a representative example of a strongly-typed
                // model that supports notification when members are added, removed, or modified.  The property
                // names chosen coincide with data bindings in the standard item templates.
                //
                // Applications may use this model as a starting point and build on it, or discard it entirely and
                // replace it with something appropriate to their needs.

                namespace App1
                {
	                namespace Data
	                {
		                ref class SampleDataGroup; // Resolve circular relationship between SampleDataItem and SampleDataGroup

		                /// <summary>
		                /// Base class for <see cref=""SampleDataItem""/> and <see cref=""SampleDataGroup""/> that
		                /// defines properties common to both.
		                /// </summary>
		                [Windows::Foundation::Metadata::WebHostHidden]
		                [Windows::UI::Xaml::Data::Bindable]
		                public ref class SampleDataCommon : App1::Common::BindableBase
		                {
		                internal:
			                SampleDataCommon(Platform::String^ uniqueId, Platform::String^ title, Platform::String^ subtitle, Platform::String^ imagePath,
				                Platform::String^ description);

		                public:
			                void SetImage(Platform::String^ path);
			                virtual Platform::String^ GetStringRepresentation() override;
			                property Platform::String^ UniqueId { Platform::String^ get(); void set(Platform::String^ value); }
			                property Platform::String^ Title { Platform::String^ get(); void set(Platform::String^ value); }
			                property Platform::String^ Subtitle { Platform::String^ get(); void set(Platform::String^ value); }
			                property Windows::UI::Xaml::Media::ImageSource^ Image { Windows::UI::Xaml::Media::ImageSource^ get(); void set(Windows::UI::Xaml::Media::ImageSource^ value); }
			                property Platform::String^ Description { Platform::String^ get(); void set(Platform::String^ value); }

		                private:
			                Platform::String^ _uniqueId;
			                Platform::String^ _title;
			                Platform::String^ _subtitle;
			                Windows::UI::Xaml::Media::ImageSource^ _image;
			                Platform::String^ _imagePath;
			                Platform::String^ _description;
		                };

		                /// <summary>
		                /// Generic item data model.
		                /// </summary>
		                [Windows::UI::Xaml::Data::Bindable]
		                public ref class SampleDataItem sealed : SampleDataCommon
		                {
		                public:
			                SampleDataItem(Platform::String^ uniqueId, Platform::String^ title, Platform::String^ subtitle, Platform::String^ imagePath,
				                Platform::String^ description, Platform::String^ content, SampleDataGroup^ group);

			                property Platform::String^ Content { Platform::String^ get(); void set(Platform::String^ value); }
			                property SampleDataGroup^ Group { SampleDataGroup^ get(); void set(SampleDataGroup^ value); }

		                private:
			                Platform::WeakReference _group; // Weak reference used to break reference counting cycle
			                Platform::String^ _content;
		                };

		                /// <summary>
		                /// Generic group data model.
		                /// </summary>
		                [Windows::UI::Xaml::Data::Bindable]
		                public ref class SampleDataGroup sealed : public SampleDataCommon
		                {
		                public:
			                SampleDataGroup(Platform::String^ uniqueId, Platform::String^ title, Platform::String^ subtitle, Platform::String^ imagePath,
				                Platform::String^ description);
			                property Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ Items
			                {
				                Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ get();
			                }
			                property Windows::Foundation::Collections::IVector<SampleDataItem^>^ TopItems
			                {
				                Windows::Foundation::Collections::IVector<SampleDataItem^>^ get();
			                }

		                private:
			                Platform::Collections::Vector<SampleDataItem^>^ _items;
			                Platform::Collections::Vector<SampleDataItem^>^ _topitems;
			                void ItemsCollectionChanged(Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ , Windows::Foundation::Collections::IVectorChangedEventArgs^ );
		                };

		                /// <summary>
		                /// Creates a collection of groups and items with hard-coded content.
		                /// 
		                /// SampleDataSource initializes with placeholder data rather than live production
		                /// data so that sample data is provided at both design-time and run-time.
		                /// </summary>
		                [Windows::UI::Xaml::Data::Bindable]
		                public ref class SampleDataSource sealed
		                {
		                public:			
			                SampleDataSource();
			                property Windows::Foundation::Collections::IObservableVector<SampleDataGroup^>^ AllGroups
			                {
				                Windows::Foundation::Collections::IObservableVector<SampleDataGroup^>^ get();
			                }
			                static Windows::Foundation::Collections::IIterable<SampleDataGroup^>^ GetGroups(Platform::String^ uniqueId);
			                static SampleDataGroup^ GetGroup(Platform::String^ uniqueId);
			                static SampleDataItem^ GetItem(Platform::String^ uniqueId);

		                private: 
			                static void Init();
			                Platform::Collections::Vector<SampleDataGroup^>^ _allGroups;
		                };
	                }
                }");
        }

        /// <summary>
        /// The property founder regex.
        /// </summary>
        [TestMethod]
        public void PropertyFounderRegex1()
        {
            var cpp = @"
			static property Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ SessionState
			{
				Windows::Foundation::Collections::IMap<Platform::String^, Platform::Object^>^ get(void);
			};

            static int Get();
            ";

            var match = CxxHeaderToCSharpDummyInterpreter.PropertyFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The property founder regex.
        /// </summary>
        [TestMethod]
        public void PropertyFounderRegex2()
        {
            var cpp = @"
			property Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ DefaultViewModel
			{
				Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ get();
				void set(Windows::Foundation::Collections::IObservableMap<Platform::String^, Platform::Object^>^ value);
			}

            static int Get();
            ";

            var match = CxxHeaderToCSharpDummyInterpreter.PropertyFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The method founder regex.
        /// </summary>
        [TestMethod]
        public void MethodFounderRegex1()
        {
            var cpp = @"void ItemsCollectionChanged(Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ a , Windows::Foundation::Collections::IVectorChangedEventArgs^ );";
            var match = CxxHeaderToCSharpDummyInterpreter.MethodDeclarationFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The method founder regex.
        /// </summary>
        [TestMethod]
        public void MethodFounderRegex2()
        {
            var cpp = @"void ItemsCollectionChanged(Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ , Windows::Foundation::Collections::IVectorChangedEventArgs^ );";
            var match = CxxHeaderToCSharpDummyInterpreter.MethodDeclarationFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The method founder regex.
        /// </summary>
        [TestMethod]
        public void MethodFounderRegex3()
        {
            var cpp = @"		
            [Windows::UI::Xaml::Data::Bindable]
		    public ref class SampleDataGroup sealed : public SampleDataCommon
		    {
		    public:
			    SampleDataGroup(Platform::String^ uniqueId, Platform::String^ title, Platform::String^ subtitle, Platform::String^ imagePath,
				    Platform::String^ description);
			    property Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ Items
			    {
				    Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ get();
			    }
			    property Windows::Foundation::Collections::IVector<SampleDataItem^>^ TopItems
			    {
				    Windows::Foundation::Collections::IVector<SampleDataItem^>^ get();
			    }

		    private:
			    Platform::Collections::Vector<SampleDataItem^>^ _items;
			    Platform::Collections::Vector<SampleDataItem^>^ _topitems;
			    void ItemsCollectionChanged(Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ , Windows::Foundation::Collections::IVectorChangedEventArgs^ );
		    };";
            var match = CxxHeaderToCSharpDummyInterpreter.MethodDeclarationFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The method founder regex.
        /// </summary>
        [TestMethod]
        public void MethodParametersFounderRegex1()
        {
            var cpp = @"		
			    void ItemsCollectionChanged(Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ , Windows::Foundation::Collections::IVectorChangedEventArgs^ );";
            var match = CxxHeaderToCSharpDummyInterpreter.MethodParametersDeclarationFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The method founder regex.
        /// </summary>
        [TestMethod]
        public void MethodParametersFounderRegex2()
        {
            var cpp = @"		
			    void ItemsCollectionChanged(Windows::Foundation::Collections::IObservableVector<SampleDataItem^>^ , Windows::Foundation::Collections::IVectorChangedEventArgs^ , Windows::Foundation::Collections::IVectorChangedEventArgs^ c);";
            var match = CxxHeaderToCSharpDummyInterpreter.MethodParametersDeclarationFounder.Match(cpp);
            Assert.IsTrue(match.Success);
        }

        /// <summary>
        /// The property resolver test.
        /// </summary>
        [TestMethod]
        public void TypeResolverTest()
        {
            var len = Stopwatch.StartNew();

            var interpreter = this.GetInterpreter(@"
            #pragma include ""Common\SuspensionManager.h""

            using Platform;
            using App1.Common;

            namespace App1
            {
                /// <summary>
                /// Provides application-specific behavior to supplement the default Application class.
                /// </summary>
                sealed partial class App
                {   
                    private async void OnSuspending(object sender, SuspendingEventArgs e)
                    {
                        var deferral = e.SuspendingOperation.GetDeferral();
                        SuspensionManager.SaveAsync().then(() => deferral.Complete());
                    }
                }
            }");

            len.Stop();

            Assert.IsNotNull(interpreter);

            var document = interpreter.Document;

            var namespaceElement = document.DocumentContents.ChildCodeElements.First(x => x is Namespace);
            var codeElement = namespaceElement.ChildCodeElements.First(x => x is Class);
            var methodElement = codeElement.ChildCodeElements.First();
            var returnElement = (methodElement as Method).ChildStatements.Skip(1).First();

            Assert.IsNotNull(document);

            var baseGetHashCodeReturnType =
                new ExpressionReturnTypeResolver(interpreter).Resolve((returnElement as ExpressionStatement).Expression).ToList();

            Assert.IsTrue((baseGetHashCodeReturnType as IEnumerable).GetEnumerator().MoveNext());
        }

        #endregion
    }
}
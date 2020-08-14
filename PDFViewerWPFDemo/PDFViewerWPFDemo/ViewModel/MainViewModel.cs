using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using pdftron;
using pdftron.PDF;
using pdftron.PDF.Tools;

namespace PDFViewerWPFDemo.ViewModel
{
    class MainViewModel : BaseViewModel
    {

        // Required for AnyCPU implementation.
        private static PDFNetLoader loader = PDFNetLoader.Instance();

        TextSearch textSearch = new TextSearch();

        ToolManager _toolManager;
                
        public MainViewModel()
        {
            // Initilizes PDFNet
            PDFNet.Initialize();

            // Make sure to Terminate any processes
            Application.Current.SessionEnding += Current_SessionEnding;

            // Init all Commands
            CMDOpenDocument = new Relaycommand(OpenDocument);
            CMDNextPage = new Relaycommand(NextPage);
            CMDPreviousPage = new Relaycommand(PreviousPage);
            CMDAnottateText = new Relaycommand(AddTextSample);
            CMDFreeTextSample = new Relaycommand(AddFreeTextSample);
            CMDSelectText = new Relaycommand(SelectText);
            CMDExit = new Relaycommand(ExitApp);
            CMDZoomIn = new Relaycommand(ZoomIn);
            CMDZoomOut = new Relaycommand(ZoomOut);
                       
            // Checks the scale factor to determine the right resolution
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double scaleFactor = 1;
            if (source != null)
            {
                scaleFactor = 1 / source.CompositionTarget.TransformFromDevice.M11;
            }

            // Set working doc to Viewer
            PDFViewer = new PDFViewWPF();
            PDFViewer.PixelsPerUnitWidth = scaleFactor;

            // PDF Viewer Events subscription
            PDFViewer.MouseLeftButtonDown += PDFView_MouseLeftButtonDown;

            // Enable access to the Tools available
            _toolManager = new ToolManager(PDFViewer);
        }

        /// <summary>
        /// On Left Mouse Button Down checks which tool is selected and anottate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PDFView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var workingDoc = PDFViewer.CurrentDocument;

            var clickPos = e.GetPosition(PDFViewer);

            Page page = workingDoc.GetPage(1);

            if (page == null) return;

        }          

        #region Public Properties

        PDFViewWPF _pDFView;
        public PDFViewWPF PDFViewer 
        { 
            get { return _pDFView; } 
            set 
            {
                _pDFView = value; 
                NotifyPropertyChanged();
            } 
        }

        public 

        string _windowTitle;
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; } }

        public bool ToolsEnabled { get { return PDFViewer.CurrentDocument == null ? false : true; } }

        #endregion

        #region Commands

        public ICommand CMDOpenDocument { get; set; }

        public ICommand CMDNextPage { get; set; }

        public ICommand CMDPreviousPage { get; set; }

        public ICommand CMDAnottateText { get; set; }

        public ICommand CMDFreeTextSample { get; set; }

        public ICommand CMDExit { get; set; }

        public ICommand CMDZoomIn { get; set; }

        public ICommand CMDZoomOut { get; set; }

        public ICommand CMDSelectText { get; set; }
        #endregion

        #region Operations

        // TODO: adjust it to do in increments of 10%
        private void ZoomIn() { PDFViewer.Zoom += 1; }
        private void ZoomOut() { PDFViewer.Zoom -= 1; }

        /// <summary>
        /// It open dialog to load a PDF File
        /// </summary>
        private void OpenDocument()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var doc = new PDFDoc(openFileDialog.FileName);
                doc.InitSecurityHandler();

                PDFViewer.CurrentDocument = doc;

                NotifyPropertyChanged(nameof(ToolsEnabled));
            }            
        }

        private void NextPage() { PDFViewer.GotoNextPage(); }
        private void PreviousPage() { PDFViewer.GotoPreviousPage(); }

        private void AddTextSample()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_sticky_note_create);

            PDFViewer.Update();            
        }

        private void AddFreeTextSample()
        {
            var workingDoc = PDFViewer.CurrentDocument;
            Page page = workingDoc.GetPage(PDFViewer.CurrentPageNumber);

            if (page == null) return;

            _toolManager.CreateTool(ToolManager.ToolType.e_text_annot_create);
        }

        private void SelectText()
        {
            _toolManager.CreateTool(ToolManager.ToolType.e_annot_edit);
        }

        private void CheckCanExcute(PDFViewWPF view) 
        { }

        private void ExitApp() 
        {
            Application.Current.Shutdown();
        }
                

        #endregion

        #region Events
        private void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            PDFNet.Terminate();
        }
        #endregion
    }
}

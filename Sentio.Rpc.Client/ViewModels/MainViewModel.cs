using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sentio.Rpc.Client.Helper;
using Sentio.Rpc.Controls;
using Sentio.Rpc.DataContracts;
using Sentio.Rpc.Enumerations;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Sentio.Rpc.Client.ViewModels;


public class MainViewModel : ObservableObject, ISentioRpcClient
{
    private SentioModules _activeModule;

    private List<ClientControl>? _clientUi;

    private string _hint = "Hello World";

    private ImageSource? _imageSource;

    private bool _isConnected;

    private bool _isInRemoteMode;

    private string _sentioVersion = "";

    private SentioSessionInfo _session;

    public MainViewModel()
    {
        CmdConnect = new AsyncRelayCommand(CmdConnectImplAsync);
        CmdDisconnect = new RelayCommand(DisconnectClient);
        CmdSelectModule = new AsyncRelayCommand<SentioModules>(SelectModuleImplAsync);
        CmdSwitchCamera = new AsyncRelayCommand<Camera>(SwitchCameraImplAsync);
        CmdStepFirstDie = new AsyncRelayCommand(StepFirstDieImplAsync);
        CmdStepNextDie = new AsyncRelayCommand(StepNextDieImplAsync);
        CmdGrabImage = new AsyncRelayCommand(GrabImageImplAsync);
        CmdShowHint = new AsyncRelayCommand(CmdShowHintImplAsync);
        CmdSetLight = new AsyncRelayCommand(SetModulePropertiesAsync);
        CmdListModuleProperties = new RelayCommand(ListModuleProperties);
    }

    public SentioModules ActiveModule
    {
        get => _activeModule;
        set => SetProperty(ref _activeModule, value);
    }

    public ICommand CmdConnect { get; }

    public ICommand CmdDisconnect { get; }

    public ICommand CmdGrabImage { get; }

    public ICommand CmdListModuleProperties { get; }

    public ICommand CmdSelectModule { get; }

    public ICommand CmdSetLight { get; }

    public ICommand CmdShowHint { get; }

    public ICommand CmdStepFirstDie { get; }

    public ICommand CmdStepNextDie { get; }

    public ICommand CmdSwitchCamera { get; }

    public string Hint
    {
        get => _hint;
        set => SetProperty(ref _hint, value);
    }

    public ImageSource? ImageSource
    {
        get => _imageSource;
        set => SetProperty(ref _imageSource, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public bool IsInRemoteMode
    {
        get => _isInRemoteMode;
        set
        {
            if (!SetProperty(ref _isInRemoteMode, value))
                return;

            Sentio?.SetIsInRemoteModeAsync(value);
        }
    }

    public IList<string> LogLines { get; } = new ObservableCollection<string>();

    public SentioProxyForClients? Sentio { get; protected set; }

    public string SentioVersion
    {
        get => _sentioVersion;
        set => SetProperty(ref _sentioVersion, value);
    }

    public SentioSessionInfo Session
    {
        get => _session;
        set => SetProperty(ref _session, value);
    }

    public bool ShowClientPanel
    {
        get
        {
            if (Sentio == null) return false;

            return Sentio.ShowClientPanel;
        }

        set
        {
            if (Sentio != null) Sentio.ShowClientPanel = value;

            OnPropertyChanged();
        }
    }

    public RemoteCommandResponse ExecuteExternalRemoteCommand(string cmd, string param)
    {
        Output($"Remote command request from SENTIO: {cmd} {param}");

        var resp = new RemoteCommandResponse
        {
            ErrorCode = 4, // 4 - Invalid Command
            Message = "Command not implemented",
            StatusCode = 0
        };
        return resp;
    }

    public void NotifyActiveModuleChanged(SentioModules module)
    {
        ActiveModule = module;
    }

    public void NotifyActiveStageChanged(Stage stage)
    {
        ActiveStage = stage;
        Output($"NotifyActiveStageChanged({stage})");
    }

    public async void NotifyButtonPressed(string btnId)
    {
        // try/catch is mandatory here!
        try
        {
            if (_clientUi is { Count: > 0 })
            {
                var btn = _clientUi.Find(i => i.Id.Equals(btnId)) as ClientButton;

                if (btn?.OnClickAsync != null)
                    await btn.OnClickAsync();
            }
        }
        catch (Exception exc)
        {
            Output(exc.Message);
        }
    }

    public async void NotifyClientPanelTextBoxChange(string tbId, string text)
    {
        if (_clientUi is { Count: > 0 })
        {
            var tb = _clientUi.Find(i => i.Id.Equals(tbId)) as ClientTextBox;
            if (tb!=null)
                await tb.OnTextChanged(text);
        }
    }

    public void NotifyProjectLoad(string projectFile)
    {
        Output($"NotifyProjectLoad({projectFile})");
    }

    public void NotifyProjectSave(string projectFile)
    {
        Output($"NotifyProjectSave({projectFile})");
    }

    public void NotifyRemoteModeChanged(bool remoteModeEnabled)
    {
        IsInRemoteMode = remoteModeEnabled;
    }

    public void NotifySentioShutdown()
    {
        Output("NotifySentioShutdown()");
    }

    public void NotifySessionChanged(SentioSessionInfo session)
    {
        Output($"NotifySessionChanged(session.User={session.User}, session.AccessLevel={session.AccessLevel})");
    }

    public void NotifyStepToCellFinished(int col, int row, int n)
    {
        Output($"NotifyStepToCell({col}, {row}, {n})");
    }

    public void NotifyWafermapDoubleClick(int col, int row, int n)
    {
        Output($"NotifyWafermapDoubleClick({col}, {row}, {n})");
    }

    public async Task ConnectAsync(string serverName, string remotePrefix, CancellationToken ct)
    {
        try
        {
            Sentio = new SentioProxyForClients(this);

            await Sentio.ConnectAsync(serverName, "Json-RPC Demo Client", "rpc", SentioCompatibilityLevel.Sentio_23_2, ct);

            // Query the compatibility level supported by the SENTIO Server.
            var compatLevel = Sentio.CompatLevel;
            LogLines.Add($"Sentio Server is reporting compatibility level {compatLevel}");

            SentioVersion = Sentio.Version;
            LogLines.Add($"Sentio Server is reporting SENTIO version {SentioVersion}");

            ActiveModule = await Sentio.GetActiveModuleAsync(ct);
            LogLines.Add($"Active Module is {ActiveModule}");

            IsInRemoteMode = await Sentio.GetIsInRemoteModeAsync(ct);
            LogLines.Add($"Remote mode is {IsInRemoteMode}");

            Session = await Sentio.GetSessionInfoAsync(ct);
            LogLines.Add($"Sentio Session: UserName={Session.User}, AccessLevel={Session.AccessLevel}");

            ActiveCamera = await Sentio.GetActiveCameraAsync(ct);
            LogLines.Add($"Active camera is {ActiveCamera}");

            ActiveStage = await Sentio.GetActiveStageAsync(ct);
            LogLines.Add($"Active stage is {ActiveStage}");

            _clientUi = new List<ClientControl>
            {
                new ClientButton("btnRun", "Run", OnRunAsync),
                new ClientButton("btnClearBins", "Clear Bins", OnClearBinsAsync),
                new ClientButton("btnStepNext", "StepNextDie", OnStepNextDieAsync),
                new ClientTextBox("edName", "DUT", OnDutChangedAsync),
                new ClientIcon("icInfo", "Icon_OpenProject"),
                new ClientLabel("lbLog", "This is a label")
            };
            await Sentio.SetupClientPanelAsync(_clientUi);
            Sentio.ShowClientPanel = true;

            IsConnected = true;
        }
        catch (Exception exc)
        {
            IsConnected = false;
            Sentio = null;
            Output($"Error while connecting with SENTIO: {exc.Message}");
        }
        finally
        {
            this.DispatchToUiAsync(CommandManager.InvalidateRequerySuggested);
        }
    }

    public Stage ActiveStage
    {
        get => _activeStage;
        set => SetProperty(ref _activeStage, value);
    }

    public void DisconnectClient()
    {
        if (Sentio == null) return;

        try
        {
            Sentio.Disconnect();
            ClearOutput();
            Output("Disconnected from SENTIO!");
        }
        catch (Exception exc)
        {
            Output($"Disconnect failed: {exc.Message}");
        }
        finally
        {
            IsConnected = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void ClearOutput()
    {
        this.DispatchToUiAsync(LogLines.Clear);
    }

    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    private async Task CmdConnectImplAsync()
    {
        try
        {
            await ConnectAsync(ServerName, "rpc", CancellationToken.None);
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    private async Task CmdShowHintImplAsync()
    {
        try
        {
            if (Sentio != null)
                await Sentio.ShowHintAsync(Hint);
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    private async Task GrabImageImplAsync()
    {
        if (Sentio == null) return;

        try
        {
            Output($"Requesting {ActiveCamera} Camera image");
            
            var imageData = await Sentio.GrabCameraImageAsync(ActiveCamera, ImageFormat.Jpeg, DownSample.Original);
            if (imageData == null) 
                return;

            Output("Camera image transferred");

            var memoryStream = new MemoryStream(imageData);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = memoryStream;
            bmp.EndInit();

            Output("Image encoded");
            // Assign the Source property of your image
            ImageSource = bmp;
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    private void ListModuleProperties()
    {
        if (Sentio == null) return;

        try
        {
            LogLines.Clear();

            var visionProp = new Dictionary<string, string>
            {
                // Jpeg Quality in percent when saving images
                { "jpeg_quality", "" },

                // Camera Parameters, The parameter refers to the camera.
                // available cameras are: scope, offaxis, chuck, vce01, scope2
                { "image_size", "scope" },
                { "light", "scope" },
                { "gain", "scope" },
                { "gain_min", "scope" },
                { "gain_max", "scope" },
                { "exposure", "scope" },
                { "exposure_min", "scope" },
                { "exposure_max", "scope" },
                { "calib", "scope" },

                // size of the camera's region of interest in µm
                { "roi_size", "scope" }
            };

            foreach (var it in visionProp)
                try
                {
                    var propName = it.Key;
                    var propArg = new SentioVariantData(it.Value);

                    var prop = Sentio.GetModuleProperty(SentioModules.Vision, propName, propArg);
                    LogLines.Add($"{it.Key} - {prop} ({prop.Type})");
                }
                catch (Exception exc)
                {
                    // This may be related to the wcf underpinnings and have nothing to
                    // do with SENTIO but with the connection as a whole.
                    LogLines.Add(exc.Message);
                }
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    /// <summary>
    ///     Clear all wafermap bins by executing the clearallbins command from the wafermap command handler
    /// </summary>
    private async Task OnClearBinsAsync()
    {
        if (Sentio == null)
            return;

        try
        {
            await Sentio.ExecuteModuleCommandAsync(SentioModules.Wafermap, "ClearAllBins");
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    private async Task OnDutChangedAsync(string text)
    {
        try
        {
            await Task.CompletedTask;
            Output($"Dut Textbox updated: \"{text}\"");
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    private async Task OnRunAsync()
    {
        try
        {
            Output("OnRun Button pressed");
            await TestAllDiesAsync();
        }
        catch (Exception exc)
        {
            Output(exc.Message);
        }
    }

    private async Task OnStepNextDieAsync()
    {
        await StepNextDieImplAsync();
    }

    private void Output(string message)
    {
        this.DispatchToUiAsync(() => { LogLines.Add(message); });
    }

    private async Task SelectModuleImplAsync(SentioModules module)
    {
        try
        {
            if (Sentio == null) return;

            if (module == SentioModules.Vision)
                // Just an Example: You can also activate tab pages!
                await Sentio.SetActiveModuleAsync(module, "Automation");
            else
                await Sentio.SetActiveModuleAsync(module);
        }
        catch (Exception exc)
        {
            Output($"Error in SelectModuleImpl: {exc.Message}");
        }
    }

    private async Task SetModulePropertiesAsync()
    {
        if (Sentio == null) return;

        try
        {
            LogLines.Clear();
            LogLines.Add("The following examples demonstrate how to set module properties");

            LogLines.Add("  - Setting light to 100");
            await Sentio.SetModulePropertyAsync(SentioModules.Vision, "light",
                new SentioVariantData(Camera.Scope.ToString()), new SentioVariantData(100));

            LogLines.Add("  - Setting gain to 0.3");
            await Sentio.SetModulePropertyAsync(SentioModules.Vision, "gain",
                new SentioVariantData(Camera.Scope.ToString()), new SentioVariantData(0.3));

            LogLines.Add("  - Setting jpeg_quality to 100");
            await Sentio.SetModulePropertyAsync(SentioModules.Vision, "jpeg_quality", new SentioVariantData(100));

            LogLines.Add("Done");
        }
        catch (Exception exc)
        {
            LogLines.Add(exc.Message);
        }
    }

    private async Task StepFirstDieImplAsync()
    {
        if (Sentio == null) return;

        try
        {
            Output("sending map:step_first_die");

            // Step to first die in route, subsite 0
            var resp = await Sentio.ExecuteRemoteCommandAsync("map:step_first_die", 0);
            if (resp.ErrorCode != 0) Output(resp.Message);
        }
        catch (Exception exc)
        {
            Output(exc.Message);
        }
    }

    private async Task StepNextDieImplAsync()
    {
        if (Sentio == null)
        {
            await Task.CompletedTask;
            return;
        }

        try
        {
            Output("sending map:step_next_die");

            await Sentio.ShowHintAsync("Stepping to next die via RPC");

            // Step to first die in route, subsite 0
            var resp = await Sentio.ExecuteRemoteCommandAsync("map:step_next_die", null);
            if (resp.ErrorCode != 0) Output(resp.Message);
        }
        catch (Exception exc)
        {
            Output(exc.Message);
            await Sentio.ShowHintAsync(exc.Message, CancellationToken.None);
        }
    }

    private Camera _activeCamera = Camera.Scope;
    private Stage _activeStage = Stage.Unknown;
    private string _serverName = "192.168.0.52";

    public Camera ActiveCamera
    {
        get => _activeCamera;
        set => SetProperty(ref _activeCamera, value);
    }

    private async Task SwitchCameraImplAsync(Camera camera)
    {
        try
        {
            if (Sentio == null)
            {
                await Task.CompletedTask;
            }
            else
            {
                await Sentio.ShowHintAsync($"Switching to camera {camera}");
                await Sentio.ExecuteModuleCommandAsync(SentioModules.Vision, "SwitchToCamera", camera.ToString());
                ActiveCamera = camera;
            }
        }
        catch (Exception exc)
        {
            Output(exc.Message);
        }
    }

    private async Task TestAllDiesAsync()
    {
        if (Sentio == null)
        {
            await Task.CompletedTask;
            return;
        }

        try
        {
            await Sentio.SetIsInRemoteModeAsync(true);

            var resp = await Sentio.ExecuteRemoteCommandAsync("map:step_first_die", 0);
            if (resp.ErrorCode != 0) throw new Exception(resp.Message);

            var rnd = new Random();
            while (resp.ErrorCode == 0)
            {
                resp = await Sentio.ExecuteRemoteCommandAsync("map:bin_step_next_die", rnd.Next(0, 5));
                if (resp.ErrorCode != 0) throw new Exception(resp.Message);
            }
        }
        catch (Exception exc)
        {
            Output(exc.Message);
            await Sentio.ShowHintAsync(exc.Message);
        }
        finally
        {
            await Sentio.SetIsInRemoteModeAsync(false);
        }
    }
}
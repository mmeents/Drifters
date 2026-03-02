using Drifters.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DriftersApp.Models;
using Microsoft.Extensions.Logging;
using Drifters.Core.Handlers.DrifterEngine;
using Drifters.Core.Entities;

namespace DriftersApp {
  public partial class Form1 : Form {
    private readonly IServiceScopeFactory _scopeFactory;
    private DisplayMode _currentDisplayMode = DisplayMode.Initializing;
    private ILogger<Form1> _logger;
    private ViewportDetails _vd = new();
    private List<RunDto> _runs = new();
    private RunDto? _selectedRun = null;
    private List<RunTurnDto> _currentRunTurns = new();

    public Form1(IServiceScopeFactory scopeFactory) {
      _scopeFactory = scopeFactory;
      using var scope = _scopeFactory.CreateScope();
      _logger = scope.ServiceProvider.GetRequiredService<ILogger<Form1>>();
      InitializeComponent();
      HideControls();
    }

    private void HideControls() {
      checkBox1.Visible = false;
    }


    private void Form1_Shown(object sender, EventArgs e) {
      displayTimer.Enabled = true;      
      
    }

    private void Form1_ResizeEnd(object sender, EventArgs e) {
      Graphics g = this.CreateGraphics();
      try {
        _vd.fWidth = g.VisibleClipBounds.Width;
        _vd.fHeight = g.VisibleClipBounds.Height;
        _vd.f20Height = _vd.fHeight * 0.2f;
        _vd.f05Height = _vd.fHeight * 0.065f;
        _vd.f15Height = _vd.fHeight * 0.145f;
        _vd.f20Width = _vd.fWidth * 0.2f;
        _vd.f15Width = _vd.fWidth * 0.15f;
        _vd.f05Width = _vd.fWidth * 0.05f;
        _vd.OffsetA = g.MeasureString("  Quantity:", _vd.fCur10);
      } finally {
        g.Dispose();
      }
    }

    private void displayTimer_Tick(object sender, EventArgs e) {
      displayTimer.Stop();
      try {
        Task.Run(async () => await ReloadRunDataAsync());
        DoRedraw();

      } catch (Exception ex) {
        _logger.LogError(ex, "Error updating display");
        MessageBox.Show($"An error occurred while updating the display: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      } finally {
        displayTimer.Start(); // Restart the timer
      }

    }

    private bool _isReloadingData = false;
    private bool _isDataDirty = true;
    private async Task ReloadRunDataAsync() {
      if (_isReloadingData || !_isDataDirty) return;
        _isReloadingData = true;
        try {
          using var scope = _scopeFactory.CreateScope();
          var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
          _runs = await mediator.Send(new GetRunsQuery());
          if (_selectedRun != null) {
          _currentRunTurns = await mediator.Send(new GetRunTurnsQuery(_selectedRun.Id));
        } else { 
          if (_runs.Count > 0) {
            _selectedRun = _runs[0];
            _currentRunTurns = await mediator.Send(new GetRunTurnsQuery(_selectedRun.Id));
          }
        }
        _currentDisplayMode = DisplayMode.Ready;
        _isDataDirty = false;
      } catch (Exception ex) {
        _logger.LogError(ex, "Error reloading run data");
        MessageBox.Show($"An error occurred while reloading run data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      } finally {
        _isReloadingData = false;
      }
    }


    delegate void UpdateControlVisibilityCallback();
    private void DoUpdateControlVisibility() {
      if (this.InvokeRequired) {
        var d = new UpdateControlVisibilityCallback(DoUpdateControlVisibility);
        this.Invoke(d, new object[] { });
      } else {
        if (_currentDisplayMode == DisplayMode.Initializing) {
          if (checkBox1.Visible) checkBox1.Visible = false;
        } else {
          if (!checkBox1.Visible) checkBox1.Visible = true;
        }
      }
    }


    private void DoRedraw() {
      DoUpdateControlVisibility();
      Graphics g = this.CreateGraphics();
      try {
        string es = "100";
        BufferedGraphics bg = BufferedGraphicsManager.Current.Allocate(g, this.DisplayRectangle);
        try {
          Form1_ResizeEnd(null, null);
          bg.Graphics.Clear(Color.FromArgb(0, 0, 12));

          switch (_currentDisplayMode) {
            case DisplayMode.Initializing:          
              es = "200";
              var initText = "Initializing...";
              var textSize = bg.Graphics.MeasureString(initText, _vd.fCur12);
              var textX = (_vd.fWidth - textSize.Width) / 2;
              var textY = (_vd.fHeight - textSize.Height) / 2;
              bg.Graphics.DrawString(initText, _vd.fCur12, Brushes.White, textX, textY);
              break;
            case DisplayMode.Ready:
              es = "300";
              
              break;
            case DisplayMode.Running:
              es = "400";
              break;
            case DisplayMode.Stopping:
              es = "500";
              break;
            case DisplayMode.AddEditRun:
              es = "600";
              break;
            case DisplayMode.AddEditCharacter:
              es = "700";
              break;
            default:break;
          }



           bg.Render(g);
        } catch (Exception e) {
          Console.WriteLine($"Error at step {es}: " + e.Message);
        } finally {
          bg.Dispose();
        }
      } finally {
        g.Dispose();
      }


    }

  }
}

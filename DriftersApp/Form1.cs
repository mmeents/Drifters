using Drifters.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DriftersApp.Models;
using Microsoft.Extensions.Logging;
using Drifters.Core.Handlers.DrifterEngine;
using Drifters.Core.Entities;
using Drifters.Core.Handlers.DrifterApp;
using System.Text;

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
      tbOut.Visible = false;
      lbIterations.Visible = false;
      tbIterations.Visible = false;

      tbCharacterName.Visible = false;
      tbRank.Visible = false;
      tbSystemPrompt.Visible = false;
      tbObjectives.Visible = false;
      tbMotives.Visible = false;
      btnCancelChar.Visible = false;
      btnUpdateChar.Visible = false;
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

    private bool _isFirstDisplay = true;
    private void displayTimer_Tick(object sender, EventArgs e) {
      displayTimer.Stop();
      try {
        if (_isFirstDisplay) {
          _isDataDirty = true;
          Task.Run(async () => await ReloadRunDataAsync());
          _isFirstDisplay = false;
        }
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
    private int LastPrintedTurnId = 0;
    private int LastPrintedTickId = 0;
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
            _currentDisplayMode = DisplayMode.Ready;
          }
        }
        var lastTurn = _currentRunTurns.LastOrDefault();
        if (lastTurn == null) { return; }

        if (_currentDisplayMode == DisplayMode.Running) {
          foreach (var turnRun in _currentRunTurns) {
            if (turnRun.TurnId > LastPrintedTurnId && turnRun.TickId > LastPrintedTickId) {
              if (turnRun.TurnId < lastTurn!.TurnId && turnRun.TickId < lastTurn.TickId) {
                string promptdetails = turnRun.Prompt.Replace("You", "They");
                DoAppendMessage($"{promptdetails}");
                DoAppendMessage($"{turnRun.CharacterReasoning}");
                DoAppendMessage($"{turnRun.ToolCallResult}");
                LastPrintedTurnId = turnRun.TurnId;
                LastPrintedTickId = turnRun.TickId;
              }
            }
          }
        }

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
          
          switch (_currentDisplayMode) {
            case DisplayMode.Ready:
              checkBox1.Text = "Ready Mode";
              if (tbOut.Visible) tbOut.Visible = false;
              if (!checkBox1.Visible) checkBox1.Visible = true;
              if (!lbIterations.Visible) lbIterations.Visible = true;
              if (!tbIterations.Visible) tbIterations.Visible = true;

              if (tbCharacterName.Visible) tbCharacterName.Visible = false;
              if (tbRank.Visible) tbRank.Visible = false;
              if (tbSystemPrompt.Visible) tbSystemPrompt.Visible = false;
              if (tbObjectives.Visible) tbObjectives.Visible = false;
              if (tbMotives.Visible) tbMotives.Visible = false;
              if (btnCancelChar.Visible) btnCancelChar.Visible = false;
              if (btnUpdateChar.Visible) btnUpdateChar.Visible = false;
              break;
            case DisplayMode.Running:
              checkBox1.Text = "Running";
              if (!tbOut.Visible) tbOut.Visible = true;
              if (!checkBox1.Visible) checkBox1.Visible = true;
              if (lbIterations.Visible) lbIterations.Visible = false;
              if (tbIterations.Visible) tbIterations.Visible = false;

              if (tbCharacterName.Visible) tbCharacterName.Visible = false;
              if (tbRank.Visible) tbRank.Visible = false;
              if (tbSystemPrompt.Visible) tbSystemPrompt.Visible = false;
              if (tbObjectives.Visible) tbObjectives.Visible = false;
              if (tbMotives.Visible) tbMotives.Visible = false;
              if (btnCancelChar.Visible) btnCancelChar.Visible = false;
              if (btnUpdateChar.Visible) btnUpdateChar.Visible = false;
              break;
            case DisplayMode.Stopping:
              checkBox1.Text = "Stopping";
              if (!tbOut.Visible) tbOut.Visible = true;
              if (!checkBox1.Visible) checkBox1.Visible = true;
              if (lbIterations.Visible) lbIterations.Visible = false;
              if (tbIterations.Visible) tbIterations.Visible = false;

              if (tbCharacterName.Visible) tbCharacterName.Visible = false;
              if (tbRank.Visible) tbRank.Visible = false;
              if (tbSystemPrompt.Visible) tbSystemPrompt.Visible = false;
              if (tbObjectives.Visible) tbObjectives.Visible = false;
              if (tbMotives.Visible) tbMotives.Visible = false;
              if (btnCancelChar.Visible) btnCancelChar.Visible = false;
              if (btnUpdateChar.Visible) btnUpdateChar.Visible = false;
              break;
            case DisplayMode.AddEditRun:
              if (tbOut.Visible) tbOut.Visible = false;
              if (checkBox1.Visible) checkBox1.Visible = false;
              if (lbIterations.Visible) lbIterations.Visible = false;
              if (tbIterations.Visible) tbIterations.Visible = false;

              if (!tbCharacterName.Visible) tbCharacterName.Visible = true;  // plays 2 roles - character name for add/edit character, run name for add/edit run
              if (tbRank.Visible) tbRank.Visible = false;
              if (!tbSystemPrompt.Visible) tbSystemPrompt.Visible = true;  // plays 2 roles - system prompt for add/edit character, run InitialScenario for add/edit run
              if (tbObjectives.Visible) tbObjectives.Visible = false;
              if (tbMotives.Visible) tbMotives.Visible = false;
              if (!btnCancelChar.Visible) btnCancelChar.Visible = true;
              if (!btnUpdateChar.Visible) btnUpdateChar.Visible = true;
              break;
            case DisplayMode.AddEditCharacter:
              if (tbOut.Visible) tbOut.Visible = false;
              if (checkBox1.Visible) checkBox1.Visible = false;
              if (lbIterations.Visible) lbIterations.Visible = false;
              if (tbIterations.Visible) tbIterations.Visible = false;

              if (!tbCharacterName.Visible) tbCharacterName.Visible = true;
              if (!tbRank.Visible) tbRank.Visible = true;
              if (!tbSystemPrompt.Visible) tbSystemPrompt.Visible = true;
              if (!tbObjectives.Visible) tbObjectives.Visible = true;
              if (!tbMotives.Visible) tbMotives.Visible = true;
              if (!btnCancelChar.Visible) btnCancelChar.Visible = true;
              if (!btnUpdateChar.Visible) btnUpdateChar.Visible = true;
              break;
            default:
              break;
          }
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
              if ((_selectedRun?.Id ?? 0) != 0) {
                var runText = $"Selected Run: {_selectedRun.Name} (Turns: {_currentRunTurns.Count})";
                var runTextSize = bg.Graphics.MeasureString(runText, _vd.fCur10);
                bg.Graphics.DrawString(runText, _vd.fCur10, Brushes.White, _vd.f05Width, _vd.f05Height);
                var characterNumber = 1;
                foreach (var character in _selectedRun.Characters) {
                  var charText1 = $"Character: {character.Name}";
                  var charTextSize1 = bg.Graphics.MeasureString(charText1, _vd.fCur10);
                  bg.Graphics.DrawString(charText1, _vd.fCur10, Brushes.White, _vd.f05Width, _vd.f05Height + charTextSize1.Height * characterNumber);
                  characterNumber++;
                }
              }
              break;
            case DisplayMode.Running:
              es = "400";
              if ((_selectedRun?.Id ?? 0) != 0) {
                var runText = $"Selected Run: {_selectedRun.Name} (Turns: {_RunningCurrentTick}/{_RunningMaxTicks})";
                var runTextSize = bg.Graphics.MeasureString(runText, _vd.fCur10);
                bg.Graphics.DrawString(runText, _vd.fCur10, Brushes.White, _vd.f05Width, _vd.f05Height);               
              }
              break;
            case DisplayMode.Stopping:
              es = "500";
              break;
            case DisplayMode.AddEditRun:
              es = "600";

              break;
            case DisplayMode.AddEditCharacter:
              es = "700";
              string charText = "System Prompt: ";
              var charTextSize = bg.Graphics.MeasureString(charText, _vd.fCur12);
              var leftStart = tbCharacterName.Left - charTextSize.Width;
              
              bg.Graphics.DrawString("Editing: ", _vd.fCur12, Brushes.White, 
                leftStart, tbCharacterName.Top);
              bg.Graphics.DrawString("Rank: ", _vd.fCur12, Brushes.White, 
                leftStart, tbRank.Top);
              bg.Graphics.DrawString("System Prompt: ", _vd.fCur12, Brushes.White,
                leftStart, tbSystemPrompt.Top);
              bg.Graphics.DrawString("Objectives: ", _vd.fCur12, Brushes.White,
                leftStart, tbObjectives.Top);
              bg.Graphics.DrawString("Motives: ", _vd.fCur12, Brushes.White,
                leftStart, tbMotives.Top);
              break;
            default: break;
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

    private void Form1_MouseClick(object sender, MouseEventArgs e) {
      switch (_currentDisplayMode) {
        case DisplayMode.Ready:
          var TextHeight = _vd.fCur10.Height + 4;
          if (_selectedRun != null && _selectedRun.Characters.Count > 0) {
            if (e.X > _vd.f05Width && e.X < _vd.f05Width + 200 && e.Y > _vd.f05Height && e.Y < _vd.f05Height + (_selectedRun.Characters.Count + 1) * TextHeight) {
              int characterIndex = (int)((e.Y - _vd.f05Height) / TextHeight) - 1;
              if (characterIndex >= 0 && characterIndex < _selectedRun.Characters.Count) {
                var selectedCharacter = _selectedRun.Characters.ElementAt(characterIndex);
                ResetCharacterDetails(selectedCharacter);
                _currentDisplayMode = DisplayMode.AddEditCharacter;
              }
            }
          }
          break;
        default: break;
      }
    }

    private int _currentCharacterId = 0;
    private void ResetCharacterDetails(CharacterDto character) {
      _currentCharacterId = character.Id;
      tbCharacterName.Text = character.Name;
      tbSystemPrompt.Text = character.SystemPrompt;
      tbObjectives.Text = character.Objectives;
      tbMotives.Text = character.Motives;
      tbRank.Text = character.Rank.ToString();
    }

    private void btnUpdateChar_Click(object sender, EventArgs e) {
      if (_currentCharacterId == 0) {
        MessageBox.Show("No character selected. Please select a character before updating.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      if (_currentDisplayMode != DisplayMode.AddEditCharacter) {
        MessageBox.Show("Not in character edit mode. Please select a character and click edit before updating.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      using var scope = _scopeFactory.CreateScope();
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
      var updateCommand = new UpdateCharacterCommand(
        _currentCharacterId,
        tbCharacterName.Text,        
        tbSystemPrompt.Text,
        tbObjectives.Text,
        tbMotives.Text,
        int.TryParse(tbRank.Text, out int rank) ? rank : 0
      );
      _currentDisplayMode = DisplayMode.Running;

    }

    delegate void AppendLogMessageCallback(string message);
    private void DoAppendMessage(string message) {
      if (this.InvokeRequired) {
        var d = new AppendLogMessageCallback(DoAppendMessage);
        this.Invoke(d, new object[] { message });
      } else {
        StringBuilder sb = new StringBuilder(tbOut.Text);
        sb.AppendLine(message);
        tbOut.Text = sb.ToString();
        tbOut.SelectionStart = tbOut.Text.Length;
        tbOut.ScrollToCaret();
      }
    }

    private bool _isRunningUpdateInProgress = false;    
    private int _RunningMaxTicks = 0;
    private int _RunningCurrentTick = 0;
    private async void RunTimer_Tick(object sender, EventArgs e) {
      RunTimer.Stop();
      if (_isRunningUpdateInProgress) return;
      if (_selectedRun == null) {
        MessageBox.Show("No run selected. Please select a run before starting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      try {
        _isRunningUpdateInProgress = true;
        _RunningMaxTicks = Convert.ToInt32(tbIterations.Value);
        _RunningCurrentTick = 1;
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        bool success = true;        
        while (_RunningCurrentTick <= _RunningMaxTicks && _currentDisplayMode == DisplayMode.Running && success) {
          var tickCommand = new ExecuteNextTickCommand(_selectedRun!.Id);
          success = await mediator.Send(tickCommand);
          if (success) {
            _isDataDirty = true;  
            await ReloadRunDataAsync();
          }
          _RunningCurrentTick++;
        }
        if (_currentDisplayMode == DisplayMode.Running) {
          _currentDisplayMode = DisplayMode.Stopping;
        }
      } catch (Exception ex) {
        _logger.LogError(ex, "Error during running update");
        MessageBox.Show($"An error occurred during running update: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      } finally {
        _isRunningUpdateInProgress = false;
      }

    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e) {

      switch (_currentDisplayMode) {
        case DisplayMode.Ready:
          if (checkBox1.Checked) {
            _currentDisplayMode = DisplayMode.Running;
            RunTimer.Enabled = true;
          }
          break;
        case DisplayMode.Running:
          if (checkBox1.Checked) {
            _currentDisplayMode = DisplayMode.Running;
          } else {
            _currentDisplayMode = DisplayMode.Stopping;
            RunTimer.Enabled = false;
          }
          break;
        case DisplayMode.Stopping:
          if (checkBox1.Checked) {
            _currentDisplayMode = DisplayMode.Running;
            RunTimer.Enabled = true;
          } else { 
            _currentDisplayMode = DisplayMode.Ready;
            RunTimer.Enabled = false;
          }
          break;
        default: break;
      }

    }

    private void btnCancelChar_Click(object sender, EventArgs e) {
      _currentDisplayMode = DisplayMode.Ready;
    }


  }
}

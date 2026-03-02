using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriftersApp.Models {
  public enum DisplayMode {
    Initializing,  
    Ready,      // db up add can edit run/characters. can run if has run and characters not started.
    Running,    //  can request stop.
    Stopping,   // stop was requested but run is not fully stopped yet returns to ready.
    AddEditRun, // can add or edit run.
    AddEditCharacter // can add or edit character.
  }
}

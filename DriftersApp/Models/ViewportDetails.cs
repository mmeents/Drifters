using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriftersApp.Models {
  public class ViewportDetails {    
    public float fWidth { get; set; }
    public float fHeight { get; set; }
    public float f20Height { get; set; }
    public float f05Height { get; set; }
    public float f15Height { get; set; }
    public float f20Width { get; set; }
    public float f15Width { get; set; }
    public float f05Width { get; set; }
    public SizeF OffsetA { get; set; } = new SizeF();

    public Font fCur12 = new Font("Courier New", 12);
    public Font fCur10 = new Font("Courier New", 10);
    public int f01Width {
      get {
        return (int)(fWidth * 0.01f);
      }
    }
    public int HalfWitdth {
      get {
        return (int)(fWidth / 2);
      }
    }
    public int HalfHeight {
      get {
        return (int)(fHeight / 2);
      }
    }
  }
}

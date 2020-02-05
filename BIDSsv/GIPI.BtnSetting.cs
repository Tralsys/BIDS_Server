using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
  public struct BtnAssign
  {
    public uint Num;
    public int Job;
  }
  static public class GIPI
  {
    static public List<BtnAssign> BtnAssignList { get; private set; } = new List<BtnAssign>();

    static public bool LoadFromFile(string FName) => LoadFromStream(new StreamReader(FName));
    static public bool LoadFromStream(StreamReader s)
    {
      using(StreamReader f = s)
        return LoadFromSArr(f?.ReadToEnd().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries), ",");
    }

    static public bool LoadFromSArr(List<string> sa, string separator) => LoadFromSArr(sa?.ToArray(), separator);

    static public bool LoadFromSArr(string[] sa, string separator)
    {
      if (sa != null && sa.Length != 0 && string.IsNullOrEmpty(separator)) return false;

      List<BtnAssign> ba = new List<BtnAssign>();
      string s = string.Empty;
      string[] ssa;
      uint n;
      int j;
      for(int i = 0; i < sa.Length; i++)
      {
        ssa = sa[i].Split(new string[] { separator, " " }, StringSplitOptions.RemoveEmptyEntries);
        try
        {
          n = uint.Parse(ssa[0]);
          j = int.Parse(ssa[1]);
        }catch(Exception e)
        {
          Console.WriteLine(e);
          return false;
        }
        ba.Add(new BtnAssign() { Num = n, Job = j });
      }

      return true;
    }

    static public bool SetTable(List<BtnAssign> ba, bool init = true)
    {
      if (init) BtnAssignList.Clear();
              
      BtnAssignList.InsertRange(init ? 0 : BtnAssignList.Count, ba);
      return true;
    }
    static public bool SetTable(BtnAssign[] ba, bool init = true)
    {
      if (init) BtnAssignList.Clear();

      BtnAssignList.InsertRange(init ? 0 : BtnAssignList.Count, ba);
      return true;
    }

    static public int[] GetBtJobNum(uint Num)
    {
      if (!(BtnAssignList?.Count > 0)) return null;
      List<int> jbl = new List<int>();

      Parallel.For(0, BtnAssignList.Count, (i) =>
      {
        if (BtnAssignList[i].Num == Num) jbl.Add(BtnAssignList[i].Job);
      });

      return jbl.Count > 0 ? jbl.ToArray() : null;
    }
  }
}

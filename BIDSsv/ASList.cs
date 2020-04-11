using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
  internal class ASList
  {
    public int Count { get => SvList.Count; }
    public List<IBIDSsv> SvList { get; }
    public List<int> DNums { get; }
    public List<char> DTypes { get; }
    public ASList(bool UseTypeChar = true)
    {
      SvList = new List<IBIDSsv>();
      DNums = new List<int>();
      DTypes = UseTypeChar ? new List<char>() : null;
    }

    //public bool Contains(KeyValuePair<IBIDSsv, int> k) => Contains(k.Key, k.Value);
    public bool Contains(IBIDSsv key, int dnum, char dtyp = '\0')
    {
      if (key == null || !((SvList?.Count ?? 0) > 0)) return false;
      bool result = false;
      Parallel.For(0, SvList.Count, (i) =>
      {
        if (SvList[i] == key && (DTypes == null ? true : DTypes[i] == dtyp) && DNums[i] == dnum) result = true;
      });
      return result;
    }

    //public void Remove(KeyValuePair<IBIDSsv, int> k) => Remove(k.Key, k.Value);
    public void Remove(IBIDSsv key, int? dnum = null, char? dtyp = null)
    {
      if (!((SvList?.Count ?? 0) > 0)) return;
      for (int i = SvList.Count - 1; i >= 0; i--)
      {
        if (SvList[i] == key)
        {
          if (dtyp != null && dtyp == DTypes[i])//type is specified and it is same
            if (!(dnum == null || DNums[i] == dnum)) continue;//not match (type same / num is specified but not same)
          if (dtyp == null && dnum != DNums[i]) continue;//type not specified and num is not same
          SvList.RemoveAt(i);
          DNums.RemoveAt(i);
          DTypes?.RemoveAt(i);
        }
      }
    }

    public void Add(IBIDSsv key, int dnum, char dtyp = '\0')
    {
      SvList.Add(key);
      DTypes?.Add(dtyp);
      DNums.Add(dnum);
    }


  }
}

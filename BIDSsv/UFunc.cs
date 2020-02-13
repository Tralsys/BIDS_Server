using System;
using System.Collections.Generic;
using System.Linq;

namespace TR.BIDSsv
{
  static public class UFunc
  {
    public static string Comp(object oldobj, object newobj) => Equals(oldobj, newobj) ? string.Empty : newobj.ToString();

    private static readonly bool isLE = BitConverter.IsLittleEndian;

    public static byte[] ToBytes(this in int arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in float arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in double arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in bool arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in uint arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in short arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in ushort arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in long arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in ulong arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }
    public static byte[] ToBytes(this in char arg)
    {
      byte[] ba = BitConverter.GetBytes(arg);
      if (isLE) Array.Reverse(ba);
      return ba;
    }

    public static int GetInt(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(int);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToInt32(ba, 0);
    }
    public static float GetFloat(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(float);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToSingle(ba, 0);
    }
    public static double GetDouble(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(double);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToDouble(ba, 0);
    }
    public static bool GetBool(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(bool);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToBoolean(ba, 0);
    }
    public static uint GetUInt(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(uint);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToUInt32(ba, 0);
    }
    public static short GetShort(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(short);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToInt16(ba, 0);
    }
    public static ushort GetUShort(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(ushort);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToUInt16(ba, 0);
    }
    public static long GetLong(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(long);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToInt64(ba, 0);
    }
    public static ulong GetULong(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(long);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToUInt64(ba, 0);
    }
    public static char GetChar(this byte[] ab, int ind = 0)
    {
      int sz = sizeof(char);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, ind, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToChar(ba, 0);
    }

    public static byte[] GetBytes(this in int arg) => ToBytes(arg);
    public static byte[] GetBytes(this in uint arg) => ToBytes(arg);
    public static byte[] GetBytes(this in float arg) => ToBytes(arg);
    public static byte[] GetBytes(this in double arg) => ToBytes(arg);
    public static byte[] GetBytes(this in bool arg) => ToBytes(arg);
    public static byte[] GetBytes(this in short arg) => ToBytes(arg);
    public static byte[] GetBytes(this in ushort arg) => ToBytes(arg);
    public static byte[] GetBytes(this in long arg) => ToBytes(arg);
    public static byte[] GetBytes(this in ulong arg) => ToBytes(arg);
    public static byte[] GetBytes(this in char arg) => ToBytes(arg);

    public static int ToInt32(this byte[] ab, int ind = 0) => GetInt(ab, ind);
    public static float ToSingle(this byte[] ab, int ind = 0) => GetFloat(ab, ind);
    public static double ToDouble(this byte[] ab, int ind = 0) => GetDouble(ab, ind);
    public static bool ToBoolean(this byte[] ab, int ind = 0) => GetBool(ab, ind);
    public static uint ToUInt32(this byte[] ab, int ind = 0) => GetUInt(ab, ind);
    public static short ToInt16(this byte[] ab, int ind = 0) => GetShort(ab, ind);
    public static ushort ToUInt16(this byte[] ab, int ind = 0) => GetUShort(ab, ind);
    public static long ToInt64(this byte[] ab, int ind = 0) => GetLong(ab, ind);
    public static ulong ToUInt64(this byte[] ab, int ind = 0) => GetULong(ab, ind);
    public static char ToChar(this byte[] ab, int ind = 0) => GetChar(ab, ind);

    public static int ToInt32(IEnumerable<byte> ab, int ind = 0) => GetInt(ab.ToArray(), ind);
    public static float ToSingle(IEnumerable<byte> ab, int ind = 0) => GetFloat(ab.ToArray(), ind);
    public static double ToDouble(IEnumerable<byte> ab, int ind = 0) => GetDouble(ab.ToArray(), ind);
    public static bool ToBoolean(IEnumerable<byte> ab, int ind = 0) => GetBool(ab.ToArray(), ind);
    public static uint ToUInt32(IEnumerable<byte> ab, int ind = 0) => GetUInt(ab.ToArray(), ind);
    public static short ToInt16(IEnumerable<byte> ab, int ind = 0) => GetShort(ab.ToArray(), ind);
    public static ushort ToUInt16(IEnumerable<byte> ab, int ind = 0) => GetUShort(ab.ToArray(), ind);
    public static long ToInt64(IEnumerable<byte> ab, int ind = 0) => GetLong(ab.ToArray(), ind);
    public static ulong ToUInt64(IEnumerable<byte> ab, int ind = 0) => GetULong(ab.ToArray(), ind);
    public static char ToChar(IEnumerable<byte> ab, int ind = 0) => GetChar(ab.ToArray(), ind);

  }
}

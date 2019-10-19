﻿using System;

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

    public static int GetInt(this byte[] ab)
    {
      int sz = sizeof(int);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToInt32(ab, 0);
    }
    public static float GetFloat(this byte[] ab)
    {
      int sz = sizeof(float);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToSingle(ab, 0);
    }
    public static double GetDouble(this byte[] ab)
    {
      int sz = sizeof(double);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToDouble(ab, 0);
    }
    public static bool GetBool(this byte[] ab)
    {
      int sz = sizeof(bool);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToBoolean(ab, 0);
    }
    public static uint GetUInt(this byte[] ab)
    {
      int sz = sizeof(uint);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToUInt32(ab, 0);
    }
    public static short GetShort(this byte[] ab)
    {
      int sz = sizeof(short);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToInt16(ab, 0);
    }
    public static ushort GetUShort(this byte[] ab)
    {
      int sz = sizeof(ushort);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToUInt16(ab, 0);
    }
    public static long GetLong(this byte[] ab)
    {
      int sz = sizeof(long);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToInt64(ab, 0);
    }
    public static ulong GetULong(this byte[] ab)
    {
      int sz = sizeof(long);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToUInt64(ab, 0);
    }
    public static char GetChar(this byte[] ab)
    {
      int sz = sizeof(char);

      byte[] ba = new byte[sz];
      int i = sz - ab.Length;
      Array.Copy(ab, 0, ba, i < 0 ? 0 : i, sz);
      if (isLE) Array.Reverse(ba);

      return BitConverter.ToChar(ab, 0);
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

    public static int ToInt32(this byte[] ab) => GetInt(ab);
    public static float ToSingle(this byte[] ab) => GetFloat(ab);
    public static double ToDouble(this byte[] ab) => GetDouble(ab);
    public static bool ToBoolean(this byte[] ab) => GetBool(ab);
    public static uint ToUInt32(this byte[] ab) => GetUInt(ab);
    public static short ToInt16(this byte[] ab) => GetShort(ab);
    public static ushort ToUInt16(this byte[] ab) => GetUShort(ab);
    public static long ToInt64(this byte[] ab) => GetLong(ab);
    public static ulong ToUInt64(this byte[] ab) => GetULong(ab);
    public static char ToChar(this byte[] ab) => GetChar(ab);
  }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace FBI.Utils
{
  public class Hash
  {
    public static string GetMD5()
    {
      FileStream stream = new FileStream(Process.GetCurrentProcess().MainModule.FileName, FileMode.Open, FileAccess.Read);
      string MD5 = GetMD5(stream);

      stream.Close();
      return (MD5);
    }

    public static string GetMD5(FileStream p_stream)
    {
      MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

      md5.ComputeHash(p_stream);

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < md5.Hash.Length; i++)
        sb.Append(md5.Hash[i].ToString("x2"));

      return sb.ToString().ToUpperInvariant();
    }

    public static string GetSHA1(string p_stringToHash)
    {
      SHA1CryptoServiceProvider sha1Obj = new SHA1CryptoServiceProvider();
      byte[] bytesToHash = System.Text.Encoding.ASCII.GetBytes(p_stringToHash);

      bytesToHash = sha1Obj.ComputeHash(bytesToHash);

      string strResult = "";

      foreach (byte b in bytesToHash)
      {
        strResult += b.ToString("x2");
      }
      return strResult;
    }
  }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBI.MVC.Model
{
  using CRUD;
  using Network;
  using Utils;

  public class NamedCRUDModel<T> : ICRUDModel<T> where T : class, NamedCRUDEntity
  {
    protected MultiIndexDictionary<UInt32, string, T> m_CRUDDic = new MultiIndexDictionary<UInt32, string, T>();
    bool m_caseSensitive;

    protected NamedCRUDModel(bool p_caseSensitiveName = false, NetworkManager p_netMgr = null) 
      : base(p_netMgr)
    {
      m_caseSensitive = p_caseSensitiveName;
    }

    string GetName(string p_name)
    {
      if (m_caseSensitive)
        return (p_name);
      return (StringUtils.RemoveDiacritics(p_name));
    }

    #region "CRUD"

    protected override void ListAnswer(ByteBuffer p_packet)
    {
      if (p_packet.GetError() == ErrorMessage.SUCCESS)
      {
        m_CRUDDic.Clear();
        Int32 nb_accounts = p_packet.ReadInt32();
        for (Int32 i = 1; i <= nb_accounts; i++)
        {
          T tmp_crud = Build(p_packet) as T;

          m_CRUDDic.Set(tmp_crud.Id, GetName(tmp_crud.Name), tmp_crud);
        }

        IsInit = true;
        RaiseObjectInitializedEvent(p_packet.GetError(), typeof(T));
      }
      else
      {
        IsInit = false;
        RaiseObjectInitializedEvent(p_packet.GetError(), typeof(T));
      }
    }

    protected override void ReadAnswer(ByteBuffer p_packet)
    {
      if (p_packet.GetError() == ErrorMessage.SUCCESS)
      {
        T tmp_crud = Build(p_packet) as T;

        m_CRUDDic.Set(tmp_crud.Id, GetName(tmp_crud.Name), tmp_crud);
        RaiseReadEvent(p_packet.GetError(), tmp_crud);
      }
      else
      {
        RaiseReadEvent(p_packet.GetError(), null);
      }
    }

    protected override void DeleteAnswer(ByteBuffer p_packet)
    {
      UInt32 id = p_packet.ReadUint32();
      if (p_packet.GetError() == ErrorMessage.SUCCESS)
        m_CRUDDic.Remove(id);
      RaiseDeleteEvent(p_packet.GetError(), id);
    }

    #endregion

    #region "Mapping"

    public UInt32 GetValueId(string p_name)
    {

      if (m_CRUDDic[GetName(p_name)] == null)
        return 0;
      return m_CRUDDic[GetName(p_name)].Id;
    }

    public string GetValueName(UInt32 p_id)
    {
      if (m_CRUDDic[p_id] == null)
        return "";
      return m_CRUDDic[p_id].Name;
    }

    public T GetValue(string p_name)
    {
      if (p_name == null)
        return null;
      return m_CRUDDic[GetName(p_name)];
    }

    public override T GetValue(UInt32 p_id)
    {
      return m_CRUDDic[p_id];
    }

    public MultiIndexDictionary<UInt32, string, T> GetDictionary()
    {
      return m_CRUDDic;
    }

    public List<string> GetMatchings(string p_name)
    {
      List<string> l_results = new List<string>();

      if (p_name.Length == 0)
        return (l_results);
      string l_name = StringUtils.RemoveDiacritics(p_name);

      foreach (T l_entity in GetDictionary().SortedValues)
        if (String.Compare(StringUtils.RemoveDiacritics(l_entity.Name), 0, l_name, 0, l_name.Length) == 0)
          l_results.Add(l_entity.Name);
      return (l_results);
    }

    #endregion
  }
}

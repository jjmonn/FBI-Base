﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBI.MVC.Model.CRUD
{
  using Network;

  public class GlobalFactVersion : BaseVersion, IComparable, NamedHierarchyCRUDEntity
  {
    public GlobalFactVersion() { }
    private GlobalFactVersion(UInt32 p_id)
    {
      Id = p_id;
    }

    public static GlobalFactVersion BuildGlobalFactVersion(ByteBuffer p_packet)
    {
      GlobalFactVersion l_version = new GlobalFactVersion(p_packet.ReadUint32());

      l_version.ParentId = p_packet.ReadUint32();
      l_version.Name = p_packet.ReadString();
      l_version.IsFolder = p_packet.ReadBool();
      l_version.ItemPosition = p_packet.ReadInt32();
      l_version.StartPeriod = p_packet.ReadUint32();
      l_version.NbPeriod = p_packet.ReadUint16();
      if (l_version.IsFolder == true)
          l_version.Image = 1;
      else
          l_version.Image = 0;

      return (l_version);
    }

    public void Dump(ByteBuffer p_packet, bool p_includeId)
    {
      if (p_includeId)
        p_packet.WriteUint32(Id);
      p_packet.WriteUint32(ParentId);
      p_packet.WriteString(Name);
      p_packet.WriteBool(IsFolder);
      p_packet.WriteInt32(ItemPosition);
      p_packet.WriteUint32(StartPeriod);
      p_packet.WriteUint16(NbPeriod);
    }

    public void CopyFrom(GlobalFactVersion p_model)
    {
      ParentId = p_model.ParentId;
      Name = p_model.Name;
      IsFolder = p_model.IsFolder;
      ItemPosition = p_model.ItemPosition;
      StartPeriod = p_model.StartPeriod;
      NbPeriod = p_model.NbPeriod;
      Image = p_model.Image;
    }

    public GlobalFactVersion Clone()
    {
      GlobalFactVersion l_clone = new GlobalFactVersion(Id);

      l_clone.CopyFrom(this);
      return (l_clone);
    }

    public override BaseVersion BaseClone()
    {
      return (Clone());
    }

    public int CompareTo(object p_obj)
    {
      if (p_obj == null)
        return 0;
      GlobalFactVersion l_cmp = p_obj as GlobalFactVersion;

      if (l_cmp == null)
        return 0;
      if (l_cmp.ItemPosition > ItemPosition)
        return -1;
      else
        return 1;
    }
  }
}

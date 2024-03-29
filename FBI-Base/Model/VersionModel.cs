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

  public class VersionModel : NamedCRUDModel<Version>
  {
    static VersionModel s_instance = null;
    public static VersionModel Instance 
    { get { return (s_instance == null) ? s_instance = new VersionModel() : s_instance; } }
    ClientMessage CopyCMSG;
    ServerMessage CopySMSG;
    public event CopyEventHandler CopyEvent;
    public delegate void CopyEventHandler(ErrorMessage p_status, UInt32 p_id);

    VersionModel() : base(false, NetworkManager.Instance)
    {
      Init();
    }

    public VersionModel(NetworkManager p_netMgr) : base(false, p_netMgr)
    {
      Init();
    }

    void Init()
    {
      CreateCMSG = ClientMessage.CMSG_CREATE_VERSION;
      ReadCMSG = ClientMessage.CMSG_READ_VERSION;
      UpdateCMSG = ClientMessage.CMSG_UPDATE_VERSION;
      UpdateListCMSG = ClientMessage.CMSG_CRUD_VERSION_LIST;
      DeleteCMSG = ClientMessage.CMSG_DELETE_VERSION;
      ListCMSG = ClientMessage.CMSG_LIST_VERSION;
      CopyCMSG = ClientMessage.CMSG_COPY_VERSION;

      CreateSMSG = ServerMessage.SMSG_CREATE_VERSION_ANSWER;
      ReadSMSG = ServerMessage.SMSG_READ_VERSION_ANSWER;
      UpdateSMSG = ServerMessage.SMSG_UPDATE_VERSION_ANSWER;
      UpdateListSMSG = ServerMessage.SMSG_CRUD_VERSION_LIST_ANSWER;
      DeleteSMSG = ServerMessage.SMSG_DELETE_VERSION_ANSWER;
      ListSMSG = ServerMessage.SMSG_LIST_VERSION_ANSWER;
      CopySMSG = ServerMessage.SMSG_COPY_VERSION_ANSWER;

      Build = Version.BuildVersion;

      InitCallbacks();
      m_netMgr.SetCallback((UInt16)CopySMSG, CopyAnswer);
    }

    ~VersionModel()
    {
      m_netMgr.RemoveCallback((UInt16)CopySMSG, CopyAnswer);
    }

    #region CRUD

    public void Copy(UInt32 p_originId, Version p_newVersion)
    {
      ByteBuffer packet = new ByteBuffer((UInt16)CopyCMSG);

      packet.WriteUint32(p_originId);
      packet.WriteString(p_newVersion.Name);
      packet.WriteUint16(p_newVersion.NbPeriod);
      packet.WriteUint32(p_newVersion.RateVersionId);
      packet.WriteUint32(p_newVersion.GlobalFactVersionId);
      packet.Release();

      m_netMgr.Send(packet);
    }

    private void CopyAnswer(ByteBuffer p_packet)
    {
      CopyEvent(p_packet.GetError(), p_packet.ReadUint32());
    }

    public TimeConfig GetHightestTimeConfig(List<uint> p_versionList)
    {
      TimeConfig l_selectedConfig = TimeConfig.MONTHS;

      foreach (UInt32 l_versionId in p_versionList)
      {
        Version l_version = GetValue(l_versionId);

        if (l_version == null)
          continue;
        if ((UInt32)l_version.TimeConfiguration < (UInt32)l_selectedConfig)
          l_selectedConfig = l_version.TimeConfiguration;
      }
      return (l_selectedConfig);
    }

    #endregion
  }
}

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

  public class FactLogModel
  {
    static FactLogModel s_instance = null;
    public static FactLogModel Instance 
    { get { return (s_instance == null) ? s_instance = new FactLogModel() : s_instance; } }
    public event ReadEventHandler ReadEvent;
    public delegate void ReadEventHandler(bool p_status, List<FactLog> p_factLogList);
    NetworkManager m_netMgr;

    FactLogModel()
    {
      m_netMgr = NetworkManager.Instance;
      Init();
    }

    public FactLogModel(NetworkManager p_netMgr)
    {
      m_netMgr = p_netMgr;
      Init();
    }

    void Init()
    {
      m_netMgr.SetCallback((UInt16)ServerMessage.SMSG_GET_FACT_LOG_ANSWER, GetFactLogAnswer);
    }

    ~FactLogModel()
    {
      m_netMgr.RemoveCallback((UInt16)ServerMessage.SMSG_GET_FACT_LOG_ANSWER, GetFactLogAnswer);
    }

    public void GetFactLog(UInt32 p_accountId, UInt32 p_entityId, UInt32 p_period, UInt32 p_versionId)
    {
      ByteBuffer packet = new ByteBuffer((UInt16)ClientMessage.CMSG_GET_FACT_LOG);

      packet.AssignRequestId();
      packet.WriteUint32(p_accountId);
      packet.WriteUint32(p_entityId);
      packet.WriteUint32(p_period);
      packet.WriteUint32(p_versionId);
      packet.Release();
      m_netMgr.Send(packet);
    }

    private void GetFactLogAnswer(ByteBuffer p_packet)
    {
      List<FactLog> factLogList = new List<FactLog>();


      if (p_packet.GetError() == ErrorMessage.SUCCESS)
      {
        UInt32 requestId = p_packet.ReadUint32();
        UInt32 nbResult = p_packet.ReadUint32();

        for (UInt32 i = 0; i < nbResult; i++)
        {
          FactLog ht = FactLog.BuildFactLog(p_packet);
          factLogList.Add(ht);
          if (ReadEvent != null)
            ReadEvent(true, factLogList);
        }
      }
      else
        if (ReadEvent != null)
          ReadEvent(false, null);
    }
  }
}

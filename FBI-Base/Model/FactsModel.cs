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

  public class FactsModel
  {
    static FactsModel s_instance = null;
    public static FactsModel Instance 
    { get { return (s_instance == null) ? s_instance = new FactsModel() : s_instance; } }
    public event UpdateEventHandler UpdateEvent;
    public delegate void UpdateEventHandler(ErrorMessage p_status, CRUDAction p_action, SafeDictionary<string, Tuple<UInt32, ErrorMessage>> p_resultsDict);
    public event DeleteEventHandler DeleteEvent;
    public delegate void DeleteEventHandler(ErrorMessage p_status, UInt32 p_factId);
    public event ReadEventHandler ReadEvent;
    public delegate void ReadEventHandler(ErrorMessage p_status, Int32 p_requestId, List<Fact> p_fact_list);
    private SafeDictionary<Int32, List<string>> m_requestIdCommitDic = new SafeDictionary<Int32, List<string>>();
    NetworkManager m_netMgr;

    public FactsModel(NetworkManager p_netMgr)
    {
      m_netMgr = p_netMgr;
      Init();
    }

    FactsModel()
    {
      m_netMgr = NetworkManager.Instance;
      Init();
    }

    void Init()
    {
      m_netMgr.SetCallback((UInt16)ServerMessage.SMSG_UPDATE_FACT_LIST_ANSWER, UpdateListAnswer);
      m_netMgr.SetCallback((UInt16)ServerMessage.SMSG_DELETE_FACT_ANSWER, DeleteFactAnswer);
    }

    ~FactsModel()
    {
      m_netMgr.RemoveCallback((UInt16)ServerMessage.SMSG_UPDATE_FACT_LIST_ANSWER, UpdateListAnswer);
      m_netMgr.RemoveCallback((UInt16)ServerMessage.SMSG_DELETE_FACT_ANSWER, DeleteFactAnswer);
    }

    public Int32 GetFactRH(UInt32 p_accountId, UInt32 p_entityId, List<AxisElem> p_employeeList, UInt32 p_versionId, UInt32 p_startPeriod, UInt32 p_endPeriod)
    {

      m_netMgr.SetCallback((UInt16)ServerMessage.SMSG_GET_FACT_ANSWER, GetFactAnswer);
      ByteBuffer l_packet = new ByteBuffer((UInt16)ClientMessage.CMSG_GET_FACT);
      Int32 l_requestId = l_packet.AssignRequestId();

      l_packet.WriteUint8((byte)Account.AccountProcess.RH);
      l_packet.WriteUint32(p_accountId);
      l_packet.WriteUint32((UInt32)p_employeeList.Count);
      foreach (AxisElem l_employee in p_employeeList)
        l_packet.WriteUint32(l_employee.Id);
      l_packet.WriteUint32(p_versionId);
      l_packet.WriteUint32(p_startPeriod);
      l_packet.WriteUint32(p_endPeriod);
      l_packet.WriteUint32(p_entityId);

      l_packet.Release();
      m_netMgr.Send(l_packet);
      return l_requestId;
    }

    public Int32 GetFactFinancial(List<AxisElem> p_entitiesList, UInt32 p_versionId, UInt32 p_clientId, UInt32 p_productId, UInt32 p_adjustmentId)
    {
      m_netMgr.SetCallback((UInt16)ServerMessage.SMSG_GET_FACT_ANSWER, GetFactAnswer);
      ByteBuffer l_packet = new ByteBuffer((UInt16)ClientMessage.CMSG_GET_FACT);
      Int32 l_requestId = l_packet.AssignRequestId();

      l_packet.WriteUint8((byte)Account.AccountProcess.FINANCIAL);
      l_packet.WriteInt32(p_entitiesList.Count);
      foreach (AxisElem l_entity in p_entitiesList)
        l_packet.WriteUint32(l_entity.Id);
      l_packet.WriteUint32(p_versionId);
      l_packet.WriteUint32(p_clientId);
      l_packet.WriteUint32(p_productId);
      l_packet.WriteUint32(p_adjustmentId);

      l_packet.Release();
      m_netMgr.Send(l_packet);
      return (l_requestId);
    }

    public void UpdateList(SafeDictionary<string, Fact> p_factsCommitDict, CRUDAction p_action)
    {
      ByteBuffer packet = new ByteBuffer(Convert.ToUInt16(ClientMessage.CMSG_UPDATE_FACT_LIST));

      Int32 l_requestId = packet.AssignRequestId();
      packet.WriteUint8((byte)p_action);
      m_requestIdCommitDic.Add(l_requestId, p_factsCommitDict.Keys.ToList<string>());
      packet.WriteInt32(p_factsCommitDict.Values.Count);
      if (p_action == CRUDAction.DELETE)
        foreach (Fact fact_value in p_factsCommitDict.Values)
          fact_value.Dump(packet, true);
      else
        foreach (Fact fact_value in p_factsCommitDict.Values)
          fact_value.Dump(packet, false);
      packet.Release();
      m_netMgr.Send(packet);
    }

    public void Delete(Fact p_fact)
    {
      ByteBuffer l_packet = new ByteBuffer(Convert.ToUInt16(ClientMessage.CMSG_DELETE_FACT));
      l_packet.WriteUint32(p_fact.Id);
      l_packet.Release();
      m_netMgr.Send(l_packet);
    }

    private void UpdateListAnswer(ByteBuffer packet)
    {
      if (packet.GetError() == ErrorMessage.SUCCESS)
      {
        Int32 requestId = packet.GetRequestId();
        CRUDAction l_action = (CRUDAction)packet.ReadUint8(); 
        SafeDictionary<string, Tuple<UInt32, ErrorMessage>> resultsDict = new SafeDictionary<string, Tuple<UInt32, ErrorMessage>>();
        UInt32 l_nbRecords = packet.ReadUint32();
        if (m_requestIdCommitDic.ContainsKey(requestId))
        {
          foreach (string cell_address in m_requestIdCommitDic[requestId])
          {
            resultsDict.Add(cell_address, new Tuple<UInt32, ErrorMessage>(packet.ReadUint32(), (ErrorMessage)packet.ReadUint8()));
          }
          m_requestIdCommitDic.Remove(requestId);
        }
        else
          System.Diagnostics.Debug.WriteLine("FACTS UDPATE LIST request id not in dictionary");
        if (UpdateEvent != null)
          UpdateEvent(packet.GetError(),l_action,  resultsDict);
      }
      else
        if (UpdateEvent != null)
          UpdateEvent(packet.GetError(), CRUDAction.CREATE, null);
    }

    private void GetFactAnswer(ByteBuffer p_packet)
    {
      List<Fact> l_factList = new List<Fact>();
      if (p_packet.GetError() == ErrorMessage.SUCCESS)
      {
        Int32 l_requestId = p_packet.GetRequestId();
        UInt32 nbResult = p_packet.ReadUint32();
        for (UInt32 i = 1; i <= nbResult; i++)
        {
          Fact hl_fact = Fact.BuildFact(p_packet);
          l_factList.Add(hl_fact);
        }
        if (ReadEvent != null)
          ReadEvent(p_packet.GetError(), l_requestId, l_factList);
      }
      else
        if (ReadEvent != null)
          ReadEvent(p_packet.GetError(), 0, null);
    }

    private void DeleteFactAnswer(ByteBuffer packet)
    {
      if (DeleteEvent != null)
        DeleteEvent(packet.GetError(), packet.ReadUint32());
    }
  }
}

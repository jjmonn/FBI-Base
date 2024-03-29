﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBI.MVC.Model
{
  using CRUD;
  using Network;

  public class LegacyComputeModel : AComputeModel 
  {
    public event ComputeCompleteEventHandler ComputeCompleteEvent;
    public delegate void ComputeCompleteEventHandler(ErrorMessage p_status, LegacyComputeRequest p_request, SafeDictionary<UInt32, ComputeResult> p_result);

    static LegacyComputeModel s_instance = null;
    public static LegacyComputeModel Instance 
    { get { return (s_instance == null) ? s_instance = new LegacyComputeModel() : s_instance; } }

    SafeDictionary<Int32, Tuple<bool, Int32>> m_toDiffList;
    NetworkManager m_netMgr;
    VersionModel m_versionModel;

    LegacyComputeModel() 
    {
      m_netMgr = NetworkManager.Instance;
      m_versionModel = VersionModel.Instance;
      Init();
    }

    public LegacyComputeModel(NetworkManager p_netMgr, VersionModel p_versionModel)
    {
      m_netMgr = p_netMgr;
      m_versionModel = p_versionModel;
      Init();
    }

    void Init()
    {
      m_toDiffList = new SafeDictionary<Int32, Tuple<bool, Int32>>();
      m_netMgr.SetCallback((UInt16)ServerMessage.SMSG_COMPUTE_RESULT, OnComputeResult);
    }

    public bool ComputeDiff(LegacyComputeRequest p_request, List<Int32> p_requestIdList = null)
    {
      if (p_request.Versions.Count > 2)
        return (false);
      p_request.IsDiff = true;
      return Compute(p_request, p_requestIdList);
    }

    public bool Compute(LegacyComputeRequest p_request, List<Int32> p_requestIdList = null)
    {
      ByteBuffer[] l_packetList = new ByteBuffer[p_request.Versions.Count];
      List<Int32> l_requestIdList = new List<int>();

      for (Int32 i = 0; i < l_packetList.Length; ++i)
      {
        l_packetList[i] = new ByteBuffer((UInt16)ClientMessage.CMSG_COMPUTE_REQUEST);
        Int32 l_requestId = l_packetList[i].AssignRequestId();
        l_requestIdList.Add(l_requestId);
        if (p_requestIdList != null)
          p_requestIdList.Add(l_requestId);
        m_requestAxisList[l_requestIdList[i]] = p_request.Versions[i];
      }
      m_requestList.Add(new Tuple<AComputeRequest, List<Int32>>(p_request, l_requestIdList));
      m_resultDic[p_request] = new SafeDictionary<uint, ComputeResult>();
      for (Int32 i = 0; i < l_packetList.Length; ++i)
      {
        p_request.Dump(l_packetList[i], p_request.Versions[i]);
        l_packetList[i].Release();
        if (m_netMgr.Send(l_packetList[i]) == false)
          return (false);
      }
      return (true);
    }

    public void OnComputeResult(ByteBuffer p_packet)
    {
      if (p_packet.GetError() != ErrorMessage.SUCCESS)
      {
        if (ComputeCompleteEvent != null)
          ComputeCompleteEvent(p_packet.GetError(), null, null);
        m_requestAxisList.Clear();
        m_requestList.Clear();
        m_requestList.Clear();
        m_toDiffList.Clear();
        return;
      }
      Int32 l_requestId = p_packet.GetRequestId();
      Tuple<AComputeRequest, List<Int32>> l_requestTuple = FindComputeRequest(l_requestId);
      if (l_requestTuple == null)
        return;
      LegacyComputeRequest l_request = l_requestTuple.Item1 as LegacyComputeRequest;
      List<Int32> l_requestIdList = l_requestTuple.Item2;
      ComputeResult l_result = null;

      if (l_request != null)
      {
        l_requestIdList.Remove(l_requestId);
        l_result = ComputeResult.BuildComputeResult(l_request, p_packet, m_versionModel.GetValue(m_requestAxisList[l_requestId]));
        l_result.RequestId = l_requestId;
        m_requestAxisList.Remove(l_requestId);
        m_resultDic[l_request][l_result.VersionId] = l_result;
        if (l_requestIdList.Count == 0)
        {
          SafeDictionary<UInt32, ComputeResult> l_resultDic = m_resultDic[l_request];
          List<ComputeResult> l_resultList = l_resultDic.Values.ToList();

          if (l_request.IsDiff && l_resultDic.Count == 2)
          {
            ComputeResult l_diffA = l_resultList[0] - l_resultList[1];
            ComputeResult l_diffB = l_resultList[1] - l_resultList[0];

            l_resultDic[l_diffA.VersionId] = l_diffA;
            l_resultDic[l_diffB.VersionId] = l_diffB;
          }
          m_requestList.Remove(l_requestTuple);
          m_resultDic.Remove(l_request);
          if (ComputeCompleteEvent != null)
            ComputeCompleteEvent(p_packet.GetError(), l_request, l_resultDic);
        }
      }
    }
  }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBI.MVC.Model.CRUD
{
  using Network;
  using AccountKey = UInt32;
  using SortKey = String;
  using PeriodTypeKey = TimeConfig;
  using PeriodKey = Int32;
  using VersionKey = UInt32;

  public class ComputeResult
  {
    public Int32 RequestId { get; set; }
    public SafeDictionary<ResultKey, double> Values { get; private set; }
    public UInt32 VersionId { get; set; }
    public UInt32 EntityId { get; set; }
    public bool IsDiff { get; private set; }
    public Tuple<VersionKey, VersionKey> VersionDiff { get; private set; }
    AComputeRequest m_request;
    Version m_version;
    List<Int32> m_periodList;
    List<Int32> m_aggregationPeriodList;

    ComputeResult()
    {
      Values = new SafeDictionary<ResultKey, double>();
      IsDiff = false;
    }

    public static ComputeResult BuildComputeResult(LegacyComputeRequest p_request, ByteBuffer p_packet, Version p_version)
    {
      ComputeResult l_result = BaseBuildComputeResult(p_request, p_packet, p_version, p_request.EntityId);
      l_result.FillResultData(p_packet, "", p_request.IsPeriodDiff);
      return (l_result);
    }

    public static ComputeResult BuildSourcedComputeResult(SourcedComputeRequest p_request, ByteBuffer p_packet, UInt32 p_entityId, Version p_version)
    {
      ComputeResult l_result = BaseBuildComputeResult(p_request, p_packet, p_version, p_entityId);
      l_result.FillEntityData(p_packet, "", "");
      return (l_result);
    }

    static ComputeResult BaseBuildComputeResult(AComputeRequest p_request, ByteBuffer p_packet, Version p_version, UInt32 p_entityId)
    {
      ComputeResult l_result = new ComputeResult();

      l_result.EntityId = p_entityId;
      l_result.VersionId = p_version.Id;
      l_result.m_request = p_request;
      l_result.m_version = p_version;

      if (p_request.Process == Account.AccountProcess.RH || l_result.m_version == null)
        l_result.m_periodList = PeriodModel.GetPeriodList((Int32)p_request.StartPeriod, (Int32)p_request.NbPeriods, l_result.m_version.TimeConfiguration);
      else
        l_result.m_periodList = PeriodModel.GetPeriodList((Int32)l_result.m_version.StartPeriod,
          (Int32)l_result.m_version.NbPeriod, l_result.m_version.TimeConfiguration);
      if (l_result.m_version.TimeConfiguration == TimeConfig.DAYS || l_result.m_version.TimeConfiguration == TimeConfig.MONTHS) 
      {
        TimeConfig l_aggregationTimeConfig = TimeUtils.GetParentConfig(l_result.m_version.TimeConfiguration);
        int l_nbPeriod = TimeUtils.GetParentConfigNbPeriods(l_result.m_version.TimeConfiguration, l_result.m_version.NbPeriod);
        int l_startPeriod = (int)l_result.m_version.StartPeriod;

        l_result.m_aggregationPeriodList = PeriodModel.GetPeriodList(l_startPeriod, l_nbPeriod, l_aggregationTimeConfig);
      }
      else
        l_result.m_aggregationPeriodList = new List<Int32>();

      Int32 l_nbValues = p_packet.ReadInt32();
      l_result.Values = new SafeDictionary<ResultKey, double>(l_nbValues);
      return (l_result);
    }

    void FillResultData(ByteBuffer p_packet, SortKey p_sortKey, bool p_usePeriodIndex)
    {
      bool l_isFiltered;
      SortKey l_currentLevelKey = "";

      if ((l_isFiltered = p_packet.ReadBool()))
      {
        AxisType l_axis = (AxisType)p_packet.ReadUint8();
        bool l_isAxis = p_packet.ReadBool();
        UInt32 l_value;

        if (l_isAxis)
          l_value = p_packet.ReadUint32();
        else
        {
          p_packet.ReadUint32();
          l_value = p_packet.ReadUint32();
        }
        l_currentLevelKey = ResultKey.GetSortKey(l_isAxis, l_axis, l_value);
        p_sortKey += l_currentLevelKey;
      }
      FillEntityData(p_packet, p_sortKey, l_currentLevelKey, "", p_usePeriodIndex);

      UInt32 l_nbChildResult = p_packet.ReadUint32();
      for (UInt32 i = 0; i < l_nbChildResult; ++i)
        FillResultData(p_packet, p_sortKey, p_usePeriodIndex);
    }

    void FillEntityData(ByteBuffer p_packet, SortKey p_sortKey, SortKey p_currentLevelKey, SortKey p_entityKey = "", bool p_usePeriodIndex = false)
    {
      UInt32 l_entityId = p_packet.ReadUint32();
      UInt32 l_nbAccount = p_packet.ReadUint32();
      bool firstLevel = (p_entityKey == "");

      p_entityKey += ResultKey.GetSortKey(true, AxisType.Entities, l_entityId);

      for (UInt32 i = 0; i < l_nbAccount; ++i)
      {
        UInt32 l_accountId = p_packet.ReadUint32();
        UInt16 l_nbPeriod = p_packet.ReadUint16();
        UInt32 l_nbAggregation;

        for (UInt16 j = 0; j < l_nbPeriod && j < m_periodList.Count; ++j)
        {
          double l_value = p_packet.ReadDouble();
          Int32 l_period = (p_usePeriodIndex) ? j : m_periodList[j];

          if (firstLevel)
            Values[new ResultKey(l_accountId, p_sortKey, "", m_version.TimeConfiguration, l_period, VersionId)] = l_value;
          Values[new ResultKey(l_accountId, p_sortKey, p_entityKey, m_version.TimeConfiguration, l_period, VersionId)] = l_value;
        }

        l_nbAggregation = p_packet.ReadUint32();

        TimeConfig l_aggregationTimeConfig = TimeUtils.GetParentConfig(m_version.TimeConfiguration);
        for (UInt16 j = 0; j < l_nbAggregation && j < m_aggregationPeriodList.Count; ++j)
        {
          double l_value = p_packet.ReadDouble();
          Int32 l_period = (p_usePeriodIndex) ? j : m_aggregationPeriodList[j];

          if (firstLevel)
            Values[new ResultKey(l_accountId, p_sortKey, "", l_aggregationTimeConfig, l_period, VersionId)] = l_value;
          Values[new ResultKey(l_accountId, p_sortKey, p_entityKey, l_aggregationTimeConfig, l_period, VersionId)] = l_value;
        }
      }

      UInt32 l_nbChildEntity = p_packet.ReadUint32();
      for (UInt32 j = 0; j < l_nbChildEntity; ++j)
        FillEntityData(p_packet, p_sortKey, p_currentLevelKey, p_entityKey, p_usePeriodIndex);
    }

    public static UInt32 GetDiffId(UInt32 p_idA, UInt32 p_idB)
    {
      return (p_idA * 1000 ^ p_idB * 20000);
    }

    public static bool IsDiffId(UInt32 p_versionId)
    {
      return (p_versionId >= 20000);
    }

    public static ComputeResult operator-(ComputeResult p_a, ComputeResult p_b)
    {
      ComputeResult l_result = new ComputeResult();

      l_result.IsDiff = true;
      l_result.VersionDiff = new Tuple<VersionKey, VersionKey>(p_a.VersionId, p_b.VersionId);
      l_result.VersionId = GetDiffId(p_a.VersionId, p_b.VersionId);
      foreach (KeyValuePair<ResultKey, double> l_pair in p_a.Values)
      {
        ResultKey l_bKey = new ResultKey(l_pair.Key.AccountId, l_pair.Key.SortHash, l_pair.Key.EntityHash,
          l_pair.Key.PeriodType, l_pair.Key.Period, p_b.VersionId);
        double l_bValue = p_b.Values[l_bKey];

       ResultKey l_key = new ResultKey(l_pair.Key.AccountId, l_pair.Key.SortHash, l_pair.Key.EntityHash,
          l_pair.Key.PeriodType, l_pair.Key.Period, l_result.VersionId);
       l_result.Values[l_key] = l_pair.Value - l_bValue;
      }
      return (l_result);
    }

    public static ComputeResult DiffRange(ComputeResult p_a, ComputeResult p_b, SafeDictionary<Int32, Int32> p_periods)
    {
      int l_periodNb = 0;
      ComputeResult l_result = new ComputeResult();

      l_result.IsDiff = true;
      l_result.VersionDiff = new Tuple<VersionKey, VersionKey>(p_a.VersionId, p_b.VersionId);
      l_result.VersionId = GetDiffId(p_a.VersionId, p_b.VersionId);
      foreach (KeyValuePair<ResultKey, double> l_pair in p_a.Values)
      {
        if (p_periods.ContainsKey(l_pair.Key.Period) == false)
          continue;
        ResultKey l_bKey = new ResultKey(l_pair.Key.AccountId, l_pair.Key.SortHash, l_pair.Key.EntityHash,
          l_pair.Key.PeriodType, p_periods[l_pair.Key.Period], p_b.VersionId);
        double l_bValue = p_b.Values[l_bKey];

        ResultKey l_key = new ResultKey(l_pair.Key.AccountId, l_pair.Key.SortHash, l_pair.Key.EntityHash,
           l_pair.Key.PeriodType, ++l_periodNb, l_result.VersionId);
        l_result.Values[l_key] = l_pair.Value - l_bValue;
      }
      return (l_result);
    }

    public void DiffPeriod(Int32 l_periodA, Int32 l_periodB, UInt32 l_diffId)
    {
      SafeDictionary<ResultKey, List<Tuple<Int32, double>>> l_values = new SafeDictionary<ResultKey, List<Tuple<PeriodKey, double>>>();

      foreach (KeyValuePair<ResultKey, double> l_pair in Values)
      {
        PeriodKey l_period = l_pair.Key.Period;

        if (l_period == l_periodA || l_period == l_periodB)
        {
          ResultKey k = l_pair.Key;
          ResultKey l_matchKey = new ResultKey(k.AccountId, k.SortHash, k.EntityHash, k.PeriodType, (Int32)l_diffId, k.VersionId, k.StrongVersion, k.Tab);
          if (l_values[l_matchKey] == null)
            l_values[l_matchKey] = new List<Tuple<PeriodKey,double>>();
          l_values[l_matchKey].Add(new Tuple<PeriodKey, double>(l_period, l_pair.Value));
        }
      }
      foreach (KeyValuePair<ResultKey, List<Tuple<Int32, double>>> l_pair in l_values)
      {
        if (l_pair.Value.Count < 1)
          continue;
        double l_val2 = (l_pair.Value.Count < 2) ? 0 : l_pair.Value[1].Item2;

        Values[l_pair.Key] = l_pair.Value[0].Item2 - l_val2;
      }
    }
  }
}

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

  public class RatesVersionModel : NamedCRUDModel<ExchangeRateVersion>
  {
    static RatesVersionModel s_instance = null;
    public static RatesVersionModel Instance 
    { get { return (s_instance == null) ? s_instance = new RatesVersionModel() : s_instance; } }

    RatesVersionModel() : base(false, NetworkManager.Instance)
    {
      Init();
    }

    public RatesVersionModel(NetworkManager p_netMgr) : base(false, p_netMgr)
    {
      Init();
    }

    void Init()
    {
      CreateCMSG = ClientMessage.CMSG_CREATE_RATE_VERSION;
      ReadCMSG = ClientMessage.CMSG_READ_RATE_VERSION;
      UpdateCMSG = ClientMessage.CMSG_UPDATE_RATE_VERSION;
      UpdateListCMSG = ClientMessage.CMSG_CRUD_RATE_VERSION_LIST;
      DeleteCMSG = ClientMessage.CMSG_DELETE_RATE_VERSION;
      ListCMSG = ClientMessage.CMSG_LIST_RATE_VERSION;

      CreateSMSG = ServerMessage.SMSG_CREATE_RATE_VERSION_ANSWER;
      ReadSMSG = ServerMessage.SMSG_READ_RATE_VERSION_ANSWER;
      UpdateSMSG = ServerMessage.SMSG_UPDATE_RATE_VERSION_ANSWER;
      UpdateListSMSG = ServerMessage.SMSG_CRUD_RATE_VERSION_LIST_ANSWER;
      DeleteSMSG = ServerMessage.SMSG_DELETE_RATE_VERSION_ANSWER;
      ListSMSG = ServerMessage.SMSG_LIST_RATE_VERSION_ANSWER;

      Build = ExchangeRateVersion.BuildExchangeRateVersion;

      InitCallbacks();
    }

    public List<Int32> GetMonthsList(UInt32 versionId)
    {
      ExchangeRateVersion l_version = GetValue(versionId);
      if (l_version == null)
        return (null);
      return (PeriodModel.GetMonthsList((int)l_version.StartPeriod, (int)l_version.NbPeriod, CRUD.TimeConfig.MONTHS));
    }
  }
}

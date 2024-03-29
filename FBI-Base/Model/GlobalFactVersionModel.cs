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

  public class GlobalFactVersionModel : NamedCRUDModel<GlobalFactVersion>
  {
    static GlobalFactVersionModel s_instance = null;
    public static GlobalFactVersionModel Instance 
    { get { return (s_instance == null) ? s_instance = new GlobalFactVersionModel() : s_instance; } }
    
    GlobalFactVersionModel() : base(false, NetworkManager.Instance)
    {
      Init();
    }

    public GlobalFactVersionModel(NetworkManager p_netMgr) : base(false, p_netMgr)
    {
      Init();
    }

    void Init()
    {
      CreateCMSG = ClientMessage.CMSG_CREATE_GLOBAL_FACT_VERSION;
      ReadCMSG = ClientMessage.CMSG_READ_GLOBAL_FACT_VERSION;
      UpdateCMSG = ClientMessage.CMSG_UPDATE_GLOBAL_FACT_VERSION;
      UpdateListCMSG = ClientMessage.CMSG_CRUD_GLOBAL_FACT_VERSION_LIST;
      DeleteCMSG = ClientMessage.CMSG_DELETE_GLOBAL_FACT_VERSION;
      ListCMSG = ClientMessage.CMSG_LIST_GLOBAL_FACT_VERSION;

      CreateSMSG = ServerMessage.SMSG_CREATE_GLOBAL_FACT_VERSION_ANSWER;
      ReadSMSG = ServerMessage.SMSG_READ_GLOBAL_FACT_VERSION_ANSWER;
      UpdateSMSG = ServerMessage.SMSG_UPDATE_GLOBAL_FACT_VERSION_ANSWER;
      UpdateListSMSG = ServerMessage.SMSG_CRUD_GLOBAL_FACT_VERSION_LIST_ANSWER;
      DeleteSMSG = ServerMessage.SMSG_DELETE_GLOBAL_FACT_VERSION_ANSWER;
      ListSMSG = ServerMessage.SMSG_LIST_GLOBAL_FACT_VERSION_ANSWER;

      Build = GlobalFactVersion.BuildGlobalFactVersion;

      InitCallbacks();
    }

    public List<Int32> GetMonthsList(UInt32 versionId)
    {
      GlobalFactVersion l_version = GetValue(versionId);
      if (l_version == null)
        return (null);
      return (PeriodModel.GetMonthsList((int)l_version.StartPeriod, (int)l_version.NbPeriod, CRUD.TimeConfig.MONTHS));
    }
  }
}

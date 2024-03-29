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

  public class FactTagModel : SimpleCRUDModel<FactTag>
  {
    static FactTagModel s_instance = null;
    public static FactTagModel Instance 
    { get { return (s_instance == null) ? s_instance = new FactTagModel() : s_instance; } }

    FactTagModel() : base(NetworkManager.Instance)
    {
      Init();
    }

    public FactTagModel(NetworkManager p_netMgr) : base(p_netMgr)
    {
      Init();
    }

    void Init()
    {
      CreateCMSG = ClientMessage.CMSG_CREATE_FACT_TAG;
      ReadCMSG = ClientMessage.CMSG_READ_FACT_TAG;
      UpdateCMSG = ClientMessage.CMSG_UPDATE_FACT_TAG;
      UpdateListCMSG = ClientMessage.CMSG_CRUD_FACT_TAG;
      ListCMSG = ClientMessage.CMSG_LIST_FACT_TAG;
      DeleteCMSG = ClientMessage.CMSG_DELETE_FACT_TAG;

      CreateSMSG = ServerMessage.SMSG_CREATE_FACT_TAG_ANSWER;
      ReadSMSG = ServerMessage.SMSG_READ_FACT_TAG_ANSWER;
      UpdateSMSG = ServerMessage.SMSG_UPDATE_FACT_TAG_ANSWER;
      UpdateListSMSG = ServerMessage.SMSG_CRUD_FACT_TAG_LIST_ANSWER;
      DeleteSMSG = ServerMessage.SMSG_DELETE_FACT_TAG_ANSWER;
      ListSMSG = ServerMessage.SMSG_LIST_FACT_TAG_ANSWER;

      Build = FactTag.BuildFactTag;

      InitCallbacks();
    }
  }
}

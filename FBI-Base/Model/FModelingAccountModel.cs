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

  public class FModelingAccountModel : NamedCRUDModel<FModelingAccount>
  {
    static FModelingAccountModel s_instance = null;
    public static FModelingAccountModel Instance 
    { get { return (s_instance == null) ? s_instance = new FModelingAccountModel() : s_instance; } }

    FModelingAccountModel() : base(false, NetworkManager.Instance)
    {
      Init();
    }

    public FModelingAccountModel(NetworkManager p_netMgr) : base(false, p_netMgr)
    {
      Init();
    }

    void Init()
    {
      CreateCMSG = ClientMessage.CMSG_CREATE_FMODELING_ACCOUNT;
      ReadCMSG = ClientMessage.CMSG_READ_FMODELING_ACCOUNT;
      UpdateCMSG = ClientMessage.CMSG_UPDATE_FMODELING_ACCOUNT;
      UpdateListCMSG = ClientMessage.CMSG_UPDATE_FMODELING_ACCOUNT;
      DeleteCMSG = ClientMessage.CMSG_DELETE_FMODELING_ACCOUNT;
      ListCMSG = ClientMessage.CMSG_LIST_FMODELING_ACCOUNT;

      CreateSMSG = ServerMessage.SMSG_CREATE_FMODELING_ACCOUNT_ANSWER;
      ReadSMSG = ServerMessage.SMSG_READ_FMODELING_ACCOUNT_ANSWER;
      UpdateSMSG = ServerMessage.SMSG_UPDATE_FMODELING_ACCOUNT_ANSWER;
      UpdateListSMSG = ServerMessage.SMSG_UPDATE_FMODELING_ACCOUNT_ANSWER;
      DeleteSMSG = ServerMessage.SMSG_DELETE_FMODELING_ACCOUNT_ANSWER;
      ListSMSG = ServerMessage.SMSG_LIST_FMODELING_ACCOUNT_ANSWER;

      Build = FModelingAccount.BuildFModelingAccount;

      InitCallbacks();
    }
  }
}

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

  public class ChartPanelModel : NamedCRUDModel<ChartPanel>
  {
    static ChartPanelModel s_instance = null;
    public static ChartPanelModel Instance 
    { get { return (s_instance == null) ? s_instance = new ChartPanelModel() : s_instance; } }

    ChartPanelModel() : base(true, NetworkManager.Instance)
    {
      Init();
    }

    public ChartPanelModel(NetworkManager p_netMgr) : base(true, p_netMgr)
    {
      Init();
    }

    void Init()
    {
      CreateCMSG = ClientMessage.CMSG_CREATE_CHART_PANEL;
      UpdateCMSG = ClientMessage.CMSG_UPDATE_CHART_PANEL;
      UpdateListCMSG = ClientMessage.CMSG_CRUD_CHART_PANEL_LIST;
      DeleteCMSG = ClientMessage.CMSG_DELETE_CHART_PANEL;
      ListCMSG = ClientMessage.CMSG_LIST_CHART_PANEL;

      CreateSMSG = ServerMessage.SMSG_CREATE_CHART_PANEL_ANSWER;
      ReadSMSG = ServerMessage.SMSG_READ_CHART_PANEL_ANSWER;
      UpdateSMSG = ServerMessage.SMSG_UPDATE_CHART_PANEL_ANSWER;
      UpdateListSMSG = ServerMessage.SMSG_CRUD_CHART_PANEL_LIST_ANSWER;
      DeleteSMSG = ServerMessage.SMSG_DELETE_CHART_PANEL_ANSWER;
      ListSMSG = ServerMessage.SMSG_LIST_CHART_PANEL_ANSWER;

      Build = ChartPanel.BuildChartPanel;

      InitCallbacks();
    }
  }
}

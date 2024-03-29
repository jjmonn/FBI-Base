﻿using System;
using System.Threading;

namespace FBI.Network
{
  public class NetworkLauncher
  {
    Thread m_thread;
    ClientState m_state;
    Action m_diconnectCallback;
    string m_ip;
    UInt16 m_port;
    NetworkManager m_mgr;

    public NetworkLauncher()
    {
      m_mgr = NetworkManager.Instance;
      m_state = ClientState.not_connected;
    }

    public NetworkLauncher(NetworkManager p_mgr)
    {
      m_mgr = p_mgr;
      m_state = ClientState.not_connected;
    }

    public bool Relaunch()
    {
      Stop();
      if (m_mgr.Connect(m_ip, m_port) == true)
      {
        m_state = ClientState.running;
        m_thread = new Thread(new ThreadStart(HandleLoop));
        m_thread.Name = "Network Module";
        m_thread.Start();
        return (true);
      }
      else
        return (false);
    }

    public bool Launch(string p_ip, UInt16 p_port, Action p_disconnectCallback = null)
    {
      m_ip = p_ip;
      m_port = p_port;
      m_diconnectCallback = p_disconnectCallback;
      return (Relaunch());
    }

    public void Stop()
    {
      if (m_state != ClientState.running)
        return;
      m_mgr.Disconnect();
      m_state = ClientState.shuting_down;
      if (m_thread != null)
        m_thread.Join();
    }

    void HandleLoop()
    {
      while (m_state == ClientState.running)
      {
        try
        {
          if (m_mgr.HandlePacket() == false)
          {
            m_state = ClientState.not_connected;
            if (m_diconnectCallback != null)
              m_diconnectCallback();
          }
        }
        catch (OutOfMemoryException e)
        {
          System.Diagnostics.Debug.WriteLine(e.Message);
        }
      }
    }

    public ClientState GetState()
    {
      return (m_state);
    }
  }
}
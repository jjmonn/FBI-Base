﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBI.MVC.Model.CRUD
{
 using Network;

  public enum LegalHolidayTag
  {
    NONE = 0,
    FER
  }

  public class LegalHoliday : CRUDEntity
  {
    public UInt32 Id { get; set; }
    public UInt32 EmployeeId { get; set; }
    public UInt32 Period { get; set; }
    public UInt32 Image { get; set; }
    public LegalHolidayTag Tag { get; set; }

    public LegalHoliday() { }
    private LegalHoliday(UInt32 p_id)
    {
      Id = p_id;
    }

    public static LegalHoliday BuildLegalHoliday(ByteBuffer p_packet)
    {
      LegalHoliday l_legalHoliday = new LegalHoliday(p_packet.ReadUint32());

      l_legalHoliday.EmployeeId = p_packet.ReadUint32();
      l_legalHoliday.Period = p_packet.ReadUint32();

      return (l_legalHoliday);
    }

    public void Dump(ByteBuffer p_packet, bool p_includeId)
    {
      if (p_includeId)
        p_packet.WriteUint32(Id);
      p_packet.WriteUint32(EmployeeId);
      p_packet.WriteUint32(Period);
    }

    public int CompareTo(object p_obj)
    {
      if (p_obj == null)
        return 0;
      LegalHoliday l_cmpEntity = p_obj as LegalHoliday;

      if (l_cmpEntity == null)
        return 0;
      if (l_cmpEntity.EmployeeId > EmployeeId)
        return -1;
      else
        return 1;
    }
  }
}

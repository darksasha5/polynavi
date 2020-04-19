﻿using SQLiteNetExtensions.Attributes;

namespace PolyNaviLib.BL
{
    public class Faculty : BusinessEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbr { get; set; }

        [OneToOne]
        public Group Group { get; set; }

        [ForeignKey(typeof(Group))]
        public int GroupID { get; set; }
    }
}

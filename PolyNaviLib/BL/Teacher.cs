﻿using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace PolyNaviLib.BL
{
    public class Teacher : BusinessEntity
    {
        public int Id { get; set; }
        public string Full_Name { get; set; }
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        public string Last_Name { get; set; }
        public string Grade { get; set; }
        public string Chair { get; set; }

        [ManyToOne]
        public Lesson Lesson { get; set; }

        [ForeignKey(typeof(Lesson))]
        public int LessonID { get; set; }
    }

    public class TeachersRoot
    {
        public List<Teacher> Teachers { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace PolyNaviLib.BL
{
	public class BuisnessEntity : IBuisnessEntity
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; } = 0;
	}
}

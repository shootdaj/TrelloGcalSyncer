using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloGcalSyncer;

namespace ConsoleRunner
{
	class Program
	{
		static void Main(string[] args)
		{
			var syncer = new Syncer();
			syncer.Sync();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TrelloGcalSyncer.Tests
{
    public class SyncerTests
    {
		[Test]
		public void Sync_Works()
		{
			Assert.DoesNotThrow(() =>
			{
				var syncer = new Syncer();
				syncer.Sync();
			});
		}

		//[Test]
	 //   public void AddToGcal_Works()
	 //   {
		//    var syncer = new Syncer();
		//	syncer.AddCardToGCal();
	 //   }
    }
}

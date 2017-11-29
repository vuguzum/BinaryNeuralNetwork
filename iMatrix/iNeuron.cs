using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMatrixSpc
{
	public enum WallType { Stop = 0, Back = 1, Forward = 2 }
	public class iNeuron
	{
		public int Memo;
		public WallType h = WallType.Stop;  //Горизонтальная верхняя стенка
		public WallType v = WallType.Stop;  //Вертикальная левая стенка

		internal int Res;
		internal iNeuron Left;
		internal iNeuron Right;

		private int ResNew;
		public int Output => (h == WallType.Forward) ? Res : 0;
		public void PutInput(int bit)
		{
			unsafe { Res = bit; }
		}

		public void Act()
		{
			//unsafe
			//{
                ResNew = Res;
                if (v == WallType.Forward)
					ResNew ^= Left.Res;
				if (Right.v == WallType.Back)
					ResNew ^= Right.Res;
			//}
		}

		public void PutInputWithMemo(int bit)
		{
			//unsafe
			//{
				Res = bit ^ Memo;
			//}
		}

		public void ActWithMemo()
		{
			unsafe
			{
                ResNew = Res;
                if (v == WallType.Forward)
					ResNew ^= (Left.Res);
				if (Right.v == WallType.Back)
					ResNew ^= (Right.Res);

				Memo = ResNew;
			}
		}
		public void FixRes()
		{
			unsafe
			{
				Res = ResNew;
			}
		}


	}
}

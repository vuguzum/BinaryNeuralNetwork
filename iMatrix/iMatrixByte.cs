using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMatrixSpc
{
	public class iMatrixByte
	{
		public iMatrix matrix;
		public iMatrixByte()
		{
			matrix = new iMatrix(8);
		}
		/// <summary>
		/// Объект матрицы клонируется внутри, параметр m можно потом изменять
		/// </summary>
		/// <param name="m"></param>
		public iMatrixByte(iMatrix m)
		{
			if (m.xWidth != 8)
				throw new Exception("Ширина матрицы не равна 8.");
			matrix = m.Clone();
		}

		/// <summary>
		/// На входе и выходе по байту
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		public byte Process(byte arg)
		{
			var a = new ByteArg(arg);
			matrix.ProcessMatrix(a.ToBitArray());
			//var output = matrix.GetOutput(8);
			var res = new ByteRes(matrix.MatrixOutput);
			//if (res.Value != 0)
			//	Debug.WriteLine("Ненулевой результат: " + res.Value);
			return res.Value;
		}

	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using iMatrixSpc;

namespace mks
{
	class Program
	{
		private static DateTime time;
		private static TimeSpan timespan = new TimeSpan(0, 0, 0, 0, 50);
		private static string[] wheel = { @"-", @"\", @"|", @"/" };
		private static int wheelIndex = 0;
		private static BigInteger LastRowCounter = 0;

		static void Main(string[] args)
		{
			Console.WriteLine("-- TEACHING MATRIX -- " + DateTime.Now.ToLongTimeString() + " -");

			var teacher = new Teaching(100, 50, 1);
			teacher.Progress = DrawProgress;

			//умножение на 2 - работает до 7
			var inpValues = new byte[] { 1, 2, 3, 4, 5, 6 };
			var outValues = new byte[] { 2, 4, 6, 8, 10, 12 };

			var res = teacher.Teach(inpValues, outValues, 1000);
			Console.CursorTop += teacher.resList.Count>0 ? teacher.resList[0].yHeight * 2 + 3 : 3;

			if (res == TeachingResult.TeachingComplete)
			{
				Console.WriteLine("Завершено. Всего матриц " + teacher.resList.Count);
				MatrixDraw.DrawMatrix(teacher.resList[0]);
			}
			else if (res == TeachingResult.MaxRowLimitReached)
			{
				Console.WriteLine("Превышен максимальный размер матрицы.");
				Console.ReadKey();
				return;
			}
			else if (res == TeachingResult.ContinueTeaching)
			{
				Console.WriteLine("Незавершено. Всего матриц: " + teacher.resList.Count);
				if (teacher.resList.Count == 0)
				{
					Console.ReadKey();
					return;
				}
			}

			Console.WriteLine();

			foreach (var m in teacher.resList)
			{
				Console.WriteLine("Тест на обучающей выборке");
				CheckFoundMatrix(m, inpValues, outValues);
			}

			while (true)
			{
				Console.WriteLine("Проверить значение: ");
				var read = Console.ReadLine();
				if (String.IsNullOrWhiteSpace(read))
					break;

				byte intTemp = Convert.ToByte(read);
				var matr = new iMatrixSpc.iMatrixByte(teacher.resList[0]);
				var mRes = matr.Process(intTemp);
				Console.WriteLine(intTemp + " => " + mRes);
			}
		}

		public static void DrawProgress(iMatrix m, int[] inputValues, int[] outputValues, DrawData drawData)
		{
			if ((DateTime.Now - time) > timespan)
			{
				//считаем число операций в сек.
				long opsPerSec = 0;
				if ((m.RowCombinationCounter > LastRowCounter) && time != DateTime.MinValue)
				{
					try
					{
						var denom = (DateTime.Now - time).TotalSeconds;
						var nomin = (double) new Numerics.BigRational(m.RowCombinationCounter - LastRowCounter);
						opsPerSec = (long) (nomin / denom);
					}
					catch 
					{
						opsPerSec = 0;
					}
				}

				LastRowCounter = m.RowCombinationCounter;
				time = DateTime.Now;
				Console.Write("                                                             \r");
				DrawWheel();
				Console.WriteLine(String.Format(" Level: {0}.{1} ({2})                        ", drawData.level, drawData.levelCount, drawData.levelTotal));
				Console.WriteLine(String.Format("  Metrika: {0}  ", drawData.currentMetrika));
				Console.WriteLine(String.Format("  Speed: {0}                                  ", ((opsPerSec != 0) ? opsPerSec.ToString("N0") : "?")));
				Console.WriteLine("Teaching last row:");
				MatrixDraw.DrawMatrix(m);
				CursorUp(4 + m.yHeight*2);
			}
		}
		public static void CursorUp(int linesUp)
		{
			for (int i = 0; i < linesUp; i++)
				Console.CursorTop--;
		}
		private static void DrawWheel()
		{
			Console.Write(wheel[wheelIndex]);
			wheelIndex = (wheelIndex >= 3) ? 0 : wheelIndex + 1;
		}
		public static void CheckFoundMatrix(iMatrix m, byte[] inp, byte[] outp)
		{
			var matr = new iMatrixByte(m);
			int i = 0;
			foreach (var item in inp)
			{
				var res = matr.Process(item);
				Console.WriteLine(item + " => " + res + " (" + outp[i++] + ")");
			}
		}
	}
}


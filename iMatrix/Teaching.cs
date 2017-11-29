using Matrix;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace iMatrixSpc
{
	public enum TeachingResult { TeachingComplete, MaxRowLimitReached, ContinueTeaching } //ZeroMetrikaNoResult
	public class DrawData
	{
		/// <summary>
		/// метрика на текущей матричной комбинации
		/// </summary>
		public ulong combMetrika;
		/// <summary>
		/// текущая метрика
		/// </summary>
		public ulong currentMetrika;
		/// <summary>
		/// сколько матриц в дереве отложено после ветвления с минимальной метрикой
		/// </summary>
		public int MatrixNodesCount; //OfMatrixWithCurrentMetrika
		public int level;
		public int levelCount;
		public int levelTotal;

	}
	public delegate void Progress(iMatrix m, int[] inputValues, int[] outputValues, DrawData drawData);
	public class Teaching
	{
		public int VariantsCountLimit;
		public int MaxRowLimit;
		public int MaxMatrixFoundLimit;
		public Progress Progress;
		public List<iMatrix> resList = new List<iMatrix>();
		private CodeMatrComparer matrComparer = new CodeMatrComparer();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="variantsCountLimit">сколько вариантов оптимизации рассматривать на строке</param>
		/// <param name="maxRowLimit">лимит строк матрицы</param>
		/// <param name="maxMatrixFoundLimit">лимит найденных решений</param>
		public Teaching(int variantsCountLimit, int maxRowLimit = int.MaxValue, int maxMatrixFoundLimit = 1)
		{
			this.VariantsCountLimit = variantsCountLimit;
			this.MaxRowLimit = maxRowLimit;
			this.MaxMatrixFoundLimit = maxMatrixFoundLimit;
		}
		public TeachingResult Teach(byte[] teachingInput, byte[] teachingOutput, long initMetrika = int.MaxValue)
		{
			if (teachingInput.Length != teachingOutput.Length)
				throw new Exception("Размеры обучающих последовательностей не совпадают.");

			return TeachMatrixRow(teachingInput, teachingOutput, null, initMetrika, 1);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="teachingInput"></param>
		/// <param name="teachingOutput"></param>
		/// <param name="matr"></param>
		/// <param name="metrika"></param>
		/// <param name="level">Высота матрицы (номер добавляемой строки)</param>
		/// <param name="levelCount">Номер варианта матрицы на текущем уровне</param>
		/// <param name="levelTotal">Всего вариантов матриц на уровне</param>
		/// <returns></returns>
		private TeachingResult TeachMatrixRow(
			byte[] teachingInput,
			byte[] teachingOutput,
			iMatrix matr,
			long metrika,
			int level = 1,
			int levelCount = 0,
			int levelTotal = 1)
		{
			Debug.WriteLine(String.Format("Метрика {0}", metrika));
			long localMetrika = metrika;
			iMatrixByte m = new iMatrixByte();
			List<iMatrix> localOptimizedList = new List<iMatrix>();

			while (m.matrix.NextWallCombinationOnLastRow() != MatrixIncrement.LimitReached)
			{
				//вычисляем метрику для текущей комбинации
				long combinationMetrika = ComputeCombinationMetrika(m, teachingInput, teachingOutput, localMetrika);

				Progress?.Invoke(m.matrix, null, null, new DrawData
				{
					currentMetrika = (ulong) localMetrika,
					level = level,
					levelCount = levelCount + 1,
					levelTotal = levelTotal
				});

				if (combinationMetrika > localMetrika)
					continue;

				if (combinationMetrika == 0)//найден результат
				{
                    localOptimizedList.Clear();
                    var resMatr = ConcatRowToMatrix(matr, m.matrix);
					resList.Add(resMatr);
                    localMetrika = 0;
                    Debug.WriteLine(String.Format("Метрика {0}", localMetrika));

                    if (resList.Count >= this.MaxMatrixFoundLimit)
					{
						Debug.WriteLine(String.Format("Достинут предел количества обученных матриц ({0}). Процесс завершен.", this.MaxMatrixFoundLimit));
						return TeachingResult.TeachingComplete;
					}
				}
				else if (combinationMetrika < localMetrika) //найдена матрица с лучшей метрикой
				{
					//сбрасываем список лучших и включаем текущую (из одной строки) в него 
					localOptimizedList.Clear();
					localOptimizedList.Add(m.matrix.Clone());
					localMetrika = combinationMetrika;
					Debug.WriteLine(String.Format("Метрика {0}", localMetrika));
				}
				else if (combinationMetrika == localMetrika) //найдена матрица для варианта оптимизации
				{
					if (localOptimizedList.Count < this.VariantsCountLimit)
					{
						localOptimizedList.Add(m.matrix.Clone());
						//Debug.WriteLine(String.Format("Добавлена матрица. Метрика {0}", localMetrika));
					}
				}
			}

			if (level >= this.MaxRowLimit)
			{
                return TeachingResult.MaxRowLimitReached;
            }

            var counter = 0;
            var optMatrArray = GetLocalOptimizedArray(localOptimizedList);
            //если нашли не все матрицы и есть оптимизированные, то продолжаем поиск
            while (resList.Count < this.MaxMatrixFoundLimit && (counter < optMatrArray.Length))
            {
                var nextMatr = ConcatRowToMatrix(matr, optMatrArray[counter]);
                var nextInputs = GetNextInputsAsOutputs(new iMatrixByte(optMatrArray[counter]), teachingInput);

                var res = TeachMatrixRow(nextInputs, teachingOutput, nextMatr, localMetrika - 1, level + 1, counter, optMatrArray.Length);
                if (res != TeachingResult.ContinueTeaching)
                    return res;
                else
                    counter++;
            }

            return TeachingResult.ContinueTeaching;
        }
		/// <summary>
		/// Присоединяет строку однострочной матрицы к матрице-получателю
		/// </summary>
		/// <param name="matrReceiver"></param>
		/// <param name="oneRowMatr"></param>
		/// <returns></returns>
		private iMatrix ConcatRowToMatrix(iMatrix matrReceiver, iMatrix oneRowMatr)
		{
			iMatrix resMatr;
			if (matrReceiver != null)
			{
				resMatr = matrReceiver.Clone();
				resMatr.AppendRow(oneRowMatr.Rows[0]);
			}
			else
				resMatr = oneRowMatr.Clone();

			return resMatr;
		}
		private byte[] GetNextInputsAsOutputs(iMatrixByte m, byte[] teachingInput)
		{
			var nextOutputs = new List<byte>();
			for (int i = 0; i < teachingInput.Length; i++)
			{
				var nextOut = m.Process(teachingInput[i]);
				nextOutputs.Add(nextOut);
			}
			return nextOutputs.ToArray();
		}

		private long ComputeCombinationMetrika(iMatrixByte m, byte[] teachingInput, byte[] teachingOutput, long localMetrika)
		{
			long combinationMetrika = 0;
			for (int i = 0; i < teachingInput.Length; i++)
			{
				combinationMetrika += Math.Abs(m.Process(teachingInput[i]) - teachingOutput[i]); 
				if (combinationMetrika > localMetrika) return combinationMetrika;
			}
			return combinationMetrika;
		}

		private iMatrix[] GetLocalOptimizedArray(List<iMatrix> matrList)
		{
			return matrList.OrderBy(t => t, matrComparer).ToArray();
		}
	}
	public class CodeMatrComparer : IComparer<iMatrix>
	{
		// Compares by Height, Length, and Width.
		public int Compare(iMatrix a, iMatrix b)
		{
			var countA = 0;
			var countB = 0;
			{
				countA = a.Rows.Last.rowItems.Where(t => t.h == WallType.Forward).Count();
				countB = b.Rows.Last.rowItems.Where(t => t.h == WallType.Forward).Count();
			}
			countA += a.Rows.Last.rowItems.Where(t => t.v != WallType.Stop).Count();
			countB += b.Rows.Last.rowItems.Where(t => t.v != WallType.Stop).Count();

			//return countB - countA;
			return countA - countB;
		}
	}
}

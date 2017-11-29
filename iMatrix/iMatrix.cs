using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using iMatrixSpc;
using Matrix;
using Newtonsoft.Json;

namespace iMatrixSpc
{
	public enum MatrixIncrement
	{
		Incremented = 0,
		LimitReached = 1
	}
	public class iMatrix: Matrix2D<iNeuron>
	{
		public string Name = "iMatrix2D";
		public string Version = "v.0.1";
		public DateTime time;
		public bool useMemo = false;

		private BigInteger LastRowCombinationCounter;
		public MatrixIncrement LastIncrementResult = MatrixIncrement.Incremented;
		private WallType MaxWallTypeIndex = WallType.Forward;//2-forward
		public BigInteger RowCombinationCounter => LastRowCombinationCounter;
		internal int[] MatrixOutput => Rows.Last.rowItems.Select(t => t.Output).ToArray();

		public iMatrix(int Width) : base(Width)
		{
			time = DateTime.UtcNow;

			for (int i = 0; i < Width; i++)
				Rows[0][i] = new iNeuron();

			SetRowItemsNeigbours(Rows.Last);
		}
		public MatrixIncrement NextWallCombinationOnLastRow()
		{
			var res = IncrementLastRowCellWalls(0);
			if (res == MatrixIncrement.Incremented) LastRowCombinationCounter++;
			LastIncrementResult = res;
			return res;
		}
		private void SetRowItemsNeigbours(Row<iNeuron> theRowItems)
		{
			for (int i = 0; i < xWidth; i++)
			{
				var neo = theRowItems[i];
				if (i > 0)
				{
					neo.Left = theRowItems[i - 1];
					theRowItems[i - 1].Right = neo;
					if (i == (xWidth - 1))
					{
						neo.Right = theRowItems[0];
						theRowItems[0].Left = neo;
					}
				} 
			}
		}
		public override void AppendRow()
		{
			base.AppendRow();
			for (int i = 0; i < xWidth; i++)
			{
				Rows.Last[i] = new iNeuron();
			}
			SetRowItemsNeigbours(Rows.Last);

			LastRowCombinationCounter = 0;
			LastIncrementResult = MatrixIncrement.Incremented;
		}
		public void AppendRow(Row<iNeuron> newRow)
		{
			base.AppendRow();
			for (int i = 0; i < xWidth; i++)
			{
				Rows.Last[i] = new iNeuron();
				Rows.Last[i].h = newRow[i].h;
				Rows.Last[i].v = newRow[i].v;
				Rows.Last[i].Memo = (useMemo)? newRow[i].Memo: 0;
			}
			SetRowItemsNeigbours(Rows.Last);
			LastRowCombinationCounter = 0;
			LastIncrementResult = MatrixIncrement.Incremented;
		}
		public void ProcessMatrix(int[] inputValues)
		{
            var rowInput = Array.ConvertAll(inputValues, t => t);// inputValues.Select(t => t).ToArray();//; 

            if (useMemo)
            {
                for (int i = 0; i < yHeight; i++)
                {
                    PutInputValuesWithMemo(i, rowInput);
                    ProcessRowWithMemo(i);
                    rowInput = Rows[i].rowItems.Select(t => t.Output).ToArray();
                }
            }
            else
            {
                unsafe
                {
                    foreach (var row in Rows.rowItems)
                    {
                        PutInputValues(row, rowInput);
                        ProcessRow(row);
                        rowInput = Array.ConvertAll(row.rowItems, t => t.Output); // row.rowItems.Select(t => t.Output).ToArray();
                    }
                }
                //for (int i = 0; i < yHeight; i++)
                //{
                //    PutInputValues(i, rowInput);
                //    ProcessRow(i);
                //    rowInput = Rows[i].rowItems.Select(t => t.Output).ToArray();
                //}
            }
        }
        private void ProcessRow(int i)
		{
			unsafe
			{
				foreach (var neo in Rows[i].rowItems)
					neo.Act();
				foreach (var neo in Rows[i].rowItems)
					neo.FixRes();
			}
		}
        private void ProcessRow(Row<iNeuron> row)
        {
            unsafe
            {
                foreach (var neo in row.rowItems)
                    neo.Act();
                foreach (var neo in row.rowItems)
                    neo.FixRes();
            }
        }
        private void ProcessRowWithMemo(int i)
		{
			unsafe
			{
				foreach (var neo in Rows[i].rowItems)
					neo.ActWithMemo();
				foreach (var neo in Rows[i].rowItems)
					neo.FixRes();
			}
		}
		private void PutInputValues(int rowIndex, int[] inputValues)
		{
			//if (inputValues.Length > this.xWidth)
				//throw new Exception("Размер входного массива превышает размер матрицы.");
			for (int x = 0; x < inputValues.Length; x++)
				Rows[rowIndex][x].PutInput(inputValues[x]);
		}
        private void PutInputValues(Row<iNeuron> row, int[] inputValues)
        {
            //if (inputValues.Length > this.xWidth)
            //throw new Exception("Размер входного массива превышает размер матрицы.");
            for (int x = 0; x < inputValues.Length; x++)
                row[x].PutInput(inputValues[x]);
        }
        private void PutInputValuesWithMemo(int rowIndex, int[] inputValues)
		{
			if (inputValues.Length > this.xWidth)
				throw new Exception("Размер входного массива превышает размер матрицы.");
			for (int x = 0; x < inputValues.Length; x++)
                Rows[rowIndex][x].PutInputWithMemo(inputValues[x]);
		}
		MatrixIncrement IncrementLastRowCellWalls(int x)
		{
			iNeuron neuron = this[x, yHeight - 1];
			if (neuron.h < MaxWallTypeIndex)
			{
				//elem.h++; //у горизонтальных два положения - вперед (вверх) и стоп
				neuron.h = WallType.Forward;
				return MatrixIncrement.Incremented;
			}
			else if (neuron.v < MaxWallTypeIndex)
			{
				neuron.h = WallType.Stop;
				neuron.v++;
				return MatrixIncrement.Incremented;
			}
			else if (x < xWidth - 1)
			{
				neuron.h = WallType.Stop;
				neuron.v = WallType.Stop;
				return IncrementLastRowCellWalls(x + 1);
			}
			else
			{
				//Предел достигнут, выставляем у всех нейронов строки перегородки Forward
				for (var i = 0; i < xWidth; i++)
				{
					this[i, yHeight - 1].h = WallType.Forward;
					this[i, yHeight - 1].v = WallType.Forward;
				}
				return MatrixIncrement.LimitReached;
			}

		}
		public iMatrix Clone()
		{
			var jsObject = WriteToJson();
			var matr = ReadFromJson(jsObject);
			foreach (var row in matr.Rows.rowItems)
			{
				SetRowItemsNeigbours(row);
			}
			return matr;
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			using (var tw = new StringWriter(sb))
			{
				var js = JsonSerializer.Create();
				js.Serialize(tw, this);
			}
			return sb.ToString();
		}
		public void SaveToFile(string filename)
		{
			var s = WriteToJson();
			File.WriteAllText(filename, s);
		}
		public string WriteToJson()
		{
			time = DateTime.Now;
			return this.ToString()
				.Replace(",[", ",\n[")
				.Replace("{[", "{\n[")
				.Replace("}]", "}\n]")
				.Replace(":[{", ":[\n{")
				.Replace("},\"", "},\n\"")
				.Replace(",\"hWalls\"", ",\n\"hWalls\"")
				.Replace("},{\"Items\"", "},\n{\"Items\"")
				.Replace("\",\"", "\",\n\"");
		}
		public static iMatrix LoadFromFile(string filename)
		{
			var s = File.ReadAllText(filename);
			return ReadFromJson(s);
		}
		public static iMatrix ReadFromJson(string s)
		{
			var matr = JsonConvert.DeserializeObject<iMatrix>(s);
			return matr;
		}
		public static void Compare(iMatrix m1, iMatrix m2)
		{
			bool res = true;
			if (m1.xWidth != m2.xWidth || m1.yHeight != m2.yHeight)
			{
				Debug.WriteLine("Размеры не совпадают");
				res = false;
			}
			for (int y = 0; y < m1.yHeight; y++)
				for (int x = 0; x < m1.xWidth; x++)
				{
					if (m1[x, y].h != m2[x, y].h || m1[x, y].v != m2[x, y].v)
					{
						Debug.WriteLine(String.Format("Перегородки нейронов [{0},{1}] не совпадают.", x, y));
						res = false;
					}
				}
			if (res == true)
				Debug.WriteLine("Проверка: Матрицы совпадают - Ok");
		}
	}
}

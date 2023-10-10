using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleActionFramework.Core
{
	[Serializable]
	public enum ValueType
	{
		Number,
		String,
		NumberList,
		StringList,
		Input,
	}
	
	[Serializable]
	public enum ConditionType
	{
		// Generic
		Equal,
		NotEqual,
		
		// Number
		Greater,
		Less,
		GreaterOrEqual,
		LessOrEqual,
		
		// String
		Contains,
		Exclusive,
	}

	[Serializable]
	public enum JointType
	{
		And,
		Or,
		Xor,
	}
	
	[Serializable]
	public class ConditionState
	{
		public JointType JointType = JointType.And;
		public DefaultKeys DefaultKey;
		public string Key;
		public ValueType ValueType;
		public ConditionType ConditionType;
		public string StringValue;
		public float NumberValue;

		public bool ConsumeInput = true;
		
		public bool ConditionCheck(ActionStateMachine machine)
		{
			if (!machine.Data.ContainsKey(Key))
			{
				return false;
			}
			
			switch (ValueType)
			{
				case ValueType.Number:
					if (machine.Data[Key] is float number)
					{
						switch (ConditionType)
						{
							case ConditionType.Equal:
								return Math.Abs(number - NumberValue) < Constants.Epsilon;
							case ConditionType.NotEqual:
								return Math.Abs(number - NumberValue) > Constants.Epsilon;
							case ConditionType.Greater:
								return number > NumberValue;
							case ConditionType.Less:
								return number < NumberValue;
							case ConditionType.GreaterOrEqual:
								return number >= NumberValue;
							case ConditionType.LessOrEqual:
								return number <= NumberValue;
						}
					}
                    return false;
				case ValueType.String:
					machine.Data.TryAdd(Key, "");
					if (machine.Data[Key] is string str)
					{
						switch (ConditionType)
						{
							case ConditionType.Equal:
								return str == StringValue;
							case ConditionType.NotEqual:
								return str != StringValue;
							case ConditionType.Contains:
								return str.Contains(StringValue);
							case ConditionType.Exclusive:
								return !str.Contains(StringValue);
						}
					}
					return false;
				case ValueType.NumberList:
					if (machine.Data[Key] is List<float> nList)
					{
						var nstr = string.Join(",", nList);
						StringValue = StringValue.Replace(" ", "");
						switch (ConditionType)
						{
							case ConditionType.Equal:
								return nstr == StringValue;
							case ConditionType.NotEqual:
								return nstr != StringValue;
							case ConditionType.Greater:
								return nList.Count > NumberValue;
							case ConditionType.Less:
								return nList.Count < NumberValue;
							case ConditionType.GreaterOrEqual:
								return nList.Count >= NumberValue;
							case ConditionType.LessOrEqual:
								return nList.Count <= NumberValue;
							case ConditionType.Contains:
								return nList.Contains(NumberValue);
							case ConditionType.Exclusive:
								return !nList.Contains(NumberValue);
						}
					}
					return false;
				case ValueType.StringList:
					if (machine.Data[Key] is IEnumerator<string> sList)
					{
						var lstr = string.Join(",", sList);
						StringValue = StringValue.Replace(" ", "");
						
						switch (ConditionType)
						{
							case ConditionType.Equal:
								return lstr == StringValue;
							case ConditionType.NotEqual:
								return lstr != StringValue;
							case ConditionType.Contains:
								return lstr.Contains(StringValue);
							case ConditionType.Exclusive:
								return !lstr.Contains(StringValue);
						}
					}
					return false;
				case ValueType.Input:
					if (machine.Data[Key] is List<InputRecord> iList)
					{
						iList.Reverse();
						var istr = string.Join(",", iList);
						StringValue = StringValue.Replace(" ", "");
						iList.Reverse();
						
						switch (ConditionType)
						{
							case ConditionType.Equal:
								if (istr != StringValue) return false;
								if (ConsumeInput) iList.Clear();
								return true;
							
							case ConditionType.NotEqual:
								return istr != StringValue;
							
							case ConditionType.Contains:
								var csplit = StringValue.Split(",");
								int ccount = 0;
								int cgoal = csplit.Length;
								
								for (int i = 0; i < iList.Count; i++)
								{
									var target = csplit[ccount];
									if (target.Contains('+') || target.Contains('-'))
									{
										if (istr.Contains(target))
										{
											ccount++;
											if (ConsumeInput) iList.RemoveAt(i);
										}
									}
									else
									{
										machine.Actor.CurrentInputs.TryGetValue(target, out var value);
										if (value)
											ccount++;
									}

									if (ccount == cgoal)
										return true;
								}
								return false;
							
							case ConditionType.Exclusive:
								var xsplit = StringValue.Split(",");
								int xgoal = xsplit.Length;
								
								for (int j = 0; j < xgoal; j++)
								{
									var target = xsplit[j];
									if (target.Contains('+') || target.Contains('-'))
									{
										if (istr.Contains(target))
											return false;
									}
									else
									{
										machine.Actor.CurrentInputs.TryGetValue(target, out var value);
										if (value)
											return false;
									}
								}
								return true;
						}
					}
					return false;
			}
			return false;
		}
	}
}
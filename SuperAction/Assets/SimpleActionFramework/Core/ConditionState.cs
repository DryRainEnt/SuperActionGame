using System;

namespace SimpleActionFramework.Core
{
	[Serializable]
	public enum ValueType
	{
		Number,
		String,
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
		public string Key;
		public ValueType ValueType;
		public ConditionType ConditionType;
		public string StringValue;
		public float NumberValue;
		
		public bool ConditionCheck(ActionStateMachine machine)
		{
			switch (ValueType)
			{
				case ValueType.Number:
					if (machine.Data.TryGetValue(Key, out var nValue) && nValue is float number)
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
					if (machine.Data.TryGetValue(Key, out var sValue) && sValue is string str)
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
					else if (machine.Data.TryGetValue(Key, out var lValue) && lValue is object[] value)
					{
						var lstr = string.Join(",", value);
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
			}
			return false;
		}
	}
}
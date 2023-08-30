using System;
using System.Drawing;
using SimpleActionFramework.Core;
using ValueType = SimpleActionFramework.Core.ValueType;

namespace SimpleActionFramework.Actant
{
	[Serializable]
	public class SetStateDataActant : SingleActant
	{
		public DefaultKeys DefaultKey;
		public string Key;
		public ValueType ValueType;
		public string StringValue;
		public float NumberValue;
		
	 	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	 	{
	 	 	base.Act(actor, progress, isFirstFrame);
	 	 	// Put your code here
		    
		    actor.ActionStateMachine.UpdateData(Key, ValueType switch
		    {
			    ValueType.Number => NumberValue,
			    ValueType.String => StringValue,
			    ValueType.StringList => StringValue.Replace(" ", "").Split(","),
			    ValueType.NumberList => Array.ConvertAll(StringValue.Replace(" ", "").Split(","), GetFloat),
			    ValueType.Input => StringValue.Replace(" ", "").Split(","),
			    _ => null
		    });
	 	}
	    
	    public static float GetFloat(object value)
	    {
		    return value switch
		    {
			    float f => f,
			    int i => i,
			    string s => float.Parse(s),
			    _ => 0f
		    };
	    }
	}
}

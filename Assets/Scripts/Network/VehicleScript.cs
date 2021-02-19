
public enum ScriptType {
    playerDriverScript,
    playerGunnerScript,
    aiDriverScript,
    aiGunnerScript
}

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]  
public class VehicleScript : System.Attribute  
{  
    public ScriptType scriptType;
  
    public VehicleScript(ScriptType scriptType)  
    {  
        this.scriptType = scriptType;
    }  
}  
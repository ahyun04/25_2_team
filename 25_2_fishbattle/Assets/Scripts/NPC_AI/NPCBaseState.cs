using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCBaseState
{
    public abstract IEnumerator ExecuteState(NPC_AI_Controller context);
}

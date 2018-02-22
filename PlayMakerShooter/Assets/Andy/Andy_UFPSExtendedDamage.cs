using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class and file is based TOTALLY on the work of forum user @pHrEaKzOiD 
// I renamed the class and the file to suit my housekeeping preferences.
// That user's question and our discussion of his problem can be found at
// http://www.opsive.com/assets/UFPS/forum/index.php?p=/discussion/comment/16218/#Comment_16218
// anyone seeing THIS discussion should first look at the flow of that entire thread before
// using this answer! By doing that, you may find an even more elegant way to achieve your goal //(but I doubt it. ha!).

public class Andy_UFPSExtendedDamage : vp_FXBullet
{

    public vp_DamageInfo.DamageType _type;

    protected override void DoUFPSDamage()
    {
        m_TargetDHandler.Damage(new vp_DamageInfo(Damage, m_Source, _type));
    }

}
using UnityEngine;

public enum EnemyState
{
    NONE = 0,
    IDLE = 1,
    MOVING = 2,
    CHASE = 3, 
    MELEE = 4,
    DEAD = 5,
    SIEGING = 6,
    SHOOT = 7,
}

public enum StateEvent
{
    CHASEPLAYER = 0,
    LOSTPLAYER = 1,
    GOTOOBJECTIVE = 2,
    MELEEPLAYER = 3,
    STOPMELEEPLAYER = 4,
    SIEGEOBJECTIVE = 5,
    DIE = 6,
    SPAWN = 7,
}
